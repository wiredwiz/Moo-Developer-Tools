#region BSD 3-Clause License
// <copyright file="FlowValueResolver.cs" company="Edgerunner.org">
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
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// The flow-resolved state of a variable's value at a caret position.
/// </summary>
public enum FlowValueKind
{
   /// <summary>The variable was definitely reassigned to a resolvable ground chain (see <see cref="FlowValue.Chain"/>).</summary>
   Reassigned,

   /// <summary>
   /// The variable is unmodified or ambiguous and should fall back to its keyword default
   /// (<c>this</c>/<c>player</c>/<c>caller</c>). Never produced for a non-keyword local.
   /// </summary>
   UseDefault,

   /// <summary>The variable's value cannot be determined (no resolvable source).</summary>
   Unknown
}

/// <summary>
/// The flow-resolved value of a variable at a caret position.
/// </summary>
/// <param name="Kind">The resolution outcome.</param>
/// <param name="Chain">
/// The fully reduced GROUND chain (base in <c>{ ObjectLiteral, CoreName, This, Player, Caller }</c>,
/// never a <see cref="ChainBaseKind.Variable"/>), non-<c>null</c> only when
/// <paramref name="Kind"/> is <see cref="FlowValueKind.Reassigned"/>.
/// </param>
public readonly record struct FlowValue(FlowValueKind Kind, ChainDescriptor? Chain);

/// <summary>
/// Resolves the <em>current value</em> of a variable at the caret by interpreting the verb's statements
/// from verb-start to the caret, honoring sequential reassignment, branch conservatism and loop
/// conservatism. Pure, synchronous, stateless: no world or provider access.
/// </summary>
/// <remarks>
/// <para>
/// For a queried name, every assignment to it that precedes the caret is folded, in document order, into
/// a three-state machine (<c>Unmodified -&gt; Reassigned -&gt; Ambiguous</c>):
/// <list type="bullet">
/// <item>A <em>Definite</em> assignment (in a block that encloses the caret, before the caret) overrides
/// any prior state.</item>
/// <item>A <em>Conditional</em> assignment (in a branch the caret is not in, or anywhere in an enclosing
/// loop body other than a sole dominating pre-caret assignment) poisons the state to <em>Ambiguous</em>.</item>
/// </list>
/// </para>
/// <para>
/// A definite assignment's right-hand side is reduced as of <em>that assignment's</em> offset (not the
/// caret's), recursively, into a ground chain whose base is <c>#N</c>/<c>$name</c>/keyword-default.
/// All assignment forms (plain <c>=</c>, the compound family, and scatter <c>{...}=</c>) bind their
/// target(s) and so participate in poisoning; only plain <c>=</c> and qualifying scatter elements carry
/// a resolvable value.
/// </para>
/// <para>
/// Guards mirror <see cref="ChainExpressionEvaluator"/>: a cycle set of names currently being reduced and
/// a depth budget bound the recursion. Unexpected exceptions route to the optional diagnostic delegate;
/// expected non-resolution is silent.
/// </para>
/// </remarks>
public static class FlowValueResolver
{
   /// <summary>The maximum number of reduction hops (variable / list-literal indirections) per request.</summary>
   public const int MaxResolutionSteps = ChainExpressionEvaluator.MaxResolutionSteps;

   // Statement-context types whose direct StatementListContext children are conditional branch bodies.
   private static readonly HashSet<string> BranchStatementTypes = new(StringComparer.Ordinal)
   {
      "IfStatementContext",
      "ElseifStatementContext",
      "ElseStatementContext",
      "ForStatementContext",
      "WhileStatementContext",
      "TryExceptStatementContext",
      "ExceptStatementContext",
      "TryFinallyStatementContext",
      "FinallyStatementContext",
   };

   // The control statements that introduce a loop body.
   private static readonly HashSet<string> LoopStatementTypes = new(StringComparer.Ordinal)
   {
      "ForStatementContext",
      "WhileStatementContext",
   };

