#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="MooWorldQueryService.cs">
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

namespace Org.Edgerunner.Mud.Common.Querying;

/// <summary>
/// A per-connection service that owns a <see cref="MooWorldQueryProviderRegistry"/> and a
/// <see cref="CachingMooWorldQueryProvider"/> wrapping it. Concrete query providers self-register here
/// when their protocol capability is detected; the editor consumes the aggregated, cached query surface
/// exposed by <see cref="Query"/>.
/// </summary>
/// <seealso cref="IDisposable"/>
public class MooWorldQueryService : IDisposable
{
   private readonly MooWorldQueryProviderRegistry _registry;

   private readonly CachingMooWorldQueryProvider _cache;

   private bool _disposed;

   /// <summary>
   /// Initializes a new instance of the <see cref="MooWorldQueryService"/> class.
   /// </summary>
   /// <param name="timeToLive">The cache entry time-to-live. Defaults to <see cref="CachingMooWorldQueryProvider.DefaultTimeToLive"/> when <c>null</c>.</param>
   public MooWorldQueryService(TimeSpan? timeToLive = null)
   {
      _registry = new MooWorldQueryProviderRegistry();
      _cache = new CachingMooWorldQueryProvider(_registry, timeToLive);

      // When the set of providers changes, drop the cache so newly-supported operations get a fresh chance.
      _registry.ProvidersChanged += OnRegistryProvidersChanged;
   }

   /// <summary>
   /// Gets the aggregated, cached query surface for this connection.
   /// </summary>
   /// <value>An <see cref="IMooWorldQueryProvider"/> that routes through the cache and registry.</value>
   public IMooWorldQueryProvider Query => _cache;

   /// <summary>
   /// Gets or sets the cache entry time-to-live for this connection. Settable at runtime so a changed
   /// editor-cache TTL setting takes effect on an already-open connection without a reconnect.
   /// </summary>
   /// <value>The cache entry lifetime; defaults to <see cref="CachingMooWorldQueryProvider.DefaultTimeToLive"/>.</value>
   public TimeSpan TimeToLive
   {
      get => _cache.TimeToLive;
      set => _cache.TimeToLive = value;
   }

   /// <summary>
   /// Occurs when the set of registered providers changes.
   /// </summary>
   public event EventHandler? ProvidersChanged;

   /// <summary>
   /// Registers a provider with the specified priority. Higher priority providers are preferred.
   /// </summary>
   /// <param name="provider">The provider to register.</param>
   /// <param name="priority">The provider priority; higher values are queried first.</param>
   public void Register(IMooWorldQueryProvider provider, int priority)
   {
      _registry.Register(provider, priority);
   }

   /// <summary>
   /// Unregisters a previously registered provider.
   /// </summary>
   /// <param name="provider">The provider to unregister.</param>
   /// <returns><c>true</c> if a registration was removed; otherwise, <c>false</c>.</returns>
   public bool Unregister(IMooWorldQueryProvider provider)
   {
      return _registry.Unregister(provider);
   }

   /// <summary>
   /// Removes all cached query entries that reference the specified object id.
   /// </summary>
   /// <param name="objectId">The object id whose entries should be dropped.</param>
   public void InvalidateObject(MooObjectId objectId)
   {
      _cache.InvalidateObject(objectId);
   }

   /// <summary>
   /// Removes all cached query entries.
   /// </summary>
   public void Clear()
   {
      _cache.Clear();
   }

   private void OnRegistryProvidersChanged(object? sender, EventArgs e)
   {
      _cache.Clear();
      ProvidersChanged?.Invoke(this, e);
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
   /// Releases the resources used by this instance.
   /// </summary>
   /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
   protected virtual void Dispose(bool disposing)
   {
      if (_disposed)
         return;

      if (disposing)
      {
         _registry.ProvidersChanged -= OnRegistryProvidersChanged;
         _cache.Dispose();
      }

      _disposed = true;
   }
}
