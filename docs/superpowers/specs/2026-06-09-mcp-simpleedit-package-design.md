# Design — MCP `dns-org-mud-moo-simpleedit` Local-Edit Package (udd-wm8)

**Date:** 2026-06-09
**Bead:** udd-wm8 (depends on udd-u5v — multiline parser fix)
**Status:** Approved (brainstorming), pending spec review

> "MCP" = MUD Client Protocol. Protocol reference for this package lives in
> [`docs/mcp-simpleedit-package.md`](../../mcp-simpleedit-package.md); host protocol in
> [`docs/mcp2-protocol.md`](../../mcp2-protocol.md). Server-side reference implementation:
> [`docs/reference/dns-org-mud-moo-simpleedit.moo`](../../reference/dns-org-mud-moo-simpleedit.moo).

---

## Goal

Implement client support for the `dns-org-mud-moo-simpleedit` MCP package (v1.0) so verb code,
property values, notes, and mail round-trip through real editor windows over a negotiated MCP
session — the MCP equivalent of today's line-based `LocalEditHandler` flow.

Round trip: server sends `…-content` (reference, name, type, multiline content) → client opens
the appropriate editor → user saves → client sends `…-set` (reference, type, multiline content)
→ server applies it and reports the outcome **in-band** (there is no MCP acknowledgement).

## Decisions (locked during brainstorming)

1. **Tracking:** the multiline parser defect is filed separately as **udd-u5v** (a defect in the
   udd-q2l deliverable); everything else is built under **udd-wm8**.
2. **Coexistence:** the legacy `LocalEditHandler` stays active. Servers that negotiate simpleedit
   use it; others fall back to the legacy `#$#edit` flow. No regression for non-MCP servers.
3. **Type → editor:** `moo-code` → `MooCodeEditorPage` (syntax highlighting + background parse);
   `string` / `string-list` → `MooDocumentEditorPage` (plain text). Unknown type → treated as
   `string-list` per spec.
4. **Upload feedback:** none at the MCP level. The editor stays **open** after a send so the user
   can read the in-band result and re-send on failure. No derived success/failure indicator and
   no in-band result scraping (results are server-specific and uncorrelated to a reference).

## Architecture

Bridge pattern **B**: the package depends on a thin consumer interface; the Udditor app
implements it over `WindowManager`. This keeps `Org.Edgerunner.Mud.MCP` UI-free and unit-testable
while mirroring the existing `IClientUploader` pattern.

### Prerequisite — udd-u5v (multiline parser fix)

`McpMessageParser` currently treats the value after `content*:` as the data-tag. Correct it to the
real MCP 2.1 model:

- Keywords ending in `*:` mark a multiline field, keyed by **field name**; their inline value is
  ignored.
- A single `_data-tag:` simple field supplies the message's data-tag.
- Continuation lines `#$#* <tag> <keyword>: <value>` route by matching `<tag>` to the message
  data-tag and appending `<value>` to the buffer for `<keyword>`. Everything after
  `<keyword>: ` is literal (no re-parse/unquote).
- Close on `#$#: <tag>`.

Also fix `docs/mcp2-protocol.md`, which wrongly states each multiline field uses a different
data-tag (there is one `_data-tag` per message; fields are distinguished by keyword).

### New in `Org.Edgerunner.Mud.MCP`

| Unit | Responsibility | Depends on |
|---|---|---|
| `McpUtils.FormatMultilineMessage(name, key, simpleFields, multilineKeyword, contentLines, dataTag)` | Emit the outbound multiline block: initial line (`… <keyword>*: "" _data-tag: <tag>`), one `* <tag> <keyword>: <line>` per content line, then `: <tag>`. Returns the ordered OOB lines (no `#$#` prefix — the terminal adds it). | existing `FormatMessage` quoting |
| `EditRequest` | Immutable model: `Reference`, `Name`, `EditType` (string), `Content` (string). | — |
| `ISimpleEditConsumer` | `void PresentEdit(EditRequest request, IClientUploader uploader)` — open an editing surface for the request, wiring the uploader as its save path. | `IClientUploader` |
| `SimpleEditPackage : IMcpPackage` | Advertises `dns-org-mud-moo-simpleedit` (min/max 1.0). `CanHandleMessage` matches `dns-org-mud-moo-simpleedit-content`. On content: parse `reference`/`name`/`type`/`content`, build an `EditRequest` and a `SimpleEditUploader`, call `consumer.PresentEdit`. Stateless per-edit. | `ISimpleEditConsumer`, session, `McpUtils` |
| `SimpleEditUploader : IClientUploader` | Captures `reference`, `type`, session key, and `IClientTerminal`. `Upload(text)`: split into lines, `FormatMultilineMessage` a `…-set`, send via `client.SendOutOfBandLines`. Returns false if disconnected. Generates a fresh data-tag per send. | `McpUtils`, `IClientTerminal` |

