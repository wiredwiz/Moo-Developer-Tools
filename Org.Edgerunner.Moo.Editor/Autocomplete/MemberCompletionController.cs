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
using System.Globalization;
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
/// <para>
/// An optional <c>diagnostic</c> callback (see constructor) receives a message string and
/// optional exception for fetch failures and core-name validation failures. It is invoked
/// outside any lock; if the callback itself throws the exception is silently swallowed so a
/// faulty diagnostic handler can never break completion.
/// </para>
/// </remarks>
public sealed class MemberCompletionController : IDisposable
{
   /// <summary>The default lifetime of a cached member list.</summary>
   public static readonly TimeSpan DefaultCacheTimeToLive = TimeSpan.FromSeconds(30);

   /// <summary>The MOO value type code for object references.</summary>
   private const int ObjectTypeCode = 1;

   private readonly Func<IMooWorldQueryProvider?> _providerAccessor;

   private readonly Func<MooObjectId?> _contextObjectAccessor;

   private readonly Action<Action> _uiMarshal;

   private readonly Action _menuRefresh;

   private readonly Action<string, Exception?>? _diagnostic;

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
   /// <param name="diagnostic">
   /// Optional callback invoked when a fetch fails or a core-name validation fails. Receives a
   /// descriptive message and, for exception-based failures, the exception (otherwise <c>null</c>).
   /// Invoked outside any lock; exceptions thrown by the callback are silently swallowed.
   /// </param>
   /// <exception cref="ArgumentNullException">Thrown when any required callback is <c>null</c>.</exception>
   public MemberCompletionController(
      Func<IMooWorldQueryProvider?> providerAccessor,
      Func<MooObjectId?> contextObjectAccessor,
      Action<Action> uiMarshal,
      Action menuRefresh,
      TimeSpan? cacheTimeToLive = null,
      Action<string, Exception?>? diagnostic = null)
   {
      _providerAccessor = providerAccessor ?? throw new ArgumentNullException(nameof(providerAccessor));
      _contextObjectAccessor = contextObjectAccessor ?? throw new ArgumentNullException(nameof(contextObjectAccessor));
      _uiMarshal = uiMarshal ?? throw new ArgumentNullException(nameof(uiMarshal));
      _menuRefresh = menuRefresh ?? throw new ArgumentNullException(nameof(menuRefresh));
      _cacheTimeToLive = cacheTimeToLive ?? DefaultCacheTimeToLive;
      _diagnostic = diagnostic;
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
               if (provider is null || InflightCoreNameMatches(inflightKey))
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
               // Always release the inflight marker when this fetch owns it, even if we are
               // about to discard the results due to disposal or cancellation.  Failing to do so
               // would leave the marker set permanently and silently suppress all future fetches
               // for this key.
               if (_inflightKey == key)
                  _inflightKey = null;

               if (_disposed || cancellationToken.IsCancellationRequested)
                  return;

               _cache[key] = new CacheEntry(items, DateTime.UtcNow);
            }

            _menuRefresh();
         });
      }
      catch (Exception ex)
      {
         // Best-effort completion: any failure leaves only the static list in place.
         InvokeDiagnostic(
            $"Member completion fetch failed for kind={kind}, object #{objectId.Number}.", ex);
         _uiMarshal(() =>
         {
            lock (_stateLock)
            {
               if (_inflightKey == key)
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
      var inflightKey = (Kind: kind, Name: name);
      try
      {
         var value = await provider.GetPropertyValueAsync(new MooObjectId(0), name, cancellationToken)
                                   .ConfigureAwait(false);

         // Validate: must be a non-null object-typed value with a parsable #N literal where N >= 0.
         // Use NumberStyles.None to reject whitespace and sign prefixes such as #+62 or #-1.
         if (value is null || value.Type != ObjectTypeCode ||
             !value.Literal.StartsWith('#') ||
             !int.TryParse(value.Literal.AsSpan(1), NumberStyles.None, CultureInfo.InvariantCulture, out var number) ||
             number < 0)
         {
            // Resolution failed — clear the inflight marker, do NOT cache, do NOT refresh.
            string diagnosticMessage;
            if (value is null)
               diagnosticMessage = $"Core reference ${name} did not resolve to an object: property value was null.";
            else if (value.Type != ObjectTypeCode)
               diagnosticMessage = $"Core reference ${name} did not resolve to an object: property value was type {value.Type}.";
            else
               diagnosticMessage = $"Core reference ${name} did not resolve to an object: literal '{value.Literal}' is not a valid object number.";
            InvokeDiagnostic(diagnosticMessage, null);
            _uiMarshal(() =>
            {
               lock (_stateLock)
               {
                  if (InflightCoreNameMatches(inflightKey))
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
               // Always release the inflight marker when this fetch owns it, even if we are
               // about to discard the results due to disposal or cancellation.  Failing to do so
               // would leave the marker set permanently and silently suppress all future fetches
               // for this core name.
               if (InflightCoreNameMatches(inflightKey))
                  _inflightCoreName = null;

               if (_disposed || cancellationToken.IsCancellationRequested)
                  return;

               _coreNameCache[name] = (resolvedId, DateTime.UtcNow);
               _cache[memberKey] = new CacheEntry(items, DateTime.UtcNow);
            }

            _menuRefresh();
         });
      }
      catch (Exception ex)
      {
         // Best-effort completion: any failure leaves only the static list in place.
         InvokeDiagnostic(
            $"Member completion fetch failed for core name ${name} (kind={kind}).", ex);
         _uiMarshal(() =>
         {
            lock (_stateLock)
            {
               if (InflightCoreNameMatches(inflightKey))
                  _inflightCoreName = null;
            }
         });
      }
   }

   /// <summary>
   /// Invokes the optional <see cref="_diagnostic"/> callback with the supplied message and
   /// exception. Exceptions thrown by the callback are silently swallowed so that a faulty
   /// diagnostic handler can never break completion.
   /// </summary>
   /// <param name="message">A human-readable description of the failure.</param>
   /// <param name="ex">The exception that caused the failure, or <c>null</c> for validation failures.</param>
   private void InvokeDiagnostic(string message, Exception? ex)
   {
      if (_diagnostic is null) return;
      try { _diagnostic(message, ex); }
      catch { /* swallow: a faulty diagnostic must not break completion */ }
   }

   /// <summary>
   /// Returns <c>true</c> when <see cref="_inflightCoreName"/> matches the supplied fetch key.
   /// Must be called under <see cref="_stateLock"/>.
   /// </summary>
   /// <param name="fetchKey">The <c>(Kind, Name)</c> tuple that identifies the fetch in progress.</param>
   /// <returns><c>true</c> when the current inflight marker belongs to this fetch.</returns>
   private bool InflightCoreNameMatches((MemberContextKind Kind, string Name) fetchKey) =>
      _inflightCoreName.HasValue &&
      _inflightCoreName.Value.Kind == fetchKey.Kind &&
      string.Equals(_inflightCoreName.Value.Name, fetchKey.Name, StringComparison.OrdinalIgnoreCase);

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
