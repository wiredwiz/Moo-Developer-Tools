#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="Editor_EditMenu.cs">
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

using Org.Edgerunner.Moo.Udditor.Pages;

namespace Org.Edgerunner.Moo.Udditor.Main;

public partial class Editor
{
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

   private void tlMnuItemToggleBookmark_Click(object sender, EventArgs e)
   {
      if (CurrentPage is MooCodeEditorPage page)
         page.Editor.ToggleBookmark(page.Editor.Selection.Start.iLine);
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

   private void mnuItemEnableCodeFolding_CheckStateChanged(object sender, EventArgs e)
   {
      if (CurrentPage is MooCodeEditorPage editorPage)
         editorPage.DisplayCodeFolding = mnuItemEnableCodeFolding.CheckState == CheckState.Checked;
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
}
