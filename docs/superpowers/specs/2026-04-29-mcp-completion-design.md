# MCP Completion Design Spec
**Date:** 2026-04-29
**Status:** Approved

---

## Context

The MUD Client Protocol (MCP) 2.1 infrastructure in `Org.Edgerunner.Mud.MCP` has partial implementation:

- `McpUtils.ParseMessage` / `SplitMessageIntoWords` — single-line message parser (complete)
- `McpClientSessionManager.NegotiationMcpSession()` — version negotiation (complete)
- `McpClientSession` — session model with key, version, `SupportedPackages` (complete)
- `McpUtils.GenerateSessionKey()` — random key generation (complete)
- `Message` class — wire message model (complete)
- All interfaces (`IMcpPackage`, `IMcpSession`, `IMcpProtocolHandler`, `IMcpConfiguration`) — defined, no concrete implementations
- `McpClientSessionManager` instantiated in `MooClientTerminal` but never connected to the live message stream

**Gaps to close:**
1. No multiline message support (`#$#*` / `#$#:`)
2. No outbound message formatting
3. MCP not wired into the OOB message pipeline
4. No `mcp-negotiate` package implementation
5. No `mcp-cord` package implementation
6. No unit tests

---

## Architecture Decision

MCP processing stays at the terminal level, integrated into the existing `OutOfBandMessageProcessor` pipeline as a registered `IOutOfBandMessageHandler`. This avoids splitting OOB handling across multiple layers (the OOB processor and local edit handler already live here).

All new code lives in `Org.Edgerunner.Mud.MCP` except for a small wiring change in `MooClientTerminal`. No new project references are needed — `Org.Edgerunner.Mud.MCP` already references `Org.Edgerunner.Mud.Communication`.

---

## Message Flow

```
RootMessageProcessor
  detects #$# prefix → strips it → routes to OutOfBandMessageProcessor

OutOfBandMessageProcessor
  iterates registered handlers → calls McpOobHandler.ProcessMessage()

McpOobHandler
  feeds line to McpMessageParser.FeedLine()

  Complete   → McpMessageDispatcher.Dispatch(client, message)
               parser.Reset()
               state.CurrentProcessor = null

  InProgress → state.CurrentProcessor = this
               (subsequent lines bypass OOB dispatch, come here directly)

  Error      → parser.Reset()
               state.CurrentProcessor = null  (drop silently per spec)

McpMessageDispatcher
  "mcp" (no key) → McpClientSessionManager.NegotiationMcpSession()
                   send handshake reply
                   send mcp-negotiate-can for each registered package
                   send mcp-negotiate-end
  all others     → validate auth key → route to IMcpPackage by name prefix

McpNegotiatePackage
  mcp-negotiate-can → record mutual package support in session
  mcp-negotiate-end → mark negotiation complete

McpCordPackage
  mcp-cord-open   → create McpCord, store by ID
  mcp-cord        → route to cord's package handler
  mcp-cord-closed → tear down cord
```

---

## Components

### 1. `McpMessageParser` (new — replaces `McpUtils.ParseMessage`)

A stateful per-client parser that handles both single-line and multiline MCP messages.

**API:**
```csharp
public enum McpParseState { InProgress, Complete, Error }

public class McpMessageParser
{
    public McpParseState FeedLine(string line);
    public Message? Result { get; private set; }
    public void Reset();
}
```

**State machine:**
- `Normal` state: classify each line independently
  - Line with no `*` keywords → parse immediately via `McpUtils.SplitMessageIntoWords`, emit `Complete`
  - Line with `*` keyword(s) → extract header fields + data-tag(s), switch to `InMultiline`
  - Line starting with `* ` → unexpected continuation; treat as `Error`
  - Line starting with `: ` → unexpected close; treat as `Error`
- `InMultiline` state:
  - `* <tag> <keyword>: <value>` → append value to field buffer for `<tag>`
  - `: <tag>` → assemble final `Message`, emit `Complete`, return to `Normal`
  - Anything else → `Error`

**Internal multiline storage:**
- `string _Name`, `string _Key` — from header line
- `Dictionary<string, string> _SimpleFields` — non-`*` fields from header
- `Dictionary<string, string> _DataTagToKeyword` — maps data-tag → field name (without `*`)
- `Dictionary<string, List<string>> _MultilineBuffers` — maps data-tag → accumulated lines

