#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="MooCodeEditorPage.cs">
// Copyright (c) Thaddeus Ryker 2022
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

using Org.Edgerunner.ANTLR4.Tools.Common;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar.Errors;
using Org.Edgerunner.Moo.Editor;
using Org.Edgerunner.Moo.Editor.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using FastColoredTextBoxNS;
using FastColoredTextBoxNS.Types;
using Krypton.Toolkit;
using NLog;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Org.Edgerunner.Moo.Editor.Configuration;
using Org.Edgerunner.Mud.Common.Querying;

namespace Org.Edgerunner.Moo.Udditor.Pages;

public class MooCodeEditorPage : MooEditorPage
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    /// <summary>
    /// Gets or sets the MOO world query provider used to power code-completion popups and tooltips.
    /// </summary>
    /// <value>
    /// The query provider, or <c>null</c> when no connection/provider is associated.
    /// </value>
    /// <remarks>
    /// This is a defensive, nullable hook; consuming code must null-check it. It will be wired to the
    /// owning connection's <c>session.QueryProviders.Query</c> in udd-am1.
    /// </remarks>
    public IMooWorldQueryProvider? QueryProvider { get; set; }

    /// <summary>
    /// Gets or sets the object the edited verb lives on (the meaning of <c>this</c> in the
    /// edited code), parsed from the local-edit upload command or simpleedit reference.
    /// </summary>
    /// <value>
    /// The context object id, or <c>null</c> for file-based or otherwise unattributed edits.
    /// </value>
    public MooObjectId? ContextObjectId { get; set; }

    private MemberCompletionController? _memberCompletionController;

    /// <summary>
    /// Initializes a new instance of the <see cref="MooCodeEditorPage"/> class.
    /// </summary>
    /// <param name="manager">The window manager.</param>
    /// <param name="verbName">Name of the verb.</param>
    /// <param name="worldName">Name of the world.</param>
    /// <param name="dialect">The dialect.</param>
    /// <param name="source">The source.</param>
    // ReSharper disable once TooManyDependencies
    public MooCodeEditorPage(WindowManager manager, string verbName, string worldName, GrammarDialect dialect, string source)
    : base(manager)
    {
        var name = $@"{verbName} - {worldName}";
        var key = $"{name}-{Guid.NewGuid()}";
        InitializeEditor(dialect, key, verbName, name);
        Editor.Document = new DocumentInfo(key, string.Empty, verbName);
        Editor.Text = source;
        PostInitialize();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MooCodeEditorPage"/> class.
    /// </summary>
    /// <param name="manager">The window manager.</param>
    /// <param name="dialect">The dialect.</param>
    /// <param name="filePath">The file path.</param>
    public MooCodeEditorPage(WindowManager manager, GrammarDialect dialect, string filePath)
    : base(manager)
    {
        var name = Path.GetFileName(filePath);
        var key = $"{name}-{Guid.NewGuid()}";
        InitializeEditor(dialect, key, name, filePath);
        //ToolTipTitle = filePath;
        Editor.Document = new DocumentInfo(key, filePath, name);
        Editor.OpenFile(filePath);
        PostInitialize();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MooCodeEditorPage"/> class.
    /// </summary>
    /// <param name="manager">The window manager.</param>
    /// <param name="dialect">The dialect.</param>
    public MooCodeEditorPage(WindowManager manager, GrammarDialect dialect)
        : base(manager)
    {
        var name = "<New>";
        var key = $"{name}-{Guid.NewGuid()}";
        InitializeEditor(dialect, key, name, name);
        Editor.Document = new DocumentInfo(key, string.Empty, name);
        PostInitialize();
    }

    /// <summary>
    /// Initializes a new editor control instance.
    /// </summary>
    /// <param name="dialect">The dialect.</param>
    /// <param name="id">The unique identifier for the page.</param>
    /// <param name="title">The page title.</param>
    /// <param name="description">The page description.</param>
    private void InitializeEditor(GrammarDialect dialect, string id, string title, string description)
    {
        Editor = new MooCodeEditor();
        Editor.LineInterval = 4;
        Editor.BorderStyle = BorderStyle.Fixed3D;
        Editor.Dock = DockStyle.Fill;
        Controls.Add(Editor);
        ConfigureEditorSettings(Editor);
        Editor.GrammarDialect = dialect;
        UniqueName = id;
        Text = title;
        TextTitle = title;
        TextDescription = description;
        ToolTipBody = description;
        ToolTipStyle = LabelStyle.ToolTip;
    }

    private void PostInitialize()
    {
        Editor.IsChanged = false;
        Editor.ClearUndo();
        Editor.SelectionChangedDelayed += Editor_SelectionChangedDelayed;
        Editor.Selection = new TextSelectionRange(SourceEditor, 0, 0, 0, 0);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _memberCompletionController?.Dispose();
        base.Dispose(disposing);
    }

    /// <summary>
    /// Occurs when [parsing complete].
    /// </summary>
    public event EventHandler<ParsingCompleteEventArgs> ParsingComplete
    {
        add { Editor.ParsingComplete += value; }
        remove { Editor.ParsingComplete -= value; }
    }

    public override FastColoredTextBox SourceEditor
    {
        get => Editor;
    }

    /// <summary>
    /// Gets the current line number.
    /// </summary>
    /// <value>The current line number.</value>
    public override int CurrentLineNumber => Editor.Selection.Start.iLine;

    /// <summary>
    /// Gets the current column position.
    /// </summary>
    /// <value>The current column position.</value>
    public override int CurrentColumnPosition => Editor.Selection.Start.iChar;

    /// <summary>
    /// Gets or sets the document.
    /// </summary>
    /// <value>The document.</value>
    public override DocumentInfo Document
    {
        get => Editor.Document;
        set => Editor.Document = value;
    }

    /// <summary>
    /// Gets or sets the editor.
    /// </summary>
    /// <value>The editor.</value>
    public MooCodeEditor Editor { get; set; }

    /// <summary>
    /// Gets the parse errors.
    /// </summary>
    /// <value>The parse errors.</value>
    public List<ParseMessage> ParseErrors => Editor.ParseErrors;

    /// <summary>
    /// Gets or sets the grammar dialect.
    /// </summary>
    /// <value>The grammar dialect.</value>
    public GrammarDialect GrammarDialect
    {
        get => Editor.GrammarDialect;
        set => Editor.GrammarDialect = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether to display code folding.
    /// </summary>
    /// <value>
    ///   <c>true</c> if [display code folding]; otherwise, <c>false</c>.
    /// </value>
    public bool DisplayCodeFolding
    {
        get => Editor.ShowCodeFolding;
        set => Editor.ShowCodeFolding = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show text block indentation guides.
    /// </summary>
    /// <value>
    ///   <c>true</c> if [show text block indentation guides]; otherwise, <c>false</c>.
    /// </value>
    public bool ShowTextBlockIndentationGuides
    {
        get => Editor.ShowTextBlockIndentationGuides;
        set => Editor.ShowTextBlockIndentationGuides = value;
    }

    private void ConfigureEditorSettings(MooCodeEditor codeEditor)
    {
        ApplyEditorSettings(codeEditor, Settings.Instance);
    }

    /// <summary>
    /// Applies the supplied <paramref name="source"/> settings (chrome colors, fonts and behavior)
    /// to the supplied editor.
    /// </summary>
    /// <param name="codeEditor">The editor to configure.</param>
    /// <param name="source">The settings source. When <see langword="null"/>, <see cref="Settings.Instance"/> is used.</param>
    public void ApplyEditorSettings(MooCodeEditor codeEditor, Settings source = null)
    {
        source ??= Settings.Instance;
        codeEditor.Font = new Font(source.EditorFontFamily, source.EditorFontSize);
        codeEditor.ForeColor = source.EditorTextColor;
        codeEditor.CaretColor = source.EditorCaretColor;
        codeEditor.BackColor = source.EditorBackgroundColor;
        codeEditor.CurrentLineColor = source.EditorCurrentLineColor;
        codeEditor.AutoIndent = source.EditorAutoIndent;
        codeEditor.WordWrapIndent = source.EditorWordWrapIndent;
        codeEditor.WordWrapAutoIndent = source.EditorWordWrapAutoIndent;
        codeEditor.WordWrap = source.EditorWordWrap;
        codeEditor.AutoCompleteBrackets = source.EditorAutoBrackets;
        codeEditor.TabLength = source.EditorTabLength;
        codeEditor.LineNumberColor = source.EditorLineNumberColor;
        codeEditor.SelectionColor = source.EditorTextSelectionColor;
        codeEditor.ChangedLineColor = source.EditorChangedLineColor;
        codeEditor.FoldingIndicatorColor = source.EditorFoldingIndicatorColor;
        codeEditor.IndentBackColor = source.EditorIndentBackColor;
        codeEditor.BookmarkColor = source.EditorBookmarkColor;
        codeEditor.ServiceLinesColor = source.EditorServiceLineColor;
        codeEditor.ShowCodeFolding = source.EditorShowCodeFolding;
        codeEditor.ShowTextBlockIndentationGuides = source.EditorShowTextIndentGuides;
        codeEditor.Zoom = source.EditorZoomFactor;
        codeEditor.FoldingHighlightColor = source.EditorFoldingHighlightColor;
        codeEditor.FoldingHighlightEnabled = source.EditorShowFoldingBlockHighlights;
        BuildAutocompleteMenu(codeEditor);
    }

    private void BuildAutocompleteMenu(MooCodeEditor codeEditor)
    {
        codeEditor.AutocompleteMenu = new AutocompleteMenu(codeEditor);

        codeEditor.AutocompleteMenu.ImageList = CompletionIconFactory.CreateImageList();
        // $ and # are fragment characters so member contexts ($foo, #123:verb) reach the items.
        codeEditor.AutocompleteMenu.SearchPattern = @"[\w\.:=!<>+-/*%&|^$#]";
        codeEditor.AutocompleteMenu.AllowTabKey = true;
        codeEditor.AutocompleteMenu.MinFragmentLength = 1;

        List<AutocompleteItem> items = new List<AutocompleteItem>();

        foreach (var item in Snippets.LoadSnippets(ApplicationPaths.ResolveDataFile("Snippets.txt")))
            items.Add(new SnippetAutocompleteItem(item) { ImageIndex = (int)CompletionIconCategory.Snippet });
        foreach (var item in Moo.Editor.Moo.Keywords)
            items.Add(new AutoIndentingSnippet(item) { ImageIndex = (int)Moo.Editor.Moo.ClassifyKeyword(item) });
        foreach (var builtin in Moo.Editor.Moo.Builtins.Values)
            items.Add(new SnippetAutocompleteItem(builtin) { ImageIndex = (int)CompletionIconCategory.Function });

        // display the completion items alphabetically by their menu text
        items.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.CurrentCultureIgnoreCase));

        items.Add(new InsertSpaceSnippet());
        items.Add(new InsertSpaceSnippet(@"^(\w+)([=<>!&|%-+*/]+)(\w+)$"));
        items.Add(new FormatCommaSnippet(@"^(\w+)(([,]+)(\w+))+$"));

        // member completion: world-queried verbs/properties/core references injected ahead of
        // the static items whenever the caret sits in a member context on a connected world.
        _memberCompletionController?.Dispose();
        _memberCompletionController = new MemberCompletionController(
            () => QueryProvider,
            () => ContextObjectId,
            action =>
            {
                if (!codeEditor.IsDisposed && codeEditor.IsHandleCreated)
                    codeEditor.BeginInvoke(action);
            },
            () =>
            {
                if (codeEditor.IsDisposed)
                    return;

                var menu = codeEditor.AutocompleteMenu;
                if (menu is { Visible: true })
                {
                    // Results arrived while the popup was already open — refresh its item list.
                    menu.Show(false);
                }
                else if (codeEditor.Focused && _memberCompletionController is not null)
                {
                    // Results arrived after the popup closed (it closed because the first
                    // DoAutocomplete call found an empty list).  Re-open it now that items are
                    // available; Show(false) runs the menu's own fragment matching so it stays
                    // closed if nothing matches the current fragment. Thread the SAME full context
                    // (tree/tokens/caret) the enumeration uses, so multi-step chains ($Mcp.package:)
                    // pass the guard — the line-prefix-only overload resolves single atoms only.
                    var context = GetMemberCompletionContext(codeEditor);
                    if (_memberCompletionController.GetMemberItems(
                            GetCaretLinePrefix(codeEditor), context.Tree, context.Tokens,
                            context.CaretLine, context.CaretColumn).Count > 0)
                    {
                        menu?.Show(false);
                    }
                }
            },
            diagnostic: (message, ex) => Logger.Warn(ex, message));

        //set as autocomplete source. The context provider threads the live parse tree, tokens, and
        // caret position so chained member-access expressions ($Mcp.package:) resolve, not just atoms.
        codeEditor.AutocompleteMenu.Items.SetAutocompleteItems(
            new DynamicCompletionSource(
                items,
                _memberCompletionController,
                () => GetCaretLinePrefix(codeEditor),
                () => GetMemberCompletionContext(codeEditor)));
        codeEditor.AutocompleteMenu.AppearInterval = Settings.Instance.EditorAutocompleteDelay;
        codeEditor.HoverContentFetcher = FetchHoverContentAsync;
        codeEditor.HoverDiagnostic = (message, ex) => Logger.Warn(ex, message);
    }

    private async Task<string> FetchHoverContentAsync(
        MooCodeEditor.MooHoverRequest request, CancellationToken cancellationToken)
    {
        var provider = QueryProvider;
        if (provider == null)
            return null;

        var objectId = await ResolveHoverChainAsync(request.Chain, request.VariableResolver, provider, cancellationToken);
        if (objectId == null)
            return null;

        if (request.Kind == MooCodeEditor.MooHoverMemberKind.Verb)
        {
            var doc = await provider.GetVerbDocumentationAsync(objectId.Value, request.Member, cancellationToken);
            if (doc == null)
                return null;
            return doc.Lines.Count > 0 ? string.Join(Environment.NewLine, doc.Lines) : "(no documentation)";
        }

        var value = await provider.GetPropertyValueAsync(objectId.Value, request.Member, cancellationToken);
        if (value == null)
            return null;
        var literal = value.Literal ?? string.Empty;
        if (literal.Length > 200)
            literal = literal.Substring(0, 200) + "…";
        return $"=> {literal}";
    }

    // Resolves a hover operand CHAIN to its final object id through the same evaluator the completion path
    // uses, so hovering $Mcp.package:foo resolves $Mcp -> #221 then .package -> #215 exactly as completing
    // $Mcp.package: does. The supplied variable resolver (bound to the live parse tree and the hovered
    // position) resolves any local-variable chain base, matching completion. Returns null when the chain
    // does not resolve. Unexpected failures (a provider throw) are logged via Logger.Warn; ordinary
    // non-resolution returns null without a log entry.
    private async Task<MooObjectId?> ResolveHoverChainAsync(
        ChainDescriptor chain,
        Func<string, ChainDescriptor?> variableResolver,
        IMooWorldQueryProvider provider,
        CancellationToken cancellationToken)
    {
        if (chain is null)
            return null;

        var evaluator = new ChainExpressionEvaluator(
            (obj, prop, ct) => ResolveHoverPropertyObjectAsync(provider, obj, prop, ct),
            provider.GetCurrentPlayerAsync,
            variableResolver ?? (_ => null),
            (message, ex) => Logger.Warn(ex, message));

        return await evaluator.EvaluateAsync(chain, ContextObjectId, cancellationToken);
    }

    // Resolves (objectId, propName) to the object the property holds (the evaluator's per-step primitive).
    private static async Task<MooObjectId?> ResolveHoverPropertyObjectAsync(
        IMooWorldQueryProvider provider, MooObjectId objectId, string propName, CancellationToken cancellationToken)
    {
        var value = await provider.GetPropertyValueAsync(objectId, propName, cancellationToken);
        if (value != null && value.Type == 1 && !string.IsNullOrEmpty(value.Literal) && value.Literal[0] == '#'
            && int.TryParse(value.Literal.AsSpan(1), out var number) && number >= 0)
            return new MooObjectId(number);
        return null;
    }

    // Snapshots the live parse tree, tokens, and caret position for chained member completion.
    private static MemberCompletionContextSnapshot GetMemberCompletionContext(MooCodeEditor codeEditor)
    {
        var place = codeEditor.Selection.Start;
        // Convert the editor's 0-based (iLine, iChar) to the 1-based caret convention the extractor uses.
        return new MemberCompletionContextSnapshot(
            codeEditor.ParseSourceCode(false),
            codeEditor.Tokens,
            place.iLine + 1,
            place.iChar + 1);
    }

    private static string GetCaretLinePrefix(MooCodeEditor codeEditor)
    {
        var place = codeEditor.Selection.Start;
        if (place.iLine < 0 || place.iLine >= codeEditor.LinesCount || place.iChar <= 0)
            return string.Empty;

        var line = codeEditor.GetLineText(place.iLine);
        return line.Substring(0, Math.Min(place.iChar, line.Length));
    }

    /// <summary>
    /// Re-applies the current <see cref="Settings.Instance"/> theme (chrome colors and syntax
    /// highlighting) to this page's editor.
    /// </summary>
    public void ApplyTheme()
    {
        ApplyEditorSettings(Editor, Settings.Instance);
        Editor.RefreshTheme();
    }

    /// <summary>
    /// Parses the editor source code.
    /// </summary>
    public void ParseSourceCode()
    {
        Editor.ParseSourceCode();
    }

    private void Editor_SelectionChangedDelayed(object sender, EventArgs e)
    {
        OnCursorPositionChanged();
    }
}
