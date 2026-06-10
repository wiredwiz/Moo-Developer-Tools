#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="CachingMooWorldQueryProvider.cs">
// Copyright (c) Thaddeus Ryker 2026
// </copyright>
//
// BSD 3-Clause License
//
// Copyright (c) 2026,
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

using System.Collections.Concurrent;
using NLog;

namespace Org.Edgerunner.Mud.Common.Querying;

/// <summary>
/// A time-based caching decorator for an <see cref="IMooWorldQueryProvider"/>. Successful results
/// (including empty/null) are cached for a configurable time-to-live so that unsupported or expensive
/// operations are not re-attempted on every keystroke. Exceptions are never cached.
/// </summary>
/// <seealso cref="IMooWorldQueryProvider"/>
/// <seealso cref="IDisposable"/>
public class CachingMooWorldQueryProvider : IMooWorldQueryProvider, IDisposable
{
   private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

   /// <summary>
   /// Identifies a cached query operation.
   /// </summary>
   public enum Operation
   {
      /// <summary>The <see cref="IMooWorldQueryProvider.GetObjectsAsync"/> operation.</summary>
      GetObjects,

      /// <summary>The <see cref="IMooWorldQueryProvider.GetChildrenAsync"/> operation.</summary>
      GetChildren,

      /// <summary>The current-player overload of <see cref="IMooWorldQueryProvider.GetOwnedObjectsAsync(CancellationToken)"/>.</summary>
      OwnedObjectsForPlayer,

      /// <summary>The owner overload of <see cref="IMooWorldQueryProvider.GetOwnedObjectsAsync(MooObjectId, CancellationToken)"/>.</summary>
      OwnedObjectsForOwner,

      /// <summary>The <see cref="IMooWorldQueryProvider.GetParentAsync"/> operation.</summary>
      GetParent,

      /// <summary>The <see cref="IMooWorldQueryProvider.GetVerbsAsync"/> operation.</summary>
      GetVerbs,

      /// <summary>The <see cref="IMooWorldQueryProvider.GetVerbDocumentationAsync"/> operation.</summary>
      GetVerbDocumentation,

      /// <summary>The <see cref="IMooWorldQueryProvider.GetPropertiesAsync"/> operation.</summary>
      GetProperties,

      /// <summary>The <see cref="IMooWorldQueryProvider.GetPropertyDocumentationAsync"/> operation.</summary>
      GetPropertyDocumentation,

      /// <summary>The <see cref="IMooWorldQueryProvider.GetVerbInfoAsync"/> operation.</summary>
      GetVerbInfo,

      /// <summary>The <see cref="IMooWorldQueryProvider.GetPropertyInfoAsync"/> operation.</summary>
      GetPropertyInfo,

      /// <summary>The <see cref="IMooWorldQueryProvider.GetVerbCodeAsync"/> operation.</summary>
      GetVerbCode,

      /// <summary>The <see cref="IMooWorldQueryProvider.GetPropertyValueAsync"/> operation.</summary>
      GetPropertyValue,
   }

   private readonly record struct CacheKey(Operation Operation, MooObjectId ObjectId, string? Name);

   private sealed class CacheEntry
   {
      public CacheEntry(object? value, DateTime timestamp)
      {
         Value = value;
         Timestamp = timestamp;
      }

      public object? Value { get; }

      public DateTime Timestamp { get; }
   }

   /// <summary>
   /// The default time-to-live applied to cached entries.
   /// </summary>
   public static readonly TimeSpan DefaultTimeToLive = TimeSpan.FromSeconds(60);

   private readonly IMooWorldQueryProvider _inner;

   private readonly TimeSpan _timeToLive;

   private readonly ConcurrentDictionary<CacheKey, CacheEntry> _cache = new();

   private readonly Timer _sweepTimer;

   private bool _disposed;

   /// <summary>
   /// Initializes a new instance of the <see cref="CachingMooWorldQueryProvider"/> class.
   /// </summary>
   /// <param name="inner">The inner provider whose results are cached.</param>
   /// <param name="timeToLive">The cache entry time-to-live. Defaults to <see cref="DefaultTimeToLive"/> when <c>null</c>.</param>
   /// <exception cref="ArgumentNullException">Thrown when <paramref name="inner"/> is <c>null</c>.</exception>
   public CachingMooWorldQueryProvider(IMooWorldQueryProvider inner, TimeSpan? timeToLive = null)
   {
      _inner = inner ?? throw new ArgumentNullException(nameof(inner));
      _timeToLive = timeToLive ?? DefaultTimeToLive;

      // Sweep at the TTL cadence (bounded to a sane minimum) to keep memory bounded.
      var sweepInterval = _timeToLive > TimeSpan.Zero ? _timeToLive : DefaultTimeToLive;
      if (sweepInterval < TimeSpan.FromSeconds(1))
         sweepInterval = TimeSpan.FromSeconds(1);

      _sweepTimer = new Timer(_ => Sweep(), null, sweepInterval, sweepInterval);
   }

   /// <summary>
   /// Removes all cached entries.
   /// </summary>
   public void Clear()
   {
      _cache.Clear();
   }

   /// <summary>
   /// Removes all cached entries that reference the specified object id.
   /// </summary>
   /// <param name="objectId">The object id whose entries should be dropped.</param>
   public void InvalidateObject(MooObjectId objectId)
   {
      foreach (var key in _cache.Keys)
         if (key.ObjectId == objectId)
            _cache.TryRemove(key, out _);
   }

   /// <summary>
   /// Removes the cached entry for the specified operation, object id and optional name.
   /// </summary>
   /// <param name="operation">The cached operation.</param>
   /// <param name="objectId">The object id.</param>
   /// <param name="name">The verb or property name, if applicable.</param>
   public void Invalidate(Operation operation, MooObjectId objectId, string? name = null)
   {
      _cache.TryRemove(new CacheKey(operation, objectId, name), out _);
   }

