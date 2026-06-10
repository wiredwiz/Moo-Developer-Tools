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

using Org.Edgerunner.Mud.Common.Querying;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// Resolves a member-completion operand to the object that should be queried for members.
/// </summary>
/// <remarks>
/// Resolution is deliberately conservative: only operands whose object identity is knowable
/// client-side resolve. <c>me</c>/<c>player</c> are deferred until a player-object-id source
/// exists; barewords and core-reference operands are unresolvable and yield <c>null</c>,
/// which silently skips member completion.
/// </remarks>
public static class MemberOperandResolver
{
   /// <summary>
   /// Resolves the operand of the supplied context to an object id.
   /// </summary>
   /// <param name="context">The detected member completion context.</param>
   /// <param name="contextObjectId">The object the edited verb lives on (the meaning of <c>this</c>), when known.</param>
   /// <returns>The object to query, or <c>null</c> when the operand cannot be resolved.</returns>
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
}