   // Compound assignment context types (bind the target with an Unknown value).
   private static readonly HashSet<string> CompoundAssignmentTypes = new(StringComparer.Ordinal)
   {
      "AddAssignmentExpressionContext",
      "SubtractAssignmentExpressionContext",
      "MultiplyAssignmentExpressionContext",
      "DivideAssignmentExpressionContext",
      "ModulusAssignmentExpressionContext",
      "PowerAssignmentExpressionContext",
      "LeftShiftAssignmentExpressionContext",
      "RightShiftAssignmentExpressionContext",
      "XorAssignmentExpressionContext",
      "ComplimentAssignmentExpressionContext",
   };

   /// <summary>
   /// Resolves the current value of <paramref name="name"/> at <paramref name="caretOffset"/>.
   /// </summary>
   /// <param name="name">The variable name (<c>this</c>/<c>player</c>/<c>caller</c> or a local identifier).</param>
   /// <param name="tree">The current parse tree root.</param>
   /// <param name="caretOffset">The absolute caret offset into the buffer.</param>
   /// <param name="diagnostic">Optional sink for unexpected (logged) failures; expected non-resolution is silent.</param>
   /// <returns>The flow value at the caret.</returns>
   public static FlowValue ResolveCurrentValue(
      string name, ParserRuleContext? tree, int caretOffset, Action<string, Exception?>? diagnostic = null)
   {
      var isKeyword = IsKeyword(name);
      var ambiguousFallback = isKeyword
         ? new FlowValue(FlowValueKind.UseDefault, null)
         : new FlowValue(FlowValueKind.Unknown, null);

      if (string.IsNullOrEmpty(name) || tree is null)
         return ambiguousFallback;

      try
      {
         var state = ReduceState(name, tree, caretOffset, new HashSet<string>(StringComparer.Ordinal),
                                 new Budget(MaxResolutionSteps), diagnostic);
         return state ?? ambiguousFallback;
      }
      catch (Exception ex)
      {
         diagnostic?.Invoke($"Flow value resolution failed for '{name}'.", ex);
         return ambiguousFallback;
      }
   }

   // Core: fold all assignments to `name` preceding `caretOffset` into the three-state machine, then
   // reduce the winning Definite RHS to a ground chain. Returns null to mean "ambiguous/unmodified"
   // (the caller applies the keyword/local fallback). Returns Unknown/Reassigned for a definite result.
   private static FlowValue? ReduceState(
      string name,
      ParserRuleContext tree,
      int caretOffset,
      HashSet<string> reducing,
      Budget budget,
      Action<string, Exception?>? diagnostic)
   {
      // The set of branch-body StatementListContexts that enclose the caret (identity set).
      var caretBranchBodies = EnclosingBranchBodies(tree, caretOffset);

      // Collect every assignment binding `name`, in document order.
      var assignments = new List<Assignment>();
      CollectAssignments(tree, name, assignments);
      assignments.Sort((a, b) => a.Offset.CompareTo(b.Offset));

      // Determine whether the caret sits inside a loop body, and which loop bodies those are.
      var caretLoopBodies = EnclosingLoopBodies(tree, caretOffset);

      // Three-state fold.
      // null winner + notAmbiguous == Unmodified; winner set == Reassigned-with-RHS; ambiguous == poisoned.
      var ambiguous = false;
      Assignment? winner = null;

      foreach (var assignment in assignments)
      {
         var classification = Classify(assignment, caretOffset, caretBranchBodies, caretLoopBodies);
         switch (classification)
         {
            case Class.Definite:
               winner = assignment;
               ambiguous = false;
               break;
            case Class.Conditional:
               ambiguous = true;
               break;
            case Class.Irrelevant:
               break;
         }
      }

      // A poison that survives to the end (no later Definite cleared it) wins, even over a prior Definite:
      // e.g. a loop back-edge assignment after the caret re-poisons a dominating pre-caret assignment.
      if (ambiguous || winner is null)
         // Unmodified or ambiguous -> caller's fallback (UseDefault keyword / Unknown local).
         return null;

      // Reduce the winner's RHS as of its own offset.
      return ReduceAssignmentValue(winner.Value, reducing, budget, diagnostic);
   }

