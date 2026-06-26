#region BSD 3-Clause License
// <copyright file="ConstantHoverResolver.cs" company="Edgerunner.org">
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

namespace Org.Edgerunner.Moo.Editor;

/// <summary>
/// Pure, control-free decision for a constant hover: prefer a live-queried value, otherwise fall back to
/// the baked-in table value. Extracted so it can be unit-tested without the editor/FCTB control.
/// </summary>
public static class ConstantHoverResolver
{
   /// <summary>
   /// Resolves the display value for a constant hover.
   /// </summary>
   /// <param name="kind">The constant kind (<c>type</c>, <c>error</c> or <c>bool</c>).</param>
   /// <param name="liveResult">The value returned by a live world query, or <c>null</c> when offline/unsupported.</param>
   /// <param name="bakedResult">The baked-in table value (from <see cref="BuiltinConstantDocs"/>).</param>
   /// <returns>
   /// The live value when it is non-empty and the constant is not a boolean literal; otherwise the baked-in
   /// value (which may itself be <c>null</c> for an unknown constant).
   /// </returns>
   public static string ResolveConstantDisplay(string kind, string liveResult, string bakedResult)
   {
      // Booleans are literal-valued (true/false): never overridden by a live query.
      if (kind != "bool" && !string.IsNullOrEmpty(liveResult))
         return liveResult;

      return bakedResult;
   }
}
