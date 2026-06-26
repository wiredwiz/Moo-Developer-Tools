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

using System.ComponentModel;
using System.Diagnostics;
using System.Security.Authentication.ExtendedProtection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
        : this(grammarDialect, null)
      { }

      /// <summary>
      /// Initializes a new instance of the <see cref="MooCodeEditor"/> class bound to an alternate
      /// settings source for syntax highlighting.
      /// </summary>
      /// <param name="grammarDialect">The grammar dialect.</param>
      /// <param name="themeSource">
      /// The settings source the syntax highlighting guide reads colors and styles from. When
      /// <see langword="null"/>, the singleton <see cref="Settings.Instance"/> is used.
      /// </param>
      public MooCodeEditor(GrammarDialect grammarDialect, Settings themeSource)
      {
         InitializeComponent();
         Tokens = new List<DetailedToken>();
         ParseErrors = new List<ParseMessage>();
         LexerErrorListener = new LexerErrorListener();
         ParserErrorListener = new ParserErrorListener();
         SyntaxHighlightingGuide = new MooSyntaxHighlightingGuide(themeSource);
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
         SmartBracketSuppressWhenUnbalanced = true;
         AutoIndentChars = false;
         WordWrapAutoIndent = true;
         FoldingHighlightEnabled = true;
         LineInterval = 4;
         AutoIndentNeeded += MooEditor_AutoIndentNeeded;
         TextChanged += MooEditor_TextChanged;
         TextChangedDelayed += MooEditor_TextChangedDelayed;
         KeyDown += MooEditor_KeyDown;
         UndoRedoStateChanged += MooEditor_UndoRedoStateChanged;
         // TEMP (member-hover tooltip plumbing proof): capture full Moo identifiers/refs under the
         // mouse and show the hovered token in a tooltip above the cursor. To be replaced by the
         // verb-documentation / property-value lookup once the hover path is verified end to end.
         HoverWordPattern = "[A-Za-z0-9_$#]";
         ToolTipNeeded += MooEditor_ToolTipNeeded;
         GrammarDialect = grammarDialect;
      }

      // The grammatical role of a hovered identifier, inferred from the surrounding tokens.
      private enum HoverMemberKind { None, Verb, Property, Function, Core, Object, Constant }

      /// <summary>The hover-content kind passed to the fetcher: a verb, a property, or a bare operand
      /// (this/player/caller/#N) resolved directly to the object it denotes.</summary>
      public enum MooHoverMemberKind { Verb, Property, Object }

      /// <summary>A request for hover content: the member kind, the member name, the operand chain to
      /// resolve to an object, and a flow resolver for any this/player/caller/local base in that chain.</summary>
      public sealed record MooHoverRequest(
         MooHoverMemberKind Kind,
         string Member,
         ChainDescriptor Chain,
         Func<string, FlowValue> VariableResolver);

      /// <summary>
      /// Supplies the real hover content body for a known verb/property reference. Set by the owning
      /// page (which has the world query provider). Returns <c>null</c> for no tooltip.
      /// </summary>
      public Func<MooHoverRequest, CancellationToken, Task<string>> HoverContentFetcher { get; set; }

      /// <summary>
      /// Supplies the live world value for a hovered literal constant. Set by the owning page (which has the
      /// world query provider). Given the constant name and whether it is a type constant (<c>true</c> →
      /// raw <c>typeof</c> value; <c>false</c> → <c>tostr()</c> message), returns the live value or
      /// <c>null</c> when unavailable/unsupported (the hover then falls back to <see cref="BuiltinConstantDocs"/>).
      /// </summary>
      public Func<string, bool, CancellationToken, Task<string>> HoverConstantValueFetcher { get; set; }

      /// <summary>
      /// Optional sink for unexpected (logged) hover chain-extraction failures. Set by the owning page,
      /// which routes it to its logger. Expected non-resolution passes silently.
      /// </summary>
      public Action<string, Exception?> HoverDiagnostic { get; set; }

      private CancellationTokenSource _hoverContentCts;

      // Hover-tooltip handler: classify (sync), then for a known verb/property fetch the real content
      // asynchronously and show it above the token once it lands. A fresh hover supersedes any
      // in-flight fetch. function / (no discernable source) / non-members are handled synchronously.
      private async void MooEditor_ToolTipNeeded(object sender, ToolTipNeededEventArgs e)
      {
         var (kind, member, chain, variableResolver) = ClassifyHoveredMember(e.Place);
         if (kind == HoverMemberKind.None)
            return;

         var caption = HoverCaption(kind, member, chain);

         // Show via ShowToolTipAbove (bottom-anchored, height-aware) — the SAME path verbs/properties
         // use. Setting e.ToolTipText instead would let FCTB's OnToolTip position it top-anchored,
         // which overlaps the cursor for anything taller than ~2 lines.
         if (kind == HoverMemberKind.Function)
         {
            ShowToolTipAbove(e.Place, caption, BuiltinFunctionDocs.GetTooltipText(member) ?? "(built-in function)");
            return;
         }
         if (kind == HoverMemberKind.Constant)
         {
            await ShowConstantToolTipAsync(e.Place, member, caption);
            return;
         }
         if (chain is null)
         {
            ShowToolTipAbove(e.Place, caption, "(unknown source)");
            return;
         }

         var fetcher = HoverContentFetcher;
         if (fetcher == null)
            return;

         _hoverContentCts?.Cancel();
         var cts = _hoverContentCts = new CancellationTokenSource();
         var place = e.Place;
         var requestKind = kind switch
         {
            HoverMemberKind.Verb => MooHoverMemberKind.Verb,
            HoverMemberKind.Object => MooHoverMemberKind.Object,
            _ => MooHoverMemberKind.Property, // Property and Core both fetch a property value
         };
         var request = new MooHoverRequest(requestKind, member, chain, variableResolver ?? DefaultFlowResolver);
         try
         {
            var body = await fetcher(request, cts.Token);
            if (!cts.IsCancellationRequested && !string.IsNullOrEmpty(body))
               ShowToolTipAbove(place, caption, body);
         }
         catch (OperationCanceledException) { }
         catch { }
      }

      // Resolves and shows a literal-constant hover: attempt a live world value (type → raw value, error →
      // tostr()) then fall back to the baked-in constant table. Booleans use the table directly. A fresh
      // hover supersedes any in-flight fetch.
      private async Task ShowConstantToolTipAsync(Place place, string name, string caption)
      {
         var doc = BuiltinConstantDocs.Get(name);
         if (doc is null)
            return;

         string live = null;
         var fetcher = HoverConstantValueFetcher;
         if (fetcher != null && doc.Kind != "bool")
         {
            _hoverContentCts?.Cancel();
            var cts = _hoverContentCts = new CancellationTokenSource();
            try
            {
               live = await fetcher(name, doc.Kind == "type", cts.Token);
            }
            catch (OperationCanceledException) { }
            catch { live = null; }

            if (cts.IsCancellationRequested)
               return;
         }

         var display = ConstantHoverResolver.ResolveConstantDisplay(doc.Kind, live, doc.Display);
         if (string.IsNullOrEmpty(display))
            return;

         ShowToolTipAbove(place, caption, $"{name} => {display}");
      }

      // The default flow resolver used when a hover request carries no tree-bound resolver: keywords use
      // their default, locals are unknown.
      private static FlowValue DefaultFlowResolver(string name) =>
         new(name is "this" or "player" or "caller" ? FlowValueKind.UseDefault : FlowValueKind.Unknown, null);

      private (HoverMemberKind Kind, string Member, ChainDescriptor Chain, Func<string, FlowValue> VariableResolver)
         ClassifyHoveredMember(Place place)
      {
         var token = FindSurroundingTokenForPosition(place.iLine + 1, place.iChar + 1);
         if (token?.TypeName == null)
            return (HoverMemberKind.None, null, null, null);

         // A standalone core reference ($name) is the property `name` on #0.
         if (token.TypeNameUpperCase == "CORE_REFERENCE")
         {
            var coreName = token.Text.Length > 1 ? token.Text.Substring(1) : token.Text;
            var coreChain = new ChainDescriptor(
               new ChainBase(ChainBaseKind.ObjectLiteral, "0"),
               System.Array.Empty<string>(), MemberContextKind.Property, coreName);
            return (HoverMemberKind.Core, coreName, coreChain, DefaultFlowResolver);
         }

         var previous = PreviousSignificantToken(Tokens.IndexOf(token));
         var precededBySeparator = previous != null && (previous.Text == ":" || previous.Text == ".");

         // A member on a chain: an IDENTIFIER after ':'/'.'.
         if (token.TypeNameUpperCase == "IDENTIFIER" && precededBySeparator)
         {
            var contextKind = previous.Text == ":" ? MemberContextKind.Verb : MemberContextKind.Property;
            var hoverKind = previous.Text == ":" ? HoverMemberKind.Verb : HoverMemberKind.Property;
            // Capture the full chain to the separator's left (same resolution completion uses), so a member
            // on a multi-step operand ($Mcp.package:foo) resolves, not just single atoms. The flow resolver
            // is bound to the live tree and the hovered member's offset for keyword/local bases.
            var tree = ParseSourceCode(false);
            var chain = ChainExtractor.ExtractAtSeparator(
               previous.StartIndex, contextKind, token.Text, Tokens, tree, HoverDiagnostic);
            if (chain is null)
               return (hoverKind, token.Text, null, null);
            return (hoverKind, token.Text, chain,
                    name => FlowValueResolver.ResolveCurrentValue(name, tree, token.StartIndex, HoverDiagnostic));
         }

         // A bare resolvable operand used directly (this/player/caller/#N), not as a member: show the object
         // it resolves to. Reassignment-aware so a reassigned this/player/caller shows its new object.
         if (!precededBySeparator)
         {
            var bareBase = ChainExtractor.ClassifyBareResolvableOperand(token.TypeNameUpperCase, token.Text);
            if (bareBase is not null)
            {
               var tree = ParseSourceCode(false);
               var objectChain = new ChainDescriptor(
                  bareBase.Value, System.Array.Empty<string>(), MemberContextKind.Property, token.Text);
               return (HoverMemberKind.Object, token.Text, objectChain,
                       name => FlowValueResolver.ResolveCurrentValue(name, tree, token.StartIndex, HoverDiagnostic));
            }
         }

         // A builtin function: an IDENTIFIER immediately followed by '(' (keeps priority over a bare local).
         if (token.TypeNameUpperCase == "IDENTIFIER")
         {
            var next = NextSignificantToken(Tokens.IndexOf(token));
            if (next != null && next.Text == "(")
               return (HoverMemberKind.Function, token.Text, null, null);
         }

         // A literal constant (type name, error code, or true/false). Type names lex as IDENTIFIER, error
         // codes as ERROR, and the booleans as BOOLEAN, so all three token kinds are accepted. Placed after
         // the builtin-function check and before the bare-local fallback so a constant name takes priority
         // over being treated as a local. The content is resolved from the constant table (with an optional
         // live world value), so no chain/resolver is needed.
         if ((token.TypeNameUpperCase == "IDENTIFIER"
              || token.TypeNameUpperCase == "ERROR"
              || token.TypeNameUpperCase == "BOOLEAN")
             && Moo.IsConstant(token.Text))
            return (HoverMemberKind.Constant, token.Text, null, null);

         // A bare local variable used directly (not a member, not this/player/caller, not a builtin call):
         // show the object it currently resolves to via the flow resolver. A local that does not resolve to
         // an object yields no tooltip (handled downstream: the flow value is Unknown -> chain resolves null).
         if (token.TypeNameUpperCase == "IDENTIFIER" && !precededBySeparator)
         {
            var tree = ParseSourceCode(false);
            var localChain = new ChainDescriptor(
               new ChainBase(ChainBaseKind.Variable, token.Text),
               System.Array.Empty<string>(), MemberContextKind.Property, token.Text);
            return (HoverMemberKind.Object, token.Text, localChain,
                    name => FlowValueResolver.ResolveCurrentValue(name, tree, token.StartIndex, HoverDiagnostic));
         }

         return (HoverMemberKind.None, token.Text, null, null);
      }

      private static string HoverCaption(HoverMemberKind kind, string member, ChainDescriptor chain)
      {
         var operand = chain is null ? string.Empty : ChainOperandDisplay(chain);
         return kind switch
         {
            HoverMemberKind.Verb => operand.Length == 0 ? $"Verb {member}()" : $"Verb {operand}:{member}()",
            HoverMemberKind.Property => operand.Length == 0 ? $"Property {member}" : $"Property {operand}.{member}",
            HoverMemberKind.Function => $"Function {member}()",
            HoverMemberKind.Core => $"Core ${member}",
            HoverMemberKind.Object => $"Object {member}",
            HoverMemberKind.Constant => $"Constant {member}",
            _ => member,
         };
      }

      // Renders a chain's base + property steps as display text for the hover caption (e.g. "$Mcp.package").
      private static string ChainOperandDisplay(ChainDescriptor chain)
      {
         var basePart = chain.Base.Kind switch
         {
            ChainBaseKind.ObjectLiteral => $"#{chain.Base.Text}",
            ChainBaseKind.CoreName => $"${chain.Base.Text}",
            ChainBaseKind.This => "this",
            ChainBaseKind.Player => "player",
            ChainBaseKind.Caller => "caller",
            _ => chain.Base.Text,
         };
         return chain.Steps.Count > 0 ? $"{basePart}.{string.Join(".", chain.Steps)}" : basePart;
      }

      private DetailedToken PreviousSignificantToken(int index)
      {
         for (var i = index - 1; i >= 0; i--)
            if (IsSignificantToken(Tokens[i]))
               return Tokens[i];
         return null;
      }

      private DetailedToken NextSignificantToken(int index)
      {
         for (var i = index + 1; i < Tokens.Count; i++)
            if (IsSignificantToken(Tokens[i]))
               return Tokens[i];
         return null;
      }

      // Significant = default channel, not whitespace, not EOF (skips hidden whitespace tokens).
      private static bool IsSignificantToken(DetailedToken token)
         => token.Channel == 0 && token.Text != "<EOF>" && !string.IsNullOrWhiteSpace(token.Text);

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
            // Whitespace-tolerant, escape-aware auto-delete: backspacing an opener (or unescaped opening
            // quote) also removes the matching closer when it is the first non-whitespace char ahead on the
            // line, collapsing the intervening whitespace too.
            var lineText = GetLineText(Selection.Start.iLine);
            var caret = Selection.Start.iChar;
            var before = Selection.CharBeforeStart;
            int closerIndex = before switch
            {
               '(' => SmartBracketDeletion.FindMatchingCloserAhead(lineText, caret, '(', ')'),
               '{' => SmartBracketDeletion.FindMatchingCloserAhead(lineText, caret, '{', '}'),
               '[' => SmartBracketDeletion.FindMatchingCloserAhead(lineText, caret, '[', ']'),
               '"' when SmartBracketDeletion.IsUnescapedQuoteDelimiter(lineText, caret - 1) =>
                  SmartBracketDeletion.FindMatchingCloserAhead(lineText, caret, '"', '"'),
               _ => -1
            };

            if (closerIndex >= 0)
               Selection = new TextSelectionRange(this,
                  new Place(caret - 1, Selection.Start.iLine),
                  new Place(closerIndex + 1, Selection.Start.iLine));
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

      /// <summary>
      /// Re-applies the current theme to the editor by clearing cached styles and re-colorizing.
      /// </summary>
      /// <remarks>
      /// Used after the active color theme changes. Clears the <see cref="StyleRegistry"/> cache and
      /// existing styles, then re-creates the code-folding markers and re-colorizes the tokens
      /// (guarding against empty text).
      /// <para>
      /// Re-creating the folding markers is required because <see cref="ClearAllStyles"/> deletes every
      /// line's <c>FoldingStartMarker</c>/<c>FoldingEndMarker</c> (see <see cref="Line.ClearAllStyles"/>).
      /// Without re-running <see cref="ConfigureCodeFolding"/>, a theme refresh would leave
      /// <see cref="ShowCodeFolding"/> enabled but with no markers, making all fold indicators vanish.
      /// </para>
      /// </remarks>
      public void RefreshTheme()
      {
         StyleRegistry.Clear();
         var context = ParseSourceCode();
         ClearAllStyles();
         if (!string.IsNullOrEmpty(Text))
         {
            // ClearAllStyles() also removes the per-line folding markers, so restore them.
            // ConfigureCodeFolding is a no-op when ShowCodeFolding is disabled.
            ConfigureCodeFolding(context);
            ColorizeTokens(null);
         }

         Invalidate();
      }

      /// <summary>
      /// Collapses the selection to an empty caret at the very start of the document.
      /// </summary>
      /// <remarks>
      /// The inherited <c>SelectionStart</c>/<c>SelectionLength</c> shims only move the selection
      /// start (and ignore a zero length), so they cannot deselect a full-document selection. This
      /// sets both ends of the selection explicitly to the home position.
      /// </remarks>
      public void CollapseSelectionToStart()
      {
         Selection = new TextSelectionRange(this, new Place(0, 0), new Place(0, 0));
         Invalidate();
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
         UpdateDiagnostics(parsingCompleteEventArgs.ErrorMessages);
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