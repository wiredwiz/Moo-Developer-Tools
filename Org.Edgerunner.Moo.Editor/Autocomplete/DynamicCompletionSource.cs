#region BSD 3-Clause License
// <copyright file="DynamicCompletionSource.cs" company="Edgerunner.org">
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
using System.Collections;
using System.Collections.Generic;
using FastColoredTextBoxNS.Types;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// The autocomplete item source handed to the popup menu. The menu re-enumerates its source on
/// every refresh, so each enumeration injects the current world-queried member items (if any)
/// ahead of the static keyword/builtin/snippet items.
/// </summary>
public sealed class DynamicCompletionSource : IEnumerable<AutocompleteItem>
{
   private readonly IReadOnlyList<AutocompleteItem> _staticItems;

   private readonly MemberCompletionController _controller;

   private readonly Func<string> _linePrefixProvider;

   /// <summary>
   /// Initializes a new instance of the <see cref="DynamicCompletionSource"/> class.
   /// </summary>
   /// <param name="staticItems">The static completion items (keywords, builtins, snippets).</param>
   /// <param name="controller">The member completion controller.</param>
   /// <param name="linePrefixProvider">Returns the caret line text from column 0 up to the caret.</param>
   /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
   public DynamicCompletionSource(
      IReadOnlyList<AutocompleteItem> staticItems,
      MemberCompletionController controller,
      Func<string> linePrefixProvider)
   {
      _staticItems = staticItems ?? throw new ArgumentNullException(nameof(staticItems));
      _controller = controller ?? throw new ArgumentNullException(nameof(controller));
      _linePrefixProvider = linePrefixProvider ?? throw new ArgumentNullException(nameof(linePrefixProvider));
   }

   /// <inheritdoc/>
   public IEnumerator<AutocompleteItem> GetEnumerator()
   {
      foreach (var item in _controller.GetMemberItems(_linePrefixProvider()))
         yield return item;

      foreach (var item in _staticItems)
         yield return item;
   }

   /// <inheritdoc/>
   IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
