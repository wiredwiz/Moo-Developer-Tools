#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="McpQueryPackage.cs">
// Copyright (c) Thaddeus Ryker 2026
// </copyright>
//
// BSD 3-Clause License
//
// Copyright (c) 2026,
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

namespace Org.Edgerunner.Mud.MCP.Packages;

/// <summary>
/// Implements client support for the <c>edgerunner-org-moo-query</c> MCP package (v1.0): receives
/// tagged <c>…-reply</c>/<c>…-error</c> messages and completes the matching pending requests of its
/// <see cref="McpQueryProvider"/>. When MCP negotiation confirms the server supports the package, the
/// provider is registered with the terminal's query service at priority
/// <see cref="ProviderPriority"/>. See <c>docs/edgerunner-org-moo-query-protocol.md</c>.
/// </summary>
/// <seealso cref="IMcpPackage"/>
/// <seealso cref="IPackageNegotiationListener"/>
public class McpQueryPackage : IMcpPackage, IPackageNegotiationListener
{
   /// <summary>The MCP package name.</summary>
   public const string PackageName = "edgerunner-org-moo-query";

   /// <summary>The shared error reply message name.</summary>
   public const string ErrorMessageName = PackageName + "-error";

   /// <summary>The registry priority of the query provider (above SDWC's 100).</summary>
   public const int ProviderPriority = 200;

   private const string ReplySuffix = "-reply";

   private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

   private readonly McpQueryCorrelator _correlator = new();

   private readonly TimeSpan? _timeout;

   private readonly object _registrationLock = new();

   private McpClientSession? _session;

   private McpQueryProvider? _provider;

   /// <summary>
   /// Initializes a new instance of the <see cref="McpQueryPackage"/> class.
   /// </summary>
   /// <param name="timeout">An optional per-request timeout override for the provider (testing hook).</param>
   public McpQueryPackage(TimeSpan? timeout = null)
   {
      _timeout = timeout;
   }

   /// <inheritdoc/>
   public string Name { get; set; } = PackageName;

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
      if (!name.StartsWith(PackageName + "-", StringComparison.Ordinal))
         return false;

      return name == ErrorMessageName || name.EndsWith(ReplySuffix, StringComparison.Ordinal);
   }

   /// <inheritdoc/>
   public bool ProcessMessage(IClientTerminal client, Message message)
   {
      if (!CanHandleMessage(message))
         return false;

      if (!message.Data.TryGetValue("tag:", out var tag) || string.IsNullOrEmpty(tag))
      {
         Logger.Trace("Dropping MCP query reply '{0}' with no tag.", message.Name);
         return true;
      }

      bool completed;
      if (message.Name.Equals(ErrorMessageName, StringComparison.OrdinalIgnoreCase))
      {
         message.Data.TryGetValue("code:", out var code);
         message.Data.TryGetValue("message:", out var errorMessage);
         completed = _correlator.CompleteError(tag, new McpQueryErrorException(code ?? string.Empty, errorMessage ?? string.Empty));
      }
      else
      {
         // The parser joins multiline 'data' continuation lines with '\n'. The payload is minified
         // JSON and MOO strings cannot contain literal newlines, so stripping them reassembles the
         // transport chunks verbatim with no separator (protocol §3.2).
         message.Data.TryGetValue("data:", out var data);
         completed = _correlator.Complete(tag, (data ?? string.Empty).Replace("\n", string.Empty));
      }

      if (!completed)
         Logger.Trace("Dropping stale MCP query reply '{0}' (tag {1}).", message.Name, tag);

      return true;
   }

   /// <inheritdoc/>
   public void OnPackageSupported(IClientTerminal client)
   {
      lock (_registrationLock)
      {
         if (_provider != null)
            return;

         if (_session == null)
            return;

         _provider = new McpQueryProvider(client, _session.Key, _correlator, _timeout);
         client.QueryProviders.Register(_provider, ProviderPriority);
      }
   }

   /// <inheritdoc/>
   public void Reset() { }
}
