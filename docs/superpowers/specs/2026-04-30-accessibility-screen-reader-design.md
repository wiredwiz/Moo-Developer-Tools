# Accessibility / Screen Reader Support Design Spec
**Date:** 2026-04-30
**Status:** Approved

---

## Goal

Add full UI Automation (UIA) screen reader support to the Moo Udditor so that blind and low-vision users can read, navigate, and edit Moo code and interact with the MUD terminal using NVDA, JAWS, or Windows Narrator.

---

## Scope

Both custom controls that inherit from `FastColoredTextBox`:

| Control | Assembly | Accessibility need |
|---|---|---|
| `MooCodeEditor` | `Org.Edgerunner.Moo.Editor` | Full editing: read, navigate, type, hear errors |
| `ConsoleWindowEmulator` | `Org.Edgerunner.Moo.Editor` | Read history + live announcements of incoming MUD text |

---

## Technology Choice

**UI Automation (UIA)** via the WinForms `Control.ControlAccessibleObject` + `ITextProvider`/`ITextRangeProvider` patterns.

Rationale: UIA gives the broadest screen reader coverage (NVDA, JAWS, Windows Narrator). `FastColoredTextBox` is a `UserControl` that custom-draws all content, so there is no underlying native EDIT control for screen readers to interrogate — the UIA `ITextProvider` pattern must be implemented explicitly.

MSAA (legacy) is intentionally out of scope; the WinForms UIA bridge makes UIA accessible to any screen reader that falls back to MSAA.

---

## Architecture — Partial Class Approach

All accessibility code lives in dedicated `*.Accessibility.cs` partial class files. Existing files are either untouched or receive only the minimal single change noted below.

```
FastColoredTextBox/
  FastColoredTextBox.cs                    ← UNCHANGED (lazy init means no edits needed)
  FastColoredTextBox.Accessibility.cs      ← NEW partial class
      CreateAccessibilityInstance()
      FireTextChangedUiaEvent()
      FireSelectionChangedUiaEvent()
      class FctbAccessibleObject
      class FctbTextRangeProvider

Org.Edgerunner.Moo.Editor/Controls/
  ConsoleWindowEmulator.cs                 ← UNCHANGED
  ConsoleWindowEmulator.Accessibility.cs   ← NEW partial class
      CreateAccessibilityInstance()
      FireLiveRegionChangedEvent()
      class ConsoleAccessibleObject

  MooCodeEditor.cs                         ← minimal: hook ParsingComplete to call UpdateErrors()
  MooCodeEditor.Accessibility.cs           ← NEW partial class
      CreateAccessibilityInstance()
      class MooCodeEditorAccessibleObject
```

**Lazy initialisation — zero changes to existing files:**
`CreateAccessibilityInstance()` is called by WinForms only when a screen reader first connects. The accessibility partial overrides it and wires up UIA event hooks at that point:

```csharp
// FastColoredTextBox.Accessibility.cs
private bool _accessibilityInitialized;

protected override AccessibleObject CreateAccessibilityInstance()
{
    if (!_accessibilityInitialized)
    {
        TextChanged += (_, _) => FireTextChangedUiaEvent();
        SelectionChanged += (_, _) => FireSelectionChangedUiaEvent();
        _accessibilityInitialized = true;
    }
    return new FctbAccessibleObject(this);
}
```

---

## Component Details

### `FctbAccessibleObject : ControlAccessibleObject, ITextProvider`

Lives in `FastColoredTextBox.Accessibility.cs`. Base class for both subclass accessible objects.

**`ITextProvider` implementation:**

| Member | Implementation |
|---|---|
| `DocumentRange` | `FctbTextRangeProvider` spanning `Place(0,0)` → last `Place` in `TextSource` |
| `GetSelection()` | Returns current `Selection` wrapped in a single `FctbTextRangeProvider` |
| `GetVisibleRanges()` | Returns one range spanning the currently visible lines |
| `RangeFromPoint(Point)` | Calls `PointToPlace()` → returns collapsed range at that position |
| `RangeFromChild(...)` | Returns `null` (leaf control, no child elements) |
| `SupportedTextSelection` | `SupportedTextSelection.Single` |

