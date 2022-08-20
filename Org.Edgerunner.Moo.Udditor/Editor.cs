using System.Diagnostics;
using System.IO;
using System.Text;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Org.Edgerunner.Moo.Udditor;

public partial class Editor : Form
{
    public Editor()
    {
        InitializeComponent();
        Pages = new Dictionary<string, KryptonPage>();
        Errors = new SortedDictionary<string, List<ParseMessage>>();
        DefaultGrammarDialect = Settings.Instance.DefaultGrammarDialect;
        UpdateDialectMenu(Settings.Instance.DefaultGrammarDialect);
    }

    private void MooCodeEditor_ParsingComplete(object sender, Moo.Editor.ParsingCompleteEventArgs e)
    {
        var key = e.Document.Id;
        if (e.ErrorMessages.Count == 0)
            Errors.Remove(key);
        else
            Errors[key] = e.ErrorMessages;
        var allErrors = new List<ParseMessage>();
        foreach (var eKey in Errors.Keys)
            allErrors.AddRange(Errors[eKey]);
        ErrorDisplay.PopulateErrors(allErrors);
    }

    public MooEditor CurrentEditor { get; set; }

    private ErrorDisplay ErrorDisplay { get; set; }

    private GrammarDialect DefaultGrammarDialect { get; set; }

    private KryptonDockingWorkspace Workspace { get; set; }

    private Dictionary<string, KryptonPage> Pages { get; set; }