`SimpleEditPackage` only ever **receives** `content`; it never receives `set` (the client sends
that). It holds no per-edit state — the `SimpleEditUploader` carries the reference for the life of
the edit, so multiple concurrent edits are naturally independent.

### Wiring changes

- **`McpMessageDispatcher`** already has `RegisterPackage`. **`McpOobHandler`** gains a constructor
  overload accepting extra packages (`IEnumerable<IMcpPackage>`), forwarded to the dispatcher, so
  the app can contribute `SimpleEditPackage`. The dispatcher's existing handshake logic then
  advertises it via `mcp-negotiate-can` and calls `SetSession` on it automatically.
- **Udditor:** `WindowManagerSimpleEditConsumer : ISimpleEditConsumer` maps type → page
  (`moo-code` → `CreateMooCodeEditorPage` with the default grammar dialect; `string` /
  `string-list` / unknown → `CreateDocumentEditorPage`), sets `page.Uploader`, and shows the page.
  `WindowManager.CreateTerminalPage` registers `new SimpleEditPackage(consumer)` alongside the
  existing `LocalEditHandler` and `McpOobHandler`.

### Data flow

```
server #$#dns-org-mud-moo-simpleedit-content …
   → OutOfBandMessageProcessor → McpOobHandler → McpMessageParser (assembles multiline)
   → McpMessageDispatcher (auth-key checked) → SimpleEditPackage.ProcessMessage
   → consumer.PresentEdit(EditRequest, SimpleEditUploader)
   → WindowManager opens MooCodeEditorPage / MooDocumentEditorPage (page.Uploader = uploader)
user edits, saves
   → page calls Uploader.Upload(text)
   → SimpleEditUploader → McpUtils.FormatMultilineMessage("dns-org-mud-moo-simpleedit-set", …)
   → client.SendOutOfBandLines → server applies, reports result in-band
```

## Error handling

- Malformed/short `content` messages: dropped silently (MCP norm).
- Unknown `type`: treated as `string-list` → document editor.
- Disconnected on save: `Upload` returns false (page already guards via `CanUpload`).
- Server-side failures (compile/permission): surfaced by the server as in-band terminal text;
  editor remains open for re-send.

## Testing

Unit (`Org.Edgerunner.Mud.MCP.Tests`):
- Parser `_data-tag` cases: canonical MCP 2.1 example; ignored inline value; multiple multiline
  fields sharing one tag; literal content (quotes/colons/leading spaces) preserved.
- `McpUtils.FormatMultilineMessage`: correct initial/continuation/close lines; fresh tag.
- `SimpleEditPackage`: a `content` message drives one `PresentEdit` with the right `EditRequest`
  (fake consumer); non-content messages ignored.
- `SimpleEditUploader`: `Upload` produces a well-formed `…-set` block echoing the reference, with
  the client's own data-tag; disconnected → false.
- Full wire round-trip: parse a captured `content`, edit, format the `set`, re-parse it.

Manual / live: connect to EdgeRunner, confirm the framing (`_data-tag`), edit a verb end-to-end,
verify the server programs it and reports results in-band.

## Out of scope

- Auto-focusing the terminal after a send (considered, deferred).
- Any derived upload-success indicator or in-band result scraping.
- Removing the legacy `LocalEditHandler`.
- `//`-comment grammar handling (server-side `v_filter`; transparent to the client — noted only as
  a syntax-highlighting tolerance consideration, not scoped work here).
