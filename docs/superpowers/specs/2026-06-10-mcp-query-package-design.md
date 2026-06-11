# MCP Dev-Info Query Package (udd-btl) — Design

**Date:** 2026-06-10
**Bead:** udd-btl — MCP dev-info query package (IMooWorldQueryProvider impl)
**Status:** Approved by user
**Package name:** `edgerunner-org-moo-query` v1.0

> MCP throughout this document = **MUD Client Protocol 2.1**, NOT the LLM Model Context Protocol.

## Goal

Implement both halves of a new MCP 2.1 package that gives the editor full world-introspection
over a negotiated MCP session:

1. **Server half** — a Moo-code package object, distributed as a moo dump file plus written
   registration instructions, that answers every query in the `IMooWorldQueryProvider`
   contract.
2. **Client half** — an `IMcpPackage` implementation in `Org.Edgerunner.Mud.MCP` backing a
   full-coverage `IMooWorldQueryProvider`, registered with the connection's `QueryProviders`
   at **priority 200** (above SDWC's 100; SDWC remains a fall-through provider).
3. **Protocol documentation** — a detailed, normative protocol specification document that
   both halves are written against.

This is the second provider implementation alongside SDWC (udd-am1) and the first that covers
all 13 interface operations.

## Non-Goals

- No change to `IMooWorldQueryProvider` or its typed models — this is purely a new provider.
- No server-side MCP framework: the package targets cores that already have a JHCore-style
  server MCP implementation (like the simpleedit reference in
  `docs/reference/dns-org-mud-moo-simpleedit.moo`). Core-agnostic self-contained dispatch is
  out of scope.
- No streaming/cord usage — plain request/reply messages only (cords would add round trips).
- No editor/UI changes; existing consumers (contextual autocomplete, future object browser)
  pick the provider up through the registry automatically.

## Design Decisions (settled with user)

| Decision | Choice |
|---|---|
| Server target | JHCore-style MCP cores (simpleedit-object pattern); classic LambdaMOO syntax only |
| Message set | One request/reply message pair per operation (12 pairs; the two `GetOwnedObjectsAsync` overloads share one message with an optional `owner` param) |
| Payload encoding | **Minified JSON**: short named keys for envelopes/singletons, positional arrays for repeated list rows; packed into as few multiline lines as possible (transfer size priority) |
| Summary payloads | **Queried object only — no definers.** Verb/property list replies carry names only; the client fills `DefiningObject` with the queried object id. `q`/`r` (queried/resolved) appear ONLY on `-verb-info` / `-verb-doc` / `-verb-code`, where the interface contract explicitly models resolution |
| Correlation | Client-generated `tag` field echoed by the server; client correlator keyed by tag |
| Permissions | Every server handler runs under `set_task_perms()` of the connected player — normal MOO read rules decide visibility |
| `GetObjects` scope | Core (`$`-registered) objects only — the `#0` property registry walk |
| Error handling | Shared `-error` reply; client degrades to `null`/empty per the interface contract but **always logs** the event (never silent) |
| Provider priority | 200 (SDWC = 100) |

## Wire Protocol

All message names are prefixed `edgerunner-org-moo-query-`. Requests carry the MCP auth key,
a `tag: "<n>"` field, and the parameters below. Every reply echoes the tag and carries one
`data*` multiline field containing minified JSON.

### Message catalog

| Request | Params (besides `tag`) | Reply | JSON payload |
|---|---|---|---|
| `-objects` | — | `-objects-reply` | `{"d":[[num,name,[aliases]],…]}` — one row per `$`-registered object |
| `-children` | `object` | `-children-reply` | `{"d":[[num,name,[aliases]],…]}` |
| `-owned` | `owner` (optional; defaults to the connected player) | `-owned-reply` | `{"d":[[num,name,[aliases]],…]}` |
| `-parent` | `object` | `-parent-reply` | `{"p":num}` (−1 = no parent) |
| `-verbs` | `object` | `-verbs-reply` | `{"d":["g*et put","look_self",…]}` — raw verb-names strings, local + inherited, deduped |
| `-verb-info` | `object`, `verb` | `-verb-info-reply` | `{"q":num,"r":num,"a":"names","o":num,"p":"rxd","g":["this","none","this"]}` |
| `-verb-doc` | `object`, `verb` | `-verb-doc-reply` | `{"q":num,"r":num,"l":[lines]}` |
| `-verb-code` | `object`, `verb` | `-verb-code-reply` | `{"q":num,"r":num,"l":[lines]}` |
| `-props` | `object` | `-props-reply` | `{"d":["name","name",…]}` — names only, local + inherited, deduped |
| `-prop-info` | `object`, `prop` | `-prop-info-reply` | `{"n":"name","o":num,"p":"rc","t":typecode,"v":"preview"}` |
| `-prop-doc` | `object`, `prop` | `-prop-doc-reply` | `{"l":[lines]}` |
| `-prop-value` | `object`, `prop` | `-prop-value-reply` | `{"t":typecode,"v":"literal"}` |

Shared error reply: **`-error`** with fields `tag`, `code` (MOO error name, e.g. `E_PERM`,
`E_INVARG`, `E_VERBNF`, `E_PROPNF`), `message` (human-readable text).

### Conventions

- **Object numbers are bare JSON ints** (no `#`, unquoted); the client renders `MooObjectId`.
- **Verb names stay as raw MOO names strings** (`"g*et put"`); consumers split/strip `*`.
- **Envelope keys are single characters**; list rows are positional arrays.
- `q` = queried object number, `r` = resolved (defining) object number — verb info/doc/code only.
- All JSON is minified (no whitespace).

### Example exchange

```
C→S: #$#edgerunner-org-moo-query-verbs K7% tag: "12" object: "#123"
S→C: #$#edgerunner-org-moo-query-verbs-reply K7% tag: "12" data*: "" _data-tag: 9911
     #$#* 9911 data: {"d":["g*et put","look_self"]}
     #$#: 9911
```

### Chunking

`data*` is a standard MCP multiline field. The server splits the JSON string at ≤4000
characters per `data` line purely as transport framing; the client concatenates all `data`
lines **verbatim, no separator**, then parses once the `#$#:` terminator closes the message.
Rationale: per-line MCP framing (~20–30 bytes/line) dominates transfer overhead, so payloads
are packed into as few lines as possible; multiline continuation lines are transmitted raw, so
the JSON needs no MCP-level escaping.

## Server Package (moo dump)

### Object shape

One package object, child of the core's generic MCP package parent (per the simpleedit
reference). Metadata properties:

- `aliases = {"edgerunner-org-moo-query"}`
- `version_range = {"1.0", "1.0"}`
- `messages_in` — the 12 request messages with their parameter lists
- `messages_out` — the 12 reply messages plus `error`
- `description` — condensed protocol summary (lines), per simpleedit precedent

### Handler verbs

One per inbound message, framework-convention named (dashes → underscores): `handle_objects`,
`handle_children`, `handle_owned`, `handle_parent`, `handle_verbs`, `handle_verb_info`,
`handle_verb_doc`, `handle_verb_code`, `handle_props`, `handle_prop_info`, `handle_prop_doc`,
`handle_prop_value`.

Skeleton (mirrors simpleedit `handle_set`):

1. `caller == this` guard (raise `E_PERM` otherwise)
2. `set_task_perms(session.connection)`
3. Parse params → compute → JSON-encode → send reply chunked at ≤4000 chars/line
4. Entire body wrapped in `try/except`: any raised error is sent as the `-error` reply with
   the MOO error name as `code` and the error message as `message`

### JSON encoding

A `:json_encode(value)` utility verb:

- Uses the `generate_json()` builtin when the server provides it (probed once via
  `function_info()`, result cached in a property).
- Otherwise falls back to a hand-rolled encoder (~30 lines): handles strings (escape only
  `\` and `"` — MOO strings cannot contain newlines), ints, floats, objnums (encoded as bare
  ints), and lists. Named-key envelopes are assembled by `tostr()` concatenation around
  encoded fragments.

All output is minified by construction. All package code is classic-LambdaMOO syntax so the
dump loads on any JHCore-style core regardless of server family.

### Per-operation semantics

