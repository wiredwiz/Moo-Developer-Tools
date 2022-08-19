#region BSD 3-Clause License
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
using Org.Edgerunner.Moo.Editor.SyntaxHighlighting;
using Org.Edgerunner.MooSharp.Language.Grammar;
using Place = FastColoredTextBoxNS.Types.Place;

namespace Org.Edgerunner.Moo.Editor.Controls
{
   public partial class MooEditor : FastColoredTextBox
   {
      public MooEditor()
        :this(GrammarDialect.Edgerunner)
      {}

      public MooEditor(GrammarDialect grammarDialect)
      {
         InitializeComponent();
         Tokens = new List<DetailedToken>();
         ParseErrors = new List<ParseMessage>();
         LexerErrorListener = new LexerErrorListener();
         ParserErrorListener = new ParserErrorListener();
         SyntaxHighlightingGuide = new MooSyntaxHighlightingGuide();
         StyleRegistry = new StyleRegistry(SyntaxHighlightingGuide);
         Highlighter = new EditorSyntaxHighlighter();
         IndentationGuide = Moo.GetIndentationGuide(GrammarDialect, TabLength);
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
         ChangedLineColor = Color.Yellow;
         AutoIndentNeeded += MooEditor_AutoIndentNeeded;
         TextChanged += MooEditor_TextChanged;
         TextChangedDelayed += MooEditor_TextChangedDelayed;
         KeyDown += MooEditor_KeyDown;
         GrammarDialect = grammarDialect;
      }

      protected Document _Document;

      protected GrammarDialect _GrammarDialect;

      protected ParserErrorListener ParserErrorListener { get; set; }

      protected LexerErrorListener LexerErrorListener { get; set; }

      protected ISyntaxHighlightingGuide SyntaxHighlightingGuide { get; }

      protected IStyleRegistry StyleRegistry { get; }

      public EditorSyntaxHighlighter Highlighter { get; private set; }

      protected IMooIndentationGuide IndentationGuide { get; set; }

      public List<DetailedToken> Tokens { get; private set; }

      public List<ParseMessage> ParseErrors { get; private set; }

      public AutocompleteMenu AutocompleteMenu { get; set; }

      public GrammarDialect GrammarDialect
      {
         get => _GrammarDialect;
         set
         {
            _GrammarDialect = value;
            IndentationGuide = Moo.GetIndentationGuide(value, TabLength);
            ParseSourceCode();
            ColorizeTokens(null);
         }
      }

      public Document Document
      {
         get => _Document;

         set
         {
            _Document = value;
            LexerErrorListener = new LexerErrorListener(_Document);
            ParserErrorListener = new ParserErrorListener(_Document);
         }
      }

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
         ParseSourceCode();

         if (string.IsNullOrEmpty(Text))
            ClearStyle(StyleIndex.All);
         else
         {
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

      private List<IToken> GetErrorTokens()
      {
         var result = new List<IToken>();

         if (ParseErrors == null)
            return result;

         foreach (var error in ParseErrors)
            if (error.Token != null)
               result.Add(error.Token);

         return result;
      }

      public void ColorizeTokens(TextSelectionRange range)
      {
         if (Handle == IntPtr.Zero)
            return;

         var tokensToColor = range == null ? Tokens : FindTokensInRange(Tokens, range);
         Highlighter.ColorizeTokens(this, StyleRegistry, tokensToColor, GetErrorTokens());
      }

      public void ParseSourceCode(bool updateGui = true)
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

         if (updateGui)
            if (ParserErrorListener != null)
            {
               ParserErrorListener.Errors.Clear();
               parser.RemoveErrorListeners();
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
               ParseErrors.AddRange(ParserErrorListener.Errors);

            // Configure code folding markers
            ConfigureCodeFolding(context, ToastStuntMooParser.ruleNames);

            // Raise our event
            OnParsingComplete(this, new ParsingCompleteEventArgs(Document, ParseErrors, Tokens, context));
         }
      }

      private void ConfigureCodeFolding(IParseTree tree, IList<string> parserRules)
      {
         ProcessTreeForFolding(tree, parserRules);
      }

      private void ProcessTreeForFolding(IParseTree node, IList<string> parserRules)
      {
         if (node != null)
         {
            var nodeName = Trees.GetNodeText(node, parserRules);
            switch (nodeName)
            {
               case "ifStatement":
               case "elseifStatement":
               case "elseStatement":
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
      /// <param name="mooEditor">The moo editor.</param>
      /// <param name="parsingCompleteEventArgs">The <see cref="ParsingCompleteEventArgs"/> instance containing the event data.</param>
      private void OnParsingComplete(MooEditor mooEditor, ParsingCompleteEventArgs parsingCompleteEventArgs)
      {
         ParsingComplete?.Invoke(mooEditor, parsingCompleteEventArgs);
      }

      private void MooEditor_AutoIndentNeeded(object? sender, AutoIndentEventArgs e)
      {
         //if (IndentationGuide != null)
         //{
         //   var indentShift = IndentationGuide.GetIndentShift(e.ILine + 1);
         //   e.Shift = indentShift;
         //   e.ShiftNextLines = indentShift;
         //   return;
         //}

         bool indent = Regex.IsMatch(e.PrevLineText, @"\b(if|while|fork|for|try|elseif|else|except|finally)\b");
         bool previousTerminates = Regex.IsMatch(e.PrevLineText, @"\b(endif|endfork|endfor|endwhile|endtry)\b");
         bool currentIndents = Regex.IsMatch(e.LineText, @"\b(if|fork|for|while|try)\b");
         bool unIndent = Regex.IsMatch(e.LineText,
            @"\b(elseif|else|except|finally|endif|endfork|endfor|endwhile|endtry)\b");
         bool isCommenting = Regex.IsMatch(e.PrevLineText, @".*/\*.*");
         bool terminatesComment = Regex.IsMatch(e.PrevLineText, @".*\*/.*");

         // Check for multi-line comments
         if (isCommenting)
         {
            e.Shift = 3;
            e.ShiftNextLines = 3;
            return;
         }
         if (terminatesComment)
         {
            e.Shift = -3;
            e.ShiftNextLines = -3;
            return;
         }

         // The previous line had something like "if (foo) bah; endif" and the current line contains something like "else"
         if (indent && previousTerminates && unIndent)
         {
            e.Shift = -e.TabLength;
            e.ShiftNextLines = -e.TabLength;
            return;
         }

         // The previous line had something like "if (foo) bah; endif" and our current line has nothing special
         if (indent && previousTerminates)
            return;

         // The previous line starting an indentation block and our current line does not contain something that would cause an un-indent
         if (indent && !unIndent)
         {
            e.Shift = e.TabLength;
            e.ShiftNextLines = e.TabLength;
            return;
         }

         // Our current line contains something like "if (foo) bah; endif"
         if (unIndent && currentIndents)
            return;

         // Lastly our current line contains something that would cause an un-indent and the previous line does not cause an indent
         if (unIndent && !indent)
         {
            e.Shift = -e.TabLength;
            e.ShiftNextLines = -e.TabLength;
         }
      }
   }
}