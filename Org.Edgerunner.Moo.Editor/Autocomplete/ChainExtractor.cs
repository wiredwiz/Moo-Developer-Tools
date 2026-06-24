#region BSD 3-Clause License
// <copyright file="ChainExtractor.cs" company="Edgerunner.org">
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
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// Extracts the chained member-access expression at the caret into a <see cref="ChainDescriptor"/>.
/// </summary>
/// <remarks>
/// Two entry points exist. <see cref="Extract"/> is parse-tree-primary: it reads the already-typed
/// chain to the left of the trailing <c>:</c>/<c>.</c> directly from the parse tree (so chain length
/// and operator/call boundaries are decided by the grammar), falling back to a token-stream scan only
/// for the incomplete tail the tree cannot represent. <see cref="ExtractFromLinePrefix"/> is the
/// token-free fallback that subsumes the legacy single-identifier
/// <see cref="MemberCompletionContextDetector"/>: it yields length-1 chains from a line prefix alone.
/// Both return <c>null</c> outside member positions (so the controller and hover degrade to no source).
/// </remarks>
public static class ChainExtractor
{
   // <operand><separator><partial-member> anchored at the caret, matching the legacy detector's shape.
   private static readonly Regex MemberPattern =
      new(@"(\$?[A-Za-z_]\w*|#-?\d+)([:.])(\w*)$", RegexOptions.Compiled);

   // A core-reference fragment ($ or $partialname) anchored at the caret.
   private static readonly Regex CoreRefPattern = new(@"\$(\w*)$", RegexOptions.Compiled);

   // The trailing separator and partial fragment of a chain (the operand to its left comes from the tree).
   private static readonly Regex TrailingSeparatorPattern = new(@"([:.])(\w*)$", RegexOptions.Compiled);

   /// <summary>
   /// Extracts the chain descriptor at the caret using the parse tree as the primary source.
   /// </summary>
   /// <param name="buffer">The full editor buffer.</param>
   /// <param name="caretLine">The 1-based caret line.</param>
   /// <param name="caretColumn">The 1-based caret column (one past the last typed character).</param>
   /// <param name="tokens">The detailed token list for the buffer.</param>
   /// <param name="tree">The current parse tree root.</param>
   /// <param name="diagnostic">Optional sink for unexpected (logged) failures; expected unknowns pass silently.</param>
   /// <returns>The chain descriptor, or <c>null</c> when the caret is not in a member position.</returns>
   public static ChainDescriptor? Extract(
      string buffer,
      int caretLine,
      int caretColumn,
      IList<DetailedToken>? tokens,
      ParserRuleContext? tree,
      Action<string, Exception?>? diagnostic = null)
   {
      try
      {
         if (string.IsNullOrEmpty(buffer))
            return null;

         var caretOffset = CaretOffset(buffer, caretLine, caretColumn);
         if (caretOffset < 0)
            return null;

         var linePrefix = LinePrefix(buffer, caretOffset);

         // The completion trigger is a trailing ':'/'.' (with optional partial member text after it).
         var trailing = TrailingSeparatorPattern.Match(linePrefix);
         if (!trailing.Success)
            // No member separator: only a bare $fragment can still be a (length-1) member position.
            return ExtractFromLinePrefix(linePrefix);

         if (IsInsideString(linePrefix))
            return null;

         var memberKind = trailing.Groups[1].Value == ":" ? MemberContextKind.Verb : MemberContextKind.Property;
         var partial = trailing.Groups[2].Value;

         // Absolute offset of the trailing separator character.
         var separatorOffset = caretOffset - partial.Length - 1;

         // Tree-primary: find the access node carrying that separator and read the chain to its left.
         var descriptor = TryExtractFromTree(tree, separatorOffset, memberKind, partial);
         if (descriptor is not null)
            return descriptor;

         // Token fallback for the incomplete tail the tree could not represent.
         var fallback = TryExtractFromTokens(tokens, separatorOffset, memberKind, partial);
         if (fallback is not null)
            return fallback;

         // Last resort: the line-prefix single-atom path (keeps legacy single-identifier behavior).
         return ExtractFromLinePrefix(linePrefix);
      }
      catch (Exception ex)
      {
         diagnostic?.Invoke("Chain extraction failed.", ex);
         return null;
      }
   }

