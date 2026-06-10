#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="WindowManagerSimpleEditConsumer.cs">
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

using NLog;
using Org.Edgerunner.Mud.Common.Querying;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP;
using Org.Edgerunner.Mud.MCP.Interfaces;
using Org.Edgerunner.Moo.Editor.Configuration;
using Org.Edgerunner.Moo.Udditor.Pages;

namespace Org.Edgerunner.Moo.Udditor.Communication.OutOfBand;

/// <summary>
/// Bridges the UI-free <see cref="SimpleEditConsumer"/>-style contract to the
/// <see cref="WindowManager"/>, opening the appropriate editor page for an incoming
/// <c>dns-org-mud-moo-simpleedit</c> edit request.
/// </summary>
/// <seealso cref="ISimpleEditConsumer" />
public class WindowManagerSimpleEditConsumer : ISimpleEditConsumer
{
   protected static ILogger Logger = LogManager.GetCurrentClassLogger();

   /// <summary>
   /// Initializes a new instance of the <see cref="WindowManagerSimpleEditConsumer"/> class.
   /// </summary>
   /// <param name="windowManager">The window manager used to open editor pages.</param>
   public WindowManagerSimpleEditConsumer(WindowManager windowManager)
   {
      _WindowManager = windowManager;
   }

   private readonly WindowManager _WindowManager;

   /// <inheritdoc/>
   public void PresentEdit(EditRequest request, IClientUploader uploader)
   {
      var world = uploader.ClientTerminal.World;

      MooEditorPage page;
      if (string.Equals(request.EditType, "moo-code", StringComparison.OrdinalIgnoreCase))
      {
         Logger.Trace("SimpleEdit: opening code editor");
         var codePage = _WindowManager.CreateMooCodeEditorPage(
            request.Name,
            world,
            Settings.Instance.DefaultGrammarDialect,
            request.Content);
         // Attach the world query provider and edited-object identity so the editor can
         // offer contextual member completion (verbs/properties/core references).
         codePage.QueryProvider = uploader.ClientTerminal.QueryProviders.Query;
         codePage.ContextObjectId = MooObjectReferenceParser.FindFirstObjectId(request.Reference);
         page = codePage;
      }
      else
      {
         Logger.Trace("SimpleEdit: opening document editor");
         page = _WindowManager.CreateDocumentEditorPage(request.Name, world, request.Content);
      }

      // Uploader must be configured before showing the page.
      page.Uploader = uploader;
      _WindowManager.ShowPage(page);
   }
}
