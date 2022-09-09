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
using FastColoredTextBoxNS;
using FastColoredTextBoxNS.Types;
using Krypton.Toolkit;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Org.Edgerunner.Moo.Editor.Configuration;

namespace Org.Edgerunner.Moo.Udditor.Pages;

public class MooCodeEditorPage : MooEditorPage
{
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
        Editor.Document = new DocumentInfo(key, key, verbName);
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
        Editor.Document = new DocumentInfo(key, name, name);
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
        codeEditor.Font = new Font(Settings.Instance.EditorFontFamily, Settings.Instance.EditorFontSize);
        codeEditor.ForeColor = Settings.Instance.EditorTextColor;
        codeEditor.CaretColor = Settings.Instance.EditorCaretColor;
        codeEditor.BackColor = Settings.Instance.EditorBackgroundColor;
        codeEditor.CurrentLineColor = Settings.Instance.EditorCurrentLineColor;
        codeEditor.AutoIndent = Settings.Instance.EditorAutoIndent;
        codeEditor.WordWrapIndent = Settings.Instance.EditorWordWrapIndent;
        codeEditor.WordWrapAutoIndent = Settings.Instance.EditorWordWrapAutoIndent;
        codeEditor.WordWrap = Settings.Instance.EditorWordWrap;
        codeEditor.AutoCompleteBrackets = Settings.Instance.EditorAutoBrackets;
        codeEditor.TabLength = Settings.Instance.EditorTabLength;
        codeEditor.LineNumberColor = Settings.Instance.EditorLineNumberColor;
        codeEditor.SelectionColor = Settings.Instance.EditorTextSelectionColor;
        codeEditor.ChangedLineColor = Settings.Instance.EditorChangedLineColor;
        codeEditor.FoldingIndicatorColor = Settings.Instance.EditorFoldingIndicatorColor;
        codeEditor.IndentBackColor = Settings.Instance.EditorIndentBackColor;
        codeEditor.BookmarkColor = Settings.Instance.EditorBookmarkColor;
        codeEditor.ServiceLinesColor = Settings.Instance.EditorServiceLineColor;
        codeEditor.ShowCodeFolding = Settings.Instance.EditorShowCodeFolding;
        codeEditor.ShowTextBlockIndentationGuides = Settings.Instance.EditorShowTextIndentGuides;
        codeEditor.Zoom = Settings.Instance.EditorZoomFactor;
        codeEditor.FoldingHighlightColor = Settings.Instance.EditorFoldingHighlightColor;
        codeEditor.FoldingHighlightEnabled = Settings.Instance.EditorShowFoldingBlockHighlights;
        BuildAutocompleteMenu(codeEditor);
    }

    private void BuildAutocompleteMenu(MooCodeEditor codeEditor)
    {
        codeEditor.AutocompleteMenu = new AutocompleteMenu(codeEditor);

        //editor.AutocompleteMenu.Items.ImageList = imageList1;
        codeEditor.AutocompleteMenu.SearchPattern = @"[\w\.:=!<>+-/*%&|^]";
        codeEditor.AutocompleteMenu.AllowTabKey = true;
        codeEditor.AutocompleteMenu.MinFragmentLength = 1;

        List<AutocompleteItem> items = new List<AutocompleteItem>();

        foreach (var item in Snippets.LoadSnippets(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Snippets.txt")))
            items.Add(new SnippetAutocompleteItem(item) { ImageIndex = 1 });
        foreach (var item in Moo.Editor.Moo.Keywords)
            items.Add(new AutoIndentingSnippet(item));
        foreach (var builtin in Moo.Editor.Moo.Builtins.Values)
            items.Add(new SnippetAutocompleteItem(builtin));

        items.Add(new InsertSpaceSnippet());
        items.Add(new InsertSpaceSnippet(@"^(\w+)([=<>!&|%-+*/]+)(\w+)$"));
        items.Add(new FormatCommaSnippet(@"^(\w+)(([,]+)(\w+))+$"));

        //set as autocomplete source
        codeEditor.AutocompleteMenu.Items.SetAutocompleteItems(items);
        codeEditor.AutocompleteMenu.AppearInterval = Settings.Instance.EditorAutocompleteDelay;
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
