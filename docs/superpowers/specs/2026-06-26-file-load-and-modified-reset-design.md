# File ▸ Load + modified-line gutter reset

**Bead:** udd-yrk
**Date:** 2026-06-26
**Status:** Design approved

---

## Goal

1. Add a **File ▸ Load** item (after Open) that replaces the *current* code editor's
   contents with a file the user picks — guarded against discarding unsaved changes, and
   grayed out when no code editor is active.
2. **Reset the modified-line gutter** (so nothing shows as modified) after:
   - saving a plain `.moo` file that is **not** bound to a session verb,
   - **uploading** from a session-bound verb window,
   - **loading** a file via the new Load item.

---

## Mechanism (verified)

The gutter's modified-line bar is FastColoredTextBox's per-line `Line.IsChanged`, painted with
`ChangedLineColor` (`FastColoredTextBox.cs:5395-5396`). The editor's `IsChanged` setter
(`FastColoredTextBox.cs:622-633`) calls `lines.ClearIsChanged()` whenever set to `false`,
clearing every line's flag. `ChangedLineColor` comes from `Settings.EditorChangedLineColor`
(default `Yellow`).

**Confirmed gap:** `MooEditorPage.UploadSource()` (`MooEditorPage.cs:124-132`) sets
`SourceEditor.IsChanged = false` on a successful upload but never calls `Invalidate()`, so the
cleared flags are not repainted and the yellow bars linger. The local-save handler
(`mnuItemSave_Click`) already calls `Invalidate()`; routing every reset through one helper makes
the clear-and-repaint pairing uniform and guaranteed.

---

## Part A — File ▸ Load

- **Menu:** new `ToolStripMenuItem` (e.g. `mnuItemLoadFile`) inserted directly after
  `mnuItemOpenFile` in the File menu's `DropDownItems` (`Editor.Designer.cs`), with its own
  field declaration. Text "Load…".
- **Enable state:** enabled only when `CurrentPage is MooCodeEditorPage`. Set in
  `Editor_MenuConfiguration.UpdateMenus()` next to the existing `isMooCodeEditor` logic; grayed
  out otherwise. (`UpdateMenus` already runs on page activation.)
- **Handler** (`Editor_FileMenu.cs`, after `mnuItemOpenFile_Click`):
  1. Only act when `CurrentPage is MooCodeEditorPage page`.
  2. Show an `OpenFileDialog` with the **same filter as Open**
     (`Moo files (*.moo)|*.moo|Text files (*.txt)|*.txt|Markdown files (*.md)|*.md|All files (*.*)|*.*`),
     `Multiselect = false`, title e.g. "Select a file to load into the current editor".
  3. On `DialogResult.OK`:
     - **Unsaved-changes guard:** if `page.SourceEditor.IsChanged`, show a Yes/No
       `MessageBox` ("Discard unsaved changes and load the selected file?"). Abort on No.
     - **Replace** the buffer: `page.SourceEditor.Text = File.ReadAllText(path)`.
     - **Clean baseline:** call `page.MarkAsUnmodified()` so the modified gutter starts clear.
  4. Wrap file I/O in try/catch; on failure show an error `MessageBox` (mirroring
     `TrySaveToFile`'s error handling) and do not change the buffer.

Load replaces the buffer in place (distinct from Open, which creates a new page).

---

## Part B — Shared reset + the three call sites

- **Helper:** add to `MooEditorPage`:
  ```csharp
  public void MarkAsUnmodified()
  {
     SourceEditor.IsChanged = false;   // clears every line.IsChanged
     SourceEditor.Invalidate();        // repaint so the gutter updates now
  }
  ```
- **Upload (session verb):** in `UploadSource()`, on a successful `Uploader.Upload(...)`, call
  `MarkAsUnmodified()` instead of the bare `IsChanged = false` (this adds the missing repaint).
- **Local save (plain `.moo`):** after a successful `TrySaveToFile`, call `MarkAsUnmodified()`
  on the page. (`SaveToFile` already sets `IsChanged=false`; the helper guarantees the repaint
  and keeps the reset explicit and uniform.)
- **Load:** the Load handler calls `MarkAsUnmodified()` as above.

---

## Scope

### In scope
- The Load menu item, its enable state, and handler.
- The `MarkAsUnmodified` helper and its use on save, upload, and load.

### Out of scope
- Changing what "Open" does (still opens a new page).
- Any change to `ChangedLineColor`/theme behavior.
- Multi-file load or load-at-caret (replace only).

---

## Testing

- `MarkAsUnmodified()` sets `SourceEditor.IsChanged` to `false`.
- `UploadSource()` success path leaves `IsChanged == false` (with a fake `IClientUploader`
  returning success).
- Load-enable predicate: a `MooCodeEditorPage` is treated as loadable; a non-code page is not.
- FCTB painting is not unit-testable; the gutter visual is verified by build + manual check.
- Full solution builds clean; all test projects green.

---

## Decisions

- Load **replaces** the whole buffer and **prompts** when there are unsaved changes.
- After Load the gutter is **clean** (loaded file is the new unmodified baseline).
- A single `MarkAsUnmodified` helper (clear + invalidate) is used by save, upload, and load.
- Load's file filter matches Open's.
