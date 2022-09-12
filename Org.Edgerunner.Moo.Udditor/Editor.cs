using Krypton.Docking;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Krypton.Navigator;
using FastColoredTextBoxNS;
using FastColoredTextBoxNS.Types;
using Krypton.Toolkit;
using Krypton.Workspace;
using Org.Edgerunner.ANTLR4.Tools.Common;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar.Errors;
using Org.Edgerunner.Moo.Editor;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Org.Edgerunner.Moo.Editor.Configuration;
using Org.Edgerunner.Moo.Editor.Controls;
using Org.Edgerunner.Moo.Udditor.Pages;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Org.Edgerunner.Moo.Common;
using Org.Edgerunner.Moo.Common.Cryptography;

namespace Org.Edgerunner.Moo.Udditor;

public partial class Editor : KryptonForm
{
   public Editor()
   {
      InitializeComponent();
      Errors = new SortedDictionary<string, List<ParseMessage>>();
      DefaultGrammarDialect = Settings.Instance.DefaultGrammarDialect;
      UpdateDialectMenu(Settings.Instance.DefaultGrammarDialect);
   }

   private void Editor_ParsingComplete(object sender, Moo.Editor.ParsingCompleteEventArgs e)
   {
      var key = e.Document.Id;
      if (e.ErrorMessages.Count == 0)
         Errors.Remove(key);
      else
         Errors[key] = e.ErrorMessages;
      UpdateParserErrors();
   }

   private void UpdateParserErrors()
   {
      var allErrors = new List<ParseMessage>();
      foreach (var eKey in Errors.Keys)
         allErrors.AddRange(Errors[eKey]);
      ErrorDisplay.PopulateErrors(allErrors);
   }

   private bool _WorldManagerEnabled;

   private ErrorDisplay ErrorDisplay { get; set; }

   private GrammarDialect DefaultGrammarDialect { get; set; }

   private KryptonDockingWorkspace Workspace { get; set; }

   private SortedDictionary<string, List<ParseMessage>> Errors { get; set; }

   private WindowManager WindowManager { get; set; }

   private ManagedPage CurrentPage { get; set; }

