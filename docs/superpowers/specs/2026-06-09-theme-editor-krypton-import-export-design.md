# Theme Editor — Krypton Conversion, Dark/Light Dropdown, Theme Import/Export

**Date:** 2026-06-09
**Bead:** udd-1p9.2 (reopened)
**Status:** Approved design

## Summary

Extend the existing `ThemeEditorDialog` (View > Theme) with three capabilities:

1. A **dark/light mode dropdown** at the top of the dialog that toggles the
   `EditorDarkTheme` setting and live-previews the Krypton client chrome
   **within the dialog only** (until Apply/OK commits it app-wide).
2. **Import/Export** of theme files in a JSON format (`.mood`) so users can
   share personal color schemes.
3. A **conversion of the dialog's WinForms controls to their Krypton
   equivalents**, which is a prerequisite for the dialog-local palette preview
   to actually re-theme the dialog surface.

> Note: the underlying setting is named `EditorDarkTheme` (there is no
> `EditorDarkMode` property — they refer to the same flag).

This is a stepping stone toward a future change that replaces the dark/light
boolean with a full Krypton-theme picker for the non-editor client chrome.

## Background

The dialog edits a `Settings.Clone()` working copy and only writes back to the
active config (`%APPDATA%\Moo Udditor\Moo.Editor.config`), the `Settings.Instance`
singleton, and open editors on Apply/OK (`ApplyTheme` →
`WindowManager.ApplyThemeToOpenEditors`). That model is unchanged here.

Today `EditorDarkTheme` only swaps `kryptonManager.GlobalPalette = kryptonPalette1`
(a `KryptonCustomPaletteBase` based on `Microsoft365BlackDarkMode`) at
application startup (`Editor_Load`). A Krypton palette only re-themes Krypton
controls; the current dialog is built from plain WinForms controls, so it would
not visibly re-theme from a local palette without the control conversion.

Krypton.Toolkit **80.23.11.321** is referenced; all required Krypton controls
(`KryptonPanel`, `KryptonButton`, `KryptonLabel`, `KryptonComboBox`,
`KryptonCheckBox`, `KryptonGroupBox`, `KryptonSplitContainer`,
`KryptonTableLayoutPanel`) are available and several are already used elsewhere
in the solution.

## 1. Control conversion (prerequisite)

Convert the dialog surface so a dialog-local Krypton palette themes it. This
touches both `ThemeEditorDialog.Designer.cs` (the static controls) and
`ThemeEditorDialog.cs` (the dynamically built left-pane controls).

| Current WinForms control | Krypton equivalent |
|---|---|
| `Panel` (leftScrollPanel, previewHostPanel, buttonPanel, new header) | `KryptonPanel` |
| `SplitContainer` | `KryptonSplitContainer` |
| `GroupBox` (Syntax, Editor chrome) | `KryptonGroupBox` |
| `TableLayoutPanel` | `KryptonTableLayoutPanel` |
| `Label` (column headers, row labels) | `KryptonLabel` |
| `ComboBox` (per-token font style, new mode dropdown) | `KryptonComboBox` |
| `CheckBox` (Transparent) | `KryptonCheckBox` |
| `Button` (OK/Apply/Cancel, new Import/Export) | `KryptonButton` |

Special cases:

- **Color swatches** remain `KryptonButton` but render as a flat color chip:
  set `StateCommon.Back.Color1` and `Color2` to the swatch color with a solid
  color draw style, so the chip always shows the true color regardless of the
  active palette, while the border still themes. The click handler (open
  `ColorDialog`, set value, refresh, refresh preview) is preserved.
- **Background-swatch inner container** (`FlowLayoutPanel` holding the swatch +
  Transparent checkbox) has no Krypton equivalent; replace it with a small
  `KryptonPanel` using anchored/manual layout (or transparent background) so the
  themed parent shows through.
- The **FastColoredTextBox preview is intentionally not Krypton-themed** — it
  keeps rendering the editor color theme via `ApplyPreviewChrome` /
  `RefreshTheme`. That is its purpose and must not change.