   // Classifies one assignment relative to the caret.
   private static Class Classify(
      Assignment assignment,
      int caretOffset,
      HashSet<ParserRuleContext> caretBranchBodies,
      HashSet<ParserRuleContext> caretLoopBodies)
   {
      // The assignment must be in a block that encloses the caret: every branch-body ancestor of the
      // assignment must also enclose the caret. If any does not, it is in a branch the caret is not in.
      foreach (var body in assignment.BranchBodies)
         if (!caretBranchBodies.Contains(body))
            return Class.Conditional;

      // Loop back-edge rule: if the caret sits inside a loop body, then any assignment also inside that
      // same loop body is conditional UNLESS it is the sole dominating pre-caret assignment. We approximate
      // the "sole dominating" exception below at the fold level by counting; here, treat an in-loop
      // assignment that is textually after the caret as conditional (the back-edge ran it), and an
      // in-loop assignment before the caret as definite (the dominating-assignment fold decides solitude).
      if (assignment.Offset >= caretOffset)
      {
         // After the caret: only conditional when it shares an enclosing loop body with the caret (back-edge).
         foreach (var body in assignment.LoopBodies)
            if (caretLoopBodies.Contains(body))
               return Class.Conditional;
         // After the caret and not on a back-edge: irrelevant (runs only after the caret).
         return Class.Irrelevant;
      }

      return Class.Definite;
   }

   // Reduces an assignment's bound value for `name` to a flow value (the RHS chain grounded, as of the
   // assignment's offset). Returns Reassigned(ground) or Unknown; never UseDefault (that is the caller's
   // keyword fallback for ambiguous/unmodified, which a definite assignment is not).
   private static FlowValue ReduceAssignmentValue(
      Assignment assignment, HashSet<string> reducing, Budget budget, Action<string, Exception?>? diagnostic)
   {
      switch (assignment.Kind)
      {
         case AssignmentKind.Compound:
            // Arithmetic/bitwise result: bound but Unknown.
            return new FlowValue(FlowValueKind.Unknown, null);

         case AssignmentKind.Plain:
            return ReduceExpression(assignment.Rhs, assignment.Offset, reducing, budget, diagnostic);

         case AssignmentKind.Scatter:
            return ReduceScatterElement(assignment, reducing, budget, diagnostic);

         default:
            return new FlowValue(FlowValueKind.Unknown, null);
      }
   }

   // Reduces an arbitrary expression subtree to a ground chain as of `offset`.
   private static FlowValue ReduceExpression(
      ParserRuleContext? expression, int offset, HashSet<string> reducing, Budget budget,
      Action<string, Exception?>? diagnostic)
   {
      var descriptor = ChainExtractor.DescribeExpression(expression);
      if (descriptor is null)
         // Non-chain RHS (call, arithmetic, literal list/string, chained assignment, …): determined Unknown.
         return new FlowValue(FlowValueKind.Unknown, null);

      return GroundChain(descriptor, offset, reducing, budget, diagnostic);
   }

   // Reduces a (possibly variable-based) chain to a ground chain as of `offset`. The base is grounded
   // recursively; the descriptor's steps are appended after the ground base's own steps.
   private static FlowValue GroundChain(
      ChainDescriptor descriptor, int offset, HashSet<string> reducing, Budget budget,
      Action<string, Exception?>? diagnostic)
   {
      switch (descriptor.Base.Kind)
      {
         case ChainBaseKind.ObjectLiteral:
         case ChainBaseKind.CoreName:
            // Already a ground base with no flow indirection: the chain is a ground chain as-is.
            return new FlowValue(FlowValueKind.Reassigned, descriptor);

         case ChainBaseKind.This:
         case ChainBaseKind.Player:
         case ChainBaseKind.Caller:
         case ChainBaseKind.Variable:
            // A keyword (this/player/caller) or local base in a reduced RHS is NOT yet ground: its value
            // must be resolved AS OF this assignment's offset, recursively. A definite reassignment grounds
            // to its own chain; an unmodified/ambiguous keyword falls back to its default keyword base.
            if (!budget.TryConsume())
               return new FlowValue(FlowValueKind.Unknown, null);
            var name = descriptor.Base.Kind == ChainBaseKind.Variable
               ? descriptor.Base.Text
               : KeywordName(descriptor.Base.Kind);
            if (!reducing.Add(name))
               // Cycle: re-entering a name currently being reduced (e.g. player = player).
               return new FlowValue(FlowValueKind.Unknown, null);
            try
            {
               // Recurse: resolve the name's current value AS OF the assignment's offset.
               return ReduceVariableAsOf(name, descriptor, offset, reducing, budget, diagnostic);
            }
            finally
            {
               reducing.Remove(name);
            }

         default:
            return new FlowValue(FlowValueKind.Unknown, null);
      }
   }

