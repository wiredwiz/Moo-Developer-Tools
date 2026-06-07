# Theme Editor Dialog (View ▸ Theme) — Design

**Issue:** udd-1p9.2 (child of epic udd-1p9 "Code Editor Enhancements")
**Sibling:** udd-1p9.1 (Editor Options dialog) — shares the new config-save infrastructure.
**Date:** 2026-06-07
**Status:** Approved design, ready for implementation plan.

---

## 1. Goal

Add a **View ▸ Theme…** menu item that opens a modal dialog letting the user design
the editor color theme through a GUI — every color/font-style setting in
`Moo.Editor.config` — with a **live, isolated preview**, instead of hand-editing the
config file. On Apply/OK the theme is persisted and applied immediately to all open
editors.

## 2. Scope (decided)

- **In scope:** all *color* settings in `Settings` — both syntax-token colors and editor
  chrome colors — plus per-token **font style** (Regular/Bold/Italic).
  - Syntax tokens (each: foreground color + background color + font style):
    Default, Keyword, Comment, Literal, String, Symbol, Operator, Parenthesis,
    Bracket, CurlyBrace, Object, CoreReference, BuiltinVariable, BuiltinFunction,
    Verb, Property.
  - Editor chrome (color swatches): EditorBackgroundColor, EditorTextColor,
    EditorCaretColor, EditorLineNumberColor, EditorCurrentLineColor,
    EditorTextSelectionColor, EditorChangedLineColor, EditorFoldingIndicatorColor,
    EditorFoldingHighlightColor, EditorIndentBackColor, EditorBookmarkColor,
    EditorServiceLineColor, ErrorIndicatorColor.
- **Out of scope:**
  - Behavior settings (word wrap, tab length, autocomplete delay, dialect, font
    family/size, zoom, folding toggles) — these belong to the Options dialog (udd-1p9.1).
  - Named theme presets / Save-As / import-export / a "Reset to defaults" preset
    picker. This dialog edits the **single active theme** only.

## 3. Current-state findings (why the design is shaped this way)

1. **No save path exists.** `Settings` (singleton `Settings.Instance`) only *loads*
   (`LoadFrom`/`LoadDefaults`). Nothing writes the config back. A `Settings.SaveTo(path)`
   must be added — and it is **shared infrastructure** the sibling Options dialog needs too.
2. **Highlighting is hard-wired to the singleton.** Each `MooCodeEditor` constructs its own
   `MooSyntaxHighlightingGuide`, which reads `Settings.Instance` directly. To preview pending
   edits in isolation, the guide must be bindable to an alternate `Settings` source.
3. **`StyleRegistry` caches computed styles** (`_TokenStyles`, `_UniqueStyles`) and has no
   `Clear()`. Re-theming a live editor therefore requires clearing that cache and re-colorizing.
4. **Chrome colors are applied in exactly one place:**
   `MooCodeEditorPage.ConfigureEditorSettings(MooCodeEditor)` (line ~219) maps
   `Settings.Instance` → editor properties (chrome colors, fonts, behavior, autocomplete).
   It runs once at editor creation; there is no "re-apply after change" path today.
5. The active config path is resolved via
   `ApplicationPaths.ResolveDataFile("Moo.Editor.config")` (now under `%APPDATA%\Moo Udditor`),
   the same call used at startup in `Program.cs`.

## 4. Architecture approach — isolated working copy

The dialog edits a **clone** of `Settings.Instance` (the "working theme") and never mutates
the live singleton until Apply/OK. The preview editor is bound to that clone; open editors are
untouched until the user commits.

**Chosen (A): inject the settings source.**
Give `MooSyntaxHighlightingGuide` an optional `Settings` source (`?? Settings.Instance`) and
bind the preview `MooCodeEditor`'s guide to the working copy. Full isolation; Cancel just
discards the clone.

**Rejected (B): mutate the singleton live, restore on Cancel.**
Simpler wiring, but re-colorizes every open editor on each color click (flicker/cost) and risks
leaving editors dirty if the dialog is killed mid-edit.

## 5. Components & changes

