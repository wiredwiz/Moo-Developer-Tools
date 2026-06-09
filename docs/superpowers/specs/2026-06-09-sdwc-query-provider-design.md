# Design: SDWC `IMooWorldQueryProvider` implementation (udd-am1)

**Date:** 2026-06-09
**Status:** Approved (design), pending spec review
**Bead:** udd-am1 (depends on udd-5oh; blocks udd-7g2; enables udd-9ge, udd-of6)

## Purpose

Implement a concrete `IMooWorldQueryProvider` over the **SDWC** out-of-band protocol
(Sindome `dome-client`), so Udditor can query a connected MOO for an object's verbs and
properties and surface verb/property hover detail. This is the first real provider behind the
`udd-5oh` abstraction; the editor's object/verb browser (`udd-9ge`) and world-aware
autocomplete (`udd-of6`) consume it through the existing registry/cache.

SDWC's feature set is small, so **most interface methods stay `NotImplemented`** and fall
through (per the registry contract) to any other provider.

References:
- Protocol: https://github.com/SindomeCorp/dome-client/blob/main/docs/SDWC-OOB.md
- Server setup + payloads: https://github.com/SindomeCorp/dome-client/blob/main/docs/MOO-SETUP.md
- Interface: `Org.Edgerunner.Mud.Common\Querying\IMooWorldQueryProvider.cs`

## Scope

In scope:
- One interface addition: `GetPropertyDocumentationAsync` (below).
- SDWC OOB handler + provider + correlation, in `Org.Edgerunner.Mud.Common` consumers'
  comms layer (`Org.Edgerunner.Mud.Communication`).
- Capability detection via the server's `#$# dome-client-user` broadcast; self-registration
  with the connection's `QueryProviders`.
- Four query operations; everything else `NotImplementedException`.
- Tests.

Out of scope:
- Editor consumption (object/verb browser `udd-9ge`, autocomplete `udd-of6`).
- The MCP dev-info provider (`udd-btl`).
- The `IDE_*_ENABLED` server-side feature toggles — not modeled (see Detection).
- Per-verb owner/permissions/args, `parent`, and property metadata that SDWC *could* surface
  via VERBS/PROPS but which map to methods we are leaving `NotImplemented` for now
  (`GetVerbInfoAsync`, `GetParentAsync`, `GetPropertyInfoAsync`).

## Interface change (`Org.Edgerunner.Mud.Common`)

Add one method to `IMooWorldQueryProvider`:

```csharp
Task<IReadOnlyList<string>> GetPropertyDocumentationAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken);
```

- Plain list (no wrapper): properties resolve to the object queried, not an ancestor, so the
  queried/resolved-id pair `MooVerbDocumentation` carries is not relevant here.
- Registry treats it as a **list-style** method (walk providers, fall through on
  `NotImplementedException`, exhaustion → empty list) — same shape as `GetVerbsAsync`.
- Update the registry, the caching decorator, the test fakes, and `Mud.Common.Tests`.

No other interface change. The other verb/property methods already exist.

## SDWC protocol (as used here)

Lines are carried on the existing `#$#` OOB channel. **Note the space:** SDWC uses
`#$# SDWC%%…` (a space after the prefix). After `RootMessageProcessor` strips the 3-char
`#$#` prefix, inbound lines arrive as `" SDWC%%…"` / `" dome-client-user"` (leading space) —
trim before matching. Outbound, requests are sent as the text `" SDWC%%…"` (leading space) so
the wire form is `#$# SDWC%%…`.

Requests (client→server):
- `SDWC%%VERBS%%<objectId>`
- `SDWC%%PROPS%%<objectId>`
- `SDWC%%VERB-OVERLAY%%<objectId>%%<verb>`
- `SDWC%%PROP-OVERLAY%%<objectId>%%<property>`

Responses (server→client), JSON payloads:
- `SDWC%%VERBS%%{ object, owner, name, verbs:[{ owner, permissions, name, args }] }`
- `SDWC%%PROPS%%{ object, name, parent, owner, flags, props:{ <name>:{ clear, owner, permissions } } }`
- `SDWC%%VERB-OVERLAY%%{ object, resolved_object, verb, value }`
- `SDWC%%PROP-OVERLAY%%{ object, property, value }`

Control/handshake lines:
- `dome-client-user` — **server broadcast; its presence means SDWC is supported** (the
  capability signal we key on).
- `SDWC-START-NOWRAP` / `SDWC-END-NOWRAP` — output-rendering hints; consumed (no-op for queries).

> Defensive parsing: MOO object values may serialize as `#123` strings or bare numbers
> depending on the server's JSON encoder; the JSON→`MooObjectId` mapping accepts both.
> Verb `name` is whitespace-separated aliases. These specifics are verified against a live
> capture during implementation.

## Method mapping

| Interface method | SDWC request | Result mapping |
|---|---|---|
| `GetVerbsAsync(obj)` | `VERBS%%obj` | `verbs[]` → `MooVerbSummary(Aliases = split(name), DefiningObject = obj)` |
| `GetPropertiesAsync(obj)` | `PROPS%%obj` | `props` keys → `MooPropertySummary(Name, DefiningObject = obj)` |
| `GetVerbDocumentationAsync(obj, verb)` | `VERB-OVERLAY%%obj%%verb` | `MooVerbDocumentation(QueriedObjectId = object, ResolvedObjectId = resolved_object, Lines = split(value))` |
| `GetPropertyDocumentationAsync(obj, prop)` | `PROP-OVERLAY%%obj%%prop` | `Lines = split(value)` (rendered value preview, truncated server-side ~500 chars) |
| all others | — | `throw new NotImplementedException()` |

