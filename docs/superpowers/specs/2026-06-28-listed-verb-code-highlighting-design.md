# Syntax-highlight verb code listed from the MOO (terminal)

**Bead:** udd-z5p
**Date:** 2026-06-28
**Status:** Design approved

---

## Goal

When the option is enabled, detect verb-code listings the MOO prints to the terminal (e.g.
`@list obj:verb`), strip the MOO's ANSI coloring, and re-display the code using the editor's
Moo syntax-highlighting color scheme — so listed code in the terminal looks like code in the
editor. Off by default; terminal output is otherwise untouched.

---

## Decisions (settled during brainstorming)

- **Highlighting approach: reuse the editor's existing token+neighbor coloring** (option A) —
  `MooSyntaxHighlightingGuide.GetTokenForegroundColor(token, prev, next)`. No parse tree. The
  editor itself highlights this way (the ANTLR parse is only for error squiggles + folding),
  so this makes the terminal match the editor exactly. This is sufficient because a verb name
  is a single word (so the `:`-neighbor rule is sound) and a dynamic call `obj:(expr)`
  correctly does not match a verb (after `:` comes `(`, not an identifier; the variable inside
  is a plain identifier colored as itself).
- **No blank lines occur inside listed code** (confirmed), so a blank line is a valid
  terminator signal.
- **Line numbers are usually present but optional**, so they cannot be required — both numbered
  and unnumbered listings must be handled.
- **There is no consistent terminator**, so the unnumbered case ends on an inter-line timing
  gap.

---

## Setting

New `bool` `EditorHighlightListedVerbCode` on **Options → Code Features**, default **off**.
Wired through `Settings` (property + SaveTo/Load/Clone/CopyFrom) like the other editor
toggles. When off, the interceptor is bypassed and terminal output is unchanged.

---

## Component & integration point

A new stateful component, `ListedCodeHighlighter`, lives on the **plain-display path** (verb
listings are plain text, **not** OOB `#$#` lines). In `MooClientTerminal`, where a received
display line is currently written via `consoleSim.WriteAnsi(line)`, wrap it:

```
if (Settings.Instance.EditorHighlightListedVerbCode && _listedCode.TryHandle(line, DateTime.UtcNow))
   return;                     // consumed (rendered as highlighted code, or buffered)
consoleSim.WriteAnsi(line);    // normal path
```

It is **not** an `IOutOfBandMessageHandler`. It sees each display line and its arrival time.

---

## State machine (per display line; ANSI stripped before matching)

1. **Idle** — test the line against the verb-list **header** regex:
   `#<id>:<verb name, optionally quoted>  <indirect object> <prepstr> <direct object>
   [<owner name> (#<owner number>), <flags>]`.
   - No match → return `false` (normal display).
   - Match → write the header through unstyled (`WriteAnsi`), enter **Pending-first**.
2. **Pending-first** — inspect the first line after the header:
   - matches `^\d+:` → **Numbered**.
   - otherwise → **Unnumbered**.
3. **Numbered** — each line whose ANSI-stripped text matches `^\d+:` is code:
   - peel the `N:` prefix (write it plainly, preserving the visible line numbers),
   - highlight the remainder, write styled.
   - **Terminate** at the first line **without** a numeric prefix → that line is passed
     through normally and state returns to Idle. Deterministic; no timing.
4. **Unnumbered** — each line is treated as code: highlight + write styled immediately.
   - **Terminate** when the next line arrives **≥ 500 ms** after the previous captured line
     (compare arrival timestamps), **or** when a blank line arrives (code has none) → the
     terminating line is passed through normally and state returns to Idle.
   - No buffering/latency for the code itself — each captured line renders immediately; the
     gap check is evaluated when the next line arrives. (Risk: a non-code line arriving
     < 500 ms after the last code line would be wrongly highlighted; accepted — code streams
     contiguously and following output reliably pauses.)

A new header line seen mid-capture also ends the current block and starts a new one
(multi-verb listings).

---

## Highlighting helper (shared rule logic)

Factor the color decision into a reusable helper so the rules are not duplicated between
editor and terminal — e.g. `MooCodeColorizer.GetColoredSegments(string code,
GrammarDialect dialect) → IReadOnlyList<(string Text, Color Color)>`:

1. Strip ANSI (`Regex.Replace(text, @"\e\[(\d+;)*\d+;*m", "")` — the same pattern
   `ConsoleWindowEmulator` already uses).
2. Lex via `Moo.GetLexer(dialect, new AntlrInputStream(code))` with the **`DetailedTokenFactory`**
   (the same factory the editor uses, so tokens are `DetailedToken`s).
3. Build the `DetailedToken` list (significant tokens), pairing each with its previous/next
   exactly as `EditorSyntaxHighlighter.ColorizeTokens` does.
4. Color each token via `MooSyntaxHighlightingGuide.GetTokenForegroundColor(tok, prev, next)`
   (and font style via `GetTokenFontStyle` if applied in the editor).
5. Emit ordered segments covering the line; characters between tokens are emitted with the
   default foreground.

The terminal writes each segment through `consoleSim` styled (`AppendText(text, Style)` /
`Write(text, Style)`), building the `Style` from the color via the console's existing style
machinery (e.g. `AnsiManager.GetStyle(color, background, fontStyle)`), then a newline.

`session.GrammarDialect` selects the lexer dialect.

---

## Scope

### In scope
- The `EditorHighlightListedVerbCode` setting + Code Features toggle.
- `ListedCodeHighlighter` state machine on the plain-display path.
- The reusable `MooCodeColorizer.GetColoredSegments` helper (option A coloring).
- ANSI stripping before match + before highlight.

### Out of scope
- Parse-tree-based classification (option B) and any change to the editor's own highlighter.
- Triggering on commands other than the standard verb-list header (e.g. `@show`/`@dump`),
  unless they emit the identical header.
- Server-side changes.

---

## Edge cases

- ANSI codes are stripped before both header matching and highlighting, so embedded color
  never breaks detection.
- Numbered listings whose numbering does not start at 1 still work — Numbered mode keys off a
  `^\d+:` prefix on the first post-header line, not specifically `1:`.
- Mid-capture header → finalize current block, start new (multi-verb `@list`).
- Disconnect / console clear → reset capture state.
- Feature off → component bypassed entirely.

---

## Testing

Unit-testable (state machine + helper; the actual FCTB console rendering is not):
- Header regex: matches standard form incl. quoted verb names, alias lists, and flag/owner
  variants; rejects ordinary lines.
- Numbered termination: capture continues across `N:` lines, ends at the first non-`N:` line.
- Unnumbered termination: with injected timestamps, a ≥ 500 ms gap ends capture; a blank line
  ends capture; contiguous (< 500 ms) lines keep capturing.
- ANSI stripped before matching and before lexing.
- `MooCodeColorizer.GetColoredSegments` returns editor-matching colors for `obj:verb`
  (verb), `func(` (builtin), `.prop` (property), builtin variables (`this`, `NUM`, …),
  and `obj:(expr)` (no verb color; variable colored as a plain identifier).

Verification: full solution builds clean; all suites green.
