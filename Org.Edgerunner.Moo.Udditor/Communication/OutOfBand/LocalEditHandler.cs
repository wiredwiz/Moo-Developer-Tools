﻿#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="LocalEditHandler.cs">
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

using System.Text;
using NLog;
using Org.Edgerunner.Mud.Communication;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.Communication.OutOfBand;
using Org.Edgerunner.Moo.Editor.Configuration;
using Org.Edgerunner.Moo.Udditor.Pages;

namespace Org.Edgerunner.Moo.Udditor.Communication.OutOfBand;

// ReSharper disable once HollowTypeName
/// <summary>
/// Class responsible for handling old style Moo local edit functionality.
/// Implements the <see cref="IOutOfBandMessageHandler" />
/// </summary>
/// <seealso cref="IOutOfBandMessageHandler" />
public class LocalEditHandler : IOutOfBandMessageHandler
{
   protected static ILogger Logger = LogManager.GetCurrentClassLogger();

   public LocalEditHandler(WindowManager windowManager)
   {
      DocumentSource = new StringBuilder();
      _WindowManager = windowManager;
   }

   /// <summary>
   /// Gets or sets the name of the verb.
   /// </summary>
   /// <value>
   /// The name of the verb.
   /// </value>
   public string DocumentName { get; protected set; }

   /// <summary>
   /// Gets or sets the upload command.
   /// </summary>
   /// <value>
   /// The upload command.
   /// </value>
   public string UploadCommand { get; protected set; }

   /// <summary>
   /// Gets or sets the verb source code.
   /// </summary>
   /// <value>
   /// The verb source.
   /// </value>
   public StringBuilder DocumentSource { get; set; }

   private WindowManager _WindowManager;

   /// <summary>
   /// Processes the message.
   /// </summary>
   /// <param name="client">The client terminal emulator.</param>
   /// <param name="message">The message.</param>
   /// <param name="state">The current message processing state.</param>
   /// <returns>
   ///   <c>true</c> if was processed, <c>false</c> otherwise.
   /// </returns>
   public bool ProcessMessage(IClientTerminal client, string message, ref MessageProcessingState state)
   {
      // We are already reading local edit source code
      if (state.CurrentProcessor == this && !state.Finished)
      {
         if (message.Trim() == ".")
         {
            Logger.Trace("Local edit: .");
            // The reset will clean up anyway, but I like to be paranoid
            state.Finished = true;
            state.CurrentProcessor = null;

            // Open editor window
            MooEditorPage page;
            if (DocumentName.Contains(":"))
            {
               Logger.Trace("Opening code editor");
               page = _WindowManager.CreateMooCodeEditorPage(DocumentName,
                                                      client.World,
                                                      Settings.Instance.DefaultGrammarDialect,
                                                      DocumentSource.ToString().Trim());
            }
            else
            {
               Logger.Trace("Opening document editor");
               page = _WindowManager.CreateDocumentEditorPage(DocumentName, client.World, DocumentSource.ToString().Trim());
            }
            // Uploader must be configured before showing the page
            page.Uploader = new LocalEditUploader(UploadCommand, client);
            _WindowManager.ShowPage(page);
            Logger.Trace("Local edit: Session Complete");
            state.Reset();

            return true;
         }

         DocumentSource.Append(message);
         Logger.Trace($"Local edit: {message.TrimEnd()}");
         return true;
      }

      message = message.Trim();
      var lower = message.ToLowerInvariant();
      var uploadStart = lower.LastIndexOf("upload:", StringComparison.Ordinal);
      if (!lower.StartsWith("edit name:") || uploadStart == -1)
         return false;

      if (uploadStart == 10)
         return false;

      if (uploadStart + 7 > message.Length)
         return false;

      var name = message.Substring(10, uploadStart - 10);
      if (string.IsNullOrEmpty(name))
         return false;

      var upload = message.Substring(uploadStart + 7);
      if (string.IsNullOrEmpty(upload))
         return false;

      DocumentSource.Clear();
      DocumentName = name.Trim();
      UploadCommand = upload.Trim();
      Logger.Trace($"Edit Name: {DocumentName}");
      Logger.Trace($"Edit Command: {UploadCommand}");

      return true;
   }

   public void Reset()
   {
      UploadCommand = string.Empty;
      DocumentName = string.Empty;
      DocumentSource.Clear();
   }
}
