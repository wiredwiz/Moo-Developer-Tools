# Design: IMooWorldQueryProvider — abstract dev-info query mechanism

**Date:** 2026-06-07
**Status:** Approved (design)
**Bead:** udd-5oh (blocks udd-am1 SDWC impl, udd-btl MCP package impl)

## Purpose

Define a protocol-agnostic mechanism for querying a connected MOO world for
developer/introspection information (verbs, properties, objects, hierarchy, verb
code, property values). Concrete providers (SDWC — `udd-am1`; an MCP package —
`udd-btl`) implement the interface and register themselves per connection. The
editor consumes the aggregated result to power code-completion popups and
tooltips.

This bead delivers the **generic mechanism** only: the interface, the data
models, the priority/fallthrough routing aggregate, the per-connection caching
layer, the per-connection service and its anchor on the session, and the
editor-side accessor. Protocol-specific capability detection and query
implementations are delivered by `udd-am1` and `udd-btl`.

## Location & dependencies

All contracts and the generic machinery live in **`Org.Edgerunner.Mud.Common`**,
namespace **`Org.Edgerunner.Mud.Common.Querying`**. It is the only project
referenced by the SDWC implementation (comms layer), the MCP implementation
(`Mud.MCP`), and the editor (`Moo.Editor`). MOO-specific types use a `Moo`
prefix.

Dependency graph (low→high): `Common`/`Mud.Common` → `Mud.Communication` →
`Mud.MCP` → `Moo.Editor`.

## A. Interface & data models

### `IMooWorldQueryProvider`

All methods are async and take a `CancellationToken`.

Summary / list:
- `GetCoreObjectsAsync(ct)` → `IReadOnlyList<MooObjectSummary>` — known core (`$`-registered) objects
- `GetChildrenAsync(MooObjectId, ct)` → `IReadOnlyList<MooObjectSummary>`
- `GetOwnedObjectsAsync(ct)` → `IReadOnlyList<MooObjectSummary>` — objects owned by the current player connection
- `GetOwnedObjectsAsync(MooObjectId owner, ct)` → `IReadOnlyList<MooObjectSummary>` — objects owned by the specified owner
- `GetParentAsync(MooObjectId, ct)` → `MooObjectId?` (none/`#-1` → null)
- `GetVerbsAsync(MooObjectId, ct)` → `IReadOnlyList<MooVerbSummary>`
- `GetVerbDocumentationAsync(MooObjectId, verbName, ct)` → `IReadOnlyList<string>`
- `GetPropertiesAsync(MooObjectId, ct)` → `IReadOnlyList<MooPropertySummary>`

Detail:
- `GetVerbInfoAsync(MooObjectId, verbName, ct)` → `MooVerbInfo?`
- `GetPropertyInfoAsync(MooObjectId, propName, ct)` → `MooPropertyInfo?`
- `GetVerbCodeAsync(MooObjectId, verbName, ct)` → `IReadOnlyList<string>` (source lines)
- `GetPropertyValueAsync(MooObjectId, propName, ct)` → `MooPropertyValue?` (nullable so exhaustion is expressible)

### Data models

- **`MooObjectId`** — readonly struct over the MOO object number (`#123`). Static
  properties matching MOO usage: `Nothing` (`#-1`), `AmbiguousMatch` (`#-2`),
  `FailedMatch` (`#-3`).
- **`MooObjectSummary`** `{ MooObjectId Id; string Name; IReadOnlyList<string> Aliases }`
- **`MooVerbSummary`** `{ IReadOnlyList<string> Aliases; MooObjectId DefiningObject }`
- **`MooPropertySummary`** `{ string Name; MooObjectId DefiningObject }`
- **`MooVerbInfo`** (level B): `{ IReadOnlyList<string> Aliases; MooObjectId Owner; VerbPermission Permissions; VerbArgs Args; MooObjectId DefiningObject }`
- **`MooPropertyInfo`** (level B): `{ string Name; MooObjectId Owner; PropertyPermission Permissions; MooObjectId DefiningObject; int ValueType; string ValuePreview }`
- **`MooPropertyValue`** `{ int Type; string Literal }`
- **`PropertyPermission`** (struct) `{ bool Read; bool Write; bool ChangeOwnership }`
- **`VerbPermission`** (struct) `{ bool Read; bool Write; bool Execute; bool Debug }`
- **`VerbArgs`** (struct) `{ DirectObject DirectObject; Preposition Preposition; IndirectObject IndirectObject }`
- **`DirectObject`** (enum): `This`, `None`, `Any`
- **`IndirectObject`** (enum): `This`, `None`, `Any`
- **`Preposition`** (enum): `None`, `Any`, plus the standard LambdaMOO prepositions
  (each member documents its aliases):
  - `With` (with/using)
  - `At` (at/to)
  - `InFrontOf` (in front of)
  - `In` (in/inside/into)
  - `OnTopOf` (on top of/on/onto/upon)
  - `OutOf` (out of/from inside/from)
  - `Over`
  - `Through`
  - `Under` (under/underneath/beneath)
  - `Behind`
  - `Beside`
  - `For` (for/about)
  - `Is`
  - `As`
  - `Off` (off/off of)

