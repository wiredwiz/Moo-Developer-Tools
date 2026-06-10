#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="SdwcQueryProvider.cs">
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

using Org.Edgerunner.Mud.Common.Querying;
using Org.Edgerunner.Mud.Communication.Interfaces;

namespace Org.Edgerunner.Mud.Communication.Sdwc;

/// <summary>
/// An <see cref="IMooWorldQueryProvider"/> implemented over the SDWC out-of-band protocol
/// (Sindome <c>dome-client</c>). Supports the four query operations the protocol exposes; every other
/// interface method throws <see cref="NotImplementedException"/> so the registry falls through to the
/// next provider.
/// </summary>
/// <remarks>
/// Each supported call registers a pending entry with the <see cref="SdwcCorrelator"/>, sends a
/// <c>#$# SDWC%%…</c> request (note the leading space carried in the OOB line), then awaits the
/// correlated JSON payload under the caller's cancellation token linked to a bounded timeout. On timeout
/// a <see cref="TimeoutException"/> is thrown; cancellation propagates; the pending entry is always
/// removed.
/// </remarks>
public sealed class SdwcQueryProvider : IMooWorldQueryProvider
{
   /// <summary>The marker for the <c>VERBS</c> request/response.</summary>
   public const string VerbsMarker = "VERBS";

   /// <summary>The marker for the <c>PROPS</c> request/response.</summary>
   public const string PropsMarker = "PROPS";

   /// <summary>The marker for the <c>VERB-OVERLAY</c> request/response.</summary>
   public const string VerbOverlayMarker = "VERB-OVERLAY";

   /// <summary>The marker for the <c>PROP-OVERLAY</c> request/response.</summary>
   public const string PropOverlayMarker = "PROP-OVERLAY";

   private readonly IClientTerminal _client;

   private readonly SdwcCorrelator _correlator;

   private readonly TimeSpan _timeout;

   /// <summary>
   /// Initializes a new instance of the <see cref="SdwcQueryProvider"/> class.
   /// </summary>
   /// <param name="client">The client terminal used to send OOB requests.</param>
   /// <param name="correlator">The correlator that matches responses to pending requests.</param>
   /// <param name="timeout">The bounded per-request timeout. Defaults to 10 seconds when <c>null</c>.</param>
   /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> or <paramref name="correlator"/> is <c>null</c>.</exception>
   public SdwcQueryProvider(IClientTerminal client, SdwcCorrelator correlator, TimeSpan? timeout = null)
   {
      _client = client ?? throw new ArgumentNullException(nameof(client));
      _correlator = correlator ?? throw new ArgumentNullException(nameof(correlator));
      _timeout = timeout ?? TimeSpan.FromSeconds(10);
   }

   /// <inheritdoc/>
   public async Task<IReadOnlyList<MooVerbSummary>> GetVerbsAsync(MooObjectId objectId, CancellationToken cancellationToken)
   {
      var key = new SdwcCorrelationKey(VerbsMarker, objectId, null);
      var json = await ExchangeAsync(key, $" SDWC%%{VerbsMarker}%%{objectId}", cancellationToken).ConfigureAwait(false);
      return SdwcMapping.MapVerbs(json);
   }

   /// <inheritdoc/>
   public async Task<IReadOnlyList<MooPropertySummary>> GetPropertiesAsync(MooObjectId objectId, CancellationToken cancellationToken)
   {
      var key = new SdwcCorrelationKey(PropsMarker, objectId, null);
      var json = await ExchangeAsync(key, $" SDWC%%{PropsMarker}%%{objectId}", cancellationToken).ConfigureAwait(false);
      return SdwcMapping.MapProperties(json);
   }

   /// <inheritdoc/>
   public async Task<MooVerbDocumentation?> GetVerbDocumentationAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken)
   {
      var key = new SdwcCorrelationKey(VerbOverlayMarker, objectId, verbName);
      var json = await ExchangeAsync(key, $" SDWC%%{VerbOverlayMarker}%%{objectId}%%{verbName}", cancellationToken).ConfigureAwait(false);
      return SdwcMapping.MapVerbOverlay(json);
   }

   /// <inheritdoc/>
   public async Task<IReadOnlyList<string>> GetPropertyDocumentationAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken)
   {
      var key = new SdwcCorrelationKey(PropOverlayMarker, objectId, propName);
      var json = await ExchangeAsync(key, $" SDWC%%{PropOverlayMarker}%%{objectId}%%{propName}", cancellationToken).ConfigureAwait(false);
      return SdwcMapping.MapPropOverlay(json);
   }

   /// <summary>
   /// Registers a pending request, sends the SDWC OOB line, and awaits the correlated JSON payload under
   /// the caller's cancellation token linked to the bounded timeout. Always removes the pending entry.
   /// </summary>
   /// <param name="key">The correlation key.</param>
   /// <param name="oobLine">The OOB line to send (with its leading space).</param>
   /// <param name="cancellationToken">The caller's cancellation token.</param>
   /// <returns>The response JSON payload.</returns>
   /// <exception cref="TimeoutException">Thrown when no response arrives within the bounded timeout.</exception>
   /// <exception cref="OperationCanceledException">Thrown when the caller cancels the operation.</exception>
   private async Task<string> ExchangeAsync(SdwcCorrelationKey key, string oobLine, CancellationToken cancellationToken)
   {
      var pending = _correlator.CreatePending(key);
      try
      {
         _client.SendOutOfBandLine(oobLine);

         using var timeoutSource = new CancellationTokenSource(_timeout);
         using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);

         var completed = await Task.WhenAny(pending, Task.Delay(Timeout.Infinite, linked.Token)).ConfigureAwait(false);
         if (completed == pending)
            return await pending.ConfigureAwait(false);

         // The delay won: distinguish a caller cancellation from a bounded timeout.
         if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

         throw new TimeoutException($"SDWC request '{key.Marker}' for {key.ObjectId} timed out after {_timeout}.");
      }
      finally
      {
         _correlator.Remove(key);
      }
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetObjectsAsync(CancellationToken cancellationToken)
   {
      throw new NotImplementedException();
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetChildrenAsync(MooObjectId objectId, CancellationToken cancellationToken)
   {
      throw new NotImplementedException();
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(CancellationToken cancellationToken)
   {
      throw new NotImplementedException();
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(MooObjectId owner, CancellationToken cancellationToken)
   {
      throw new NotImplementedException();
   }

   /// <inheritdoc/>
   public Task<MooObjectId?> GetParentAsync(MooObjectId objectId, CancellationToken cancellationToken)
   {
      throw new NotImplementedException();
   }

   /// <inheritdoc/>
   public Task<MooVerbInfo?> GetVerbInfoAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken)
   {
      throw new NotImplementedException();
   }

   /// <inheritdoc/>
   public Task<MooPropertyInfo?> GetPropertyInfoAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken)
   {
      throw new NotImplementedException();
   }

   /// <inheritdoc/>
   public Task<MooVerbCode?> GetVerbCodeAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken)
   {
      throw new NotImplementedException();
   }

   /// <inheritdoc/>
   public Task<MooPropertyValue?> GetPropertyValueAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken)
   {
      throw new NotImplementedException();
   }
}
