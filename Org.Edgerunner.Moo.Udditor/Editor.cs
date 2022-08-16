﻿using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using FastColoredTextBoxNS;
using FastColoredTextBoxNS.Types;
using Krypton.Docking;
using Krypton.Navigator;
using Krypton.Toolkit;
using Org.Edgerunner.ANTLR4.Tools.Common;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar.Errors;
using Org.Edgerunner.Moo.Editor;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Org.Edgerunner.Moo.Editor.Configuration;
using Org.Edgerunner.Moo.Editor.Controls;

namespace Org.Edgerunner.Moo.Udditor;

public partial class Editor : Form
{
    public Editor()
    {
        InitializeComponent();
        Pages = new Dictionary<string, KryptonPage>();
        Errors = new Dictionary<string, List<ParseMessage>>();
        GrammarDialect = Settings.Instance.DefaultGrammarDialect;
        UpdateDialectMenu(Settings.Instance.DefaultGrammarDialect);
    }

    private void MooCodeEditor_ParsingComplete(object? sender, Moo.Editor.ParsingCompleteEventArgs e)
    {
        ErrorDisplay.PopulateErrors(e.ErrorMessages);
    }

    public MooEditor CurrentEditor { get; set; }

    private ErrorDisplay ErrorDisplay { get; set; }

    private GrammarDialect GrammarDialect { get; set; }

    private KryptonDockingWorkspace Workspace { get; set; }

    private Dictionary<string, KryptonPage> Pages { get; set; }

