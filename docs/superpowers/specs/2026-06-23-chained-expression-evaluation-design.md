# Chained-expression evaluation for member completion — design (udd-efk)

**Date:** 2026-06-23
**Status:** Approved (design), pending spec review
**Bead:** udd-efk (feature, P3). Depends on udd-lu1 (static `this`/`player`/`caller` resolution).

## Goal

Resolve **chained member-access expressions** at the caret so member (verb/property) autocomplete and
hover tooltips offer the right object's members. Typing `$Mcp.package:` should evaluate the chain
`$Mcp` → `.package` to a concrete object number, then complete verbs/properties on that object.

The chain **base** may be `#N`, `$name`, `this`, `player`, `caller`, or a **local variable**. A
local-variable base is resolved by a grammar-aware **tree walk** over the verb's parse tree to its
nearest preceding assignment, whose right-hand side is itself evaluated as a chain (recursively).

### Scope

In scope:

- Chained member access `<base>.prop[.prop…]:` / `.prop` for both completion and hover.
- A **minimal assignment walker**: a local-variable base resolved from its nearest preceding
  assignment (any block depth), with the RHS evaluated as a full resolvable chain (recursive).

Out of scope (→ follow-up):

- **General lazy reassignment tracking** of `this`/`player`/`caller` (and arbitrary locals) for the
  member directly under the caret independent of chains — i.e. `player = #23; … player:` resolving to
  `#23`. This design resolves variables only when they appear as a **chain base**; full flow-aware
  reassignment interpretation remains the larger second half of udd-efk and is split to its own bead.
- Branch-reachability analysis. The walker takes the textually-nearest preceding assignment regardless
  of `if`/`while`/`for` nesting; it does not reason about whether that branch executes.

## Current state

- `MemberCompletionContextDetector` extracts the operand before `:`/`.` via a regex that captures only
  the **last single identifier** (`(\$?[A-Za-z_]\w*|#-?\d+)([:.])\w*$`). It cannot see a chain like
  `$Mcp.package`.
- `MemberOperandResolver.Resolve` maps `this` → `contextObjectId` (sync) and `#N` → object (sync), and
  returns `null` for `$name`/`player`/`caller`, which the controller then resolves asynchronously.
- `MemberCompletionController` (one instance **per `MooCodeEditorPage`** — `MooCodeEditorPage.cs:324`)
  drives async resolution with an empty-then-refresh popup flow and instance-field caches
  (`_cache`, `_coreNameCache`, `_currentPlayerCache`) with a TTL (`DefaultCacheTimeToLive` = 30s).
- `MooCodeEditorPage.ResolveHoverOperandAsync` resolves the same single-atom forms for hover, awaiting
  async cases directly.
- `IMooWorldQueryProvider.GetPropertyValueAsync(objectId, propName, ct)` already returns a
  `MooPropertyValue(int Type, string Literal)`; it is the existing mechanism used to resolve `$name`
  (read property `name` off `#0`, require an object literal `#N`). **It is the per-step primitive chained
  evaluation needs.**
- `MooCodeEditor.ParseSourceCode()` already produces a current `ParserRuleContext` parse tree (and a
  `DetailedToken` list) for the buffer on every change, used today for folding/indentation/errors.

## Design

Three new units in the **Editor library** (`Org.Edgerunner.Moo.Editor`) so both the completion
controller and the hover page share them, plus integration at the two existing call sites.

```
caret + buffer + tokens + parse tree
        │
        ▼
  ChainExtractor  ──►  ChainDescriptor { Base, Steps[], MemberKind(:/.), PartialFragment }
        │
        ▼
  ChainExpressionEvaluator  ◄──►  LocalVariableResolver   (pure tree walk)
   (async, recursive, guards)      name+caret → RHS subtree → ChainDescriptor
        │
        ▼  final MooObjectId?
  existing GetVerbsAsync / GetPropertiesAsync  ──►  completion items / hover content
```

### 1. `ChainExtractor` (synchronous; Approach A — parse-tree-primary, token fallback)

