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
         components = new System.ComponentModel.Container();
         var resources = new System.ComponentModel.ComponentResourceManager(typeof(Editor));
         menuStrip1 = new MenuStrip();
         fileToolStripMenuItem = new ToolStripMenuItem();
         tlMnuNew = new ToolStripMenuItem();
         mnuItemOpenFile = new ToolStripMenuItem();
         mnuItemLoadFile = new ToolStripMenuItem();
         mnuItemUpload = new ToolStripMenuItem();
         mnuItemUpload2 = new ToolStripMenuItem();
         mnuItemFileSave = new ToolStripMenuItem();
         mnuItemSaveAsFile = new ToolStripMenuItem();
         tlMnuItemClose = new ToolStripMenuItem();
         toolStripSeparator1 = new ToolStripSeparator();
         mnuItemExit = new ToolStripMenuItem();
         editToolStripMenuItem = new ToolStripMenuItem();
         mnuItemFormat = new ToolStripMenuItem();
         toolStripSeparator2 = new ToolStripSeparator();
         mnuItemCut = new ToolStripMenuItem();
         mnuItemCopy = new ToolStripMenuItem();
         mnuItemCutPaste = new ToolStripMenuItem();
         toolStripSeparator3 = new ToolStripSeparator();
         mnuItemFind = new ToolStripMenuItem();
         toolStripSeparator4 = new ToolStripSeparator();
         mnuItemBookmarks = new ToolStripMenuItem();
         tlMnuItemToggleBookmark = new ToolStripMenuItem();
         tlMnuItemNextBookmark = new ToolStripMenuItem();
         tlMnuItemPrevBookmark = new ToolStripMenuItem();
         mnuItemFolding = new ToolStripMenuItem();
         mnuItemEnableCodeFolding = new ToolStripMenuItem();
         tlMnuItemToggleFolding = new ToolStripMenuItem();
         tlMnuItemExpandAll = new ToolStripMenuItem();
         tlMnuItemCollapseAll = new ToolStripMenuItem();
         toolStripSeparator7 = new ToolStripSeparator();
         mnuItemMarkdown = new ToolStripMenuItem();
         mnuItemMarkdownSupport = new ToolStripMenuItem();
         mnuItemMooText = new ToolStripMenuItem();
         mnuItemMooTextColor = new ToolStripMenuItem();
         mnuItemView = new ToolStripMenuItem();
         mnuItemZoomIn = new ToolStripMenuItem();
         mnuItemZoomOut = new ToolStripMenuItem();
         toolStripSeparator6 = new ToolStripSeparator();
         mnuItemWordWrap = new ToolStripMenuItem();
         mnuItemShowLineNumbers = new ToolStripMenuItem();
         mnuItemIndentationGuides = new ToolStripMenuItem();
         mnuItemShowPreviewPane = new ToolStripMenuItem();
         toolStripSeparatorTheme = new ToolStripSeparator();
         mnuItemTheme = new ToolStripMenuItem();
         toolStripSeparatorOptions = new ToolStripSeparator();
         mnuItemOptions = new ToolStripMenuItem();
         mnuItemTerminal = new ToolStripMenuItem();
         mnuItemWorldManager = new ToolStripMenuItem();
         tlMnuItemOpenConnection = new ToolStripMenuItem();
         mnuItemCloseConnection = new ToolStripMenuItem();
         toolStripSeparator5 = new ToolStripSeparator();
         mnuItemEchoCommands = new ToolStripMenuItem();
         mnuItemEnableColor = new ToolStripMenuItem();
         mnuItemEnableBlinking = new ToolStripMenuItem();
         mnuItemEnableAudio = new ToolStripMenuItem();
         grammarToolStripMenuItem = new ToolStripMenuItem();
         tlMnuLanguageMoo = new ToolStripMenuItem();
         tlMnuLanguageTsMoo = new ToolStripMenuItem();
         tlMnuLanguageEdgeMoo = new ToolStripMenuItem();
         mnuItemWindow = new ToolStripMenuItem();
         mnuItemPrevEditor = new ToolStripMenuItem();
         mnuItemPrevTerminal = new ToolStripMenuItem();
         helpToolStripMenuItem = new ToolStripMenuItem();
         tlMnuHelp = new ToolStripMenuItem();
         statusStrip1 = new StatusStrip();
         tlLblLine = new ToolStripStatusLabel();
         tlStatusLine = new ToolStripStatusLabel();
         tlLblColumn = new ToolStripStatusLabel();
         tlStatusColumn = new ToolStripStatusLabel();
         openFileDialog = new OpenFileDialog();
         saveFileDialog = new SaveFileDialog();
         kryptonDockingManager = new KryptonDockingManager();
         kryptonManager = new Krypton.Toolkit.KryptonManager(components);
         kryptonPalette1 = new Krypton.Toolkit.KryptonCustomPaletteBase(components);
         kryptonPanel = new Krypton.Toolkit.KryptonPanel();
         kryptonDockableWorkspace = new KryptonDockableWorkspace();
         toolsToolStripMenuItem = new ToolStripMenuItem();
         flushEditorCacheToolStripMenuItem = new ToolStripMenuItem();
         menuStrip1.SuspendLayout();
         statusStrip1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)kryptonPanel).BeginInit();
         kryptonPanel.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)kryptonDockableWorkspace).BeginInit();
         SuspendLayout();
         // 
         // menuStrip1
         // 
         menuStrip1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
         menuStrip1.ImageScalingSize = new Size(20, 20);
         menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, mnuItemView, toolsToolStripMenuItem, mnuItemTerminal, grammarToolStripMenuItem, mnuItemWindow, helpToolStripMenuItem });
         menuStrip1.Location = new Point(0, 0);
         menuStrip1.Name = "menuStrip1";
         menuStrip1.Size = new Size(957, 24);
         menuStrip1.TabIndex = 0;
         menuStrip1.Text = "menuStrip1";
         // 
         // fileToolStripMenuItem
         // 
         fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tlMnuNew, mnuItemOpenFile, mnuItemLoadFile, mnuItemUpload, mnuItemUpload2, mnuItemFileSave, mnuItemSaveAsFile, tlMnuItemClose, toolStripSeparator1, mnuItemExit });
         fileToolStripMenuItem.Name = "fileToolStripMenuItem";
         fileToolStripMenuItem.Size = new Size(37, 20);
         fileToolStripMenuItem.Text = "&File";
         // 
         // tlMnuNew
         // 
         tlMnuNew.Name = "tlMnuNew";
         tlMnuNew.ShortcutKeys = Keys.Control | Keys.N;
         tlMnuNew.Size = new Size(184, 22);
         tlMnuNew.Text = "&New";
         tlMnuNew.Click += tlMnuNew_Click;
         // 
         // mnuItemOpenFile
         // 
         mnuItemOpenFile.Name = "mnuItemOpenFile";
         mnuItemOpenFile.ShortcutKeys = Keys.Control | Keys.O;
         mnuItemOpenFile.Size = new Size(184, 22);
         mnuItemOpenFile.Text = "&Open";
         mnuItemOpenFile.Click += mnuItemOpenFile_Click;
         //
         // mnuItemLoadFile
         //
         mnuItemLoadFile.Name = "mnuItemLoadFile";
         mnuItemLoadFile.Size = new Size(184, 22);
         mnuItemLoadFile.Text = "&Load…";
         mnuItemLoadFile.Click += mnuItemLoadFile_Click;
         //
         // mnuItemUpload
         // 
         mnuItemUpload.Name = "mnuItemUpload";
         mnuItemUpload.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
         mnuItemUpload.Size = new Size(184, 22);
         mnuItemUpload.Text = "Upload";
         mnuItemUpload.Click += mnuItemUpload_Click;
         // 
         // mnuItemUpload2
         // 
         mnuItemUpload2.Name = "mnuItemUpload2";
         mnuItemUpload2.ShortcutKeys = Keys.F7;
         mnuItemUpload2.Size = new Size(184, 22);
         mnuItemUpload2.Text = "Upload 2";
         mnuItemUpload2.Visible = false;
         mnuItemUpload2.Click += mnuItemUpload_Click;
         // 
         // mnuItemFileSave
         // 
         mnuItemFileSave.Name = "mnuItemFileSave";
         mnuItemFileSave.ShortcutKeys = Keys.Control | Keys.S;
         mnuItemFileSave.Size = new Size(184, 22);
         mnuItemFileSave.Text = "&Save";
         mnuItemFileSave.Click += mnuItemSave_Click;
         // 
         // mnuItemSaveAsFile
         // 
         mnuItemSaveAsFile.Name = "mnuItemSaveAsFile";
         mnuItemSaveAsFile.Size = new Size(184, 22);
         mnuItemSaveAsFile.Text = "Save &As";
         mnuItemSaveAsFile.Click += mnuItemSaveAsFile_Click;
         // 
         // tlMnuItemClose
         // 
         tlMnuItemClose.Name = "tlMnuItemClose";
         tlMnuItemClose.Size = new Size(184, 22);
         tlMnuItemClose.Text = "&Close";
         tlMnuItemClose.Click += tlMnuItemClose_Click;
         // 
         // toolStripSeparator1
         // 
         toolStripSeparator1.Name = "toolStripSeparator1";
         toolStripSeparator1.Size = new Size(181, 6);
         // 
         // mnuItemExit
         // 
         mnuItemExit.Name = "mnuItemExit";
         mnuItemExit.Size = new Size(184, 22);
         mnuItemExit.Text = "E&xit";
         mnuItemExit.Click += mnuItemExit_Click;
         // 
         // editToolStripMenuItem
         // 
         editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mnuItemFormat, toolStripSeparator2, mnuItemCut, mnuItemCopy, mnuItemCutPaste, toolStripSeparator3, mnuItemFind, toolStripSeparator4, mnuItemBookmarks, mnuItemFolding, toolStripSeparator7, mnuItemMarkdown, mnuItemMooText });
         editToolStripMenuItem.Name = "editToolStripMenuItem";
         editToolStripMenuItem.Size = new Size(39, 20);
         editToolStripMenuItem.Text = "&Edit";
         // 
         // mnuItemFormat
         // 
         mnuItemFormat.Name = "mnuItemFormat";
         mnuItemFormat.Size = new Size(137, 22);
         mnuItemFormat.Text = "Format";
         mnuItemFormat.Click += mnuItemFormat_Click;
         // 
         // toolStripSeparator2
         // 
         toolStripSeparator2.Name = "toolStripSeparator2";
         toolStripSeparator2.Size = new Size(134, 6);
         // 
         // mnuItemCut
         // 
         mnuItemCut.Name = "mnuItemCut";
         mnuItemCut.Size = new Size(137, 22);
         mnuItemCut.Text = "Cut";
         mnuItemCut.Click += mnuItemCut_Click;
         // 
         // mnuItemCopy
         // 
         mnuItemCopy.Name = "mnuItemCopy";
         mnuItemCopy.Size = new Size(137, 22);
         mnuItemCopy.Text = "Copy";
         mnuItemCopy.Click += mnuItemCopy_Click;
         // 
         // mnuItemCutPaste
         // 
         mnuItemCutPaste.Name = "mnuItemCutPaste";
         mnuItemCutPaste.Size = new Size(137, 22);
         mnuItemCutPaste.Text = "Paste";
         mnuItemCutPaste.Click += mnuItemCutPaste_Click;
         // 
         // toolStripSeparator3
         // 
         toolStripSeparator3.Name = "toolStripSeparator3";
         toolStripSeparator3.Size = new Size(134, 6);
         // 
         // mnuItemFind
         // 
         mnuItemFind.Name = "mnuItemFind";
         mnuItemFind.ShortcutKeys = Keys.Control | Keys.F;
         mnuItemFind.Size = new Size(137, 22);
         mnuItemFind.Text = "Find";
         mnuItemFind.Click += mnuItemFind_Click;
         // 
         // toolStripSeparator4
         // 
         toolStripSeparator4.Name = "toolStripSeparator4";
         toolStripSeparator4.Size = new Size(134, 6);
         // 
         // mnuItemBookmarks
         // 
         mnuItemBookmarks.DropDownItems.AddRange(new ToolStripItem[] { tlMnuItemToggleBookmark, tlMnuItemNextBookmark, tlMnuItemPrevBookmark });
         mnuItemBookmarks.Name = "mnuItemBookmarks";
         mnuItemBookmarks.Size = new Size(137, 22);
         mnuItemBookmarks.Text = "Bookmarks";
         // 
         // tlMnuItemToggleBookmark
         // 
         tlMnuItemToggleBookmark.Name = "tlMnuItemToggleBookmark";
         tlMnuItemToggleBookmark.ShortcutKeys = Keys.Control | Keys.K;
         tlMnuItemToggleBookmark.Size = new Size(257, 22);
         tlMnuItemToggleBookmark.Text = "Toggle Bookmark";
         tlMnuItemToggleBookmark.Click += tlMnuItemToggleBookmark_Click;
         // 
         // tlMnuItemNextBookmark
         // 
         tlMnuItemNextBookmark.Name = "tlMnuItemNextBookmark";
         tlMnuItemNextBookmark.ShortcutKeys = Keys.Control | Keys.Shift | Keys.Down;
         tlMnuItemNextBookmark.Size = new Size(257, 22);
         tlMnuItemNextBookmark.Text = "Next Bookmark";
         tlMnuItemNextBookmark.Click += tlMnuItemNextBookmark_Click;
         // 
         // tlMnuItemPrevBookmark
         // 
         tlMnuItemPrevBookmark.Name = "tlMnuItemPrevBookmark";
         tlMnuItemPrevBookmark.ShortcutKeys = Keys.Control | Keys.Shift | Keys.Up;
         tlMnuItemPrevBookmark.Size = new Size(257, 22);
         tlMnuItemPrevBookmark.Text = "Previous Bookmark";
         tlMnuItemPrevBookmark.Click += tlMnuItemPrevBookmark_Click;
         // 
         // mnuItemFolding
         // 
         mnuItemFolding.DropDownItems.AddRange(new ToolStripItem[] { mnuItemEnableCodeFolding, tlMnuItemToggleFolding, tlMnuItemExpandAll, tlMnuItemCollapseAll });
         mnuItemFolding.Name = "mnuItemFolding";
         mnuItemFolding.Size = new Size(137, 22);
         mnuItemFolding.Text = "Folding";
         // 
         // mnuItemEnableCodeFolding
         // 
         mnuItemEnableCodeFolding.Checked = true;
         mnuItemEnableCodeFolding.CheckOnClick = true;
         mnuItemEnableCodeFolding.CheckState = CheckState.Checked;
         mnuItemEnableCodeFolding.Name = "mnuItemEnableCodeFolding";
         mnuItemEnableCodeFolding.Size = new Size(260, 22);
         mnuItemEnableCodeFolding.Text = "Enable folding";
         mnuItemEnableCodeFolding.CheckStateChanged += mnuItemEnableCodeFolding_CheckStateChanged;
         // 
         // tlMnuItemToggleFolding
         // 
         tlMnuItemToggleFolding.Name = "tlMnuItemToggleFolding";
         tlMnuItemToggleFolding.ShortcutKeys = Keys.Control | Keys.Shift | Keys.Space;
         tlMnuItemToggleFolding.Size = new Size(260, 22);
         tlMnuItemToggleFolding.Text = "Expand/Collapse";
         tlMnuItemToggleFolding.Click += tlMnuItemToggleFolding_Click;
         // 
         // tlMnuItemExpandAll
         // 
         tlMnuItemExpandAll.Name = "tlMnuItemExpandAll";
         tlMnuItemExpandAll.Size = new Size(260, 22);
         tlMnuItemExpandAll.Text = "Expand all Blocks";
         tlMnuItemExpandAll.Click += tlMnuItemExpandAll_Click;
         // 
         // tlMnuItemCollapseAll
         // 
         tlMnuItemCollapseAll.Name = "tlMnuItemCollapseAll";
         tlMnuItemCollapseAll.Size = new Size(260, 22);
         tlMnuItemCollapseAll.Text = "Collapse All Blocks";
         tlMnuItemCollapseAll.Click += tlMnuItemCollapseAll_Click;
         // 
         // toolStripSeparator7
         // 
         toolStripSeparator7.Name = "toolStripSeparator7";
         toolStripSeparator7.Size = new Size(134, 6);
         // 
         // mnuItemMarkdown
         // 
         mnuItemMarkdown.Checked = true;
         mnuItemMarkdown.CheckState = CheckState.Checked;
         mnuItemMarkdown.DropDownItems.AddRange(new ToolStripItem[] { mnuItemMarkdownSupport });
         mnuItemMarkdown.Name = "mnuItemMarkdown";
         mnuItemMarkdown.Size = new Size(137, 22);
         mnuItemMarkdown.Text = "Markdown";
         // 
         // mnuItemMarkdownSupport
         // 
         mnuItemMarkdownSupport.Checked = true;
         mnuItemMarkdownSupport.CheckOnClick = true;
         mnuItemMarkdownSupport.CheckState = CheckState.Checked;
         mnuItemMarkdownSupport.Name = "mnuItemMarkdownSupport";
         mnuItemMarkdownSupport.Size = new Size(153, 22);
         mnuItemMarkdownSupport.Text = "Enable support";
         mnuItemMarkdownSupport.CheckStateChanged += mnuItemMarkdownSupport_CheckStateChanged;
         // 
         // mnuItemMooText
         // 
         mnuItemMooText.DropDownItems.AddRange(new ToolStripItem[] { mnuItemMooTextColor });
         mnuItemMooText.Name = "mnuItemMooText";
         mnuItemMooText.Size = new Size(137, 22);
         mnuItemMooText.Text = "Moo Text";
         // 
         // mnuItemMooTextColor
         // 
         mnuItemMooTextColor.Checked = true;
         mnuItemMooTextColor.CheckOnClick = true;
         mnuItemMooTextColor.CheckState = CheckState.Checked;
         mnuItemMooTextColor.Name = "mnuItemMooTextColor";
         mnuItemMooTextColor.Size = new Size(183, 22);
         mnuItemMooTextColor.Text = "Enable color support";
         mnuItemMooTextColor.CheckStateChanged += mnuItemMooTextColor_CheckStateChanged;
         // 
         // mnuItemView
         // 
         mnuItemView.DropDownItems.AddRange(new ToolStripItem[] { mnuItemZoomIn, mnuItemZoomOut, toolStripSeparator6, mnuItemWordWrap, mnuItemShowLineNumbers, mnuItemIndentationGuides, mnuItemShowPreviewPane, toolStripSeparatorTheme, mnuItemTheme });
         mnuItemView.Name = "mnuItemView";
         mnuItemView.Size = new Size(44, 20);
         mnuItemView.Text = "&View";
         // 
         // mnuItemZoomIn
         // 
         mnuItemZoomIn.Name = "mnuItemZoomIn";
         mnuItemZoomIn.ShortcutKeyDisplayString = "Ctrl+Plus";
         mnuItemZoomIn.ShortcutKeys = Keys.Control | Keys.Oemplus;
         mnuItemZoomIn.Size = new Size(195, 22);
         mnuItemZoomIn.Text = "Zoom in";
         mnuItemZoomIn.Click += mnuItemZoomIn_Click;
         // 
         // mnuItemZoomOut
         // 
         mnuItemZoomOut.Name = "mnuItemZoomOut";
         mnuItemZoomOut.ShortcutKeyDisplayString = "Ctrl+Minus";
         mnuItemZoomOut.ShortcutKeys = Keys.Control | Keys.OemMinus;
         mnuItemZoomOut.Size = new Size(195, 22);
         mnuItemZoomOut.Text = "Zoom out";
         mnuItemZoomOut.Click += mnuItemZoomOut_Click;
         // 
         // toolStripSeparator6
         // 
         toolStripSeparator6.Name = "toolStripSeparator6";
         toolStripSeparator6.Size = new Size(192, 6);
         // 
         // mnuItemWordWrap
         // 
         mnuItemWordWrap.Checked = true;
         mnuItemWordWrap.CheckOnClick = true;
         mnuItemWordWrap.CheckState = CheckState.Checked;
         mnuItemWordWrap.Name = "mnuItemWordWrap";
         mnuItemWordWrap.Size = new Size(195, 22);
         mnuItemWordWrap.Text = "Word wrap";
         mnuItemWordWrap.CheckStateChanged += mnuItemWordWrap_CheckStateChanged;
         // 
         // mnuItemShowLineNumbers
         // 
         mnuItemShowLineNumbers.Checked = true;
         mnuItemShowLineNumbers.CheckOnClick = true;
         mnuItemShowLineNumbers.CheckState = CheckState.Checked;
         mnuItemShowLineNumbers.Name = "mnuItemShowLineNumbers";
         mnuItemShowLineNumbers.Size = new Size(195, 22);
         mnuItemShowLineNumbers.Text = "Show line numbers";
         mnuItemShowLineNumbers.CheckStateChanged += mnuItemShowLineNumbers_CheckStateChanged;
         // 
         // mnuItemIndentationGuides
         // 
         mnuItemIndentationGuides.Checked = true;
         mnuItemIndentationGuides.CheckOnClick = true;
         mnuItemIndentationGuides.CheckState = CheckState.Checked;
         mnuItemIndentationGuides.Name = "mnuItemIndentationGuides";
         mnuItemIndentationGuides.Size = new Size(195, 22);
         mnuItemIndentationGuides.Text = "Indentation guide lines";
         mnuItemIndentationGuides.CheckStateChanged += mnuItemIndentationGuides_CheckStateChanged;
         // 
         // mnuItemShowPreviewPane
         // 
         mnuItemShowPreviewPane.Checked = true;
         mnuItemShowPreviewPane.CheckOnClick = true;
         mnuItemShowPreviewPane.CheckState = CheckState.Checked;
         mnuItemShowPreviewPane.Name = "mnuItemShowPreviewPane";
         mnuItemShowPreviewPane.Size = new Size(195, 22);
         mnuItemShowPreviewPane.Text = "Show Preview Pane";
         mnuItemShowPreviewPane.CheckStateChanged += mnuItemShowPreviewPane_CheckStateChanged;
         // 
         // toolStripSeparatorTheme
         // 
         toolStripSeparatorTheme.Name = "toolStripSeparatorTheme";
         toolStripSeparatorTheme.Size = new Size(192, 6);
         // 
         // mnuItemTheme
         // 
         mnuItemTheme.Name = "mnuItemTheme";
         mnuItemTheme.Size = new Size(195, 22);
         mnuItemTheme.Text = "Theme…";
         mnuItemTheme.Click += mnuItemTheme_Click;
         // 
         // toolStripSeparatorOptions
         // 
         toolStripSeparatorOptions.Name = "toolStripSeparatorOptions";
         toolStripSeparatorOptions.Size = new Size(192, 6);
         // 
         // mnuItemOptions
         // 
         mnuItemOptions.Name = "mnuItemOptions";
         mnuItemOptions.Size = new Size(195, 22);
         mnuItemOptions.Text = "Options…";
         mnuItemOptions.Click += mnuItemOptions_Click;
         // 
         // mnuItemTerminal
         // 
         mnuItemTerminal.DropDownItems.AddRange(new ToolStripItem[] { mnuItemWorldManager, tlMnuItemOpenConnection, mnuItemCloseConnection, toolStripSeparator5, mnuItemEchoCommands, mnuItemEnableColor, mnuItemEnableBlinking, mnuItemEnableAudio });
         mnuItemTerminal.Name = "mnuItemTerminal";
         mnuItemTerminal.Size = new Size(64, 20);
         mnuItemTerminal.Text = "&Terminal";
         // 
         // mnuItemWorldManager
         // 
         mnuItemWorldManager.Name = "mnuItemWorldManager";
         mnuItemWorldManager.Size = new Size(241, 22);
         mnuItemWorldManager.Text = "World Manager";
         mnuItemWorldManager.Click += mnuItemWorldManager_Click;
         // 
         // tlMnuItemOpenConnection
         // 
         tlMnuItemOpenConnection.Name = "tlMnuItemOpenConnection";
         tlMnuItemOpenConnection.ShortcutKeys = Keys.Control | Keys.Shift | Keys.O;
         tlMnuItemOpenConnection.Size = new Size(241, 22);
         tlMnuItemOpenConnection.Text = "Open connection";
         tlMnuItemOpenConnection.Click += tlMnuItemOpenConnection_Click;
         // 
         // mnuItemCloseConnection
         // 
         mnuItemCloseConnection.Name = "mnuItemCloseConnection";
         mnuItemCloseConnection.ShortcutKeys = Keys.Control | Keys.Shift | Keys.D;
         mnuItemCloseConnection.Size = new Size(241, 22);
         mnuItemCloseConnection.Text = "Close connection";
         mnuItemCloseConnection.Click += tlMnuItemCloseConnection_Click;
         // 
         // toolStripSeparator5
         // 
         toolStripSeparator5.Name = "toolStripSeparator5";
         toolStripSeparator5.Size = new Size(238, 6);
         // 
         // mnuItemEchoCommands
         // 
         mnuItemEchoCommands.Checked = true;
         mnuItemEchoCommands.CheckOnClick = true;
         mnuItemEchoCommands.CheckState = CheckState.Checked;
         mnuItemEchoCommands.Name = "mnuItemEchoCommands";
         mnuItemEchoCommands.Size = new Size(241, 22);
         mnuItemEchoCommands.Text = "Echo commands";
         mnuItemEchoCommands.CheckStateChanged += mnuItemEchoCommands_CheckStateChanged;
         // 
         // mnuItemEnableColor
         // 
         mnuItemEnableColor.Checked = true;
         mnuItemEnableColor.CheckOnClick = true;
         mnuItemEnableColor.CheckState = CheckState.Checked;
         mnuItemEnableColor.Name = "mnuItemEnableColor";
         mnuItemEnableColor.Size = new Size(241, 22);
         mnuItemEnableColor.Text = "Enable color";
         mnuItemEnableColor.CheckStateChanged += mnuItemEnableColor_CheckStateChanged;
         // 
         // mnuItemEnableBlinking
         // 
         mnuItemEnableBlinking.Checked = true;
         mnuItemEnableBlinking.CheckOnClick = true;
         mnuItemEnableBlinking.CheckState = CheckState.Checked;
         mnuItemEnableBlinking.Name = "mnuItemEnableBlinking";
         mnuItemEnableBlinking.Size = new Size(241, 22);
         mnuItemEnableBlinking.Text = "Enable blinking";
         mnuItemEnableBlinking.CheckStateChanged += mnuItemEnableBlinking_CheckStateChanged;
         // 
         // mnuItemEnableAudio
         // 
         mnuItemEnableAudio.Checked = true;
         mnuItemEnableAudio.CheckOnClick = true;
         mnuItemEnableAudio.CheckState = CheckState.Checked;
         mnuItemEnableAudio.Name = "mnuItemEnableAudio";
         mnuItemEnableAudio.Size = new Size(241, 22);
         mnuItemEnableAudio.Text = "Enable audio";
         mnuItemEnableAudio.CheckStateChanged += mnuItemEnableAudio_CheckStateChanged;
         // 
         // grammarToolStripMenuItem
         // 
         grammarToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tlMnuLanguageMoo, tlMnuLanguageTsMoo, tlMnuLanguageEdgeMoo });
         grammarToolStripMenuItem.Name = "grammarToolStripMenuItem";
         grammarToolStripMenuItem.Size = new Size(108, 20);
         grammarToolStripMenuItem.Text = "Grammar Dialect";
         // 
         // tlMnuLanguageMoo
         // 
         tlMnuLanguageMoo.Name = "tlMnuLanguageMoo";
         tlMnuLanguageMoo.Size = new Size(142, 22);
         tlMnuLanguageMoo.Text = "LambdaMoo";
         tlMnuLanguageMoo.Click += tlMnuLanguageMoo_Click;
         // 
         // tlMnuLanguageTsMoo
         // 
         tlMnuLanguageTsMoo.Name = "tlMnuLanguageTsMoo";
         tlMnuLanguageTsMoo.Size = new Size(142, 22);
         tlMnuLanguageTsMoo.Text = "ToastStunt";
         tlMnuLanguageTsMoo.Click += tlMnuLanguageTsMoo_Click;
         // 
         // tlMnuLanguageEdgeMoo
         // 
         tlMnuLanguageEdgeMoo.Checked = true;
         tlMnuLanguageEdgeMoo.CheckState = CheckState.Checked;
         tlMnuLanguageEdgeMoo.Name = "tlMnuLanguageEdgeMoo";
         tlMnuLanguageEdgeMoo.Size = new Size(142, 22);
         tlMnuLanguageEdgeMoo.Text = "Edgerunner";
         tlMnuLanguageEdgeMoo.Click += tlMnuLanguageEdgeMoo_Click;
         // 
         // mnuItemWindow
         // 
         mnuItemWindow.DropDownItems.AddRange(new ToolStripItem[] { mnuItemPrevEditor, mnuItemPrevTerminal });
         mnuItemWindow.Name = "mnuItemWindow";
         mnuItemWindow.Size = new Size(63, 20);
         mnuItemWindow.Text = "Window";
         // 
         // mnuItemPrevEditor
         // 
         mnuItemPrevEditor.Name = "mnuItemPrevEditor";
         mnuItemPrevEditor.ShortcutKeys = Keys.Control | Keys.E;
         mnuItemPrevEditor.Size = new Size(207, 22);
         mnuItemPrevEditor.Text = "Previous Editor";
         mnuItemPrevEditor.Click += mnuItemPrevEditor_Click;
         // 
         // mnuItemPrevTerminal
         // 
         mnuItemPrevTerminal.Name = "mnuItemPrevTerminal";
         mnuItemPrevTerminal.ShortcutKeys = Keys.Control | Keys.T;
         mnuItemPrevTerminal.Size = new Size(207, 22);
         mnuItemPrevTerminal.Text = "Previous Terminal";
         mnuItemPrevTerminal.Click += mnuItemPrevTerminal_Click;
         // 
         // helpToolStripMenuItem
         // 
         helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tlMnuHelp });
         helpToolStripMenuItem.Name = "helpToolStripMenuItem";
         helpToolStripMenuItem.Size = new Size(44, 20);
         helpToolStripMenuItem.Text = "Help";
         // 
         // tlMnuHelp
         // 
         tlMnuHelp.Name = "tlMnuHelp";
         tlMnuHelp.Size = new Size(107, 22);
         tlMnuHelp.Text = "About";
         tlMnuHelp.Click += tlMnuHelp_Click;
         // 
         // statusStrip1
         // 
         statusStrip1.ImageScalingSize = new Size(20, 20);
         statusStrip1.Items.AddRange(new ToolStripItem[] { tlLblLine, tlStatusLine, tlLblColumn, tlStatusColumn });
         statusStrip1.Location = new Point(0, 745);
         statusStrip1.Name = "statusStrip1";
         statusStrip1.Size = new Size(957, 22);
         statusStrip1.TabIndex = 2;
         statusStrip1.Text = "statusStrip1";
         // 
         // tlLblLine
         // 
         tlLblLine.Name = "tlLblLine";
         tlLblLine.Size = new Size(29, 17);
         tlLblLine.Text = "Line";
         // 
         // tlStatusLine
         // 
         tlStatusLine.Name = "tlStatusLine";
         tlStatusLine.Size = new Size(13, 17);
         tlStatusLine.Text = "1";
         // 
         // tlLblColumn
         // 
         tlLblColumn.Name = "tlLblColumn";
         tlLblColumn.Padding = new Padding(15, 0, 0, 0);
         tlLblColumn.Size = new Size(65, 17);
         tlLblColumn.Text = "Column";
         // 
         // tlStatusColumn
         // 
         tlStatusColumn.Name = "tlStatusColumn";
         tlStatusColumn.Size = new Size(13, 17);
         tlStatusColumn.Text = "1";
         // 
         // kryptonDockingManager
         // 
         kryptonDockingManager.DefaultCloseRequest = DockingCloseRequest.RemovePageAndDispose;
         kryptonDockingManager.PageCloseRequest += kryptonDockingManager_PageCloseRequest;
         // 
         // kryptonPalette1
         // 
         kryptonPalette1.BaseFont = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
         kryptonPalette1.BaseFontSize = 9F;
         kryptonPalette1.BasePaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlackDarkMode;
         kryptonPalette1.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
         kryptonPalette1.TabStyles.TabCommon.StateCommon.Back.Color1 = Color.DimGray;
         kryptonPalette1.TabStyles.TabCommon.StateCommon.Back.Color2 = Color.Gray;
         kryptonPalette1.TabStyles.TabCommon.StateSelected.Back.Color1 = Color.DarkGray;
         kryptonPalette1.TabStyles.TabCommon.StateSelected.Back.Color2 = Color.Silver;
         kryptonPalette1.ThemeName = "";
         kryptonPalette1.UseKryptonFileDialogs = true;
         // 
         // kryptonPanel
         // 
         kryptonPanel.Controls.Add(kryptonDockableWorkspace);
         kryptonPanel.Dock = DockStyle.Fill;
         kryptonPanel.Location = new Point(0, 24);
         kryptonPanel.Name = "kryptonPanel";
         kryptonPanel.Size = new Size(957, 721);
         kryptonPanel.TabIndex = 5;
         // 
         // kryptonDockableWorkspace
         // 
         kryptonDockableWorkspace.ActivePage = null;
         kryptonDockableWorkspace.AutoHiddenHost = false;
         kryptonDockableWorkspace.CompactFlags = Krypton.Workspace.CompactFlags.RemoveEmptyCells | Krypton.Workspace.CompactFlags.RemoveEmptySequences | Krypton.Workspace.CompactFlags.PromoteLeafs;
         kryptonDockableWorkspace.ContainerBackStyle = Krypton.Toolkit.PaletteBackStyle.PanelClient;
         kryptonDockableWorkspace.Dock = DockStyle.Fill;
         kryptonDockableWorkspace.Location = new Point(0, 0);
         kryptonDockableWorkspace.Name = "kryptonDockableWorkspace";
         // 
         // 
         // 
         kryptonDockableWorkspace.Root.UniqueName = "1ca2dbc169f14bf7b554571d33e1fb83";
         kryptonDockableWorkspace.Root.WorkspaceControl = kryptonDockableWorkspace;
         kryptonDockableWorkspace.SeparatorStyle = Krypton.Toolkit.SeparatorStyle.LowProfile;
         kryptonDockableWorkspace.ShowMaximizeButton = false;
         kryptonDockableWorkspace.Size = new Size(957, 721);
         kryptonDockableWorkspace.SplitterWidth = 5;
         kryptonDockableWorkspace.TabIndex = 0;
         kryptonDockableWorkspace.TabStop = true;
         kryptonDockableWorkspace.PageCloseClicked += kryptonDockableWorkspace_PageCloseClicked;
         kryptonDockableWorkspace.ActivePageChanged += kryptonDockableWorkspace_ActivePageChanged;
         // 
         // toolsToolStripMenuItem
         // 
         toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { flushEditorCacheToolStripMenuItem, toolStripSeparatorOptions, mnuItemOptions });
         toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
         toolsToolStripMenuItem.Size = new Size(46, 20);
         toolsToolStripMenuItem.Text = "Tools";
         // 
         // flushEditorCacheToolStripMenuItem
         // 
         flushEditorCacheToolStripMenuItem.Name = "flushEditorCacheToolStripMenuItem";
         flushEditorCacheToolStripMenuItem.Size = new Size(180, 22);
         flushEditorCacheToolStripMenuItem.Text = "Flush Editor Cache";
         flushEditorCacheToolStripMenuItem.Click += flushEditorCacheToolStripMenuItem_Click;
         // 
         // Editor
         // 
         ClientSize = new Size(957, 767);
         Controls.Add(kryptonPanel);
         Controls.Add(menuStrip1);
         Controls.Add(statusStrip1);
         Icon = (Icon)resources.GetObject("$this.Icon");
         MainMenuStrip = menuStrip1;
         Name = "Editor";
         Text = "Moo Udditor - A Moo IDE";
         Load += Editor_Load;
         menuStrip1.ResumeLayout(false);
         menuStrip1.PerformLayout();
         statusStrip1.ResumeLayout(false);
         statusStrip1.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)kryptonPanel).EndInit();
         kryptonPanel.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)kryptonDockableWorkspace).EndInit();
         ResumeLayout(false);
         PerformLayout();
      }

      #endregion

      private MenuStrip menuStrip1;
      private ToolStripMenuItem fileToolStripMenuItem;
      private ToolStripMenuItem mnuItemOpenFile;
      private ToolStripMenuItem mnuItemLoadFile;
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
      private Krypton.Toolkit.KryptonCustomPaletteBase kryptonPalette1;
      private ToolStripMenuItem mnuItemShowPreviewPane;
      private ToolStripSeparator toolStripSeparatorTheme;
      private ToolStripMenuItem mnuItemTheme;
      private ToolStripSeparator toolStripSeparatorOptions;
      private ToolStripMenuItem mnuItemOptions;
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
      private ToolStripMenuItem toolsToolStripMenuItem;
      private ToolStripMenuItem flushEditorCacheToolStripMenuItem;
   }
}
