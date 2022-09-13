#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="WindowManager.cs">
// Copyright (c) Thaddeus Ryker 2022
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

using JetBrains.Annotations;
using Krypton.Docking;
using Krypton.Navigator;
using Org.Edgerunner.Moo.Editor;
using Org.Edgerunner.Moo.Udditor.Pages;
using System;
using Org.Edgerunner.Mud.Communication;
using Org.Edgerunner.Mud.Communication.OutOfBand;
using Org.Edgerunner.Moo.Editor.Controls;
using Org.Edgerunner.Moo.Udditor.Communication.OutOfBand;
using Antlr4.Runtime.Misc;
using Krypton.Workspace;

namespace Org.Edgerunner.Moo.Udditor;

/// <summary>
/// Class responsible for managing application windows.
/// </summary>
// ReSharper disable once HollowTypeName
public class WindowManager
{
   /// <summary>
   /// Initializes a new instance of the <see cref="WindowManager" /> class.
   /// </summary>
   /// <param name="workspace">The workspace.</param>
   /// <param name="owner">The form that owns this instance.</param>
   public WindowManager(KryptonDockingWorkspace workspace, Form owner)
   {
      Workspace = workspace;
      _Owner = owner;
   }

   public KryptonDockingWorkspace Workspace { get; set; }

   public TerminalPage RecentTerminal { get; set; }

   public MooCodeEditorPage RecentEditor { get; set; }

   protected Dictionary<string, ManagedPage> Pages { get; } = new();

   protected Form _Owner;

   protected string _EditorWorkspaceName = "Workspace";

   public KryptonWorkspaceCell LastEditorCell { get; set; }

   public event EventHandler<MooEditorPage> EditorCursorUpdated;

   public event EventHandler<MooCodeEditorPage> EditorParsingComplete;

   /// <summary>
   /// Registers the page.
   /// </summary>
   /// <param name="key">The unique key for the page.</param>
   /// <param name="page">The page.</param>
   /// <returns>The registered <see cref="KryptonPage"/>.</returns>
   public ManagedPage RegisterPage(string key, ManagedPage page)
   {
      Pages[key] = page;
      return page;
   }

   /// <summary>
   /// Registers the page.
   /// </summary>
   /// <param name="page">The page.</param>
   /// <returns>The registered <see cref="KryptonPage"/>.</returns>
   public ManagedPage RegisterPage(ManagedPage page)
   {
      Pages[page.UniqueName] = page;
      return page;
   }

   /// <summary>
   /// Uns the register page.
   /// </summary>
   /// <param name="key">The key.</param>
   /// <returns>The un-registered <see cref="ManagedPage"/>.</returns>
   public ManagedPage UnRegisterPage(string key)
   {
      if (Pages.TryGetValue(key, out ManagedPage page))
      {
         Pages.Remove(key);
         return page;
      }

      return null;
   }

   /// <summary>
   /// Window manager knows about page.
   /// </summary>
   /// <param name="key">The key for the page.</param>
   /// <returns><c>true</c> if this manager instance knows about the page, <c>false</c> otherwise.</returns>
   public bool KnowsAboutPage(string key)
   {
      return Pages.ContainsKey(key);
   }

   /// <summary>
   /// Creates a new editor page and registers it.
   /// </summary>
   /// <param name="dialect">The dialect.</param>
   /// <returns>A new see<see cref="MooCodeEditorPage"/> instance.</returns>
   public MooCodeEditorPage CreateMooCodeEditorPage(GrammarDialect dialect)
   {
      MooCodeEditorPage CreatePage()
      {
         var page = new MooCodeEditorPage(this, dialect);
         RegisterPage(page);
         page.CursorPositionChanged += Page_CursorPositionChanged;
         page.ParsingComplete += Page_ParsingComplete;
         page.DockChanged += EditorPage_DockChanged;
         if (LastEditorCell != null)
            LastEditorCell.Pages.Add(page);
         else
            Workspace.DockingManager.AddToWorkspace(_EditorWorkspaceName, new KryptonPage[] { page });
         return page;
      }

      return _Owner.InvokeRequired ? _Owner.Invoke(CreatePage) : CreatePage();
   }

