using Org.Edgerunner.Moo.Editor.Controls;

namespace Org.Edgerunner.Moo.Udditor
{
   partial class Editor
   {
      /// <summary>
      ///  Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      ///  Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      ///  Required method for Designer support - do not modify
      ///  the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.components = new System.ComponentModel.Container();
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Editor));
         this.menuStrip1 = new System.Windows.Forms.MenuStrip();
         this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemOpenFile = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemSaveFile = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
         this.mnuItemExit = new System.Windows.Forms.ToolStripMenuItem();
         this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemFormat = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
         this.mnuItemCut = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemCutPaste = new System.Windows.Forms.ToolStripMenuItem();
         this.statusStrip1 = new System.Windows.Forms.StatusStrip();
         this.tlLblLine = new System.Windows.Forms.ToolStripStatusLabel();
         this.tlStatusLine = new System.Windows.Forms.ToolStripStatusLabel();
         this.tlLblColumn = new System.Windows.Forms.ToolStripStatusLabel();
         this.tlStatusColumn = new System.Windows.Forms.ToolStripStatusLabel();
         this.mooCodeEditor = new Org.Edgerunner.Moo.Editor.Controls.MooEditor();
         this.errorDisplay1 = new Org.Edgerunner.Moo.Editor.Controls.ErrorDisplay();
         this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
         this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
         this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
         this.mnuItemFind = new System.Windows.Forms.ToolStripMenuItem();
         this.menuStrip1.SuspendLayout();
         this.statusStrip1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.mooCodeEditor)).BeginInit();
         this.SuspendLayout();
         // 
         // menuStrip1
         // 
         this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
         this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem});
         this.menuStrip1.Location = new System.Drawing.Point(0, 0);
         this.menuStrip1.Name = "menuStrip1";
         this.menuStrip1.Size = new System.Drawing.Size(957, 28);
         this.menuStrip1.TabIndex = 0;
         this.menuStrip1.Text = "menuStrip1";
         // 
         // fileToolStripMenuItem
         // 
         this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuItemOpenFile,
            this.mnuItemSaveFile,
            this.toolStripSeparator1,
            this.mnuItemExit});
         this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
         this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
         this.fileToolStripMenuItem.Text = "&File";
         // 
         // mnuItemOpenFile
         // 
         this.mnuItemOpenFile.Name = "mnuItemOpenFile";
         this.mnuItemOpenFile.Size = new System.Drawing.Size(128, 26);
         this.mnuItemOpenFile.Text = "&Open";
         this.mnuItemOpenFile.Click += new System.EventHandler(this.mnuItemOpenFile_Click);
         // 
         // mnuItemSaveFile
         // 
         this.mnuItemSaveFile.Name = "mnuItemSaveFile";
         this.mnuItemSaveFile.Size = new System.Drawing.Size(128, 26);
         this.mnuItemSaveFile.Text = "&Save";
         this.mnuItemSaveFile.Click += new System.EventHandler(this.mnuItemSaveFile_Click);
         // 
         // toolStripSeparator1
         // 
         this.toolStripSeparator1.Name = "toolStripSeparator1";
         this.toolStripSeparator1.Size = new System.Drawing.Size(125, 6);
         // 
         // mnuItemExit
         // 
         this.mnuItemExit.Name = "mnuItemExit";
         this.mnuItemExit.Size = new System.Drawing.Size(128, 26);
         this.mnuItemExit.Text = "E&xit";
         this.mnuItemExit.Click += new System.EventHandler(this.mnuItemExit_Click);
         // 
         // editToolStripMenuItem
         // 
         this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuItemFormat,
            this.toolStripSeparator2,
            this.mnuItemCut,
            this.mnuItemCopy,
            this.mnuItemCutPaste,
            this.toolStripSeparator3,
            this.mnuItemFind});
         this.editToolStripMenuItem.Name = "editToolStripMenuItem";
         this.editToolStripMenuItem.Size = new System.Drawing.Size(49, 24);
         this.editToolStripMenuItem.Text = "&Edit";
         // 
         // mnuItemFormat
         // 
         this.mnuItemFormat.Name = "mnuItemFormat";
         this.mnuItemFormat.Size = new System.Drawing.Size(224, 26);
         this.mnuItemFormat.Text = "Format";
         this.mnuItemFormat.Click += new System.EventHandler(this.mnuItemFormat_Click);
         // 
         // toolStripSeparator2
         // 
         this.toolStripSeparator2.Name = "toolStripSeparator2";
         this.toolStripSeparator2.Size = new System.Drawing.Size(221, 6);
         // 
         // mnuItemCut
         // 
         this.mnuItemCut.Name = "mnuItemCut";
         this.mnuItemCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
         this.mnuItemCut.Size = new System.Drawing.Size(224, 26);
         this.mnuItemCut.Text = "Cut";
         this.mnuItemCut.Click += new System.EventHandler(this.mnuItemCut_Click);
         // 
         // mnuItemCopy
         // 
         this.mnuItemCopy.Name = "mnuItemCopy";
         this.mnuItemCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
         this.mnuItemCopy.Size = new System.Drawing.Size(224, 26);
         this.mnuItemCopy.Text = "Copy";
         this.mnuItemCopy.Click += new System.EventHandler(this.mnuItemCopy_Click);
         // 
         // mnuItemCutPaste
         // 
         this.mnuItemCutPaste.Name = "mnuItemCutPaste";
         this.mnuItemCutPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
         this.mnuItemCutPaste.Size = new System.Drawing.Size(224, 26);
         this.mnuItemCutPaste.Text = "Paste";
         this.mnuItemCutPaste.Click += new System.EventHandler(this.mnuItemCutPaste_Click);
         // 
         // statusStrip1
         // 
         this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
         this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tlLblLine,
            this.tlStatusLine,
            this.tlLblColumn,
            this.tlStatusColumn});
         this.statusStrip1.Location = new System.Drawing.Point(0, 741);
         this.statusStrip1.Name = "statusStrip1";
         this.statusStrip1.Size = new System.Drawing.Size(957, 26);
         this.statusStrip1.TabIndex = 2;
         this.statusStrip1.Text = "statusStrip1";
         // 
         // tlLblLine
         // 
         this.tlLblLine.Name = "tlLblLine";
         this.tlLblLine.Size = new System.Drawing.Size(36, 20);
         this.tlLblLine.Text = "Line";
         // 
         // tlStatusLine
         // 
         this.tlStatusLine.Name = "tlStatusLine";
         this.tlStatusLine.Size = new System.Drawing.Size(17, 20);
         this.tlStatusLine.Text = "1";
         // 
         // tlLblColumn
         // 
         this.tlLblColumn.AutoSize = false;
         this.tlLblColumn.Name = "tlLblColumn";
         this.tlLblColumn.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
         this.tlLblColumn.Size = new System.Drawing.Size(60, 20);
         this.tlLblColumn.Text = "Column";
         // 
         // tlStatusColumn
         // 
         this.tlStatusColumn.Name = "tlStatusColumn";
         this.tlStatusColumn.Size = new System.Drawing.Size(17, 20);
         this.tlStatusColumn.Text = "1";
         // 
         // mooCodeEditor
         // 
         this.mooCodeEditor.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
         this.mooCodeEditor.AutoIndentChars = false;
         this.mooCodeEditor.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\r\n^\\s*(case|default)\\s*[^:]*" +
    "(?<range>:)\\s*(?<range>[^;]+);";
         this.mooCodeEditor.AutoScrollMinSize = new System.Drawing.Size(0, 18);
         this.mooCodeEditor.BackBrush = null;
         this.mooCodeEditor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
         this.mooCodeEditor.CharHeight = 18;
         this.mooCodeEditor.CharWidth = 10;
         this.mooCodeEditor.DefaultMarkerSize = 8;
         this.mooCodeEditor.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
         this.mooCodeEditor.Dock = System.Windows.Forms.DockStyle.Fill;
         this.mooCodeEditor.Document = null;
         this.mooCodeEditor.FindForm = null;
         this.mooCodeEditor.GoToForm = null;
         this.mooCodeEditor.Hotkeys = resources.GetString("mooCodeEditor.Hotkeys");
         this.mooCodeEditor.IsReplaceMode = false;
         this.mooCodeEditor.LeftBracket = '(';
         this.mooCodeEditor.LeftBracket2 = '{';
         this.mooCodeEditor.LeftBracket3 = '[';
         this.mooCodeEditor.Location = new System.Drawing.Point(0, 28);
         this.mooCodeEditor.Name = "mooCodeEditor";
         this.mooCodeEditor.Paddings = new System.Windows.Forms.Padding(0);
         this.mooCodeEditor.ReplaceForm = null;
         this.mooCodeEditor.RightBracket = ')';
         this.mooCodeEditor.RightBracket2 = '}';
         this.mooCodeEditor.RightBracket3 = ']';
         this.mooCodeEditor.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
         this.mooCodeEditor.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("mooCodeEditor.ServiceColors")));
         this.mooCodeEditor.Size = new System.Drawing.Size(957, 555);
         this.mooCodeEditor.TabIndex = 3;
         this.mooCodeEditor.TabLength = 2;
         this.mooCodeEditor.WordWrap = true;
         this.mooCodeEditor.WordWrapIndent = 2;
         this.mooCodeEditor.Zoom = 100;
         this.mooCodeEditor.SelectionChanged += new System.EventHandler(this.MooEditor_SelectionChanged);
         // 
         // errorDisplay1
         // 
         this.errorDisplay1.Dock = System.Windows.Forms.DockStyle.Bottom;
         this.errorDisplay1.FullRowSelect = true;
         this.errorDisplay1.GridLines = true;
         this.errorDisplay1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
         this.errorDisplay1.Location = new System.Drawing.Point(0, 583);
         this.errorDisplay1.MultiSelect = false;
         this.errorDisplay1.Name = "errorDisplay1";
         this.errorDisplay1.Size = new System.Drawing.Size(957, 158);
         this.errorDisplay1.Sorting = System.Windows.Forms.SortOrder.Ascending;
         this.errorDisplay1.TabIndex = 4;
         this.errorDisplay1.UseCompatibleStateImageBehavior = false;
         this.errorDisplay1.View = System.Windows.Forms.View.Details;
         // 
         // toolStripSeparator3
         // 
         this.toolStripSeparator3.Name = "toolStripSeparator3";
         this.toolStripSeparator3.Size = new System.Drawing.Size(221, 6);
         // 
         // mnuItemFind
         // 
         this.mnuItemFind.Name = "mnuItemFind";
         this.mnuItemFind.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
         this.mnuItemFind.Size = new System.Drawing.Size(224, 26);
         this.mnuItemFind.Text = "Find";
         this.mnuItemFind.Click += new System.EventHandler(this.mnuItemFind_Click);
         // 
         // Editor
         // 
         this.ClientSize = new System.Drawing.Size(957, 767);
         this.Controls.Add(this.mooCodeEditor);
         this.Controls.Add(this.menuStrip1);
         this.Controls.Add(this.errorDisplay1);
         this.Controls.Add(this.statusStrip1);
         this.MainMenuStrip = this.menuStrip1;
         this.Name = "Editor";
         this.Text = "Moo Udditor - A Moo IDE";
         this.menuStrip1.ResumeLayout(false);
         this.menuStrip1.PerformLayout();
         this.statusStrip1.ResumeLayout(false);
         this.statusStrip1.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.mooCodeEditor)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem mnuItemOpenFile;
        private ToolStripMenuItem mnuItemSaveFile;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem mnuItemExit;
        private StatusStrip statusStrip1;
        private MooEditor mooCodeEditor;
        private ErrorDisplay errorDisplay1;
        private ToolStripStatusLabel tlLblLine;
        private ToolStripStatusLabel tlStatusLine;
        private ToolStripStatusLabel tlLblColumn;
        private ToolStripStatusLabel tlStatusColumn;
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem mnuItemFormat;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem mnuItemCut;
        private ToolStripMenuItem mnuItemCopy;
        private ToolStripMenuItem mnuItemCutPaste;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem mnuItemFind;
    }
}
