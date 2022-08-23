#region BSD 3-Clause License

// <copyright file="EditorSyntaxHighlighter.cs" company="Edgerunner.org">
// Copyright 2020
// </copyright>
//
// BSD 3-Clause License
//
// Copyright (c) 2020,
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

using Antlr4.Runtime;
using FastColoredTextBoxNS;
using Org.Edgerunner.ANTLR4.Tools.Common.Extensions;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar;
using Place = FastColoredTextBoxNS.Types.Place;

namespace Org.Edgerunner.Moo.Editor.SyntaxHighlighting
{
   /// <summary>
   /// Class used to handle syntax highlighting of a FastColoredTextBox editor.
   /// </summary>
   public class EditorSyntaxHighlighter
   {
      private int _TokenColoringInProgress;

      /// <summary>
      /// Gets a value indicating whether this instance is busy coloring.
      /// </summary>
      /// <value>
      ///   <c>true</c> if this instance is busy coloring; otherwise, <c>false</c>.
      /// </value>
      public bool IsBusy => _TokenColoringInProgress == 1;

      /// <summary>
      /// Colorizes the tokens.
      /// </summary>
      /// <param name="editor">The editor.</param>
      /// <param name="registry">The registry.</param>
      /// <param name="tokens">The tokens.</param>
      /// <param name="errorTokens">The error tokens.</param>
      public void ColorizeTokens(FastColoredTextBox editor, IStyleRegistry registry, IList<DetailedToken> tokens, IList<DetailedToken> errorTokens)
      {
         int coloring = Interlocked.Exchange(ref _TokenColoringInProgress, 1);
         if (coloring != 0)
            return;

         editor.BeginInvoke(
             new MethodInvoker(() =>
                {
                   editor.BeginUpdate();

                   try
                   {
                      for (int i = 0; i < tokens.Count; i++)
                      {
                         var token = tokens[i];
                         var prev = (i == 0) ? null : tokens[i - 1];
                         var next = (i == tokens.Count - 1) ? null : tokens[i + 1];
                         var startingPlace = new Place(token.Column, token.Line - 1);
                         var stoppingPlace = new Place(token.EndingColumn, token.EndingLine - 1);
                         var tokenRange = editor.GetRange(startingPlace, stoppingPlace);
                         tokenRange.ClearStyle(StyleIndex.All);
                         var style = registry.GetTokenStyle(token, prev, next);
                         tokenRange.SetStyle(style);
                      }
                      foreach (var token in errorTokens)
                      {
                         var startingPlace = new Place(token.Column, token.Line - 1);
                         var stoppingPlace = new Place(token.EndingColumn, token.EndingLine - 1);

                         var tokenRange = editor.GetRange(startingPlace, stoppingPlace);
                         tokenRange.SetStyle(registry.GetParseErrorStyle());
                      }
                   }
                   // ReSharper disable once CatchAllClause
                   catch (Exception ex)
                   {
                      // #TODO Fix this error handling to do something useful
                   }
                   finally
                   {
                      editor.EndUpdate();
                      _TokenColoringInProgress = 0;
                   }
                }));
      }
   }
}