| Op | Server behavior |
|---|---|
| `objects` | Walk `properties(#0)`; for each property whose value is a valid object: row `[num, .name, [.aliases]]` |
| `children` | `children(object)` → summary rows |
| `owned` | Target player's `.owned_objects` (core bookkeeping maintained by `@create`/`@recycle`). Property absent → `-error E_INVARG` (no DB walk, ever) |
| `parent` | `parent(object)`; `#-1` → `{"p":-1}` |
| `verbs` | Local + inherited verb-names strings, ancestor walk, deduped |
| `verb-info` | Resolve the defining ancestor (`r`), then `verb_info` + `verb_args` |
| `verb-doc` | Resolve definer; doc = leading string-literal lines of `verb_code` output |
| `verb-code` | Resolve definer; `verb_code()` lines |
| `props` | Local + inherited property names, ancestor walk, deduped |
| `prop-info` | `property_info()` → owner/perms; `typeof` for the type code; `v` = first **80 characters** of `toliteral(value)` |
| `prop-doc` | Value-preview lines: `toliteral(value)` split into lines of at most **78 characters**, capped at **50 lines** (classic cores have no property-doc convention) |
| `prop-value` | `typeof` code + full `toliteral(value)` (transport chunking handles size) |

All visibility/readability outcomes are whatever the player's own permissions yield — `E_PERM`
surfaces as the `-error` reply like any other raised error.

### Distribution

- **Dump file:** `Server Packages/edgerunner-org-moo-query.moo` — `;;`-property and
  `@args`/`@program` blocks in the style of the simpleedit reference, with the placeholder
  object number `#XXX` everywhere the reference hardcodes `#230`.
- **Install doc:** `Server Packages/edgerunner-org-moo-query-INSTALL.md` — steps: create the
  object as a child of the core's MCP package parent, search-replace `#XXX` with the created
  object number, load the dump, register the package with the core's MCP registry, verify by
  connecting with Udditor and confirming `edgerunner-org-moo-query` appears in negotiation.

## Client Implementation (`Org.Edgerunner.Mud.MCP`)

New files in `Packages/` (plus an `NLog` 5.2.8 package reference for this project — currently
absent, consistent with the rest of the solution):

### `McpQueryPackage : IMcpPackage`

- `Name = "edgerunner-org-moo-query"`, `MinimumVersion = MaximumVersion = 1.0`.
- `CanHandleMessage` matches the 12 `-reply` names + `-error`.
- `ProcessMessage`: extract `tag`; reassemble the `data` multiline field (concatenate lines
  verbatim); complete the matching pending request via the correlator with either the JSON
  payload or the error (code, message). Unknown/stale tags are dropped and logged at Trace.
- Package support is negotiated **during the initial MCP handshake**: the `mcp` startup
  exchange followed by `mcp-negotiate`, where each side advertises its packages and versions.
  The client advertises `edgerunner-org-moo-query 1.0`; when the completed handshake shows the
  server offers it at a compatible version, the package constructs the provider (client
  terminal + session key are available at that point) and registers it
  `client.QueryProviders.Register(provider, 200)` — exactly once, idempotent, the
  `SdwcOobHandler.EnsureProviderRegistered` pattern. No support → no registration; the
  registry falls through to SDWC or returns the contract defaults.
- Integration point to verify during planning: how `McpNegotiatePackage` surfaces the
  negotiated package set to package implementations when the handshake completes; if no such
  signal exists yet, add a small event/callback to `McpNegotiatePackage`.

### `McpQueryCorrelator`

Thread-safe `tag → TaskCompletionSource` map: `CreatePending` / `Complete` / `CompleteError` /
`Remove`. Tags from an `Interlocked` counter — unique by construction (no composite keys
needed, unlike `SdwcCorrelator`).

### `McpQueryProvider : IMooWorldQueryProvider`

Implements **all 13 operations** with the `SdwcQueryProvider.ExchangeAsync` shape: create
pending (fresh tag) → `McpUtils.FormatMessage` request with session auth key → send → await
under the caller's token linked to a bounded timeout (default 10 s) → map → always remove the
pending entry. Timeout → `TimeoutException`; cancellation propagates.

