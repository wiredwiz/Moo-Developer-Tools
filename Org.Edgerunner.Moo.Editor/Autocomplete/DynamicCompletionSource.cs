#region BSD 3-Clause License
// <copyright file="DynamicCompletionSource.cs" company="Edgerunner.org">
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
using System.Collections;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using FastColoredTextBoxNS.Types;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// A snapshot of the editor state needed to resolve a chained member-access expression at the caret.
/// </summary>
/// <param name="Tree">The current parse tree root.</param>
/// <param name="Tokens">The current detailed token list.</param>
/// <param name="CaretLine">The 1-based caret line.</param>
/// <param name="CaretColumn">The 1-based caret column.</param>
public readonly record struct MemberCompletionContextSnapshot(
   ParserRuleContext? Tree,
   IList<DetailedToken>? Tokens,
   int CaretLine,
   int CaretColumn);

/// <summary>
/// The autocomplete item source handed to the popup menu. The menu re-enumerates its source on
/// every refresh, so each enumeration injects the current world-queried member items (if any)
/// ahead of the static keyword/builtin/snippet items.
/// </summary>
public sealed class DynamicCompletionSource : IEnumerable<AutocompleteItem>
{
   private readonly IReadOnlyList<AutocompleteItem> _staticItems;

   private readonly MemberCompletionController _controller;

   private readonly Func<string> _linePrefixProvider;

   private readonly Func<MemberCompletionContextSnapshot>? _contextProvider;

   /// <summary>
   /// Initializes a new instance of the <see cref="DynamicCompletionSource"/> class.
   /// </summary>
   /// <param name="staticItems">The static completion items (keywords, builtins, snippets).</param>
   /// <param name="controller">The member completion controller.</param>
   /// <param name="linePrefixProvider">Returns the caret line text from column 0 up to the caret.</param>
   /// <param name="contextProvider">
   /// Optional: returns the current parse tree, tokens, and caret position so chained member-access
   /// expressions can be resolved. When <c>null</c>, only single-atom (length-1) chains resolve.
   /// </param>
   /// <exception cref="ArgumentNullException">Thrown when any required argument is <c>null</c>.</exception>
   public DynamicCompletionSource(
      IReadOnlyList<AutocompleteItem> staticItems,
      MemberCompletionController controller,
      Func<string> linePrefixProvider,
      Func<MemberCompletionContextSnapshot>? contextProvider = null)
   {
      _staticItems = staticItems ?? throw new ArgumentNullException(nameof(staticItems));
      _controller = controller ?? throw new ArgumentNullException(nameof(controller));
      _linePrefixProvider = linePrefixProvider ?? throw new ArgumentNullException(nameof(linePrefixProvider));
      _contextProvider = contextProvider;
   }

   /// <inheritdoc/>
   public IEnumerator<AutocompleteItem> GetEnumerator()
   {
      var linePrefix = _linePrefixProvider();
      var context = _contextProvider?.Invoke();
      var memberItems = context is { } snapshot
         ? _controller.GetMemberItems(linePrefix, snapshot.Tree, snapshot.Tokens, snapshot.CaretLine, snapshot.CaretColumn)
         : _controller.GetMemberItems(linePrefix);

      // Member items (world-queried verbs/properties) yield first, unchanged.
      foreach (var item in memberItems)
         yield return item;

      // Variables are harvested dynamically from the live parse tree on every popup refresh (they
      // change on every keystroke), then merged into the already-sorted static keyword/builtin/snippet
      // list so the whole non-member block stays alphabetical. Matching stays closest-match
      // (framework-handled); we only control sort/yield order. A variable in a member context never
      // shows because its text cannot start with a "base:partial" fragment.
      var variableItems = context is { Tree: not null } ctx
         ? BuildVariableItems(VariableReferenceCollector.CollectVariableNames(ctx.Tree, ComputeCaretOffset(ctx)))
         : Array.Empty<AutocompleteItem>();

      foreach (var item in Merge(_staticItems, variableItems))
         yield return item;
   }

   /// <summary>
   /// Wraps harvested variable names as <see cref="CompletionIconCategory.Variable"/> completion items,
   /// applying the asymmetric dedup against the static list: a name that matches a built-in variable /
   /// keyword (<c>Moo.Keywords</c>) is suppressed (the keyword entry already is that variable),
   /// while a name that matches a built-in function (<c>Moo.Builtins</c>) is kept (a local
   /// shadowing a function is meaningful, and the two are distinguished by icon and inserted text).
   /// The result is sorted alphabetically (case-insensitive) by name.
   /// </summary>
   /// <param name="variableNames">The harvested, caret-excluded, de-duplicated variable names.</param>
   /// <returns>The variable completion items, alphabetically sorted; never <c>null</c>.</returns>
   public static IReadOnlyList<AutocompleteItem> BuildVariableItems(IReadOnlyCollection<string> variableNames)
   {
      if (variableNames is null || variableNames.Count == 0)
         return Array.Empty<AutocompleteItem>();

      var keywords = new HashSet<string>(Org.Edgerunner.Moo.Editor.Moo.Keywords, StringComparer.OrdinalIgnoreCase);
      var items = new List<AutocompleteItem>();
      foreach (var name in variableNames)
      {
         if (string.IsNullOrEmpty(name) || keywords.Contains(name))
            continue;
         items.Add(new AutocompleteItem(name, (int)CompletionIconCategory.Variable));
      }

      items.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.CurrentCultureIgnoreCase));
      return items;
   }

   // Merges the (already alphabetically sorted) static list with the sorted variable list into one
   // alphabetical sequence. The static list's trailing behavioral snippets (insert-space / format-comma)
   // sit after the sorted block; they only match their own regex via Compare, so their final position
   // is cosmetic and they remain at the tail.
   private static IEnumerable<AutocompleteItem> Merge(
      IReadOnlyList<AutocompleteItem> staticItems, IReadOnlyList<AutocompleteItem> variableItems)
   {
      var v = 0;
      foreach (var staticItem in staticItems)
      {
         var staticText = staticItem.ToString();
         while (v < variableItems.Count &&
                string.Compare(variableItems[v].ToString(), staticText, StringComparison.CurrentCultureIgnoreCase) <= 0)
            yield return variableItems[v++];
         yield return staticItem;
      }

      while (v < variableItems.Count)
         yield return variableItems[v++];
   }

   // Converts the snapshot's 1-based caret line/column into an absolute buffer offset, using the same
   // line/column -> offset convention MemberCompletionController relies on. Returns -1 (drop nothing)
   // when no buffer is reachable.
   private static int ComputeCaretOffset(MemberCompletionContextSnapshot snapshot)
   {
      if (snapshot.Tree is null || snapshot.CaretLine <= 0 || snapshot.CaretColumn <= 0)
         return -1;

      var input = snapshot.Tree.Start?.InputStream;
      if (input is null)
         return -1;

      var buffer = input.GetText(Interval.Of(0, input.Size - 1));
      var line = 1;
      var offset = 0;
      while (line < snapshot.CaretLine && offset < buffer.Length)
      {
         var newline = buffer.IndexOf('\n', offset);
         if (newline < 0)
            break;
         offset = newline + 1;
         line++;
      }

      var target = offset + (snapshot.CaretColumn - 1);
      return Math.Min(Math.Max(target, 0), buffer.Length);
   }

   /// <inheritdoc/>
   IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
