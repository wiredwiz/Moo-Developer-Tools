﻿#region BSD 3-Clause License
// <copyright file="Settings.cs" company="Edgerunner.org">
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

using System.ComponentModel;
using System.Diagnostics;
using System.Security.Authentication.ExtendedProtection;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using FastColoredTextBoxNS;
using FastColoredTextBoxNS.Types;
using Org.Edgerunner.ANTLR4.Tools.Common;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar.Errors;
using Org.Edgerunner.ANTLR4.Tools.Common.Syntax;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Org.Edgerunner.Moo.Editor.Configuration;
using Org.Edgerunner.Moo.Editor.Language.Navigation;
using Org.Edgerunner.Moo.Editor.Language.Parsing;
using Org.Edgerunner.Moo.Editor.SyntaxHighlighting;
using Org.Edgerunner.MooSharp.Language.Grammar;
using Place = FastColoredTextBoxNS.Types.Place;

namespace Org.Edgerunner.Moo.Editor.Controls
{
   public partial class MooCodeEditor : FastColoredTextBox
   {
      public MooCodeEditor()
        : this(GrammarDialect.Edgerunner)
      { }

      public MooCodeEditor(GrammarDialect grammarDialect)
      {
         InitializeComponent();
         Tokens = new List<DetailedToken>();
         ParseErrors = new List<ParseMessage>();
         LexerErrorListener = new LexerErrorListener();
         ParserErrorListener = new ParserErrorListener();
         SyntaxHighlightingGuide = new MooSyntaxHighlightingGuide();
         StyleRegistry = new StyleRegistry(SyntaxHighlightingGuide);
         Highlighter = new EditorSyntaxHighlighter();
         LeftBracket = '(';
         LeftBracket2 = '{';
         LeftBracket3 = '[';
         RightBracket = ')';
         RightBracket2 = '}';
         RightBracket3 = ']';
         AutoCompleteBrackets = true;
         AutoCompleteBracketsList = new[] { '(', ')', '{', '}', '[', ']', '"', '"' };
         AutoIndentChars = false;
         WordWrapAutoIndent = true;
         FoldingHighlightEnabled = true;
         LineInterval = 4;
         AutoIndentNeeded += MooEditor_AutoIndentNeeded;
         TextChanged += MooEditor_TextChanged;
         TextChangedDelayed += MooEditor_TextChangedDelayed;
         KeyDown += MooEditor_KeyDown;
         UndoRedoStateChanged += MooEditor_UndoRedoStateChanged;
         GrammarDialect = grammarDialect;
      }

      public override int TabLength
      {
          get => base.TabLength;
          set
          {
              base.TabLength = value;
              IndentationGuide = Moo.GetIndentationGuide(GrammarDialect, TabLength);
          }
      }

      private void MooEditor_UndoRedoStateChanged(object sender, EventArgs e)
      {
         if (this.Document != null)
         {
            var context = ParseSourceCode();
            ClearAllStyles();
            if (!string.IsNullOrEmpty(Text))
            {
               // Configure code folding markers
               ConfigureCodeFolding(context);

               // Perhaps we will find a range in the future rather than passing null to colorize all of it
               ColorizeTokens(null);
            }
         }
      }

      protected DocumentInfo _Document;

      protected GrammarDialect _GrammarDialect;

      private bool _ShowCodeFolding;

      protected ParserErrorListener ParserErrorListener { get; set; }

      protected LexerErrorListener LexerErrorListener { get; set; }

      protected ISyntaxHighlightingGuide SyntaxHighlightingGuide { get; }

      protected IStyleRegistry StyleRegistry { get; }

      [Browsable(false)]
      public EditorSyntaxHighlighter Highlighter { get; private set; }

      protected IMooIndentationGuide IndentationGuide { get; set; }

      [Browsable(false)]
      public List<DetailedToken> Tokens { get; private set; }

      [Browsable(false)]
      public List<ParseMessage> ParseErrors { get; private set; }

      [Browsable(false)]
      public AutocompleteMenu AutocompleteMenu { get; set; }

      /// <summary>
      /// Determines whether to display code folding for moo code.
      /// </summary>
      [DefaultValue(false)]
      [Description("Shows code folding guides for moo code.")]
      public bool ShowCodeFolding
      {
         get => _ShowCodeFolding;
         set
         {
            _ShowCodeFolding = value;
            if (value)
               ConfigureCodeFolding(ParseSourceCode());
            else
               ClearFoldingMarkers();
            Invalidate();
         }
      }