## Components (`Org.Edgerunner.Mud.Communication`)

1. **`SdwcCorrelator`** — pending-request store. Keyed by `(marker, objectId[, verb|prop])`
   (every response echoes `object`, plus `verb`/`property` for overlays). Holds
   `TaskCompletionSource`s; `CreatePending(key)` returns a `Task`, `Complete(key, payload)` /
   `Fail(key, ex)` resolve it. Thread-safe.
2. **`SdwcQueryProvider : IMooWorldQueryProvider`** — constructed with the connection's
   `IClientTerminal` (for sending) and the `SdwcCorrelator`. Each of the four supported
   methods: register a pending entry, send `client.SendOutOfBandLine(" SDWC%%…")`, await the
   correlated payload under the caller's `CancellationToken` linked with a bounded timeout,
   deserialize → models. The other eight methods `throw new NotImplementedException()`.
3. **`SdwcOobHandler : IOutOfBandMessageHandler`** — registered on every connection's OOB
   pipeline (in `WindowManager.CreateTerminalPage`, beside `LocalEditHandler`/`McpOobHandler`).
   On each line (after trimming the leading space):
   - `dome-client-user` → if not already done, construct the `SdwcQueryProvider` (with the
     `IClientTerminal` from this call) and register it with the connection's `QueryProviders`
     (idempotent). This is the capability-detection point.
   - `SDWC%%<MARKER>%%<json>` → parse marker + JSON, extract the correlation key, and
     `Complete` the matching pending request.
   - `SDWC-START-NOWRAP` / `SDWC-END-NOWRAP` → consume.
   - Anything else → not handled (return false).
4. **JSON DTOs + mapping** (System.Text.Json) — internal DTOs for the four payloads and pure
   mapping functions to the `Mud.Common.Querying` models. Kept in a dedicated file so mapping
   is unit-testable without the network.

### Wiring to `QueryProviders`

`SdwcOobHandler.ProcessMessage` receives the `IClientTerminal`. To register/send, the handler
needs the connection's `MooWorldQueryService` (anchored on `IMudClientSession.QueryProviders`)
and the outbound channel. The cleanest seam is to expose `QueryProviders` on `IClientTerminal`
(delegating to its session), so the handler does `client.QueryProviders.Register(provider,
priority)` and the provider sends via `client.SendOutOfBandLine(...)`. If exposing it on
`IClientTerminal` proves awkward, fall back to constructing the handler with the
`IMudClientSession` directly. Registration priority is a sensible constant (e.g. 100); a future
provider (`udd-btl`) coordinates its own priority.

## Data flow (`GetVerbsAsync(#123)`)

```
editor → Caching → Registry → SdwcQueryProvider.GetVerbsAsync(#123)
   correlator.CreatePending((VERBS,#123))
   client.SendOutOfBandLine(" SDWC%%VERBS%%#123")        → wire: #$# SDWC%%VERBS%%#123
… server …
   #$# SDWC%%VERBS%%{ "object":#123, …, "verbs":[…] }
   → RootMessageProcessor strips #$# → SdwcOobHandler
   → parse marker VERBS, object=#123 → correlator.Complete((VERBS,#123), json)
   → provider maps verbs[] → IReadOnlyList<MooVerbSummary>, returns
```

## Error handling

- Unsupported methods → `NotImplementedException` → registry falls through. ✓
- A request with no response (slow/dropped) → bounded **timeout** → throw `TimeoutException`.
  Because the `dome-client-user` broadcast already established support, a missing response is a
  genuine failure, not a disabled feature; it surfaces per the registry contract and the
  editor's defensive layer logs/skips. (No swallowing to empty.)
- `OperationCanceledException` propagates (and removes the pending entry).
- Malformed/unparseable JSON → fail that pending request (`Fail`) with a parse exception;
  unmatched/stray `SDWC%%` responses are dropped.
- Every path removes its pending correlator entry (success, failure, timeout, cancel) so the
  store doesn't leak.

## Testing (`Org.Edgerunner.Mud.Communication.Tests`)

xUnit + FluentAssertions, with a fake `IClientTerminal` that captures sent OOB lines and lets
the test feed scripted inbound `SDWC%%…` responses:
- Request formatting for all four ops (correct marker, object id form, `%%`-joined args, leading space).
- JSON→model mapping for all four payloads, including the `resolved_object` on verb overlays
  and the value-preview lines on prop overlays.
- Correlation: concurrent and out-of-order responses resolve the right requests; a stray
  response for an unknown key is ignored.
- `dome-client-user` triggers a single provider registration on `QueryProviders` (idempotent).
- The eight unsupported methods throw `NotImplementedException`.
- Timeout throws `TimeoutException`; cancellation propagates; both clear the pending entry.
- Pure JSON→model mapping unit tests independent of the handler/correlator.

## Out-of-scope follow-ups (file as needed)

- `GetVerbInfoAsync` / `GetParentAsync` / `GetPropertyInfoAsync` from the VERBS/PROPS payloads,
  if a feature wants them.
- Registry behavior when a higher-priority provider times out (whether to fall through to a
  lower-priority provider on timeout) — currently it surfaces; revisit if both SDWC and MCP
  providers bind to one world.
