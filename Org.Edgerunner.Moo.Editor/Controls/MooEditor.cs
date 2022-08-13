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
using Org.Edgerunner.Moo.Editor.SyntaxHighlighting;
using Org.Edgerunner.MooSharp.Language.Grammar;

namespace Org.Edgerunner.Moo.Editor.Controls
{
   public partial class MooEditor : FastColoredTextBox
   {
      public MooEditor()
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
         WordWrap = true;
         AutoIndentChars = false;
         WordWrapAutoIndent = true;
         WordWrapIndent = 2;
         TabLength = 2;
         AutoIndentNeeded += MooEditor_AutoIndentNeeded;
         TextChangedDelayed += MooEditor_TextChangedDelayed;
      }

      protected Document _Document;

      protected ParserErrorListener ParserErrorListener { get; set; }

      protected LexerErrorListener LexerErrorListener { get; set; }

      protected ISyntaxHighlightingGuide SyntaxHighlightingGuide { get; }

      protected IStyleRegistry StyleRegistry { get; }

      protected EditorSyntaxHighlighter Highlighter { get; set; }

      public List<DetailedToken> Tokens { get; private set; }

      public List<ParseMessage> ParseErrors { get; private set; }

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

      private void MooEditor_TextChangedDelayed(object? sender, TextChangedEventArgs e)
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

      private void ColorizeTokens(TextSelectionRange range)
      {
         if (Handle == IntPtr.Zero)
            return;

         var tokensToColor = range == null ? Tokens : FindTokensInRange(Tokens, range);
         Highlighter.ColorizeTokens(this, StyleRegistry, tokensToColor, GetErrorTokens());
      }

      public void ParseSourceCode()
      {
         // Generate our lexer
         var inputStream = new AntlrInputStream(this.Text);
         var lexer = new ToastStuntMooLexer(inputStream);
         lexer.TokenFactory = DetailedTokenFactory.Instance;

         // Attach our error listener
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
         var parser = new ToastStuntMooParser(commonTokenStream);
         if (ParserErrorListener != null)
         {
            ParserErrorListener.Errors.Clear();
            parser.RemoveErrorListeners();
            parser.AddErrorListener(ParserErrorListener);
         }

         // Let's parse!
         ToastStuntMooParser.CodeContext context = parser.code();

         // Publish our errors
         ParseErrors.Clear();
         if (LexerErrorListener?.Errors != null)
            ParseErrors.AddRange(LexerErrorListener.Errors);
         if (ParserErrorListener?.Errors != null)
            ParseErrors.AddRange(ParserErrorListener.Errors);

         // Configure code folding markers
         ConfigureCodeFolding(context, ToastStuntMooParser.ruleNames);

         // Raise our event
         OnParsingComplete(this, new ParsingCompleteEventArgs(ParseErrors, Tokens, context));
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
               case "forStatement":
               case "forkStatement":
               case "whileStatement":
               case "tryStatement":
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