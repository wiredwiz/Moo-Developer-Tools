using Krypton.Docking;

namespace Org.Edgerunner.Moo.Udditor.Main
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
         this.tlMnuNew = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemOpenFile = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemUpload = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemUpload2 = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemFileSave = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemSaveAsFile = new System.Windows.Forms.ToolStripMenuItem();
         this.tlMnuItemClose = new System.Windows.Forms.ToolStripMenuItem();
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
         this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
         this.mnuItemBookmarks = new System.Windows.Forms.ToolStripMenuItem();
         this.tlMnuItemToggleBookmark = new System.Windows.Forms.ToolStripMenuItem();
         this.tlMnuItemNextBookmark = new System.Windows.Forms.ToolStripMenuItem();
         this.tlMnuItemPrevBookmark = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemFolding = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemEnableCodeFolding = new System.Windows.Forms.ToolStripMenuItem();
         this.tlMnuItemToggleFolding = new System.Windows.Forms.ToolStripMenuItem();
         this.tlMnuItemExpandAll = new System.Windows.Forms.ToolStripMenuItem();
         this.tlMnuItemCollapseAll = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
         this.mnuItemMarkdown = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemMarkdownSupport = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemMooText = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemMooTextColor = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemView = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemZoomIn = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemZoomOut = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
         this.mnuItemWordWrap = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemShowLineNumbers = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemIndentationGuides = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemShowPreviewPane = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemTerminal = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemWorldManager = new System.Windows.Forms.ToolStripMenuItem();
         this.tlMnuItemOpenConnection = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemCloseConnection = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
         this.mnuItemEchoCommands = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemEnableColor = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemEnableBlinking = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemEnableAudio = new System.Windows.Forms.ToolStripMenuItem();
         this.grammarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.tlMnuLanguageMoo = new System.Windows.Forms.ToolStripMenuItem();
         this.tlMnuLanguageTsMoo = new System.Windows.Forms.ToolStripMenuItem();
         this.tlMnuLanguageEdgeMoo = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemWindow = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemPrevEditor = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuItemPrevTerminal = new System.Windows.Forms.ToolStripMenuItem();
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
         this.kryptonPalette1 = new Krypton.Toolkit.KryptonPalette(this.components);
         this.kryptonPanel = new Krypton.Toolkit.KryptonPanel();
         this.kryptonDockableWorkspace = new Krypton.Docking.KryptonDockableWorkspace();
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
            this.mnuItemView,
            this.mnuItemTerminal,
            this.grammarToolStripMenuItem,
            this.mnuItemWindow,
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
            this.mnuItemUpload,
            this.mnuItemUpload2,
            this.mnuItemFileSave,
            this.mnuItemSaveAsFile,
            this.tlMnuItemClose,
            this.toolStripSeparator1,
            this.mnuItemExit});
         this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
         this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
         this.fileToolStripMenuItem.Text = "&File";
         // 
         // tlMnuNew
         // 
         this.tlMnuNew.Name = "tlMnuNew";
         this.tlMnuNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
         this.tlMnuNew.Size = new System.Drawing.Size(231, 26);
         this.tlMnuNew.Text = "&New";
         this.tlMnuNew.Click += new System.EventHandler(this.tlMnuNew_Click);
         // 
         // mnuItemOpenFile
         // 
         this.mnuItemOpenFile.Name = "mnuItemOpenFile";
         this.mnuItemOpenFile.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
         this.mnuItemOpenFile.Size = new System.Drawing.Size(231, 26);
         this.mnuItemOpenFile.Text = "&Open";
         this.mnuItemOpenFile.Click += new System.EventHandler(this.mnuItemOpenFile_Click);
         // 
         // mnuItemUpload
         // 
         this.mnuItemUpload.Name = "mnuItemUpload";
         this.mnuItemUpload.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
         this.mnuItemUpload.Size = new System.Drawing.Size(231, 26);
         this.mnuItemUpload.Text = "Upload";
         this.mnuItemUpload.Click += new System.EventHandler(this.mnuItemUpload_Click);
         // 
         // mnuItemUpload2
         // 
         this.mnuItemUpload2.Name = "mnuItemUpload2";
         this.mnuItemUpload2.ShortcutKeys = System.Windows.Forms.Keys.F7;
         this.mnuItemUpload2.Size = new System.Drawing.Size(231, 26);
         this.mnuItemUpload2.Text = "Upload 2";
         this.mnuItemUpload2.Visible = false;
         this.mnuItemUpload2.Click += new System.EventHandler(this.mnuItemUpload_Click);
         // 
         // mnuItemFileSave
         // 
         this.mnuItemFileSave.Name = "mnuItemFileSave";
         this.mnuItemFileSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
         this.mnuItemFileSave.Size = new System.Drawing.Size(231, 26);
         this.mnuItemFileSave.Text = "&Save";
         this.mnuItemFileSave.Click += new System.EventHandler(this.mnuItemSave_Click);
         // 
         // mnuItemSaveAsFile
         // 
         this.mnuItemSaveAsFile.Name = "mnuItemSaveAsFile";
         this.mnuItemSaveAsFile.Size = new System.Drawing.Size(231, 26);
         this.mnuItemSaveAsFile.Text = "Save &As";
         this.mnuItemSaveAsFile.Click += new System.EventHandler(this.mnuItemSaveAsFile_Click);
         // 
         // tlMnuItemClose
         // 
         this.tlMnuItemClose.Name = "tlMnuItemClose";
         this.tlMnuItemClose.Size = new System.Drawing.Size(231, 26);
         this.tlMnuItemClose.Text = "&Close";
         this.tlMnuItemClose.Click += new System.EventHandler(this.tlMnuItemClose_Click);
         // 
         // toolStripSeparator1
         // 
         this.toolStripSeparator1.Name = "toolStripSeparator1";
         this.toolStripSeparator1.Size = new System.Drawing.Size(228, 6);
         // 
         // mnuItemExit
         // 
         this.mnuItemExit.Name = "mnuItemExit";
         this.mnuItemExit.Size = new System.Drawing.Size(231, 26);
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
            this.mnuItemFind,
            this.toolStripSeparator4,
            this.mnuItemBookmarks,
            this.mnuItemFolding,
            this.toolStripSeparator7,
            this.mnuItemMarkdown,
            this.mnuItemMooText});
         this.editToolStripMenuItem.Name = "editToolStripMenuItem";
         this.editToolStripMenuItem.Size = new System.Drawing.Size(49, 24);
         this.editToolStripMenuItem.Text = "&Edit";
         // 
         // mnuItemFormat
         // 
         this.mnuItemFormat.Name = "mnuItemFormat";
         this.mnuItemFormat.Size = new System.Drawing.Size(169, 26);
         this.mnuItemFormat.Text = "Format";
         this.mnuItemFormat.Click += new System.EventHandler(this.mnuItemFormat_Click);
         // 
         // toolStripSeparator2
         // 
         this.toolStripSeparator2.Name = "toolStripSeparator2";
         this.toolStripSeparator2.Size = new System.Drawing.Size(166, 6);
         // 
         // mnuItemCut
         // 
         this.mnuItemCut.Name = "mnuItemCut";
         this.mnuItemCut.Size = new System.Drawing.Size(169, 26);
         this.mnuItemCut.Text = "Cut";
         this.mnuItemCut.Click += new System.EventHandler(this.mnuItemCut_Click);
         // 
         // mnuItemCopy
         // 
         this.mnuItemCopy.Name = "mnuItemCopy";
         this.mnuItemCopy.Size = new System.Drawing.Size(169, 26);
         this.mnuItemCopy.Text = "Copy";
         this.mnuItemCopy.Click += new System.EventHandler(this.mnuItemCopy_Click);
         // 
         // mnuItemCutPaste
         // 
         this.mnuItemCutPaste.Name = "mnuItemCutPaste";
         this.mnuItemCutPaste.Size = new System.Drawing.Size(169, 26);
         this.mnuItemCutPaste.Text = "Paste";
         this.mnuItemCutPaste.Click += new System.EventHandler(this.mnuItemCutPaste_Click);
         // 
         // toolStripSeparator3
         // 
         this.toolStripSeparator3.Name = "toolStripSeparator3";
         this.toolStripSeparator3.Size = new System.Drawing.Size(166, 6);
         // 
         // mnuItemFind
         // 
         this.mnuItemFind.Name = "mnuItemFind";
         this.mnuItemFind.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
         this.mnuItemFind.Size = new System.Drawing.Size(169, 26);
         this.mnuItemFind.Text = "Find";
         this.mnuItemFind.Click += new System.EventHandler(this.mnuItemFind_Click);
         // 
         // toolStripSeparator4
         // 
         this.toolStripSeparator4.Name = "toolStripSeparator4";
         this.toolStripSeparator4.Size = new System.Drawing.Size(166, 6);
         // 
         // mnuItemBookmarks
         // 
         this.mnuItemBookmarks.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tlMnuItemToggleBookmark,
            this.tlMnuItemNextBookmark,
            this.tlMnuItemPrevBookmark});
         this.mnuItemBookmarks.Name = "mnuItemBookmarks";
         this.mnuItemBookmarks.Size = new System.Drawing.Size(169, 26);
         this.mnuItemBookmarks.Text = "Bookmarks";
         // 
         // tlMnuItemToggleBookmark
         // 
         this.tlMnuItemToggleBookmark.Name = "tlMnuItemToggleBookmark";
         this.tlMnuItemToggleBookmark.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.K)));
         this.tlMnuItemToggleBookmark.Size = new System.Drawing.Size(319, 26);
         this.tlMnuItemToggleBookmark.Text = "Toggle Bookmark";
         this.tlMnuItemToggleBookmark.Click += new System.EventHandler(this.tlMnuItemToggleBookmark_Click);
         // 
         // tlMnuItemNextBookmark
         // 
         this.tlMnuItemNextBookmark.Name = "tlMnuItemNextBookmark";
         this.tlMnuItemNextBookmark.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.Down)));
         this.tlMnuItemNextBookmark.Size = new System.Drawing.Size(319, 26);
         this.tlMnuItemNextBookmark.Text = "Next Bookmark";
         this.tlMnuItemNextBookmark.Click += new System.EventHandler(this.tlMnuItemNextBookmark_Click);
         // 
         // tlMnuItemPrevBookmark
         // 
         this.tlMnuItemPrevBookmark.Name = "tlMnuItemPrevBookmark";
         this.tlMnuItemPrevBookmark.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.Up)));
         this.tlMnuItemPrevBookmark.Size = new System.Drawing.Size(319, 26);
         this.tlMnuItemPrevBookmark.Text = "Previous Bookmark";
         this.tlMnuItemPrevBookmark.Click += new System.EventHandler(this.tlMnuItemPrevBookmark_Click);
         // 
         // mnuItemFolding
         // 
         this.mnuItemFolding.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuItemEnableCodeFolding,
            this.tlMnuItemToggleFolding,
            this.tlMnuItemExpandAll,
            this.tlMnuItemCollapseAll});
         this.mnuItemFolding.Name = "mnuItemFolding";
         this.mnuItemFolding.Size = new System.Drawing.Size(169, 26);
         this.mnuItemFolding.Text = "Folding";
         // 
         // mnuItemEnableCodeFolding
         // 
         this.mnuItemEnableCodeFolding.Checked = true;
         this.mnuItemEnableCodeFolding.CheckOnClick = true;
         this.mnuItemEnableCodeFolding.CheckState = System.Windows.Forms.CheckState.Checked;
         this.mnuItemEnableCodeFolding.Name = "mnuItemEnableCodeFolding";
         this.mnuItemEnableCodeFolding.Size = new System.Drawing.Size(326, 26);
         this.mnuItemEnableCodeFolding.Text = "Enable folding";
         this.mnuItemEnableCodeFolding.CheckStateChanged += new System.EventHandler(this.mnuItemEnableCodeFolding_CheckStateChanged);
         // 
         // tlMnuItemToggleFolding
         // 
         this.tlMnuItemToggleFolding.Name = "tlMnuItemToggleFolding";
         this.tlMnuItemToggleFolding.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.Space)));
         this.tlMnuItemToggleFolding.Size = new System.Drawing.Size(326, 26);
         this.tlMnuItemToggleFolding.Text = "Expand/Collapse";
         this.tlMnuItemToggleFolding.Click += new System.EventHandler(this.tlMnuItemToggleFolding_Click);
         // 
         // tlMnuItemExpandAll
         // 
         this.tlMnuItemExpandAll.Name = "tlMnuItemExpandAll";
         this.tlMnuItemExpandAll.Size = new System.Drawing.Size(326, 26);
         this.tlMnuItemExpandAll.Text = "Expand all Blocks";
         this.tlMnuItemExpandAll.Click += new System.EventHandler(this.tlMnuItemExpandAll_Click);
         // 
         // tlMnuItemCollapseAll
         // 
         this.tlMnuItemCollapseAll.Name = "tlMnuItemCollapseAll";
         this.tlMnuItemCollapseAll.Size = new System.Drawing.Size(326, 26);
         this.tlMnuItemCollapseAll.Text = "Collapse All Blocks";
         this.tlMnuItemCollapseAll.Click += new System.EventHandler(this.tlMnuItemCollapseAll_Click);
         // 
         // toolStripSeparator7
         // 
         this.toolStripSeparator7.Name = "toolStripSeparator7";
         this.toolStripSeparator7.Size = new System.Drawing.Size(166, 6);
         // 
         // mnuItemMarkdown
         // 
         this.mnuItemMarkdown.Checked = true;
         this.mnuItemMarkdown.CheckState = System.Windows.Forms.CheckState.Checked;
         this.mnuItemMarkdown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuItemMarkdownSupport});
         this.mnuItemMarkdown.Name = "mnuItemMarkdown";
         this.mnuItemMarkdown.Size = new System.Drawing.Size(169, 26);
         this.mnuItemMarkdown.Text = "Markdown";
         // 
         // mnuItemMarkdownSupport
         // 
         this.mnuItemMarkdownSupport.Checked = true;
         this.mnuItemMarkdownSupport.CheckOnClick = true;
         this.mnuItemMarkdownSupport.CheckState = System.Windows.Forms.CheckState.Checked;
         this.mnuItemMarkdownSupport.Name = "mnuItemMarkdownSupport";
         this.mnuItemMarkdownSupport.Size = new System.Drawing.Size(192, 26);
         this.mnuItemMarkdownSupport.Text = "Enable support";
         this.mnuItemMarkdownSupport.CheckStateChanged += new System.EventHandler(this.mnuItemMarkdownSupport_CheckStateChanged);
         // 
         // mnuItemMooText
         // 
         this.mnuItemMooText.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuItemMooTextColor});
         this.mnuItemMooText.Name = "mnuItemMooText";
         this.mnuItemMooText.Size = new System.Drawing.Size(169, 26);
         this.mnuItemMooText.Text = "Moo Text";
         // 
         // mnuItemMooTextColor
         // 
         this.mnuItemMooTextColor.Checked = true;
         this.mnuItemMooTextColor.CheckOnClick = true;
         this.mnuItemMooTextColor.CheckState = System.Windows.Forms.CheckState.Checked;
         this.mnuItemMooTextColor.Name = "mnuItemMooTextColor";
         this.mnuItemMooTextColor.Size = new System.Drawing.Size(230, 26);
         this.mnuItemMooTextColor.Text = "Enable color support";
         this.mnuItemMooTextColor.CheckStateChanged += new System.EventHandler(this.mnuItemMooTextColor_CheckStateChanged);
         // 
         // mnuItemView
         // 
         this.mnuItemView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuItemZoomIn,
            this.mnuItemZoomOut,
            this.toolStripSeparator6,
            this.mnuItemWordWrap,
            this.mnuItemShowLineNumbers,
            this.mnuItemIndentationGuides,
            this.mnuItemShowPreviewPane});
         this.mnuItemView.Name = "mnuItemView";
         this.mnuItemView.Size = new System.Drawing.Size(55, 24);
         this.mnuItemView.Text = "&View";
         // 
         // mnuItemZoomIn
         // 
         this.mnuItemZoomIn.Name = "mnuItemZoomIn";
         this.mnuItemZoomIn.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Oemplus)));
         this.mnuItemZoomIn.Size = new System.Drawing.Size(271, 26);
         this.mnuItemZoomIn.Text = "Zoom in";
         this.mnuItemZoomIn.Click += new System.EventHandler(this.mnuItemZoomIn_Click);
         // 
         // mnuItemZoomOut
         // 
         this.mnuItemZoomOut.Name = "mnuItemZoomOut";
         this.mnuItemZoomOut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.OemMinus)));
         this.mnuItemZoomOut.Size = new System.Drawing.Size(271, 26);
         this.mnuItemZoomOut.Text = "Zoom out";
         this.mnuItemZoomOut.Click += new System.EventHandler(this.mnuItemZoomOut_Click);
         // 
         // toolStripSeparator6
         // 
         this.toolStripSeparator6.Name = "toolStripSeparator6";
         this.toolStripSeparator6.Size = new System.Drawing.Size(268, 6);
         // 
         // mnuItemWordWrap
         // 
         this.mnuItemWordWrap.Checked = true;
         this.mnuItemWordWrap.CheckOnClick = true;
         this.mnuItemWordWrap.CheckState = System.Windows.Forms.CheckState.Checked;
         this.mnuItemWordWrap.Name = "mnuItemWordWrap";
         this.mnuItemWordWrap.Size = new System.Drawing.Size(271, 26);
         this.mnuItemWordWrap.Text = "Word wrap";
         this.mnuItemWordWrap.CheckStateChanged += new System.EventHandler(this.mnuItemWordWrap_CheckStateChanged);
         // 
         // mnuItemShowLineNumbers
         // 
         this.mnuItemShowLineNumbers.Checked = true;
         this.mnuItemShowLineNumbers.CheckOnClick = true;
         this.mnuItemShowLineNumbers.CheckState = System.Windows.Forms.CheckState.Checked;
         this.mnuItemShowLineNumbers.Name = "mnuItemShowLineNumbers";
         this.mnuItemShowLineNumbers.Size = new System.Drawing.Size(271, 26);
         this.mnuItemShowLineNumbers.Text = "Show line numbers";
         this.mnuItemShowLineNumbers.CheckStateChanged += new System.EventHandler(this.mnuItemShowLineNumbers_CheckStateChanged);
         // 
         // mnuItemIndentationGuides
         // 
         this.mnuItemIndentationGuides.Checked = true;
         this.mnuItemIndentationGuides.CheckOnClick = true;
         this.mnuItemIndentationGuides.CheckState = System.Windows.Forms.CheckState.Checked;
         this.mnuItemIndentationGuides.Name = "mnuItemIndentationGuides";
         this.mnuItemIndentationGuides.Size = new System.Drawing.Size(271, 26);
         this.mnuItemIndentationGuides.Text = "Indentation guide lines";
         this.mnuItemIndentationGuides.CheckStateChanged += new System.EventHandler(this.mnuItemIndentationGuides_CheckStateChanged);
         // 
         // mnuItemShowPreviewPane
         // 
         this.mnuItemShowPreviewPane.Checked = true;
         this.mnuItemShowPreviewPane.CheckOnClick = true;
         this.mnuItemShowPreviewPane.CheckState = System.Windows.Forms.CheckState.Checked;
         this.mnuItemShowPreviewPane.Name = "mnuItemShowPreviewPane";
         this.mnuItemShowPreviewPane.Size = new System.Drawing.Size(271, 26);
         this.mnuItemShowPreviewPane.Text = "Show Preview Pane";
         this.mnuItemShowPreviewPane.CheckStateChanged += new System.EventHandler(this.mnuItemShowPreviewPane_CheckStateChanged);
         // 
         // mnuItemTerminal
         // 
         this.mnuItemTerminal.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuItemWorldManager,
            this.tlMnuItemOpenConnection,
            this.mnuItemCloseConnection,
            this.toolStripSeparator5,
            this.mnuItemEchoCommands,
            this.mnuItemEnableColor,
            this.mnuItemEnableBlinking,
            this.mnuItemEnableAudio});
         this.mnuItemTerminal.Name = "mnuItemTerminal";
         this.mnuItemTerminal.Size = new System.Drawing.Size(80, 24);
         this.mnuItemTerminal.Text = "&Terminal";
         // 
         // mnuItemWorldManager
         // 
         this.mnuItemWorldManager.Name = "mnuItemWorldManager";
         this.mnuItemWorldManager.Size = new System.Drawing.Size(298, 26);
         this.mnuItemWorldManager.Text = "World Manager";
         this.mnuItemWorldManager.Click += new System.EventHandler(this.mnuItemWorldManager_Click);
         // 
         // tlMnuItemOpenConnection
         // 
         this.tlMnuItemOpenConnection.Name = "tlMnuItemOpenConnection";
         this.tlMnuItemOpenConnection.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.O)));
         this.tlMnuItemOpenConnection.Size = new System.Drawing.Size(298, 26);
         this.tlMnuItemOpenConnection.Text = "Open connection";
         this.tlMnuItemOpenConnection.Click += new System.EventHandler(this.tlMnuItemOpenConnection_Click);
         // 
         // mnuItemCloseConnection
         // 
         this.mnuItemCloseConnection.Name = "mnuItemCloseConnection";
         this.mnuItemCloseConnection.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.D)));
         this.mnuItemCloseConnection.Size = new System.Drawing.Size(298, 26);
         this.mnuItemCloseConnection.Text = "Close connection";
         this.mnuItemCloseConnection.Click += new System.EventHandler(this.tlMnuItemCloseConnection_Click);
         // 
         // toolStripSeparator5
         // 
         this.toolStripSeparator5.Name = "toolStripSeparator5";
         this.toolStripSeparator5.Size = new System.Drawing.Size(295, 6);
         // 
         // mnuItemEchoCommands
         // 
         this.mnuItemEchoCommands.Checked = true;
         this.mnuItemEchoCommands.CheckOnClick = true;
         this.mnuItemEchoCommands.CheckState = System.Windows.Forms.CheckState.Checked;
         this.mnuItemEchoCommands.Name = "mnuItemEchoCommands";
         this.mnuItemEchoCommands.Size = new System.Drawing.Size(298, 26);
         this.mnuItemEchoCommands.Text = "Echo commands";
         this.mnuItemEchoCommands.CheckStateChanged += new System.EventHandler(this.mnuItemEchoCommands_CheckStateChanged);
         // 
         // mnuItemEnableColor
         // 
         this.mnuItemEnableColor.Checked = true;
         this.mnuItemEnableColor.CheckOnClick = true;
         this.mnuItemEnableColor.CheckState = System.Windows.Forms.CheckState.Checked;
         this.mnuItemEnableColor.Name = "mnuItemEnableColor";
         this.mnuItemEnableColor.Size = new System.Drawing.Size(298, 26);
         this.mnuItemEnableColor.Text = "Enable color";
         this.mnuItemEnableColor.CheckStateChanged += new System.EventHandler(this.mnuItemEnableColor_CheckStateChanged);
         // 
         // mnuItemEnableBlinking
         // 
         this.mnuItemEnableBlinking.Checked = true;
         this.mnuItemEnableBlinking.CheckOnClick = true;
         this.mnuItemEnableBlinking.CheckState = System.Windows.Forms.CheckState.Checked;
         this.mnuItemEnableBlinking.Name = "mnuItemEnableBlinking";
         this.mnuItemEnableBlinking.Size = new System.Drawing.Size(298, 26);
         this.mnuItemEnableBlinking.Text = "Enable blinking";
         this.mnuItemEnableBlinking.CheckStateChanged += new System.EventHandler(this.mnuItemEnableBlinking_CheckStateChanged);
         // 
         // mnuItemEnableAudio
         // 
         this.mnuItemEnableAudio.Checked = true;
         this.mnuItemEnableAudio.CheckOnClick = true;
         this.mnuItemEnableAudio.CheckState = System.Windows.Forms.CheckState.Checked;
         this.mnuItemEnableAudio.Name = "mnuItemEnableAudio";
         this.mnuItemEnableAudio.Size = new System.Drawing.Size(298, 26);
         this.mnuItemEnableAudio.Text = "Enable audio";
         this.mnuItemEnableAudio.CheckStateChanged += new System.EventHandler(this.mnuItemEnableAudio_CheckStateChanged);
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
         // mnuItemWindow
         // 
         this.mnuItemWindow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuItemPrevEditor,
            this.mnuItemPrevTerminal});
         this.mnuItemWindow.Name = "mnuItemWindow";
         this.mnuItemWindow.Size = new System.Drawing.Size(78, 24);
         this.mnuItemWindow.Text = "Window";
         // 
         // mnuItemPrevEditor
         // 
         this.mnuItemPrevEditor.Name = "mnuItemPrevEditor";
         this.mnuItemPrevEditor.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
         this.mnuItemPrevEditor.Size = new System.Drawing.Size(258, 26);
         this.mnuItemPrevEditor.Text = "Previous Editor";
         this.mnuItemPrevEditor.Click += new System.EventHandler(this.mnuItemPrevEditor_Click);
         // 
         // mnuItemPrevTerminal
         // 
         this.mnuItemPrevTerminal.Name = "mnuItemPrevTerminal";
         this.mnuItemPrevTerminal.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
         this.mnuItemPrevTerminal.Size = new System.Drawing.Size(258, 26);
         this.mnuItemPrevTerminal.Text = "Previous Terminal";
         this.mnuItemPrevTerminal.Click += new System.EventHandler(this.mnuItemPrevTerminal_Click);
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
         this.tlLblColumn.Name = "tlLblColumn";
         this.tlLblColumn.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
         this.tlLblColumn.Size = new System.Drawing.Size(75, 20);
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
         this.kryptonDockingManager.DefaultCloseRequest = Krypton.Docking.DockingCloseRequest.RemovePageAndDispose;
         this.kryptonDockingManager.PageCloseRequest += new System.EventHandler<Krypton.Docking.CloseRequestEventArgs>(this.kryptonDockingManager_PageCloseRequest);
         // 
         // kryptonPalette1
         // 
         this.kryptonPalette1.BasePaletteMode = Krypton.Toolkit.PaletteMode.Office365BlackDarkMode;
         this.kryptonPalette1.TabStyles.TabCommon.StateCommon.Back.Color1 = System.Drawing.Color.DimGray;
         this.kryptonPalette1.TabStyles.TabCommon.StateCommon.Back.Color2 = System.Drawing.Color.Gray;
         this.kryptonPalette1.TabStyles.TabCommon.StateSelected.Back.Color1 = System.Drawing.Color.DarkGray;
         this.kryptonPalette1.TabStyles.TabCommon.StateSelected.Back.Color2 = System.Drawing.Color.Silver;
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
         this.kryptonDockableWorkspace.PageCloseClicked += new System.EventHandler<Krypton.Docking.UniqueNameEventArgs>(this.kryptonDockableWorkspace_PageCloseClicked);
         this.kryptonDockableWorkspace.ActivePageChanged += new System.EventHandler<Krypton.Workspace.ActivePageChangedEventArgs>(this.kryptonDockableWorkspace_ActivePageChanged);
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
        private ToolStripMenuItem mnuItemSaveAsFile;
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
        private KryptonDockingManager kryptonDockingManager;
        private Krypton.Toolkit.KryptonManager kryptonManager;
        private Krypton.Toolkit.KryptonPanel kryptonPanel;
        private KryptonDockableWorkspace kryptonDockableWorkspace;
        private ToolStripMenuItem tlMnuNew;
        private ToolStripMenuItem mnuItemFileSave;
        private ToolStripMenuItem tlMnuItemClose;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem mnuItemBookmarks;
        private ToolStripMenuItem tlMnuItemToggleBookmark;
        private ToolStripMenuItem tlMnuItemNextBookmark;
        private ToolStripMenuItem tlMnuItemPrevBookmark;
        private ToolStripMenuItem mnuItemFolding;
        private ToolStripMenuItem tlMnuItemToggleFolding;
        private ToolStripMenuItem tlMnuItemExpandAll;
        private ToolStripMenuItem tlMnuItemCollapseAll;
        private ToolStripMenuItem mnuItemTerminal;
        private ToolStripMenuItem tlMnuItemOpenConnection;
        private ToolStripMenuItem mnuItemCloseConnection;
        private ToolStripMenuItem mnuItemUpload;
        private ToolStripMenuItem mnuItemWindow;
        private ToolStripMenuItem mnuItemPrevEditor;
        private ToolStripMenuItem mnuItemPrevTerminal;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem mnuItemEchoCommands;
        private ToolStripMenuItem mnuItemView;
        private ToolStripMenuItem mnuItemWordWrap;
        private ToolStripMenuItem mnuItemShowLineNumbers;
        private ToolStripMenuItem mnuItemEnableCodeFolding;
        private ToolStripMenuItem mnuItemIndentationGuides;
        private ToolStripMenuItem mnuItemZoomIn;
        private ToolStripMenuItem mnuItemZoomOut;
        private ToolStripSeparator toolStripSeparator6;
        private Krypton.Toolkit.KryptonPalette kryptonPalette1;
        private ToolStripMenuItem mnuItemShowPreviewPane;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem mnuItemMarkdown;
        private ToolStripMenuItem mnuItemMarkdownSupport;
        private ToolStripMenuItem mnuItemMooText;
        private ToolStripMenuItem mnuItemMooTextColor;
        private ToolStripMenuItem mnuItemWorldManager;
        private ToolStripMenuItem mnuItemEnableColor;
        private ToolStripMenuItem mnuItemEnableBlinking;
        private ToolStripMenuItem mnuItemEnableAudio;
      private ToolStripMenuItem mnuItemUpload2;
   }
}
