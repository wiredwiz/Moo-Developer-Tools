#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="MooObjectReferenceParser.cs">
// Copyright (c) Thaddeus Ryker 2026
// </copyright>
//
// BSD 3-Clause License
//
// Copyright (c) 2026,
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

namespace Org.Edgerunner.Mud.Common.Querying;

/// <summary>
/// Extracts MOO object references (<c>#n</c>) from free-form text such as local-edit upload
/// commands (<c>@program #123:verbname</c>) or simpleedit references.
/// </summary>
public static class MooObjectReferenceParser
{
   private static readonly Regex ObjectIdPattern = new(@"#(-?\d+)", RegexOptions.Compiled);

   /// <summary>
   /// Finds the first <c>#n</c> object reference in the supplied text.
   /// </summary>
   /// <param name="text">The text to scan. May be <c>null</c> or empty.</param>
   /// <returns>The first object id found, or <c>null</c> when the text contains none.</returns>
   public static MooObjectId? FindFirstObjectId(string? text)
   {
      if (string.IsNullOrEmpty(text))
         return null;

      var match = ObjectIdPattern.Match(text);
      return match.Success && int.TryParse(match.Groups[1].Value, out var number)
                ? new MooObjectId(number)
                : null;
   }
}
