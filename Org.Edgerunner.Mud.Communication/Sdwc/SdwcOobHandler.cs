#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="SdwcOobHandler.cs">
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
using Org.Edgerunner.Mud.Communication.OutOfBand;

namespace Org.Edgerunner.Mud.Communication.Sdwc;

/// <summary>
/// An <see cref="IOutOfBandMessageHandler"/> that implements the SDWC SUPPORT handshake. On the
/// server's inbound <c>SDWC%%SUPPORT%%</c> broadcast it parses the advertised ability set into
/// <see cref="ServerCapabilities"/>, registers an <see cref="SdwcQueryProvider"/> with the connection's
/// query providers exactly once (gated on a queryable ability), and replies with the client's own
/// <c>#$# SDWC%%SUPPORT%%</c> declaration exactly once per session. It also correlates
/// <c>SDWC%%&lt;MARKER&gt;%%&lt;json&gt;</c> responses back to pending requests and consumes the
/// <c>SDWC-START-NOWRAP</c>/<c>SDWC-END-NOWRAP</c> rendering hints.
/// </summary>
/// <remarks>
/// Lines arrive with the leading space the SDWC wire form carries after the <c>#$#</c> prefix is
/// stripped; the handler trims before matching. Unrecognized lines return <c>false</c> (not handled).
/// </remarks>
public sealed class SdwcOobHandler : IOutOfBandMessageHandler
{
   /// <summary>The SDWC message prefix (after the leading space is trimmed).</summary>
   public const string SdwcPrefix = "SDWC%%";

   /// <summary>The inbound SUPPORT broadcast prefix; the remainder is a <c>|</c>-separated ability list.</summary>
   public const string SupportPrefix = "SDWC%%SUPPORT%%";

   /// <summary>
   /// The outbound SUPPORT declaration value passed to <see cref="IClientTerminal.SendOutOfBandLine"/>.
   /// Its leading space, combined with the <c>#$#</c> OOB prefix the send path prepends, produces the
   /// exact wire line <c>#$# SDWC%%SUPPORT%%</c>.
   /// </summary>
   public const string OutboundSupportDeclaration = " SDWC%%SUPPORT%%";

   /// <summary>The start-no-wrap rendering hint.</summary>
   public const string StartNoWrap = "SDWC-START-NOWRAP";

   /// <summary>The end-no-wrap rendering hint.</summary>
   public const string EndNoWrap = "SDWC-END-NOWRAP";

   /// <summary>The priority under which the SDWC provider self-registers.</summary>
   public const int ProviderPriority = 100;

   private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

   private readonly SdwcCorrelator _correlator;

   private readonly TimeSpan? _requestTimeout;

   private readonly object _registrationLock = new();

   private SdwcQueryProvider? _provider;

   /// <summary>
   /// The query registry the provider was registered into. Captured at registration time so the
   /// disconnect/dispose paths can unregister it.
   /// </summary>
   private MooWorldQueryService? _queryProviders;

   private bool _disposed;

   /// <summary>Whether the outbound SUPPORT declaration has already been sent this session.</summary>
   private bool _declarationSent;

   /// <summary>
   /// Initializes a new instance of the <see cref="SdwcOobHandler"/> class.
   /// </summary>
   /// <param name="correlator">The correlator shared with the provider. Defaults to a fresh instance.</param>
   /// <param name="requestTimeout">The per-request timeout passed to the provider; the provider default is used when <c>null</c>.</param>
   public SdwcOobHandler(SdwcCorrelator? correlator = null, TimeSpan? requestTimeout = null)
   {
      _correlator = correlator ?? new SdwcCorrelator();
      _requestTimeout = requestTimeout;
   }

   /// <summary>
   /// Gets the correlator backing this handler and its provider.
   /// </summary>
   public SdwcCorrelator Correlator => _correlator;

   /// <summary>
   /// Gets the registered provider, or <c>null</c> if no queryable SUPPORT broadcast has been seen yet.
   /// </summary>
   public SdwcQueryProvider? Provider => _provider;

   /// <summary>
   /// Gets the abilities advertised by the most recent server <c>SDWC%%SUPPORT%%</c> broadcast, or
   /// <c>null</c> until the first SUPPORT line is received. Last broadcast wins. This is the surface
   /// future SDWC feature-gating reads (analogous to MCP's supported-package set).
   /// </summary>
   public SdwcServerCapabilities? ServerCapabilities { get; private set; }

   /// <inheritdoc/>
   public bool ProcessMessage(IClientTerminal client, string message, ref MessageProcessingState state)
   {
      var line = message.Trim();

      if (line == StartNoWrap || line == EndNoWrap)
         return true;

      // SUPPORT broadcasts share the SDWC%% prefix but carry a |-list payload, not JSON, so they must
      // be handled before the JSON-correlation path below.
      if (line.StartsWith(SupportPrefix, StringComparison.Ordinal))
      {
         HandleSupport(client, line);
         return true;
      }

      if (line.StartsWith(SdwcPrefix, StringComparison.Ordinal))
      {
         HandleResponse(line);
         return true;
      }

      return false;
   }

