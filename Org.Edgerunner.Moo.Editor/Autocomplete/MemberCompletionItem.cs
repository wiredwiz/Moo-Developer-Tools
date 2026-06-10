#region BSD 3-Clause License
// <copyright file="MemberCompletionItem.cs" company="Edgerunner.org">
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
using FastColoredTextBoxNS.Types;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// An autocomplete item for a world-queried member (verb, property or core reference).
/// Matches only the typed part after the last member separator and replaces the whole
/// fragment with the original prefix plus the member name.
/// </summary>
/// <remarks>
/// The popup fragment includes the operand (for example <c>this:te</c>), because the menu's
/// search pattern treats <c>:</c>, <c>.</c> and <c>$</c> as fragment characters. Like the
/// upstream <see cref="MethodAutocompleteItem"/>, <see cref="Compare"/> records the fragment
/// prefix so <see cref="GetTextForReplace"/> can reproduce it.
/// </remarks>
public class MemberCompletionItem : AutocompleteItem
{
   private static readonly char[] SeparatorChars = { ':', '.', '$' };

   private string _replacementPrefix = string.Empty;

   /// <summary>
   /// Initializes a new instance of the <see cref="MemberCompletionItem"/> class.
   /// </summary>
   /// <param name="memberName">The member name offered for completion.</param>
   /// <param name="category">The icon category (verb, property or core reference).</param>
   public MemberCompletionItem(string memberName, CompletionIconCategory category)
      : base(memberName, (int)category)
   {
   }

   /// <inheritdoc/>
   public override CompareResult Compare(string fragmentText)
   {
      var index = fragmentText.LastIndexOfAny(SeparatorChars);
      if (index < 0)
         return CompareResult.Hidden;

      _replacementPrefix = fragmentText[..(index + 1)];
      var typed = fragmentText[(index + 1)..];
      if (typed.Length == 0)
         return CompareResult.Visible;

      return Text.StartsWith(typed, StringComparison.OrdinalIgnoreCase)
                ? CompareResult.VisibleAndSelected
                : CompareResult.Hidden;
   }

   /// <inheritdoc/>
   public override string GetTextForReplace() => _replacementPrefix + Text;
}