   /// <summary>
   /// Removes expired cache entries.
   /// </summary>
   private void Sweep()
   {
      try
      {
         var now = DateTime.UtcNow;
         foreach (var pair in _cache)
            if (now - pair.Value.Timestamp >= _timeToLive)
               _cache.TryRemove(pair.Key, out _);
      }
      catch (Exception ex)
      {
         // A sweep failure must never crash the process via the timer thread.
         Logger.Warn(ex, "An error occurred while sweeping expired cache entries.");
      }
   }

   private async Task<T> GetOrAddAsync<T>(CacheKey key, Func<Task<T>> factory)
   {
      if (_cache.TryGetValue(key, out var entry) && DateTime.UtcNow - entry.Timestamp < _timeToLive)
         return (T)entry.Value!;

      // Miss or expired: call the inner provider. Exceptions propagate and are not cached.
      var result = await factory().ConfigureAwait(false);
      _cache[key] = new CacheEntry(result, DateTime.UtcNow);
      return result;
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetObjectsAsync(CancellationToken cancellationToken)
   {
      return GetOrAddAsync(
         new CacheKey(Operation.GetObjects, MooObjectId.Nothing, null),
         () => _inner.GetObjectsAsync(cancellationToken));
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetChildrenAsync(MooObjectId objectId, CancellationToken cancellationToken)
   {
      return GetOrAddAsync(
         new CacheKey(Operation.GetChildren, objectId, null),
         () => _inner.GetChildrenAsync(objectId, cancellationToken));
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(CancellationToken cancellationToken)
   {
      // Current-player overload: no owner id, so it is keyed only by its own operation
      // (MooObjectId.Nothing as a placeholder). It is dropped only by Clear()/ProvidersChanged.
      return GetOrAddAsync(
         new CacheKey(Operation.OwnedObjectsForPlayer, MooObjectId.Nothing, null),
         () => _inner.GetOwnedObjectsAsync(cancellationToken));
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(MooObjectId owner, CancellationToken cancellationToken)
   {
      // Owner overload: keyed by the owner id so InvalidateObject(owner) drops it.
      return GetOrAddAsync(
         new CacheKey(Operation.OwnedObjectsForOwner, owner, null),
         () => _inner.GetOwnedObjectsAsync(owner, cancellationToken));
   }

   /// <inheritdoc/>
   public Task<MooObjectId?> GetParentAsync(MooObjectId objectId, CancellationToken cancellationToken)
   {
      return GetOrAddAsync(
         new CacheKey(Operation.GetParent, objectId, null),
         () => _inner.GetParentAsync(objectId, cancellationToken));
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooVerbSummary>> GetVerbsAsync(MooObjectId objectId, CancellationToken cancellationToken)
   {
      return GetOrAddAsync(
         new CacheKey(Operation.GetVerbs, objectId, null),
         () => _inner.GetVerbsAsync(objectId, cancellationToken));
   }

   /// <inheritdoc/>
   public Task<MooVerbDocumentation?> GetVerbDocumentationAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken)
   {
      return GetOrAddAsync(
         new CacheKey(Operation.GetVerbDocumentation, objectId, verbName),
         () => _inner.GetVerbDocumentationAsync(objectId, verbName, cancellationToken));
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooPropertySummary>> GetPropertiesAsync(MooObjectId objectId, CancellationToken cancellationToken)
   {
      return GetOrAddAsync(
         new CacheKey(Operation.GetProperties, objectId, null),
         () => _inner.GetPropertiesAsync(objectId, cancellationToken));
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<string>> GetPropertyDocumentationAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken)
   {
      return GetOrAddAsync(
         new CacheKey(Operation.GetPropertyDocumentation, objectId, propName),
         () => _inner.GetPropertyDocumentationAsync(objectId, propName, cancellationToken));
   }

   /// <inheritdoc/>
   public Task<MooVerbInfo?> GetVerbInfoAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken)
   {
      return GetOrAddAsync(
         new CacheKey(Operation.GetVerbInfo, objectId, verbName),
         () => _inner.GetVerbInfoAsync(objectId, verbName, cancellationToken));
   }

   /// <inheritdoc/>
   public Task<MooPropertyInfo?> GetPropertyInfoAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken)
   {
      return GetOrAddAsync(
         new CacheKey(Operation.GetPropertyInfo, objectId, propName),
         () => _inner.GetPropertyInfoAsync(objectId, propName, cancellationToken));
   }

   /// <inheritdoc/>
   public Task<MooVerbCode?> GetVerbCodeAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken)
   {
      return GetOrAddAsync(
         new CacheKey(Operation.GetVerbCode, objectId, verbName),
         () => _inner.GetVerbCodeAsync(objectId, verbName, cancellationToken));
   }

   /// <inheritdoc/>
   public Task<MooPropertyValue?> GetPropertyValueAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken)
   {
      return GetOrAddAsync(
         new CacheKey(Operation.GetPropertyValue, objectId, propName),
         () => _inner.GetPropertyValueAsync(objectId, propName, cancellationToken));
   }

   /// <summary>
   /// Performs application-defined tasks associated with freeing, releasing, or resetting resources.
   /// </summary>
   public void Dispose()
   {
      Dispose(true);
      GC.SuppressFinalize(this);
   }

   /// <summary>
   /// Releases the unmanaged resources used by this instance and optionally releases the managed resources.
   /// </summary>
   /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
   protected virtual void Dispose(bool disposing)
   {
      if (_disposed)
         return;

      if (disposing)
      {
         _sweepTimer.Dispose();
         _cache.Clear();
      }

      _disposed = true;
   }
}
