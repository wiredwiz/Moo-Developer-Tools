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
/// <para>
/// Core-name operands (<c>$foo</c>) are resolved asynchronously: the controller queries
/// <c>#0</c>'s property value for the name to obtain the target object id, then proceeds with
/// the normal verb/property fetch. Resolved names are cached alongside member lists (using the
/// same <c>_cacheTimeToLive</c>), keyed case-insensitively since MOO property lookup is
/// case-insensitive.
/// </para>
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

   private readonly Dictionary<string, (MooObjectId Id, DateTime CreatedUtc)> _coreNameCache =
      new(StringComparer.OrdinalIgnoreCase);

   private (MemberContextKind Kind, int ObjectNumber)? _inflightKey;

   private (MemberContextKind Kind, string Name)? _inflightCoreName;

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
      {
         // Check for a core-name operand ($foo) that needs async resolution.
         if (!MemberOperandResolver.TryGetCoreName(context, out var coreName))
            return Array.Empty<AutocompleteItem>();

         lock (_stateLock)
         {
            if (_disposed)
               return Array.Empty<AutocompleteItem>();

            // Check if the core name is already resolved (and not stale).
            if (_coreNameCache.TryGetValue(coreName, out var coreEntry))
            {
               if (DateTime.UtcNow - coreEntry.CreatedUtc < _cacheTimeToLive)
               {
                  // Name is resolved — fall through to the regular member fetch below using the cached id.
                  objectId = coreEntry.Id;
               }
               else
               {
                  _coreNameCache.Remove(coreName);
               }
            }

            if (objectId is null)
            {
               // Need to resolve the name first.
               var provider = _providerAccessor();
               var inflightKey = (context.Kind, coreName);
               if (provider is null || (_inflightCoreName.HasValue &&
                   _inflightCoreName.Value.Kind == context.Kind &&
                   string.Equals(_inflightCoreName.Value.Name, coreName, StringComparison.OrdinalIgnoreCase)))
                  return Array.Empty<AutocompleteItem>();

               var staleFetchCancellation = _fetchCancellation;
               _fetchCancellation = new CancellationTokenSource();
               staleFetchCancellation?.Cancel();
               staleFetchCancellation?.Dispose();
               _inflightCoreName = inflightKey;
               _ = FetchCoreNameAsync(provider, context.Kind, coreName, _fetchCancellation.Token);
               return Array.Empty<AutocompleteItem>();
            }
         }
      }

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

   private async Task FetchCoreNameAsync(
      IMooWorldQueryProvider provider,
      MemberContextKind kind,
      string name,
      CancellationToken cancellationToken)
   {
      var inflightKey = (kind, name);
      try
      {
         var value = await provider.GetPropertyValueAsync(new MooObjectId(0), name, cancellationToken)
                                   .ConfigureAwait(false);

         // Validate: must be a non-null object-typed value with a parsable #N literal.
         if (value is null || value.Type != 1 ||
             !value.Literal.StartsWith('#') ||
             !int.TryParse(value.Literal[1..], out var number))
         {
            // Resolution failed — clear the inflight marker, do NOT cache, do NOT refresh.
            _uiMarshal(() =>
            {
               lock (_stateLock)
               {
                  if (!_disposed &&
                      _inflightCoreName.HasValue &&
                      _inflightCoreName.Value.Kind == inflightKey.kind &&
                      string.Equals(_inflightCoreName.Value.Name, inflightKey.name, StringComparison.OrdinalIgnoreCase))
                     _inflightCoreName = null;
               }
            });
            return;
         }

         var resolvedId = new MooObjectId(number);

         // Now fetch the member list for the resolved object.
         IReadOnlyList<AutocompleteItem> items;
         if (kind == MemberContextKind.Verb)
            items = BuildVerbItems(await provider.GetVerbsAsync(resolvedId, cancellationToken).ConfigureAwait(false));
         else
            items = BuildPropertyItems(await provider.GetPropertiesAsync(resolvedId, cancellationToken).ConfigureAwait(false), kind);

         var memberKey = (kind, resolvedId.Number);

         _uiMarshal(() =>
         {
            lock (_stateLock)
            {
               if (_disposed || cancellationToken.IsCancellationRequested)
                  return;

               _coreNameCache[name] = (resolvedId, DateTime.UtcNow);
               _cache[memberKey] = new CacheEntry(items, DateTime.UtcNow);
               if (_inflightCoreName.HasValue &&
                   _inflightCoreName.Value.Kind == inflightKey.kind &&
                   string.Equals(_inflightCoreName.Value.Name, inflightKey.name, StringComparison.OrdinalIgnoreCase))
                  _inflightCoreName = null;
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
               if (!_disposed &&
                   _inflightCoreName.HasValue &&
                   _inflightCoreName.Value.Kind == inflightKey.kind &&
                   string.Equals(_inflightCoreName.Value.Name, inflightKey.name, StringComparison.OrdinalIgnoreCase))
                  _inflightCoreName = null;
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
