# Terminal Buffer Cutoff — Investigation Findings

**Date:** 2026-06-01  
**Symptom:** When a MUD server sends a large burst of text, the terminal only shows the last N characters/lines. Earlier output appears cut off.

---

## Data Flow (Socket → Screen)

```
TCP socket
  └─ ReadFromConnection() [MudClientSession.cs:314]
       └─ ProcessReadBuffer() [MudClientSession.cs:344]
            └─ CommunicationBuffer (circular, 80KB) [MudClientSession.cs:62]
                 └─ CommandChannel (unbounded Channel<string>) [MudClientSession.cs:63]
                      └─ PerformReadFromChannel() [MooClientTerminal.cs:679]
                           └─ Invoke(SafeWrite) → consoleSim.WriteAnsi() [ConsoleWindowEmulator.cs:153]
                                └─ FastColoredTextBox.AppendText() / GoEnd()
```

---

## Issue 1 — Root Cause of Burst Bottleneck (Priority: HIGH)

**Files:** `MooClientTerminal.cs:679`, `ConsoleWindowEmulator.cs:153,194`

### The problem

`PerformReadFromChannel` calls **synchronous `Invoke`** (not `BeginInvoke`) for every line:

```csharp
// MooClientTerminal.cs:698
Invoke(SafeWrite);  // blocks background thread until UI thread finishes
```

Inside `SafeWrite`, the call chain is:
1. `consoleSim.WriteAnsi(text)` — appends text, then calls `Application.DoEvents()` (line 194)
2. `consoleSim.GoEnd()` — calls `DoCaretVisible()` → **`Recalc()`** → full word-wrap recalculation

`Recalc()` recalculates word-wrap positions for the **entire buffer** every time it's called. As the terminal accumulates lines, this gets progressively slower. For an N-line burst, `Recalc()` is called N times.

### Why DoEvents is there (intentional)

`Application.DoEvents()` in `WriteAnsi` forces a `WM_PAINT` to fire between lines, giving the streaming per-line appearance. Without it, multiple `Invoke` messages queue up and all lines appear at once (chunky). This is the right UX intent for a MUD client — **do not remove DoEvents without a replacement mechanism.**

### The fix

Remove `GoEnd()` from the per-line `SafeWrite` path. Replace it with a `System.Windows.Forms.Timer` (UI-thread timer) at ~100ms interval that calls `GoEnd()` once per tick, but only when content has been added and the user is at the bottom.

- Text still appears per-line via `Invoke + DoEvents` (streaming preserved) ✓
- `Recalc()` runs at most 10×/second instead of N×/burst ✓
- 100ms scroll-tracking lag is imperceptible ✓
- A 500-line burst: 500 `Recalc()` calls → ~5 `Recalc()` calls ✓

**Locations to change:**
- `MooClientTerminal.cs:679` — `PerformReadFromChannel`: remove `GoEnd()` from `SafeWrite`
- `MooClientTerminal.cs` — add a `System.Windows.Forms.Timer` field; start it in constructor; stop/dispose it in `Close()`
- Timer tick handler: check `atBottom`, call `consoleSim.GoEnd()` if true and new lines were written since last tick

---

## Issue 2 — Silent Data Loss: 80KB Per-Line Hard Limit (Priority: MEDIUM)

**Files:** `MudClientSession.cs:62`, `Buffers/CommunicationBuffer.cs`, `Org.Edgerunner.Common/Buffers/CircularBuffer.cs:285`

### The problem

```csharp
// MudClientSession.cs:62
CommandBuffer = new CommunicationBuffer(80000);  // circular, drop-oldest
```

`CommunicationBuffer` inherits from `ConcurrentCircularBuffer<byte>` → `CircularBuffer<byte>`. `PushBack()` (CircularBuffer.cs:285) silently drops the **oldest** byte when full:

```csharp
if (IsFull)
{
    popped = Buffer[_End];   // oldest byte discarded
    Buffer[_End] = item;
    Increment(ref _End);
    _Start = _End;           // data is gone
}
```

`BufferData()` (MudClientSession.cs:395) detects the overflow and counts dropped bytes, but `OnDataDropped` fires **after** the data is permanently gone.

The buffer is used as a **per-line** accumulator — it is cleared on each `\n` (ProcessReadBuffer:357-369). So the 80KB limit applies per logical line, not per burst.

### Impact

