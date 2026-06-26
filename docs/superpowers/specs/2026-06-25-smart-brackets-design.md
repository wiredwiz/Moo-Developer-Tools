# Smart brackets enhancement

**Bead:** udd-lyt
**Date:** 2026-06-25
**Status:** Design approved
**Scope:** auto-close for `()` `[]` `{}`; auto-delete for `()` `[]` `{}` **and** `"`.

---

## Goal

Improve the editor's existing auto-bracket behavior so that:

1. Typing a bracket opener inserts the matching closer **only if the matching bracket doesn't
   already exist** on the line ahead of the caret (`()` `[]` `{}`).
2. Backspacing a bracket opener also deletes the matching closer when it is the first
   non-whitespace character ahead on the same line — **ignoring (and removing) intervening
   whitespace** (`()` `[]` `{}`).
3. Backspacing an unescaped `"` deletes its matching `"` under the same whitespace-tolerant
   rule, with escape-awareness so string content (`\"`) is never disturbed.

This is an **enhancement of existing functionality**, not a from-scratch feature.

---

## Existing behavior (baseline)

- **Auto-close on open** — `FastColoredTextBox.DoAutocompleteBrackets`
  (`FastColoredTextBox.cs:4892`), gated by `AutoCompleteBrackets`. Enabled on `MooCodeEditor`
  (`MooCodeEditor.cs:96-97`, list `() {} [] ""`) and driven by the `EditorAutoBrackets`
  config setting (`MooCodeEditorPage.cs:278`). On an opener keystroke it **always** inserts
  the pair and centers the caret; it also performs **type-over** (typing a closer when the
  next char is that closer steps over it) and **selection-wrap** (typing an opener with a
  selection wraps it). These behaviors are preserved.
- **Auto-delete on backspace** — `MooCodeEditor.MooEditor_KeyDown`
  (`MooCodeEditor.cs:458-467`). Backspacing an opener deletes the closer **only when opener
  and closer are immediately adjacent** (`(|)`), covering `()` `[]` `{}` and `""`.

---

## Refinement 1 — line-balance-aware auto-close

Add an **opt-in property** on the FCTB fork (default **off**) that changes the auto-close
decision. It is enabled **only on `MooCodeEditor`**, so the terminal (`MooClientTerminal`)
and document editor keep today's always-insert behavior.

When the user types an opener `L` (matching closer `R`):

1. Scan the current line **from the caret to end-of-line**, tracking depth: `+1` for each
   `L`, `−1` for each `R`.
2. If the depth ever goes **negative** — there is an **unmatched closer ahead** that this
   opener would pair with — insert **only `L`** (no auto-closer).
3. Otherwise, insert the `LR` pair as today.

- Applies to `()` `[]` `{}` only. **Quote (`"`) auto-close is unchanged** — a symmetric `"`
  has no well-defined opener/closer depth.
- Type-over and selection-wrap paths are untouched.

Examples (caret = `|`):

| Before | Type | After |
|---|---|---|
| `\|)` | `(` | `(\|)` — closer already ahead, none added |
| `foo\|` | `(` | `foo(\|)` — pair inserted |
| `(a, \|)` | `(` | `(a, (\|)` — `)` ahead → only `(` inserted |

---

## Refinement 2 — whitespace-tolerant auto-delete

Extend the backspace branch of `MooEditor_KeyDown` for `()` `[]` `{}`:

1. On backspace with no selection and an **opener** immediately before the caret,
2. scan **rightward from the caret on the same line, skipping spaces and tabs**,
3. if the first non-whitespace character is the **matching closer**, expand the selection to
   remove **opener + intervening whitespace + closer** in one edit.