`LiveSetting` is `virtual` so subclasses can override to `AutomationLiveSetting.Polite`.

**UIA events fired by the base partial:**

```csharp
void FireTextChangedUiaEvent() =>
    AccessibilityObject?.RaiseAutomationEvent(AutomationEvents.TextPatternOnTextChanged);

void FireSelectionChangedUiaEvent() =>
    AccessibilityObject?.RaiseAutomationEvent(AutomationEvents.TextPatternOnTextSelectionChanged);
```

---

### `FctbTextRangeProvider : ITextRangeProvider`

Lives in `FastColoredTextBox.Accessibility.cs`. Holds a reference to the `FastColoredTextBox` instance and two `Place` endpoints (start, end). All 17 `ITextRangeProvider` members use existing public FCTB APIs — no new internal API surface is required.

| Member | Implementation |
|---|---|
| `GetText(maxLength)` | Reads from `Lines`, joins with `\n`, trims to `maxLength` |
| `Move(TextUnit, count)` | Walks `Lines` collection by Character / Word / Line using FCTB word-boundary helpers |
| `MoveEndpointByUnit(endpoint, unit, count)` | Moves one endpoint by unit without affecting the other |
| `MoveEndpointByRange(...)` | Sets one endpoint to match another range's endpoint |
| `ExpandToEnclosingUnit(TextUnit)` | Snaps both endpoints to enclosing word or line boundaries |
| `GetBoundingRectangles()` | `PlaceToPoint()` per line + `CharWidth` + line height → screen-coordinate `double[]` |
| `GetAttributeValue(attributeId)` | Returns font name, font size, foreground/background colour from control style; returns grammar annotation metadata for error spans (see below) |
| `GetChildren()` | Returns empty array |
| `GetEnclosingElement()` | Returns the owning `FctbAccessibleObject` |
| `Select()` | Sets `Selection` to this range |
| `AddToSelection()` / `RemoveFromSelection()` | Not supported (throws `InvalidOperationException`) |
| `ScrollIntoView(alignToTop)` | Scrolls FCTB to make start Place visible |
| `Clone()` | Returns a new `FctbTextRangeProvider` with copied endpoints |
| `Compare(range)` | Returns `true` if both endpoints match |
| `CompareEndpoints(...)` | Returns -1 / 0 / 1 by line then column |
| `FindText(text, backward, ignoreCase)` | Searches within range, returns matching sub-range or `null` |
| `FindAttribute(attributeId, value, backward)` | Searches for an attribute value within range |

---

### `ConsoleAccessibleObject : FctbAccessibleObject`

Lives in `ConsoleWindowEmulator.Accessibility.cs`.

**Differences from base:**

1. **Live region setting** — overrides `LiveSetting` to return `AutomationLiveSetting.Polite`. Screen readers discover this on connection and automatically read new content.

2. **Live region event** — `ConsoleWindowEmulator.Accessibility.cs` fires `LiveRegionChanged` after each write operation. `ConsoleWindowEmulator.cs` is not modified. `TextChanged` already fires whenever any write method appends text. The accessibility partial hooks this event in `CreateAccessibilityInstance()`:

```csharp
// ConsoleWindowEmulator.Accessibility.cs
protected override AccessibleObject CreateAccessibilityInstance()
{
    if (!_accessibilityInitialized)
    {
        TextChanged += (_, _) => FireLiveRegionChangedEvent();
        _accessibilityInitialized = true;
    }
    return new ConsoleAccessibleObject(this);
}

internal void FireLiveRegionChangedEvent() =>
    AccessibilityObject?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
```

**History navigation** is handled entirely by the inherited `ITextProvider`/`ITextRangeProvider` with no additional work.

---

### `MooCodeEditorAccessibleObject : FctbAccessibleObject`

Lives in `MooCodeEditor.Accessibility.cs`.

**Three additions over the base:**

**1. Editing echo (free from UIA)**
Character-by-character echo comes naturally from `TextPatternOnTextChanged` events already fired by the base. No extra work needed.