   private void SetDocumentGrammar(GrammarDialect grammarDialect)
   {
      if (CurrentPage is MooCodeEditorPage page)
         page.GrammarDialect = grammarDialect;
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

   private void EnableGrammarMenu(bool enabled)
   {
      grammarToolStripMenuItem.Enabled = enabled;
   }

   private void UpdateMenus()
   {
      var editPage = CurrentPage as MooEditorPage;
      var mooCodeEditorPage = CurrentPage as MooCodeEditorPage;
      var documentEditorPage = CurrentPage as MooDocumentEditorPage;
      var terminalPage = CurrentPage as TerminalPage;
      var isEditor = editPage != null;
      var isMooCodeEditor = mooCodeEditorPage != null;
      var isDocumentEditor = documentEditorPage != null;
      var isTerminal = terminalPage != null;
      grammarToolStripMenuItem.Enabled = isMooCodeEditor;
      mnuItemSaveAsFile.Enabled = isEditor;
      mnuItemFileSave.Enabled = isEditor;
      mnuItemFormat.Enabled = isMooCodeEditor;
      mnuItemBookmarks.Enabled = isMooCodeEditor;
      mnuItemFolding.Enabled = isMooCodeEditor;
      mnuItemCloseConnection.Enabled = isTerminal;
      mnuItemSend.Enabled = isEditor && editPage.CanUpload;
      mnuItemEchoCommands.Enabled = isTerminal;
      mnuItemWordWrap.Enabled = isTerminal || isEditor;
      mnuItemShowLineNumbers.Enabled = isEditor;
      mnuItemIndentationGuides.Enabled = isMooCodeEditor;
      mnuItemMarkdown.Enabled = isDocumentEditor;
      mnuItemMooText.Enabled = isDocumentEditor;
      mnuItemShowPreviewPane.Enabled = isDocumentEditor;
      mnuItemWorldManager.Enabled = _WorldManagerEnabled;
      UpdateEditMenu();
      UpdateTerminalMenu();
      UpdateViewMenu();
   }

   private void UpdateEditMenu()
   {
      if (CurrentPage is MooCodeEditorPage page)
         mnuItemEnableCodeFolding.CheckState = page.ShowTextBlockIndentationGuides ? CheckState.Checked : CheckState.Unchecked;
      if (CurrentPage is MooDocumentEditorPage documentPage)
      {
         mnuItemMarkdownSupport.CheckState =
             documentPage.Editor.EnableMarkdownProcessing ? CheckState.Checked : CheckState.Unchecked;
         mnuItemMooTextColor.CheckState = documentPage.Editor.EnableMooTextProcessing ? CheckState.Checked : CheckState.Unchecked;
      }
   }

   private void UpdateViewMenu()
   {
      var mooCodeEditorPage = CurrentPage as MooCodeEditorPage;
      var documentEditorPage = CurrentPage as MooDocumentEditorPage;
      var editorPage = CurrentPage as MooEditorPage;
      var terminalPage = CurrentPage as TerminalPage;
      var isMooCodeEditor = mooCodeEditorPage != null;
      var isEditor = editorPage != null;
      var isDocumentEditor = documentEditorPage != null;
      var isTerminal = terminalPage != null;
      mnuItemWordWrap.CheckState = (isEditor && editorPage.WordWrap) || (isTerminal && terminalPage.WordWrap)
          ? CheckState.Checked
          : CheckState.Unchecked;
      mnuItemShowLineNumbers.CheckState = isEditor && editorPage.ShowLineNumbers ? CheckState.Checked : CheckState.Unchecked;
      mnuItemIndentationGuides.CheckState =
          isMooCodeEditor && mooCodeEditorPage.ShowTextBlockIndentationGuides ? CheckState.Checked : CheckState.Unchecked;
      mnuItemShowPreviewPane.CheckState =
          isDocumentEditor && documentEditorPage.EnablePreview ? CheckState.Checked : CheckState.Unchecked;
   }

   void UpdateTerminalMenu()
   {
      var terminalPage = CurrentPage as TerminalPage;
      var isTerminal = terminalPage != null;
      mnuItemEchoCommands.CheckState = isTerminal && terminalPage.Terminal.EchoEnabled ? CheckState.Checked : CheckState.Unchecked;
   }

   private void Editor_Load(object sender, EventArgs e)
   {
      if (Settings.Instance.EditorDarkTheme)
         kryptonManager.GlobalPalette = kryptonPalette1;
      // Setup docking functionality
      Workspace = kryptonDockingManager.ManageWorkspace(kryptonDockableWorkspace);
      WindowManager = new WindowManager(Workspace, this);
      WindowManager.EditorCursorUpdated += WindowManager_EditorCursorUpdated;
      WindowManager.EditorParsingComplete += WindowManager_EditorParsingComplete;
      kryptonDockingManager.ManageControl("FooterControls", kryptonPanel);
      kryptonDockingManager.ManageFloating(this);

      var messageDisplay = WindowManager.CreateParserMessageDisplayPage();
      ErrorDisplay = messageDisplay.MessageDisplay;
      messageDisplay.DoubleClick += MessageDisplay_DoubleClick;
      ConfigureMasterKey();
      UpdateMenus();
      BuildTerminalShortcutMenu();
   }

   private void ConfigureMasterKey()
   {
      _WorldManagerEnabled = true;
      string key = String.Empty;
      try
      {
         KeyManager.RetrieveMasterKey(out key);
      }
      catch (Exception)
      {
      }
      if (string.IsNullOrEmpty(key))
      {
         var setup = new Setup();
         setup.StartPosition = FormStartPosition.CenterParent;
         if (setup.ShowDialog(this) != DialogResult.OK)
         {
            MessageBox.Show("Without a proper master key, the world manager will be disabled.", "World Manager Disabled");
            _WorldManagerEnabled = false;
         }
         else
         {
            _WorldManagerEnabled = true;
            key = Hash.Sha256(setup.Password);
            KeyManager.SaveMasterKey(key);
         }
      }
   }

   void BuildTerminalShortcutMenu()
   {
      while (mnuItemTerminal.DropDownItems.Count > 5)
         mnuItemTerminal.DropDownItems.RemoveAt(5);
      if (!_WorldManagerEnabled)
         return;

      var book = GetWorldsAddressBook();
      if (book.Worlds.Count(world => world.ShowAsMenuShortcut) != 0)
      {
         mnuItemTerminal.DropDownItems.Add(new ToolStripSeparator());
         foreach (var world in book.Worlds)
         {
            if (world.ShowAsMenuShortcut)
            {
               var item = new ToolStripMenuItem();
               item.Text = $"Open {world.Name}";
               item.Tag = world;
               item.Click += WorldShortcut_Click;
               mnuItemTerminal.DropDownItems.Add(item);
            }
         }
      }
   }

   private async void WorldShortcut_Click(object sender, EventArgs e)
   {
      if (sender is ToolStripMenuItem { Tag: WorldConfiguration world })
      {
         Debug.WriteLine($"World {world.Name} clicked");
         await OpenTerminalConnectionAsync(world);
      }
   }

   private void WindowManager_EditorParsingComplete(object sender, MooCodeEditorPage e)
   {
      if (CurrentPage == e)
      {
         var key = e.Document.Id;
         if (e.ParseErrors.Count == 0)
            Errors.Remove(key);
         else
            Errors[key] = e.ParseErrors;
         var allErrors = new List<ParseMessage>();
         foreach (var eKey in Errors.Keys)
            allErrors.AddRange(Errors[eKey]);
         ErrorDisplay.PopulateErrors(allErrors);
         e.Editor.Invalidate();
      }
   }

   private void WindowManager_EditorCursorUpdated(object sender, MooEditorPage e)
   {
      if (CurrentPage == e)
      {
         tlStatusLine.Text = ((e.SourceEditor?.Selection.Start.iLine + 1) ?? 1).ToString();
         tlStatusColumn.Text = ((e.SourceEditor?.Selection.Start.iChar + 1) ?? 1).ToString();
      }
   }

   private void MessageDisplay_DoubleClick(object sender, ParserMessageDoubleClickEventArgs e)
   {
      if (WindowManager.ShowPage(e.PageId) is MooCodeEditorPage page)
      {
         page.Editor.Selection =
             new TextSelectionRange(page.Editor, e.ColumnPosition, e.LineNumber, e.ColumnPosition, e.LineNumber);
         page.Editor.Focus();
         page.Editor.DoCaretVisible();
      }
   }

   private void mnuItemExit_Click(object sender, EventArgs e)
   {
      Application.Exit();
   }

   private void tlMnuNew_Click(object sender, EventArgs e)
   {
      var page = WindowManager.CreateMooCodeEditorPage(DefaultGrammarDialect);
      WindowManager.ShowPage(page);
   }

   private void mnuItemOpenFile_Click(object sender, EventArgs e)
   {
      openFileDialog.Multiselect = false;
      openFileDialog.DefaultExt = "moo";
      openFileDialog.Filter = @"Moo files (*.moo)|*.moo|Text files (*.txt)|*.txt|Markdown files (*.md)|*.md|All files (*.*)|*.*";
      openFileDialog.Title = "Please select a moo source file to open";
      if (openFileDialog.ShowDialog() == DialogResult.OK)
      {
         var path = openFileDialog.FileName;
         var page = WindowManager.CreateMooCodeEditorPage(DefaultGrammarDialect, path);
         WindowManager.ShowPage(page);
      }
   }
   private void mnuItemSave_Click(object sender, EventArgs e)
   {
      if (CurrentPage is MooEditorPage page)
      {
         if (!string.IsNullOrEmpty(page.Document.Path))
            page.SourceEditor.SaveToFile(page.Document.Path, Encoding.Default);
         else
         {
            saveFileDialog.DefaultExt = "moo";
            saveFileDialog.Filter = @"Moo files (*.moo)|*.moo|Text files (*.txt)|*.txt|Markdown files (*.md)|*.md|All files (*.*)|*.*";
            saveFileDialog.Title = "Please select a file name to save as";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
               var path = saveFileDialog.FileName;
               var name = Path.GetFileName(path);
               page.SourceEditor.SaveToFile(path, Encoding.Default);
               page.Document.Path = path;
               page.Document.Name = name;
               if (page is MooCodeEditorPage mooCodeEditorPage)
                  mooCodeEditorPage.ParseSourceCode();
            }
         }
         page.SourceEditor.Invalidate();
      }
   }