    private Dictionary<string, List<ParseMessage>> Errors { get; set; }

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
        BuildAutocompleteMenu(editor);
    }

    private void SetGrammar(GrammarDialect grammarDialect)
    {
        GrammarDialect = grammarDialect;
        CurrentEditor.GrammarDialect = grammarDialect;
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

    private void ConfigureMessageDisplay(ErrorDisplay display)
    {
        var font = new Font(Settings.Instance.EditorFontFamily, Settings.Instance.EditorFontSize);
        display.Font = font;
        display.ForeColor = Settings.Instance.EditorTextColor;
        display.BackColor = Settings.Instance.EditorBackgroundColor;
    }

    private void UpdateDialectMenu(GrammarDialect dialect)
    {
        if (dialect == GrammarDialect.Edgerunner)
        {
            tlMnuLanguageMoo.Checked = false;
            tlMnuLanguageTsMoo.Checked = false;
            tlMnuLanguageEdgeMoo.Checked = true;
        }
        else if (dialect == GrammarDialect.ToastStunt)
        {
            tlMnuLanguageMoo.Checked = false;
            tlMnuLanguageTsMoo.Checked = true;
            tlMnuLanguageEdgeMoo.Checked = false;
        }
        else
        {
            tlMnuLanguageMoo.Checked = true;
            tlMnuLanguageTsMoo.Checked = false;
            tlMnuLanguageEdgeMoo.Checked = false;
        }
    }

    private void Editor_Load(object sender, EventArgs e)
    {
        // Setup docking functionality
        Workspace = kryptonDockingManager.ManageWorkspace(kryptonDockableWorkspace);
        kryptonDockingManager.ManageControl("Control1", kryptonPanel);
        kryptonDockingManager.ManageFloating(this);

        kryptonDockingManager.AddDockspace("Control1", DockingEdge.Bottom, new KryptonPage[] { NewErrorDisplay() });
    }

    private KryptonPage NewPage(string id, string name, string title, string description, int image, Control content)
    {
        // Create new page with title and image
        KryptonPage page = new KryptonPage();
        page.UniqueName = id;
        page.Text = name;
        page.TextTitle = title;
        page.TextDescription = description;
        //p.ImageSmall = imageListSmall.Images[image];

        // Add the control for display inside the page
        content.Dock = DockStyle.Fill;
        page.Controls.Add(content);

        return page;
    }

    private KryptonPage NewEditorPage(string filePath)
    {
        var name = Path.GetFileName(filePath);
        var source = File.ReadAllText(filePath);
        var editor = NewEditor(source, GrammarDialect);
        var key = $"{name}-{Guid.NewGuid().ToString()}";
        editor.Document = new Document(key, name);
        editor.Text = source;
        editor.Selection = new TextSelectionRange(editor, 0, 0, 0, 0);
        var page = NewPage(key, name, filePath, filePath, 0, editor);
        Pages[key] = page;
        return page;
    }

    private KryptonPage NewEditorPage(string verbName, string hostName, GrammarDialect dialect, string source)
    {
        var editor = NewEditor(source, dialect);
        var title = $"{hostName}/{verbName}";
        var key = $"{verbName}-{Guid.NewGuid().ToString()}";
        editor.Document = new Document(key, verbName);
        editor.Text = source;
        editor.Selection = new TextSelectionRange(editor, 0, 0, 0, 0);
        var page = NewPage(key, verbName, title, title, 0, editor);
        Pages[key] = page;
        return page;
    }

    private KryptonPage NewEditorPage(GrammarDialect dialect)
    {
        var editor = NewEditor(string.Empty, dialect);
        var key = $"<New>-{Guid.NewGuid().ToString()}";
        var name = "<New>";
        editor.Document = new Document(key, name);
        var page = NewPage(key, name, name, name, 0, editor);
        Pages[key] = page;
        return page;
    }

    private MooEditor NewEditor(string source, GrammarDialect dialect)
    {
        var editor = new MooEditor();
        editor.BorderStyle = BorderStyle.Fixed3D;
        editor.GrammarDialect = dialect;
        ConfigureEditorSettings(editor);
        editor.ParsingComplete += MooCodeEditor_ParsingComplete;
        editor.Enter += Editor_Enter;
        editor.SelectionChanged += Editor_SelectionChanged;
        //CurrentEditor = editor;
        return editor;
    }

    private void Editor_SelectionChanged(object sender, EventArgs e)
    {
        var editor = (MooEditor)sender;
        if (editor != null && editor == CurrentEditor)
        {
            tlStatusLine.Text = ((CurrentEditor?.Selection.Start.iLine + 1) ?? 1).ToString();
            tlStatusColumn.Text = ((CurrentEditor?.Selection.Start.iChar + 1) ?? 1).ToString();
        }
    }

    private void Editor_Enter(object sender, EventArgs e)
    {
        var editor = (MooEditor)sender;
        CurrentEditor = editor;
        tlStatusLine.Text = (editor.Selection.Start.iLine + 1).ToString();
        tlStatusColumn.Text = (editor.Selection.Start.iChar + 1).ToString();
    }

    private KryptonPage NewErrorDisplay()
    {
        var display = new ErrorDisplay();
        ConfigureMessageDisplay(display);
        ErrorDisplay = display;
        display.DoubleClick += Display_DoubleClick;
        var page = NewPage("Errors", "Errors", "Error Messages", "A list of parser errors", 0, display);
        page.ClearFlags(KryptonPageFlags.DockingAllowClose);
        return page;
    }

    private void Display_DoubleClick(object sender, EventArgs e)
    {
        var errDisplay = (ErrorDisplay)sender;
        var key = errDisplay.SelectedItems[0].SubItems[0].Text;
        var line = int.Parse(errDisplay.SelectedItems[0].SubItems[2].Text);
        var column = int.Parse(errDisplay.SelectedItems[0].SubItems[3].Text);
        var page = Pages[key];
        Workspace.SelectPage(key);
        var editor = (MooEditor)page.Controls[0];
        editor.Selection = new TextSelectionRange(editor, column - 1, line - 1, column - 1, line - 1);
        editor.Focus();
    }

    private void mnuItemExit_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }

    private void tlMnuNew_Click(object sender, EventArgs e)
    {
        kryptonDockingManager.AddToWorkspace("Workspace", new KryptonPage[] { NewEditorPage(GrammarDialect) });
    }

    private void mnuItemOpenFile_Click(object sender, EventArgs e)
    {
        openFileDialog.Multiselect = false;
        openFileDialog.DefaultExt = "moo";
        openFileDialog.Filter = @"Moo files (*.moo)|*.moo|Text files (*.txt)|*.txt|All files (*.*)|*.*";
        openFileDialog.Title = "Please select a moo source file to open";
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            var path = openFileDialog.FileName;
            var page = NewEditorPage(path);
            kryptonDockingManager.AddToWorkspace("Workspace", new KryptonPage[] { page });
            Workspace.SelectPage(page.UniqueName);
        }
    }

    private void mnuItemSaveFile_Click(object sender, EventArgs e)
    {
        saveFileDialog.DefaultExt = "moo";
        saveFileDialog.Filter = @"Moo files (*.moo)|*.moo|Text files (*.txt)|*.txt|All files (*.*)|*.*";
        saveFileDialog.Title = "Please select a file name to save as";
        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            var path = saveFileDialog.FileName;
            var name = Path.GetFileName(path);
            File.WriteAllText(path, CurrentEditor.Text);
            // #TODO fix for multi-window support later
            CurrentEditor.Document = new Document(path, name); // #TODO fix for multi-window support later
            CurrentEditor.ParseSourceCode();
        }
    }

    private void mnuItemFormat_Click(object sender, EventArgs e)
    {
        CurrentEditor.SuspendLayout();
        var current = CurrentEditor.Selection.Clone();
        CurrentEditor.SelectAll();
        CurrentEditor.DoAutoIndent();
        CurrentEditor.Selection = current;
        CurrentEditor.ResumeLayout();
    }

    private void MooEditor_SelectionChanged(object sender, EventArgs e)
    {
        tlStatusLine.Text = ((CurrentEditor?.Selection.Start.iLine + 1) ?? 1).ToString();
        tlStatusColumn.Text = ((CurrentEditor?.Selection.Start.iChar + 1) ?? 1).ToString();
    }

    private void mnuItemCut_Click(object sender, EventArgs e)
    {
        CurrentEditor.Cut();
    }

    private void mnuItemCopy_Click(object sender, EventArgs e)
    {
        CurrentEditor.Copy();
    }

    private void mnuItemCutPaste_Click(object sender, EventArgs e)
    {
        CurrentEditor.Paste();
    }

    private void mnuItemFind_Click(object sender, EventArgs e)
    {
        CurrentEditor.ShowFindDialog();
    }

    private void tlMnuHelp_Click(object sender, EventArgs e)
    {
        AboutBox.Instance.ShowDialog();
    }

    private void tlMnuLanguageMoo_Click(object sender, EventArgs e)
    {
        tlMnuLanguageMoo.Checked = true;
        tlMnuLanguageTsMoo.Checked = false;
        tlMnuLanguageEdgeMoo.Checked = false;
        SetGrammar(GrammarDialect.LambdaMoo);
    }

    private void tlMnuLanguageTsMoo_Click(object sender, EventArgs e)
    {
        tlMnuLanguageMoo.Checked = false;
        tlMnuLanguageTsMoo.Checked = true;
        tlMnuLanguageEdgeMoo.Checked = false;
        SetGrammar(GrammarDialect.ToastStunt);
    }

    private void tlMnuLanguageEdgeMoo_Click(object sender, EventArgs e)
    {
        tlMnuLanguageMoo.Checked = false;
        tlMnuLanguageTsMoo.Checked = false;
        tlMnuLanguageEdgeMoo.Checked = true;
        SetGrammar(GrammarDialect.Edgerunner);
    }

    private void kryptonDockingManager_PageCloseRequest(object sender, CloseRequestEventArgs e)
    {

    }

    private void kryptonDockingManager_PageLoading(object sender, DockPageLoadingEventArgs e)
    {
        KryptonPage page = (KryptonPage)sender;
        Debug.WriteLine(page.Text);
    }
}
