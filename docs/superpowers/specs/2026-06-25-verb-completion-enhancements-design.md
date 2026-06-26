# Verb completion enhancements

**Beads:** udd-16x (verb paren insertion), udd-wwk (core-reference verb listing)
**Date:** 2026-06-25
**Status:** Design approved
**Relationship:** udd-wwk depends on udd-16x (core-ref verbs reuse the paren-insertion).

---

## Goal

Make verbs first-class citizens in autocomplete:

1. **udd-16x** — selecting a verb inserts `verbname()` with the cursor **between** the parens
   (it's a call), instead of the bare name.
2. **udd-wwk** — typing `$` lists #0's **verbs** (callable as `$foo()`) in addition to its
   properties, which is all `$` lists today.

---

## Background — current behavior

- `FetchMemberListAsync` (`MemberCompletionController.cs:573-580`) routes
  `MemberContextKind.Verb` (the `:` trigger) to `GetVerbsAsync` → `BuildVerbItems`, and
  **everything else** — including `$` (`CoreReference`) — to `GetPropertiesAsync` →
  `BuildPropertyItems`.
- `BuildVerbItems` (`MemberCompletionController.cs:653-660`) emits
  `MemberCompletionItem(name, icon)`, which inserts the **bare verb name**.
- Built-in functions, by contrast, are pre-built into snippets in the `Moo` static
  constructor (`Moo.cs:12-15`): arg-taking → `name(^)` (cursor between), no-arg →
  `name()^` (cursor after), where `^` is FCTB's caret marker.

---

## udd-16x — Verb insertion with parens

Verb completion items become **snippet-capable**, inserting `verbname(^)` — parens with the
cursor between them — mirroring how arg-taking builtins already work.

- Change `BuildVerbItems` so each verb item carries the snippet form `name(^)` (FCTB caret
  marker) rather than a bare-name `MemberCompletionItem`. Keep the existing verb / inherited
  icon selection.
- **Arity:** `MooVerbSummary` does not give us argument count, so verbs *always* insert
  `name(^)` (cursor between) regardless of whether the verb takes arguments. Accepted.
- **Properties and core-reference properties are unchanged** — they insert the bare name
  (not callable, no parens).
- **Built-in functions are left exactly as-is**: arg-taking stay `name(^)`, no-arg stay
  `name()^` (cursor after). We do *not* unify no-arg builtins to cursor-between, because for
  builtins we *do* know arity.

### Decision summary (insertion)

| Item kind | Inserts | Cursor |
|---|---|---|
| Verb (member `:` or core `$`) | `name()` | between parens |
| Built-in function, arg-taking | `name()` | between parens |
| Built-in function, no-arg | `name()` | after parens |
| Property / core-ref property | `name` | n/a |

---

## udd-wwk — Core-reference (`$`) lists verbs too

For `MemberContextKind.CoreReference`, fetch **both** properties and verbs on #0 and merge.

- In `FetchMemberListAsync`, when `kind == CoreReference`: call **both**
  `GetPropertiesAsync` and `GetVerbsAsync` on #0; build property items via
  `BuildPropertyItems` (core/property icon) and verb items via `BuildVerbItems` (verb icon);
  then return them as **one list sorted alphabetically by name together** (case-insensitive,
  stable) — verbs and properties **interleaved**, never two separate runs. (Per the global
  rule that every completion list is alphabetically sorted as a whole. Fixed in udd-7wl after
  an initial implementation concatenated props-then-verbs.)
- **Same-name property + verb both shown.** #0 can have a property `foo` *and* a verb `foo`
  (`$foo` reads the property, `$foo()` calls the verb). Both entries appear — FCTB's
  `DoAutocomplete` does not dedup by text, so each renders with its own icon and inserts its
  own form (property → `foo`, verb → `foo(^)`).
- **Reuses udd-16x:** core-reference verb items go through the same `BuildVerbItems`
  paren-insertion, so udd-wwk depends on udd-16x.
- The member-verb (`:`) and member-property/core-property paths are otherwise unchanged; TTL
  caching behavior is preserved. Only the set fetched for `$` widens.

---

## Tests & verification

udd-16x:
- Selecting a member verb inserts `verbname()` with caret between the parens.
- Properties / core-ref properties still insert bare names.
- Built-in function insertion unchanged (arg-taking between, no-arg after).

udd-wwk:
- `$` lists #0 properties (core icon) **and** #0 verbs (verb icon).
- A name present as both a property and a verb on #0 produces two entries.
- Selecting a core-ref verb inserts `name()` cursor-between (via udd-16x).

Both: full Editor test suite green; app builds clean.

---

## Decisions

- Two beads (udd-16x, udd-wwk); udd-wwk depends on udd-16x.
- Verbs always insert `name(^)` (cursor between), arity unknown — accepted.
- No-arg builtins left as `name()^` (cursor after); not unified.
- Properties never get parens.
- Same-name property+verb both listed (no dedup by text).
