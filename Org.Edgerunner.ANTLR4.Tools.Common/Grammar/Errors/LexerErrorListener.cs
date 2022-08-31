﻿#region BSD 3-Clause License
// <copyright file="LexerErrorListener.cs" company="Edgerunner.org">
// Copyright 2021 Thaddeus Ryker
// </copyright>
//
// BSD 3-Clause License
//
// Copyright (c) 2021, Thaddeus Ryker
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

namespace Org.Edgerunner.ANTLR4.Tools.Common.Grammar.Errors
{
   /// <summary>
   /// Class for gathering ANTLR4 lexer errors during testing.
   /// Implements the <see cref="IToken" />
   /// </summary>
   public class LexerErrorListener : IAntlrErrorListener<int>
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="LexerErrorListener"/> class.
      /// </summary>
      public LexerErrorListener()
      {
         Document = null;
         Errors = new List<ParseMessage>();
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="LexerErrorListener"/> class.
      /// </summary>
      /// <param name="document">The source document.</param>
      public LexerErrorListener(DocumentInfo document)
         : this()
      {
         Document = document;
      }

      /// <summary>
      /// Gets or sets the source document.
      /// </summary>
      /// <value>
      /// The source document.
      /// </value>
      protected DocumentInfo Document { get; set; }

      /// <summary>
      /// Gets the parsing errors.
      /// </summary>
      /// <value>The parsing errors.</value>
      public List<ParseMessage> Errors { get; }

      /// <summary>
      /// Upon syntax error, notify any interested parties.
      /// </summary>
      /// <param name="recognizer">What parser got the error. From this
      /// object, you can access the context as well
      /// as the input stream.</param>
      /// <param name="offendingSymbol">The offending token in the input token
      /// stream, unless recognizer is a lexer (then it's null). If
      /// no viable alternative error,
      /// <paramref name="e" />
      /// has token at which we
      /// started production for the decision.</param>
      /// <param name="line">The line number in the input where the error occurred.</param>
      /// <param name="charPositionInLine">The character position within that line where the error occurred.</param>
      /// <param name="msg">The message to emit.</param>
      /// <param name="e">The exception generated by the parser that led to
      /// the reporting of an error. It is null in the case where
      /// the parser was able to recover in line without exiting the
      /// surrounding rule.</param>
      /// <remarks>Upon syntax error, notify any interested parties. This is not how to
      /// recover from errors or compute error messages.
      /// <see cref="T:Antlr4.Runtime.IAntlrErrorStrategy" />
      /// specifies how to recover from syntax errors and how to compute error
      /// messages. This listener's job is simply to emit a computed message,
      /// though it has enough information to create its own message in many cases.
      /// <p>The
      /// <see cref="T:Antlr4.Runtime.RecognitionException" />
      /// is non-null for all syntax errors except
      /// when we discover mismatched token errors that we can recover from
      /// in-line, without returning from the surrounding rule (via the single
      /// token insertion and deletion mechanism).</p></remarks>
      // ReSharper disable once TooManyArguments
      public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
      {
         Errors.Add(new ParseMessage(Document, line, charPositionInLine + 1, "Lexer", msg, null));
      }
   }
}