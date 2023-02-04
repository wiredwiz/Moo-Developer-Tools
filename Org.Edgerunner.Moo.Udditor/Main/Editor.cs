using System.Diagnostics;
using System.Reflection;
using System.Text;
using FastColoredTextBoxNS.Types;
using Krypton.Docking;
using Krypton.Toolkit;
using Krypton.Workspace;
using NLog;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar.Errors;
using Org.Edgerunner.Moo.Editor;
using Org.Edgerunner.Moo.Editor.Configuration;
using Org.Edgerunner.Moo.Editor.Controls;
using Org.Edgerunner.Moo.Udditor.Events;
using Org.Edgerunner.Moo.Udditor.Pages;
using Org.Edgerunner.Mud.Common;
using Org.Edgerunner.Mud.Common.Cryptography;

namespace Org.Edgerunner.Moo.Udditor.Main;

public partial class Editor : KryptonForm
{
   protected static ILogger Logger = LogManager.GetCurrentClassLogger();

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

   private void Editor_Load(object sender, EventArgs e)
   {
      var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Moo Udditor.log");
      try
      {
         if (File.Exists(logFile))
            File.Delete(logFile);
      }
      catch (UnauthorizedAccessException)
      {
         Debug.WriteLine("Unable to access log file");
         MessageBox.Show("Unable to access log file", "File Access Denied");
      }
      catch (IOException)
      {
         Debug.WriteLine("Error accessing log file");
      }
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
      Logger.Trace($"Loading {Assembly.GetExecutingAssembly().GetName()}");
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
         ApplicationKeyManager.RetrieveMasterKey(out key);
         Logger.Trace("Master key loaded successfully");
      }
      catch (Exception)
      {
      }
      if (string.IsNullOrEmpty(key))
      {
         Logger.Trace("No master key found");
         var setup = new Setup();
         setup.StartPosition = FormStartPosition.CenterParent;
         if (setup.ShowDialog(this) != DialogResult.OK)
         {
            MessageBox.Show("Without a proper master key, the world manager will be disabled.", "World Manager Disabled");
            _WorldManagerEnabled = false;
            Logger.Trace("Master key entry cancelled");
         }
         else
         {
            _WorldManagerEnabled = true;
            key = Hash.Sha256(setup.Password);
            ApplicationKeyManager.SaveMasterKey(key);
            Logger.Trace("A new master key was successfully set");
         }
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

   private void kryptonDockableWorkspace_ActivePageChanged(object sender, Krypton.Workspace.ActivePageChangedEventArgs e)
   {
      if (e.NewPage != null)
         Logger.Trace($"Page {e.NewPage.TextTitle} activated");

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
      Logger.Trace($"Page \"{entry?.TextTitle}\" close requested");
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
      Logger.Trace($"Page \"{entry?.TextTitle}\" close clicked");
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

   private AddressBook GetWorldsAddressBook()
   {
      var worldsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Worlds.xml");
      return !File.Exists(worldsFile) ? new AddressBook() : AddressBook.LoadFromFile(worldsFile);
   }
}
