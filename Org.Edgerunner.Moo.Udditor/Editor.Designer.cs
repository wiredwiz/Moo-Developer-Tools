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
         this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
         this.mnuItemFind = new System.Windows.Forms.ToolStripMenuItem();
         this.grammarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.tlMnuLanguageMoo = new System.Windows.Forms.ToolStripMenuItem();
         this.tlMnuLanguageTsMoo = new System.Windows.Forms.ToolStripMenuItem();
         this.tlMnuLanguageEdgeMoo = new System.Windows.Forms.ToolStripMenuItem();
         this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.tlMnuHelp = new System.Windows.Forms.ToolStripMenuItem();
         this.statusStrip1 = new System.Windows.Forms.StatusStrip();
         this.tlLblLine = new System.Windows.Forms.ToolStripStatusLabel();
         this.tlStatusLine = new System.Windows.Forms.ToolStripStatusLabel();
         this.tlLblColumn = new System.Windows.Forms.ToolStripStatusLabel();
         this.tlStatusColumn = new System.Windows.Forms.ToolStripStatusLabel();
         this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
         this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
         this.kryptonDockingManager = new Krypton.Docking.KryptonDockingManager();
         this.kryptonManager = new Krypton.Toolkit.KryptonManager(this.components);
         this.kryptonPanel = new Krypton.Toolkit.KryptonPanel();
         this.kryptonDockableWorkspace = new Krypton.Docking.KryptonDockableWorkspace();
         this.tlMnuNew = new System.Windows.Forms.ToolStripMenuItem();
         this.menuStrip1.SuspendLayout();
         this.statusStrip1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel)).BeginInit();
         this.kryptonPanel.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.kryptonDockableWorkspace)).BeginInit();
         this.SuspendLayout();
         // 
         // menuStrip1
         // 
         this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
         this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.grammarToolStripMenuItem,
            this.helpToolStripMenuItem});
         this.menuStrip1.Location = new System.Drawing.Point(0, 0);
         this.menuStrip1.Name = "menuStrip1";
         this.menuStrip1.Size = new System.Drawing.Size(957, 28);
         this.menuStrip1.TabIndex = 0;
         this.menuStrip1.Text = "menuStrip1";
         // 
         // fileToolStripMenuItem
         // 
         this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tlMnuNew,
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
         this.mnuItemOpenFile.Size = new System.Drawing.Size(224, 26);
         this.mnuItemOpenFile.Text = "&Open";
         this.mnuItemOpenFile.Click += new System.EventHandler(this.mnuItemOpenFile_Click);
         // 
         // mnuItemSaveFile
         // 
         this.mnuItemSaveFile.Name = "mnuItemSaveFile";
         this.mnuItemSaveFile.Size = new System.Drawing.Size(224, 26);
         this.mnuItemSaveFile.Text = "&Save";
         this.mnuItemSaveFile.Click += new System.EventHandler(this.mnuItemSaveFile_Click);
         // 
         // toolStripSeparator1
         // 
         this.toolStripSeparator1.Name = "toolStripSeparator1";
         this.toolStripSeparator1.Size = new System.Drawing.Size(221, 6);
         // 
         // mnuItemExit
         // 
         this.mnuItemExit.Name = "mnuItemExit";
         this.mnuItemExit.Size = new System.Drawing.Size(224, 26);
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
         this.mnuItemFormat.Size = new System.Drawing.Size(177, 26);
         this.mnuItemFormat.Text = "Format";
         this.mnuItemFormat.Click += new System.EventHandler(this.mnuItemFormat_Click);
         // 
         // toolStripSeparator2
         // 
         this.toolStripSeparator2.Name = "toolStripSeparator2";
         this.toolStripSeparator2.Size = new System.Drawing.Size(174, 6);
         // 
         // mnuItemCut
         // 
         this.mnuItemCut.Name = "mnuItemCut";
         this.mnuItemCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
         this.mnuItemCut.Size = new System.Drawing.Size(177, 26);
         this.mnuItemCut.Text = "Cut";
         this.mnuItemCut.Click += new System.EventHandler(this.mnuItemCut_Click);
         // 
         // mnuItemCopy
         // 
         this.mnuItemCopy.Name = "mnuItemCopy";
         this.mnuItemCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
         this.mnuItemCopy.Size = new System.Drawing.Size(177, 26);
         this.mnuItemCopy.Text = "Copy";
         this.mnuItemCopy.Click += new System.EventHandler(this.mnuItemCopy_Click);
         // 
         // mnuItemCutPaste
         // 
         this.mnuItemCutPaste.Name = "mnuItemCutPaste";
         this.mnuItemCutPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
         this.mnuItemCutPaste.Size = new System.Drawing.Size(177, 26);
         this.mnuItemCutPaste.Text = "Paste";
         this.mnuItemCutPaste.Click += new System.EventHandler(this.mnuItemCutPaste_Click);
         // 
         // toolStripSeparator3
         // 
         this.toolStripSeparator3.Name = "toolStripSeparator3";
         this.toolStripSeparator3.Size = new System.Drawing.Size(174, 6);
         // 
         // mnuItemFind
         // 
         this.mnuItemFind.Name = "mnuItemFind";
         this.mnuItemFind.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
         this.mnuItemFind.Size = new System.Drawing.Size(177, 26);
         this.mnuItemFind.Text = "Find";
         this.mnuItemFind.Click += new System.EventHandler(this.mnuItemFind_Click);
         // 
         // grammarToolStripMenuItem
         // 
         this.grammarToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tlMnuLanguageMoo,
            this.tlMnuLanguageTsMoo,
            this.tlMnuLanguageEdgeMoo});
         this.grammarToolStripMenuItem.Name = "grammarToolStripMenuItem";
         this.grammarToolStripMenuItem.Size = new System.Drawing.Size(136, 24);
         this.grammarToolStripMenuItem.Text = "Grammar Dialect";
         // 
         // tlMnuLanguageMoo
         // 
         this.tlMnuLanguageMoo.Name = "tlMnuLanguageMoo";
         this.tlMnuLanguageMoo.Size = new System.Drawing.Size(177, 26);
         this.tlMnuLanguageMoo.Text = "LambdaMoo";
         this.tlMnuLanguageMoo.Click += new System.EventHandler(this.tlMnuLanguageMoo_Click);
         // 
         // tlMnuLanguageTsMoo
         // 
         this.tlMnuLanguageTsMoo.Name = "tlMnuLanguageTsMoo";
         this.tlMnuLanguageTsMoo.Size = new System.Drawing.Size(177, 26);
         this.tlMnuLanguageTsMoo.Text = "ToastStunt";
         this.tlMnuLanguageTsMoo.Click += new System.EventHandler(this.tlMnuLanguageTsMoo_Click);
         // 
         // tlMnuLanguageEdgeMoo
         // 
         this.tlMnuLanguageEdgeMoo.Checked = true;
         this.tlMnuLanguageEdgeMoo.CheckState = System.Windows.Forms.CheckState.Checked;
         this.tlMnuLanguageEdgeMoo.Name = "tlMnuLanguageEdgeMoo";
         this.tlMnuLanguageEdgeMoo.Size = new System.Drawing.Size(177, 26);
         this.tlMnuLanguageEdgeMoo.Text = "Edgerunner";
         this.tlMnuLanguageEdgeMoo.Click += new System.EventHandler(this.tlMnuLanguageEdgeMoo_Click);
         // 
         // helpToolStripMenuItem
         // 
         this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tlMnuHelp});
         this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
         this.helpToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
         this.helpToolStripMenuItem.Text = "Help";
         // 
         // tlMnuHelp
         // 
         this.tlMnuHelp.Name = "tlMnuHelp";
         this.tlMnuHelp.Size = new System.Drawing.Size(133, 26);
         this.tlMnuHelp.Text = "About";
         this.tlMnuHelp.Click += new System.EventHandler(this.tlMnuHelp_Click);
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
         // kryptonDockingManager
         // 
         this.kryptonDockingManager.PageCloseRequest += new System.EventHandler<Krypton.Docking.CloseRequestEventArgs>(this.kryptonDockingManager_PageCloseRequest);
         // 
         // kryptonPanel
         // 
         this.kryptonPanel.Controls.Add(this.kryptonDockableWorkspace);
         this.kryptonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.kryptonPanel.Location = new System.Drawing.Point(0, 28);
         this.kryptonPanel.Name = "kryptonPanel";
         this.kryptonPanel.Size = new System.Drawing.Size(957, 713);
         this.kryptonPanel.TabIndex = 5;
         // 
         // kryptonDockableWorkspace
         // 
         this.kryptonDockableWorkspace.ActivePage = null;
         this.kryptonDockableWorkspace.AutoHiddenHost = false;
         this.kryptonDockableWorkspace.CompactFlags = ((Krypton.Workspace.CompactFlags)(((Krypton.Workspace.CompactFlags.RemoveEmptyCells | Krypton.Workspace.CompactFlags.RemoveEmptySequences) 
            | Krypton.Workspace.CompactFlags.PromoteLeafs)));
         this.kryptonDockableWorkspace.ContainerBackStyle = Krypton.Toolkit.PaletteBackStyle.PanelClient;
         this.kryptonDockableWorkspace.Dock = System.Windows.Forms.DockStyle.Fill;
         this.kryptonDockableWorkspace.Location = new System.Drawing.Point(0, 0);
         this.kryptonDockableWorkspace.Name = "kryptonDockableWorkspace";
         // 
         // 
         // 
         this.kryptonDockableWorkspace.Root.UniqueName = "1ca2dbc169f14bf7b554571d33e1fb83";
         this.kryptonDockableWorkspace.Root.WorkspaceControl = this.kryptonDockableWorkspace;
         this.kryptonDockableWorkspace.SeparatorStyle = Krypton.Toolkit.SeparatorStyle.LowProfile;
         this.kryptonDockableWorkspace.ShowMaximizeButton = false;
         this.kryptonDockableWorkspace.Size = new System.Drawing.Size(957, 713);
         this.kryptonDockableWorkspace.SplitterWidth = 5;
         this.kryptonDockableWorkspace.TabIndex = 0;
         this.kryptonDockableWorkspace.TabStop = true;
         // 
         // tlMnuNew
         // 
         this.tlMnuNew.Name = "tlMnuNew";
         this.tlMnuNew.Size = new System.Drawing.Size(224, 26);
         this.tlMnuNew.Text = "&New";
         this.tlMnuNew.Click += new System.EventHandler(this.tlMnuNew_Click);
         // 
         // Editor
         // 
         this.ClientSize = new System.Drawing.Size(957, 767);
         this.Controls.Add(this.kryptonPanel);
         this.Controls.Add(this.menuStrip1);
         this.Controls.Add(this.statusStrip1);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.MainMenuStrip = this.menuStrip1;
         this.Name = "Editor";
         this.Text = "Moo Udditor - A Moo IDE";
         this.Load += new System.EventHandler(this.Editor_Load);
         this.menuStrip1.ResumeLayout(false);
         this.menuStrip1.PerformLayout();
         this.statusStrip1.ResumeLayout(false);
         this.statusStrip1.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel)).EndInit();
         this.kryptonPanel.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.kryptonDockableWorkspace)).EndInit();
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
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem tlMnuHelp;
        private ToolStripMenuItem grammarToolStripMenuItem;
        private ToolStripMenuItem tlMnuLanguageMoo;
        private ToolStripMenuItem tlMnuLanguageTsMoo;
        private ToolStripMenuItem tlMnuLanguageEdgeMoo;
        private Krypton.Docking.KryptonDockingManager kryptonDockingManager;
        private Krypton.Toolkit.KryptonManager kryptonManager;
        private Krypton.Toolkit.KryptonPanel kryptonPanel;
        private Krypton.Docking.KryptonDockableWorkspace kryptonDockableWorkspace;
        private ToolStripMenuItem tlMnuNew;
    }
}
