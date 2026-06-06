# Code Review — Bug Findings
**Date:** 2026-06-06  
**Branch:** Feature/Better-Buffer (post-merge to master)  
**Reviewer:** Claude Sonnet 4.6 (multi-angle automated review)

---

## Finding 1 — CONFIRMED · CRITICAL
**File:** `Org.Edgerunner.Mud.Communication/MudClientSession.cs:339`  
**Summary:** Partial lines (no `\n`) are never flushed while the connection is alive — login prompts and any interactive prompt are invisible until disconnect.

**Failure scenario:** A server sends `Password: ` with no trailing newline. The bytes accumulate in `_lineBuffer`. `FlushCommandBuffer` is only called in the `finally` block (on connection close). The partial line is never dispatched to `CommandChannel` and never displayed. Every MOO login prompt and any verb that writes partial output expecting user input is broken.

---

## Finding 2 — CONFIRMED · HIGH
**File:** `Org.Edgerunner.Moo.Editor/Controls/MooClientTerminal.cs:685`  
**Summary:** `ConnectAsync` spawns a second `PerformReadFromChannel` task on reconnect with no guard, violating the `SingleReader` contract on `CommandChannel`.

**Failure scenario:** User connects, server disconnects (`Session_Closed` fires but `TokenSource` is not cancelled). User reconnects via the same terminal page. `ConnectAsync` replaces `_Session` with a new one (new `SingleReader` channel) and calls `ReadFromChannel()` again. The old `PerformReadFromChannel` task is still alive; when its `WaitToReadAsync` on the old channel drains, it re-reads `_Session.CommandChannel` — now the NEW channel — alongside the new task. Two concurrent readers on a `SingleReader` channel causes messages to be silently dropped or processed twice.

---

## Finding 3 — CONFIRMED · HIGH
**File:** `Org.Edgerunner.Moo.Editor/Controls/MooClientTerminal.cs:637`  
**Summary:** `ConnectAsync` never unsubscribes `Closed`/`DataDropped`/`MessageReceived` from the previous `_Session` before replacing it.

**Failure scenario:** On reconnect, the old session's `ReadFromConnection` finally-block eventually fires `OnClosed()` → `BeginInvokes Session_Closed` → `consoleSim.WriteLine("** Connection closed **")` appears spuriously on the new live connection. With repeated reconnects, stale delegate references accumulate, preventing GC of old session objects and firing spurious events on every old session close.

---

## Finding 4 — CONFIRMED · MEDIUM
**File:** `Org.Edgerunner.Mud.Communication/MudClientSession.cs:413`  
**Summary:** `FlushLine` passes `TokenSource.Token` to `WriteAsync` — if the token is cancelled before the last complete line is written, that line is silently lost.

**Failure scenario:** `Close()` cancels `TokenSource`. `ProcessReadBuffer` is mid-execution on the final read chunk. `FlushLine` clears `_lineBuffer` then calls `WriteAsync` with the already-cancelled token — it throws `OperationCanceledException`. The line is gone: `_lineBuffer` is already cleared so `FlushCommandBuffer` (which correctly uses `CancellationToken.None`) sees nothing to flush. The last complete line of server output before disconnect is silently dropped.

---

## Finding 5 — PLAUSIBLE · MEDIUM
**File:** `Org.Edgerunner.Moo.Editor/Controls/MooClientTerminal.cs:675`  
**Summary:** `Session_DataDropped` writes to the current `_Session.CommandChannel` field, not the session that fired the event — wrong channel on reconnect.

**Failure scenario:** Old session fires `DataDropped` via `BeginInvoke` (async). Before the UI thread executes `Session_DataDropped`, `ConnectAsync` has replaced `_Session` with a new session. The truncation warning is `TryWrite`'d into the NEW session's channel, appearing on the new connection's terminal output without cause. The old session's warning is effectively lost.

---

## Finding 6 — PLAUSIBLE · MEDIUM
**File:** `Org.Edgerunner.Moo.Editor/Controls/MooClientTerminal.cs:742`  
**Summary:** `Session_Closed` calls `consoleSim.WriteLine` without an `IsHandleCreated` guard — throws if the terminal control is disposed when the `BeginInvoke` callback fires.

**Failure scenario:** Terminal is closed by the user: `OnHandleDestroyed` calls `Close()`, which cancels `TokenSource`. `ReadFromConnection` finally fires `OnClosed()` via `BeginInvoke`. The `WM_DESTROY` sequence continues disposing the control. The `BeginInvoke` callback is delivered after handle destruction; `consoleSim.WriteLine` calls `AppendText` on a disposed `FastColoredTextBox`, throwing `ObjectDisposedException` on the UI thread with no surrounding catch.

---

## Finding 7 — PLAUSIBLE · MEDIUM
**File:** `Org.Edgerunner.Moo.Editor/Controls/MooClientTerminal.cs:763`  
**Summary:** `OnScrollTimerTick` can fire after `consoleSim` is disposed — WinForms does not drain queued `WM_TIMER` messages on `Stop()`/`Dispose()`.

**Failure scenario:** A `WM_TIMER` message is queued at the moment `Close()` calls `_scrollTimer.Stop()`. `Stop()` does not flush the Win32 message queue. The tick fires after the control handle is destroyed; `consoleSim.GoEnd()` accesses `FastColoredTextBox` internals on a disposed control, potentially throwing `ObjectDisposedException` from `DoCaretVisible()`.

---

## Finding 8 — CONFIRMED · LOW
**File:** `Org.Edgerunner.Mud.Communication/MudClientSession.cs:404`  
**Summary:** `FlushLine` unconditionally writes `\n` past the `MaxLineBytes` cap — violates the 20MB safety limit by one byte.

**Failure scenario:** A line arrives at exactly `MaxLineBytes` of content. `WriteByteToLine` correctly caps and starts counting `_droppedLineBytes`. `FlushLine` then writes `\n` via `GetSpan(1)`/`Advance(1)` without checking `WrittenCount` — `ArrayBufferWriter` silently grows past `MaxLineBytes`. The decoded string is 20MB + 1 byte. Minor in isolation but breaks the documented invariant.

---

## Finding 9 — CONFIRMED · LOW
**File:** `Org.Edgerunner.Mud.Communication/MudClientSession.cs:389`  
**Summary:** `DataDropped` never fires for a line that never terminates — user gets no warning for a runaway stream without newlines.

**Failure scenario:** Server sends a continuous binary blob or infinite output with no newline. `WriteByteToLine` caps at 20MB and accumulates `_droppedLineBytes`. `OnDataDropped` is only called inside `FlushLine` (needs `\n`) or `FlushCommandBuffer` (connection close). The warning never appears while the connection is live, regardless of how many bytes are dropped.

---

## Finding 10 — PLAUSIBLE · LOW
**File:** `Org.Edgerunner.Mud.Communication/MudClientSession.cs:321`  
**Summary:** Zero-byte `ReadAsync` return treated as permanent EOF — `SslStream` can return 0 transiently during TLS renegotiation.

**Failure scenario:** `TlsMudClientSession` wraps the stream in `SslStream`. `SslStream.ReadAsync` can return 0 bytes during a TLS renegotiation handshake without the connection being closed. The new code hits `if (bytes == 0) break` and exits the loop permanently, fires `OnClosed()`, and drops the connection as if the server disconnected — even though the TLS session was live.
