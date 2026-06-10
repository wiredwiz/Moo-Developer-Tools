#region BSD 3-Clause License
// <copyright file="MemberCompletionController.cs" company="Edgerunner.org">
// Copyright 2020
// </copyright>
//
// BSD 3-Clause License
//
// Copyright (c) 2022,
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastColoredTextBoxNS.Types;
using Org.Edgerunner.Mud.Common.Querying;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// Supplies world-queried member completion items (verbs, properties, core references) to the
/// autocomplete popup. Lookups are synchronous against a local cache; misses start a single
/// background fetch and return nothing, and the menu is refreshed when results arrive.
/// </summary>
/// <remarks>
/// In production all state mutation happens on the UI thread (popup enumeration plus actions the
/// owner marshals there), but a lock guards the cache and in-flight state anyway so that hosts
/// (and tests) with an immediate marshal are also safe. Member completion is best-effort: provider
/// failures (timeout, cancellation, disconnect, protocol errors) are swallowed and simply leave
/// the static completion list in place. The menu-refresh callback is invoked outside the state
/// lock and may therefore fire after disposal; owners must supply a callback that is safe to call
/// on a closed editor (the production callback only re-shows an open popup).
/// </remarks>
public sealed class MemberCompletionController : IDisposable
{
   /// <summary>The default lifetime of a cached member list.</summary>
   public static readonly TimeSpan DefaultCacheTimeToLive = TimeSpan.FromSeconds(30);

   private readonly Func<IMooWorldQueryProvider?> _providerAccessor;

   private readonly Func<MooObjectId?> _contextObjectAccessor;

   private readonly Action<Action> _uiMarshal;

   private readonly Action _menuRefresh;

   private readonly TimeSpan _cacheTimeToLive;

   private readonly object _stateLock = new();

   private readonly Dictionary<(MemberContextKind Kind, int ObjectNumber), CacheEntry> _cache = new();

   private (MemberContextKind Kind, int ObjectNumber)? _inflightKey;

   private CancellationTokenSource? _fetchCancellation;

   private bool _disposed;

   private sealed record CacheEntry(IReadOnlyList<AutocompleteItem> Items, DateTime CreatedUtc);

   /// <summary>
   /// Initializes a new instance of the <see cref="MemberCompletionController"/> class.
   /// </summary>
   /// <param name="providerAccessor">Returns the current query provider, or <c>null</c> when none is attached.</param>
   /// <param name="contextObjectAccessor">Returns the object the edited verb lives on (the meaning of <c>this</c>), when known.</param>
   /// <param name="uiMarshal">Runs the supplied action on the UI thread (tests may invoke immediately).</param>
   /// <param name="menuRefresh">Asks the owner to refresh the autocomplete popup if it is open.</param>
   /// <param name="cacheTimeToLive">Cache entry lifetime; defaults to <see cref="DefaultCacheTimeToLive"/>.</param>
   /// <exception cref="ArgumentNullException">Thrown when any callback is <c>null</c>.</exception>
   public MemberCompletionController(
      Func<IMooWorldQueryProvider?> providerAccessor,
      Func<MooObjectId?> contextObjectAccessor,
      Action<Action> uiMarshal,
      Action menuRefresh,
      TimeSpan? cacheTimeToLive = null)
   {
      _providerAccessor = providerAccessor ?? throw new ArgumentNullException(nameof(providerAccessor));
      _contextObjectAccessor = contextObjectAccessor ?? throw new ArgumentNullException(nameof(contextObjectAccessor));
      _uiMarshal = uiMarshal ?? throw new ArgumentNullException(nameof(uiMarshal));
      _menuRefresh = menuRefresh ?? throw new ArgumentNullException(nameof(menuRefresh));
      _cacheTimeToLive = cacheTimeToLive ?? DefaultCacheTimeToLive;
   }