| Component | Change |
|---|---|
| `Settings` (`Org.Edgerunner.Moo.Editor/Configuration/Settings.cs`) | Add `Settings Clone()` (deep copy of all public properties to a new instance). Add `void SaveTo(string path)` that writes the `appSettings` XML using the exact existing keys, colors serialized to hex/name form the loader accepts, font styles via the `;`-joined form `ParseFontStyles` reads. Shared with udd-1p9.1. |
| `MooSyntaxHighlightingGuide` | Add optional ctor param `Settings source = null`; store `_settings = source ?? Settings.Instance`; replace all `Settings.Instance.*` reads with `_settings.*`. Default behavior unchanged for existing callers. |
| `StyleRegistry` (`IStyleRegistry`) | Add `void Clear()` that empties `_TokenStyles` and `_UniqueStyles` (and resets `_ErrorStyle`), so a re-theme recomputes styles. |
| `MooCodeEditor` (`Org.Edgerunner.Moo.Editor/Controls`) | Allow supplying a guide/registry bound to an alternate `Settings` (ctor overload or settable property used by the preview). Add public `void RefreshTheme()` = `StyleRegistry.Clear()` + `ClearAllStyles()` + `ColorizeTokens(null)`. |
| `MooCodeEditorPage` | Refactor `ConfigureEditorSettings(MooCodeEditor)` into `ApplyEditorSettings(MooCodeEditor, Settings source)` (default `Settings.Instance`) so the same mapping serves real editors, open-editor refresh, and the preview. Existing call site updated. |
| `WindowManager` | Add a way to enumerate open `MooCodeEditorPage`s (and the `ParserMessageDisplayPage`) so Apply can refresh them. |
| `ThemeEditorDialog` (new — `Org.Edgerunner.Moo.Udditor/Dialogs/ThemeEditorDialog.cs` + `.Designer.cs`) | The dialog. |
| `ThemePreviewSample.moo` (new `EmbeddedResource`) | Predefined Moo sample exercising every token category; loaded at runtime into the read-only preview. |
| `Editor_ViewMenu.cs` + `Editor_MenuConfiguration.cs` | New "Theme…" menu item + click handler that opens `ThemeEditorDialog`. |

## 6. Dialog UI

Modal dialog, horizontal `SplitContainer`:

- **Left pane (scrollable), grouped blocks:**
  - **Syntax** group: one row per token category. Row = label + foreground swatch +
    background swatch + font-style control (Regular/Bold/Italic). Clicking a swatch opens the
    standard `ColorDialog`; the swatch repaints with the chosen color.
  - **Editor chrome** group: one labeled color swatch per chrome color listed in §2.
- **Right pane:** read-only `MooCodeEditor` showing the embedded sample, guide bound to the
  working copy. Read-only = not user-editable; used purely for rendering.
- **Buttons:** OK (apply + close), Apply (apply, keep open), Cancel (discard + close).

## 7. Data flow

1. **Open:** `working = Settings.Instance.Clone()`. Build left controls from `working`.
   Construct preview editor with a guide bound to `working`; `ApplyEditorSettings(preview, working)`;
   load embedded sample; colorize.
2. **Edit:** swatch/style change writes into `working`, then
   `preview.RefreshTheme()` + `ApplyEditorSettings(preview, working)` (chrome). Isolated & live.
3. **Apply / OK:** `working.SaveTo(activeConfigPath)` → copy `working` into `Settings.Instance`
   → for each open `MooCodeEditorPage`: `ApplyEditorSettings(editor, Settings.Instance)` +
   `editor.RefreshTheme()`; refresh `ParserMessageDisplayPage` fore/back colors. OK then closes.
4. **Cancel:** discard `working`; nothing persisted, no editors touched.

## 8. Error handling

- `SaveTo` wraps file IO in try/catch; on failure shows a message box and leaves the in-memory
  `Settings.Instance` unchanged (no partial apply to editors).
- `ColorDialog` ARGB stored through the same hex/`ColorTranslator` form the loader accepts.
- **Transparent caveat:** the standard `ColorDialog` cannot express `Transparent` (alpha 0),
  yet every background swatch defaults to `Transparent`. Background swatches therefore need a
  small "Transparent" affordance (a checkbox or a clear/none button beside the swatch) so the
  user can both reach `ColorDialog` for an opaque color and restore `Transparent`. Foreground
  and chrome swatches use `ColorDialog` directly.
- Loader is already defensive (each setting try/catch with defaults), so a malformed write
  degrades gracefully on next load.

## 9. Testing

- **Unit:** `Settings.Clone()` round-trips every property; `SaveTo` → `LoadFrom` round-trips all
  values (colors incl. `Transparent`, font styles incl. combined `Bold;Italic`).
- **Manual:** open dialog → change a token color and a chrome color → preview updates immediately
  while open editors stay unchanged; Apply → open editors and parser-message panel re-theme live;
  Cancel → no change; restart → persisted theme loads from `%APPDATA%\Moo Udditor\Moo.Editor.config`.

## 10. Implementation notes

- Per project CLAUDE.md: implement in a git worktree via subagents; commit and clean up.
- The `Settings.SaveTo` + `Settings.Clone` + `ApplyEditorSettings(source)` refactor is deliberately
  reusable so udd-1p9.1 (Options dialog) can build on it rather than duplicate it.