   // Resolves a variable base `varName` as of `offset`, then appends the outer descriptor's steps onto
   // the variable's ground chain. The tree is reached via the descriptor's owning context.
   private static FlowValue ReduceVariableAsOf(
      string varName, ChainDescriptor outerDescriptor, int offset, HashSet<string> reducing, Budget budget,
      Action<string, Exception?>? diagnostic)
   {
      var tree = _currentTree;
      if (tree is null)
         return new FlowValue(FlowValueKind.Unknown, null);

      var isKeyword = IsKeyword(varName);

      var inner = ReduceState(varName, tree, offset, reducing, budget, diagnostic);
      if (inner is null)
      {
         // Unmodified/ambiguous: keyword -> default keyword base + outer steps; local -> Unknown.
         if (!isKeyword)
            return new FlowValue(FlowValueKind.Unknown, null);

         var keywordBase = KeywordBase(varName);
         return new FlowValue(
            FlowValueKind.Reassigned,
            new ChainDescriptor(keywordBase, new List<string>(outerDescriptor.Steps),
                                MemberContextKind.Property, string.Empty));
      }

      var innerValue = inner.Value;
      if (innerValue.Kind != FlowValueKind.Reassigned || innerValue.Chain is null)
         return new FlowValue(FlowValueKind.Unknown, null);

      // Merge: ground base's steps ++ outer descriptor's steps.
      var mergedSteps = new List<string>(innerValue.Chain.Steps);
      mergedSteps.AddRange(outerDescriptor.Steps);
      return new FlowValue(
         FlowValueKind.Reassigned,
         new ChainDescriptor(innerValue.Chain.Base, mergedSteps, MemberContextKind.Property, string.Empty));
   }

   // Reduces a scatter target's positional element to a ground chain (Unknown for any non-qualifying shape).
   private static FlowValue ReduceScatterElement(
      Assignment assignment, HashSet<string> reducing, Budget budget, Action<string, Exception?>? diagnostic)
   {
      // The target must be plain (no ?/@) and at a fixed position (no ?/@ targets before it).
      if (!assignment.ScatterIsPlainFixedPosition)
         return new FlowValue(FlowValueKind.Unknown, null);

      var list = ResolveListLiteral(assignment.Rhs, assignment.Offset, reducing, budget, diagnostic);
      if (list is null)
         return new FlowValue(FlowValueKind.Unknown, null);

      var element = ListElementAt(list, assignment.ScatterPosition);
      if (element is null)
         return new FlowValue(FlowValueKind.Unknown, null);

      // Reduce the element as a chain expression, as of the scatter's offset.
      return ReduceExpression(element, assignment.Offset, reducing, budget, diagnostic);
   }

   // Reduces an expression to a list-literal node (directly, or through a variable whose definite value is
   // a list literal), as of `offset`. Returns null when no determinate list literal is reachable, or when
   // the list contains a splat (`@`) at/before any position (which makes positions indeterminate).
   private static ParserRuleContext? ResolveListLiteral(
      ParserRuleContext? expression, int offset, HashSet<string> reducing, Budget budget,
      Action<string, Exception?>? diagnostic)
   {
      var listContext = FindListContext(expression);
      if (listContext is not null)
         return listContext;

      // Indirection: a bare identifier whose definite value (as of `offset`) is a list literal.
      if (expression is not null && expression.GetType().Name == "IdentifierExpressionContext")
      {
         var name = expression.GetText();
         if (IsKeyword(name))
            return null;
         if (!budget.TryConsume())
            return null;
         if (!reducing.Add(name))
            return null;
         try
         {
            var assignment = WinningPlainAssignmentAsOf(name, offset);
            if (assignment is null)
               return null;
            return ResolveListLiteral(assignment.Value.Rhs, assignment.Value.Offset, reducing, budget, diagnostic);
         }
         finally
         {
            reducing.Remove(name);
         }
      }

      return null;
   }