      [Browsable(false)]
      public GrammarDialect GrammarDialect
      {
         get => _GrammarDialect;
         set
         {
            _GrammarDialect = value;
            IndentationGuide = Moo.GetIndentationGuide(value, TabLength);
            var context = ParseSourceCode();

            ClearAllStyles();

            if (!string.IsNullOrEmpty(Text))
            {
               // Configure code folding markers
               ConfigureCodeFolding(context);

               // Colorize tokens
               ColorizeTokens(null);
            }
         }
      }

      [Browsable(false)]
      public DocumentInfo Document
      {
         get => _Document;

         set
         {
            _Document = value;
            LexerErrorListener = new LexerErrorListener(_Document);
            ParserErrorListener = new ParserErrorListener(_Document);
         }
      }

        /// <summary>
        /// Occurs when [parsing complete].
        /// </summary>
        public event EventHandler<ParsingCompleteEventArgs> ParsingComplete;

      private void MooEditor_KeyDown(object sender, KeyEventArgs e)
      {
         if ((e.KeyCode == Keys.OemSemicolon && e.Modifiers == 0) && NoTextSelection())

         {
            // We add 1 to each because the selection values start at 0 rather than the human reference 1
            var token = FindSurroundingTokenForPosition(Selection.Start.iLine + 1, Selection.Start.iChar + 1);
            if (token == null ||
                (token.TypeNameUpperCase != "SINGLE_LINE_COMMENT" &&
                 token.TypeNameUpperCase != "DELIMITED_COMMENT" &&
                 token.TypeNameUpperCase != "STRING"))
            {
               while (Selection.CharAfterStart != '\r' && Selection.CharAfterStart != '\n')
                  Selection.GoRight();
               if (Selection.CharBeforeStart == ';')
                  e.Handled = true;
               DoCaretVisible();
               Invalidate();
            }
         }
         else if (e.KeyCode == Keys.Back && NoTextSelection() && Selection.Start.iChar != 0)
         {
            if ((Selection.CharBeforeStart == '(' && Selection.CharAfterStart == ')') ||
                (Selection.CharBeforeStart == '{' && Selection.CharAfterStart == '}') ||
                (Selection.CharBeforeStart == '"' && Selection.CharAfterStart == '"') ||
                (Selection.CharBeforeStart == '[' && Selection.CharAfterStart == ']'))
               Selection = new TextSelectionRange(this,
                  new Place(Selection.Start.iChar - 1, Selection.Start.iLine),
                  new Place(Selection.End.iChar + 1, Selection.End.iLine));
         }
      }

      private bool NoTextSelection()
      {
         return Selection.Start.iLine == Selection.End.iLine &&
                Selection.Start.iChar == Selection.End.iChar;
      }

      private void MooEditor_TextChanged(object? sender, TextChangedEventArgs e)
      {
         if (AutoIndent)
         {
            ParseSourceCode(false);
            DoAutoIndentIfNeed();
         }
      }


      private void MooEditor_TextChangedDelayed(object sender, TextChangedEventArgs e)
      {
         var context = ParseSourceCode();
         ClearAllStyles();
         if (!string.IsNullOrEmpty(Text))
         {
            // Configure code folding markers
            ConfigureCodeFolding(context);

            // Perhaps we will find a range in the future rather than passing null to colorize all of it
            ColorizeTokens(null);
         }
      }

      private IList<DetailedToken> FindTokensInRange(IList<DetailedToken> tokens, TextSelectionRange range)
      {
         var results = new List<DetailedToken>();
         var startLine = range.FromLine;
         var stopLine = range.ToLine + 2;

         foreach (var token in tokens)
         {
            if (token.Line < startLine || token.Line > stopLine)
               continue;

            results.Add(token);
         }

         return results;
      }

      public DetailedToken FindTokenForPosition(int line, int position)
      {
         if (Tokens == null)
            return null;

         foreach (var token in Tokens)
         {
            if (token.Line <= line && token.EndingLine >= line)
               if (token.ColumnPosition <= position && token.EndingColumn >= position)
                  return token;
         }

         return null;
      }

      public DetailedToken FindSurroundingTokenForPosition(int line, int position)
      {
         if (Tokens == null)
            return null;

         foreach (var token in Tokens)
         {
            if (IsWithinTokenBounds(token, line, position))
               return token;
         }

         return null;
      }