**Assembly:** on close tag, multiline field values are joined with `\n` and merged with simple fields into the final `Message.Data` dictionary. `Message` is constructed directly — no re-parsing.

**`McpUtils` changes:**
- `ParseMessage` — removed; `McpMessageParser` replaces it
- `SplitMessageIntoWords` — kept as `internal static`, used by `McpMessageParser`
- `GenerateSessionKey` — unchanged
- `FormatMessage(string name, string key, Dictionary<string, string> data)` — **new** static method for outbound message formatting

**Outbound format rules (`FormatMessage`):**
- Values containing spaces or special chars are double-quoted
- Auth key follows message name, separated by space
- Each keyword: value pair separated by space; colon is part of the keyword token

---

### 2. `McpMessageDispatcher` (new)

Routes fully-assembled `Message` objects to the correct `IMcpPackage`. One instance per client, held by `McpOobHandler`.

**Holds:**
- `McpClientSessionManager` — for version negotiation
- `McpClientSession?` — null until handshake completes
- `Dictionary<string, IMcpPackage>` — package registry keyed by package name

**Required packages** pre-registered on construction:
- `"mcp-negotiate"` → `McpNegotiatePackage`
- `"mcp-cord"` → `McpCordPackage`

**`Dispatch(IClientTerminal client, Message message)` logic:**
1. If `message.Name == "mcp"` and `message.Key` is empty → handshake:
   - Call `McpClientSessionManager.NegotiationMcpSession(message)`
   - If null (incompatible versions) → do nothing; MCP disabled for this session
   - If session returned → store it, send handshake reply, then immediately send `mcp-negotiate-can` for every registered package + `mcp-negotiate-end`
2. All other messages:
   - If no active session → drop silently
   - Validate `message.Key == session.Key` → drop silently on mismatch
   - Find package where `message.Name.StartsWith(packageName)` → call `package.ProcessMessage(message)`
   - No matching package → drop silently per spec

---

### 3. `McpOobHandler` (new — implements `IOutOfBandMessageHandler`)

Thin coordinator. Connects `McpMessageParser` to `McpMessageDispatcher` and manages `MessageProcessingState`.

```csharp
public class McpOobHandler : IOutOfBandMessageHandler
{
    public McpOobHandler(Version minVersion, Version maxVersion);

    public bool ProcessMessage(IClientTerminal client, string line, ref MessageProcessingState state);
    public void Reset();
}
```

**`ProcessMessage` body:**

```
var result = _Parser.FeedLine(line);

switch result:
  Complete   → _Dispatcher.Dispatch(client, _Parser.Result)
               _Parser.Reset()
               state.CurrentProcessor = null
               return true

  InProgress → state.CurrentProcessor = this
               return true

  Error      → _Parser.Reset()
               state.CurrentProcessor = null
               return true   (consumed but discarded — don't display garbage)
```

**`Reset()`** — `_Parser.Reset()`.

---

### 4. `McpNegotiatePackage` (new — implements `IMcpPackage`)

| Property | Value |
|---|---|
| `Name` | `"mcp-negotiate"` |
| `MinimumVersion` | `1.0` |
| `MaximumVersion` | `2.0` |

**`CanHandleMessage(Message)`:** returns true for `mcp-negotiate-can` and `mcp-negotiate-end`.

**`ProcessMessage(IClientTerminal client, Message message)`:**

- `mcp-negotiate-can`: extract `package:`, `min-version:`, `max-version:`. Check dispatcher's registry for a matching package. If found and version ranges overlap, add to `McpClientSession.SupportedPackages`.
- `mcp-negotiate-end`: set `IsNegotiationComplete = true` on the session. No other action required for initial packages.

**Session access:** `IMcpPackage` exposes `void SetSession(McpClientSession session)`. The dispatcher calls this on all registered packages immediately after handshake succeeds and the session is created. Packages store the session reference internally.

---

### 5. `McpCordPackage` (new — implements `IMcpPackage`)

| Property | Value |
|---|---|
| `Name` | `"mcp-cord"` |
| `MinimumVersion` | `1.0` |
| `MaximumVersion` | `1.0` |

**`McpCord` model (new small class):**
```csharp
public class McpCord
{
    public string Id { get; }        // "I..." server-created, "R..." client-created
    public string Type { get; }      // application-level protocol type
    public bool IsOpen { get; set; }
}
```

**`ProcessMessage(IClientTerminal client, Message message)`:**