- Lines under 80KB: no loss (typical MUD text is well under 1KB/line)
- Lines over 80KB: the **beginning** of that line is silently dropped; only the last 80KB is written to `CommandChannel`
- Triggers for: `@dump` on large objects, raw data without newlines, very long property values

### Structural concern

A circular drop-oldest buffer is semantically wrong for a TCP stream. TCP guarantees ordered, lossless delivery — the application layer should not silently discard received bytes.

### The fix

Replace `CommunicationBuffer` (circular) with a growable per-line accumulator. Options:
- `List<byte>` cleared on each `\n`
- `System.IO.MemoryStream` reset on each `\n`
- `System.Buffers.ArrayBufferWriter<byte>` reset on each `\n`

No hard size limit. The buffer only holds one line at a time and is drained on `\n`, so memory usage is bounded by the longest single line received.

---

## Issue 3 — Read Loop Architecture (Priority: LOW)

**File:** `MudClientSession.cs:314`

### 3a — Thread.Sleep(5) poll loop

```csharp
// MudClientSession.cs:319
Thread.Sleep(5);   // blocks a thread-pool thread every iteration
while (_Stream != null && Client.Available > 0)
{
    var bytes = await _Stream.ReadAsync(buffer, 0, buffer.Length, ...);
```

`Thread.Sleep` inside `async void` blocks a thread-pool thread for 5ms per outer loop iteration. Data is not lost, but there is a 5ms polling latency before each read cycle.

**Fix:** Replace polling with a continuous async read loop — call `ReadAsync` directly in a loop without checking `Available` or sleeping. The `ReadAsync` call blocks asynchronously until data arrives, which is the correct pattern for async stream reading.

### 3b — `\r\n` at chunk boundary bug

```csharp
// MudClientSession.cs:350
if (buffer[i] == '\r' && bytes - i > 1 && buffer[i + 1] == '\n')
{
    // skip \r
}
```

The bounds check `bytes - i > 1` fails when `\r` is the **last byte of a 10KB read chunk** and `\n` is the first byte of the next chunk. In that case the `\r` falls through to `BufferData()` and is stored as a literal `\r` character in the output.

**Fix:** Track a `_lastByteWasCR` flag across calls to `ProcessReadBuffer` and skip `\n` at the start of the next chunk if the previous chunk ended with `\r`.

### 3c — `Client.Available` unreliable under TLS

```csharp
// MudClientSession.cs:322
while (_Stream != null && Client.Available > 0)
```

`Client.Available` counts bytes in the OS socket receive buffer — encrypted bytes for TLS. An `SslStream` decrypts TLS records as complete units. The inner loop can exit early because `Available > 0` does not mean there is a complete TLS record ready to decrypt and return from `ReadAsync`.

**Fix:** Covered by fix for 3a — a continuous `ReadAsync` loop removes the need for `Available` checking entirely.

---

## Issue 4 — TlsMudClientSession Inherits All of the Above

**File:** `TlsMudClientSession.cs`

`TlsMudClientSession` is a thin subclass of `MudClientSession` that only overrides `GetStream()` to return an `SslStream`. All of the above issues apply equally to TLS connections. The `Client.Available` problem (3c) is particularly relevant for TLS users.

---

## Summary Table

| # | Issue | File(s) | Type | Trigger |
|---|-------|---------|------|---------|
| 1 | `GoEnd()` / `Recalc()` called per line — burst bottleneck | `MooClientTerminal.cs:692`, `FastColoredTextBox.cs` | Display bottleneck | Any burst > ~50 lines |
| 2 | 80KB circular drop-oldest per-line buffer | `MudClientSession.cs:62`, `CircularBuffer.cs:285` | Silent data loss | Single line > 80,000 bytes |
| 3a | `Thread.Sleep(5)` polling read loop | `MudClientSession.cs:319` | Latency / wasted thread | Always |
| 3b | `\r\n` split across 10KB read chunks | `MudClientSession.cs:350` | Stray `\r` in output | Chunk-boundary `\r\n` |
| 3c | `Client.Available` unreliable under TLS | `MudClientSession.cs:322` | Early read termination | TLS connections |

---

## Recommended Fix Order

1. **Issue 1** — Move `GoEnd()` to a 100ms timer. Highest impact; fixes the reported symptom for typical MUD bursts.
2. **Issue 2** — Replace `CommunicationBuffer` with a growable per-line accumulator. Eliminates the silent data loss risk.
3. **Issues 3a/3b/3c** — Rewrite `ReadFromConnection` as a continuous async read loop with `\r` state tracking. Lower urgency but cleans up the architecture.
