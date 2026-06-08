#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="Editor_FileMenu.cs">
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

using System.Text;
using Org.Edgerunner.Moo.Udditor.Pages;

namespace Org.Edgerunner.Moo.Udditor.Main;

public partial class Editor
{
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
            TrySaveToFile(page, page.Document.Path);
         else
            SaveAs(page);
         page.SourceEditor.Invalidate();
      }
   }

   private void mnuItemSaveAsFile_Click(object sender, EventArgs e)
   {
      if (CurrentPage is MooEditorPage page)
         SaveAs(page);
   }

   /// <summary>
   /// Prompts the user for a destination file name and saves a local disk copy of the page's source.
   /// </summary>
   /// <param name="page">The editor page whose source should be saved.</param>
   private void SaveAs(MooEditorPage page)
   {
      Directory.CreateDirectory(ApplicationPaths.DocumentsFolder);
      saveFileDialog.DefaultExt = "moo";
      saveFileDialog.Filter = @"Moo files (*.moo)|*.moo|Text files (*.txt)|*.txt|Markdown files (*.md)|*.md|All files (*.*)|*.*";
      saveFileDialog.Title = "Please select a file name to save as";
      saveFileDialog.InitialDirectory = ApplicationPaths.DocumentsFolder;
      saveFileDialog.FileName = ApplicationPaths.SanitizeFileName(page.Document.Name);
      if (saveFileDialog.ShowDialog() == DialogResult.OK)
      {
         var path = saveFileDialog.FileName;
         if (TrySaveToFile(page, path))
         {
            page.Document.Path = path;
            page.Document.Name = Path.GetFileName(path);
            if (page is MooCodeEditorPage mooCodeEditorPage)
               mooCodeEditorPage.ParseSourceCode();
         }
      }
   }

   /// <summary>
   /// Saves the page's source to the specified file, reporting any failure without crashing.
   /// </summary>
   /// <param name="page">The editor page whose source should be saved.</param>
   /// <param name="path">The destination file path.</param>
   /// <returns><see langword="true"/> if the file was saved successfully; otherwise, <see langword="false"/>.</returns>
   private bool TrySaveToFile(MooEditorPage page, string path)
   {
      try
      {
         page.SourceEditor.SaveToFile(path, Encoding.Default);
         return true;
      }
      catch (Exception ex)
      {
         Logger.Error(ex, "Failed to save '{0}'", path);
         MessageBox.Show($"Could not save the file:\n{ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
         return false;
      }
   }

   private void mnuItemUpload_Click(object sender, EventArgs e)
   {
      if (CurrentPage is MooEditorPage { CanUpload: true } page)
         page.UploadSource();
   }

   private void tlMnuItemClose_Click(object sender, EventArgs e)
   {
      if (CurrentPage != null)
         WindowManager.ClosePage(CurrentPage.UniqueName);
   }
}