      private bool IsWithinTokenBounds(DetailedToken token, int line, int position)
      {
         if (token.Line <= line && token.EndingLine >= line)
         {
            if (token.Line == line && token.StartingColumn >= position)
               return false;
            if (token.EndingLine == line && token.EndingColumn < position)
               return false;

            return true;
         }

         return false;
      }

      private List<ISyntaxErrorGuide> GetErrorGuides()
      {
         var result = new List<ISyntaxErrorGuide>();

         if (ParseErrors == null)
            return result;

         foreach (var error in ParseErrors)
            if (error.Guide != null)
               result.Add(error.Guide);

         return result;
      }

      public void ColorizeTokens(TextSelectionRange range)
      {
         if (Handle == IntPtr.Zero)
            return;

         var tokensToColor = range == null ? Tokens : FindTokensInRange(Tokens, range);
         Highlighter.ColorizeTokens(this, StyleRegistry, tokensToColor, GetErrorGuides());
      }

      public ParserRuleContext ParseSourceCode(bool updateGui = true)
      {
         // Generate our lexer
         var inputStream = new AntlrInputStream(this.Text);
         var lexer = Moo.GetLexer(GrammarDialect, inputStream);

         lexer.TokenFactory = DetailedTokenFactory.Instance;

         // Attach our error listener
         if (updateGui)
            if (LexerErrorListener != null)
            {
               LexerErrorListener.Errors.Clear();
               lexer.RemoveErrorListeners();
               lexer.AddErrorListener(LexerErrorListener);
            }

         // Fetch our tokens
         var commonTokenStream = new CommonTokenStream(lexer);
         commonTokenStream.Fill();
         Tokens = commonTokenStream.GetTokens().Cast<DetailedToken>().ToList();

         // Create our parser and attach our error listener
         var parser = Moo.GetParser(GrammarDialect, commonTokenStream);

         if (IndentationGuide != null)
            parser.AddParseListener(IndentationGuide);

         parser.RemoveErrorListeners();
         if (updateGui)
            if (ParserErrorListener != null)
            {
               ParserErrorListener.Errors.Clear();
               parser.AddErrorListener(ParserErrorListener);
            }

         // Let's parse!
         ParserRuleContext context;
         if (GrammarDialect == GrammarDialect.ToastStunt)
            context = ((ToastStuntMooParser)parser).code();
         else if (GrammarDialect == GrammarDialect.Edgerunner)
            context = ((EdgerunnerMooParser)parser).code();
         else
            context = ((MooParser)parser).code();

         if (updateGui)
         {
            // Publish our errors
            ParseErrors.Clear();
            if (LexerErrorListener?.Errors != null)
               ParseErrors.AddRange(LexerErrorListener.Errors);

            if (ParserErrorListener?.Errors != null)
            {
               var whitespaceId = -1;
               if (GrammarDialect == GrammarDialect.Edgerunner)
                  whitespaceId = EdgerunnerMooLexer.WS;
               else if (GrammarDialect == GrammarDialect.ToastStunt)
                  whitespaceId = ToastStuntMooLexer.WS;
               else if (GrammarDialect == GrammarDialect.LambdaMoo)
                  whitespaceId = MooLexer.WS;
               foreach (var current in ParserErrorListener.Errors)
               {
                  var error = current;
                  if (error.Guide is DetailedToken bad)
                  {
                     var tokenIndex = bad.TokenIndex;
                     // Keeping this in place, in case I modify FCTB for painting styles beyond the character region.
                     //while (tokenIndex > 0)
                     //{
                     //   tokenIndex--;
                     //   var previousToken = commonTokenStream.Get(tokenIndex) as DetailedToken;
                     //   if (previousToken?.TypeNameUpperCase != "WS")
                     //      break;
                     //   bad = previousToken;
                     //}

                     //error.Guide = bad;
                     //error.LineNumber = bad.Line;
                     //error.Column = bad.StartingColumn;
                     //error.Message = error.Message.Replace("' at '", "' before '");
                     while (tokenIndex > 0)
                     {
                        tokenIndex--;
                        bad = commonTokenStream.Get(tokenIndex) as DetailedToken;
                        if (bad?.Type != whitespaceId)
                           break;
                     }
                     if (bad != null)
                     {
                        error.Guide = bad;
                        error.LineNumber = bad.EndingLine;
                        error.Column = bad.EndingColumn + 1;
                        error.Message = error.Message.Replace("' at '", "' before '");
                     }
                  }
                  ParseErrors.Add(error);
               }
            }

            // Do extra syntax validation
            if (GrammarDialect == GrammarDialect.Edgerunner)
            {
               var validator = new EdgerunnerMooValidator
                               {
                                  Document = Document
                               };
               ParseTreeWalker.Default.Walk(validator, context);
               ParseErrors.AddRange(validator.Errors);
            }
            else if (GrammarDialect == GrammarDialect.ToastStunt)
            {
               var validator = new ToastStuntMooValidator
                               {
                                  Document = Document
                               };
               ParseTreeWalker.Default.Walk(validator, context);
               ParseErrors.AddRange(validator.Errors);
            }
            else if (GrammarDialect == GrammarDialect.LambdaMoo)
            {
               var validator = new LambdaMooValidator
                               {
                                  Document = Document
                               };
               ParseTreeWalker.Default.Walk(validator, context);
               ParseErrors.AddRange(validator.Errors);
            }

            // Raise our event
            OnParsingComplete(this, new ParsingCompleteEventArgs(Document, ParseErrors, Tokens, context));
         }

         return context;
      }

