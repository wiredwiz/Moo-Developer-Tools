#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="SimpleEditUploader.cs">
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

using Org.Edgerunner.Mud.Communication.Interfaces;

namespace Org.Edgerunner.Mud.MCP.Packages;

/// <summary>
/// An <see cref="IClientUploader"/> that returns edited content to the server as a
/// <c>dns-org-mud-moo-simpleedit-set</c> MCP message, echoing the originating reference.
/// </summary>
/// <seealso cref="IClientUploader" />
public sealed class SimpleEditUploader : IClientUploader
{
   private const string SetMessageName = "dns-org-mud-moo-simpleedit-set";

   private readonly string _sessionKey;
   private readonly string _reference;
   private readonly string _type;

   /// <summary>
   /// Initializes a new instance of the <see cref="SimpleEditUploader"/> class.
   /// </summary>
   /// <param name="client">The client terminal used to send the outbound message.</param>
   /// <param name="sessionKey">The negotiated MCP session authentication key.</param>
   /// <param name="reference">The opaque reference echoed back verbatim from the originating edit.</param>
   /// <param name="type">The content type echoed back from the originating edit.</param>
   public SimpleEditUploader(IClientTerminal client, string sessionKey, string reference, string type)
   {
      ClientTerminal = client;
      _sessionKey = sessionKey;
      _reference = reference;
      _type = type;
   }

   /// <inheritdoc/>
   public IClientTerminal ClientTerminal { get; }

   /// <inheritdoc/>
   public bool Upload(string sourceCode)
   {
      if (!ClientTerminal.IsConnected)
         return false;

      var lines = (sourceCode ?? string.Empty).Replace("\r\n", "\n").Split('\n');

      // The client generates its own fresh data-tag for each outbound multiline block.
      // Keep it alphanumeric so it never collides with the ' ', ':' or '*' the wire format uses.
      var dataTag = Guid.NewGuid().ToString("N").Substring(0, 8);

      var wireLines = McpUtils.FormatMultilineMessage(
         SetMessageName,
         _sessionKey,
         new Dictionary<string, string>
         {
            ["reference:"] = _reference,
            ["type:"] = _type
         },
         "content",
         lines,
         dataTag);

      ClientTerminal.SendOutOfBandLines(wireLines);
      return true;
   }
}
