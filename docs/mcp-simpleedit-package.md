# MCP Package — `dns-org-mud-moo-simpleedit` (Simple Edit) 1.0

Sources:
- https://www.moo.mud.org/mcp/simpleedit.html
- https://www.moo.mud.org/mcp/mcp2.html (host MCP 2.1 protocol)

> **Note:** In this project "MCP" always refers to the **MUD Client Protocol**, not any
> LLM-related protocol. This package rides on top of MCP 2.1; see
> [`mcp2-protocol.md`](mcp2-protocol.md) for the underlying handshake, negotiation, and
> message-format rules.

---

## Purpose

`dns-org-mud-moo-simpleedit` is the standard MCP replacement for the old line-based
"local edit" facility (the `#$#edit name: … upload: …` flow handled today by
`LocalEditHandler`). It lets a server hand the client a blob of text — a verb's MOO code,
a property value, a note, a mail message — for the user to edit in a real editor window,
and lets the client hand the edited text back. The server decides what the text *is* and
what to do with the result; the client only needs to display it, let the user edit it, and
send it back tagged with the same opaque reference.

It is a small package: **two messages**, no session state of its own beyond correlating a
returned edit with the request that produced it.

---

## Package Identity

| Field | Value |
|---|---|
| Package name (for `mcp-negotiate`) | `dns-org-mud-moo-simpleedit` |
| Version | 1.0 (min 1.0, max 1.0) |
| Requires | MCP 2.1 handshake + `mcp-negotiate` |
| Naming | reversed-DNS of `moo.mud.org` → `dns-org-mud-moo`, plus `-simpleedit` |

The package must be advertised during negotiation with an `mcp-negotiate-can` line and is
usable once both sides have advertised a compatible version range.

---

## Messages

### 1. `dns-org-mud-moo-simpleedit-content` — server → client

The server sends text for the user to edit.

| Keyword | Multiline? | Meaning |
|---|---|---|
| `reference:` | no | Opaque, machine-readable identifier. The client treats it as a black box and echoes it back verbatim in the matching `set` message. |
| `name:` | no | Human-readable label for the edit buffer — use it as the editor window/tab title. |
| `type:` | no | Content category. One of `string`, `string-list`, `moo-code` (see below). |
| `content*:` | **yes** | The text to edit. Multiline, carried via the standard MCP 2.1 `_data-tag` mechanism. |

### 2. `dns-org-mud-moo-simpleedit-set` — client → server

The client returns the edited text when the user saves/sends.

| Keyword | Multiline? | Meaning |
|---|---|---|
| `reference:` | no | **Must** equal the `reference:` from the originating `content` message. |
| `type:` | no | The content type (echo the type received, unless intentionally changing it). |
| `content*:` | **yes** | The edited text. |

There is no acknowledgement message — `set` is fire-and-forget. The server reports success
or failure however it likes (typically in-band text).

---

## The `type:` Parameter

