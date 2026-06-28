#region BSD 3-Clause License
// <copyright file="MooCodeColorizer.cs" company="Edgerunner.org">
// Copyright 2024
// </copyright>
//
// BSD 3-Clause License
//
// Copyright (c) 2024,
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

using System.Drawing;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar;
using Org.Edgerunner.Moo.Editor.Configuration;

namespace Org.Edgerunner.Moo.Editor.SyntaxHighlighting
{
   /// <summary>
   /// Reusable helper that colorizes a single line of Moo source code using the same token+neighbor
   /// coloring rules the editor uses (<see cref="MooSyntaxHighlightingGuide"/>). It returns the line
   /// broken into ordered, contiguous (text, color) segments so the colors can be applied anywhere
   /// (editor or terminal) without duplicating the highlighting rules.
   /// </summary>
   public static class MooCodeColorizer
   {
      /// <summary>
      /// Matches ANSI SGR color escape sequences (the same pattern the console emulator uses).
      /// </summary>
      private static readonly Regex AnsiPattern = new(@"\e\[(\d+;)*\d+;*m", RegexOptions.Compiled);

      /// <summary>
      /// Strips ANSI color escape sequences from the supplied text.
      /// </summary>
      /// <param name="text">The text to strip.</param>
      /// <returns>The text with ANSI color codes removed.</returns>
      public static string StripAnsi(string text)
      {
         return text == null ? string.Empty : AnsiPattern.Replace(text, string.Empty);
      }

      /// <summary>
      /// Colorizes a single line of Moo code into ordered (text, color) segments.
      /// </summary>
      /// <param name="code">The line of code to colorize. ANSI color codes are stripped first.</param>
      /// <param name="dialect">The grammar dialect used to lex the code.</param>
      /// <param name="settings">
      /// The settings source for colors. When <see langword="null"/>, the singleton
      /// <see cref="Settings.Instance"/> is used.
      /// </param>
      /// <returns>
      /// An ordered list of (text, color) segments whose concatenated text equals the ANSI-stripped
      /// input. Text between tokens (and any unrecognized text) uses the default word color.
      /// </returns>
      public static IReadOnlyList<(string Text, Color Color)> GetColoredSegments(string code, GrammarDialect dialect, Settings settings = null)
      {
         settings ??= Settings.Instance;
         var stripped = StripAnsi(code);
         var segments = new List<(string Text, Color Color)>();

         if (string.IsNullOrEmpty(stripped))
            return segments;

         var defaultColor = settings.DefaultWordColor;

         try
         {
            var guide = new MooSyntaxHighlightingGuide(settings);
            var inputStream = new AntlrInputStream(stripped);
            var lexer = Moo.GetLexer(dialect, inputStream);
            lexer.TokenFactory = DetailedTokenFactory.Instance;
            lexer.RemoveErrorListeners();

            var tokenStream = new CommonTokenStream(lexer);
            tokenStream.Fill();

            // Build the significant token list (default channel, not EOF, not whitespace) so neighbor
            // relationships (':'+identifier, identifier+'(', '.'+identifier) are evaluated exactly as
            // they appear in the source, ignoring hidden whitespace tokens.
            var significant = new List<DetailedToken>();
            foreach (var raw in tokenStream.GetTokens())
               if (raw is DetailedToken detailed && IsSignificantToken(detailed))
                  significant.Add(detailed);

            var cursor = 0;
            for (var i = 0; i < significant.Count; i++)
            {
               var token = significant[i];
               var prev = i == 0 ? null : significant[i - 1];
               var next = i == significant.Count - 1 ? null : significant[i + 1];

               var start = token.StartIndex;
               var stop = token.StopIndex; // inclusive
               if (start < 0 || stop < start || start >= stripped.Length)
                  continue;
               if (stop >= stripped.Length)
                  stop = stripped.Length - 1;

               // Emit any plain text preceding this token (whitespace / gaps).
               if (start > cursor)
                  segments.Add((stripped.Substring(cursor, start - cursor), defaultColor));

               var color = guide.GetTokenForegroundColor(token, prev, next);
               segments.Add((stripped.Substring(start, stop - start + 1), color));
               cursor = stop + 1;
            }

            // Emit any trailing plain text.
            if (cursor < stripped.Length)
               segments.Add((stripped.Substring(cursor), defaultColor));
         }
         catch (Exception)
         {
            // On any lexing failure, fall back to rendering the whole line in the default color so the
            // terminal still shows the text rather than dropping it.
            segments.Clear();
            segments.Add((stripped, defaultColor));
         }

         return segments;
      }

      // Significant = default channel, not whitespace, not EOF (skips hidden whitespace tokens).
      private static bool IsSignificantToken(DetailedToken token)
         => token.Channel == 0 && token.Text != "<EOF>" && !string.IsNullOrWhiteSpace(token.Text);
   }
}