   private void mnuItemSaveAsFile_Click(object sender, EventArgs e)
   {
      if (CurrentPage is MooEditorPage page)
      {
         saveFileDialog.DefaultExt = "moo";
         saveFileDialog.Filter = @"Moo files (*.moo)|*.moo|Text files (*.txt)|*.txt|Markdown files (*.md)|*.md|All files (*.*)|*.*";
         saveFileDialog.Title = "Please select a file name to save as";
         if (saveFileDialog.ShowDialog() == DialogResult.OK)
         {
            var path = saveFileDialog.FileName;
            var name = Path.GetFileName(path);
            page.SourceEditor.SaveToFile(path, Encoding.Default);
            page.Document.Path = path;
            page.Document.Name = name;
            if (page is MooCodeEditorPage mooCodeEditorPage)
               mooCodeEditorPage.ParseSourceCode();
         }
      }
   }

   private void tlMnuItemClose_Click(object sender, EventArgs e)
   {
      if (CurrentPage != null)
         WindowManager.ClosePage(CurrentPage.UniqueName);
   }

   private void mnuItemFormat_Click(object sender, EventArgs e)
   {
      if (CurrentPage is MooCodeEditorPage page)
      {
         page.Editor.SuspendLayout();
         var current = page.Editor.Selection.Clone();
         page.Editor.SelectAll();
         page.Editor.DoAutoIndent();
         page.Editor.Selection = current;
         page.Editor.ResumeLayout();
      }
   }

