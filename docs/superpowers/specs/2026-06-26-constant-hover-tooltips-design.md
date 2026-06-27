# Hover tooltips for Moo literal constants

**Date:** 2026-06-26
**Status:** Design approved

---

## Goal

Show a hover tooltip for Moo literal constants:
- **Type constants** (`NUM`, `INT`, `OBJ`, `STR`, `ERR`, `LIST`, `FLOAT`, `MAP`, `ANON`,
  `WAIF`, `BOOL`) → their numeric `typeof` code, e.g. `NUM => 0`.
- **Error constants** (`E_PERM`, `E_TYPE`, …) → their `tostr()` message, e.g.
  `E_PERM => Permission denied`.

A single comprehensive **baked-in table** is the source of truth, dialect-agnostic. A
**live-query layer** is added to the query provider (attempt → fall back to the table), but
**SDWC does not support constant queries**, so today the hover always uses the baked-in table.
No SDWC wire-protocol changes.

---

## A. Constant data table — `BuiltinConstantDocs`

New static class mirroring `BuiltinFunctionDocs` (JSON resource + `Lazy<IReadOnlyDictionary>`
+ `GetTooltipText`):
- Resource: `Org.Edgerunner.Moo.Editor/Resources/builtin-constant-docs.json`.
- Entry per constant: `{ "kind": "type" | "error" | "bool", "display": "<value-or-message>" }`.
- `GetTooltipText(name)` returns `"<name> => <display>"`, or `null` if unknown.

**Type codes** (`typeof`): `INT`=0, `NUM`=0, `OBJ`=1, `STR`=2, `ERR`=3, `LIST`=4, `FLOAT`=9,
`MAP`=10, `ANON`=12, `WAIF`=13, `BOOL`=14. **Verify the ToastStunt-specific codes
(`MAP`/`ANON`/`WAIF`/`BOOL`) against an authoritative ToastStunt source during
implementation** — don't trust memory.

**Error messages** (`tostr()`): `E_NONE`="No error" (0), `E_TYPE`="Type mismatch" (1),
`E_DIV`="Division by zero" (2), `E_PERM`="Permission denied" (3), `E_PROPNF`="Property not
found" (4), `E_VERBNF`="Verb not found" (5), `E_VARNF`="Variable not found" (6),
`E_INVIND`="Invalid indirection" (7), `E_RECMOVE`="Recursive move" (8), `E_MAXREC`="Too many
verb calls" (9), `E_RANGE`="Range error" (10), `E_ARGS`="Incorrect number of arguments" (11),
`E_NACC`="Move refused by destination" (12), `E_INVARG`="Invalid argument" (13),
`E_QUOTA`="Resource limit exceeded" (14), `E_FLOAT`="Floating-point arithmetic error" (15),
plus the ToastStunt additions `E_FILE`, `E_EXEC`, `E_INTRPT` — **verify their exact messages
during implementation.** (Errors display the message, not the number.)

**bool**: `true` / `false` (display their value).

`Moo.Constants` / `ConstantSet` already enumerate these. Add a public `Moo.IsConstant(name)`
accessor since `ConstantSet` is `private`.

---

## B. Hover branch

- `MooCodeEditor.ClassifyHoveredMember`: add a `HoverMemberKind.Constant` branch — when the
  token is an `IDENTIFIER` and `Moo.IsConstant(token.Text)` — placed **after** the built-in
  function check and **before** the bare-local fallback so it takes priority for constant
  names.
- `MooEditor_ToolTipNeeded`: for `HoverMemberKind.Constant`, show the tooltip via the existing
  `ShowToolTipAbove` path, content `"<name> => <display>"` (resolved per §C).

---

## C. Live-query layer (with fast fallback)

Add to `IMooWorldQueryProvider`:
```csharp
Task<string?> GetConstantValueAsync(string name, CancellationToken cancellationToken); // raw value (types)
Task<string?> GetConstantToStrAsync(string name, CancellationToken cancellationToken); // tostr() (errors)
```
Implementations:
- **`SdwcQueryProvider`** → return `null` (**unsupported**; no new SDWC command or capability
  token — SDWC does not support constant queries).
- **MCP query provider (`McpQueryProvider`)** → **implements** the queries via two new
  `edgerunner-org-moo-query` messages, `-constant-value` (raw `toliteral`) and
  `-constant-tostr` (`tostr()`), each taking a `constant` param. The protocol doc
  (`docs/edgerunner-org-moo-query-protocol.md`) and the server dump
  (`Server Packages/edgerunner-org-moo-query.moo`) are updated with the messages and
  server-side handlers; the handlers validate `constant` to a bare identifier and `eval` it, so
  the value/`tostr` are server-authoritative. (Originally stubbed to `null` — corrected.)
- **`CachingMooWorldQueryProvider`** → delegate to inner; pass through `null`, do not cache
  `null`.
- **`MooWorldQueryService`** → delegate to the registered provider; `null` when none.
- **Test fakes** → return configured values.

**Hover resolution** (in the constant branch): obtain the editor's query provider (same
accessor the verb/property hover uses); for a **type** call `GetConstantValueAsync`, for an
**error** call `GetConstantToStrAsync`. On a non-null result, render that; on `null` /
exception / no provider (offline), fall back to `BuiltinConstantDocs`. When connected over the
MCP query package the value is server-authoritative; over SDWC-only or offline it falls back
to the baked-in table.

---

## Scope

### In scope
- `BuiltinConstantDocs` table + JSON resource, `Moo.IsConstant`.
- Constant hover branch.
- Two `IMooWorldQueryProvider` methods. MCP implements them (new `-constant-value` /
  `-constant-tostr` messages + protocol doc + server `.moo` dump handlers); SDWC returns
  `null` (unsupported); caching/service delegate; fakes configurable.
- Attempt-then-fallback hover resolution.

### Out of scope
- Any SDWC wire-protocol change / server-side constant support (SDWC stays unsupported).
- Dialect-specific tables (one comprehensive table).
- Hover for non-constant literals (numbers, strings).

---

## D. Testing (`Org.Edgerunner.Moo.Editor.Tests`)

- `BuiltinConstantDocs`: type → `"NAME => <code>"`, error → `"NAME => <message>"`, bool, and
  unknown → `null`.
- `Moo.IsConstant`: true for listed constants, false otherwise.
- A constant-hover resolution helper (extract the resolve-then-fallback logic so it's testable
  without the FCTB control): with a fake provider returning a value → uses it; with a provider
  returning `null` → uses the table; offline (null provider) → uses the table.
- FCTB tooltip painting itself is not unit-testable; verified by build + manual hover.
- Full solution builds clean; all suites green.

---

## Decisions

- One comprehensive baked-in table (dialect-agnostic).
- Types display the numeric `typeof` code; errors display the `tostr()` message.
- Live query methods added to the provider. The MCP query package implements them
  (`-constant-value` / `-constant-tostr` messages, documented in the protocol doc and the
  server `.moo` dump); SDWC returns `null` (unsupported). The hover uses the live value when
  connected over MCP and falls back to the baked-in table over SDWC-only or offline.
- ToastStunt-specific type codes and error messages are verified against an authoritative
  source during implementation, not taken from memory.
