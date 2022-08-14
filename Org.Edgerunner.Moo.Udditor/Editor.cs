using FastColoredTextBoxNS;
using FastColoredTextBoxNS.Types;
using Org.Edgerunner.ANTLR4.Tools.Common;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Org.Edgerunner.Moo.Editor.Configuration;
using Org.Edgerunner.Moo.Editor.Controls;

namespace Org.Edgerunner.Moo.Udditor;

public partial class Editor : Form
{
    public Editor()
    {
        InitializeComponent();
        CurrentEditor = mooCodeEditor;
        ConfigureEditorSettings(CurrentEditor);
        ConfigureMessageDisplay(errorDisplay1);
        mooCodeEditor.ParsingComplete += MooCodeEditor_ParsingComplete;
    }

    private void MooCodeEditor_ParsingComplete(object? sender, Moo.Editor.ParsingCompleteEventArgs e)
    {
        errorDisplay1.PopulateErrors(e.ErrorMessages);
    }

    public MooEditor CurrentEditor { get; set; }

    private void mnuItemExit_Click(object sender, EventArgs e)
    {
        Application.Exit();
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

    private void ConfigureMessageDisplay(ErrorDisplay display)
    {
        var font = new Font(Settings.Instance.EditorFontFamily, Settings.Instance.EditorFontSize);
        display.Font = font;
        display.ForeColor = Settings.Instance.EditorTextColor;
        display.BackColor = Settings.Instance.EditorBackgroundColor;
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
            var name = Path.GetFileName(path);
            var stream = openFileDialog.OpenFile();
            using StreamReader reader = new StreamReader(stream);
            CurrentEditor.Text = reader.ReadToEnd();
            CurrentEditor.Document = new Document(path, name); // #TODO: fix for multi-window support later
            CurrentEditor.Selection = new TextSelectionRange(mooCodeEditor,0, 0, 0, 0);
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
        var current = mooCodeEditor.Selection.Clone();
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
}
