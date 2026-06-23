#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="SdwcCorrelator.cs">
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

using System.Collections.Concurrent;
using Org.Edgerunner.Mud.Common.Querying;

namespace Org.Edgerunner.Mud.Communication.Sdwc;

/// <summary>
/// Identifies a pending SDWC request, keyed by the request marker, the echoed object id and an
/// optional verb/property name (for the overlay markers).
/// </summary>
/// <param name="Marker">The SDWC marker (for example <c>VERBS</c>, <c>VERB-OVERLAY</c>).</param>
/// <param name="ObjectId">The object id echoed by the server response.</param>
/// <param name="Name">The verb or property name, for overlay requests; otherwise <c>null</c>.</param>
public readonly record struct SdwcCorrelationKey(string Marker, MooObjectId ObjectId, string? Name);

/// <summary>
/// Thread-safe store of pending SDWC requests. Each request is registered with a
/// <see cref="SdwcCorrelationKey"/> and produces a <see cref="Task{TResult}"/> that the caller awaits;
/// the inbound OOB handler later completes (or fails) the matching entry when the server responds.
/// Every terminal outcome (completion, failure, cancellation) removes the entry so the store cannot leak.
/// </summary>
public sealed class SdwcCorrelator : IDisposable
{
   private readonly ConcurrentDictionary<SdwcCorrelationKey, TaskCompletionSource<string>> _pending = new();

   private volatile bool _disposed;

   /// <summary>
   /// Gets the number of currently pending requests. Intended for diagnostics and tests.
   /// </summary>
   public int PendingCount => _pending.Count;

   /// <summary>
   /// Registers a pending request and returns the task that resolves with the correlated JSON payload.
   /// </summary>
   /// <param name="key">The correlation key.</param>
   /// <returns>A task that completes with the response JSON when the matching response arrives.</returns>
   /// <exception cref="InvalidOperationException">Thrown when a request with the same key is already pending.</exception>
   public Task<string> CreatePending(SdwcCorrelationKey key)
   {
      var source = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

      // After disposal nothing more may register: hand back an already-faulted task so any straggling
      // caller fails fast with the closed-connection error rather than hanging forever.
      if (_disposed)
      {
         source.TrySetException(new QueryConnectionClosedException());
         return source.Task;
      }

      if (!_pending.TryAdd(key, source))
         throw new InvalidOperationException($"A SDWC request for '{key}' is already pending.");

      return source.Task;
   }

   /// <summary>
   /// Completes the pending request for <paramref name="key"/> with the supplied JSON payload, removing it.
   /// </summary>
   /// <param name="key">The correlation key.</param>
   /// <param name="payload">The response JSON payload.</param>
   /// <returns><c>true</c> if a pending request was matched and completed; otherwise, <c>false</c>.</returns>
   public bool Complete(SdwcCorrelationKey key, string payload)
   {
      if (_disposed)
         return false;

      if (!_pending.TryRemove(key, out var source))
         return false;

      source.TrySetResult(payload);
      return true;
   }

   /// <summary>
   /// Fails the pending request for <paramref name="key"/> with the supplied exception, removing it.
   /// </summary>
   /// <param name="key">The correlation key.</param>
   /// <param name="exception">The exception to fault the awaiting task with.</param>
   /// <returns><c>true</c> if a pending request was matched and failed; otherwise, <c>false</c>.</returns>
   public bool Fail(SdwcCorrelationKey key, Exception exception)
   {
      if (_disposed)
         return false;

      if (!_pending.TryRemove(key, out var source))
         return false;

      source.TrySetException(exception);
      return true;
   }

   /// <summary>
   /// Removes the pending request for <paramref name="key"/> without resolving it (used on timeout or
   /// cancellation, where the awaiting task has already observed its own outcome).
   /// </summary>
   /// <param name="key">The correlation key.</param>
   /// <returns><c>true</c> if a pending request was removed; otherwise, <c>false</c>.</returns>
   public bool Remove(SdwcCorrelationKey key)
   {
      if (_disposed)
         return false;

      return _pending.TryRemove(key, out _);
   }

   /// <summary>
   /// Faults every pending request with the supplied exception and clears the store. The correlator
   /// remains usable afterward (no disposed flag is set), so the same instance can serve the next
   /// connection. Used by the deterministic disconnect-teardown path.
   /// </summary>
   /// <param name="exception">The exception to fault every awaiting task with.</param>
   public void FaultAll(Exception exception)
   {
      foreach (var key in _pending.Keys.ToArray())
         if (_pending.TryRemove(key, out var source))
            source.TrySetException(exception);
   }

   /// <summary>
   /// Faults every pending request with a <see cref="QueryConnectionClosedException"/> and marks the
   /// correlator disposed. After disposal, <see cref="CreatePending"/> returns an already-faulted task
   /// and <see cref="Complete"/>/<see cref="Fail"/>/<see cref="Remove"/> become safe no-ops.
   /// </summary>
   public void Dispose()
   {
      if (_disposed)
         return;

      FaultAll(new QueryConnectionClosedException());
      _disposed = true;
   }
}