   private void mnuItemCut_Click(object sender, EventArgs e)
   {
      // TODO: add support for different window types
      if (CurrentPage is MooEditorPage page)
         page.SourceEditor.Cut();
   }

   private void mnuItemCopy_Click(object sender, EventArgs e)
   {
      // TODO: add support for different window types
      if (CurrentPage is MooEditorPage page)
         page.SourceEditor.Copy();
   }

   private void mnuItemCutPaste_Click(object sender, EventArgs e)
   {
      // TODO: add support for different window types
      if (CurrentPage is MooEditorPage page)
         page.SourceEditor.Paste();
   }

   private void mnuItemFind_Click(object sender, EventArgs e)
   {
      if (CurrentPage is MooEditorPage editorPage)
         editorPage.SourceEditor.ShowFindDialog();
      else if (CurrentPage is TerminalPage terminalPage)
         terminalPage.Terminal.Output.ShowFindDialog();
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

   private void tlMnuItemToggleBookmark_Click(object sender, EventArgs e)
   {
      if (CurrentPage is MooCodeEditorPage page)
         page.Editor.BookmarkLine(page.Editor.Selection.Start.iLine);
   }

   private void tlMnuItemNextBookmark_Click(object sender, EventArgs e)
   {
      if (CurrentPage is MooCodeEditorPage page)
         page.Editor.GotoNextBookmark(page.Editor.Selection.Start.iLine);
   }

   private void tlMnuItemPrevBookmark_Click(object sender, EventArgs e)
   {
      if (CurrentPage is MooCodeEditorPage page)
         page.Editor.GotoPrevBookmark(page.Editor.Selection.Start.iLine);
   }

   private void tlMnuItemToggleFolding_Click(object sender, EventArgs e)
   {
      if (CurrentPage is MooCodeEditorPage page)
         page.Editor.ToggleFoldingBlock(page.Editor.Selection.Start.iLine);
   }

   private void tlMnuItemExpandAll_Click(object sender, EventArgs e)
   {
      if (CurrentPage is MooCodeEditorPage page)
         page.Editor.ExpandAllFoldingBlocks();
   }

   private void tlMnuItemCollapseAll_Click(object sender, EventArgs e)
   {
      if (CurrentPage is MooCodeEditorPage page)
         page.Editor.CollapseAllFoldingBlocks();
   }

   private async void tlMnuItemOpenConnection_Click(object sender, EventArgs e)
   {
      var prompt = new ConnectionInfoPrompt();
      prompt.StartPosition = FormStartPosition.CenterParent;
      var result = prompt.ShowDialog();
      if (result == DialogResult.OK)
      {
         var world = $"{prompt.HostAddress}:{prompt.HostPort}";
         await OpenTerminalConnectionAsync(prompt.HostAddress, prompt.HostPort, world, prompt.UseTls);
      }
   }

   private async Task OpenTerminalConnectionAsync(string host,int port, string world, bool useTls = false)
   {
      TerminalPage page = CurrentPage as TerminalPage;
      if (page == null || page.Terminal.IsConnected)
      {
         page = WindowManager.CreateTerminalPage(host);
      }
      else
      {
         page.Text = world;
         page.TextTitle = world;
         page.TextDescription = world;
      }

      WindowManager.ShowPage(page);
      try
      {
         page.Terminal.FocusOnInput();
         UpdateTerminalMenu();
         await page.Terminal.ConnectAsync(world, host, port, useTls).ConfigureAwait(true);
      }
      catch (Exception ex)
      {
         if (useTls && ex is IOException)
            MessageBox.Show("This host address may not support TLS", "Unable to connect");
         else
            MessageBox.Show(ex.Message, "Unable to connect");
      }
   }

   private async Task OpenTerminalConnectionAsync(WorldConfiguration world)
   {
      var userName = world.UserInfo.Name;
      var password = world.UserInfo.DecryptedPassword;
      if (world.UserInfo.PromptForCredentials)
         if (!PromptForCredentials(ref userName, ref password))
            return;

      TerminalPage page = CurrentPage as TerminalPage;
      if (page == null || page.Terminal.IsConnected)
      {
         page = WindowManager.CreateTerminalPage(world.Name);
      }
      else
      {
         page.Text = world.Name;
         page.TextTitle = world.Name;
         page.TextDescription = world.Name;
      }

      WindowManager.ShowPage(page);
      try
      {
         UpdateTerminalMenu();
         await page.Terminal.ConnectAsync(world.Name, world.HostAddress, world.PortNumber, world.UseTls).ConfigureAwait(true);
         page.Terminal.EchoEnabled = false;
         if (world.UserInfo.AutomaticallyLogin && !string.IsNullOrEmpty(userName))
         {
            var loginText = world.UserInfo.ConnectionString
                                 .Replace("%u", userName)
                                 .Replace("%p", password);
            page.Terminal.SendTextLine(loginText);
         }
         page.Terminal.EchoEnabled = world.EchoEnabled;
         page.Terminal.FocusOnInput();
      }
      catch (Exception ex)
      {
         if (world.UseTls && ex is IOException)
            MessageBox.Show("This host address may not support TLS", "Unable to connect");
         else
            MessageBox.Show(ex.Message, "Unable to connect");
      }
   }

   private bool PromptForCredentials(ref string userName, ref string password)
   {
      var prompt = new CredentialsPrompt();
      prompt.StartPosition = FormStartPosition.CenterParent;
      prompt.UserName = userName;
      prompt.Password = password;
      if (prompt.ShowDialog(this) != DialogResult.OK)
         return false;

      userName = prompt.UserName;
      password = prompt.Password;
      return true;
   }

   private void tlMnuItemCloseConnection_Click(object sender, EventArgs e)
   {
      if (CurrentPage is TerminalPage page)
         page.Terminal.Close();
   }

   private void kryptonDockableWorkspace_ActivePageChanged(object sender, Krypton.Workspace.ActivePageChangedEventArgs e)
   {
      CurrentPage = e.NewPage as ManagedPage;

      if (e.NewPage is MooCodeEditorPage editorPage && WindowManager.RecentEditor == null)
         WindowManager.RecentEditor = editorPage;
      else if (e.OldPage is MooCodeEditorPage oldEditorPage)
         WindowManager.RecentEditor = oldEditorPage;

      if (e.NewPage is TerminalPage terminalPage && WindowManager.RecentTerminal == null)
         WindowManager.RecentTerminal = terminalPage;
      else if (e.OldPage is TerminalPage oldTerminalPage)
         WindowManager.RecentTerminal = oldTerminalPage;

      if (CurrentPage is MooEditorPage editorPage2)
      {
         tlStatusLine.Text = ((editorPage2.SourceEditor?.Selection.Start.iLine + 1) ?? 1).ToString();
         tlStatusColumn.Text = ((editorPage2.SourceEditor?.Selection.Start.iChar + 1) ?? 1).ToString();
         if (editorPage2.KryptonParentContainer is KryptonWorkspaceCell cell)
            WindowManager.LastEditorCell = cell;
      }

      UpdateMenus();

      if (CurrentPage is TerminalPage terminalPage2)
         terminalPage2.Terminal.FocusOnInput();
   }

   private void kryptonDockingManager_PageCloseRequest(object sender, CloseRequestEventArgs e)
   {
      var key = e.UniqueName;
      var entry = WindowManager.GetPage(key);
      if (entry is MooCodeEditorPage page)
      {
         e.CloseRequest = PromptForSave(page, e.CloseRequest);
         if (e.CloseRequest is DockingCloseRequest.RemovePage or DockingCloseRequest.RemovePageAndDispose)
         {
            Errors.Remove(e.UniqueName);
            UpdateParserErrors();
         }
      }
      else if (entry is TerminalPage terminalPage)
         terminalPage.Terminal.Close();
   }

   private void kryptonDockableWorkspace_PageCloseClicked(object sender, UniqueNameEventArgs e)
   {
      var key = e.UniqueName;
      var entry = WindowManager.GetPage(key);
      if (entry is MooCodeEditorPage page)
      {
         PromptForSave(page, DockingCloseRequest.RemovePageAndDispose);
         Errors.Remove(e.UniqueName);
         UpdateParserErrors();
      }
      else if (entry is TerminalPage terminalPage)
         terminalPage.Terminal.Close();
   }

   private DockingCloseRequest PromptForSave(MooCodeEditorPage page, DockingCloseRequest request)
   {
      if (page.Editor.IsChanged)
      {
         var name = page.Editor.Document.Name;
         DialogResult dialogResult = MessageBox.Show($"\"{name}\" has been modified but has not been saved.  Would you like to save this file?", "Modified File", MessageBoxButtons.YesNo);
         if (dialogResult == DialogResult.Yes)
         {
            saveFileDialog.DefaultExt = "moo";
            saveFileDialog.Filter = @"Moo files (*.moo)|*.moo|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.Title = "Please select a file name to save as";
            saveFileDialog.FileName = name;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
               var path = saveFileDialog.FileName;
               page.Editor.SaveToFile(path, Encoding.Default);
               return request;
            }

            return DockingCloseRequest.None;
         }
      }

      return request;
   }

