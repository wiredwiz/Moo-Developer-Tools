# SDWC SUPPORT handshake

**Bead:** udd-3w0
**Date:** 2026-06-26
**Status:** Design approved
**Protocol refs:** `docs/MOO-SETUP.md` and `SDWC-OOB.md` from `SindomeCorp/dome-client`
(copies fetched to the session scratchpad during design).

---

## Goal

Implement the SDWC **SUPPORT handshake** so the Moo Udditor client:

1. **Declares** SDWC support to the server, and
2. **Detects** the server's supported abilities,

replacing the current `dome-client-user`-as-capability-signal hack in `SdwcOobHandler` with
the real `SDWC%%SUPPORT%%` mechanism.

---

## Protocol

SDWC rides the existing `#$#` OOB prefix. Wire form: `#$# SDWC%%<COMMAND>%%<payload>`.

- **Server → client (detection):**
  `#$# SDWC%%SUPPORT%%verbs|props|PROP-OVERLAY|VERB-OVERLAY|SUPPORT`
  — a `|`-separated list of supported abilities. The server emits this automatically when a
  player connects (`$sdwc:notify_client_of_sdwc_support`) and again whenever it receives a
  `SUPPORT` request.
- **Client → server (declaration):** `#$# SDWC%%SUPPORT%%` (empty payload). The server's
  `parse_command` `SUPPORT` branch responds by re-broadcasting its SUPPORT line.

---

## Handshake flow

1. Server auto-broadcasts `#$# SDWC%%SUPPORT%%…` on connect (post-login).
2. Client receives it → **parses + stores the ability set → registers `SdwcQueryProvider`**
   (gated on a queryable ability). This is the *detection* half.
3. Client sends its own `#$# SDWC%%SUPPORT%%` **exactly once** — the *declaration* half.

The outbound declaration makes the server re-broadcast SUPPORT; a once-only guard prevents a
ping-pong. Later broadcasts refresh the stored abilities without re-sending.

---

## Scope

### In scope
- Inbound `SDWC%%SUPPORT%%` parsing → `SdwcServerCapabilities`.
- Provider registration driven by SUPPORT (gated on a queryable ability).
- Outbound `#$# SDWC%%SUPPORT%%` declaration, once per session.
- Removal of the `dome-client-user` capability hack.

### Out of scope (separate beads)
- The `@dome-client-user <host/ip>` reply (connection metadata).
- NOWRAP behavior changes.
- New payload features — `VERBS`/`PROPS` browser queries and `VERB-OVERLAY`/`PROP-OVERLAY`
  hover already exist via `SdwcQueryProvider`.

---

## Components

### `SdwcServerCapabilities` (new value object)

Immutable. Constructed from the parsed token set.

- Typed accessors (case-insensitive against the doc tokens):
  `SupportsVerbs` (`verbs`), `SupportsProps` (`props`),
  `SupportsVerbOverlay` (`VERB-OVERLAY`), `SupportsPropOverlay` (`PROP-OVERLAY`).
- `RawTokens` — `IReadOnlySet<string>` (case-insensitive), preserving unknown/future tokens
  so nothing is silently dropped.
- The `SUPPORT` token is informational; it lives in `RawTokens` with no special accessor.
- A convenience `HasAnyQueryableAbility` =
  `SupportsVerbs || SupportsProps || SupportsVerbOverlay || SupportsPropOverlay`, used for
  provider gating.

### `SdwcOobHandler` (modified)

- **Remove** `CapabilitySignal = "dome-client-user"` and its `ProcessMessage` branch.
- **Add** `ServerCapabilities` property (`SdwcServerCapabilities?`, null until first SUPPORT;
  last broadcast wins). This becomes the surface future feature-gating reads (analogous to
  MCP's `SupportedPackages`). Keep the existing `Provider` property.
- In `ProcessMessage`, special-case a line starting `SDWC%%SUPPORT%%` **before** the existing
  JSON-correlation `HandleResponse` path (its payload is a `|`-list, not JSON):
  1. Strip `SDWC%%SUPPORT%%`, split the remainder on `|`, trim tokens, drop empties.
  2. Build `SdwcServerCapabilities`; assign to `ServerCapabilities`.
  3. If `HasAnyQueryableAbility`, ensure the provider is registered (existing once-only
     `EnsureProviderRegistered`). If not, store caps but do **not** register.
  4. If `_declarationSent` is false, send the outbound declaration and set the flag.
  5. Return `true` (consumed).
- The `SDWC%%VERBS/PROPS/VERB-OVERLAY/PROP-OVERLAY%%<json>` correlation path is unchanged.
- `dome-client-user` lines now fall through and return `false` (unhandled; may display in the
  terminal — accepted for now).

### Outbound send

- On the first SUPPORT broadcast, send `#$# SDWC%%SUPPORT%%` via
  `IClientTerminal.SendOutOfBandLine`. The implementer verifies the OOB send path emits
  exactly `#$# SDWC%%SUPPORT%%` (correct prefix/spacing).
- `_declarationSent` (bool, guarded by the existing `_registrationLock`) enforces once-only.

### Lifecycle

- `OnDisconnected` additionally clears `ServerCapabilities` and resets `_declarationSent`
  (alongside the existing provider/correlator teardown), so a reconnect re-handshakes.

---

## Edge cases

- **Empty payload** (`SDWC%%SUPPORT%%` with nothing after): caps with an empty token set;
  `HasAnyQueryableAbility` false → provider not registered; declaration still sent once
  (it's a valid SUPPORT broadcast).
- **Leading space**: lines arrive with the leading space after `#$#` is stripped; the handler
  already `Trim()`s before matching.
- **Unknown tokens**: preserved in `RawTokens`; ignored by typed accessors.

---

## Testing (`Org.Edgerunner.Mud.Communication.Tests`)

`SdwcServerCapabilities` parsing:
- full list, subset, empty payload, unknown tokens preserved, case-insensitivity, whitespace
  trimming.

`SdwcOobHandler`:
- Inbound SUPPORT → `ServerCapabilities` populated (typed + raw) **and** provider registered
  when a queryable ability is present.
- Only-`SUPPORT` / no queryable ability → caps stored, provider **not** registered.
- Outbound `#$# SDWC%%SUPPORT%%` sent **exactly once** (fake `IClientTerminal` capturing
  `SendOutOfBandLine`); a second SUPPORT broadcast refreshes caps with **no** second send.
- `dome-client-user` line now returns `false` (no registration).
- Existing `VERBS`/`PROPS`/overlay JSON correlation still works (regression).
- `OnDisconnected` resets `ServerCapabilities` and the declaration flag.

Verification: `Communication.Tests` green; solution builds clean.

---

## Decisions

- React to the inbound SUPPORT broadcast (option 3); no login-event invention.
- Outbound declaration once-only to avoid ping-pong.
- `dome-client-user` handling removed entirely (out of scope; unhandled is acceptable).
- Capabilities modeled as typed accessors **plus** a raw token set (forward-compatible).
- Provider registered only when a queryable ability is advertised.
