#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="McpOobHandler.cs">
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

using Org.Edgerunner.Mud.Communication;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.Communication.OutOfBand;
using Org.Edgerunner.Mud.MCP.Interfaces;

namespace Org.Edgerunner.Mud.MCP;

/// <summary>
/// IOutOfBandMessageHandler that feeds raw OOB lines through McpMessageParser and
/// dispatches complete messages to McpMessageDispatcher.
/// </summary>
public class McpOobHandler : IOutOfBandMessageHandler
{
   private readonly McpMessageParser _parser = new();
   private readonly McpMessageDispatcher _dispatcher;

   /// <summary>Initializes a new instance of <see cref="McpOobHandler"/>.</summary>
   public McpOobHandler(Version minVersion, Version maxVersion)
   {
      _dispatcher = new McpMessageDispatcher(minVersion, maxVersion);
   }

   /// <summary>
   /// Initializes a new instance of <see cref="McpOobHandler"/> that also registers a set of
   /// extra application-supplied packages with the dispatcher before connection.
   /// </summary>
   /// <param name="minVersion">The minimum supported MCP protocol version.</param>
   /// <param name="maxVersion">The maximum supported MCP protocol version.</param>
   /// <param name="extraPackages">Additional packages to register (e.g. dns-org-mud-moo-simpleedit).</param>
   public McpOobHandler(Version minVersion, Version maxVersion, IEnumerable<IMcpPackage> extraPackages)
      : this(minVersion, maxVersion)
   {
      foreach (var package in extraPackages)
         _dispatcher.RegisterPackage(package);
   }

   /// <inheritdoc/>
   public bool ProcessMessage(IClientTerminal client, string line, ref MessageProcessingState state)
   {
      var result = _parser.FeedLine(line);

      switch (result)
      {
         case McpParseState.Complete:
            _dispatcher.Dispatch(client, _parser.Result!);
            _parser.Reset();
            state.CurrentProcessor = null;
            state.Finished = true;
            return true;

         case McpParseState.InProgress:
            state.CurrentProcessor = this;
            return true;

         default:
            _parser.Reset();
            state.CurrentProcessor = null;
            state.Finished = true;
            return true;
      }
   }

   /// <inheritdoc/>
   public void Reset() => _parser.Reset();
}