### `McpQueryMapping`

Static JSON → typed-record mapping via `System.Text.Json` (mirroring `SdwcMapping`):

- List replies → summaries with **`DefiningObject` = queried object id**; verb names strings
  kept raw.
- `-verb-info` → `MooVerbInfo`: parse `"rxd"` flag string → `VerbPermission`,
  `["this","none","this"]` → `VerbArgs` enums; `q`/`r` → queried/resolved ids.
- `-prop-info` → `MooPropertyInfo` with `DefiningObject` = queried object id.
- `-parent` → `p` of `-1` maps to a `null` `MooObjectId?` (no parent), per the interface contract.
- Bare-int object numbers → `MooObjectId`.

### Error handling — degrade but always log

Returned values follow the interface contract (`E_VERBNF`/`E_PROPNF` → `null`; `E_PERM` →
`null`/empty list; unknown codes → `null`/empty — never an exception into editor paths), but
every degraded event is logged:

| Event | Level | Context logged |
|---|---|---|
| `-error` reply (known code) | Debug | op, tag, params, code, server message |
| Unknown error code | Warn | same + the unrecognized code |
| Malformed/unparseable JSON | Warn | op, tag, exception, payload length (full payload at Trace) |
| Query timeout | Debug | op, tag, params, configured timeout |
| Stale/unknown tag reply | Trace | message name, tag |

`NotImplementedException` is never thrown by this provider (full coverage); it stays reserved
for registry fall-through by partial providers.

### Wiring

`WindowManager.CreateTerminalPage` adds the query package to the `IMcpPackage[]` array beside
`SimpleEditPackage`. No other UI change; existing consumers reach the provider through
`client.QueryProviders.Query` automatically.

## Protocol Documentation (deliverable)

`docs/edgerunner-org-moo-query-protocol.md` — the **normative** specification both halves are
written against: package identity/version, negotiation behavior, full message catalog with
parameters, JSON payload schema and a worked example per message, encoding conventions
(minified, short keys, positional rows, bare-int objnums, raw verb-names strings), chunking
rules, the error message and code semantics, permission model, and the `GetObjects` core-registry
scope. The server dump's `description` property carries a condensed version.

## Testing

Headless unit tests only (NO GUI test hosts), in `Org.Edgerunner.Mud.MCP.Tests`:

- **Correlator:** pending lifecycle, complete/error/remove, concurrent tags, double-complete
  safety.
- **Request formatting:** each op produces the correct message name/params/tag.
- **Reply parsing/mapping:** each reply type → correct typed records (including
  `DefiningObject` = queried id rule); multiline chunk reassembly; `-verb-info` permission/args
  parsing; malformed JSON → degraded result + log; `-error` code mapping per op.
- **Provider behavior:** timeout, caller cancellation, stale-tag drop.
- **Negotiation participation:** package advertised, provider registered exactly once on
  confirmation, not registered when the server lacks the package.

Build-only verification for the wiring change in `WindowManager`. Live verification: user loads
the dump into the EdgeRunner moo per the install doc and exercises queries from Udditor.

## Error Handling Summary

- Server: every handler failure → `-error` reply with MOO error name + message; nothing raised
  into the server's MCP framework.
- Client: degrade to `null`/empty per contract, always logged (table above); timeout →
  `TimeoutException` (callers already treat it as a degraded result); never throw into editor
  consumers.

## Implementation Risks (validate during planning)

1. **Negotiation confirmation hook** — package support is settled during the initial MCP
   handshake (`mcp` + `mcp-negotiate`); confirm how `McpNegotiatePackage` exposes the
   negotiated package set to package implementations at handshake completion, and add a small
   notification if absent.
2. **Multiline reassembly** — confirm `McpMessageParser` preserves `data` line content
   verbatim and in order for concatenation (it implements the MCP 2.1 `_data-tag` model per
   udd-u5v; verify no separator/trim behavior).
3. **JHCore handler dispatch convention** — confirm the exact framework calling convention for
   package handler verbs (verb naming, argument list including the session object) against the
   core before finalizing the dump; the simpleedit reference is the model.
