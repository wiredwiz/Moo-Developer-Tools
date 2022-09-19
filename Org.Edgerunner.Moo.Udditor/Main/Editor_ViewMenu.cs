#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="Editor_ViewMenu.cs">
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
}