      private void ConfigureCodeFolding(ParserRuleContext context)
      {
         if (!ShowCodeFolding)
            return;

         IList<string> parserRules;

         // Generate our list of parser rule names
         if (GrammarDialect == GrammarDialect.Edgerunner)
            parserRules = EdgerunnerMooParser.ruleNames;
         else if (GrammarDialect == GrammarDialect.ToastStunt)
            parserRules = ToastStuntMooParser.ruleNames;
         else if (GrammarDialect == GrammarDialect.LambdaMoo)
            parserRules = MooParser.ruleNames;
         else
            parserRules = new List<string>();

         // Configure code folding markers
         ProcessTreeForFolding(context, parserRules);
      }

      private void ProcessTreeForFolding(IParseTree node, IList<string> parserRules)
      {
         if (node != null)
         {
            var nodeName = Trees.GetNodeText(node, parserRules);
            switch (nodeName)
            {
               case "ifStatement":
               case "forStatement":
               case "forkStatement":
               case "whileStatement":
               case "tryExceptStatement":
               case "tryFinallyStatement":
                  if (node is ParserRuleContext rule)
                  {
                     var marker = Guid.NewGuid().ToString("N");
                     this[rule.Start.Line - 1].FoldingStartMarker = marker;
                     this[rule.stop.Line - 1].FoldingEndMarker = marker;
                     Invalidate();
                  }
                  break;
            }
            for (int i = 0; i < node.ChildCount; i++)
               ProcessTreeForFolding(node.GetChild(i), parserRules);
         }
      }

      /// <summary>
      /// Called when [parsing complete].
      /// </summary>
      /// <param name="mooCodeEditor">The moo editor.</param>
      /// <param name="parsingCompleteEventArgs">The <see cref="ParsingCompleteEventArgs"/> instance containing the event data.</param>
      private void OnParsingComplete(MooCodeEditor mooCodeEditor, ParsingCompleteEventArgs parsingCompleteEventArgs)
      {
         ParsingComplete?.Invoke(mooCodeEditor, parsingCompleteEventArgs);
      }

      public void ToggleFoldingBlock(int iLine)
      {
         var fLine = -1;
         for (int i = iLine; i >= 0; i--)
         {
            var line = TextSource[i];
            if (!string.IsNullOrEmpty(line.FoldingStartMarker) && string.IsNullOrEmpty(line.FoldingEndMarker))
            {
               fLine = i;
               break;
            }
         }

         if (fLine == -1)
            return;

         LineInfo lineInfo = LineInfos[fLine];
         if (lineInfo.VisibleState == VisibleState.Visible)
            CollapseFoldingBlock(fLine);
         else
            ExpandFoldedBlock(fLine);
      }

      private void MooEditor_AutoIndentNeeded(object? sender, AutoIndentEventArgs e)
      {
         if (IndentationGuide != null)
         {
            var indentShift = IndentationGuide.GetIndentShift(e.ILine + 1);
            e.Shift = indentShift;
            e.ShiftNextLines = indentShift;
            return;
         }
      }
   }
}