﻿#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="EditorPage.cs">
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

using Krypton.Navigator;
using Org.Edgerunner.ANTLR4.Tools.Common;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar.Errors;
using Org.Edgerunner.Moo.Editor;
using Org.Edgerunner.Moo.Editor.Controls;
using System;
using FastColoredTextBoxNS;
using FastColoredTextBoxNS.Types;
using Krypton.Toolkit;
using Org.Edgerunner.Moo.Communication.Interfaces;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Org.Edgerunner.Moo.Editor.Configuration;

namespace Org.Edgerunner.Moo.Udditor.Pages;

public class EditorPage : ManagedPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EditorPage"/> class.
    /// </summary>
    /// <param name="manager">The window manager.</param>
    /// <param name="verbName">Name of the verb.</param>
    /// <param name="worldName">Name of the world.</param>
    /// <param name="dialect">The dialect.</param>
    /// <param name="source">The source.</param>
    // ReSharper disable once TooManyDependencies
    public EditorPage(WindowManager manager, string verbName, string worldName, GrammarDialect dialect, string source)
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
    /// Initializes a new instance of the <see cref="EditorPage"/> class.
    /// </summary>
    /// <param name="manager">The window manager.</param>
    /// <param name="dialect">The dialect.</param>
    /// <param name="filePath">The file path.</param>
    public EditorPage(WindowManager manager, GrammarDialect dialect, string filePath)
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
    /// Initializes a new instance of the <see cref="EditorPage"/> class.
    /// </summary>
    /// <param name="manager">The window manager.</param>
    /// <param name="dialect">The dialect.</param>
    public EditorPage(WindowManager manager, GrammarDialect dialect)
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
        Editor = new MooEditor();
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
        Editor.SelectionChangedDelayed += Editor_SelectionChangedDelayed;
        Editor.Selection = new TextSelectionRange(Editor, 0, 0, 0, 0);
    }

    /// <summary>
    /// Occurs when [parsing complete].
    /// </summary>
    public event EventHandler<ParsingCompleteEventArgs> ParsingComplete
    {
        add { Editor.ParsingComplete += value; }
        remove { Editor.ParsingComplete -= value; }
    }

    /// <summary>
    /// Occurs when [cursor position changed].
    /// </summary>
    public event EventHandler CursorPositionChanged;

    /// <summary>
    /// Gets the current line number.
    /// </summary>
    /// <value>The current line number.</value>
    public int CurrentLineNumber => Editor.Selection.Start.iLine;

    /// <summary>
    /// Gets the current column position.
    /// </summary>
    /// <value>The current column position.</value>
    public int CurrentColumnPosition => Editor.Selection.Start.iChar;

    /// <summary>
    /// Gets or sets the document.
    /// </summary>
    /// <value>The document.</value>
    public DocumentInfo Document
    {
        get => Editor.Document;
        set => Editor.Document = value;
    }

    /// <summary>
    /// Gets or sets the editor.
    /// </summary>
    /// <value>The editor.</value>
    public MooEditor Editor { get; set; }

    /// <summary>
    /// Gets or sets the uploader.
    /// </summary>
    /// <value>
    /// The uploader.
    /// </value>
    public IClientUploader Uploader { get; set; }

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
    /// Gets or sets a value indicating whether word wrap is enabled in the editor.
    /// </summary>
    /// <value>
    ///   <c>true</c> if word wrap enabled; otherwise, <c>false</c>.
    /// </value>
    public bool WordWrap
    {
        get => Editor.WordWrap;
        set => Editor.WordWrap = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show line numbers in the editor.
    /// </summary>
    /// <value>
    ///   <c>true</c> if show line numbers is enabled; otherwise, <c>false</c>.
    /// </value>
    /// <exception cref="System.NotImplementedException"></exception>
    public bool ShowLineNumbers
    {
        get => Editor.ShowLineNumbers;
        set => Editor.ShowLineNumbers = value;
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

    public bool CanUpload => Uploader != null && Uploader.ClientTerminal.IsConnected;

    /// <summary>
    /// Attempts to upload the source code to the linked client terminal.
    /// </summary>
    /// <returns></returns>
    public bool UploadSource()
    {
        if (Uploader == null)
            return false;

        if (Uploader.Upload(Editor.Text))
            Editor.IsChanged = false;
        return true;
    }

    private void Editor_SelectionChangedDelayed(object sender, EventArgs e)
    {
        CursorPositionChanged?.Invoke(this, e);
    }

    private void ConfigureEditorSettings(MooEditor editor)
    {
        editor.Font = new Font(Settings.Instance.EditorFontFamily, Settings.Instance.EditorFontSize);
        editor.ForeColor = Settings.Instance.EditorTextColor;
        editor.CaretColor = Settings.Instance.EditorCaretColor;
        editor.BackColor = Settings.Instance.EditorBackgroundColor;
        editor.CurrentLineColor = Settings.Instance.EditorCurrentLineColor;
        editor.AutoIndent = Settings.Instance.EditorAutoIndent;
        editor.WordWrapIndent = Settings.Instance.EditorWordWrapIndent;
        editor.WordWrapAutoIndent = Settings.Instance.EditorWordWrapAutoIndent;
        editor.WordWrap = Settings.Instance.EditorWordWrap;
        editor.AutoCompleteBrackets = Settings.Instance.EditorAutoBrackets;
        editor.TabLength = Settings.Instance.EditorTabLength;
        editor.LineNumberColor = Settings.Instance.EditorLineNumberColor;
        editor.SelectionColor = Settings.Instance.EditorTextSelectionColor;
        editor.ChangedLineColor = Settings.Instance.EditorChangedLineColor;
        editor.FoldingIndicatorColor = Settings.Instance.EditorFoldingIndicatorColor;
        editor.IndentBackColor = Settings.Instance.EditorIndentBackColor;
        editor.BookmarkColor = Settings.Instance.EditorBookmarkColor;
        editor.ServiceLinesColor = Settings.Instance.EditorServiceLineColor;
        editor.ShowCodeFolding = Settings.Instance.EditorShowCodeFolding;
        editor.ShowTextBlockIndentationGuides = Settings.Instance.EditorShowTextIndentGuides;
        editor.Zoom = Settings.Instance.EditorZoomFactor;
        BuildAutocompleteMenu(editor);
    }

    private void BuildAutocompleteMenu(MooEditor editor)
    {
        editor.AutocompleteMenu = new AutocompleteMenu(editor);

        //editor.AutocompleteMenu.Items.ImageList = imageList1;
        editor.AutocompleteMenu.SearchPattern = @"[\w\.:=!<>+-/*%&|^]";
        editor.AutocompleteMenu.AllowTabKey = true;
        editor.AutocompleteMenu.MinFragmentLength = 1;

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
        editor.AutocompleteMenu.Items.SetAutocompleteItems(items);
        editor.AutocompleteMenu.AppearInterval = Settings.Instance.EditorAutocompleteDelay;
    }

    /// <summary>
    /// Parses the editor source code.
    /// </summary>
    public void ParseSourceCode()
    {
        Editor.ParseSourceCode();
    }
}