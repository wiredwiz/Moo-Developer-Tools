#region BSD 3-Clause License
// <copyright file="ChainDescriptor.cs" company="Edgerunner.org">
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

using System.Collections.Generic;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// The kind of the leftmost element (the <em>base</em>) of a chained member-access expression.
/// </summary>
public enum ChainBaseKind
{
   /// <summary>An object literal (<c>#N</c>); <see cref="ChainBase.Text"/> is the number text.</summary>
   ObjectLiteral,

   /// <summary>A core reference (<c>$name</c>); <see cref="ChainBase.Text"/> is the bare name.</summary>
   CoreName,

   /// <summary>The <c>this</c> keyword (the edited verb's object).</summary>
   This,

   /// <summary>The <c>player</c> built-in variable.</summary>
   Player,

   /// <summary>The <c>caller</c> built-in variable.</summary>
   Caller,

   /// <summary>A local variable; <see cref="ChainBase.Text"/> is the variable name.</summary>
   Variable
}

/// <summary>
/// The leftmost element of a chained member-access expression.
/// </summary>
/// <param name="Kind">The base kind.</param>
/// <param name="Text">
/// The associated text: the number for <see cref="ChainBaseKind.ObjectLiteral"/>, the bare name for
/// <see cref="ChainBaseKind.CoreName"/> / <see cref="ChainBaseKind.Variable"/>, otherwise empty.
/// </param>
public readonly record struct ChainBase(ChainBaseKind Kind, string Text);

/// <summary>
/// A parsed chained member-access expression at the caret, ready for resolution.
/// </summary>
/// <param name="Base">The leftmost element of the chain.</param>
/// <param name="Steps">
/// The ordered <c>.prop</c> identifiers already typed between the base and the trailing separator.
/// </param>
/// <param name="MemberKind">
/// The member kind being completed: <see cref="MemberContextKind.Verb"/> for a <c>:</c> separator,
/// <see cref="MemberContextKind.Property"/> for a <c>.</c> separator, or
/// <see cref="MemberContextKind.CoreReference"/> for a bare <c>$fragment</c>.
/// </param>
/// <param name="PartialFragment">The already-typed member text after the trailing separator (filter text).</param>
public sealed record ChainDescriptor(
   ChainBase Base,
   IReadOnlyList<string> Steps,
   MemberContextKind MemberKind,
   string PartialFragment);
