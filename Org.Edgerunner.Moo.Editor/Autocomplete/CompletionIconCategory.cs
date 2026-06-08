#region BSD 3-Clause License
// <copyright file="CompletionIconCategory.cs" company="Edgerunner.org">
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
/// Category of an autocomplete completion item, used to select the icon painted
/// in the autocomplete popup's left gutter.
/// </summary>
/// <remarks>
/// The numeric value of each member is significant: it is used directly as the
/// <c>ImageIndex</c> into the autocomplete menu's <see cref="System.Windows.Forms.ImageList"/>
/// built by <see cref="CompletionIconFactory"/>. The declaration order here must
/// stay in sync with the order images are added to that image list.
/// </remarks>
public enum CompletionIconCategory
{
   /// <summary>A built-in constant (error codes, type names, true/false). CHIP, amber.</summary>
   Constant = 0,

   /// <summary>A built-in variable (player, caller, this, args, ...). CHIP, blue.</summary>
   Variable = 1,

   /// <summary>A core reference ($foo). LINE, green. Designed but not yet wired.</summary>
   CoreReference = 2,

   /// <summary>A verb. SOLID, purple. Designed but not yet wired.</summary>
   Verb = 3,

   /// <summary>A property. SOLID, cyan. Designed but not yet wired.</summary>
   Property = 4,

   /// <summary>A built-in function. LINE, pink.</summary>
   Function = 5,

   /// <summary>A code snippet. SOLID, lime.</summary>
   Snippet = 6,

   /// <summary>A control-flow keyword (if, while, for, try, ...). CHIP, steel.</summary>
   Keyword = 7
}