   private void mnuItemSend_Click(object sender, EventArgs e)
   {
      if (CurrentPage is MooEditorPage { CanUpload: true } page)
         page.UploadSource();
   }

   private void mnuItemPrevEditor_Click(object sender, EventArgs e)
   {
      if (WindowManager.RecentEditor != null)
      {
         WindowManager.ShowPage(WindowManager.RecentEditor);
         WindowManager.RecentEditor.Editor.Select();
      }
   }

   private void mnuItemPrevTerminal_Click(object sender, EventArgs e)
   {
      if (WindowManager.RecentTerminal != null)
      {
         WindowManager.ShowPage(WindowManager.RecentTerminal);
         WindowManager.RecentTerminal.Terminal.Select();
      }
   }

   private void mnuItemEchoCommands_CheckStateChanged(object sender, EventArgs e)
   {
      if (CurrentPage is TerminalPage page)
         page.Terminal.EchoEnabled = mnuItemEchoCommands.CheckState == CheckState.Checked;
   }

   private void mnuItemWordWrap_CheckStateChanged(object sender, EventArgs e)
   {
      if (CurrentPage is MooEditorPage editorPage)
         editorPage.WordWrap = mnuItemWordWrap.CheckState == CheckState.Checked;
      else if (CurrentPage is TerminalPage terminalPage)
         terminalPage.WordWrap = mnuItemWordWrap.CheckState == CheckState.Checked;
   }