   // Returns the ListContext for a list-literal expression (LiteralExpression -> Literal -> List), or null.
   private static ParserRuleContext? FindListContext(ParserRuleContext? expression)
   {
      if (expression is null || expression.GetType().Name != "LiteralExpressionContext")
         return null;

      // LiteralExpressionContext -> LiteralContext -> ListContext
      if (expression.ChildCount < 1 || expression.GetChild(0) is not ParserRuleContext literal ||
          literal.GetType().Name != "LiteralContext")
         return null;
      if (literal.ChildCount < 1 || literal.GetChild(0) is not ParserRuleContext list ||
          list.GetType().Name != "ListContext")
         return null;

      return list;
   }

   // Returns the element expression at `position` (0-based) in a ListContext, or null when out of range
   // or when a splat (`@`) element appears at/before that position (which makes positions indeterminate).
   private static ParserRuleContext? ListElementAt(ParserRuleContext list, int position)
   {
      var index = 0;
      for (var i = 0; i < list.ChildCount; i++)
      {
         var child = list.GetChild(i);
         if (child is ITerminalNode)
            // '{', ',', '}' separators.
            continue;
         if (child is not ParserRuleContext element)
            continue;

         // A splat element makes this and every later position indeterminate.
         if (element.GetType().Name == "SplicerExpressionContext")
            return null;

         if (index == position)
            return element;
         index++;
      }

      return null;
   }

   // Finds the single winning DEFINITE plain assignment to `name` as of `offset` (treating `offset` as the
   // caret), used by list-literal indirection. Returns null when not a single definite plain assignment.
   private static Assignment? WinningPlainAssignmentAsOf(string name, int offset)
   {
      var tree = _currentTree;
      if (tree is null)
         return null;

      var caretBranchBodies = EnclosingBranchBodies(tree, offset);
      var caretLoopBodies = EnclosingLoopBodies(tree, offset);

      var assignments = new List<Assignment>();
      CollectAssignments(tree, name, assignments);
      assignments.Sort((a, b) => a.Offset.CompareTo(b.Offset));

      var ambiguous = false;
      Assignment? winner = null;
      foreach (var assignment in assignments)
      {
         switch (Classify(assignment, offset, caretBranchBodies, caretLoopBodies))
         {
            case Class.Definite:
               winner = assignment;
               ambiguous = false;
               break;
            case Class.Conditional:
               ambiguous = true;
               break;
         }
      }

      if (ambiguous || winner is null)
         return null;

      return winner.Value.Kind == AssignmentKind.Plain ? winner : null;
   }

   // ---- Assignment collection ----

   [ThreadStatic]
   private static ParserRuleContext? _currentTree;

   private static void CollectAssignments(ParserRuleContext tree, string name, List<Assignment> into)
   {
      _currentTree = tree;
      Walk(tree, node =>
      {
         var typeName = node.GetType().Name;

         if (typeName == "AssignmentExpressionContext")
         {
            // child 0 = LHS, child 1 = '=', child 2 = RHS.
            if (node.ChildCount >= 3 &&
                node.GetChild(0) is ParserRuleContext lhs &&
                lhs.GetType().Name == "IdentifierExpressionContext" &&
                string.Equals(lhs.GetText(), name, StringComparison.Ordinal))
            {
               into.Add(new Assignment(
                  AssignmentKind.Plain, node.Start?.StartIndex ?? -1,
                  node.GetChild(2) as ParserRuleContext, node,
                  false, 0));
            }

            return;
         }

         if (CompoundAssignmentTypes.Contains(typeName))
         {
            if (node.ChildCount >= 1 &&
                node.GetChild(0) is ParserRuleContext clhs &&
                clhs.GetType().Name == "IdentifierExpressionContext" &&
                string.Equals(clhs.GetText(), name, StringComparison.Ordinal))
            {
               into.Add(new Assignment(
                  AssignmentKind.Compound, node.Start?.StartIndex ?? -1, null, node, false, 0));
            }

            return;
         }

         if (typeName == "ScatteringAssignmentExpressionContext")
         {
            CollectScatterTargets(node, name, into);
         }
      });
   }