   /// <summary>
   /// Creates a new editor page and registers it.
   /// </summary>
   /// <param name="dialect">The dialect.</param>
   /// <param name="filePath">The file path.</param>
   /// <returns>A new see<see cref="MooCodeEditorPage"/> instance.</returns>
   public MooCodeEditorPage CreateMooCodeEditorPage(GrammarDialect dialect, string filePath)
   {
      MooCodeEditorPage CreatePage()
      {
         var page = new MooCodeEditorPage(this, dialect, filePath);
         RegisterPage(page);
         page.CursorPositionChanged += Page_CursorPositionChanged;
         page.ParsingComplete += Page_ParsingComplete;
         page.DockChanged += EditorPage_DockChanged;
         if (LastEditorCell != null)
            LastEditorCell.Pages.Add(page);
         else
            Workspace.DockingManager.AddToWorkspace(_EditorWorkspaceName, new KryptonPage[] { page });
         return page;
      }

      return _Owner.InvokeRequired ? _Owner.Invoke(CreatePage) : CreatePage();
   }

   /// <summary>
   /// Creates a new editor page and registers it.
   /// </summary>
   /// <param name="verbName">Name of the verb.</param>
   /// <param name="worldName">Name of the world.</param>
   /// <param name="dialect">The dialect.</param>
   /// <param name="source">The source.</param>
   /// <returns>A new see<see cref="MooCodeEditorPage"/> instance.</returns>
   public MooCodeEditorPage CreateMooCodeEditorPage(string verbName, string worldName, GrammarDialect dialect, string source)
   {
      MooCodeEditorPage CreatePage()
      {
         var page = new MooCodeEditorPage(this, verbName, worldName, dialect, source);
         RegisterPage(page);
         page.CursorPositionChanged += Page_CursorPositionChanged;
         page.ParsingComplete += Page_ParsingComplete;
         page.DockChanged += EditorPage_DockChanged;
         if (LastEditorCell != null)
            LastEditorCell.Pages.Add(page);
         else
            Workspace.DockingManager.AddToWorkspace(_EditorWorkspaceName, new KryptonPage[] { page });
         return page;
      }

      return _Owner.InvokeRequired ? _Owner.Invoke(CreatePage) : CreatePage();
   }

   /// <summary>
   /// Creates a new document editor page and registers it.
   /// </summary>
   /// <param name="documentName">Name of the document.</param>
   /// <param name="worldName">Name of the world.</param>
   /// <param name="source">The source.</param>
   /// <returns>A new see<see cref="MooDocumentEditorPage"/> instance.</returns>
   public MooDocumentEditorPage CreateDocumentEditorPage(string documentName, string worldName, string source)
   {
      MooDocumentEditorPage CreatePage()
      {
         var page = new MooDocumentEditorPage(this, documentName, worldName, source);
         RegisterPage(page);
         page.Editor.PreviewPaneBackgroundColor = Color.Black;
         page.Editor.PreviewPaneForegroundColor = Color.White;
         page.CursorPositionChanged += Page_CursorPositionChanged;
         page.DockChanged += EditorPage_DockChanged;
         if (LastEditorCell != null)
            LastEditorCell.Pages.Add(page);
         else
            Workspace.DockingManager.AddToWorkspace(_EditorWorkspaceName, new KryptonPage[] { page });
         return page;
      }

      return _Owner.InvokeRequired ? _Owner.Invoke(CreatePage) : CreatePage();
   }

   private void EditorPage_DockChanged(object sender, EventArgs e)
   {
      if ((sender as MooCodeEditorPage)?.KryptonParentContainer is KryptonWorkspaceCell cell)
         LastEditorCell = cell;
      else if ((sender as MooDocumentEditorPage)?.KryptonParentContainer is KryptonWorkspaceCell cell2)
         LastEditorCell = cell2;
   }

   /// <summary>
   /// Creates a parser message display page and registers it.
   /// </summary>
   /// <returns>A new <see cref="ParserMessageDisplayPage"/> instance.</returns>
   public ParserMessageDisplayPage CreateParserMessageDisplayPage()
   {
      var page = new ParserMessageDisplayPage(this);
      RegisterPage(page);
      Workspace.DockingManager.AddDockspace("FooterControls", DockingEdge.Bottom, new KryptonPage[] { page });
      return page;
   }

