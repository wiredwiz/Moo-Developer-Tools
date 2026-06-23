# Query-provider teardown on disconnect — design (udd-bju)

**Date:** 2026-06-23
**Status:** Approved (design), pending spec review
**Bead:** udd-bju (bug, P2)

## Problem

There is no session-teardown path for MOO world query providers. Nothing calls
`QueryProviders.Unregister` when a terminal connection ends, and pending correlator entries dangle
until their bounded timeout. After a disconnect:

- A consumer hitting `session.QueryProviders.Query` still routes to the now-dead `McpQueryProvider`,
  which calls `SendOutOfBandLine` on a dead terminal — burning the full ~10 s timeout per call.
- In-flight requests (awaiting a `TaskCompletionSource` in the correlator) are never faulted; they
  too wait out the timeout.
- The same applies to `SdwcQueryProvider` / `SdwcOobHandler`.

`McpQueryPackage.Reset()` is an empty no-op nobody calls.

## Key findings (current architecture)

- **Registration is per connection, into a per-session registry.** Providers register into
  `session.QueryProviders` (a `MooWorldQueryService`/`MooWorldQueryProviderRegistry` owned by
  `MudClientSession`): `McpQueryProvider` (priority 200) when MCP negotiation confirms
  `edgerunner-org-moo-query` (`McpQueryPackage.OnPackageSupported`); `SdwcQueryProvider` (priority
  100) on the SDWC capability signal (`SdwcOobHandler.EnsureProviderRegistered`).
- **Pending requests** live in a correlator per source — `McpQueryPackage` owns an
  `McpQueryCorrelator`; `SdwcOobHandler` owns an `SdwcCorrelator` — each a
  `ConcurrentDictionary<key, TaskCompletionSource<string>>`. There is **no bulk-fault** method.
- **Dead-terminal send is silent.** `MudClientSession.SendData` swallows `IOException`, so a send on
  a dead terminal does not throw; the caller just waits out the timeout.
- **The disconnect signal already exists:** `MudClientSession.Closed` fires once in
  `ReadFromConnection().finally`. Its only current listener (`MooClientTerminal.Session_Closed`)
  does UI cleanup — no provider/query teardown.
- **`Reset()` is the wrong hook.** `RootMessageProcessor` calls `OutOfBandMessageProcessor.Reset()`
  after *every* completed OOB exchange (or OOB timeout), cascading to handlers. Putting teardown in
  `Reset()` would unregister providers mid-session. The correct hook is `Closed`.
- **Lifetime / reconnect:** `OutOfBandMessageProcessor`, `McpOobHandler`, `McpQueryPackage`,
  `SdwcOobHandler` and their correlators are created **once per `TerminalPage`**
  (`WindowManager.CreateTerminalPage`) and **persist across reconnects** within that tab. So
  disconnect ≠ disposal: disconnect teardown must leave each source **reusable** (re-register on the
  next negotiate / capability signal); disposal happens when the tab/session is torn down.

## Approach

A **hybrid**: a deterministic, reuse-safe disconnect teardown fired once off `Closed`, plus
`IDisposable` as a leak-proof safety net for final teardown.

- `OnDisconnected()` (deterministic, reuse-safe) is the primary path.
- `IDisposable` guarantees pending tasks are faulted and providers unregistered even if `Closed`
  never fires (abnormal teardown, app exit, or any future path that skips it).

## Components

### 1. Exception — `Org.Edgerunner.Mud.Common/Querying/QueryConnectionClosedException.cs`

```csharp
public sealed class QueryConnectionClosedException : Exception
{
   public QueryConnectionClosedException()
      : base("The query failed because the connection to the world was closed.") { }
   public QueryConnectionClosedException(string message) : base(message) { }
   public QueryConnectionClosedException(string message, Exception inner) : base(message, inner) { }
}
```
Lives in `Common.Querying` (both `Org.Edgerunner.Mud.MCP` and `Org.Edgerunner.Mud.Communication`
already reference it). Pending query tasks are faulted with this so callers can distinguish
"connection closed" from a `TimeoutException`.

### 2. Correlators — `McpQueryCorrelator`, `SdwcCorrelator`

Both gain:
- `void FaultAll(Exception exception)` — snapshot and `TryRemove` each pending entry, calling
  `TrySetException(exception)`; clears the dictionary. **Reusable afterward** (no disposed flag set),
  so the same correlator serves the next connection.
- `IDisposable`: `Dispose()` calls `FaultAll(new QueryConnectionClosedException())` and sets a
  `_disposed` flag. After disposal, `CreatePending(...)` returns an **already-faulted** task
  (faulted with `QueryConnectionClosedException`) instead of registering a new entry — so nothing
  can hang post-teardown. `Complete`/`CompleteError`/`Remove` become safe no-ops when disposed.

