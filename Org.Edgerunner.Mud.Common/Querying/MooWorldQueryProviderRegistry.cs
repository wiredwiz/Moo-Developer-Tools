#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="MooWorldQueryProviderRegistry.cs">
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
/// Aggregates multiple <see cref="IMooWorldQueryProvider"/> instances and routes each query to them
/// in priority order, falling through to the next provider on <see cref="NotImplementedException"/>.
/// </summary>
/// <seealso cref="IMooWorldQueryProvider"/>
public class MooWorldQueryProviderRegistry : IMooWorldQueryProvider
{
   private sealed class Registration
   {
      public Registration(IMooWorldQueryProvider provider, int priority, long sequence)
      {
         Provider = provider;
         Priority = priority;
         Sequence = sequence;
      }

      public IMooWorldQueryProvider Provider { get; }

      public int Priority { get; }

      public long Sequence { get; }
   }

   private readonly object _lock = new();

   private readonly List<Registration> _registrations = new();

   private long _sequence;

   /// <summary>
   /// Occurs when the set of registered providers changes (a provider is registered or unregistered).
   /// </summary>
   public event EventHandler? ProvidersChanged;

   /// <summary>
   /// Registers a provider with the specified priority. Higher priority providers are preferred.
   /// </summary>
   /// <param name="provider">The provider to register.</param>
   /// <param name="priority">The provider priority; higher values are queried first.</param>
   /// <exception cref="ArgumentNullException">Thrown when <paramref name="provider"/> is <c>null</c>.</exception>
   public void Register(IMooWorldQueryProvider provider, int priority)
   {
      if (provider == null)
         throw new ArgumentNullException(nameof(provider));

      lock (_lock)
      {
         _registrations.Add(new Registration(provider, priority, _sequence++));
      }

      OnProvidersChanged();
   }

   /// <summary>
   /// Unregisters a previously registered provider.
   /// </summary>
   /// <param name="provider">The provider to unregister.</param>
   /// <returns><c>true</c> if a registration was removed; otherwise, <c>false</c>.</returns>
   public bool Unregister(IMooWorldQueryProvider provider)
   {
      if (provider == null)
         throw new ArgumentNullException(nameof(provider));

      bool removed;
      lock (_lock)
      {
         removed = _registrations.RemoveAll(r => ReferenceEquals(r.Provider, provider)) > 0;
      }

      if (removed)
         OnProvidersChanged();

      return removed;
   }

   /// <summary>
   /// Raises the <see cref="ProvidersChanged"/> event.
   /// </summary>
   protected virtual void OnProvidersChanged()
   {
      ProvidersChanged?.Invoke(this, EventArgs.Empty);
   }

   private IReadOnlyList<IMooWorldQueryProvider> GetOrderedProviders()
   {
      lock (_lock)
      {
         // Higher priority first; for equal priority, preserve registration order (stable).
         return _registrations
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.Sequence)
            .Select(r => r.Provider)
            .ToList();
      }
   }

   /// <summary>
   /// Walks the registered providers in priority order, invoking <paramref name="operation"/> on each.
   /// A provider that throws <see cref="NotImplementedException"/> is skipped and the next provider is
   /// tried. The first provider that returns a value wins. If no provider answers, <paramref name="fallback"/>
   /// is returned. Any exception other than <see cref="NotImplementedException"/> is allowed to propagate.
   /// </summary>
   /// <typeparam name="T">The result type.</typeparam>
   /// <param name="operation">The query operation to invoke on each provider.</param>
   /// <param name="fallback">The value to return when the provider chain is exhausted.</param>
   /// <returns>The first provider's result, or <paramref name="fallback"/> when exhausted.</returns>
   private async Task<T> QueryAsync<T>(Func<IMooWorldQueryProvider, Task<T>> operation, T fallback)
   {
      foreach (var provider in GetOrderedProviders())
      {
         try
         {
            return await operation(provider).ConfigureAwait(false);
         }
         catch (NotImplementedException)
         {
            // This provider does not support the operation; fall through to the next.
         }
      }

      return fallback;
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetObjectsAsync(CancellationToken cancellationToken)
   {
      return QueryAsync(p => p.GetObjectsAsync(cancellationToken), Array.Empty<MooObjectSummary>());
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetChildrenAsync(MooObjectId objectId, CancellationToken cancellationToken)
   {
      return QueryAsync(p => p.GetChildrenAsync(objectId, cancellationToken), Array.Empty<MooObjectSummary>());
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(CancellationToken cancellationToken)
   {
      return QueryAsync(p => p.GetOwnedObjectsAsync(cancellationToken), Array.Empty<MooObjectSummary>());
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(MooObjectId owner, CancellationToken cancellationToken)
   {
      return QueryAsync(p => p.GetOwnedObjectsAsync(owner, cancellationToken), Array.Empty<MooObjectSummary>());
   }

   /// <inheritdoc/>
   public Task<MooObjectId?> GetParentAsync(MooObjectId objectId, CancellationToken cancellationToken)
   {
      return QueryAsync<MooObjectId?>(p => p.GetParentAsync(objectId, cancellationToken), null);
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooVerbSummary>> GetVerbsAsync(MooObjectId objectId, CancellationToken cancellationToken)
   {
      return QueryAsync(p => p.GetVerbsAsync(objectId, cancellationToken), Array.Empty<MooVerbSummary>());
   }

   /// <inheritdoc/>
   public Task<MooVerbDocumentation?> GetVerbDocumentationAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken)
   {
      return QueryAsync<MooVerbDocumentation?>(p => p.GetVerbDocumentationAsync(objectId, verbName, cancellationToken), null);
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooPropertySummary>> GetPropertiesAsync(MooObjectId objectId, CancellationToken cancellationToken)
   {
      return QueryAsync(p => p.GetPropertiesAsync(objectId, cancellationToken), Array.Empty<MooPropertySummary>());
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<string>> GetPropertyDocumentationAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken)
   {
      return QueryAsync(p => p.GetPropertyDocumentationAsync(objectId, propName, cancellationToken), (IReadOnlyList<string>)Array.Empty<string>());
   }

   /// <inheritdoc/>
   public Task<MooVerbInfo?> GetVerbInfoAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken)
   {
      return QueryAsync<MooVerbInfo?>(p => p.GetVerbInfoAsync(objectId, verbName, cancellationToken), null);
   }

   /// <inheritdoc/>
   public Task<MooPropertyInfo?> GetPropertyInfoAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken)
   {
      return QueryAsync<MooPropertyInfo?>(p => p.GetPropertyInfoAsync(objectId, propName, cancellationToken), null);
   }

   /// <inheritdoc/>
   public Task<MooVerbCode?> GetVerbCodeAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken)
   {
      return QueryAsync<MooVerbCode?>(p => p.GetVerbCodeAsync(objectId, verbName, cancellationToken), null);
   }

   /// <inheritdoc/>
   public Task<MooPropertyValue?> GetPropertyValueAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken)
   {
      return QueryAsync<MooPropertyValue?>(p => p.GetPropertyValueAsync(objectId, propName, cancellationToken), null);
   }
}
