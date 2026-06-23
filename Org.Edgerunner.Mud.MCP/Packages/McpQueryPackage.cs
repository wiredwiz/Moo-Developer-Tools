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
using Org.Edgerunner.Mud.Common.Querying;
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
   /// The query registry the provider was registered into. Captured at registration time so the
   /// disconnect/dispose paths can unregister even when no terminal reference is supplied.
   /// </summary>
   private MooWorldQueryService? _queryProviders;

   /// <summary>
   /// The session key that was in effect when <see cref="_provider"/> was last registered.
   /// Used to detect a mid-session re-handshake that supplies a new key.
   /// </summary>
   private string? _providerKey;

   private bool _disposed;

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
   // Both SetSession and OnPackageSupported are invoked on the single OOB dispatch thread, so the
   // unguarded write here and the locked read in OnPackageSupported do not race in practice.
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
   /// <remarks>
   /// A mid-session re-handshake produces a new <see cref="McpClientSession"/> with a fresh key
   /// and calls this method again. In that case the stale provider is unregistered first so that
   /// subsequent queries carry the new authentication key; skipping the swap would cause every
   /// query to time out while the server rejects the outdated key.
   /// </remarks>
   public void OnPackageSupported(IClientTerminal client)
   {
      lock (_registrationLock)
      {
         if (_session == null)
         {
            Logger.Warn("edgerunner-org-moo-query confirmed but no MCP session is set; provider not registered.");
            return;
         }

         // Idempotent: same key confirmed again — nothing to do.
         if (_provider != null && _providerKey == _session.Key)
            return;

         // Renegotiation with a new key: unregister the stale provider first.
         if (_provider != null)
         {
            Logger.Debug("Re-registering MCP query provider for renegotiated session (key changed).");
            client.QueryProviders.Unregister(_provider);
            _provider = null;
         }

         _provider = new McpQueryProvider(client, _session.Key, _correlator, _timeout);
         _queryProviders = client.QueryProviders;
         client.QueryProviders.Register(_provider, ProviderPriority);
         _providerKey = _session.Key;
      }
   }

   /// <inheritdoc/>
   public void Reset() { }

   /// <inheritdoc/>
   /// <remarks>
   /// Deterministic, reuse-safe disconnect teardown fired once off <c>MudClientSession.Closed</c>.
   /// The provider is unregistered (so a post-disconnect query no longer routes to a dead terminal),
   /// every in-flight request is faulted with <see cref="QueryConnectionClosedException"/> rather than
   /// being left to wait out its bounded timeout, and the registration state is cleared so the next
   /// negotiation re-registers a fresh provider into the (possibly new) registry.
   /// </remarks>
   public void OnDisconnected()
   {
      lock (_registrationLock)
      {
         if (_provider != null)
            _queryProviders?.Unregister(_provider);

         _correlator.FaultAll(new QueryConnectionClosedException());

         _provider = null;
         _providerKey = null;
         _queryProviders = null;
      }
   }

   /// <summary>
   /// Releases the resources used by this package. Final teardown (no reuse): the provider is
   /// unregistered and the correlator is disposed so that any straggling request fails fast.
   /// </summary>
   public void Dispose()
   {
      lock (_registrationLock)
      {
         if (_disposed)
            return;

         if (_provider != null)
            _queryProviders?.Unregister(_provider);

         _provider = null;
         _providerKey = null;
         _queryProviders = null;
         _correlator.Dispose();
         _disposed = true;
      }
   }
}