Given the buffer, caret, `DetailedToken` list, and `ParserRuleContext`, produce a `ChainDescriptor`:

- **`Base`** — the leftmost element: `#N`, `$name`, `this`, `player`, `caller`, or a local-var ident.
- **`Steps`** — the ordered `.prop` identifiers already typed between base and the trailing separator.
- **`MemberKind`** — verb (`:`) vs property (`.`) being completed.
- **`PartialFragment`** — the text after the trailing separator (filter text), **not** part of the
  chain to resolve.

Mechanism:

1. The completion trigger is the trailing `:`/`.`. Read the chain to its **left from the parse tree**:
   find the expression node ending immediately before the separator. A grammatically-complete chain
   such as `$Mcp.package` yields a property-reference subtree read directly (base `$Mcp`, steps
   `[package]`). Operator/call boundaries are decided by the grammar, so `foo() + $bar.baz` extracts
   base `$bar`, not the whole left operand.
2. **Token fallback** only for the incomplete tail the tree cannot represent (e.g. a just-typed
   `$Mcp.` with a dangling dot): scan left over a contiguous `base (. ident)*` run in the token list.
3. A plain single identifier is a **length-1 chain**, subsuming the current detector behavior.
4. No valid base (a bareword that is not a known local var, or a non-chainable expression such as a
   list/string literal) → no descriptor → **no source**.

### 2. `ChainExpressionEvaluator` (async, recursive)

Resolves a `ChainDescriptor` to a final `MooObjectId?`.

**Base resolution:**

