#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="McpCordPackage.cs">
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

using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP.Interfaces;

namespace Org.Edgerunner.Mud.MCP.Packages;

/// <summary>Implements the mcp-cord package, managing multiplexed cord channels.</summary>
public class McpCordPackage : IMcpPackage
{
   private McpClientSession? _session;
   private readonly Dictionary<string, McpCord> _cords = new();
   private readonly Dictionary<string, IMcpPackage> _cordTypeHandlers;

   /// <summary>Initializes a new instance of <see cref="McpCordPackage"/>.</summary>
   /// <param name="cordTypeHandlers">Handlers keyed by cord type string (e.g., "whiteboard").</param>
   public McpCordPackage(Dictionary<string, IMcpPackage> cordTypeHandlers)
   {
      _cordTypeHandlers = cordTypeHandlers;
   }

   /// <inheritdoc/>
   public string Name { get; set; } = "mcp-cord";

   /// <inheritdoc/>
   public double MinimumVersion { get; set; } = 1.0;

   /// <inheritdoc/>
   public double MaximumVersion { get; set; } = 1.0;

   /// <inheritdoc/>
   public void SetSession(McpClientSession session) => _session = session;

   /// <inheritdoc/>
   public bool CanHandleMessage(Message message)
   {
      var name = message.Name.ToLowerInvariant();
      return name is "mcp-cord-open" or "mcp-cord" or "mcp-cord-closed";
   }

   /// <inheritdoc/>
   public bool ProcessMessage(IClientTerminal client, Message message)
   {
      return message.Name.ToLowerInvariant() switch
      {
         "mcp-cord-open"   => ProcessCordOpen(message),
         "mcp-cord"        => ProcessCord(client, message),
         "mcp-cord-closed" => ProcessCordClosed(message),
         _ => false
      };
   }

   private bool ProcessCordOpen(Message message)
   {
      if (!message.Data.TryGetValue("_id:", out var id)) return false;
      if (!message.Data.TryGetValue("_type:", out var type)) return false;

      _cords[id] = new McpCord(id, type);
      return true;
   }

   private bool ProcessCord(IClientTerminal client, Message message)
   {
      if (!message.Data.TryGetValue("_id:", out var id)) return false;
      if (!_cords.TryGetValue(id, out var cord) || !cord.IsOpen) return false;
      if (!message.Data.TryGetValue("_message:", out var cordMessage)) return false;

      if (_cordTypeHandlers.TryGetValue(cord.Type.ToLowerInvariant(), out var handler))
      {
         var innerData = message.Data
            .Where(kvp => kvp.Key != "_id:" && kvp.Key != "_message:")
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
         handler.ProcessMessage(client, new Message(cordMessage, message.Key, innerData));
      }

      return true;
   }

   private bool ProcessCordClosed(Message message)
   {
      if (!message.Data.TryGetValue("_id:", out var id)) return false;

      if (_cords.TryGetValue(id, out var cord))
      {
         cord.IsOpen = false;
         _cords.Remove(id);
      }

      return true;
   }

   /// <inheritdoc/>
   public void Reset() => _cords.Clear();
}
