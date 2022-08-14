﻿using FastColoredTextBoxNS.Types;
using Org.Edgerunner.ANTLR4.Tools.Common;
using Org.Edgerunner.Moo.Editor.Controls;

namespace Org.Edgerunner.Moo.Udditor;

public partial class Editor : Form
{
    public Editor()
    {
        InitializeComponent();
        CurrentEditor = mooCodeEditor;
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
            CurrentEditor.Document = new Document(path, name); // #TODO fix for multi-window support later
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
}