    private SortedDictionary<string, List<ParseMessage>> Errors { get; set; }

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
        BuildAutocompleteMenu(editor);
    }

    private void SetDocumentGrammar(GrammarDialect grammarDialect)
    {
        if (CurrentEditor != null)
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

    private KryptonPage NewEditorPage(string filePath, GrammarDialect dialect)
    {
        var name = Path.GetFileName(filePath);
        var source = File.ReadAllText(filePath);
        var editor = NewEditor(dialect);
        editor.OpenFile(filePath);
        var key = $"{name}-{Guid.NewGuid()}";
        editor.Document = new Document(key, filePath, name);
        editor.Text = source;
        editor.Selection = new TextSelectionRange(editor, 0, 0, 0, 0);
        var page = NewPage(key, name, filePath, filePath, 0, editor);
        Pages[key] = page;
        return page;
    }

    private KryptonPage NewEditorPage(string verbName, string hostName, GrammarDialect dialect, string source)
    {
        var editor = NewEditor(dialect);
        var title = $"{hostName}/{verbName}";
        var key = $"{verbName}-{Guid.NewGuid().ToString()}";
        editor.Document = new Document(key, string.Empty, verbName);
        editor.Text = source;
        editor.IsChanged = false;
        editor.Selection = new TextSelectionRange(editor, 0, 0, 0, 0);
        var page = NewPage(key, verbName, title, title, 0, editor);
        Pages[key] = page;
        return page;
    }

    private KryptonPage NewEditorPage(GrammarDialect dialect)
    {
        var editor = NewEditor(dialect);
        var key = $"<New>-{Guid.NewGuid().ToString()}";
        var name = "<New>";
        editor.Document = new Document(key, string.Empty, name);
        var page = NewPage(key, name, name, name, 0, editor);
        Pages[key] = page;
        return page;
    }

    private MooEditor NewEditor(GrammarDialect dialect)
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
        tlStatusLine.Text = ((CurrentEditor?.Selection.Start.iLine + 1) ?? 1).ToString();
        tlStatusColumn.Text = ((CurrentEditor?.Selection.Start.iChar + 1) ?? 1).ToString();
        UpdateDialectMenu(CurrentEditor.GrammarDialect);
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

    public void SwitchToPage(string id)
    {
        var page = Pages[id];
        Workspace.SelectPage(page.UniqueName);
        var editor = (MooEditor)page.Controls[0];
        editor.Focus();
    }

    private void Display_DoubleClick(object sender, EventArgs e)
    {
        var errDisplay = (ErrorDisplay)sender;
        var key = errDisplay.SelectedItems[0].SubItems[0].Text;
        var line = int.Parse(errDisplay.SelectedItems[0].SubItems[2].Text);
        var column = int.Parse(errDisplay.SelectedItems[0].SubItems[3].Text);
        var page = Pages[key];
        var editor = (MooEditor)page.Controls[0];
        editor.Selection = new TextSelectionRange(editor, column - 1, line - 1, column - 1, line - 1);
        SwitchToPage(page.UniqueName);
    }

    private void mnuItemExit_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }

    private void tlMnuNew_Click(object sender, EventArgs e)
    {
        var page = NewEditorPage(DefaultGrammarDialect);
        kryptonDockingManager.AddToWorkspace("Workspace", new KryptonPage[] { page });
        SwitchToPage(page.UniqueName);
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
            var page = NewEditorPage(path, DefaultGrammarDialect);
            kryptonDockingManager.AddToWorkspace("Workspace", new KryptonPage[] { page });
            SwitchToPage(page.UniqueName);
        }
    }
    private void tlMnuItemFileSave_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentEditor.Document.Path))
            CurrentEditor.SaveToFile(CurrentEditor.Document.Path, Encoding.Default);
        else
        {
            saveFileDialog.DefaultExt = "moo";
            saveFileDialog.Filter = @"Moo files (*.moo)|*.moo|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.Title = "Please select a file name to save as";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var path = saveFileDialog.FileName;
                var name = Path.GetFileName(path);
                CurrentEditor.SaveToFile(path, Encoding.Default);
                CurrentEditor.Document.Path = path;
                CurrentEditor.Document.Name = name;
                CurrentEditor.ParseSourceCode(false);
            }
        }
        CurrentEditor.Invalidate();
    }

    private void mnuItemSaveAsFile_Click(object sender, EventArgs e)
    {
        saveFileDialog.DefaultExt = "moo";
        saveFileDialog.Filter = @"Moo files (*.moo)|*.moo|Text files (*.txt)|*.txt|All files (*.*)|*.*";
        saveFileDialog.Title = "Please select a file name to save as";
        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            var path = saveFileDialog.FileName;
            var name = Path.GetFileName(path);
            CurrentEditor.SaveToFile(path, Encoding.Default);
            CurrentEditor.Document.Path = path;
            CurrentEditor.Document.Name = name;
            CurrentEditor.ParseSourceCode(false);
        }
    }

    private void tlMnuItemClose_Click(object sender, EventArgs e)
    {
        var page = kryptonDockingManager.PageForUniqueName(CurrentEditor.Document.Id);
        kryptonDockingManager.RemovePage(page, true);
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
        SetDocumentGrammar(GrammarDialect.LambdaMoo);
    }

    private void tlMnuLanguageTsMoo_Click(object sender, EventArgs e)
    {
        tlMnuLanguageMoo.Checked = false;
        tlMnuLanguageTsMoo.Checked = true;
        tlMnuLanguageEdgeMoo.Checked = false;
        SetDocumentGrammar(GrammarDialect.ToastStunt);
    }

    private void tlMnuLanguageEdgeMoo_Click(object sender, EventArgs e)
    {
        tlMnuLanguageMoo.Checked = false;
        tlMnuLanguageTsMoo.Checked = false;
        tlMnuLanguageEdgeMoo.Checked = true;
        SetDocumentGrammar(GrammarDialect.Edgerunner);
    }

    private void kryptonDockingManager_PageCloseRequest(object sender, CloseRequestEventArgs e)
    {
        var key = e.UniqueName;
        var page = Pages[key];
        var editor = (MooEditor)page.Controls[0];
        if (editor == null && editor.IsChanged)
        {
            var name = editor.Document.Name;
            DialogResult dialogResult = MessageBox.Show($"\"{name}\" has been modified but has not been saved.  Would you like to save this file?", "Modified File", MessageBoxButtons.YesNo);
            if(dialogResult == DialogResult.Yes)
            {
                saveFileDialog.DefaultExt = "moo";
                saveFileDialog.Filter = @"Moo files (*.moo)|*.moo|Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.Title = "Please select a file name to save as";
                saveFileDialog.FileName = name;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var path = saveFileDialog.FileName;
                    editor.SaveToFile(path, Encoding.Default);
                }
                else
                    e.CloseRequest = DockingCloseRequest.None;
            }
        }
    }

    private void tlMnuItemToggleBookmark_Click(object sender, EventArgs e)
    {
       if (CurrentEditor != null)
         CurrentEditor.BookmarkLine(CurrentEditor.Selection.Start.iLine);
    }

    private void tlMnuItemNextBookmark_Click(object sender, EventArgs e)
    {
        if (CurrentEditor != null)
            CurrentEditor.GotoNextBookmark(CurrentEditor.Selection.Start.iLine);
    }

    private void tlMnuItemPrevBookmark_Click(object sender, EventArgs e)
    {
        if (CurrentEditor != null)
            CurrentEditor.GotoPrevBookmark(CurrentEditor.Selection.Start.iLine);
    }

    private void tlMnuItemToggleFolding_Click(object sender, EventArgs e)
    {
        if (CurrentEditor != null)
            CurrentEditor.ToggleFoldingBlock(CurrentEditor.Selection.Start.iLine);
    }

    private void tlMnuItemExpandAll_Click(object sender, EventArgs e)
    {
        if (CurrentEditor != null)
            CurrentEditor.ExpandAllFoldingBlocks();
    }

    private void tlMnuItemCollapseAll_Click(object sender, EventArgs e)
    {
        if (CurrentEditor != null)
            CurrentEditor.CollapseAllFoldingBlocks();
    }
}
