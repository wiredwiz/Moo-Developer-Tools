#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="MooText.cs">
// Copyright (c)  2022
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

using System.Text;

namespace Org.Edgerunner.Moo.MooText;

/// <summary>
/// Class for formatting moo text.
/// </summary>
public static class MooText
{
   /// <summary>
   /// Converts the supplied Moo text to html.
   /// </summary>
   /// <param name="text">The text to convert.</param>
   /// <param name="processorPipeline">The processor pipeline.</param>
   /// <returns>The resulting formatted HTML text.</returns>
   public static string ToHtml(string text, MooTextPipeline? processorPipeline)
   {
      if (string.IsNullOrEmpty(text))
         return string.Empty;

      processorPipeline?.Reset();
      var builder = new StringBuilder(text.Length + 40);
      var position = 0;
      var chars = text.ToCharArray();
      while (position < chars.Length)
      {
         if (processorPipeline != null)
         {
            if (!processorPipeline.Process(ref chars, ref position, ref builder))
               ProcessCharacter(ref chars, ref position, ref builder);
         }
         else
            ProcessCharacter(ref chars, ref position, ref builder);
      }

      return builder.ToString();
   }

   /// <summary>
   /// Processes the character.
   /// </summary>
   /// <param name="chars">The character array.</param>
   /// <param name="position">The current position.</param>
   /// <param name="builder">The output string builder.</param>
   private static void ProcessCharacter(ref char[] chars, ref int position, ref StringBuilder builder)
   {
      var c = chars[position++];
      if (c == '\r')
      {
         // do nothing
      }
      else if (c == '\n')
      {
         builder.Append(c);
         builder.Append("<br>");
      }
      else
         builder.Append(c);
   }
}