   private void mnuItemShowLineNumbers_CheckStateChanged(object sender, EventArgs e)
   {
      if (CurrentPage is MooEditorPage editorPage)
         editorPage.ShowLineNumbers = mnuItemShowLineNumbers.CheckState == CheckState.Checked;
   }

   private void mnuItemIndentationGuides_CheckStateChanged(object sender, EventArgs e)
   {
      if (CurrentPage is MooCodeEditorPage editorPage)
         editorPage.ShowTextBlockIndentationGuides = mnuItemIndentationGuides.CheckState == CheckState.Checked;
   }

   private void mnuItemEnableCodeFolding_CheckStateChanged(object sender, EventArgs e)
   {
      if (CurrentPage is MooCodeEditorPage editorPage)
         editorPage.DisplayCodeFolding = mnuItemEnableCodeFolding.CheckState == CheckState.Checked;
   }

   private void mnuItemZoomIn_Click(object sender, EventArgs e)
   {
      if (CurrentPage is MooEditorPage editorPage)
         editorPage.SourceEditor.Zoom += 20;
      if (CurrentPage is TerminalPage terminalPage)
         terminalPage.Terminal.Output.Zoom += 20;
   }

   private void mnuItemZoomOut_Click(object sender, EventArgs e)
   {
      if (CurrentPage is MooEditorPage editorPage)
         if (editorPage.SourceEditor.Zoom > 30)
            editorPage.SourceEditor.Zoom -= 20;
      if (CurrentPage is TerminalPage terminalPage)
         if (terminalPage.Terminal.Output.Zoom > 30)
            terminalPage.Terminal.Output.Zoom -= 20;
   }

