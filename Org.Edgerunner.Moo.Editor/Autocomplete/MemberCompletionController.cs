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
using Antlr4.Runtime;
using FastColoredTextBoxNS.Types;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar;
using Org.Edgerunner.Mud.Common.Querying;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// Supplies world-queried member completion items (verbs, properties, core references) to the
/// autocomplete popup. The caret's chained member-access expression is extracted to a
/// <see cref="ChainDescriptor"/>, resolved to a target object via the <see cref="ChainExpressionEvaluator"/>,
/// then queried for members. Synchronous cache hits return immediately; misses start a single
/// background resolve+fetch and return nothing, and the menu is refreshed when results arrive.
/// </summary>
/// <remarks>
/// In production all state mutation happens on the UI thread (popup enumeration plus actions the
/// owner marshals there), but a lock guards the caches and in-flight state anyway so that hosts
/// (and tests) with an immediate marshal are also safe. Member completion is best-effort: provider
/// failures (timeout, cancellation, disconnect, protocol errors) are swallowed and simply leave
/// the static completion list in place. The menu-refresh callback is invoked outside the state
/// lock and may therefore fire after disposal; owners must supply a callback that is safe to call
/// on a closed editor.
/// <para>
/// Two cache layers are kept differently. World/server state — <c>(objectId, propName)</c> resolving
/// to an object id (e.g. <c>#0.Mcp -> #123</c>) — is verb-independent and TTL-cached. Local-variable
/// values are derived from the live parse tree on every request and are <em>never</em> cached, which
/// is what keeps resolution unique per verb / per window.
/// </para>
/// <para>
/// An optional <c>diagnostic</c> callback (see constructor) receives a message string and optional
/// exception for unexpected failures (a provider throw, a parse-tree access failure). Ordinary
/// non-resolution (a non-object step, a missing property, a non-chain right-hand side, a cycle, the
/// depth budget, an unknown base) yields no source <em>without</em> a warning so the log is not
/// flooded on every keystroke. The callback is invoked outside any lock; exceptions it throws are
/// swallowed so a faulty diagnostic handler can never break completion.
/// </para>
/// </remarks>
public sealed class MemberCompletionController : IDisposable
{
   /// <summary>The default lifetime of a cached member list / resolved world-state entry.</summary>
   public static readonly TimeSpan DefaultCacheTimeToLive = TimeSpan.FromSeconds(30);

   /// <summary>The MOO value type code for object references.</summary>
   private const int ObjectTypeCode = 1;

   private readonly Func<IMooWorldQueryProvider?> _providerAccessor;

   private readonly Func<MooObjectId?> _contextObjectAccessor;

   private readonly Action<Action> _uiMarshal;

   private readonly Action _menuRefresh;

   private readonly Action<string, Exception?>? _diagnostic;

   /// <summary>
   /// Gets or sets the lifetime of a cached member list / resolved world-state entry. Settable at
   /// runtime so a changed editor-cache TTL setting takes effect on already-open editors without a reopen.
   /// </summary>
   /// <value>The cache entry lifetime; defaults to <see cref="DefaultCacheTimeToLive"/>.</value>
   public TimeSpan CacheTimeToLive { get; set; } = DefaultCacheTimeToLive;

   private readonly object _stateLock = new();

   private readonly Dictionary<(MemberContextKind Kind, int ObjectNumber), CacheEntry> _cache = new();

   // World/server state: (objectId, propName) -> resolved object id. Generalizes the old core-name
   // cache (which was the (#0, name) special case). Verb-independent; shared across requests.
   private readonly Dictionary<(int ObjectNumber, string PropertyName), (MooObjectId Id, DateTime CreatedUtc)>
      _propertyObjectCache = new(PropertyKeyComparer.Instance);

   private (MooObjectId Id, DateTime CreatedUtc)? _currentPlayerCache;

   private (MemberContextKind Kind, int ObjectNumber)? _inflightKey;

