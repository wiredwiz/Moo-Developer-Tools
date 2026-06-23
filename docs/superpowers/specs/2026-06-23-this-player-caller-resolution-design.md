# `this` / `player` / `caller` default resolution — design (udd-lu1)

**Date:** 2026-06-23
**Status:** Approved (design), pending spec review
**Bead:** udd-lu1 (feature, P2). Phase 2 (dynamic reassignment) is udd-efk.

## Goal

Make the predefined Moo variables `this`, `player`, and `caller` work as known operands for member
(verb/property) **autocomplete** and **hover tooltips**, resolving to their **default** objects so
that typing `this:`, `player.`, `caller:` etc. offers the right object's verbs/properties.

This phase is **static default resolution only**. Tracking reassignment (`player = #23;`) and chained
expressions is the follow-up (udd-efk).

## Key fact

The client does **not** know the connected player's object number — `MooClientTerminal` only tracks
a `_LoggedInConnection` bool. So `player`/`caller` must be resolved by **querying the server**.
`this`, by contrast, is already known locally via the page's `ContextObjectId` (the object the edited
verb lives on).

## Current state

- `MemberOperandResolver.Resolve` already maps operand `this` → `contextObjectId` (sync), `#N` →
  object (sync), and returns `null` for `$foo` (which `TryGetCoreName` then flags for async core-name
  resolution in `MemberCompletionController.FetchCoreNameAsync`).
- `MooCodeEditorPage.ResolveHoverOperandAsync` already resolves `this` → `ContextObjectId`, `#N`, and
  `$name` (via `#0` prop-value).
- `player`/`caller` are not handled anywhere: `Resolve` returns `null`, they are not core names, so no
  resolution happens and no completion/hover is offered.

## Design

### 1. `this` — verify only

`this` already resolves to `ContextObjectId` in both the completion path (`MemberOperandResolver`) and
the hover path (`ResolveHoverOperandAsync`). Phase 1 verifies end-to-end behavior for **verbs and
properties** in both surfaces; no new code unless a gap is found.

### 2. New provider query — `GetCurrentPlayerAsync`

Add to `IMooWorldQueryProvider`:
```csharp
Task<MooObjectId?> GetCurrentPlayerAsync(CancellationToken cancellationToken);
```
Returns the connected player's object id, or `null` if unknown/unsupported.

- **`MooWorldQueryProviderRegistry`** (the aggregate): route like every other method — first provider
  that answers wins; a provider throwing `NotImplementedException` falls through; exhausted → `null`.
- **`McpQueryProvider`**: a new `player` request (no parameters beyond `tag`) → awaits a
  `player-reply` carrying the connected player's object id; maps to `MooObjectId?`.
- **Server `Server Packages/edgerunner-org-moo-query.moo`**: new `handle_player(session, tag)` that
  returns the connection's player object — `toint(session.connection)` — via `send_reply` with a new
  `player-reply` message. Add to the `;;messages_in` (`{"player", {"tag"}}`) and `;;messages_out`
  (`{"player-reply", {"tag", "data"}}`) declarations and to
  `docs/edgerunner-org-moo-query-protocol.md`. The handler verb is `handle_player` (single word — no
  hyphen-dispatch issue from the earlier handler-naming work).
- **`SdwcQueryProvider`**: throws `NotImplementedException` for now (registry falls through; MCP at
  priority 200 answers anyway). A real SDWC implementation can be added later.

### 3. Resolution wiring — `player` / `caller`

Both `player` and `caller` resolve to the **current player** in this phase (caller's true dynamic
value is udd-efk).

- **`MemberOperandResolver`**: add `bool TryGetCurrentPlayerOperand(MemberCompletionContext context)`
  (or equivalent) returning `true` when the Verb/Property operand is `player` or `caller`. `Resolve`
  keeps returning `null` for them (they need async resolution), mirroring the `$foo`/`TryGetCoreName`
  shape.
- **`MemberCompletionController`**: add a current-player resolution path that mirrors
  `FetchCoreNameAsync` — when the operand is `player`/`caller`, call `GetCurrentPlayerAsync`, **cache**
  the resulting object id, then proceed to the normal verb/property fetch on that object. Cache is
  invalidated on disconnect (see §4).
- **`MooCodeEditorPage.ResolveHoverOperandAsync`**: resolve operands `player`/`caller` via
  `GetCurrentPlayerAsync` (using the same cache) in addition to the existing `this`/`#N`/`$name`.

### 4. Caching + reset

The current player is stable for the life of a connection, so cache it in `MemberCompletionController`
as one nullable object id, using the **same TTL approach as the existing core-name cache** (the
`_cacheTimeToLive` already in that controller) — consistent with how `$foo` resolutions are cached.
On disconnect the provider is unregistered/faulted (`udd-bju`), so a stale entry cannot be re-fetched
against a dead connection, and the TTL refreshes it on the next `player`/`caller` use after reconnect.
No separate invalidation hook is added.

## Data flow (player/caller completion)

1. User types `player:` → context detector classifies Verb, operand `player`.
2. `MemberOperandResolver.Resolve` returns null; `TryGetCurrentPlayerOperand` returns true.
3. `MemberCompletionController` checks the current-player cache; on miss, calls
   `provider.GetCurrentPlayerAsync` → object id, caches it.
4. Fetches verbs on that object and refreshes the popup (same as the core-name path).
5. Hover over `player:foo` runs the same resolution via `ResolveHoverOperandAsync`.

## Testing

- **Provider:** `MooWorldQueryProviderRegistry.GetCurrentPlayerAsync` routes/falls through;
  `McpQueryProvider.GetCurrentPlayerAsync` round-trips a `player`/`player-reply` against a fake
  terminal; `SdwcQueryProvider.GetCurrentPlayerAsync` throws `NotImplementedException`.
- **Resolver:** `TryGetCurrentPlayerOperand` true for `player`/`caller`, false otherwise; `Resolve`
  still returns `ContextObjectId` for `this`.
- **Controller/hover:** `player`/`caller` resolve to the queried player object for both completion and
  hover; cache hit avoids a second query; cache reset on disconnect re-queries.
- **`this`:** still resolves to `ContextObjectId` in completion + hover (regression guard).

## Out of scope (→ udd-efk)

- Reassigned variables (`player = #23;` from that point on) and any non-default value of
  `this`/`player`/`caller`.
- Chained-expression evaluation (`$Mcp.package:`), and lazy interpretation of arbitrary local
  variables via a syntax-tree walker.
- A real SDWC `GetCurrentPlayerAsync`.
