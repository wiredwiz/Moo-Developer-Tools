#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="MarkdownEditorPage.cs">
// Copyright (c)  2022
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

using FastColoredTextBoxNS;
using FastColoredTextBoxNS.Types;
using Krypton.Toolkit;
using Org.Edgerunner.ANTLR4.Tools.Common;
using Org.Edgerunner.Moo.Editor.Controls;

namespace Org.Edgerunner.Moo.Udditor.Pages;

public class MarkdownEditorPage : EditorPage
{
    public MarkdownEditorPage(WindowManager manager, string documentName, string worldName, string source)
        : base(manager)
    {
        var name = $@"{documentName} - {worldName}";
        var key = $"{name}-{Guid.NewGuid()}";
        InitializeEditor(key, documentName, name);
        Editor.Document = new DocumentInfo(key, key, documentName);
        Editor.Input.Text = source;
        PostInitialize();
    }

    public override FastColoredTextBox SourceEditor => Editor.Input;

    public override int CurrentLineNumber => Editor.Input.Selection.Start.iLine;

    public override int CurrentColumnPosition => Editor.Input.Selection.Start.iChar;

    public override DocumentInfo Document
    {
        get => Editor.Document;
        set => Editor.Document = value;
    }

    /// <summary>
    /// Gets or sets the editor.
    /// </summary>
    /// <value>The editor.</value>
    public MarkdownEditor Editor { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether [enable preview].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [enable preview]; otherwise, <c>false</c>.
    /// </value>
    public bool EnablePreview
    {
        get => Editor.EnablePreview;
        set => Editor.EnablePreview = value;
    }

    /// <summary>
    /// Initializes a new editor control instance.
    /// </summary>
    /// <param name="id">The unique identifier for the page.</param>
    /// <param name="title">The page title.</param>
    /// <param name="description">The page description.</param>
    private void InitializeEditor(string id, string title, string description)
    {
        Editor = new MarkdownEditor();
        Editor.BorderStyle = BorderStyle.Fixed3D;
        Editor.Dock = DockStyle.Fill;
        Controls.Add(Editor);
        UniqueName = id;
        Text = title;
        TextTitle = title;
        TextDescription = description;
        ToolTipBody = description;
        ToolTipStyle = LabelStyle.ToolTip;
        PostInitialize();
    }

    private void PostInitialize()
    {
        Editor.Input.SelectionChangedDelayed += Editor_SelectionChangedDelayed;
        Editor.Input.Selection = new TextSelectionRange(SourceEditor, 0, 0, 0, 0);
    }

    private void Editor_SelectionChangedDelayed(object sender, EventArgs e)
    {
        OnCursorPositionChanged();
    }
}
