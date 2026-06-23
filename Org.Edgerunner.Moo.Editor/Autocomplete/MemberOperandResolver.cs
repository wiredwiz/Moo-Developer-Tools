#region BSD 3-Clause License
// <copyright file="MemberOperandResolver.cs" company="Edgerunner.org">
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

using System.Text.RegularExpressions;
using Org.Edgerunner.Mud.Common.Querying;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// Resolves a member-completion operand to the object that should be queried for members.
/// </summary>
/// <remarks>
/// Resolution is deliberately conservative: only operands whose object identity is knowable
/// client-side resolve. <c>me</c>/<c>player</c> are deferred until a player-object-id source
/// exists; barewords remain unresolvable and yield <c>null</c>, which silently skips member
/// completion. Core-name operands (<c>$foo</c>) are resolved asynchronously by the controller
/// via a <c>#0</c> property-value lookup; <see cref="Resolve"/> still returns <c>null</c> for
/// them so the controller can distinguish them via <see cref="TryGetCoreName"/>.
/// </remarks>
public static class MemberOperandResolver
{
   private static readonly Regex CoreNamePattern =
      new(@"^\$([A-Za-z_]\w*)$", RegexOptions.Compiled);

   /// <summary>
   /// Resolves the operand of the supplied context to an object id.
   /// </summary>
   /// <param name="context">The detected member completion context.</param>
   /// <param name="contextObjectId">The object the edited verb lives on (the meaning of <c>this</c>), when known.</param>
   /// <returns>The object to query, or <c>null</c> when the operand cannot be resolved synchronously.</returns>
   public static MooObjectId? Resolve(MemberCompletionContext context, MooObjectId? contextObjectId)
   {
      switch (context.Kind)
      {
         case MemberContextKind.CoreReference:
            return new MooObjectId(0);
         case MemberContextKind.Verb:
         case MemberContextKind.Property:
            var operand = context.Operand;
            if (operand.StartsWith('#') && int.TryParse(operand[1..], out var number))
               return new MooObjectId(number);
            if (operand == "this")
               return contextObjectId;
            return null;
         default:
            return null;
      }
   }

   /// <summary>
   /// Returns <c>true</c> when the context is a Verb or Property context whose operand is a
   /// core-name reference (<c>$identifier</c>), setting <paramref name="name"/> to the bare
   /// name (without the leading <c>$</c>). Returns <c>false</c> with <paramref name="name"/>
   /// set to <see cref="string.Empty"/> for any other context or operand shape.
   /// </summary>
   /// <param name="context">The detected member completion context.</param>
   /// <param name="name">
   /// On return, the bare identifier (e.g. <c>"network"</c> for operand <c>"$network"</c>),
   /// or <see cref="string.Empty"/> when the method returns <c>false</c>.
   /// </param>
   /// <returns><c>true</c> when the operand is a valid core-name reference.</returns>
   public static bool TryGetCoreName(MemberCompletionContext context, out string name)
   {
      if (context.Kind == MemberContextKind.Verb || context.Kind == MemberContextKind.Property)
      {
         var m = CoreNamePattern.Match(context.Operand);
         if (m.Success)
         {
            name = m.Groups[1].Value;
            return true;
         }
      }

      name = string.Empty;
      return false;
   }

   /// <summary>
   /// Returns <c>true</c> when the context is a Verb or Property context whose operand is the
   /// predefined current-player variable (<c>player</c> or <c>caller</c>). Both resolve to the
   /// connected player's object in this phase; <see cref="Resolve"/> still returns <c>null</c> for
   /// them so the controller can resolve the player object asynchronously, mirroring the
   /// <see cref="TryGetCoreName"/> shape.
   /// </summary>
   /// <param name="context">The detected member completion context.</param>
   /// <returns><c>true</c> when the operand is <c>player</c> or <c>caller</c> in a member context.</returns>
   public static bool TryGetCurrentPlayerOperand(MemberCompletionContext context)
   {
      if (context.Kind != MemberContextKind.Verb && context.Kind != MemberContextKind.Property)
         return false;

      return context.Operand is "player" or "caller";
   }
}