| Base form          | Resolution                                              |
|--------------------|---------------------------------------------------------|
| `#N`               | `MooObjectId(N)` directly                               |
| `$name`            | `GetPropertyValueAsync(#0, name)` → require object lit   |
| `this`             | `contextObjectId` (the page's edited object)            |
| `player` / `caller`| `GetCurrentPlayerAsync()`                               |
| local-var ident    | `LocalVariableResolver` → RHS `ChainDescriptor` → recurse|

**Step walk:** for each `.prop` in `Steps`, call `GetPropertyValueAsync(currentObj, prop)`; require
`Type == object` and a parsable `#N` (N ≥ 0). Any non-object value, missing property, or provider
error → `null` → **no source**. The final surviving object id feeds the existing verb/property fetch.

**Guards:**

- **Cycle guard** — a set of variable names currently being resolved; re-entry (`x = y; y = x;`)
  aborts that branch → unknown.
- **Depth guard** — a max budget of **8** total resolution steps (var-hops + property steps) to bound
  async query fan-out; exceeding it → unknown.

Every guard trip and every unresolved step degrades to **no source**, never a user-facing error.

### 3. `LocalVariableResolver` (pure tree walk)

Given a variable name and the caret position, walk the `ParserRuleContext` for assignment nodes whose
LHS is that variable and whose position **precedes the caret**; take the **textually nearest** one
(any block depth). Convert its RHS expression subtree into a `ChainDescriptor` and return it to the
evaluator for recursive resolution. If the RHS is not a resolvable chain (verb call, function result,
arithmetic, list/string literal, etc.) → unknown → **no source**.

### 4. Caching — two layers, cached differently

The cross-window/per-verb correctness hinge: separate **world state** from **verb state**.

1. **World/server state** — `(objectId, propName) → resolved MooObjectId?` (e.g. `#0.Mcp → #123`,
   `#123.package → #456`). Verb-independent and identical in every window on the same connection
   (`$Mcp` is `#0.Mcp` everywhere). **TTL-cached** as an instance field on the per-page controller —
   generalizing today's `_coreNameCache`, which is the `(#0, name)` special case. Per-page instancing
   keeps it connection-correct.
2. **Local-variable values** — `x → value`, derived from **this verb's** parse tree and caret. This is
   verb state and changes keystroke-to-keystroke, so it gets **no persistent cache**. The
   `LocalVariableResolver` re-walks the live parse tree (kept current by the editor on every edit) on
   **every** completion/hover request. Its only short-lived memory is the **in-call memo** for the
   duration of one resolution pass (which also powers the cycle guard), then discarded.

This makes variable resolution inherently unique per verb — recomputed from that verb's own tree every
time, never stored, never shared. Only genuine server state is shared across windows, which is safe
because it does not depend on any buffer.

In-flight dedup carries over from today: identical chains resolving concurrently share one fetch; the
popup/tooltip refreshes when results land. Disconnect resets caches (consistent with udd-bju).

### 5. Async flow & integration

- **Completion** — `MemberCompletionController.GetMemberItems` stays non-blocking: extract the
  descriptor synchronously; if the resolved object is already cached, return members immediately;
  otherwise kick off async evaluation, return empty, and refresh the popup on completion — the existing
  empty-then-refresh path. The branchy sync/async per-form operand logic is replaced by
  **extract → evaluate → fetch**; single-atom forms become length-1 chains.
- **Hover** — `MooCodeEditorPage.ResolveHoverOperandAsync` replaces its per-form `if` ladder with one
  evaluator call (it already runs async with the provider in hand).
- **Consolidation, not parallel paths** — the evaluator subsumes every operand form both call sites
  handle today (`#N`, `$name`, `this`, `player`, `caller`). `MemberOperandResolver` /
  `MemberCompletionContextDetector` logic is folded into the extractor; their existing tests become
  regression coverage for the length-1 case.
- The evaluator needs `contextObjectId` (for `this`) and the provider — both already threaded to both
  call sites.

## Data flow (chained completion)

1. User types `$Mcp.package:` → `ChainExtractor` produces `{ Base: $Mcp, Steps: [package],
   MemberKind: Verb, PartialFragment: "" }`.
2. `ChainExpressionEvaluator` resolves base `$Mcp` via `GetPropertyValueAsync(#0, "Mcp")` → `#123`
   (world-state cache).
3. Walks step `package`: `GetPropertyValueAsync(#123, "package")` → `#456` (cache).
4. Fetches verbs on `#456` via the existing path and refreshes the popup.
5. Hover over `$Mcp.package:foo` runs the same resolution via `ResolveHoverOperandAsync`.

Data flow (local-var base): `x.bar:` where `x = $foo;` earlier → extractor base `x` → resolver finds
nearest preceding `x = $foo;` → RHS descriptor `{ Base: $foo }` → evaluator resolves `$foo` →
object → step `bar` → final object → complete on it.

## Error handling

Every failure mode — unresolvable base, non-object step, missing property, non-chain RHS, cycle,
depth-exceeded, provider exception — degrades to **no source** (empty completion / no hover member
info). Never a dialog or thrown error. Best-effort completion leaves the static list in place, as today.

## Testing

- **`ChainExtractor`:** caret after `$Mcp.package:`, `#10.owner.name.`, `x.bar:`, dangling `$Mcp.`,
  single atom (regression), non-chain expression rejected, operator/call-boundary cases
  (`foo() + $bar.baz` → base `$bar`).
- **`ChainExpressionEvaluator`** (fake `IMooWorldQueryProvider`): two-step `$foo.bar`, deep
  `#N.a.b.c`, each base form, non-object intermediate → null, missing property → null, depth-budget
  trip → null.
- **`LocalVariableResolver`** (real parsed verb buffers): nearest preceding at top level and nested in
  `if`/`for`; var-to-var (`x = y`); RHS chain (`x = $foo.bar`); cycle guard (`x = y; y = x`); RHS that
  is a verb call / function result → unknown.
- **Caching:** world-state `(objectId, propName)` cache hit avoids a second query; variable value is
  re-derived (not cached) after an edit changes the assignment; per-page instancing isolates two
  windows (same var name, different values).
- **Integration:** mirror existing `MemberCompletionController` / hover tests for the new path; hover
  parity with completion.

## Out of scope (→ follow-up bead)

- Full flow-aware lazy reassignment interpretation of `this`/`player`/`caller` and arbitrary locals for
  a member directly under the caret (not as a chain base).
- Branch-reachability / control-flow analysis of which assignment actually executes.