   /// <inheritdoc/>
   public void Reset()
   {
      // No multi-line state to reset; pending correlation entries are owned by their awaiting callers.
   }

   /// <inheritdoc/>
   /// <remarks>
   /// Deterministic, reuse-safe disconnect teardown fired once off <c>MudClientSession.Closed</c>.
   /// The provider is unregistered (so a post-disconnect query no longer routes to a dead terminal),
   /// every in-flight request is faulted with <see cref="QueryConnectionClosedException"/> rather than
   /// being left to wait out its bounded timeout, and the registration state is cleared so the next
   /// capability signal re-registers a fresh provider.
   /// </remarks>
   public void OnDisconnected()
   {
      lock (_registrationLock)
      {
         if (_provider != null)
            _queryProviders?.Unregister(_provider);

         _correlator.FaultAll(new QueryConnectionClosedException());

         _provider = null;
         _queryProviders = null;
         ServerCapabilities = null;
         _declarationSent = false;
      }
   }

   /// <summary>
   /// Releases the resources used by this handler. Final teardown (no reuse): the provider is
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
         _queryProviders = null;
         _correlator.Dispose();
         _disposed = true;
      }
   }

   private void EnsureProviderRegistered(IClientTerminal client)
   {
      lock (_registrationLock)
      {
         if (_provider != null)
            return;

         _provider = new SdwcQueryProvider(client, _correlator, _requestTimeout);
         _queryProviders = client.QueryProviders;
         client.QueryProviders.Register(_provider, ProviderPriority);
         Logger.Trace("SDWC capability detected; registered SdwcQueryProvider.");
      }
   }

   /// <summary>
   /// Handles an inbound <c>SDWC%%SUPPORT%%…</c> broadcast: parses the <c>|</c>-separated ability list
   /// into <see cref="ServerCapabilities"/> (last broadcast wins), registers the query provider when a
   /// queryable ability is advertised, and sends the client's outbound SUPPORT declaration exactly once.
   /// </summary>
   /// <param name="client">The client terminal used to send the outbound declaration.</param>
   /// <param name="line">The trimmed inbound line, beginning with <see cref="SupportPrefix"/>.</param>
   private void HandleSupport(IClientTerminal client, string line)
   {
      var payload = line.Substring(SupportPrefix.Length);
      var tokens = payload.Split('|')
                          .Select(token => token.Trim())
                          .Where(token => token.Length > 0);
      var capabilities = new SdwcServerCapabilities(tokens);

      lock (_registrationLock)
      {
         ServerCapabilities = capabilities;

         if (capabilities.HasAnyQueryableAbility)
            EnsureProviderRegistered(client);

         if (!_declarationSent)
         {
            client.SendOutOfBandLine(OutboundSupportDeclaration);
            _declarationSent = true;
            Logger.Trace("Sent SDWC SUPPORT declaration to server.");
         }
      }
   }

   private void HandleResponse(string line)
   {
      // Form: SDWC%%<MARKER>%%<json>
      var afterPrefix = line.Substring(SdwcPrefix.Length);
      var separator = afterPrefix.IndexOf("%%", StringComparison.Ordinal);
      if (separator < 0)
      {
         Logger.Trace("Dropping malformed SDWC line (no marker separator): {0}", line);
         return;
      }

      var marker = afterPrefix.Substring(0, separator);
      var json = afterPrefix.Substring(separator + 2);

      SdwcCorrelationKey key;
      try
      {
         key = BuildKey(marker, json);
      }
      catch (Exception ex)
      {
         Logger.Trace(ex, "Dropping unparseable SDWC response for marker {0}.", marker);
         return;
      }

      if (!_correlator.Complete(key, json))
         Logger.Trace("No pending SDWC request matched key {0}; dropping response.", key);
   }

   private static SdwcCorrelationKey BuildKey(string marker, string json)
   {
      using var document = System.Text.Json.JsonDocument.Parse(json);
      var root = document.RootElement;
      var objectId = SdwcMapping.ParseObjectId(root.GetProperty("object"));

      string? name = null;
      switch (marker)
      {
         case SdwcQueryProvider.VerbOverlayMarker:
            name = root.TryGetProperty("verb", out var verb) ? verb.GetString() : null;
            break;
         case SdwcQueryProvider.PropOverlayMarker:
            name = root.TryGetProperty("property", out var property) ? property.GetString() : null;
            break;
      }

      return new SdwcCorrelationKey(marker, objectId, name);
   }
}