   /// <summary>
   /// Gets the member completion items for the caret position described by <paramref name="linePrefix"/>.
   /// Returns an empty list outside member contexts, for unresolved operands, without a provider,
   /// or while a fetch is still in flight (the menu is refreshed when it completes).
   /// </summary>
   /// <param name="linePrefix">The text on the caret line, from column 0 up to the caret.</param>
   /// <returns>The items to offer; never <c>null</c>.</returns>
   public IReadOnlyList<AutocompleteItem> GetMemberItems(string linePrefix)
   {
      var context = MemberCompletionContextDetector.Detect(linePrefix);
      if (context.Kind == MemberContextKind.None)
         return Array.Empty<AutocompleteItem>();

      var objectId = MemberOperandResolver.Resolve(context, _contextObjectAccessor());
      if (objectId is null)
         return Array.Empty<AutocompleteItem>();

      var key = (context.Kind, objectId.Value.Number);
      lock (_stateLock)
      {
         if (_disposed)
            return Array.Empty<AutocompleteItem>();

         if (_cache.TryGetValue(key, out var entry))
         {
            if (DateTime.UtcNow - entry.CreatedUtc < _cacheTimeToLive)
               return entry.Items;

            _cache.Remove(key);
         }

         var provider = _providerAccessor();
         if (provider is null || _inflightKey == key)
            return Array.Empty<AutocompleteItem>();

         var staleFetchCancellation = _fetchCancellation;
         _fetchCancellation = new CancellationTokenSource();
         staleFetchCancellation?.Cancel();
         staleFetchCancellation?.Dispose();
         _inflightKey = key;
         _ = FetchAsync(provider, context.Kind, objectId.Value, key, _fetchCancellation.Token);
      }

      return Array.Empty<AutocompleteItem>();
   }

   /// <inheritdoc/>
   public void Dispose()
   {
      lock (_stateLock)
      {
         if (_disposed)
            return;

         _disposed = true;
         var fetchCancellation = _fetchCancellation;
         _fetchCancellation = null;
         fetchCancellation?.Cancel();
         fetchCancellation?.Dispose();
      }
   }

   private async Task FetchAsync(
      IMooWorldQueryProvider provider,
      MemberContextKind kind,
      MooObjectId objectId,
      (MemberContextKind Kind, int ObjectNumber) key,
      CancellationToken cancellationToken)
   {
      try
      {
         IReadOnlyList<AutocompleteItem> items;
         if (kind == MemberContextKind.Verb)
            items = BuildVerbItems(await provider.GetVerbsAsync(objectId, cancellationToken).ConfigureAwait(false));
         else
            items = BuildPropertyItems(await provider.GetPropertiesAsync(objectId, cancellationToken).ConfigureAwait(false), kind);

         _uiMarshal(() =>
         {
            lock (_stateLock)
            {
               if (_disposed || cancellationToken.IsCancellationRequested)
                  return;

               _cache[key] = new CacheEntry(items, DateTime.UtcNow);
               if (_inflightKey == key)
                  _inflightKey = null;
            }

            _menuRefresh();
         });
      }
      catch (Exception)
      {
         // Best-effort completion: any failure leaves only the static list in place.
         _uiMarshal(() =>
         {
            lock (_stateLock)
            {
               if (!_disposed && _inflightKey == key)
                  _inflightKey = null;
            }
         });
      }
   }

   private static IReadOnlyList<AutocompleteItem> BuildVerbItems(IReadOnlyList<MooVerbSummary> verbs)
   {
      // Flatten aliases, strip MOO prefix-match stars ("g*et" => "get"), de-duplicate and sort.
      var names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
      foreach (var verb in verbs)
         foreach (var alias in verb.Aliases)
         {
            var name = alias.Replace("*", string.Empty);
            if (name.Length > 0)
               names.Add(name);
         }

      return names.Select(name => (AutocompleteItem)new MemberCompletionItem(name, CompletionIconCategory.Verb)).ToList();
   }

   private static IReadOnlyList<AutocompleteItem> BuildPropertyItems(IReadOnlyList<MooPropertySummary> properties, MemberContextKind kind)
   {
      var category = kind == MemberContextKind.CoreReference
                        ? CompletionIconCategory.CoreReference
                        : CompletionIconCategory.Property;
      var names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
      foreach (var property in properties)
         if (!string.IsNullOrEmpty(property.Name))
            names.Add(property.Name);

      return names.Select(name => (AutocompleteItem)new MemberCompletionItem(name, category)).ToList();
   }
}