### 3. Per-source teardown

**`McpQueryPackage`:**
- Stash the registry reference at registration time (`OnPackageSupported` already has `client`;
  keep `client.QueryProviders` or the `client`).
- `void OnDisconnected()`:
  - `_queryProviders?.Unregister(_provider)` (if registered),
  - `_correlator.FaultAll(new QueryConnectionClosedException())`,
  - reset `_provider = null; _providerKey = null;` so the next negotiate re-registers a fresh
    provider into the (possibly new) registry.
- `IDisposable.Dispose()`: unregister `_provider`, `_correlator.Dispose()`, mark disposed. (Final
  teardown — no reuse.)

**`SdwcOobHandler`:**
- `void OnDisconnected()`: unregister `_provider`, `_correlator.FaultAll(...)`, reset `_provider`
  and the "registered once" flag so the next capability signal re-registers.
- `IDisposable.Dispose()`: unregister `_provider`, `_correlator.Dispose()`.

### 4. Cascade + trigger

- **`IMcpPackage`** gains `void OnDisconnected();` (and `IDisposable`). Provider-less packages
  (`SimpleEditPackage`, `McpNegotiatePackage`, `McpCordPackage`) implement it as a no-op (and a
  no-op/minimal `Dispose`).
- **`McpMessageDispatcher`** gains `OnDisconnected()` (and `Dispose()`) that cascade to every entry
  in `_packages`.
- **`McpOobHandler`** gains `OnDisconnected()` → `dispatcher.OnDisconnected()` (and `Dispose`).
- **OOB-handler contract** (`IOutOfBandMessageHandler`) gains `OnDisconnected()`;
  `OutOfBandMessageProcessor.OnDisconnected()` cascades to all handlers (`LocalEditHandler` no-op,
  `McpOobHandler`, `SdwcOobHandler`). `RootMessageProcessor.OnDisconnected()` →
  `OutOfBandMessageProcessor.OnDisconnected()`.
- **Trigger:** on `MudClientSession.Closed`, the owner (`MooClientTerminal`/`TerminalPage`, which
  holds the `RootMessageProcessor`) calls `RootMessageProcessor.OnDisconnected()` exactly once per
  disconnect.
- **Final disposal (safety net):** `OutOfBandMessageProcessor` → handlers → packages → correlators
  implement `IDisposable`. When the `TerminalPage`/session is disposed (tab closed), it disposes the
  processor chain, guaranteeing fault + unregister even if `Closed` never fired.

### 5. Registry

`MooWorldQueryProviderRegistry.QueryAsync` is left unchanged (still only swallows
`NotImplementedException`). Unregister stops dead-provider routing; `FaultAll` handles anything
in-flight. No behavior change there.

## Data flow on disconnect

1. Socket dies → `MudClientSession.ReadFromConnection` finally → `Closed` fires.
2. Owner calls `RootMessageProcessor.OnDisconnected()` →
   `OutOfBandMessageProcessor.OnDisconnected()` → each handler's `OnDisconnected()`.
3. `McpOobHandler` cascades to packages → `McpQueryPackage.OnDisconnected()`: unregister provider,
   `FaultAll`, reset. `SdwcOobHandler.OnDisconnected()`: same.
4. Any in-flight query awaiting a pending TCS faults immediately with `QueryConnectionClosedException`.
5. A new query after this finds no registered provider → registry returns the fallback (no hang).
6. On reconnect, negotiation / capability signal re-registers fresh providers.

## Testing

- **Correlator:** `FaultAll(ex)` faults every pending task with `ex` and leaves the correlator
  usable (a subsequent `CreatePending` works). `Dispose()` faults pending and, afterward,
  `CreatePending` returns an already-faulted `QueryConnectionClosedException` task;
  `Complete`/`Remove` are safe no-ops.
- **`McpQueryPackage.OnDisconnected`:** after a registered provider + an in-flight query, calling it
  (a) unregisters the provider so the registry no longer routes to it, and (b) faults the in-flight
  query with `QueryConnectionClosedException` (not a timeout). A subsequent `OnPackageSupported`
  re-registers a fresh provider.
- **`SdwcOobHandler.OnDisconnected`:** same, with re-registration on the next capability signal.
- **Cascade/trigger:** `RootMessageProcessor.OnDisconnected()` reaches both sources; firing it twice
  is idempotent (no throw). Wire-level: a fake terminal/session whose `Closed` triggers teardown.
- **Dispose:** disposing the processor chain faults pending and unregisters providers.

## Out of scope

- Changing the bounded request timeout or the registry's fall-through policy.
- Auto-reconnect behavior (only that teardown leaves sources re-registerable).
- Any UI/notification on disconnect beyond existing behavior.