   // Records every binding of `name` in a scatter assignment, with its plain-fixed-position metadata.
   private static void CollectScatterTargets(ParserRuleContext scatter, string name, List<Assignment> into)
   {
      // children: '{', ScatteringTargetContext, '}', '=', RHS.
      ParserRuleContext? targetList = null;
      ParserRuleContext? rhs = null;
      var sawEquals = false;
      for (var i = 0; i < scatter.ChildCount; i++)
      {
         var child = scatter.GetChild(i);
         if (child is ParserRuleContext ctx)
         {
            if (ctx.GetType().Name == "ScatteringTargetContext")
               targetList = ctx;
            else if (sawEquals)
               rhs = ctx;
         }
         else if (child is ITerminalNode term && term.GetText() == "=")
         {
            sawEquals = true;
         }
      }

      if (targetList is null)
         return;

      var offset = scatter.Start?.StartIndex ?? -1;

      // Walk the target items in order, tracking whether a position shift (?, @) has occurred.
      var position = 0;
      var positionShifted = false;
      for (var i = 0; i < targetList.ChildCount; i++)
      {
         if (targetList.GetChild(i) is not ParserRuleContext item ||
             item.GetType().Name != "ScatteringTargetItemContext")
            continue;

         var (itemName, isOptional, isRest) = DecodeScatterItem(item);
         var isPlain = !isOptional && !isRest;

         if (string.Equals(itemName, name, StringComparison.Ordinal))
         {
            var plainFixed = isPlain && !positionShifted;
            into.Add(new Assignment(
               AssignmentKind.Scatter, offset, rhs, scatter, plainFixed, position));
         }

         // Optional and rest targets shift every subsequent plain target's position.
         if (isOptional || isRest)
            positionShifted = true;

         if (isPlain)
            position++;
      }
   }

   // Decodes a ScatteringTargetItemContext into (name, isOptional, isRest).
   private static (string Name, bool IsOptional, bool IsRest) DecodeScatterItem(ParserRuleContext item)
   {
      // plain:    TERMINAL name
      // optional: TERMINAL '?' TERMINAL name (optionally '=' default...)
      // rest:     TERMINAL '@' TERMINAL name
      string? leadText = null;
      string? identText = null;
      for (var i = 0; i < item.ChildCount; i++)
      {
         if (item.GetChild(i) is ITerminalNode term)
         {
            var text = term.GetText();
            if (text == "?" || text == "@")
               leadText = text;
            else if (identText is null && text != "=")
               identText = text;
         }
      }

      var isOptional = leadText == "?";
      var isRest = leadText == "@";
      return (identText ?? string.Empty, isOptional, isRest);
   }

   // ---- Block / branch / loop topology ----

   // The set (by identity) of branch-body StatementListContexts that enclose `offset`.
   private static HashSet<ParserRuleContext> EnclosingBranchBodies(ParserRuleContext tree, int offset)
   {
      var result = new HashSet<ParserRuleContext>();
      Walk(tree, node =>
      {
         if (node.GetType().Name != "StatementListContext")
            return;
         if (!IsBranchBody(node))
            return;
         if (Encloses(node, offset))
            result.Add(node);
      });
      return result;
   }

   // The set (by identity) of loop-body StatementListContexts that enclose `offset`.
   private static HashSet<ParserRuleContext> EnclosingLoopBodies(ParserRuleContext tree, int offset)
   {
      var result = new HashSet<ParserRuleContext>();
      Walk(tree, node =>
      {
         if (node.GetType().Name != "StatementListContext")
            return;
         if (node.Parent is not ParserRuleContext parent || !LoopStatementTypes.Contains(parent.GetType().Name))
            return;
         if (Encloses(node, offset))
            result.Add(node);
      });
      return result;
   }

