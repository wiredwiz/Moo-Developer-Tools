#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="McpQueryCorrelator.cs">
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
using Org.Edgerunner.Mud.MCP.Exceptions;

namespace Org.Edgerunner.Mud.MCP.Packages;

/// <summary>
/// Thread-safe map of in-flight <c>edgerunner-org-moo-query</c> requests keyed by tag. Tags come
/// from an <see cref="Interlocked"/> counter and are unique by construction.
/// </summary>
public class McpQueryCorrelator : IDisposable
{
   private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pending = new();

   private int _tagCounter;

   private volatile bool _disposed;

   /// <summary>
   /// Generates the next unique request tag.
   /// </summary>
   /// <returns>The tag.</returns>
   public string NextTag() => Interlocked.Increment(ref _tagCounter).ToString();

   /// <summary>
   /// Registers a pending request for the given tag.
   /// </summary>
   /// <param name="tag">The request tag.</param>
   /// <returns>A task that completes with the reply's JSON payload.</returns>
   public Task<string> CreatePending(string tag)
   {
      var source = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

      // After disposal nothing more may register: hand back an already-faulted task so any straggling
      // caller fails fast with the closed-connection error rather than hanging forever.
      if (_disposed)
      {
         source.TrySetException(new QueryConnectionClosedException());
         ObserveFault(source);
         return source.Task;
      }

      _pending[tag] = source;

      // Observe faults eagerly: if the awaiting consumer times out and abandons this task while a
      // racing CompleteError still faults it, the exception must not surface as unobserved.
      ObserveFault(source);

      return source.Task;
   }

   /// <summary>
   /// Attaches a continuation that observes any fault on the supplied source so an abandoned, faulted
   /// task never surfaces as an unobserved exception.
   /// </summary>
   /// <param name="source">The pending task source.</param>
   private static void ObserveFault(TaskCompletionSource<string> source)
   {
      _ = source.Task.ContinueWith(
         static t => _ = t.Exception,
         CancellationToken.None,
         TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
         TaskScheduler.Default);
   }

   /// <summary>
   /// Completes the pending request for the given tag with a JSON payload.
   /// </summary>
   /// <param name="tag">The reply tag.</param>
   /// <param name="payload">The reassembled JSON payload.</param>
   /// <returns><c>true</c> when a pending request was completed; <c>false</c> for unknown/stale tags.</returns>
   public bool Complete(string tag, string payload) =>
      !_disposed && _pending.TryRemove(tag, out var source) && source.TrySetResult(payload);

   /// <summary>
   /// Faults the pending request for the given tag with a server error.
   /// </summary>
   /// <param name="tag">The reply tag.</param>
   /// <param name="error">The typed server error.</param>
   /// <returns><c>true</c> when a pending request was faulted; <c>false</c> for unknown/stale tags.</returns>
   public bool CompleteError(string tag, McpQueryErrorException error) =>
      !_disposed && _pending.TryRemove(tag, out var source) && source.TrySetException(error);

   /// <summary>
   /// Removes the pending request for the given tag, if any.
   /// </summary>
   /// <param name="tag">The request tag.</param>
   public void Remove(string tag)
   {
      if (_disposed)
         return;

      _pending.TryRemove(tag, out _);
   }

   /// <summary>
   /// Faults every pending request with the supplied exception and clears the map. The correlator
   /// remains usable afterward (no disposed flag is set), so the same instance can serve the next
   /// connection. Used by the deterministic disconnect-teardown path.
   /// </summary>
   /// <param name="exception">The exception to fault every awaiting task with.</param>
   public void FaultAll(Exception exception)
   {
      foreach (var tag in _pending.Keys.ToArray())
         if (_pending.TryRemove(tag, out var source))
            source.TrySetException(exception);
   }

   /// <summary>
   /// Faults every pending request with a <see cref="QueryConnectionClosedException"/> and marks the
   /// correlator disposed. After disposal, <see cref="CreatePending"/> returns an already-faulted task
   /// and <see cref="Complete"/>/<see cref="CompleteError"/>/<see cref="Remove"/> become safe no-ops.
   /// </summary>
   public void Dispose()
   {
      if (_disposed)
         return;

      FaultAll(new QueryConnectionClosedException());
      _disposed = true;
   }
}
