# Variable-name harvesting for non-member completion

**Bead:** udd-afl
**Date:** 2026-06-25
**Status:** Design approved
**Depends on:** udd-efk (completion snapshot + `DynamicCompletionSource`), udd-2s4 (`FlowValueResolver` traversal pattern)

---

## Goal

When the user types a bare identifier prefix (a **non-member** completion — no preceding
`.` or `:`), add every variable name referenced in the verb to the completion list,
alongside the existing keywords, built-in functions, and snippets.

This is a **flat lexical harvest**. We deliberately do **not** reason about whether a
variable is assigned inside branching logic, or assigned only later in the code. It is not
our job to second-guess the user's logic — we surface every known variable name and let the
user choose.

This is distinct from udd-2s4: udd-2s4 resolves the *current value* of a single variable at
the caret (flow-aware); this feature merely *lists the names* of all variables (flow-blind).

---

## Scope

### In scope
- Collect every identifier that appears in a **variable position** in the verb's parse tree.
- "Variable position" = an identifier that is **not** the member name on the right of `.` or
  `:` (those are property/verb names, not variables) and **not** a function-call name.
- Include all forms: plain/compound/scatter assignment targets, `for`-loop variables,
  `try`/`except` error variables, and bare read references to names assigned nowhere. Under
  the chosen semantics, every identifier sitting in a variable position counts — assigned or
  not.
- Inject the harvested names as `CompletionIconCategory.Variable` entries into the
  non-member completion list, re-collected dynamically on every popup refresh.

### Out of scope
- No flow / branch / reachability reasoning (that is udd-2s4).
- No value resolution — names only.
- No changes to member completion (the `.`/`:` path).
- No new icon assets — `CompletionIconCategory.Variable` already exists.

---

## Component 1 — `VariableReferenceCollector`

New pure, static, stateless helper in `Org.Edgerunner.Moo.Editor/Autocomplete/`, alongside
`FlowValueResolver`.

```csharp
public static IReadOnlyCollection<string> CollectVariableNames(
    ParserRuleContext? tree,
    int caretOffset)
```

Behaviour:

1. Walk the parse tree (reusing the same `Walk` traversal pattern as `FlowValueResolver`),
   visiting every identifier token.
2. Keep only identifiers in a **variable position** — exclude the member name on the right of
   `.`/`:` and exclude call names.
3. Record each surviving occurrence **with its source span**.
4. Identify the occurrence the caret sits in (or immediately right of) — the in-progress
   identifier the user is actively typing — and **drop that single occurrence**.
5. Build the result set from the remaining occurrences, de-duplicated **case-insensitively**
   while preserving the as-written casing of the first occurrence kept.
6. Null / empty tree → empty set.

### Caret-exclusion rule (no self-referential noise)

The name being typed must never be offered as a completion of itself. Because we drop only
the *occurrence under the caret*, a name survives if and only if it genuinely appears
**elsewhere** in the verb as a variable:

- `cou|` where `count` exists elsewhere → `count` is offered (the caret occurrence `cou`
  is dropped, the other `count` occurrence keeps the name).
- `newName|` used nowhere else → dropped entirely (its only occurrence is the one being
  typed).

`caretOffset` is the absolute buffer offset, derived from the snapshot's 1-based
`CaretLine`/`CaretColumn` using the same line/col → offset conversion `FlowValueResolver`
already relies on.

---

## Component 2 — Wiring into the completion list

The static completion list is built once in `MooCodeEditorPage.BuildAutocompleteMenu`, but
variables change on every keystroke. So variable collection happens **dynamically**, inside
`DynamicCompletionSource.GetEnumerator()`, using the `Tree` and caret already carried on
`MemberCompletionContextSnapshot` (no new plumbing).

On each popup refresh:

1. **Member items** yield first — unchanged.
2. **Variables + static items** are produced as one alphabetically-sorted block:
   1. `VariableReferenceCollector.CollectVariableNames(snapshot.Tree, caretOffset)`.
   2. **Asymmetric dedup against the static list** (see below).
   3. Wrap survivors as `AutocompleteItem`s with `CompletionIconCategory.Variable`.
   4. **Merge** the already-sorted static list with the sorted variable list into one
      alphabetical sequence and yield it.

Because variables are dynamic, the merge happens at enumeration time rather than being baked
into the pre-sorted static list at build time. Matching stays closest-match
(framework-handled in `AutocompleteMenu.DoAutocomplete` /
`AutocompleteMatchRanker`); we only control sort/yield order.

### Asymmetric dedup rule

FCTB's `AutocompleteMenu.DoAutocomplete` does **not** dedup by text — it shows every item
whose `Compare(text)` matches, each rendered with its own icon. We exploit this:

- **Built-in *functions*** (`Moo.Builtins` — e.g. `read`, `notify`): **do NOT dedup.** A
  local variable shadowing a function is meaningful. Emit **both**, distinguished by icon and
  by what each inserts:
  - `read` — Variable icon, inserts `read`
  - `read` — Function icon, inserts `read(^)` (the builtin's existing parameterized snippet)
- **Built-in *variables* / keywords** (`Moo.Keywords` — `this`, `player`, `caller`, `args`,
  type/error constants, control-flow words): **do dedup** — suppress the harvested variable
  entry. Here the keyword entry *is* the variable: same name, same concept, inserts the same
  text, so a second row is pure duplicate noise. (Control-flow words like `if`/`for` can't be
  variables anyway and never collide.)

So a harvested name is suppressed only when it matches a `Moo.Keywords` entry; otherwise it
is always emitted, even if it matches a built-in function.

---

## Component 3 — Tests & verification

Unit tests in the Editor test project, alongside the existing `FlowValueResolver` tests:

`VariableReferenceCollector`:
- Plain, compound (`+=` etc.), and scatter (`{a, b} = ...`) assignment targets.
- `for`-loop variables and `try`/`except` error variables.
- Bare read references to names assigned nowhere.
- A name used both as a member name (right of `.`/`:` — must be **excluded**) and elsewhere
  as a variable (must be **included**).
- Caret-exclusion: only occurrence under the caret → dropped; also-used-elsewhere → kept.

Dedup:
- A name equal to a `Moo.Keywords` entry (e.g. `player`) → no separate variable entry.
- A name equal to a built-in function (e.g. `read`) → variable entry **and** function entry
  both present.

Verification bar (matching udd-2s4):
- Full Editor test suite green.
- App builds clean.

---

## Decisions

- **Semantics:** every identifier in a variable position counts (assigned or not) — broadest,
  simplest, and can't second-guess the user's logic.
- **Source:** parse tree only (ANTLR error recovery still yields a usable tree mid-edit).
- **Presentation:** uniform — alphabetically sorted, closest-match filtered, exactly like
  every other list. No special priority tier for variables.
- **Caret occurrence excluded** unless the name appears elsewhere (no self-referential noise).
- **Dedup is asymmetric:** suppress vs `Moo.Keywords`, keep vs `Moo.Builtins`.
- **Case:** dedup case-insensitively, preserve as-written casing.