   /// <summary>
   /// Extracts a length-1 chain descriptor from a caret line prefix alone (no parse tree).
   /// Subsumes the legacy single-identifier <see cref="MemberCompletionContextDetector"/>.
   /// </summary>
   /// <param name="linePrefix">The text on the caret line, from column 0 up to the caret.</param>
   /// <returns>The chain descriptor, or <c>null</c> when the prefix is not a member position.</returns>
   public static ChainDescriptor? ExtractFromLinePrefix(string? linePrefix)
   {
      if (string.IsNullOrEmpty(linePrefix) || IsInsideString(linePrefix))
         return null;

      var member = MemberPattern.Match(linePrefix);
      if (member.Success)
      {
         var operand = member.Groups[1].Value;
         var memberKind = member.Groups[2].Value == ":" ? MemberContextKind.Verb : MemberContextKind.Property;
         var partial = member.Groups[3].Value;
         var chainBase = ClassifyOperand(operand);
         if (chainBase is null)
            return null;

         return new ChainDescriptor(chainBase.Value, Array.Empty<string>(), memberKind, partial);
      }

      var core = CoreRefPattern.Match(linePrefix);
      if (core.Success)
         // A bare $fragment completes properties of #0 (the core-reference context).
         return new ChainDescriptor(
            new ChainBase(ChainBaseKind.ObjectLiteral, "0"),
            Array.Empty<string>(),
            MemberContextKind.CoreReference,
            core.Groups[1].Value);

      return null;
   }

   // Classifies a single operand token (the legacy detector's operand) into a chain base.
   private static ChainBase? ClassifyOperand(string operand)
   {
      if (operand.StartsWith('#'))
         return new ChainBase(ChainBaseKind.ObjectLiteral, operand[1..]);
      if (operand.StartsWith('$'))
         return new ChainBase(ChainBaseKind.CoreName, operand[1..]);

      return operand switch
      {
         "this" => new ChainBase(ChainBaseKind.This, string.Empty),
         "player" => new ChainBase(ChainBaseKind.Player, string.Empty),
         "caller" => new ChainBase(ChainBaseKind.Caller, string.Empty),
         _ => new ChainBase(ChainBaseKind.Variable, operand),
      };
   }

   // Walks the tree to the DOT/COLON terminal at separatorOffset, then decomposes the expression to its left.
   private static ChainDescriptor? TryExtractFromTree(
      ParserRuleContext? tree, int separatorOffset, MemberContextKind memberKind, string partial)
   {
      if (tree is null)
         return null;

      var separator = FindTerminalAtOffset(tree, separatorOffset);
      // The separator terminal lives in a Property_access / Verb_access node whose parent
      // (a PropertyExpression / VerbCallExpression) holds the left expression as its first child.
      var accessNode = separator?.Parent as ParserRuleContext;
      if (accessNode is null)
         return null;

      var name = accessNode.GetType().Name;
      if (name is not ("Property_accessContext" or "Verb_accessContext"))
         return null;

      if (accessNode.Parent is not ParserRuleContext memberExpr || memberExpr.ChildCount == 0)
         return null;

      var leftExpression = memberExpr.GetChild(0) as ParserRuleContext;
      var chain = DecomposeChain(leftExpression);
      if (chain is null)
         return null;

      return new ChainDescriptor(chain.Value.Base, chain.Value.Steps, memberKind, partial);
   }

