#region BSD 3-Clause License
// <copyright file="MemberCompletionContext.cs" company="Edgerunner.org">
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

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// The kind of member completion context detected at the caret.
/// </summary>
public enum MemberContextKind
{
   /// <summary>Not a member completion position; only static completion applies.</summary>
   None,

   /// <summary>A core reference (<c>$foo</c>): completes properties of object <c>#0</c>.</summary>
   CoreReference,

   /// <summary>A verb call (<c>obj:verb</c>): completes verbs of the operand object.</summary>
   Verb,

   /// <summary>A property access (<c>obj.prop</c>): completes properties of the operand object.</summary>
   Property
}

/// <summary>
/// The member completion context detected from the text left of the caret.
/// </summary>
/// <param name="Kind">The context kind.</param>
/// <param name="Operand">The operand text left of the trigger character (empty for core references).</param>
public readonly record struct MemberCompletionContext(MemberContextKind Kind, string Operand)
{
   /// <summary>Gets the "no member context" value.</summary>
   public static MemberCompletionContext None => new(MemberContextKind.None, string.Empty);
}
