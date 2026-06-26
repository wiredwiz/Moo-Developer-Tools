# Smart brackets enhancement

**Bead:** udd-lyt
**Date:** 2026-06-25
**Status:** Design approved
**Scope:** bracket pairs `()` `[]` `{}` only (quotes intentionally excluded).

---

## Goal

Improve the editor's existing auto-bracket behavior so that:

1. Typing an opener inserts the matching closer **only if the matching bracket doesn't
   already exist** on the line ahead of the caret.
2. Backspacing an opener also deletes the matching closer when it is the first non-whitespace
   character ahead on the same line — **ignoring (and removing) intervening whitespace**.

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

- Applies to `()` `[]` `{}` only. **Quotes keep immediate-adjacency deletion** — whitespace
  tolerance on a symmetric `"` could corrupt adjacent strings (`"a"   "b"`: the caret after
  the close quote of `"a"` could treat the open quote of `"b"` as its match). Note that
  string *content* is never at risk — `(" hello ")` is safe because the first non-whitespace
  char ahead is `h`, not `"`, which stops the scan.

Examples:

| Before | Backspace | After |
|---|---|---|
| `(\|)` | ⌫ | `\|` |
| `(\|   )` | ⌫ | `\|` — opener, whitespace, and closer all removed |
| `(\|  x)` | ⌫ | `\|  x)` — first non-ws is `x`, only opener removed (normal) |
| `x\|` | ⌫ | `\|` — non-bracket, normal backspace |

---

## Tests & verification

- Auto-close: opener with no unmatched closer ahead → pair inserted; opener with an unmatched
  closer ahead on the line → only the opener inserted; type-over and selection-wrap still work.
- Delete: adjacent closer → both removed; whitespace-then-closer → opener + whitespace +
  closer removed; non-whitespace before the closer → only opener removed; non-bracket before
  caret → normal backspace.
- Quotes: unaffected by line-balance suppression and by whitespace-tolerant deletion.
- Refinement 1 is off for the terminal and document editors.
- Editor test suite green; app builds clean.

---

## Decisions

- Scope limited to `()` `[]` `{}`; quotes excluded from both refinements (kept at current
  behavior).
- Auto-close suppression uses **caret-to-EOL line balance** (an unmatched closer ahead),
  implemented as an opt-in FCTB property enabled only on `MooCodeEditor`.
- Whitespace-tolerant delete removes the **intervening whitespace too**, not just the two
  brackets.