   // Token fallback: scan a contiguous "base (. ident)*" run ending just before the separator offset.
   private static ChainDescriptor? TryExtractFromTokens(
      IList<DetailedToken>? tokens, int separatorOffset, MemberContextKind memberKind, string partial)
   {
      if (tokens is null || tokens.Count == 0)
         return null;

      // Collect the significant tokens up to (and excluding) the separator, then walk left over a chain.
      var significant = new List<DetailedToken>();
      foreach (var token in tokens)
      {
         if (token.Channel != 0 || token.Text == "<EOF>" || string.IsNullOrWhiteSpace(token.Text))
            continue;
         if (token.StartIndex >= separatorOffset)
            break;
         significant.Add(token);
      }

      if (significant.Count == 0)
         return null;

      // Walk left: expect (IDENTIFIER (DOT IDENTIFIER)*) or a single atom, then the base atom.
      var steps = new List<string>();
      var i = significant.Count - 1;

      while (i >= 1 && significant[i].TypeNameUpperCase == "IDENTIFIER" &&
             significant[i - 1].Text == ".")
      {
         steps.Insert(0, significant[i].Text);
         i -= 2;
      }

      if (i < 0)
         return null;

      var baseToken = significant[i];
      // The base must sit at the head of the chain (no clinging '.' to its left would make it a step).
      var chainBase = ClassifyBaseToken(baseToken);
      if (chainBase is null)
         return null;

      // If there is a token immediately to the base's left that is a '.', this run is not a complete
      // chain base (it is itself a step of a larger expression we failed to read from the tree); bail.
      if (i >= 1 && significant[i - 1].Text == ".")
         return null;

      return new ChainDescriptor(chainBase.Value, steps, memberKind, partial);
   }

   private static ChainBase? ClassifyBaseToken(DetailedToken token)
   {
      switch (token.TypeNameUpperCase)
      {
         case "CORE_REFERENCE":
            return new ChainBase(ChainBaseKind.CoreName, token.Text.Length > 1 ? token.Text[1..] : string.Empty);
         case "OBJECT":
            return new ChainBase(ChainBaseKind.ObjectLiteral, token.Text.Length > 1 ? token.Text[1..] : string.Empty);
         case "IDENTIFIER":
            return token.Text switch
            {
               "this" => new ChainBase(ChainBaseKind.This, string.Empty),
               "player" => new ChainBase(ChainBaseKind.Player, string.Empty),
               "caller" => new ChainBase(ChainBaseKind.Caller, string.Empty),
               _ => new ChainBase(ChainBaseKind.Variable, token.Text),
            };
         default:
            return null;
      }
   }

   /// <summary>
   /// Describes a standalone expression subtree as a resolvable chain (base + ordered property steps),
   /// or returns <c>null</c> when the expression is not a resolvable chain (verb call, function call,
   /// arithmetic, list/string literal, index access, etc.). Used by <see cref="LocalVariableResolver"/>
   /// to convert an assignment's right-hand side into a chain for recursive resolution.
   /// </summary>
   /// <param name="expression">The expression subtree.</param>
   /// <returns>
   /// A descriptor carrying the chain base and steps (its <see cref="ChainDescriptor.MemberKind"/> and
   /// <see cref="ChainDescriptor.PartialFragment"/> are placeholders and unused), or <c>null</c>.
   /// </returns>
   internal static ChainDescriptor? DescribeExpression(ParserRuleContext? expression)
   {
      var chain = DecomposeChain(expression);
      return chain is null
         ? null
         : new ChainDescriptor(chain.Value.Base, chain.Value.Steps, MemberContextKind.Property, string.Empty);
   }

