#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="McpQueryProvider.cs">
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

using System.Text.Json;
using NLog;
using Org.Edgerunner.Mud.Common.Querying;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP.Exceptions;

namespace Org.Edgerunner.Mud.MCP.Packages;

/// <summary>
/// An <see cref="IMooWorldQueryProvider"/> implemented over the <c>edgerunner-org-moo-query</c>
/// MCP 2.1 package. Covers all interface operations; see
/// <c>docs/edgerunner-org-moo-query-protocol.md</c> for the wire protocol.
/// </summary>
/// <remarks>
/// Each call registers a pending entry with the <see cref="McpQueryCorrelator"/> under a fresh tag,
/// sends a single-line MCP request, then awaits the tag-correlated JSON reply under the caller's
/// cancellation token linked to a bounded timeout. Server <c>-error</c> replies and unparseable
/// payloads degrade to the interface contract value (<c>null</c>/empty) but are always logged;
/// timeouts throw <see cref="TimeoutException"/>; cancellation propagates.
/// </remarks>
public sealed class McpQueryProvider : IMooWorldQueryProvider
{
   /// <summary>The MCP message-name prefix shared by every message of this package.</summary>
   public const string MessagePrefix = "edgerunner-org-moo-query";

   private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

   private static readonly HashSet<string> KnownErrorCodes = new(StringComparer.OrdinalIgnoreCase)
   {
      "E_NONE", "E_TYPE", "E_DIV", "E_PERM", "E_PROPNF", "E_VERBNF", "E_VARNF", "E_INVIND",
      "E_RECMOVE", "E_MAXREC", "E_RANGE", "E_ARGS", "E_NACC", "E_INVARG", "E_QUOTA", "E_FLOAT"
   };

   private readonly IClientTerminal _client;

   private readonly string _sessionKey;

   private readonly McpQueryCorrelator _correlator;

   private readonly TimeSpan _timeout;

   /// <summary>
   /// Initializes a new instance of the <see cref="McpQueryProvider"/> class.
   /// </summary>
   /// <param name="client">The client terminal used to send OOB requests.</param>
   /// <param name="sessionKey">The negotiated MCP session authentication key.</param>
   /// <param name="correlator">The correlator that matches replies to pending requests.</param>
   /// <param name="timeout">The bounded per-request timeout. Defaults to 10 seconds when <c>null</c>.</param>
   /// <exception cref="ArgumentNullException">Thrown when a required argument is <c>null</c>.</exception>
   public McpQueryProvider(IClientTerminal client, string sessionKey, McpQueryCorrelator correlator, TimeSpan? timeout = null)
   {
      _client = client ?? throw new ArgumentNullException(nameof(client));
      _sessionKey = sessionKey ?? throw new ArgumentNullException(nameof(sessionKey));
      _correlator = correlator ?? throw new ArgumentNullException(nameof(correlator));
      _timeout = timeout ?? TimeSpan.FromSeconds(10);
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetCoreObjectsAsync(CancellationToken cancellationToken) =>
      QueryAsync<IReadOnlyList<MooObjectSummary>>(
         "core-objects",
         new Dictionary<string, string>(),
         McpQueryMapping.MapObjectSummaries,
         Array.Empty<MooObjectSummary>(),
         cancellationToken);

   /// <inheritdoc/>
   public Task<MooObjectId?> GetCurrentPlayerAsync(CancellationToken cancellationToken) =>
      QueryAsync<MooObjectId?>(
         "player",
         new Dictionary<string, string>(),
         McpQueryMapping.MapCurrentPlayer,
         null,
         cancellationToken);

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetChildrenAsync(MooObjectId objectId, CancellationToken cancellationToken) =>
      QueryAsync<IReadOnlyList<MooObjectSummary>>(
         "children",
         new Dictionary<string, string> { ["object:"] = objectId.ToString() },
         McpQueryMapping.MapObjectSummaries,
         Array.Empty<MooObjectSummary>(),
         cancellationToken);

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(CancellationToken cancellationToken) =>
      QueryAsync<IReadOnlyList<MooObjectSummary>>(
         "owned",
         new Dictionary<string, string> { ["owner:"] = string.Empty },
         McpQueryMapping.MapObjectSummaries,
         Array.Empty<MooObjectSummary>(),
         cancellationToken);

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(MooObjectId owner, CancellationToken cancellationToken) =>
      QueryAsync<IReadOnlyList<MooObjectSummary>>(
         "owned",
         new Dictionary<string, string> { ["owner:"] = owner.ToString() },
         McpQueryMapping.MapObjectSummaries,
         Array.Empty<MooObjectSummary>(),
         cancellationToken);

   /// <inheritdoc/>
   public Task<MooObjectId?> GetParentAsync(MooObjectId objectId, CancellationToken cancellationToken) =>
      QueryAsync<MooObjectId?>(
         "parent",
         new Dictionary<string, string> { ["object:"] = objectId.ToString() },
         McpQueryMapping.MapParent,
         null,
         cancellationToken);

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooVerbSummary>> GetVerbsAsync(MooObjectId objectId, CancellationToken cancellationToken) =>
      QueryAsync<IReadOnlyList<MooVerbSummary>>(
         "verbs",
         new Dictionary<string, string> { ["object:"] = objectId.ToString() },
         json => McpQueryMapping.MapVerbSummaries(json, objectId),
         Array.Empty<MooVerbSummary>(),
         cancellationToken);

   /// <inheritdoc/>
   public Task<MooVerbInfo?> GetVerbInfoAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) =>
      QueryAsync<MooVerbInfo?>(
         "verb-info",
         new Dictionary<string, string> { ["object:"] = objectId.ToString(), ["verb:"] = verbName },
         McpQueryMapping.MapVerbInfo,
         null,
         cancellationToken);