   // A StatementListContext is a branch body when its parent is a branch/loop/try statement.
   private static bool IsBranchBody(ParserRuleContext statementList)
   {
      return statementList.Parent is ParserRuleContext parent &&
             BranchStatementTypes.Contains(parent.GetType().Name);
   }

   // Whether a node's source span contains `offset` (inclusive of the span up to and including stop).
   private static bool Encloses(ParserRuleContext node, int offset)
   {
      var start = node.Start?.StartIndex ?? -1;
      var stop = node.Stop?.StopIndex ?? -1;
      if (start < 0)
         return false;
      // The caret may sit one past the body's last character (e.g. an in-branch caret at the body tail).
      return offset >= start && offset <= stop + 1;
   }

   // The branch-body StatementListContext ancestors of a node (those whose parent is a branch statement).
   private static List<ParserRuleContext> BranchBodyAncestors(ParserRuleContext node)
   {
      var result = new List<ParserRuleContext>();
      var current = node.Parent;
      while (current is not null)
      {
         if (current is ParserRuleContext ctx && ctx.GetType().Name == "StatementListContext" && IsBranchBody(ctx))
            result.Add(ctx);
         current = current.Parent;
      }

      return result;
   }

   // The loop-body StatementListContext ancestors of a node.
   private static List<ParserRuleContext> LoopBodyAncestors(ParserRuleContext node)
   {
      var result = new List<ParserRuleContext>();
      var current = node.Parent;
      while (current is not null)
      {
         if (current is ParserRuleContext ctx && ctx.GetType().Name == "StatementListContext" &&
             ctx.Parent is ParserRuleContext p && LoopStatementTypes.Contains(p.GetType().Name))
            result.Add(ctx);
         current = current.Parent;
      }

      return result;
   }

   // ---- Misc helpers ----

   private static bool IsKeyword(string name) =>
      name is "this" or "player" or "caller";

   private static ChainBase KeywordBase(string name) => name switch
   {
      "this" => new ChainBase(ChainBaseKind.This, string.Empty),
      "player" => new ChainBase(ChainBaseKind.Player, string.Empty),
      "caller" => new ChainBase(ChainBaseKind.Caller, string.Empty),
      _ => new ChainBase(ChainBaseKind.Variable, name),
   };

   private static string KeywordName(ChainBaseKind kind) => kind switch
   {
      ChainBaseKind.This => "this",
      ChainBaseKind.Player => "player",
      ChainBaseKind.Caller => "caller",
      _ => string.Empty,
   };

   private static void Walk(ParserRuleContext node, Action<ParserRuleContext> visit)
   {
      visit(node);
      for (var i = 0; i < node.ChildCount; i++)
         if (node.GetChild(i) is ParserRuleContext child)
            Walk(child, visit);
   }

   private enum Class { Definite, Conditional, Irrelevant }

   private enum AssignmentKind { Plain, Compound, Scatter }

   private readonly struct Assignment
   {
      public Assignment(
         AssignmentKind kind, int offset, ParserRuleContext? rhs, ParserRuleContext node,
         bool scatterIsPlainFixedPosition, int scatterPosition)
      {
         Kind = kind;
         Offset = offset;
         Rhs = rhs;
         Node = node;
         ScatterIsPlainFixedPosition = scatterIsPlainFixedPosition;
         ScatterPosition = scatterPosition;
         BranchBodies = BranchBodyAncestors(node);
         LoopBodies = LoopBodyAncestors(node);
      }

      public AssignmentKind Kind { get; }
      public int Offset { get; }
      public ParserRuleContext? Rhs { get; }
      public ParserRuleContext Node { get; }
      public bool ScatterIsPlainFixedPosition { get; }
      public int ScatterPosition { get; }
      public List<ParserRuleContext> BranchBodies { get; }
      public List<ParserRuleContext> LoopBodies { get; }
   }

   private sealed class Budget
   {
      private int _remaining;
      public Budget(int budget) => _remaining = budget;

      public bool TryConsume()
      {
         if (_remaining <= 0)
            return false;
         _remaining--;
         return true;
      }
   }
}
