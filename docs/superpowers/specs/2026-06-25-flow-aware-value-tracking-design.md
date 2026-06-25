# Flow-aware lazy reassignment tracking (udd-2s4)

## Overview

Resolve the **current value** of a variable at the caret by interpreting the verb's
statements from verb-start to the caret, honoring sequential reassignment. This upgrades
autocomplete **and** hover so that a variable that has been reassigned earlier resolves to
its new value:

```moo
player = #23;
...
player:        // completes verbs on #23, not the connected player
```

It applies to the built-in object variables `this` / `player` / `caller` **and** arbitrary
local variables, used either as a chain base (`x.foo:`) or directly under the caret
(`x:`, `player.`), in both completion and hover.

Builds on udd-efk (`ChainExtractor` / `ChainExpressionEvaluator` / `LocalVariableResolver`)
and udd-3y3 (bare-operand hover). Supersedes `LocalVariableResolver.ResolveAssignmentChain`
(textually-nearest) with a flow-aware resolver.

## Scope

**In scope**

- Sequential, **linear pre-caret** value tracking for `this`/`player`/`caller` and locals.
- Branch conservatism: an assignment inside a branch the caret is **not** in makes the value
  ambiguous (it might not have run).
- Loop conservatism: when the caret is inside a loop body, any assignment in that body other than a
  sole dominating pre-caret assignment — including one textually after the caret (the back-edge ran
  it on a prior iteration) — makes the value ambiguous (see "Loops, precisely" below).
- **Offset-correct nested snapshots**: an assignment's right-hand side is reduced as of *that
  assignment's* position, not the caret's. `x = player; player = #5; x:` resolves `x` to the
  player value at the `x = player` line (the default player), **not** `#5`.
- Bare-local **hover** (deferred to here by udd-3y3): hovering a bare local shows the object it
  currently resolves to.
- Wording: the hover "no source" label becomes **"(unknown source)"**.

**Ambiguity fallback**

- A `this`/`player`/`caller` that is ambiguous (or unmodified) falls back to its **default**
  (`this` → `ContextObjectId`; `player`/`caller` → `GetCurrentPlayerAsync`).
- A local that is ambiguous or unassigned is **unknown** → no source.

**Out of scope (deferred)**

- Loop **fixpoint** analysis (proving every iteration converges to the same value); we only
  decline (ambiguous) rather than prove convergence.
- Scatter / multiple-assignment targets (`{a, b} = expr;`, `a = b = expr;`) — recognized as
  *assigned* (so they can poison), but their value is treated as unknown for now.
- Cross-verb / suspend effects; MOO locals are per-frame, so this does not arise.

## Resolution semantics

For a queried name, walk every assignment to it that precedes the caret, in document order,
tracking one of three states — `Unmodified` → `Reassigned(chain @ offset)` → `Ambiguous`:

| Assignment relative to the caret | Classification | Effect on state |
|---|---|---|
| In a block that **encloses** the caret (top-level, or an `if`/`for`/`while`/`try` body the caret sits inside), **before** the caret | **Definite** | `Reassigned(rhs @ assignmentOffset)` — overrides any prior state, including `Ambiguous` |
| In a branch the caret is **not** in | **Conditional** | poisons to `Ambiguous` (unless a later **Definite** assignment overrides) |
| Anywhere in a **loop body enclosing the caret** other than a sole dominating pre-caret Definite assignment (so: conditional within the loop, or textually **after** the caret via the back-edge) | **Conditional** (back-edge) | poisons to `Ambiguous` |

**Loops, precisely.** The caret being inside a loop body means a prior iteration ran the *whole*
body. The conservative rule: the value is `Definite` only when a single pre-caret path assignment
textually dominates the caret **and** `name` has no other assignment anywhere in that loop body;
otherwise it is `Ambiguous`. This is always safe (never wrong), but it over-declines one shape —
`while(c) player = #10; player:; player = #20; endwhile`, where `player` is in fact always `#10` at
the caret (the pre-caret assignment re-runs each iteration) — which falls back to default. Proving
that dominating case is **loop-fixpoint future work** (see below).

At the caret:

- `Reassigned(chain @ O)` → the value is `chain`, with any variable in `chain`'s base reduced as
  of offset `O` (see "offset-correct reduction").
- `Unmodified` → keyword: **UseDefault**; local: **Unknown**.
- `Ambiguous` → keyword: **UseDefault**; local: **Unknown**.

A Definite assignment whose RHS is **not** a resolvable chain (e.g. `x = foo()`, `x = 1 + 2`,
a list/string literal) yields **Unknown** for that name (we know it is not a resolvable object).

### Offset-correct reduction