The existing `_refreshers` mechanism and the row-definition builders
(`BuildTokenRowDefinitions`, `BuildChromeRowDefinitions`,
`CreateForegroundSwatch`, `CreateBackgroundSwatch`, `CreateFontStyleControl`)
are retained; only the concrete control types and their color-setting calls change.

## 2. Header strip (new)

A `KryptonPanel` docked to the top of the dialog (above the split container)
containing:

- **Left:** `KryptonLabel` "Mode:" + `KryptonComboBox` (`DropDownList`) with
  items **Light** / **Dark**. Initial selection reflects
  `_working.EditorDarkTheme`.
- **Right:** `KryptonButton` **Import…** and `KryptonButton` **Export…**.

## 3. Dark/light dropdown → dialog-local palette preview (Approach B)

Changing the dropdown previews the Krypton chrome **inside the dialog only**:

- **Dark** → `ThemeEditorDialog.PaletteMode = PaletteMode.Custom` and
  `ThemeEditorDialog.Palette =` the shared dark `KryptonCustomPaletteBase`
  (`kryptonPalette1`), passed into the dialog from the main form.
- **Light** → the application's default builtin palette
  (`PaletteMode.Microsoft365Blue`).

The handler also updates `_working.EditorDarkTheme`. The rest of the application
is untouched while the dialog is open.

On **Apply/OK**: persist `EditorDarkTheme` (already covered by `SaveTo` /
`CopyFrom`) and swap `kryptonManager.GlobalPalette` to match the selected mode
(dark → `kryptonPalette1`, light → default global palette) so the whole app
updates live without a restart. On **Cancel**: leave the global palette as it
was.

Wiring: `mnuItemTheme_Click` (and the `ThemeEditorDialog` constructor) gain a
reference to the dark `KryptonCustomPaletteBase` and the `KryptonManager` so the
dialog can preview locally and commit globally.

## 4. JSON theme format + `Settings` methods

New public methods on `Settings`:

- `void ExportThemeToJson(string filePath)`
- `void ImportThemeFromJson(string filePath)`

File shape (`.mood`, JSON via `System.Text.Json`):

```json
{
  "name": "My Theme",
  "formatVersion": 1,
  "settings": {
    "KeywordColor": "#0000FF",
    "KeywordBackgroundColor": "Transparent",
    "KeywordFontStyle": "Bold;Italic",
    "EditorBackgroundColor": "#1E1E1E",
    "EditorFontFamily": "Consolas",
    "EditorFontSize": "10",
    "EditorDarkTheme": "True"
  }
}
```

**Scope** = the appearance subset only: every syntax-token foreground /
background / font-style, every editor-chrome color, `ErrorIndicatorColor`,
`EditorFontFamily`, `EditorFontSize`, and `EditorDarkTheme`. It deliberately
**excludes** editor behavior settings (word wrap, tab length, autocomplete
delay, dialect, zoom, etc.).

- **Export** builds the `settings` dictionary using the existing
  `SerializeColor` / `SerializeFontStyle` helpers (factored into a shared
  `BuildThemeDictionary()` so export and the key list stay in one place), then
  serializes the wrapper object with indentation.
- **Import** deserializes the wrapper, then applies **only the keys present** in
  the `settings` dictionary onto the target `Settings` instance. Keys absent
  from the file are left at their current value. Values that fail to parse keep
  the current value (colors via `ColorTranslator.FromHtml` in try/catch; styles
  via `ParseFontStyles(value, currentValue)`; bool via `bool.TryParse`).
  Behavior settings and any omitted appearance keys are never touched.

`formatVersion` is written and read; an unrecognized future version still
imports best-effort by key (forward-compatible).

**Atomicity / malformed files:** `ImportThemeFromJson` fully reads and
deserializes the file (and validates the wrapper shape) **before** mutating the
target instance. A malformed/garbage/unreadable file therefore throws a clear
exception *before any value is applied*, leaving the working copy completely
unchanged. The dialog (section 6) catches that exception and surfaces a clean
error message, and the user continues with their in-dialog state intact.

