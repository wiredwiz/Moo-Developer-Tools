#region BSD 3-Clause License
// <copyright file="VerbCompletionItem.cs" company="Edgerunner.org">
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

using FastColoredTextBoxNS;
using FastColoredTextBoxNS.Types;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// An autocomplete item for a world-queried verb. Unlike a plain <see cref="MemberCompletionItem"/>,
/// a verb is callable, so selecting it inserts <c>verbname()</c> with the caret positioned between
/// the parentheses — mirroring how arg-taking built-in functions insert (see <c>Moo.Builtins</c>).
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="AutocompleteItem.Text"/> stays the bare verb name so the popup's prefix matching
/// (inherited from <see cref="MemberCompletionItem.Compare"/>) and menu display are unaffected. The
/// call parentheses and caret marker are added only at insertion time by
/// <see cref="GetTextForReplace"/>.
/// </para>
/// <para>
/// Verb argument count is unknown from the world query, so a verb <em>always</em> inserts
/// <c>name(^)</c> (caret between) regardless of whether it takes arguments. This is intentional.
/// </para>
/// </remarks>
public sealed class VerbCompletionItem : MemberCompletionItem
{
   /// <summary>FCTB's caret-position marker within snippet/replacement text.</summary>
   private const char CaretMarker = '^';

   /// <summary>
   /// Initializes a new instance of the <see cref="VerbCompletionItem"/> class.
   /// </summary>
   /// <param name="verbName">The verb name offered for completion.</param>
   /// <param name="category">The icon category (<see cref="CompletionIconCategory.Verb"/> or
   /// <see cref="CompletionIconCategory.VerbInherited"/>).</param>
   public VerbCompletionItem(string verbName, CompletionIconCategory category)
      : base(verbName, category)
   {
   }

   /// <inheritdoc/>
   /// <remarks>Appends the call parentheses with the caret marker between them.</remarks>
   public override string GetTextForReplace() => base.GetTextForReplace() + "(" + CaretMarker + ")";

   /// <inheritdoc/>
   /// <remarks>
   /// After the replacement text (which contains the <see cref="CaretMarker"/>) is inserted, this
   /// walks the caret forward to the marker and removes it, leaving the caret between the parens.
   /// Mirrors <see cref="SnippetAutocompleteItem.OnSelected"/>.
   /// </remarks>
   public override void OnSelected(AutocompleteMenu popupMenu, SelectedEventArgs e)
   {
      e.Tb.BeginUpdate();
      e.Tb.Selection.BeginUpdate();

      // Start at the replaced fragment, move right until the caret marker is just behind us, then delete it.
      e.Tb.Selection.SetStartAndEnd(popupMenu.Fragment.Start);
      while (e.Tb.Selection.CharBeforeStart != CaretMarker)
         if (!e.Tb.Selection.GoRightThroughFolded())
            break;
      e.Tb.Selection.GoLeft(true);
      e.Tb.InsertText("");

      e.Tb.Selection.EndUpdate();
      e.Tb.EndUpdate();
   }
}
