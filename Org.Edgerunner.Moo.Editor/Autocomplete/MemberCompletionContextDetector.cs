#region BSD 3-Clause License
// <copyright file="MemberCompletionContextDetector.cs" company="Edgerunner.org">
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

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// Classifies the member completion context from the text left of the caret on the current line.
/// </summary>
/// <remarks>
/// Only syntactically adjacent operands are recognized (<c>obj:</c>, <c>#123.</c>, <c>$frag</c>).
/// Chained expressions resolve to their last segment (a bareword, which the resolver will reject),
/// range operators (<c>..</c>) and float literals never match, and positions inside an open string
/// are never member contexts. MOO has no comment syntax inside verbs, so only strings are tracked.
/// </remarks>
public static class MemberCompletionContextDetector
{
   // <operand><separator><partial-member> anchored at the caret. Operand forms: bareword/keyword
   // (this, foo), core reference ($foo) or object literal (#123 / #-1).
   private static readonly Regex MemberPattern =
      new(@"(\$?[A-Za-z_]\w*|#-?\d+)([:.])\w*$", RegexOptions.Compiled);

   // A core-reference fragment ($ or $partialname) anchored at the caret.
   private static readonly Regex CoreRefPattern = new(@"\$\w*$", RegexOptions.Compiled);

   /// <summary>
   /// Detects the member completion context for the supplied line prefix.
   /// </summary>
   /// <param name="linePrefix">The text on the caret line, from column 0 up to the caret.</param>
   /// <returns>The detected context; <see cref="MemberCompletionContext.None"/> when not a member position.</returns>
   public static MemberCompletionContext Detect(string linePrefix)
   {
      if (string.IsNullOrEmpty(linePrefix) || IsInsideString(linePrefix))
         return MemberCompletionContext.None;

      var member = MemberPattern.Match(linePrefix);
      if (member.Success)
      {
         var kind = member.Groups[2].Value == ":" ? MemberContextKind.Verb : MemberContextKind.Property;
         return new MemberCompletionContext(kind, member.Groups[1].Value);
      }

      if (CoreRefPattern.IsMatch(linePrefix))
         return new MemberCompletionContext(MemberContextKind.CoreReference, string.Empty);

      return MemberCompletionContext.None;
   }

   private static bool IsInsideString(string linePrefix)
   {
      var inString = false;
      for (var i = 0; i < linePrefix.Length; i++)
      {
         var ch = linePrefix[i];
         if (inString && ch == '\\')
         {
            i++; // skip the escaped character
            continue;
         }

         if (ch == '"')
            inString = !inString;
      }

      return inString;
   }
}