   private void mnuItemShowPreviewPane_CheckStateChanged(object sender, EventArgs e)
   {
      if (CurrentPage is MooDocumentEditorPage page)
         page.EnablePreview = mnuItemShowPreviewPane.CheckState == CheckState.Checked;
   }

   private void mnuItemMooTextColor_CheckStateChanged(object sender, EventArgs e)
   {
      if (CurrentPage is MooDocumentEditorPage page)
         page.Editor.EnableMooTextProcessing = mnuItemMooTextColor.CheckState == CheckState.Checked;
   }

   private void mnuItemMarkdownSupport_CheckStateChanged(object sender, EventArgs e)
   {
      mnuItemMarkdown.CheckState = mnuItemMarkdownSupport.CheckState;
      if (CurrentPage is MooDocumentEditorPage page)
         page.Editor.EnableMarkdownProcessing = mnuItemMarkdownSupport.CheckState == CheckState.Checked;
   }

   private void mnuItemWorldManager_Click(object sender, EventArgs e)
   {
      var manager = new WorldManager();
      var book = GetWorldsAddressBook();
      manager.LoadAddressBook(book);
      manager.SourceFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Worlds.xml");
      manager.ConnectToWorld += Manager_ConnectToWorld;
      manager.ShowDialog(this);
      BuildTerminalShortcutMenu();
   }

   private async void Manager_ConnectToWorld(object sender, WorldConfiguration e)
   {
      await OpenTerminalConnectionAsync(e);
   }

   private AddressBook GetWorldsAddressBook()
   {
      var worldsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Worlds.xml");
      return !File.Exists(worldsFile) ? new AddressBook() : AddressBook.LoadFromFile(worldsFile);
   }
}