| Value | Meaning | Client treatment |
|---|---|---|
| `string` | A single-line value (e.g. an object's `.name`). | Edit as one line of plain text. |
| `string-list` | Multiple lines of unstructured text (e.g. a note, a `.description`). | Edit as plain multi-line text. |
| `moo-code` | A MOO verb program. | Edit with MOO syntax highlighting / parsing. **A client may treat `moo-code` as `string-list`** if it has no code editor. |

This maps cleanly onto the existing editor surfaces in this codebase:
- `moo-code` → `MooCodeEditorPage` (syntax highlighting + background parse)
- `string` / `string-list` → `MooDocumentEditorPage` (plain text)

---

## The `reference:` Parameter

- It is **opaque to the client** — never parse, validate, or transform it.
- It is **machine-readable** (the server uses it to locate what to update — e.g. which verb on
  which object), as opposed to `name:` which is for humans.
- The client's only job is to remember it for the lifetime of the edit and send it back
  unchanged in `set`. Multiple edits can be open at once, each distinguished by its reference.

---

## Multiline Encoding (`content*:`)

Per MCP 2.1, a keyword suffixed with `*` is a multiline value. The mechanics are easy to get
subtly wrong, so to be explicit:

1. On the message line, the value written after `content*:` is **syntactically required but
   ignored** — the empty string `""` is conventional.
2. The message line must also carry a separate **`_data-tag:`** keyword whose value is a
   string unique within the session. This tag — *not* the value after `content*:` — is what
   correlates the continuation lines.
3. Each continuation line is `#$#* <data-tag> <keyword>: <text>`. Everything after
   `<keyword>: ` is literal value — it is **not** re-parsed for keywords or quoting, so it can
   contain quotes, colons, leading spaces, etc.
4. The block is closed by `#$#: <data-tag>`.

### Canonical multiline example (from the MCP 2.1 spec, verbatim)

```
#$#spam 12345 from: Biff text*: "" _data-tag: 9b76
#$#* 9b76 text: This is some sample text.
#$#* 9b76 text:
#$#* 9b76 text: Note that you don't need to quote strings
#$#* 9b76 text: in multiline data.  Also, you can include "special"
#$#* 9b76 text: characters like quotes.  Everything after the
#$#* 9b76 text: space after the keyword and colon is considered
#$#* 9b76 text: part of the value.
#$#* 9b76 text:     This means that spaces can also be part of the value.
#$#: 9b76
```

(`12345` is the session auth key; `9b76` is the data-tag.)

---

## Full Round-Trip Example

Editing object `#73`'s `name` property (a single-line `string`). `3487` is the session auth key.

**Server → client:**
```
#$#dns-org-mud-moo-simpleedit-content 3487 reference: #73.name name: "Joe's name" type: string content*: "" _data-tag: 12345
#$#* 12345 content: Joe
#$#: 12345
```

**Client → server** (user changed "Joe" to "Erik"):
```
#$#dns-org-mud-moo-simpleedit-set 3487 reference: #73.name type: string content*: "" _data-tag: 54321
#$#* 54321 content: Erik
#$#: 54321
```

Note the client generates its **own** fresh data-tag (`54321`) for its outbound multiline
block; only the `reference:` is echoed.

---

## Relationship to the Legacy Local-Edit Flow

| | Legacy (`LocalEditHandler`) | `dns-org-mud-moo-simpleedit` |
|---|---|---|
| Transport | Raw `#$#edit name: … upload: …` then lines then `.` | Negotiated MCP package, multiline `content*` |
| Upload | Client replays a stored *upload command* + lines + `.` | Client sends a `set` message echoing `reference` |
| Correlation | The upload command string | Opaque `reference` |
| Content typing | Inferred from name containing `:` | Explicit `type:` field |
| Requires | Nothing (works on any server) | MCP 2.1 + negotiation |

Both can be active simultaneously: servers that negotiate simpleedit use it; others fall back
to the legacy flow.

---

## Server Behavior — JHCore / EdgeRunner Reference Implementation

Confirmed from the server-side package object (`#230`, see
[`reference/dns-org-mud-moo-simpleedit.moo`](reference/dns-org-mud-moo-simpleedit.moo)). The
client must not depend on these specifics — they illustrate how a real server uses the opaque
fields — but they are useful for testing against EdgeRunner.

- **Package object declares:** `version_range = {"1.0","1.0"}`, `aliases =
  {"dns-org-mud-moo-simpleedit"}`, out-message `content (reference, name, type, content)`,
  in-message `set (reference, type, content)`.
- **`reference` is opaque and polymorphic.** The server's `handle_set` interprets it three ways:
  | Edit kind | `type` | `reference` example | Server handler |
  |---|---|---|---|
  | Verb code | `moo-code` | `#73:verbname` (a verbref) | `edit_set_program` |
  | Mail message | (any) | `sendmail` (literal) | `edit_sendmail` |
  | Property / note | `string` / `string-list` | `str:#73.pname` or `val:#73.pname` | `edit_set_note_value` |

  The client treats all of these as black boxes and echoes them back unchanged.
- **Dispatch priority on `set`:** `type == "moo-code"` → program a verb; else `reference ==
  "sendmail"` → send mail; else set a property/note value.
- **No MCP acknowledgement.** Outcomes (e.g. `"0 errors."`, `"Verb programmed."`, or
  compilation/permission errors) are returned as **in-band** text via `player:notify_lines`. A
  `set` does not guarantee the value was saved — the package description explicitly recommends
  clients let the user re-send without closing the editor.
- **Verb-code comment filtering (`v_filter_in` / `v_filter_out`).** When the player's
  `//_comments` prog_option is on, the server rewrites `"comment";` ↔ `// comment` on the way
  out and back in. This is **transparent to the client**: code may arrive containing `//`-style
  line comments, and we send it back verbatim for the server to re-internalize. Implication: the
  editor's MOO syntax highlighting/parsing should tolerate `//` line comments without flagging
  them as errors.

---

## Implementation Notes for This Codebase

> These are observations for the upcoming implementation (bead **udd-wm8**), not part of the
> protocol itself.

- **Inbound parser convention (`_data-tag`).** The current `McpMessageParser` multiline handling
  assumes the data-tag is the *value* written after `content*:`. The published **MCP 2.1** spec
  (and the standard generic-package core the EdgeRunner server uses) instead ignores that value
  and uses a separate `_data-tag:` keyword (see above). The *package* version (1.0) does not
  govern this — the *protocol* version (2.1) does. Plan: correct the parser to honor `_data-tag:`
  and confirm with one live `…-content` capture on first connect.
- **Outbound multiline gap.** `McpUtils.FormatMessage` cannot emit multiline (`content*` +
  `_data-tag` + continuation + close) blocks yet; the `set` message needs this.
- **Editor bridge.** A `dns-org-mud-moo-simpleedit` `IMcpPackage` needs access to the
  `WindowManager` to open editor pages — mirroring how `LocalEditHandler` is constructed with
  one. The package's outbound `set` is naturally an `IClientUploader` implementation attached to
  the opened page (parallel to `LocalEditUploader`).
- **Cross-check pending.** Server-specific details (exact `reference` formats, whether verbs are
  sent as `moo-code` vs `string-list`) will be confirmed against the server-side dump of the
  package before finalizing the design.