   private string? _inflightChain;

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
   /// Optional callback invoked on an unexpected failure (a provider throw or a parse-tree access
   /// failure). Receives a descriptive message and the exception. Ordinary non-resolution does not
   /// invoke it. Invoked outside any lock; exceptions thrown by the callback are silently swallowed.
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
      CacheTimeToLive = cacheTimeToLive ?? DefaultCacheTimeToLive;
      _diagnostic = diagnostic;
   }

   /// <summary>
   /// Gets the member completion items for the caret position. Supply the current parse tree and
   /// token list to enable chained-expression resolution; with only a line prefix, single-atom
   /// (length-1) chains are resolved. Returns an empty list outside member contexts, for unresolved
   /// operands, without a provider, or while a resolve/fetch is still in flight (the menu is
   /// refreshed when it completes).
   /// </summary>
   /// <param name="linePrefix">The text on the caret line, from column 0 up to the caret.</param>
   /// <param name="tree">The current parse tree root (optional; enables chain resolution).</param>
   /// <param name="tokens">The current detailed token list (optional; used as a chain-tail fallback).</param>
   /// <param name="caretLine">The 1-based caret line (used with <paramref name="tree"/>).</param>
   /// <param name="caretColumn">The 1-based caret column (used with <paramref name="tree"/>).</param>
   /// <returns>The items to offer; never <c>null</c>.</returns>
   public IReadOnlyList<AutocompleteItem> GetMemberItems(
      string linePrefix,
      ParserRuleContext? tree = null,
      IList<DetailedToken>? tokens = null,
      int caretLine = 0,
      int caretColumn = 0)
   {
      ChainDescriptor? descriptor;
      int caretOffset;
      if (tree is not null && caretLine > 0 && caretColumn > 0)
      {
         var buffer = ReconstructBuffer(tree, linePrefix, caretLine, caretColumn);
         descriptor = ChainExtractor.Extract(buffer, caretLine, caretColumn, tokens, tree, InvokeDiagnostic);
         caretOffset = AbsoluteCaretOffset(buffer, caretLine, caretColumn);
      }
      else
      {
         descriptor = ChainExtractor.ExtractFromLinePrefix(linePrefix);
         caretOffset = 0;
      }

      if (descriptor is null)
         return Array.Empty<AutocompleteItem>();

      // Top-level keyword/variable bases are flow-resolved from the live tree on every request and never
      // cached: this is what keeps reassignment-aware resolution unique per verb / per window.
      FlowValue FlowResolver(string name) =>
         tree is null
            ? new FlowValue(
               name is "this" or "player" or "caller" ? FlowValueKind.UseDefault : FlowValueKind.Unknown, null)
            : FlowValueResolver.ResolveCurrentValue(name, tree, caretOffset, InvokeDiagnostic);

      lock (_stateLock)
      {
         if (_disposed)
            return Array.Empty<AutocompleteItem>();

         // Fast path: every base/step needed is already cached, so resolve the target synchronously.
         if (TryResolveFromCache(descriptor, FlowResolver, out var resolved))
         {
            if (resolved is null)
               return Array.Empty<AutocompleteItem>();

            var memberKey = (descriptor.MemberKind, resolved.Value.Number);
            if (_cache.TryGetValue(memberKey, out var entry))
            {
               if (DateTime.UtcNow - entry.CreatedUtc < CacheTimeToLive)
                  return entry.Items;
               _cache.Remove(memberKey);
            }

            var memberProvider = _providerAccessor();
            if (memberProvider is null || _inflightKey == memberKey)
               return Array.Empty<AutocompleteItem>();

            BeginFetch();
            _inflightKey = memberKey;
            _ = FetchMembersAsync(memberProvider, descriptor.MemberKind, resolved.Value, memberKey, _fetchCancellation!.Token);
            return Array.Empty<AutocompleteItem>();
         }

         // Slow path: resolution needs at least one uncached world query. Run it in the background.
         var provider = _providerAccessor();
         var chainId = CanonicalChain(descriptor);
         if (provider is null || _inflightChain == chainId)
            return Array.Empty<AutocompleteItem>();

         BeginFetch();
         _inflightChain = chainId;
         _ = ResolveAndFetchAsync(provider, descriptor, chainId, FlowResolver, _fetchCancellation!.Token);
      }

      return Array.Empty<AutocompleteItem>();
   }

   /// <summary>
   /// Clears every cache and in-flight marker so the next request re-queries the world from scratch.
   /// Used by the Tools &gt; Flush Editor Cache action to force newly-added verbs/properties and stale
   /// values to be re-fetched immediately. A no-op after disposal.
   /// </summary>
   public void ClearCache()
   {
      lock (_stateLock)
      {
         if (_disposed)
            return;

         _cache.Clear();
         _propertyObjectCache.Clear();
         _currentPlayerCache = null;
         _inflightKey = null;
         _inflightChain = null;
      }
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

   // Swaps in a fresh cancellation source, cancelling any prior in-flight work, and eagerly releases
   // both in-flight markers so a switch between the fast (member) and slow (resolve) paths cannot leave
   // a stale marker that briefly suppresses a new request. Any task that owned a marker only clears it
   // under an "== current key/chain" guard, so eager release here cannot corrupt the markers the caller
   // is about to set. Caller holds the lock.
   private void BeginFetch()
   {
      var stale = _fetchCancellation;
      _fetchCancellation = new CancellationTokenSource();
      _inflightKey = null;
      _inflightChain = null;
      stale?.Cancel();
      stale?.Dispose();
   }

   // Attempts to resolve the chain entirely from caches (no provider calls). Returns false on any miss.
   // Caller holds the lock.
   private bool TryResolveFromCache(
      ChainDescriptor descriptor, Func<string, FlowValue> flowResolver, out MooObjectId? resolved)
   {
      resolved = null;
      return TryResolveBaseFromCache(descriptor, flowResolver,
                                     new[] { ChainExpressionEvaluator.MaxResolutionSteps }, out resolved);
   }

   // Reduces a top-level keyword/variable base through the flow resolver to a GROUND chain (already
   // offset-correct and variable-free), then walks ground.Steps ++ descriptor.Steps against the caches.
   // Mirrors ChainExpressionEvaluator's flow handling so the sync fast path and async path agree.
   private bool TryResolveBaseFromCache(
      ChainDescriptor descriptor,
      Func<string, FlowValue> flowResolver,
      int[] budget,
      out MooObjectId? resolved)
   {
      resolved = null;

      ChainBase groundBase;
      IReadOnlyList<string> steps;

      switch (descriptor.Base.Kind)
      {
         case ChainBaseKind.This:
         case ChainBaseKind.Player:
         case ChainBaseKind.Caller:
         case ChainBaseKind.Variable:
         {
            var name = descriptor.Base.Text.Length > 0 ? descriptor.Base.Text : KeywordName(descriptor.Base.Kind);
            var flow = flowResolver(name);
            switch (flow.Kind)
            {
               case FlowValueKind.Reassigned when flow.Chain is not null:
                  groundBase = flow.Chain.Base;
                  var merged = new List<string>(flow.Chain.Steps);
                  merged.AddRange(descriptor.Steps);
                  steps = merged;
                  break;

               case FlowValueKind.UseDefault:
                  // Keyword default: resolve the original keyword base + steps (player/context cache logic).
                  groundBase = descriptor.Base;
                  steps = descriptor.Steps;
                  break;

               default:
                  return true; // Unknown -> determined no source.
            }

            break;
         }

         default:
            // ObjectLiteral / CoreName: already ground.
            groundBase = descriptor.Base;
            steps = descriptor.Steps;
            break;
      }

      MooObjectId? current;
      switch (groundBase.Kind)
      {
         case ChainBaseKind.ObjectLiteral:
            if (!int.TryParse(groundBase.Text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var n))
               return true; // unresolvable but determined: no source
            current = new MooObjectId(n);
            break;

         case ChainBaseKind.This:
            current = _contextObjectAccessor();
            if (current is null)
               return true; // determined: no source
            break;

         case ChainBaseKind.Player:
         case ChainBaseKind.Caller:
            if (_currentPlayerCache is { } player && DateTime.UtcNow - player.CreatedUtc < CacheTimeToLive)
               current = player.Id;
            else
               return false; // miss: needs async player query
            break;

         case ChainBaseKind.CoreName:
            if (budget[0]-- <= 0)
               return true; // determined: budget exhausted -> no source
            if (!TryGetCachedPropertyObject(0, groundBase.Text, out current))
               return false; // miss
            break;

         default:
            // A Variable base must never reach here (the resolver guarantees full reduction).
            return true;
      }

      if (current is null)
         return true;

      // Walk the merged property steps from caches.
      foreach (var step in steps)
      {
         if (budget[0]-- <= 0)
            return true;
         if (!TryGetCachedPropertyObject(current.Value.Number, step, out current))
            return false; // miss
         if (current is null)
            return true;
      }

      resolved = current;
      return true;
   }

   private static string KeywordName(ChainBaseKind kind) => kind switch
   {
      ChainBaseKind.This => "this",
      ChainBaseKind.Player => "player",
      ChainBaseKind.Caller => "caller",
      _ => string.Empty,
   };

   private bool TryGetCachedPropertyObject(int objectNumber, string propertyName, out MooObjectId? value)
   {
      value = null;
      if (_propertyObjectCache.TryGetValue((objectNumber, propertyName), out var entry))
      {
         if (DateTime.UtcNow - entry.CreatedUtc < CacheTimeToLive)
         {
            value = entry.Id;
            return true;
         }

         _propertyObjectCache.Remove((objectNumber, propertyName));
      }

      return false;
   }

   // Background resolve (via the evaluator, populating the world-state cache) then member fetch.
   private async Task ResolveAndFetchAsync(
      IMooWorldQueryProvider provider,
      ChainDescriptor descriptor,
      string chainId,
      Func<string, FlowValue> flowResolver,
      CancellationToken cancellationToken)
   {
      try
      {
         var evaluator = new ChainExpressionEvaluator(
            (obj, prop, ct) => ResolvePropertyObjectAsync(provider, obj, prop, ct),
            ct => ResolveCurrentPlayerAsync(provider, ct),
            flowResolver,
            InvokeDiagnostic);

         var resolved = await evaluator.EvaluateAsync(descriptor, _contextObjectAccessor(), cancellationToken)
                                       .ConfigureAwait(false);

         if (resolved is null)
         {
            // Expected non-resolution: clear the marker, do not cache, do not refresh.
            _uiMarshal(() =>
            {
               lock (_stateLock)
               {
                  if (_inflightChain == chainId)
                     _inflightChain = null;
               }
            });
            return;
         }

         var memberKey = (descriptor.MemberKind, resolved.Value.Number);
         var items = await FetchMemberListAsync(provider, descriptor.MemberKind, resolved.Value, cancellationToken)
                        .ConfigureAwait(false);

         _uiMarshal(() =>
         {
            lock (_stateLock)
            {
               if (_inflightChain == chainId)
                  _inflightChain = null;

               if (_disposed || cancellationToken.IsCancellationRequested)
                  return;

               _cache[memberKey] = new CacheEntry(items, DateTime.UtcNow);
            }

            _menuRefresh();
         });
      }
      catch (Exception ex)
      {
         InvokeDiagnostic($"Member completion resolve/fetch failed for chain '{chainId}'.", ex);
         _uiMarshal(() =>
         {
            lock (_stateLock)
            {
               if (_inflightChain == chainId)
                  _inflightChain = null;
            }
         });
      }
   }

   // Resolves (objectId, propName) to the object the property holds, caching successful results.
   private async Task<MooObjectId?> ResolvePropertyObjectAsync(
      IMooWorldQueryProvider provider, MooObjectId objectId, string propertyName, CancellationToken cancellationToken)
   {
      lock (_stateLock)
      {
         if (TryGetCachedPropertyObject(objectId.Number, propertyName, out var cached))
            return cached;
      }

      var value = await provider.GetPropertyValueAsync(objectId, propertyName, cancellationToken).ConfigureAwait(false);

      // Require a non-null object-typed value with a parsable #N literal where N >= 0. Use
      // NumberStyles.None to reject whitespace and sign prefixes such as #+62 or #-1.
      if (value is null || value.Type != ObjectTypeCode ||
          !value.Literal.StartsWith('#') ||
          !int.TryParse(value.Literal.AsSpan(1), NumberStyles.None, CultureInfo.InvariantCulture, out var number) ||
          number < 0)
         return null; // expected non-resolution: not cached, not logged

      var resolved = new MooObjectId(number);
      lock (_stateLock)
      {
         if (!_disposed)
            _propertyObjectCache[(objectId.Number, propertyName)] = (resolved, DateTime.UtcNow);
      }

      return resolved;
   }

   private async Task<MooObjectId?> ResolveCurrentPlayerAsync(
      IMooWorldQueryProvider provider, CancellationToken cancellationToken)
   {
      lock (_stateLock)
      {
         if (_currentPlayerCache is { } cached && DateTime.UtcNow - cached.CreatedUtc < CacheTimeToLive)
            return cached.Id;
      }

      var player = await provider.GetCurrentPlayerAsync(cancellationToken).ConfigureAwait(false);
      if (player is null)
         return null;

      lock (_stateLock)
      {
         if (!_disposed)
            _currentPlayerCache = (player.Value, DateTime.UtcNow);
      }

      return player.Value;
   }

   // Member fetch for an already-resolved object (fast path).
   private async Task FetchMembersAsync(
      IMooWorldQueryProvider provider,
      MemberContextKind kind,
      MooObjectId objectId,
      (MemberContextKind Kind, int ObjectNumber) key,
      CancellationToken cancellationToken)
   {
      try
      {
         var items = await FetchMemberListAsync(provider, kind, objectId, cancellationToken).ConfigureAwait(false);

         _uiMarshal(() =>
         {
            lock (_stateLock)
            {
               // Always release the inflight marker when this fetch owns it, even when discarding the
               // results due to disposal or cancellation, or the marker would suppress all future fetches.
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
         InvokeDiagnostic($"Member completion fetch failed for kind={kind}, object #{objectId.Number}.", ex);
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

   private async Task<IReadOnlyList<AutocompleteItem>> FetchMemberListAsync(
      IMooWorldQueryProvider provider, MemberContextKind kind, MooObjectId objectId, CancellationToken cancellationToken)
   {
      if (kind == MemberContextKind.Verb)
         return BuildVerbItems(await provider.GetVerbsAsync(objectId, cancellationToken).ConfigureAwait(false));

      if (kind == MemberContextKind.CoreReference)
      {
         // A core reference ($foo) can be either a #0 property ($foo) or a #0 verb call ($foo()).
         // List both: properties keep the core-reference icon, verbs use the verb icon and insert
         // paren-call text (via VerbCompletionItem). A name present as both yields two entries — #0
         // can carry a property foo AND a verb foo, and FCTB does not dedup by text.
         var properties = await provider.GetPropertiesAsync(objectId, cancellationToken).ConfigureAwait(false);
         var verbs = await provider.GetVerbsAsync(objectId, cancellationToken).ConfigureAwait(false);
         var merged = new List<AutocompleteItem>(BuildPropertyItems(properties, kind));
         merged.AddRange(BuildVerbItems(verbs));
         // Return one alphabetical list with verbs and properties INTERLEAVED, never two
         // separate runs (the global rule: every completion list is sorted as a whole).
         // OrderBy is stable, so on a same-name tie the property — added first — precedes the
         // verb; both entries are preserved (no dedup by text).
         return merged.OrderBy(item => item.Text, StringComparer.OrdinalIgnoreCase).ToList();
      }

      return BuildPropertyItems(await provider.GetPropertiesAsync(objectId, cancellationToken).ConfigureAwait(false), kind);
   }

   // The canonical textual form of a chain, used to dedup concurrent in-flight resolutions.
   private static string CanonicalChain(ChainDescriptor descriptor)
   {
      var basePart = descriptor.Base.Kind switch
      {
         ChainBaseKind.ObjectLiteral => $"#{descriptor.Base.Text}",
         ChainBaseKind.CoreName => $"${descriptor.Base.Text}",
         ChainBaseKind.This => "this",
         ChainBaseKind.Player => "player",
         ChainBaseKind.Caller => "caller",
         _ => $"var:{descriptor.Base.Text}",
      };
      var steps = descriptor.Steps.Count > 0 ? "." + string.Join(".", descriptor.Steps) : string.Empty;
      var separator = descriptor.MemberKind == MemberContextKind.Verb ? ":" : ".";
      return $"{basePart}{steps}{separator}";
   }

   // Reconstructs the buffer text from the parse tree so the extractor sees the same input the parser did.
   private static string ReconstructBuffer(ParserRuleContext tree, string linePrefix, int caretLine, int caretColumn)
   {
      var inputStream = tree.Start?.InputStream;
      if (inputStream is not null)
         return inputStream.GetText(Antlr4.Runtime.Misc.Interval.Of(0, inputStream.Size - 1));

      // Fallback: the single caret line (only the line-prefix region matters to the extractor).
      return linePrefix;
   }

   private static int AbsoluteCaretOffset(string buffer, int caretLine, int caretColumn)
   {
      var line = 1;
      var offset = 0;
      while (line < caretLine && offset < buffer.Length)
      {
         var newline = buffer.IndexOf('\n', offset);
         if (newline < 0)
            break;
         offset = newline + 1;
         line++;
      }

      var target = offset + (caretColumn - 1);
      return Math.Min(Math.Max(target, 0), buffer.Length);
   }

   /// <summary>
   /// Invokes the optional diagnostic callback. Exceptions thrown by the callback are silently
   /// swallowed so that a faulty diagnostic handler can never break completion.
   /// </summary>
   /// <param name="message">A human-readable description of the failure.</param>
   /// <param name="ex">The exception that caused the failure, or <c>null</c> for validation failures.</param>
   private void InvokeDiagnostic(string message, Exception? ex)
   {
      if (_diagnostic is null) return;
      try { _diagnostic(message, ex); }
      catch { /* swallow: a faulty diagnostic must not break completion */ }
   }

   private static IReadOnlyList<AutocompleteItem> BuildVerbItems(IReadOnlyList<MooVerbSummary> verbs)
   {
      // Flatten aliases, strip MOO prefix-match stars ("g*et" => "get"), de-duplicate and sort,
      // carrying each name's origin so inherited verbs get the badged icon.
      var origins = new Dictionary<string, MemberOrigin>(StringComparer.OrdinalIgnoreCase);
      foreach (var verb in verbs)
         foreach (var alias in verb.Aliases)
         {
            var name = alias.Replace("*", string.Empty);
            if (name.Length > 0)
               MergeOrigin(origins, name, verb.Origin);
         }

      return origins.Keys
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .Select(name => (AutocompleteItem)new VerbCompletionItem(
                       name,
                       origins[name] == MemberOrigin.Inherited
                          ? CompletionIconCategory.VerbInherited
                          : CompletionIconCategory.Verb))
                    .ToList();
   }

   private static IReadOnlyList<AutocompleteItem> BuildPropertyItems(IReadOnlyList<MooPropertySummary> properties, MemberContextKind kind)
   {
      var baseCategory = kind == MemberContextKind.CoreReference
                            ? CompletionIconCategory.CoreReference
                            : CompletionIconCategory.Property;
      var origins = new Dictionary<string, MemberOrigin>(StringComparer.OrdinalIgnoreCase);
      foreach (var property in properties)
         if (!string.IsNullOrEmpty(property.Name))
            MergeOrigin(origins, property.Name, property.Origin);

      return origins.Keys
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .Select(name => (AutocompleteItem)new MemberCompletionItem(
                       name,
                       // CoreReference keeps its own icon; only the regular property context
                       // distinguishes inherited members.
                       baseCategory == CompletionIconCategory.Property && origins[name] == MemberOrigin.Inherited
                          ? CompletionIconCategory.PropertyInherited
                          : baseCategory))
                    .ToList();
   }

   /// <summary>
   /// Records the origin for a member name, de-duplicating case-insensitively. When the same name
   /// appears under two origins, a known origin deterministically wins over
   /// <see cref="MemberOrigin.Unknown"/>.
   /// </summary>
   /// <param name="origins">The accumulated name-to-origin map.</param>
   /// <param name="name">The member name.</param>
   /// <param name="origin">The origin to merge in.</param>
   private static void MergeOrigin(IDictionary<string, MemberOrigin> origins, string name, MemberOrigin origin)
   {
      if (!origins.TryGetValue(name, out var existing))
      {
         origins[name] = origin;
         return;
      }

      if (existing == MemberOrigin.Unknown && origin != MemberOrigin.Unknown)
         origins[name] = origin;
   }

   // Case-insensitive comparison of (objectNumber, propertyName) keys (MOO property lookup is case-insensitive).
   private sealed class PropertyKeyComparer : IEqualityComparer<(int ObjectNumber, string PropertyName)>
   {
      public static readonly PropertyKeyComparer Instance = new();

      public bool Equals((int ObjectNumber, string PropertyName) x, (int ObjectNumber, string PropertyName) y) =>
         x.ObjectNumber == y.ObjectNumber &&
         string.Equals(x.PropertyName, y.PropertyName, StringComparison.OrdinalIgnoreCase);

      public int GetHashCode((int ObjectNumber, string PropertyName) obj) =>
         HashCode.Combine(obj.ObjectNumber, StringComparer.OrdinalIgnoreCase.GetHashCode(obj.PropertyName));
   }
}
