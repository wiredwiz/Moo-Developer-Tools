#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="McpMessageDispatcher.cs">
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

using NLog;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP.Exceptions;
using Org.Edgerunner.Mud.MCP.Interfaces;
using Org.Edgerunner.Mud.MCP.Packages;

namespace Org.Edgerunner.Mud.MCP;

/// <summary>Routes fully-assembled MCP messages to registered package handlers.</summary>
public class McpMessageDispatcher : IDisposable
{
   private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

   private readonly McpClientSessionManager _sessionManager;
   private readonly Dictionary<string, IMcpPackage> _packages = new();

   /// <summary>Gets the active session, or null if handshake has not completed.</summary>
   public McpClientSession? Session { get; private set; }

   /// <summary>
   /// Initializes a new instance of <see cref="McpMessageDispatcher"/> with the required mcp-negotiate and mcp-cord packages pre-registered.
   /// </summary>
   public McpMessageDispatcher(Version minVersion, Version maxVersion)
   {
      _sessionManager = new McpClientSessionManager(minVersion, maxVersion, new List<IMcpPackage>());

      var negotiatePackage = new McpNegotiatePackage(_packages);
      var cordPackage = new McpCordPackage(new Dictionary<string, IMcpPackage>());

      _packages["mcp-negotiate"] = negotiatePackage;
      _packages["mcp-cord"] = cordPackage;
   }

   /// <summary>Registers an additional package with this dispatcher.</summary>
   public void RegisterPackage(IMcpPackage package)
   {
      _packages[package.Name.ToLowerInvariant()] = package;
   }

   /// <summary>Dispatches a fully-assembled MCP message to the appropriate handler.</summary>
   public void Dispatch(IClientTerminal client, Message message)
   {
      if (message.Name.ToLowerInvariant() == "mcp" && string.IsNullOrEmpty(message.Key))
      {
         ProcessHandshake(client, message);
         return;
      }

      if (Session == null) return;
      if (message.Key != Session.Key)
      {
         Logger.Warn("Discarding MCP message '{0}': received auth key '{1}' does not match the session key.", message.Name, message.Key);
         return;
      }

      var lowerName = message.Name.ToLowerInvariant();
      var packageName = _packages.Keys
         .Where(k => lowerName.StartsWith(k))
         .OrderByDescending(k => k.Length)
         .FirstOrDefault();

      if (packageName == null) return;

      _packages[packageName].ProcessMessage(client, message);
   }

   /// <summary>
   /// Notifies every registered package that the underlying connection has closed, so each can tear
   /// down its provider and fault in-flight requests while remaining reusable for the next connection.
   /// </summary>
   public void OnDisconnected()
   {
      foreach (var package in _packages.Values)
         package.OnDisconnected();
   }

   /// <summary>
   /// Disposes every registered package (final teardown). Safe to call more than once.
   /// </summary>
   public void Dispose()
   {
      foreach (var package in _packages.Values)
         package.Dispose();
   }

   private void ProcessHandshake(IClientTerminal client, Message message)
   {
      IMcpSession? session;
      try
      {
         session = _sessionManager.NegotiationMcpSession(message);
      }
      catch (InvalidMcpMessageFormatException)
      {
         // Malformed handshake — per MCP spec, drop silently
         return;
      }
      if (session == null) return;

      Session = (McpClientSession)session;

      client.SendOutOfBandLine(Session.Handshake());

      foreach (var pkg in _packages.Values)
         pkg.SetSession(Session);

      foreach (var pkg in _packages.Values)
      {
         client.SendOutOfBandLine(McpUtils.FormatMessage(
            "mcp-negotiate-can",
            Session.Key,
            new Dictionary<string, string>
            {
               ["package:"] = pkg.Name,
               ["min-version:"] = pkg.MinimumVersion.ToString("F1"),
               ["max-version:"] = pkg.MaximumVersion.ToString("F1")
            }));
      }

      client.SendOutOfBandLine(McpUtils.FormatMessage(
         "mcp-negotiate-end", Session.Key, new Dictionary<string, string>()));
   }
}