   /// <summary>
   /// Creates a new terminal page and registers it.
   /// </summary>
   /// <param name="world">The world.</param>
   /// <param name="useTls">if set to <c>true</c> [use TLS].</param>
   /// <returns>
   /// A new <see cref="TerminalPage" /> instance.
   /// </returns>
   public TerminalPage CreateTerminalPage(string world, bool useTls = false)
   {
      TerminalPage CreatePage()
      {
         var oobPrefix = "#$#";
         var oobHandler = new OutOfBandMessageProcessor();
         oobHandler.RegisterHandler(new LocalEditHandler(this));
         var processor = new RootMessageProcessor(oobPrefix, oobHandler);
         processor.OutOfBandMessagingTimeout = 500000;
         var page = new TerminalPage(this, processor, world, useTls);
         RegisterPage(page);
         Workspace.DockingManager.AddToWorkspace(_EditorWorkspaceName, new KryptonPage[] { page });
         page.OutOfBandPrefix = oobPrefix;
         page.ClearFlags(KryptonPageFlags.DockingAllowAutoHidden);
         page.ClearFlags(KryptonPageFlags.DockingAllowClose);
         return page;
      }

      return _Owner.InvokeRequired ? _Owner.Invoke(CreatePage) : CreatePage();
   }

   private void Page_CursorPositionChanged(object sender, EventArgs e)
   {
      OnEditorCursorUpdated(sender as MooEditorPage);
   }

   private void Page_ParsingComplete(object sender, ParsingCompleteEventArgs e)
   {
      MooCodeEditor codeEditor = sender as MooCodeEditor;
      var page = codeEditor.Parent;
      OnEditorParsingCompleted(page as MooCodeEditorPage);
   }

   /// <summary>
   /// Gets the page referenced by the supplied key.
   /// </summary>
   /// <param name="key">The key.</param>
   /// <returns>The related page or null if not found.</returns>
   [CanBeNull]
   public ManagedPage GetPage(string key)
   {
      if (Pages.TryGetValue(key, out var page))
         return page;

      return null;
   }

   /// <summary>
   /// Shows the page with the specified key.
   /// </summary>
   /// <param name="key">The key.</param>
   /// <returns>The <see cref="KryptonPage"/> instance.</returns>
   [CanBeNull]
   public ManagedPage ShowPage(string key)
   {
      ManagedPage DoShowPage()
      {
         if (Pages.TryGetValue(key, out ManagedPage page))
         {
            Workspace.SelectPage(key);
            page.Focus();
            return page;
         }

         return null;
      }

      return _Owner.InvokeRequired ? _Owner.Invoke(DoShowPage) : DoShowPage();
   }

   /// <summary>
   /// Shows the page.
   /// </summary>
   /// <param name="page">The page to show.</param>
   /// <returns>
   /// The <see cref="KryptonPage" /> instance.
   /// </returns>
   [CanBeNull]
   public ManagedPage ShowPage(ManagedPage page)
   {
      return ShowPage(page.UniqueName);
   }

   /// <summary>
   /// Closes the page.
   /// </summary>
   /// <param name="key">The key.</param>
   public void ClosePage(string key)
   {
      void DoClosePage()
      {
         if (!Pages.TryGetValue(key, out ManagedPage page))
            return;

         Workspace.DockingManager.CloseRequest(new[] { key });
         if (!Workspace.DockingManager.ContainsPage(page))
            UnRegisterPage(page.UniqueName);
      }

      if (_Owner.InvokeRequired)
         _Owner.Invoke(DoClosePage);
      else
         DoClosePage();
   }

   protected virtual void OnEditorCursorUpdated(MooEditorPage e)
   {
      EditorCursorUpdated?.Invoke(this, e);
   }

   protected virtual void OnEditorParsingCompleted(MooCodeEditorPage e)
   {
      EditorParsingComplete?.Invoke(this, e);
   }
}