   // Recursively unwinds a (possibly nested) PropertyExpression chain into base + ordered steps.
   private static (ChainBase Base, List<string> Steps)? DecomposeChain(ParserRuleContext? expression)
   {
      if (expression is null)
         return null;

      var name = expression.GetType().Name;

      if (name == "PropertyExpressionContext")
      {
         // child 0 = inner expression, child 1 = Property_access(DOT, IDENTIFIER?)
         if (expression.ChildCount < 2)
            return null;
         var inner = DecomposeChain(expression.GetChild(0) as ParserRuleContext);
         if (inner is null)
            return null;
         if (expression.GetChild(1) is not ParserRuleContext access || access.GetType().Name != "Property_accessContext")
            return null;
         var prop = PropertyAccessIdentifier(access);
         if (prop is null)
            // A trailing '.' with no identifier yet is the active (dangling) step, not a completed step.
            return inner;
         inner.Value.Steps.Add(prop);
         return inner;
      }

      // Atom forms.
      switch (name)
      {
         case "CorePropertyExpressionContext":
            var coreText = expression.GetText();
            return (new ChainBase(ChainBaseKind.CoreName, coreText.Length > 1 ? coreText[1..] : string.Empty),
                    new List<string>());
         case "IdentifierExpressionContext":
            var id = expression.GetText();
            var baseKind = id switch
            {
               "this" => ChainBaseKind.This,
               "player" => ChainBaseKind.Player,
               "caller" => ChainBaseKind.Caller,
               _ => ChainBaseKind.Variable,
            };
            return (new ChainBase(baseKind, baseKind == ChainBaseKind.Variable ? id : string.Empty),
                    new List<string>());
         case "LiteralExpressionContext":
            var objectNumber = ObjectLiteralNumber(expression);
            return objectNumber is null
               ? null
               : (new ChainBase(ChainBaseKind.ObjectLiteral, objectNumber), new List<string>());
         default:
            return null;
      }
   }

   private static string? PropertyAccessIdentifier(ParserRuleContext propertyAccess)
   {
      for (var i = 0; i < propertyAccess.ChildCount; i++)
         if (propertyAccess.GetChild(i) is ITerminalNode terminal &&
             !string.IsNullOrEmpty(terminal.GetText()) && terminal.GetText() != ".")
            return terminal.GetText();
      return null;
   }

   // Extracts the "N" of an #N object literal from a LiteralExpression subtree.
   private static string? ObjectLiteralNumber(ParserRuleContext literalExpression)
   {
      var text = literalExpression.GetText();
      return text.StartsWith('#') && text.Length > 1 ? text[1..] : null;
   }

   // Finds the terminal node whose token starts at the given absolute offset.
   private static ITerminalNode? FindTerminalAtOffset(IParseTree node, int offset)
   {
      for (var i = 0; i < node.ChildCount; i++)
      {
         var child = node.GetChild(i);
         if (child is ITerminalNode terminal)
         {
            if (terminal.Symbol.StartIndex == offset)
               return terminal;
         }
         else
         {
            var found = FindTerminalAtOffset(child, offset);
            if (found is not null)
               return found;
         }
      }

      return null;
   }

   // Translates a 1-based (line, column) caret to an absolute offset into the buffer.
   private static int CaretOffset(string buffer, int caretLine, int caretColumn)
   {
      var line = 1;
      var offset = 0;
      while (line < caretLine && offset < buffer.Length)
      {
         var newline = buffer.IndexOf('\n', offset);
         if (newline < 0)
            return -1;
         offset = newline + 1;
         line++;
      }

      var target = offset + (caretColumn - 1);
      return target <= buffer.Length ? target : -1;
   }

   // The text of the caret line from its start up to the caret offset.
   private static string LinePrefix(string buffer, int caretOffset)
   {
      if (caretOffset <= 0)
         return string.Empty;

      var lineStart = buffer.LastIndexOf('\n', caretOffset - 1);
      lineStart = lineStart < 0 ? 0 : lineStart + 1;
      // A newline landing exactly at the caret means the caret is at the very start of a line.
      if (lineStart >= caretOffset)
         return string.Empty;
      return buffer.Substring(lineStart, caretOffset - lineStart);
   }

   // Whether the caret sits inside an open string literal on its line (MOO has no in-verb comments).
   private static bool IsInsideString(string linePrefix)
   {
      var inString = false;
      for (var i = 0; i < linePrefix.Length; i++)
      {
         var ch = linePrefix[i];
         if (inString && ch == '\\')
         {
            i++;
            continue;
         }

         if (ch == '"')
            inString = !inString;
      }

      return inString;
   }
}