**2. Syntax error annotations on text spans**
`GetAttributeValue(UIA_AnnotationTypesAttributeId)` returns `AnnotationType_GrammarError` for any range that overlaps an active parser error. The accessible object holds the current error list, updated via a method called from the existing `ParsingComplete` event:

```csharp
// MooCodeEditor.Accessibility.cs
private IReadOnlyList<SyntaxError> _currentErrors = Array.Empty<SyntaxError>();

internal void UpdateErrors(IReadOnlyList<SyntaxError> errors) =>
    _currentErrors = errors;
```

When the caret enters an error span, screen readers automatically announce "grammar error." The user can then request the error message text, which is surfaced through `GetAttributeValue(UIA_FullDescriptionAttributeId)` returning the parser error message for that position.

**3. Debounced error count announcement (2-second idle)**

A `System.Windows.Forms.Timer` (2000ms, single-shot) lives in the accessibility partial. It is reset on every `TextChanged` event. When it fires (user idle for 2 full seconds):

- Compare current error count with `_lastAnnouncedErrorCount`
- If changed: update `AccessibleDescription` to `"3 syntax errors"` or `"No errors"`, then fire `RaiseAutomationEvent(AutomationEvents.LiveRegionChanged)`
- `LiveSetting` on `MooCodeEditorAccessibleObject` is `AutomationLiveSetting.Polite` — announcement waits for a screen-reader reading gap before speaking

```
User types → TextChanged fires → resets 2-second timer
...
2 seconds of no keystrokes → timer fires → error count changed?
  Yes → update AccessibleDescription → fire LiveRegionChanged
        → screen reader says "3 syntax errors" (politely)
  No  → do nothing

User navigates caret into error span
  → GetAttributeValue returns AnnotationType_GrammarError
  → screen reader says "grammar error"
  → user requests detail → error message text is read
```

The 2-second timer measures true keyboard idle (resets on `TextChanged`), independent of parser completion timing.

---

## What Screen Reader Users Can Do

### Code editor (`MooCodeEditor`)

| Action | Screen reader behaviour |
|---|---|
| Navigate line by line | Screen reader reads each line as caret moves |
| Navigate word by word | Screen reader reads each word |
| Type a character | UIA text-change event → character echoed |
| Pause 2 seconds with errors | Screen reader says "N syntax errors" |
| Move caret to error line | Screen reader says "grammar error" |
| Request error details | Full parser error message read aloud |
| Select text | Selection-change event → region described |
| Scroll | Bounding rects update; screen reader follows focus |

### Terminal (`ConsoleWindowEmulator`)

| Action | Screen reader behaviour |
|---|---|
| Server sends text | Screen reader reads incoming text (polite live region) |
| Review previous output | Navigate by line/word/character through history |
| Find text in history | `FindText` range search |

---

## Files Changed / Created

| File | Change |
|---|---|
| `FastColoredTextBox/FastColoredTextBox.Accessibility.cs` | New partial class |
| `FastColoredTextBox/FastColoredTextBox.csproj` | No changes (already targets net6.0-windows) |
| `Org.Edgerunner.Moo.Editor/Controls/ConsoleWindowEmulator.Accessibility.cs` | New partial class |
| `Org.Edgerunner.Moo.Editor/Controls/MooCodeEditor.Accessibility.cs` | New partial class |
| `FastColoredTextBox/FastColoredTextBox.cs` | **Unchanged** (lazy init) |
| `Org.Edgerunner.Moo.Editor/Controls/ConsoleWindowEmulator.cs` | **Unchanged** (TextChanged event used instead) |
| `Org.Edgerunner.Moo.Editor/Controls/MooCodeEditor.cs` | Minimal: hook `ParsingComplete` to call `UpdateErrors()` |

---

## Out of Scope

- Multi-selection UIA support (FCTB uses single selection only)
- `IScrollProvider` pattern (UIA scroll pattern) — existing scroll works via keyboard; adding the pattern is a future enhancement
- High-contrast theme support — separate accessibility concern
- Keyboard navigation customisation — FCTB already has standard keyboard handling