When a Definite assignment `x = <rhs>` at offset `O` is found, `<rhs>` is converted to a chain
(`ChainExtractor.DescribeExpression`) and its **base** is reduced *recursively as of `O`*:

- base `#N` / `$name` → already ground.
- base `this`/`player`/`caller`/local → recurse: `ResolveCurrentValue(thatName, tree, O)` (note the
  offset is `O`, the assignment's position, not the caret).

The recursion accumulates property steps and terminates in a **ground chain** whose base is one of
`#N` (`ObjectLiteral`), `$name` (`CoreName`), or a keyword (`This`/`Player`/`Caller`) carrying
**default** meaning. Guards: a cycle set (names currently being reduced) and a depth budget,
mirroring `ChainExpressionEvaluator`'s existing `MaxResolutionSteps`.

Because reduction is done by the (pure, synchronous) resolver, the async evaluator never has to
thread offsets — it receives a fully reduced ground chain.

### Worked examples

```moo
player = #10;                 // Definite     -> player = #10
if (foo) player = #20; endif  // Conditional  -> Ambiguous
player:                       // Ambiguous keyword -> default player
```
```moo
if (foo) player = #20; endif  // Conditional  -> Ambiguous
player = #10;                 // Definite     -> player = #10  (override)
player:                       // -> #10
```
```moo
if (foo)
   player = #20;              // caret's enclosing branch -> Definite
   player:                    // -> #20
endif
```
```moo
x = player;                   // O1: Definite x = (player reduced @ O1 -> default player)
player = #5;                  // O2
x:                            // -> default player  (NOT #5; player=#5 is at O2 > O1)
```
```moo
pack = $mcp.package;          // Definite pack = ground chain { base $mcp, steps [package] }
pack:                         // ground.steps [package] ++ pack-chain.steps [] -> complete verbs on
                              // the object that $mcp -> #N, .package -> #M resolves to (as udd-efk)
```
```moo
while (cond)
   player:                    // iter 2+ would see #5 -> Ambiguous -> default player
   player = #5;               // back-edge: poisons player
endwhile
```

## Components

### 1. `FlowValueResolver` (new, `Org.Edgerunner.Moo.Editor/Autocomplete`)

Pure, synchronous, stateless tree walk. No world/provider access.

```csharp
public enum FlowValueKind { Reassigned, UseDefault, Unknown }

// Chain is non-null only when Kind == Reassigned; it is a GROUND chain
// (base in { ObjectLiteral, CoreName, This, Player, Caller }, no Variable base).
public readonly record struct FlowValue(FlowValueKind Kind, ChainDescriptor? Chain);

public static class FlowValueResolver
{
   public static FlowValue ResolveCurrentValue(
      string name, ParserRuleContext? tree, int caretOffset,
      Action<string, Exception?>? diagnostic = null);
}
```

- `name` is `"this"`/`"player"`/`"caller"` or a local identifier; keyword-ness is decided by the
  name (matching the existing lowercase keywords).
- Algorithm: collect assignments to `name` (top-level and nested), classify each as
  Definite / Conditional per the table above (needs the caret's ancestor-block chain and each
  assignment's enclosing-block chain; loop bodies enclosing the caret contribute their *whole*
  body as conditional-on-back-edge), fold in document order into the three-state machine, then on
  a Definite winner perform offset-correct reduction.
- Guards: cycle set + depth budget. Unexpected exceptions route to `diagnostic`; expected
  non-resolution is silent (consistent with udd-efk logging policy).

`LocalVariableResolver.ResolveAssignmentChain` is **replaced** by this resolver as the evaluator's
value source. `LocalVariableResolver` is either removed or reduced to the assignment-finding helper
that `FlowValueResolver` builds on (decided during implementation; its tests migrate to
`FlowValueResolver`).

### 2. `ChainExpressionEvaluator` changes

The evaluator consults the flow resolver **once** for a top-level base of kind
`This`/`Player`/`Caller`/`Variable`, then resolves the resulting **ground** chain (which has no
variable base) with keyword bases meaning *default*:

- Delegate change: replace `Func<string, ChainDescriptor?> resolveVariableChain` with
  `Func<string, FlowValue> resolveFlowValue` (bound by the caller to `tree` + `caretOffset`).
- `EvaluateAsync(chain, contextObjectId, ct)`:
  - base ∈ {This, Player, Caller, Variable}: `fv = resolveFlowValue(name)`:
    - `Reassigned(ground)` → resolve `ground.Base` + (`ground.Steps` ++ `chain.Steps`) as a ground
      chain.
    - `UseDefault` → resolve `chain` as a ground chain (keyword base → default).
    - `Unknown` → `null` (no source).
  - base ∈ {ObjectLiteral, CoreName} → resolve `chain` as a ground chain (unchanged behavior).
- New private `ResolveGroundAsync`: `ObjectLiteral` → `#N`; `CoreName` → property `name` on `#0`;
  `This`/`Player`/`Caller` → **default** (`contextObjectId` / `getCurrentPlayer`) with **no** flow
  re-entry; then walk steps via the existing per-step `resolvePropertyObject`. Variable bases never
  occur here (the resolver guarantees full reduction).

This removes the evaluator's own variable recursion; all variable/offset logic now lives in the
pure resolver. Existing `ChainExpressionEvaluatorTests` are updated to the new delegate.

### 3. Completion wiring (`MemberCompletionController`)

- The async resolve path builds the evaluator with
  `resolveFlowValue = name => FlowValueResolver.ResolveCurrentValue(name, tree, caretOffset, diag)`.
- The synchronous cache fast-path (`TryResolveFromCache` / `TryResolveBaseFromCache`) is updated to
  reduce the base through `FlowValueResolver` (synchronous — fits the fast path), then walk
  `ground.Steps ++ descriptor.Steps` against the property-object cache. `UseDefault` keyword →
  the existing player/context cache logic; `Unknown` → determined no-source.
- In-flight dedup (`CanonicalChain`) keys on the original descriptor and is unaffected (reduction is
  deterministic).

### 4. Hover wiring (`MooCodeEditorPage` + `MooCodeEditor`)

- The hover page builds the same flow-aware evaluator, so `player:foo` / `x.foo:` member hovers and
  bare `player` / `this` / `caller` operand hovers all become reassignment-aware automatically.
- **Bare-local hover** (deferred here from udd-3y3): `ClassifyHoveredMember` gains a branch for a
  bare IDENTIFIER that is **not** a member, **not** `this`/`player`/`caller` (handled already), and
  **not** immediately followed by `(` (a builtin — that branch keeps priority). Such a token yields
  an `Object`-kind hover with a `Variable`-base chain; the page resolves it through the flow
  evaluator. A local that reduces to an object shows `=> #N ("name")`; a local that is `Unknown`
  shows **no** tooltip (a non-object local is normal and should not flash an error).
- Branch order in `ClassifyHoveredMember`: member (preceded by `:`/`.`) → bare keyword/`#N`
  (`ClassifyBareResolvableOperand`) → builtin function (`next == "("`) → **bare local** → none.

### 5. Wording

The single literal `"(no discernable source)"` in `MooCodeEditor.MooEditor_ToolTipNeeded` becomes
`"(unknown source)"`. (This is the only place the label is shown; flow-`Unknown` member/operand
hovers return `null` and show nothing, as today.)

## Error handling & logging

Every failure mode degrades to no-source / default — never a dialog or thrown error. Expected
non-resolution (ambiguous, unknown, non-chain RHS, cycle, depth budget) is silent. Unexpected
exceptions during the tree walk route through the injected `diagnostic` delegate →
`Logger.Warn` / `HoverDiagnostic`, consistent with udd-efk.

## Testing

- **`FlowValueResolver`** (pure, on real parsed verb buffers — the highest-value unit):
  Definite path assignment; conditional poison; later-Definite override; caret-inside-branch
  Definite; keyword ambiguous → `UseDefault`; local ambiguous/unassigned → `Unknown`; non-chain RHS
  → `Unknown`; **offset-correct nested snapshot** (`x = player; player = #5; x:` → default player);
  **local assigned to a multi-step chain** (`pack = $mcp.package; pack:` → ground chain
  `{ base $mcp, steps [package] }`, steps merged onto the original); multi-hop reduction
  (`a = b; b = #5; a:` honoring offsets); loop back-edge poison; cycle guard (`x = y; y = x`);
  depth budget.
- **`ChainExpressionEvaluator`** (fake provider): updated for the new delegate; keyword
  `UseDefault` falls back to default; `Reassigned` resolves the ground chain with merged steps;
  `Unknown` → null.
- **`MemberCompletionController`**: the sync cache fast-path and async path agree with the resolver;
  a reassigned `player` completes on the new object; `pack = $mcp.package; pack:` completes verbs on
  the same object `$mcp.package:` would (local-indirection end-to-end); cache hit avoids a re-query.
- **Hover/classification**: bare-local classification ordering (function vs variable), bare-local
  resolves to an object, `Unknown` local shows no tooltip; the `"(unknown source)"` literal.

## Future work (separate beads)

- Loop fixpoint: when every iteration provably assigns the same value, resolve it instead of
  declining.
- Scatter / chained assignment value tracking.
