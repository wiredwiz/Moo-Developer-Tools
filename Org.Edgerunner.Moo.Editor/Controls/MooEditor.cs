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
using Org.Edgerunner.Moo.Editor.SyntaxHighlighting;
using Org.Edgerunner.MooSharp.Language.Grammar;
using Place = FastColoredTextBoxNS.Types.Place;

namespace Org.Edgerunner.Moo.Editor.Controls
{
   public partial class MooEditor : FastColoredTextBox
   {
      AutocompleteMenu popupMenu;
      string[] keywords = { "if", "elseif", "else", "endif", "return", "while", "endwhile", "for", "endfor", "fork", "endfork", "try", "except", "finally", "endtry",
         "in", "player", "caller", "verb", "dobj", "dobjstr", "prepstr", "iobj", "iobjstr", "this", "args", "argstr", "E_NONE", "E_TYPE", "E_DIV", "E_PERM", "E_PROPNF",
         "E_VERBNF", "E_VARNF", "E_INVIND", "E_RECMOVE", "E_MAXREC", "E_RANGE", "E_ARGS", "E_NACC", "E_INVARG", "E_QUOTA", "E_FLOAT", "E_FILE", "E_EXEC", "E_INTRPT",
         "STR", "LIST", "OBJ", "MAP", "INT", "FLOAT", "ERR", "BOOL", "WAIF", "ANON", "NUM", "true", "false"
      };
      //string[] methods = { "Equals()", "GetHashCode()", "GetType()", "ToString()"};
      string[] snippets =
      {
         "if (^)\nendif", "if (^)\nelse\nendif", "for x in (^)\nendfor", "while (^)\nendwhile", "fork (^)\nendfork;", "try\n^\nexcept ex (ANY)\nendtry"
      };
      //string[] declarationSnippets = {
      //   "public class ^\n{\n}", "private class ^\n{\n}", "internal class ^\n{\n}",
      //   "public struct ^\n{\n;\n}", "private struct ^\n{\n;\n}", "internal struct ^\n{\n;\n}",
      //   "public void ^()\n{\n;\n}", "private void ^()\n{\n;\n}", "internal void ^()\n{\n;\n}", "protected void ^()\n{\n;\n}",
      //   "public ^{ get; set; }", "private ^{ get; set; }", "internal ^{ get; set; }", "protected ^{ get; set; }"
      //};
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
         AutoCompleteBrackets = true;
         AutoCompleteBracketsList = new[] { '(', ')', '{', '}', '[', ']', '"', '"' };
         WordWrap = true;
         AutoIndentChars = false;
         WordWrapAutoIndent = true;
         WordWrapIndent = 2;
         TabLength = 2;
         AutoIndentNeeded += MooEditor_AutoIndentNeeded;
         TextChangedDelayed += MooEditor_TextChangedDelayed;
         KeyDown += MooEditor_KeyDown;