- Applies to `()` `[]` `{}`. Quote deletion is handled separately by **Refinement 3** below
  (it needs escape-awareness, so it can't share the plain bracket scan).

Examples:

| Before | Backspace | After |
|---|---|---|
| `(\|)` | ⌫ | `\|` |
| `(\|   )` | ⌫ | `\|` — opener, whitespace, and closer all removed |
| `(\|  x)` | ⌫ | `\|  x)` — first non-ws is `x`, only opener removed (normal) |
| `x\|` | ⌫ | `\|` — non-bracket, normal backspace |

---

## Refinement 3 — escape-aware quote auto-delete

Replaces the current immediate-adjacency `"` handling in `MooEditor_KeyDown`. On backspace
with no selection and a `"` immediately before the caret:

1. **Escape check on the deleted quote.** Count the run of consecutive `\` immediately
   preceding the `"`. If the count is **odd**, the `"` is escaped string content (`\"`,
   `\\\"`, …) → do nothing special, normal backspace. If the count is **even** (including
   zero, e.g. `"`, `\\"`), the `"` is a real unescaped delimiter → proceed.
2. **Scan rightward** from the caret on the same line, skipping spaces and tabs:
   - First non-whitespace char is a `"` → it is the matching quote (it is necessarily
     unescaped — any escaping `\` would have been hit first and stopped the scan). Expand the
     selection to remove **deleted `"` + intervening whitespace + matching `"`** in one edit
     (whitespace collapsed, same as brackets).
   - First non-whitespace char is anything else → stop, normal backspace.

Escape-ness is defined by **backslash parity**, not merely "preceded by a `\`", so `\\"`
(escaped backslash + real quote) is correctly treated as a delimiter.

Examples (caret = `\|`):

| Before | Backspace | After | Why |
|---|---|---|---|
| `"\|"` | ⌫ | `\|` | adjacent matching quote removed |
| `"\|   "` | ⌫ | `\|` | quote + whitespace + quote all removed (collapses a spaces-only string) |
| `"\| hello "` | ⌫ | `\| hello "` | first non-ws is `h` → only the deleted quote removed |
| `\\"\|"` | ⌫ | `\\\|` | deleted `"` is unescaped (even `\` run) → matching quote removed, `\\` kept |
| `\"\|` | ⌫ | `\\|` | deleted `"` is escaped content (odd `\` run) → normal backspace, `\` kept |

Note: per "same for all," a spaces-only string `"   "` collapses fully on opening-quote
backspace — accepted, consistent with bracket whitespace removal.

---

## Tests & verification

- Auto-close: opener with no unmatched closer ahead → pair inserted; opener with an unmatched
  closer ahead on the line → only the opener inserted; type-over and selection-wrap still work.
- Bracket delete: adjacent closer → both removed; whitespace-then-closer → opener +
  whitespace + closer removed; non-whitespace before the closer → only opener removed;
  non-bracket before caret → normal backspace.
- Quote delete: adjacent matching `"` removed; whitespace-then-`"` → both + whitespace
  removed; non-ws content before the `"` → only the deleted quote removed; escaped `\"`
  (odd backslash run) → normal backspace; `\\"` (even run) → treated as delimiter.
- Quotes: unaffected by line-balance auto-close suppression (Refinement 1 stays
  `()` `[]` `{}`-only).
- Refinement 1 is off for the terminal and document editors.
- Editor test suite green; app builds clean.

---

## Decisions

- **Auto-close** suppression applies to `()` `[]` `{}` only; quote auto-close is unchanged
  (a symmetric `"` can't be depth-counted for line balance).
- **Auto-delete** applies to `()` `[]` `{}` (Refinement 2) **and** `"` (Refinement 3).
- Auto-close suppression uses **caret-to-EOL line balance** (an unmatched closer ahead),
  implemented as an opt-in FCTB property enabled only on `MooCodeEditor`.
- Whitespace-tolerant delete removes the **intervening whitespace too**, not just the pair —
  for brackets and quotes alike (a spaces-only string `"   "` collapses fully; accepted).
- Quote escape detection uses **backslash parity** (escaped iff an odd run of `\` precedes
  the `"`), so `\\"` is correctly a delimiter.
