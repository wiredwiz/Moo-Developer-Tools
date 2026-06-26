#region BSD 3-Clause License
// <copyright file="VariableReferenceCollector.cs" company="Edgerunner.org">
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
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// Harvests, from a verb's parse tree, every identifier that appears in a <em>variable position</em>
/// (assignment target, scatter target, <c>for</c>-loop variable, <c>try</c>/<c>except</c> error
/// variable, or a bare read reference), so the names can be offered as non-member completions.
/// </summary>
/// <remarks>
/// <para>
/// This is a flat lexical harvest: it does <em>not</em> reason about flow, branches, or reachability
/// (that is <see cref="FlowValueResolver"/>'s job). Every identifier sitting in a variable position
/// counts, assigned or not.
/// </para>
/// <para>
/// A "variable position" is exactly an <c>IdentifierExpressionContext</c> (which wraps the operand
/// identifier of a read, an assignment target, a chain base, a call argument, a loop iterable, ...),
/// plus the bare identifier terminals that name a <c>for</c>-loop variable, an <c>except</c> error
/// variable, and a scatter target. Member names (the identifier to the right of <c>.</c> or <c>:</c>)
/// and function-call names are <em>not</em> <c>IdentifierExpressionContext</c> nodes — they are bare
/// terminals under <c>Property_access</c>/<c>Verb_access</c>/call contexts — so they are naturally
/// excluded.
/// </para>
/// <para>
/// The single occurrence the caret sits in (or immediately to the right of) is dropped: the name the
/// user is actively typing must never be offered as a completion of itself. A name therefore survives
/// only if it occurs <em>elsewhere</em> in the verb as a variable. Results are de-duplicated
/// case-insensitively, preserving the as-written casing of the first surviving occurrence.
/// </para>
/// </remarks>
public static class VariableReferenceCollector
{
   // Identifier-shaped text: used to distinguish variable-name terminals from keywords/punctuation
   // when scanning the bare-terminal positions (for-loop, except, scatter targets).
   private static readonly Regex IdentifierShape =
      new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

   // Structural keywords that appear as bare terminals alongside the variable terminals we want.
   private static readonly HashSet<string> ForClauseKeywords = new(StringComparer.Ordinal) { "for", "in" };

   private static readonly HashSet<string> ExceptClauseKeywords = new(StringComparer.Ordinal) { "except" };

   /// <summary>
   /// Collects every variable-position identifier name in <paramref name="tree"/>, dropping the single
   /// occurrence under (or immediately right of) the caret, de-duplicated case-insensitively.
   /// </summary>
   /// <param name="tree">The verb's parse tree root, or <c>null</c>.</param>
   /// <param name="caretOffset">
   /// The absolute buffer offset of the caret. The occurrence the caret sits in or immediately right
   /// of (the in-progress identifier) is dropped; pass a negative value to drop nothing.
   /// </param>
   /// <returns>
   /// The distinct variable names, in first-surviving-occurrence document order (the caller sorts).
   /// Never <c>null</c>; empty when <paramref name="tree"/> is <c>null</c>.
   /// </returns>
   public static IReadOnlyCollection<string> CollectVariableNames(ParserRuleContext? tree, int caretOffset)
   {
      if (tree is null)
         return Array.Empty<string>();

      var occurrences = new List<Occurrence>();
      Walk(tree, node => CollectFromNode(node, occurrences));

      // Identify the single occurrence the caret sits in or immediately to the right of. When several
      // qualify (adjacent spans at a boundary), prefer the one starting latest — the token the caret
      // is at the tail of, i.e. the in-progress identifier.
      var dropIndex = -1;
      var dropStart = int.MinValue;
      if (caretOffset >= 0)
         for (var i = 0; i < occurrences.Count; i++)
         {
            var occurrence = occurrences[i];
            if (caretOffset >= occurrence.Start && caretOffset <= occurrence.Stop + 1 && occurrence.Start > dropStart)
            {
               dropStart = occurrence.Start;
               dropIndex = i;
            }
         }

      var result = new List<string>();
      var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      for (var i = 0; i < occurrences.Count; i++)
      {
         if (i == dropIndex)
            continue;
         var name = occurrences[i].Name;
         if (seen.Add(name))
            result.Add(name);
      }

      return result;
   }

   private static void CollectFromNode(ParserRuleContext node, List<Occurrence> into)
   {
      switch (node.GetType().Name)
      {
         case "IdentifierExpressionContext":
            // A bare operand identifier: read reference, assignment target, chain base, call argument,
            // loop iterable, etc. Member names and call names are not IdentifierExpressionContext.
            AddNode(node, into);
            break;

         case "ForClauseContext":
            // for VAR in (expr) — VAR is a bare identifier terminal directly under the clause; the
            // iterable is wrapped in its own expression context, so only VAR is a direct terminal.
            AddDirectIdentifierTerminals(node, ForClauseKeywords, into);
            break;

         case "ExceptClauseContext":
            // except VAR (codes) — VAR is a bare identifier terminal directly under the clause; the
            // exception codes are wrapped in their own context. VAR is optional.
            AddDirectIdentifierTerminals(node, ExceptClauseKeywords, into);
            break;

         case "ScatteringTargetItemContext":
            // {VAR, ?VAR, @VAR} — the target name is the identifier terminal (after any ?/@ lead).
            AddFirstIdentifierTerminal(node, into);
            break;
      }
   }

   // Records an occurrence spanning a whole context node (used for IdentifierExpressionContext).
   private static void AddNode(ParserRuleContext node, List<Occurrence> into)
   {
      var name = node.GetText();
      if (string.IsNullOrEmpty(name) || node.Start is null)
         return;
      var start = node.Start.StartIndex;
      var stop = node.Stop?.StopIndex ?? start;
      into.Add(new Occurrence(name, start, stop));
   }

   // Records every direct identifier-shaped terminal child whose text is not a structural keyword.
   private static void AddDirectIdentifierTerminals(ParserRuleContext node, HashSet<string> keywords, List<Occurrence> into)
   {
      for (var i = 0; i < node.ChildCount; i++)
         if (node.GetChild(i) is ITerminalNode terminal)
         {
            var text = terminal.GetText();
            if (IdentifierShape.IsMatch(text) && !keywords.Contains(text))
               into.Add(new Occurrence(text, terminal.Symbol.StartIndex, terminal.Symbol.StopIndex));
         }
   }

   // Records the first direct identifier-shaped terminal child (the scatter target name).
   private static void AddFirstIdentifierTerminal(ParserRuleContext node, List<Occurrence> into)
   {
      for (var i = 0; i < node.ChildCount; i++)
         if (node.GetChild(i) is ITerminalNode terminal)
         {
            var text = terminal.GetText();
            if (IdentifierShape.IsMatch(text))
            {
               into.Add(new Occurrence(text, terminal.Symbol.StartIndex, terminal.Symbol.StopIndex));
               return;
            }
         }
   }

   private static void Walk(ParserRuleContext node, Action<ParserRuleContext> visit)
   {
      visit(node);
      for (var i = 0; i < node.ChildCount; i++)
         if (node.GetChild(i) is ParserRuleContext child)
            Walk(child, visit);
   }

   private readonly struct Occurrence
   {
      public Occurrence(string name, int start, int stop)
      {
         Name = name;
         Start = start;
         Stop = stop;
      }

      public string Name { get; }
      public int Start { get; }
      public int Stop { get; }
   }
}