         //create autocomplete popup menu
         popupMenu = new AutocompleteMenu(this);
         //popupMenu.Items.ImageList = imageList1;
         popupMenu.SearchPattern = @"[\w\.:=!<>+-/*%&|^]";
         popupMenu.AllowTabKey = true;
         popupMenu.MinFragmentLength = 1;
         //
         BuildAutocompleteMenu();
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
      private void MooEditor_KeyDown(object sender, KeyEventArgs e)
      {
         if ((e.KeyCode == Keys.OemSemicolon && e.Modifiers == 0) &&
             ((Selection.Start.iLine == Selection.End.iLine) &&
              (Selection.Start.iChar == Selection.End.iChar)))

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
               {
                  e.Handled = true;
                  DoCaretVisible();
                  Invalidate();
               }
            }
         }
      }

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
      private void BuildAutocompleteMenu()
      {
         List<AutocompleteItem> items = new List<AutocompleteItem>();

         foreach (var item in snippets)
            items.Add(new SnippetAutocompleteItem(item) { ImageIndex = 1 });
         //foreach (var item in declarationSnippets)
         //   items.Add(new DeclarationSnippet(item) { ImageIndex = 0 });
         //foreach (var item in methods)
         //   items.Add(new MethodAutocompleteItem(item) { ImageIndex = 2 });
         foreach (var item in keywords)
            items.Add(new AutocompleteItem(item));
         foreach (var builtin in Moo.Builtins.Values)
            items.Add(new SnippetAutocompleteItem(builtin));

         items.Add(new InsertSpaceSnippet());
         items.Add(new InsertSpaceSnippet(@"^(\w+)([=<>!&|%-+*/]+)(\w+)$"));
         items.Add(new InsertEnterSnippet());

         //set as autocomplete source
         popupMenu.Items.SetAutocompleteItems(items);
      }

      /// <summary>
        /// This item appears when any part of snippet text is typed
        /// </summary>
        class DeclarationSnippet : SnippetAutocompleteItem
        {
            public DeclarationSnippet(string snippet)
                : base(snippet)
            {
            }

            public override CompareResult Compare(string fragmentText)
            {
                var pattern = Regex.Escape(fragmentText);
                if (Regex.IsMatch(Text, "\\b" + pattern, RegexOptions.IgnoreCase))
                    return CompareResult.Visible;
                return CompareResult.Hidden;
            }
        }

        /// <summary>
        /// Divides numbers and words: "123AND456" -> "123 AND 456"
        /// Or "i=2" -> "i = 2"
        /// </summary>
        class InsertSpaceSnippet : AutocompleteItem
        {
            string pattern;

            public InsertSpaceSnippet(string pattern):base("")
            {
                this.pattern = pattern;
            }

            public InsertSpaceSnippet()
                : this(@"^(\d+)([a-zA-Z_]+)(\d*)$")
            {
            }

            public override CompareResult Compare(string fragmentText)
            {
                if (Regex.IsMatch(fragmentText, pattern))
                {
                    Text = InsertSpaces(fragmentText);
                    if(Text != fragmentText)
                        return CompareResult.Visible;
                }
                return CompareResult.Hidden;
            }

            public string InsertSpaces(string fragment)
            {
                var m = Regex.Match(fragment, pattern);
                if (m == null)
                    return fragment;
                if (m.Groups[1].Value == "" && m.Groups[3].Value == "")
                    return fragment;
                return (m.Groups[1].Value + " " + m.Groups[2].Value + " " + m.Groups[3].Value).Trim();
            }

            public override string ToolTipTitle
            {
                get
                {
                    return Text;
                }
            }
        }

        /// <summary>
        /// Inerts line break after '}'
        /// </summary>
        class InsertEnterSnippet : AutocompleteItem
        {
           Place enterPlace = Place.Empty;

           public InsertEnterSnippet()
              : base("[Line break]")
           {
           }

           public override CompareResult Compare(string fragmentText)
           {
              var r = Parent.Fragment.Clone();
              while (r.Start.iChar > 0)
              {
                 if (r.CharBeforeStart == '}')
                 {
                    enterPlace = r.Start;
                    return CompareResult.Visible;
                 }

                 r.GoLeftThroughFolded();
              }

              return CompareResult.Hidden;
           }

           public override string GetTextForReplace()
           {
              //extend range
              TextSelectionRange r = Parent.Fragment;
              Place end = r.End;
              r.Start = enterPlace;
              r.End = r.End;
              //insert line break
              return Environment.NewLine + r.Text;
           }

           public override void OnSelected(AutocompleteMenu popupMenu, SelectedEventArgs e)
           {
              base.OnSelected(popupMenu, e);
              if (Parent.Fragment.tb.AutoIndent)
                 Parent.Fragment.tb.DoAutoIndent();
           }

           public override string ToolTipTitle
           {
              get { return "Insert line break after '}'"; }
           }
        }
   }
}