## 5. Import font mitigation

When the imported `settings` contains `EditorFontFamily`:

- If the named family is installed locally, apply it.
- If it is **not** installed, fall back to **generic monospace**
  (`FontFamily.GenericMonospace`), keep the imported `EditorFontSize`, and
  record that the font was unavailable so the dialog can notify the user, e.g.
  `"Theme font 'Fira Code' isn't installed; using a monospace fallback."`

Detection: attempt `new FontFamily(name)` (throws `ArgumentException` when
missing) or check against `InstalledFontCollection`. `ImportThemeFromJson`
returns enough information (e.g. the missing font name, or an out-parameter /
small result object) for the dialog to show the notice.

## 6. Import / Export flow in the dialog

- **Export…** → `SaveFileDialog` (filter `Moo theme (*.mood)|*.mood`,
  default ext `.mood`) → `_working.ExportThemeToJson(path)`; show an error
  `MessageBox` on failure (mirrors `ApplyTheme`'s error handling).
- **Import…** → `OpenFileDialog` (same filter) → `_working.ImportThemeFromJson(path)`
  inside a `try`/`catch`:
  - **On success:** **rebuild the left controls** (clear
    `leftScrollPanel.Controls` and `_refreshers`, then re-run `BuildLeftControls`
    so swatches, font-style combos, and transparent checkboxes reflect the
    imported values), reset the mode dropdown from `_working.EditorDarkTheme`,
    re-apply the dialog-local palette to match, and `RefreshPreview()`. If a
    font fallback occurred, show the notice.
  - **On failure** (malformed/unreadable file): catch the exception and show a
    clean error `MessageBox` (mirroring `ApplyTheme`'s error handling) naming the
    problem. Because import is atomic (section 4), `_working` and every control
    are unchanged, so the user dismisses the message and continues editing as if
    nothing happened.
  - Nothing touches live editors until Apply/OK (unchanged).

## 7. Testing

Add to `Org.Edgerunner.Moo.Editor.Tests/SettingsThemeTests.cs`:

- **Round-trip:** `ExportThemeToJson` then `ImportThemeFromJson` reproduces all
  theme values, including a `Transparent` background and a combined
  `Bold;Italic` style and the `EditorDarkTheme` flag.
- **Only-present-keys:** importing a file whose `settings` omits a key, and
  whose file contains no behavior keys, leaves a pre-set behavior value (e.g.
  `EditorTabLength`) and the omitted appearance key unchanged on the target.
- **Missing font:** importing `EditorFontFamily` set to a guaranteed-absent
  family falls back to generic monospace and reports the missing name; the
  imported size is still applied.
- **Malformed file:** importing garbage throws a clear exception **and leaves
  the target instance unchanged** (atomicity — set a sentinel value on the
  target first, attempt the import, assert it threw and the sentinel survived).

(UI/Krypton conversion is validated by build + manual smoke test; logic lives in
`Settings` where it is unit-testable.)

## Out of scope

- The full Krypton-theme picker that will replace the dark/light boolean for the
  client chrome (future change).
- MCP-based local edit and any non-theme settings.
- Exporting/importing editor behavior settings.

## Files touched

- `Org.Edgerunner.Moo.Editor/Configuration/Settings.cs` — `ExportThemeToJson`,
  `ImportThemeFromJson`, `BuildThemeDictionary`, font-mitigation helper.
- `Org.Edgerunner.Moo.Udditor/Dialogs/ThemeEditorDialog.cs` — control types,
  header strip, mode dropdown handler, import/export handlers, rebuild-left
  helper, local-palette preview, global commit on Apply.
- `Org.Edgerunner.Moo.Udditor/Dialogs/ThemeEditorDialog.Designer.cs` — Krypton
  control declarations and the header strip.
- `Org.Edgerunner.Moo.Udditor/Main/Editor_ViewMenu.cs` — pass the dark palette
  and `KryptonManager` into the dialog.
- `Org.Edgerunner.Moo.Editor.Tests/SettingsThemeTests.cs` — new tests.
