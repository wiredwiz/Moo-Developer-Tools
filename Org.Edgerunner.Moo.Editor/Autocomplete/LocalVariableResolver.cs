#region BSD 3-Clause License
// <copyright file="LocalVariableResolver.cs" company="Edgerunner.org">
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
using Antlr4.Runtime;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// Resolves a local-variable chain base from its nearest preceding assignment, by a pure walk over
/// the current parse tree. The returned chain is the assignment's right-hand side, ready for recursive
/// resolution by the <see cref="ChainExpressionEvaluator"/>.
/// </summary>
/// <remarks>
/// This walker is deliberately stateless and re-runs against the live parse tree on every request, so
/// variable resolution is recomputed per verb / per window and never cached. It does not reason about
/// control flow: it takes the textually-nearest assignment to the variable that precedes the caret,
/// regardless of <c>if</c>/<c>while</c>/<c>for</c> nesting. A right-hand side that is not a resolvable
/// chain (a verb call, function result, arithmetic, list/string literal, etc.) yields <c>null</c>.
/// </remarks>
public static class LocalVariableResolver
{
   /// <summary>
   /// Finds the nearest assignment to <paramref name="variable"/> preceding <paramref name="caretOffset"/>
   /// and returns its right-hand side as a resolvable chain descriptor.
   /// </summary>
   /// <param name="variable">The variable name to resolve.</param>
   /// <param name="tree">The current parse tree root.</param>
   /// <param name="caretOffset">The absolute caret offset into the buffer.</param>
   /// <returns>The right-hand side chain, or <c>null</c> when no resolvable preceding assignment exists.</returns>
   public static ChainDescriptor? ResolveAssignmentChain(string variable, ParserRuleContext? tree, int caretOffset)
   {
      if (string.IsNullOrEmpty(variable) || tree is null)
         return null;

      ParserRuleContext? rhs = null;
      var bestStart = -1;

      Walk(tree, node =>
      {
         if (node.GetType().Name != "AssignmentExpressionContext" || node.ChildCount < 3)
            return;

         // child 0 = LHS, child 1 = '=', child 2 = RHS.
         if (node.GetChild(0) is not ParserRuleContext lhs ||
             lhs.GetType().Name != "IdentifierExpressionContext")
            return;

         if (!string.Equals(lhs.GetText(), variable, StringComparison.Ordinal))
            return;

         var start = node.Start?.StartIndex ?? -1;

         // Must precede the caret, and be the nearest such assignment seen so far.
         if (start < 0 || start >= caretOffset || start <= bestStart)
            return;

         if (node.GetChild(2) is ParserRuleContext rightHandSide)
         {
            bestStart = start;
            rhs = rightHandSide;
         }
      });

      return rhs is null ? null : ChainExtractor.DescribeExpression(rhs);
   }

   // Pre-order walk over every ParserRuleContext in the tree.
   private static void Walk(ParserRuleContext node, Action<ParserRuleContext> visit)
   {
      visit(node);
      for (var i = 0; i < node.ChildCount; i++)
         if (node.GetChild(i) is ParserRuleContext child)
            Walk(child, visit);
   }
}