## B. Routing aggregate + caching

Composition per connection (both decorators implement `IMooWorldQueryProvider`,
so they stack cleanly and the editor holds a single `IMooWorldQueryProvider`):

```
editor → CachingMooWorldQueryProvider → MooWorldQueryProviderRegistry → [SDWC, MCP, …]
```

### `MooWorldQueryProviderRegistry` (priority + fallthrough aggregate)

- `Register(IMooWorldQueryProvider provider, int priority)` / `Unregister(provider)`;
  higher priority is preferred. A `ProvidersChanged` event fires on changes.
- For each method: walk providers high→low priority, `await` each; on
  `NotImplementedException` move to the next provider; the first provider that
  returns wins.
- **Any exception other than `NotImplementedException` surfaces** to the caller
  (a timeout is a real failure, not a fall-through). `OperationCanceledException`
  propagates.
- **Exhaustion** (no providers, or all threw `NotImplementedException`) degrades
  gracefully so the editor needs no try/catch for the common case: list methods
  return empty, nullable single-value methods return null.

### `CachingMooWorldQueryProvider` (per-connection TTL cache decorator)

- Key: `(operation, objectId, verbName/propName)`. On read: return the cached
  entry if present and unexpired; otherwise call the inner provider, store the
  result with a timestamp, and return it.
- **Time-based expiry**: configurable TTL (default ~60s), checked lazily on read,
  plus a periodic sweep to bound memory.
- Caches successful results **including empty/null** (so unsupported ops are not
  re-attempted on every keystroke). **Never caches exceptions.**
- **Manual invalidation**: `InvalidateObject(MooObjectId)`, `Invalidate(operation, …)`,
  `Clear()` — so the local-edit/upload flow can drop stale entries after a verb
  or property changes.
- On the registry's `ProvidersChanged`, the cache is cleared so newly-supported
  operations get a fresh chance.
- Thread-safe (`ConcurrentDictionary` + timestamps); cancellation flows through to
  the inner provider on a miss.

## C. Connection binding, editor access, errors, testing

### Per-connection service

`MooWorldQueryService` (in `Mud.Common`) owns the registry + caching decorator and
exposes: `Register`/`Unregister`, the query surface (`IMooWorldQueryProvider`, =
the cache), invalidation, and `ProvidersChanged`. It is connection-agnostic
(references no session type).

### Anchor

Exposed on **`IMudClientSession`** as `MooWorldQueryService QueryProviders { get; }`,
instantiated per session in `MudClientSession` and `TlsMudClientSession`. This is
the only level reachable by SDWC (comms), the MCP layer (references comms), and
the editor (references comms). It is a mild layering note (a MOO-specific service
on a generic MUD session) accepted because it is the shared level and the bead
calls for binding to the session.

Providers self-register: SDWC's OOB handler and the MCP dev-info package each call
`session.QueryProviders.Register(provider, priority)` when their capability is
detected. Detection itself is out of scope here (`udd-am1` / `udd-btl`).

### Editor access

A `MooCodeEditorPage` obtains its connection's `IMooWorldQueryProvider` by
resolving the owning session (the `MooClientTerminal` owns the live
`IMudClientSession`; resolution uses the existing `WindowManager`/world
association) and reading `session.QueryProviders`. The current editor→connection
link is thin (the page carries only `worldName`), so this bead adds a minimal
resolver (the page holds or looks up its session). Full editor consumption is
proven when the first real provider lands in `udd-am1`. Editor query calls are
defensive: they catch/log failures and skip the enhancement (graceful exhaustion
already covers the "no provider" case).

### Error handling summary

- `NotImplementedException` → fall through to next provider (registry).
- `OperationCanceledException` → propagate; not cached.
- Any other exception → surfaces to caller; not cached.
- Exhausted chain → empty (lists) / null (nullable singles).

### Testing

Add **`Org.Edgerunner.Mud.Common.Tests`** (mirroring `Mud.MCP.Tests` — xUnit +
FluentAssertions) exercising the pure logic with fake providers:
- priority ordering
- `NotImplementedException` fall-through to the next provider
- non-`NotImplementedException` exceptions surface (no fall-through)
- exhaustion → empty/null
- cache hit/miss
- TTL expiry
- manual invalidation (`InvalidateObject`, `Clear`)
- `ProvidersChanged` clears the cache

## Out of scope (this bead)

- SDWC capability detection and query implementation (`udd-am1`).
- MCP dev-info package and its detection/implementation (`udd-btl`).
- Per-operation capability advertisement for UI enablement (e.g., enabling an
  object browser only when listing objects is supported). Not needed for
  autocomplete/tooltips, which degrade gracefully. Can be added later if a
  feature requires it.
- Full editor↔connection wiring beyond the minimal resolver; completed alongside
  the first provider.
