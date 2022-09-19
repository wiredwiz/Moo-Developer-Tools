#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="Editor_MenuConfiguration.cs">
// Copyright (c)  2022
// </copyright>
// 
// BSD 3-Clause License
// 
// Copyright (c) 2022,
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
// 
// 3. Neither the name of the copyright holder nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

using Org.Edgerunner.Moo.Editor;
using Org.Edgerunner.Moo.Udditor.Pages;

namespace Org.Edgerunner.Moo.Udditor.Main;

public partial class Editor
{
   void BuildTerminalShortcutMenu()
   {
      while (mnuItemTerminal.DropDownItems.Count > 8)
         mnuItemTerminal.DropDownItems.RemoveAt(8);
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
      mnuItemUpload.Enabled = isEditor && editPage.CanUpload;
      mnuItemEchoCommands.Enabled = isTerminal;
      mnuItemEnableColor.Enabled = isTerminal;
      mnuItemEnableBlinking.Enabled = isTerminal;
      mnuItemEnableAudio.Enabled = isTerminal;
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
      mnuItemEnableAudio.CheckState = isTerminal && terminalPage.AsciiBellEnabled ? CheckState.Checked : CheckState.Unchecked;
      mnuItemEnableBlinking.CheckState =
         isTerminal && terminalPage.BlinkingTextEnabled ? CheckState.Checked : CheckState.Unchecked;
      mnuItemEnableColor.CheckState = isTerminal && terminalPage.AnsiColorEnabled ? CheckState.Checked : CheckState.Unchecked;
   }
}
