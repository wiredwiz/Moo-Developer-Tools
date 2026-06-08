# Zoom-aware Autocomplete Popup — Design

**Date:** 2026-06-08
**Status:** Approved design, ready for implementation plan.
**Related:** builds on udd-lvn (autocomplete category icons / `CompletionIconFactory`).

---

## 1. Goal

When the user zooms the Moo Code Editor with **Ctrl + mouse wheel** (and the other zoom paths: Ctrl+`=`/`-`, Ctrl+wheel, `ChangeFontSize`), the autocomplete popup should scale to match — font, item height, overall window size, and the category icons + gutter — so the popup stays proportional to the editor text at any zoom level. Icons must stay sharp across the practical zoom range.

## 2. Current behaviour (why it doesn't scale)

- Zoom path: `FastColoredTextBox.OnMouseWheel` (Ctrl held) → `ChangeFontSize` → sets `Zoom` (%) → `DoZoom` rescales the editor font; `ZoomChanged` event fires (`FastColoredTextBox.cs:5851‑5953`).
- The popup is FCTB's `AutocompleteListView` (`AutocompleteMenu.cs:185`). Its font is hard-set once to **9 pt GenericSansSerif** (`:238`) and never tracks zoom. `ItemHeight = Font.Height + 2` (`:193`); window width/height derive from that font. `OnPaint` uses a hardcoded `leftPadding = 18` and draws icons at native size via `DrawImage(img, 1, y)` (`:482`, `:492`).
- The list view already holds the editor reference (`tb`, `:199`), so it can read `tb.Zoom` and subscribe to `tb.ZoomChanged` — it simply doesn't today.
- Icons come from **our** `CompletionIconFactory` (`Org.Edgerunner.Moo.Editor/Autocomplete/CompletionIconFactory.cs`), assigned to the menu in `MooCodeEditorPage.BuildAutocompleteMenu`. FCTB only *renders* the supplied `ImageList`; it does not (and must not) generate icons.

## 3. Responsibility split

The work divides cleanly across the two projects, with no new coupling between them (FCTB stays ignorant of icon content; it scales the draw rect of whatever `ImageList` it is given).

### 3a. Our editor — `CompletionIconFactory`
- Render the autocomplete `ImageList` at a **64 px base resolution** instead of 16 px (`ImageList.ImageSize = 64×64`, `ColorDepth.Depth32Bit`). The factory's drawing is already resolution-independent (it applies `ScaleTransform(size/24)` over 24×24 design coordinates), so this is just passing a larger size; no glyph rework. Memory cost is negligible (8 icons).
- One high-res `ImageList`, built once and cached. **No** per-zoom regeneration and **no** zoom subscription in the app/factory.
- Rationale for 64 px: at the editor's nominal 16 px icon, zoom up to 400% is still a pure *downscale* from 64 px (crisp); softness would only appear beyond 4× zoom, which is impractical.

### 3b. FCTB fork — `AutocompleteListView` (`AutocompleteMenu.cs`)
- Introduce a **base font** (the font assigned to the list view; default stays 9 pt GenericSansSerif = the 100% baseline) kept separate from the **effective font** actually used for layout/paint.
- Compute `scale = tb.Zoom / 100f`, clamped so the effective font never drops below a floor (≈ 6 pt) and the effective icon size never drops below ~8 px.
- Effective font = `new Font(baseFont.FontFamily, baseFont.SizeInPoints * scale, baseFont.Style)`. `ItemHeight` already follows `Font.Height`, so item height and the computed window width/height scale automatically.
- Subscribe to `tb.ZoomChanged`: recompute the effective font; if the menu is currently visible, recalc size + reposition live; always re-apply the current zoom when the menu is shown (so it is correct even if the zoom changed while the menu was closed). Unsubscribe in `Dispose`.
- `OnPaint`: replace the hardcoded constants with scaled values:
  - `iconSize = round(16 * scale)` (nominal 16 px icon at 100%).
  - gutter / text x-offset = `iconSize + 2` (was the fixed `18`).
  - draw each image into `new Rectangle(1, y + (ItemHeight - iconSize)/2, iconSize, iconSize)` with `Graphics.InterpolationMode = HighQualityBicubic` and `PixelOffsetMode = HighQuality` so downscaling from the 64 px source stays sharp and the icon is vertically centred in the (now taller) row.
- Scale the `MaximumSize` height cap (currently fixed `180`) by `scale`, so a comparable number of the taller rows remains visible when zoomed in.

## 4. Testability

Factor the pure sizing math into a small helper (e.g. a static method or struct that, given a base font size + zoom %, returns effective font size, icon size, gutter, and max-height, applying the floors). Unit-test it without showing a window:
- 100% → font 9 pt, icon 16 px, gutter 18, maxHeight 180.
- 200% → font 18 pt, icon 32 px, gutter 34, maxHeight 360.
- 50% → font ~6 pt (floor), icon ≥ ~8 px (floor), scaled accordingly.
Live popup appearance (sharpness, alignment, reposition-on-zoom) is verified manually — WinForms cannot be exercised headlessly.

## 5. Scope / non-goals

- **In scope:** proportional scaling of the autocomplete popup (font, item height, window, icons, gutter, max-height) driven by `tb.Zoom`; 64 px icon base in the factory.
- **Always-on**, no config toggle (YAGNI).
- **Non-goals:** no change to zoom input handling itself; no change to tooltip sizing beyond what falls out of the font; no high-DPI/per-monitor work beyond using the existing `Zoom` factor; no icon redesign.

## 6. Risk

Single control plus a one-line factory size change; additive. Main risk is the existing hardcoded paint constants (`18`, native `DrawImage`) — addressed in 3b. The FCTB fork is already heavily customized, so this stays consistent with it. The 64 px `ImageList.ImageSize` change must be verified against the icon assignment in `BuildAutocompleteMenu` (ImageIndex ordering is unaffected).