- `mcp-cord-open`: create `McpCord` from `_id:` and `_type:`, store in `Dictionary<string, McpCord>`
- `mcp-cord`: look up cord by `_id:`, extract `_message:` and remaining kwargs, route to registered `IMcpPackage` for that cord type via `package.ProcessMessage(client, message)` (drop silently if none)
- `mcp-cord-closed`: mark cord closed, remove from dictionary

---

### 6. Terminal wiring (`MooClientTerminal` — only change outside `Org.Edgerunner.Mud.MCP`)

**Remove:** `protected McpClientSessionManager McpSessionManager { get; set; }` and its construction in the constructor — this is now owned internally by `McpOobHandler`.

**Change:** `MooClientTerminal` currently receives `MessageProcessor` as an injected `IMessageProcessor`. Instead, it should construct the `OutOfBandMessageProcessor` and `RootMessageProcessor` internally so it has a direct reference to the OOB processor for handler registration:

```csharp
// In MooClientTerminal constructor:
var oobProcessor = new OutOfBandMessageProcessor();
var localEditHandler = new LocalEditHandler(...);   // already registered today
oobProcessor.RegisterHandler(localEditHandler);

var mcpHandler = new McpOobHandler(new Version(2, 1), new Version(2, 1));
oobProcessor.RegisterHandler(mcpHandler);

MessageProcessor = new RootMessageProcessor(OutOfBandPrefix, oobProcessor);
```

This removes the external `MessageProcessor` injection from `MooClientTerminal`'s constructor — the terminal owns its processor stack and knows about the OOB layer directly.

---

### 7. `Org.Edgerunner.Mud.MCP.Tests` (new xUnit project)

**Framework:** xUnit + FluentAssertions, targeting `net6.0`.

**Initial test classes:**

| Class | Coverage |
|---|---|
| `McpMessageParserTests` | Single-line parse, multiline parse, malformed lines, auth key extraction, quoted values, reset behaviour |
| `McpClientSessionManagerTests` | Version range overlap, no overlap returns null, session key generated |
| `McpMessageDispatcherTests` | Handshake triggers negotiate-can/end, auth key validation, package routing by name prefix, unknown package drops silently |
| `McpNegotiatePackageTests` | Mutual package recorded, version mismatch not recorded, negotiate-end sets flag |
| `McpCordPackageTests` | Cord open/route/close lifecycle |

---

## Files Changed / Created

| File | Change |
|---|---|
| `Org.Edgerunner.Mud.MCP/McpUtils.cs` | Remove `ParseMessage`; add `FormatMessage` |
| `Org.Edgerunner.Mud.MCP/McpMessageParser.cs` | New |
| `Org.Edgerunner.Mud.MCP/McpMessageDispatcher.cs` | New |
| `Org.Edgerunner.Mud.MCP/McpOobHandler.cs` | New |
| `Org.Edgerunner.Mud.MCP/Packages/McpNegotiatePackage.cs` | New |
| `Org.Edgerunner.Mud.MCP/Packages/McpCordPackage.cs` | New |
| `Org.Edgerunner.Mud.MCP/Packages/McpCord.cs` | New |
| `Org.Edgerunner.Mud.MCP/Interfaces/IMcpSession.cs` | Add `IsNegotiationComplete` flag |
| `Org.Edgerunner.Mud.MCP/McpClientSession.cs` | Implement `IsNegotiationComplete` |
| `Org.Edgerunner.Mud.MCP/Interfaces/IMcpProtocolHandler.cs` | Add `IClientTerminal client` param to `ProcessMessage`; add `SetSession(McpClientSession)` to `IMcpPackage` |
| `Org.Edgerunner.Mud.MCP/Org.Edgerunner.Mud.MCP.csproj` | No changes needed (Communication already referenced) |
| `Org.Edgerunner.Moo.Editor/Controls/MooClientTerminal.cs` | Remove `McpSessionManager`; register `McpOobHandler` |
| `Org.Edgerunner.Mud.MCP.Tests/` | New xUnit project |
| `Moo Developer Tools.sln` | Add test project |

---

## Out of Scope (deferred)

- Concrete application packages beyond `mcp-negotiate` and `mcp-cord` (e.g. `mcp-edit`)
- Client-initiated cord creation (server-initiated only for now)
- `IMcpConfiguration` implementation — interface exists but no concrete config class needed for this scope