   /// <inheritdoc/>
   public Task<MooVerbDocumentation?> GetVerbDocumentationAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) =>
      QueryAsync<MooVerbDocumentation?>(
         "verb-doc",
         new Dictionary<string, string> { ["object:"] = objectId.ToString(), ["verb:"] = verbName },
         McpQueryMapping.MapVerbDocumentation,
         null,
         cancellationToken);

   /// <inheritdoc/>
   public Task<MooVerbCode?> GetVerbCodeAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) =>
      QueryAsync<MooVerbCode?>(
         "verb-code",
         new Dictionary<string, string> { ["object:"] = objectId.ToString(), ["verb:"] = verbName },
         McpQueryMapping.MapVerbCode,
         null,
         cancellationToken);

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooPropertySummary>> GetPropertiesAsync(MooObjectId objectId, CancellationToken cancellationToken) =>
      QueryAsync<IReadOnlyList<MooPropertySummary>>(
         "props",
         new Dictionary<string, string> { ["object:"] = objectId.ToString() },
         json => McpQueryMapping.MapPropertySummaries(json, objectId),
         Array.Empty<MooPropertySummary>(),
         cancellationToken);

   /// <inheritdoc/>
   public Task<MooPropertyInfo?> GetPropertyInfoAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) =>
      QueryAsync<MooPropertyInfo?>(
         "prop-info",
         new Dictionary<string, string> { ["object:"] = objectId.ToString(), ["prop:"] = propName },
         json => McpQueryMapping.MapPropertyInfo(json, objectId),
         null,
         cancellationToken);

   /// <inheritdoc/>
   public Task<IReadOnlyList<string>> GetPropertyDocumentationAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) =>
      QueryAsync<IReadOnlyList<string>>(
         "prop-doc",
         new Dictionary<string, string> { ["object:"] = objectId.ToString(), ["prop:"] = propName },
         McpQueryMapping.MapLines,
         Array.Empty<string>(),
         cancellationToken);

   /// <inheritdoc/>
   public Task<MooPropertyValue?> GetPropertyValueAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) =>
      QueryAsync<MooPropertyValue?>(
         "prop-value",
         new Dictionary<string, string> { ["object:"] = objectId.ToString(), ["prop:"] = propName },
         McpQueryMapping.MapPropertyValue,
         null,
         cancellationToken);

   /// <summary>
   /// Sends one request and awaits its mapped reply: register pending (fresh tag) → format and send →
   /// await under linked caller/timeout tokens → map. Server errors and unparseable payloads degrade
   /// to <paramref name="degraded"/> and are always logged; the pending entry is always removed.
   /// </summary>
   /// <typeparam name="T">The mapped result type.</typeparam>
   /// <param name="operation">The message-name suffix (e.g. <c>verbs</c>).</param>
   /// <param name="parameters">The request parameters (keys carry their trailing colon).</param>
   /// <param name="map">The payload mapper.</param>
   /// <param name="degraded">The contract value returned on server error or unparseable payload.</param>
   /// <param name="cancellationToken">The caller's cancellation token.</param>
   /// <returns>The mapped result or <paramref name="degraded"/>.</returns>
   /// <exception cref="TimeoutException">Thrown when no reply arrives within the bounded timeout.</exception>
   /// <exception cref="OperationCanceledException">Thrown when the caller cancels the operation.</exception>
   private async Task<T> QueryAsync<T>(
      string operation,
      Dictionary<string, string> parameters,
      Func<string, T> map,
      T degraded,
      CancellationToken cancellationToken)
   {
      var tag = _correlator.NextTag();
      var pending = _correlator.CreatePending(tag);

      var data = new Dictionary<string, string> { ["tag:"] = tag };
      foreach (var (keyword, value) in parameters)
         data[keyword] = value;

      try
      {
         _client.SendOutOfBandLine(McpUtils.FormatMessage($"{MessagePrefix}-{operation}", _sessionKey, data));

         using var timeoutSource = new CancellationTokenSource(_timeout);
         using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);

         var completed = await Task.WhenAny(pending, Task.Delay(Timeout.Infinite, linked.Token)).ConfigureAwait(false);
         if (completed != pending)
         {
            // The delay won: distinguish a caller cancellation from a bounded timeout.
            if (cancellationToken.IsCancellationRequested)
               throw new OperationCanceledException(cancellationToken);

            Logger.Debug("MCP query '{0}' (tag {1}, {2}) timed out after {3}.", operation, tag, Describe(parameters), _timeout);
            throw new TimeoutException($"MCP query '{operation}' timed out after {_timeout}.");
         }

         string json;
         try
         {
            json = await pending.ConfigureAwait(false);
         }
         catch (McpQueryErrorException error)
         {
            if (KnownErrorCodes.Contains(error.Code))
               Logger.Debug(
                  "MCP query '{0}' (tag {1}, {2}) answered {3}: {4}",
                  operation, tag, Describe(parameters), error.Code, error.Message);
            else
               Logger.Warn(
                  "MCP query '{0}' (tag {1}, {2}) answered unrecognized error code {3}: {4}",
                  operation, tag, Describe(parameters), error.Code, error.Message);

            return degraded;
         }

         try
         {
            return map(json);
         }
         catch (Exception ex) when (ex is JsonException or FormatException or KeyNotFoundException or InvalidOperationException or IndexOutOfRangeException)
         {
            Logger.Warn(ex, "MCP query '{0}' (tag {1}) returned an unparseable payload ({2} chars).", operation, tag, json.Length);
            Logger.Trace("MCP query '{0}' (tag {1}) payload: {2}", operation, tag, json);
            return degraded;
         }
      }
      finally
      {
         _correlator.Remove(tag);
      }
   }

   private static string Describe(Dictionary<string, string> parameters) =>
      parameters.Count == 0 ? "no params" : string.Join(" ", parameters.Select(p => $"{p.Key} {p.Value}"));
}
