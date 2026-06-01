# Accessibility / Screen Reader Support Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add full UI Automation (UIA) screen reader support to `FastColoredTextBox`, `ConsoleWindowEmulator`, and `MooCodeEditor` so NVDA, JAWS, and Windows Narrator users can read, navigate, and edit Moo code and MUD terminal output.

**Architecture:** Three partial-class files add UIA infrastructure without touching existing logic: `FastColoredTextBox.Accessibility.cs` provides `FctbAccessibleObject` (ITextProvider) and `FctbTextRangeProvider` (ITextRangeProvider) using the lazy `CreateAccessibilityInstance()` override; `ConsoleWindowEmulator.Accessibility.cs` adds live-region support; `MooCodeEditor.Accessibility.cs` adds error/warning annotations and a 2-second debounced diagnostic announcement.

**Tech Stack:** .NET 6 / C# / WinForms, UIAutomationProvider NuGet 1.0.0, xUnit + FluentAssertions (existing test patterns)

---

## Key Types Reference

Before starting, know these existing types:

```
FastColoredTextBoxNS.Place         — struct { int iChar; int iLine; }
                                     Place(iChar, iLine) constructor
FastColoredTextBoxNS.TextSelectionRange
  .Start / .End                    — Place
  .Text                            — string (read-only)
  .IsEmpty                         — bool
  .GoRight() / GoLeft()            — bool (true if moved)
  .GoWordRight(bool shift)
  .GoWordLeft(bool shift)

FastColoredTextBox (partial — FastColoredTextBox.cs)
  .Lines                           — IList<string>
  .LinesCount                      — int
  .Selection                       — TextSelectionRange (current caret/selection)
  .VisibleRange                    — TextSelectionRange (on-screen portion)
  .CharWidth / CharHeight          — int (virtual)
  .Font                            — Font (inherited from Control)
  .ForeColor / BackColor           — Color
  .PlaceToPoint(Place)             — Point (client coordinates)
  .PointToPlace(Point)             — Place
  .GetRange(Place, Place)          — TextSelectionRange
  .GetLine(int iLine)              — TextSelectionRange spanning one line
  .GetLineText(int iLine)          — string
  .DoRangeVisible(TextSelectionRange) — scrolls to make range visible
  .TextChanged                     — event EventHandler<TextChangedEventArgs>
  .SelectionChanged                — event EventHandler
```

---

## File Map

| File | Action | Purpose |
|---|---|---|
| `Org.Edgerunner.ANTLR4.Tools.Common/Grammar/Errors/ParseMessage.cs` | Modify | Add `ParseMessageSeverity` enum + `Severity` property |
| `Org.Edgerunner.ANTLR4.Tools.Common/Grammar/Errors/LexerErrorListener.cs` | Modify | Set `Severity = Error` when constructing `ParseMessage` |
| `Org.Edgerunner.ANTLR4.Tools.Common/Grammar/Errors/ParserErrorListener.cs` | Modify | Set `Severity = Error` when constructing `ParseMessage` |
| `Org.Edgerunner.Moo.Editor.Tests/` | Create | xUnit test project for pure logic tests |
| `FastColoredTextBox/FastColoredTextBox.csproj` | Modify | Add UIAutomationProvider NuGet |
| `Org.Edgerunner.Moo.Editor/Org.Edgerunner.Moo.Editor.csproj` | Modify | Add UIAutomationProvider NuGet |
| `FastColoredTextBox/FastColoredTextBox.Accessibility.cs` | Create | Partial class: FctbAccessibleObject + FctbTextRangeProvider |
| `Org.Edgerunner.Moo.Editor/Controls/ConsoleWindowEmulator.Accessibility.cs` | Create | Partial class: ConsoleAccessibleObject with live region |
| `Org.Edgerunner.Moo.Editor/Controls/MooCodeEditor.Accessibility.cs` | Create | Partial class: MooCodeEditorAccessibleObject with diagnostics |
| `Org.Edgerunner.Moo.Editor/Controls/MooCodeEditor.cs` | Modify | Wire `ParsingComplete` → `UpdateDiagnostics()` |

---

## Task 1: ParseMessage Severity

**Files:**
- Modify: `Org.Edgerunner.ANTLR4.Tools.Common/Grammar/Errors/ParseMessage.cs`
- Modify: `Org.Edgerunner.ANTLR4.Tools.Common/Grammar/Errors/LexerErrorListener.cs`
- Modify: `Org.Edgerunner.ANTLR4.Tools.Common/Grammar/Errors/ParserErrorListener.cs`

- [ ] **Step 1: Add `ParseMessageSeverity` enum and `Severity` property to `ParseMessage.cs`**

Read the file first to find where to insert. Add the enum before the struct and the property inside it. The struct constructor should set `Severity = ParseMessageSeverity.Error` as the default:

```csharp
// Add before the ParseMessage struct declaration:
/// <summary>Severity level of a parse message.</summary>
public enum ParseMessageSeverity
{
   Error,
   Warning
}
```

Add inside the `ParseMessage` struct, after the existing `Guide` property:

```csharp
/// <summary>Gets or sets the severity of this message.</summary>
/// <value>The severity.</value>
public ParseMessageSeverity Severity { get; set; }
```

- [ ] **Step 2: Update `LexerErrorListener.cs` to set Severity**

In the `SyntaxError` method where it does `Errors.Add(new ParseMessage(...))`, add `Severity = ParseMessageSeverity.Error` using the object initializer or by setting the property after construction:

```csharp
// The existing line is:
Errors.Add(new ParseMessage(Document, line, charPositionInLine + 1, "Lexer", msg, null));
// Change to:
var msg_ = new ParseMessage(Document, line, charPositionInLine + 1, "Lexer", msg, null);
msg_.Severity = ParseMessageSeverity.Error;
Errors.Add(msg_);
```

> Note: `ParseMessage` is a struct so you cannot use object initializer on an existing Add call directly. Use the two-line pattern above or convert to use a local variable.

- [ ] **Step 3: Update `ParserErrorListener.cs` similarly**

Find the `SyntaxError` method in `ParserErrorListener.cs` and apply the same two-line pattern to set `Severity = ParseMessageSeverity.Error`.

- [ ] **Step 4: Build to verify**

```bash
cd "D:\Projects\Moo Developer Tools"
dotnet build "Org.Edgerunner.ANTLR4.Tools.Common/Org.Edgerunner.ANTLR4.Tools.Common.csproj"
```

Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
cd "D:\Projects\Moo Developer Tools"
git add Org.Edgerunner.ANTLR4.Tools.Common/Grammar/Errors/
git commit -m "Add ParseMessageSeverity enum and Severity property to ParseMessage"
```

---

## Task 2: Test Project + UIAutomationProvider NuGet

**Files:**
- Create: `Org.Edgerunner.Moo.Editor.Tests/Org.Edgerunner.Moo.Editor.Tests.csproj`
- Create: `Org.Edgerunner.Moo.Editor.Tests/DiagnosticAnnouncementTests.cs`
- Modify: `FastColoredTextBox/FastColoredTextBox.csproj`
- Modify: `Org.Edgerunner.Moo.Editor/Org.Edgerunner.Moo.Editor.csproj`

- [ ] **Step 1: Add UIAutomationProvider to FastColoredTextBox.csproj**

Read the file. Add inside the first `<ItemGroup>` with package references (or create one):

```xml
<PackageReference Include="UIAutomationProvider" Version="1.0.0" />
```

- [ ] **Step 2: Add UIAutomationProvider to Org.Edgerunner.Moo.Editor.csproj**

Read the file. Add the same package reference:

```xml
<PackageReference Include="UIAutomationProvider" Version="1.0.0" />
```

- [ ] **Step 3: Create test project**

Create `Org.Edgerunner.Moo.Editor.Tests/Org.Edgerunner.Moo.Editor.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net6.0-windows</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>disable</Nullable>
    <IsPackable>false</IsPackable>
    <Platforms>AnyCPU;x64;x86</Platforms>
    <UseWindowsForms>true</UseWindowsForms>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
    <PackageReference Include="xunit" Version="2.7.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.7">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Org.Edgerunner.Moo.Editor\Org.Edgerunner.Moo.Editor.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 4: Add to solution**

```bash
cd "D:\Projects\Moo Developer Tools"
dotnet sln "Moo Developer Tools.sln" add "Org.Edgerunner.Moo.Editor.Tests/Org.Edgerunner.Moo.Editor.Tests.csproj"
```

- [ ] **Step 5: Write failing tests for announcement string logic**

Create `Org.Edgerunner.Moo.Editor.Tests/DiagnosticAnnouncementTests.cs`:

```csharp
using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Controls;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class DiagnosticAnnouncementTests
{
    [Fact]
    public void BuildAnnouncementString_NoErrorsNoWarnings_ReturnsNoErrors()
    {
        MooCodeEditorAccessibleObject.BuildAnnouncementString(0, 0)
            .Should().Be("No errors");
    }

    [Fact]
    public void BuildAnnouncementString_OneError_ReturnsSingular()
    {
        MooCodeEditorAccessibleObject.BuildAnnouncementString(1, 0)
            .Should().Be("1 syntax error");
    }

    [Fact]
    public void BuildAnnouncementString_MultipleErrors_ReturnsPlural()
    {
        MooCodeEditorAccessibleObject.BuildAnnouncementString(3, 0)
            .Should().Be("3 syntax errors");
    }

    [Fact]
    public void BuildAnnouncementString_OneWarning_ReturnsSingular()
    {
        MooCodeEditorAccessibleObject.BuildAnnouncementString(0, 1)
            .Should().Be("1 warning");
    }

    [Fact]
    public void BuildAnnouncementString_MultipleWarnings_ReturnsPlural()
    {
        MooCodeEditorAccessibleObject.BuildAnnouncementString(0, 2)
            .Should().Be("2 warnings");
    }

    [Fact]
    public void BuildAnnouncementString_ErrorsAndWarnings_ReturnsBoth()
    {
        MooCodeEditorAccessibleObject.BuildAnnouncementString(3, 2)
            .Should().Be("3 syntax errors and 2 warnings");
    }

    [Fact]
    public void BuildAnnouncementString_OneErrorOneWarning_ReturnsSingulars()
    {
        MooCodeEditorAccessibleObject.BuildAnnouncementString(1, 1)
            .Should().Be("1 syntax error and 1 warning");
    }
}
```

- [ ] **Step 6: Run tests to confirm they fail**

```bash
cd "D:\Projects\Moo Developer Tools"
dotnet test "Org.Edgerunner.Moo.Editor.Tests/Org.Edgerunner.Moo.Editor.Tests.csproj" --filter "FullyQualifiedName~DiagnosticAnnouncementTests"
```

Expected: Build error — `MooCodeEditorAccessibleObject` does not exist yet.

- [ ] **Step 7: Build solution to verify NuGet additions**

```bash
cd "D:\Projects\Moo Developer Tools"
dotnet build "Moo Developer Tools.sln"
```

Expected: `Build succeeded.`

- [ ] **Step 8: Commit**

```bash
cd "D:\Projects\Moo Developer Tools"
git add FastColoredTextBox/FastColoredTextBox.csproj Org.Edgerunner.Moo.Editor/Org.Edgerunner.Moo.Editor.csproj Org.Edgerunner.Moo.Editor.Tests/ "Moo Developer Tools.sln"
git commit -m "Add UIAutomationProvider NuGet and Moo.Editor.Tests project"
```

---

## Task 3: FctbAccessibleObject + ITextProvider

**Files:**
- Create: `FastColoredTextBox/FastColoredTextBox.Accessibility.cs`

This is a partial class for `FastColoredTextBox`. It overrides `CreateAccessibilityInstance()` lazily (no changes to `FastColoredTextBox.cs`), creating `FctbAccessibleObject` and wiring UIA events.

- [ ] **Step 1: Create `FastColoredTextBox.Accessibility.cs`**

```csharp
#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="FastColoredTextBox.Accessibility.cs">
// Copyright (c) Thaddeus Ryker 2022
// </copyright>
//
// BSD 3-Clause License
// ... (standard header)
#endregion

using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;
using System.Windows.Forms;

namespace FastColoredTextBoxNS;

public partial class FastColoredTextBox
{
   private bool _accessibilityInitialized;

   protected override AccessibleObject CreateAccessibilityInstance()
   {
      if (!_accessibilityInitialized)
      {
         TextChanged += (_, _) => AccessibilityObject?.RaiseAutomationEvent(AutomationEvents.TextPatternOnTextChanged);
         SelectionChanged += (_, _) => AccessibilityObject?.RaiseAutomationEvent(AutomationEvents.TextPatternOnTextSelectionChanged);
         _accessibilityInitialized = true;
      }
      return new FctbAccessibleObject(this);
   }
}

/// <summary>
/// Accessibility object for FastColoredTextBox implementing the UIA Text Pattern.
/// </summary>
public class FctbAccessibleObject : Control.ControlAccessibleObject, ITextProvider
{
   protected readonly FastColoredTextBox Tb;

   public FctbAccessibleObject(FastColoredTextBox tb) : base(tb)
   {
      Tb = tb;
   }

   // ── ITextProvider ──────────────────────────────────────────────────

   public virtual ITextRangeProvider DocumentRange =>
      new FctbTextRangeProvider(Tb,
         new Place(0, 0),
         new Place(Tb.Lines[Tb.LinesCount - 1].Length, Tb.LinesCount - 1));

   public SupportedTextSelection SupportedTextSelection => SupportedTextSelection.Single;

   public ITextRangeProvider[] GetSelection()
   {
      var sel = Tb.Selection;
      return new ITextRangeProvider[]
      {
         new FctbTextRangeProvider(Tb, sel.Start, sel.End)
      };
   }

   public ITextRangeProvider[] GetVisibleRanges()
   {
      var vis = Tb.VisibleRange;
      return new ITextRangeProvider[]
      {
         new FctbTextRangeProvider(Tb, vis.Start, vis.End)
      };
   }

   public ITextRangeProvider RangeFromChild(IRawElementProviderSimple childElement) => null;

   public ITextRangeProvider RangeFromPoint(System.Windows.Point screenLocation)
   {
      var clientPt = Tb.PointToClient(
         new System.Drawing.Point((int)screenLocation.X, (int)screenLocation.Y));
      var place = Tb.PointToPlace(clientPt);
      return new FctbTextRangeProvider(Tb, place, place);
   }

   /// <summary>Gets the UIA live region setting. Virtual so subclasses can override.</summary>
   public virtual AutomationLiveSetting LiveSetting => AutomationLiveSetting.Off;
}
```

- [ ] **Step 2: Build to verify compilation**

```bash
cd "D:\Projects\Moo Developer Tools"
dotnet build "FastColoredTextBox/FastColoredTextBox.csproj"
```

Expected: `Build succeeded.` (FctbTextRangeProvider not yet defined — expect error referencing it. That is fine for this step; address in Task 4.)

> Note: The build will fail with "FctbTextRangeProvider not found" until Task 4. That is expected — proceed to Task 4 immediately.

- [ ] **Step 3: Commit skeleton (pre-compile)**

Do NOT commit yet — wait for Task 4 to have a compiling state before committing.

---

## Task 4: FctbTextRangeProvider — Fundamentals

**Files:**
- Modify: `FastColoredTextBox/FastColoredTextBox.Accessibility.cs` (add FctbTextRangeProvider class)

Add the `FctbTextRangeProvider` class to the same file, after `FctbAccessibleObject`. This task covers the simpler 10 methods; Tasks 5 and 6 cover the navigation and attribute methods.

- [ ] **Step 1: Add `FctbTextRangeProvider` with fundamentals**

Append to `FastColoredTextBox.Accessibility.cs` (after the closing `}` of `FctbAccessibleObject`):

```csharp
/// <summary>
/// Implements ITextRangeProvider for a span of text within FastColoredTextBox.
/// </summary>
public class FctbTextRangeProvider : ITextRangeProvider
{
   protected readonly FastColoredTextBox Tb;
   protected Place _start;
   protected Place _end;

   public FctbTextRangeProvider(FastColoredTextBox tb, Place start, Place end)
   {
      Tb = tb;
      _start = start;
      _end = end;
   }

   /// <summary>Returns (normalizedStart, normalizedEnd) — start always before end.</summary>
   private (Place s, Place e) Normalized()
   {
      bool startFirst = _start.iLine < _end.iLine ||
                        (_start.iLine == _end.iLine && _start.iChar <= _end.iChar);
      return startFirst ? (_start, _end) : (_end, _start);
   }

   // ── Simple members ─────────────────────────────────────────────────

   public ITextRangeProvider Clone() =>
      new FctbTextRangeProvider(Tb, _start, _end);

   public bool Compare(ITextRangeProvider range)
   {
      if (range is not FctbTextRangeProvider other) return false;
      return _start.iLine == other._start.iLine &&
             _start.iChar == other._start.iChar &&
             _end.iLine == other._end.iLine &&
             _end.iChar == other._end.iChar;
   }

   public int CompareEndpoints(
      TextPatternRangeEndpoint endpoint,
      ITextRangeProvider targetRange,
      TextPatternRangeEndpoint targetEndpoint)
   {
      var mine = endpoint == TextPatternRangeEndpoint.Start ? _start : _end;
      var other = targetRange is FctbTextRangeProvider r
         ? (targetEndpoint == TextPatternRangeEndpoint.Start ? r._start : r._end)
         : new Place(0, 0);

      if (mine.iLine != other.iLine) return mine.iLine.CompareTo(other.iLine);
      return mine.iChar.CompareTo(other.iChar);
   }

   public IRawElementProviderSimple GetEnclosingElement() =>
      Tb.AccessibilityObject as IRawElementProviderSimple;

   public IRawElementProviderSimple[] GetChildren() =>
      Array.Empty<IRawElementProviderSimple>();

   public string GetText(int maxLength)
   {
      var (s, e) = Normalized();
      var sb = new System.Text.StringBuilder();
      for (int line = s.iLine; line <= e.iLine; line++)
      {
         if (maxLength >= 0 && sb.Length >= maxLength) break;
         int fromChar = line == s.iLine ? s.iChar : 0;
         int toChar   = line == e.iLine ? e.iChar  : Tb.Lines[line].Length;
         toChar = Math.Min(toChar, Tb.Lines[line].Length);
         if (line > s.iLine) sb.Append('\n');
         if (fromChar < toChar)
            sb.Append(Tb.Lines[line].Substring(fromChar, toChar - fromChar));
      }
      var result = sb.ToString();
      if (maxLength >= 0 && result.Length > maxLength)
         result = result[..maxLength];
      return result;
   }

   public double[] GetBoundingRectangles()
   {
      if (Tb.IsDisposed || !Tb.IsHandleCreated) return Array.Empty<double>();
      var (s, e) = Normalized();
      var rects = new List<double>();
      for (int line = s.iLine; line <= e.iLine && line < Tb.LinesCount; line++)
      {
         int fromChar = line == s.iLine ? s.iChar : 0;
         int toChar   = line == e.iLine ? e.iChar  : Tb.Lines[line].Length;
         toChar = Math.Min(toChar, Tb.Lines[line].Length);
         if (fromChar >= toChar) continue;
         var p1 = Tb.PointToScreen(Tb.PlaceToPoint(new Place(fromChar, line)));
         var p2 = Tb.PointToScreen(Tb.PlaceToPoint(new Place(toChar, line)));
         rects.Add(p1.X);
         rects.Add(p1.Y);
         rects.Add(Math.Abs(p2.X - p1.X));
         rects.Add(Tb.CharHeight);
      }
      return rects.ToArray();
   }

   public void AddToSelection() =>
      throw new InvalidOperationException("Single selection only.");

   public void RemoveFromSelection() =>
      throw new InvalidOperationException("Single selection only.");

   public void Select()
   {
      if (Tb.InvokeRequired)
         Tb.Invoke(() => Select());
      else
      {
         Tb.Selection.Start = _start;
         Tb.Selection.End = _end;
         Tb.Invalidate();
      }
   }

   public void ScrollIntoView(bool alignToTop)
   {
      var range = Tb.GetRange(_start, _end);
      if (Tb.InvokeRequired) Tb.Invoke(() => Tb.DoRangeVisible(range, alignToTop));
      else Tb.DoRangeVisible(range, alignToTop);
   }

   // ── Navigation, attributes, search — implemented in follow-on tasks ──

   public void ExpandToEnclosingUnit(TextUnit unit) => ExpandToEnclosingUnitImpl(unit);
   public int Move(TextUnit unit, int count) => MoveImpl(unit, count);
   public int MoveEndpointByUnit(TextPatternRangeEndpoint endpoint, TextUnit unit, int count)
      => MoveEndpointByUnitImpl(endpoint, unit, count);
   public void MoveEndpointByRange(TextPatternRangeEndpoint endpoint,
      ITextRangeProvider targetRange, TextPatternRangeEndpoint targetEndpoint)
      => MoveEndpointByRangeImpl(endpoint, targetRange, targetEndpoint);
   public object GetAttributeValue(int attribute) => GetAttributeValueImpl(attribute);
   public ITextRangeProvider FindText(string text, bool backward, bool ignoreCase)
      => FindTextImpl(text, backward, ignoreCase);
   public ITextRangeProvider FindAttribute(int attribute, object value, bool backward)
      => null; // not required for basic screen reader support

   // Stubs — replaced in Tasks 5 and 6
   protected virtual void ExpandToEnclosingUnitImpl(TextUnit unit) { }
   protected virtual int MoveImpl(TextUnit unit, int count) => 0;
   protected virtual int MoveEndpointByUnitImpl(TextPatternRangeEndpoint endpoint, TextUnit unit, int count) => 0;
   protected virtual void MoveEndpointByRangeImpl(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, TextPatternRangeEndpoint targetEndpoint) { }
   protected virtual object GetAttributeValueImpl(int attribute) => AutomationElementIdentifiers.NotSupportedValue;
   protected virtual ITextRangeProvider FindTextImpl(string text, bool backward, bool ignoreCase) => null;
}
```

- [ ] **Step 2: Build to verify**

```bash
cd "D:\Projects\Moo Developer Tools"
dotnet build "FastColoredTextBox/FastColoredTextBox.csproj"
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
cd "D:\Projects\Moo Developer Tools"
git add FastColoredTextBox/FastColoredTextBox.Accessibility.cs
git commit -m "Add FctbAccessibleObject (ITextProvider) and FctbTextRangeProvider skeleton"
```

---

## Task 5: FctbTextRangeProvider — Navigation

**Files:**
- Modify: `FastColoredTextBox/FastColoredTextBox.Accessibility.cs`

Replace the stub implementations with real navigation logic. These methods are what screen readers use to move the reading cursor by character, word, and line.

- [ ] **Step 1: Replace navigation stubs in `FctbTextRangeProvider`**

Replace the four `protected virtual ...Impl` stub methods with these implementations:

```csharp
protected override void ExpandToEnclosingUnitImpl(TextUnit unit)
{
   switch (unit)
   {
      case TextUnit.Character:
         // Collapse to single character at start
         _end = _start;
         if (_start.iChar < Tb.Lines[_start.iLine].Length)
            _end = new Place(_start.iChar + 1, _start.iLine);
         break;

      case TextUnit.Word:
         // Snap start backward to word boundary
         var startRange = new TextSelectionRange(Tb, _start.iChar, _start.iLine, _start.iChar, _start.iLine);
         startRange.GoWordLeft(false);
         _start = startRange.Start;
         // Snap end forward to word boundary
         var endRange = new TextSelectionRange(Tb, _start.iChar, _start.iLine, _start.iChar, _start.iLine);
         endRange.GoWordRight(false);
         _end = endRange.End;
         break;

      case TextUnit.Line:
      case TextUnit.Paragraph:
         _start = new Place(0, _start.iLine);
         _end   = new Place(Tb.Lines[_start.iLine].Length, _start.iLine);
         break;

      case TextUnit.Page:
      case TextUnit.Document:
         _start = new Place(0, 0);
         _end   = new Place(Tb.Lines[Tb.LinesCount - 1].Length, Tb.LinesCount - 1);
         break;
   }
}

protected override int MoveImpl(TextUnit unit, int count)
{
   // Collapse to start, then move start by count units, set end = start
   int moved = MoveEndpointByUnitImpl(TextPatternRangeEndpoint.Start, unit, count);
   _end = _start;
   return moved;
}

protected override int MoveEndpointByUnitImpl(
   TextPatternRangeEndpoint endpoint, TextUnit unit, int count)
{
   ref Place place = ref endpoint == TextPatternRangeEndpoint.Start ? ref _start : ref _end;
   int moved = 0;
   int direction = count < 0 ? -1 : 1;
   int steps = Math.Abs(count);

   for (int i = 0; i < steps; i++)
   {
      bool advanced = AdvanceByUnit(ref place, unit, direction);
      if (!advanced) break;
      moved += direction;
   }
   return moved;
}

private bool AdvanceByUnit(ref Place place, TextUnit unit, int direction)
{
   switch (unit)
   {
      case TextUnit.Character:
         if (direction > 0)
         {
            if (place.iChar < Tb.Lines[place.iLine].Length)
               place = new Place(place.iChar + 1, place.iLine);
            else if (place.iLine < Tb.LinesCount - 1)
               place = new Place(0, place.iLine + 1);
            else return false;
         }
         else
         {
            if (place.iChar > 0)
               place = new Place(place.iChar - 1, place.iLine);
            else if (place.iLine > 0)
               place = new Place(Tb.Lines[place.iLine - 1].Length, place.iLine - 1);
            else return false;
         }
         return true;

      case TextUnit.Word:
         var r = new TextSelectionRange(Tb, place.iChar, place.iLine, place.iChar, place.iLine);
         if (direction > 0) r.GoWordRight(false, true);
         else r.GoWordLeft(false);
         var newPlace = direction > 0 ? r.End : r.Start;
         if (newPlace.iLine == place.iLine && newPlace.iChar == place.iChar) return false;
         place = newPlace;
         return true;

      case TextUnit.Line:
      case TextUnit.Paragraph:
         int newLine = place.iLine + direction;
         if (newLine < 0 || newLine >= Tb.LinesCount) return false;
         place = new Place(0, newLine);
         return true;

      case TextUnit.Page:
      case TextUnit.Document:
         if (direction > 0)
            place = new Place(Tb.Lines[Tb.LinesCount - 1].Length, Tb.LinesCount - 1);
         else
            place = new Place(0, 0);
         return true;

      default:
         return false;
   }
}

protected override void MoveEndpointByRangeImpl(
   TextPatternRangeEndpoint endpoint,
   ITextRangeProvider targetRange,
   TextPatternRangeEndpoint targetEndpoint)
{
   if (targetRange is not FctbTextRangeProvider other) return;
   var source = targetEndpoint == TextPatternRangeEndpoint.Start ? other._start : other._end;
   if (endpoint == TextPatternRangeEndpoint.Start) _start = source;
   else _end = source;
}
```

> Note: `TextSelectionRange` constructor signature is `(FastColoredTextBox tb, int iStartChar, int iStartLine, int iEndChar, int iEndLine)`. See the Key Types Reference at the top of this plan.

- [ ] **Step 2: Build to verify**

```bash
cd "D:\Projects\Moo Developer Tools"
dotnet build "FastColoredTextBox/FastColoredTextBox.csproj"
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
cd "D:\Projects\Moo Developer Tools"
git add FastColoredTextBox/FastColoredTextBox.Accessibility.cs
git commit -m "Implement FctbTextRangeProvider navigation (Move, Expand, MoveEndpoint)"
```

---

## Task 6: FctbTextRangeProvider — Attributes and Search

**Files:**
- Modify: `FastColoredTextBox/FastColoredTextBox.Accessibility.cs`

Implement `GetAttributeValueImpl` and `FindTextImpl`. `GetAttributeValue` is called by screen readers to get font info at a range. `FindText` powers search navigation.

- [ ] **Step 1: Add UIA attribute ID constants to `FctbTextRangeProvider`**

Add these private constants near the top of the `FctbTextRangeProvider` class (before `Normalized()`):

```csharp
// Standard UIA text attribute IDs (from Windows SDK UIAutomation headers)
private const int UIA_FontNameAttributeId       = 40005;
private const int UIA_FontSizeAttributeId       = 40006;
private const int UIA_ForegroundColorAttributeId = 40008;
private const int UIA_BackgroundColorAttributeId = 40009;
private const int UIA_AnnotationTypesAttributeId = 40031;
private const int UIA_FullDescriptionAttributeId = 40035;

// Standard UIA annotation type IDs
protected const int AnnotationType_GrammarError        = 60002;
protected const int AnnotationType_AdvancedProofingIssue = 60017;
```

- [ ] **Step 2: Replace `GetAttributeValueImpl` stub**

```csharp
protected override object GetAttributeValueImpl(int attribute)
{
   switch (attribute)
   {
      case UIA_FontNameAttributeId:
         return Tb.Font.Name;

      case UIA_FontSizeAttributeId:
         return (double)Tb.Font.SizeInPoints;

      case UIA_ForegroundColorAttributeId:
         return (int)(
            ((uint)Tb.ForeColor.R << 16) |
            ((uint)Tb.ForeColor.G << 8)  |
             (uint)Tb.ForeColor.B);

      case UIA_BackgroundColorAttributeId:
         return (int)(
            ((uint)Tb.BackColor.R << 16) |
            ((uint)Tb.BackColor.G << 8)  |
             (uint)Tb.BackColor.B);

      case UIA_AnnotationTypesAttributeId:
         // Base implementation: no annotations. Overridden in MooCodeEditorAccessibleObject.
         return AutomationElementIdentifiers.NotSupportedValue;

      case UIA_FullDescriptionAttributeId:
         return AutomationElementIdentifiers.NotSupportedValue;

      default:
         return AutomationElementIdentifiers.NotSupportedValue;
   }
}
```

- [ ] **Step 3: Replace `FindTextImpl` stub**

```csharp
protected override ITextRangeProvider FindTextImpl(string text, bool backward, bool ignoreCase)
{
   if (string.IsNullOrEmpty(text)) return null;
   var (s, e) = Normalized();
   var comparison = ignoreCase
      ? StringComparison.OrdinalIgnoreCase
      : StringComparison.Ordinal;

   if (!backward)
   {
      for (int line = s.iLine; line <= e.iLine && line < Tb.LinesCount; line++)
      {
         string lineText = Tb.Lines[line];
         int startChar = line == s.iLine ? s.iChar : 0;
         int idx = lineText.IndexOf(text, startChar, comparison);
         if (idx >= 0 && (line < e.iLine || idx + text.Length <= e.iChar))
            return new FctbTextRangeProvider(Tb,
               new Place(idx, line),
               new Place(idx + text.Length, line));
      }
   }
   else
   {
      for (int line = e.iLine; line >= s.iLine; line--)
      {
         string lineText = Tb.Lines[line];
         int endChar = line == e.iLine ? e.iChar : lineText.Length;
         int searchLen = Math.Max(0, endChar - text.Length);
         int idx = lineText.LastIndexOf(text, endChar - 1, Math.Max(0, endChar), comparison);
         if (idx >= 0 && (line > s.iLine || idx >= s.iChar))
            return new FctbTextRangeProvider(Tb,
               new Place(idx, line),
               new Place(idx + text.Length, line));
      }
   }
   return null;
}
```

- [ ] **Step 4: Build to verify**

```bash
cd "D:\Projects\Moo Developer Tools"
dotnet build "FastColoredTextBox/FastColoredTextBox.csproj"
```

Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
cd "D:\Projects\Moo Developer Tools"
git add FastColoredTextBox/FastColoredTextBox.Accessibility.cs
git commit -m "Implement FctbTextRangeProvider attribute values and text search"
```

---

## Task 7: ConsoleWindowEmulator.Accessibility.cs

**Files:**
- Create: `Org.Edgerunner.Moo.Editor/Controls/ConsoleWindowEmulator.Accessibility.cs`

Adds live-region support so screen readers automatically read new terminal output.

- [ ] **Step 1: Create `ConsoleWindowEmulator.Accessibility.cs`**

```csharp
#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="ConsoleWindowEmulator.Accessibility.cs">
// Copyright (c) Thaddeus Ryker 2022
// </copyright>
// ... (standard BSD header)
#endregion

using System.Windows.Automation;
using System.Windows.Forms;
using FastColoredTextBoxNS;

namespace Org.Edgerunner.Moo.Editor.Controls;

public partial class ConsoleWindowEmulator
{
   private bool _consoleAccessibilityInitialized;

   protected override AccessibleObject CreateAccessibilityInstance()
   {
      if (!_consoleAccessibilityInitialized)
      {
         // Fire LiveRegionChanged every time new text is written
         TextChanged += (_, _) => FireLiveRegionChangedEvent();
         _consoleAccessibilityInitialized = true;
      }
      return new ConsoleAccessibleObject(this);
   }

   internal void FireLiveRegionChangedEvent() =>
      AccessibilityObject?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
}

/// <summary>
/// Accessible object for ConsoleWindowEmulator — adds live-region support so screen
/// readers automatically announce incoming MUD server text.
/// </summary>
public class ConsoleAccessibleObject : FctbAccessibleObject
{
   public ConsoleAccessibleObject(ConsoleWindowEmulator console) : base(console) { }

   /// <summary>
   /// Polite live setting: screen reader reads new content during natural pauses.
   /// </summary>
   public override AutomationLiveSetting LiveSetting => AutomationLiveSetting.Polite;
}
```

- [ ] **Step 2: Build to verify**

```bash
cd "D:\Projects\Moo Developer Tools"
dotnet build "Org.Edgerunner.Moo.Editor/Org.Edgerunner.Moo.Editor.csproj"
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
cd "D:\Projects\Moo Developer Tools"
git add "Org.Edgerunner.Moo.Editor/Controls/ConsoleWindowEmulator.Accessibility.cs"
git commit -m "Add ConsoleWindowEmulator live-region accessibility (ConsoleAccessibleObject)"
```

---

## Task 8: MooCodeEditor.Accessibility.cs — Diagnostics Infrastructure

**Files:**
- Create: `Org.Edgerunner.Moo.Editor/Controls/MooCodeEditor.Accessibility.cs`

Adds error/warning annotations to text spans. A range overlapping a parser error returns `AnnotationType_GrammarError`; a warning returns `AnnotationType_AdvancedProofingIssue`.

- [ ] **Step 1: Create `MooCodeEditor.Accessibility.cs`**

```csharp
#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="MooCodeEditor.Accessibility.cs">
// Copyright (c) Thaddeus Ryker 2022
// </copyright>
// ... (standard BSD header)
#endregion

using System.Windows.Automation;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar.Errors;

namespace Org.Edgerunner.Moo.Editor.Controls;

public partial class MooCodeEditor
{
   private bool _editorAccessibilityInitialized;

   protected override AccessibleObject CreateAccessibilityInstance()
   {
      if (!_editorAccessibilityInitialized)
      {
         _editorAccessibilityInitialized = true;
      }
      return new MooCodeEditorAccessibleObject(this);
   }

   /// <summary>Updates the diagnostic list used by the accessible object.</summary>
   internal void UpdateDiagnostics(List<ParseMessage> messages)
   {
      if (AccessibilityObject is MooCodeEditorAccessibleObject acc)
         acc.UpdateDiagnostics(messages);
   }
}

/// <summary>
/// Accessible object for MooCodeEditor — adds parser error/warning annotations on
/// text spans and a debounced diagnostic count announcement.
/// </summary>
public class MooCodeEditorAccessibleObject : FctbAccessibleObject
{
   private readonly MooCodeEditor _editor;
   private IReadOnlyList<ParseMessage> _errors   = Array.Empty<ParseMessage>();
   private IReadOnlyList<ParseMessage> _warnings = Array.Empty<ParseMessage>();

   // UIA annotation type constants (duplicated from FctbTextRangeProvider for accessibility)
   private const int AnnotationType_GrammarError         = 60002;
   private const int AnnotationType_AdvancedProofingIssue = 60017;

   public MooCodeEditorAccessibleObject(MooCodeEditor editor) : base(editor)
   {
      _editor = editor;
   }

   public override AutomationLiveSetting LiveSetting => AutomationLiveSetting.Polite;

   /// <summary>Called by MooCodeEditor.UpdateDiagnostics when parsing completes.</summary>
   internal void UpdateDiagnostics(List<ParseMessage> messages)
   {
      _errors   = messages.Where(m => m.Severity == ParseMessageSeverity.Error).ToList();
      _warnings = messages.Where(m => m.Severity == ParseMessageSeverity.Warning).ToList();
   }

   /// <summary>
   /// Returns a range provider that surfaces error/warning annotation attributes
   /// when the caret is within a diagnostic span.
   /// </summary>
   public override ITextRangeProvider[] GetSelection()
   {
      var sel = Tb.Selection;
      return new ITextRangeProvider[]
      {
         new MooCodeEditorRangeProvider(Tb, sel.Start, sel.End, _errors, _warnings)
      };
   }

   public override ITextRangeProvider DocumentRange =>
      new MooCodeEditorRangeProvider(Tb,
         new Place(0, 0),
         new Place(Tb.Lines[Tb.LinesCount - 1].Length, Tb.LinesCount - 1),
         _errors, _warnings);

   /// <summary>Builds the announcement string for a given error and warning count.</summary>
   internal static string BuildAnnouncementString(int errorCount, int warningCount)
   {
      if (errorCount == 0 && warningCount == 0)
         return "No errors";
      if (errorCount > 0 && warningCount == 0)
         return $"{errorCount} syntax {(errorCount == 1 ? "error" : "errors")}";
      if (errorCount == 0 && warningCount > 0)
         return $"{warningCount} {(warningCount == 1 ? "warning" : "warnings")}";
      return $"{errorCount} syntax {(errorCount == 1 ? "error" : "errors")} " +
             $"and {warningCount} {(warningCount == 1 ? "warning" : "warnings")}";
   }
}

/// <summary>
/// Range provider subclass that overlays error/warning annotation attributes from
/// the MooCodeEditor diagnostic list.
/// </summary>
internal class MooCodeEditorRangeProvider : FctbTextRangeProvider
{
   private readonly IReadOnlyList<ParseMessage> _errors;
   private readonly IReadOnlyList<ParseMessage> _warnings;

   private const int UIA_AnnotationTypesAttributeId  = 40031;
   private const int UIA_FullDescriptionAttributeId  = 40035;
   private const int AnnotationType_GrammarError          = 60002;
   private const int AnnotationType_AdvancedProofingIssue = 60017;

   public MooCodeEditorRangeProvider(
      FastColoredTextBox tb, Place start, Place end,
      IReadOnlyList<ParseMessage> errors,
      IReadOnlyList<ParseMessage> warnings)
      : base(tb, start, end)
   {
      _errors   = errors;
      _warnings = warnings;
   }

   protected override object GetAttributeValueImpl(int attribute)
   {
      if (attribute == UIA_AnnotationTypesAttributeId)
      {
         // Return annotation type for the first diagnostic overlapping this range
         foreach (var err in _errors)
            if (OverlapsRange(err)) return AnnotationType_GrammarError;
         foreach (var warn in _warnings)
            if (OverlapsRange(warn)) return AnnotationType_AdvancedProofingIssue;
      }

      if (attribute == UIA_FullDescriptionAttributeId)
      {
         foreach (var err in _errors)
            if (OverlapsRange(err)) return err.Message;
         foreach (var warn in _warnings)
            if (OverlapsRange(warn)) return warn.Message;
      }

      return base.GetAttributeValueImpl(attribute);
   }

   private bool OverlapsRange(ParseMessage msg)
   {
      if (msg.Guide == null)
      {
         // No span info — check by line number only (1-indexed in ParseMessage)
         int msgLine = msg.LineNumber - 1; // convert to 0-indexed
         var (s, e) = Normalized_();
         return msgLine >= s.iLine && msgLine <= e.iLine;
      }
      // Guide has start/end; check if our range intersects
      int msgStartLine = msg.Guide.StartLine - 1;
      int msgEndLine   = msg.Guide.EndLine - 1;
      var (rs, re) = Normalized_();
      return msgStartLine <= re.iLine && msgEndLine >= rs.iLine;
   }

   // Expose Normalized for use in this class (can't call protected method from parent directly)
   private (Place s, Place e) Normalized_()
   {
      bool startFirst = _start.iLine < _end.iLine ||
                        (_start.iLine == _end.iLine && _start.iChar <= _end.iChar);
      return startFirst ? (_start, _end) : (_end, _start);
   }

   public override ITextRangeProvider Clone() =>
      new MooCodeEditorRangeProvider(Tb, _start, _end, _errors, _warnings);
}
```

> Note: `ISyntaxErrorGuide` has `StartLine`, `EndLine`, `StartColumn`, `EndColumn` properties (1-indexed line numbers). Check `Org.Edgerunner.ANTLR4.Tools.Common/Grammar/Errors/ISyntaxErrorGuide.cs` for exact property names before implementing. Adjust if they differ.

- [ ] **Step 2: Verify `ISyntaxErrorGuide` property names**

```bash
cat "D:\Projects\Moo Developer Tools\Org.Edgerunner.ANTLR4.Tools.Common\Grammar\Errors\ISyntaxErrorGuide.cs"
```

Read the output and confirm `StartLine`, `EndLine` property names match. Fix `OverlapsRange` in `MooCodeEditorRangeProvider` if names differ.

- [ ] **Step 3: Run announcement string tests**

```bash
cd "D:\Projects\Moo Developer Tools"
dotnet test "Org.Edgerunner.Moo.Editor.Tests/Org.Edgerunner.Moo.Editor.Tests.csproj" --filter "FullyQualifiedName~DiagnosticAnnouncementTests"
```

Expected: 7 passed, 0 failed.

- [ ] **Step 4: Build solution**

```bash
cd "D:\Projects\Moo Developer Tools"
dotnet build "Moo Developer Tools.sln"
```

Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
cd "D:\Projects\Moo Developer Tools"
git add "Org.Edgerunner.Moo.Editor/Controls/MooCodeEditor.Accessibility.cs" Org.Edgerunner.Moo.Editor.Tests/
git commit -m "Add MooCodeEditorAccessibleObject with error/warning annotations and BuildAnnouncementString"
```

---

## Task 9: MooCodeEditor.Accessibility.cs — Debounced Announcement Timer

**Files:**
- Modify: `Org.Edgerunner.Moo.Editor/Controls/MooCodeEditor.Accessibility.cs`

Adds the 2-second idle timer that announces the diagnostic count after the user stops typing.

- [ ] **Step 1: Add timer fields and wiring to `MooCodeEditorAccessibleObject`**

Inside the `MooCodeEditorAccessibleObject` class, add after the existing fields:

```csharp
private System.Windows.Forms.Timer _announcementTimer;
private int _lastAnnouncedErrorCount   = -1;
private int _lastAnnouncedWarningCount = -1;
```

Add an `InitializeTimer()` method:

```csharp
internal void InitializeTimer()
{
   _announcementTimer = new System.Windows.Forms.Timer { Interval = 2000 };
   _announcementTimer.Tick += AnnouncementTimer_Tick;
   _editor.TextChanged += (_, _) =>
   {
      _announcementTimer.Stop();
      _announcementTimer.Start();
   };
}

private void AnnouncementTimer_Tick(object sender, EventArgs e)
{
   _announcementTimer.Stop();
   int errors   = _errors.Count;
   int warnings = _warnings.Count;
   if (errors == _lastAnnouncedErrorCount && warnings == _lastAnnouncedWarningCount)
      return;
   _lastAnnouncedErrorCount   = errors;
   _lastAnnouncedWarningCount = warnings;
   AccessibleDescription = BuildAnnouncementString(errors, warnings);
   RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
}
```

- [ ] **Step 2: Call `InitializeTimer()` from `CreateAccessibilityInstance()` in `MooCodeEditor.Accessibility.cs`**

In the partial class's `CreateAccessibilityInstance()`, change:

```csharp
protected override AccessibleObject CreateAccessibilityInstance()
{
   if (!_editorAccessibilityInitialized)
   {
      _editorAccessibilityInitialized = true;
   }
   return new MooCodeEditorAccessibleObject(this);
}
```

To:

```csharp
protected override AccessibleObject CreateAccessibilityInstance()
{
   if (!_editorAccessibilityInitialized)
   {
      _editorAccessibilityInitialized = true;
   }
   var acc = new MooCodeEditorAccessibleObject(this);
   acc.InitializeTimer();
   return acc;
}
```

- [ ] **Step 3: Build to verify**

```bash
cd "D:\Projects\Moo Developer Tools"
dotnet build "Org.Edgerunner.Moo.Editor/Org.Edgerunner.Moo.Editor.csproj"
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
cd "D:\Projects\Moo Developer Tools"
git add "Org.Edgerunner.Moo.Editor/Controls/MooCodeEditor.Accessibility.cs"
git commit -m "Add 2-second debounced diagnostic count announcement to MooCodeEditor accessibility"
```

---

## Task 10: Wire MooCodeEditor.cs + Final Build Verification

**Files:**
- Modify: `Org.Edgerunner.Moo.Editor/Controls/MooCodeEditor.cs`

The last connection: hook the existing `ParsingComplete` event to call `UpdateDiagnostics()` on the accessible object.

- [ ] **Step 1: Find where ParsingComplete is fired in `MooCodeEditor.cs`**

```bash
grep -n "ParsingComplete\|OnParsingComplete\|RaiseParsingComplete\|ErrorMessages" "D:\Projects\Moo Developer Tools\Org.Edgerunner.Moo.Editor\Controls\MooCodeEditor.cs" | head -20
```

Read the output to find the exact line where parsing results are available and `ParsingComplete` is raised.

- [ ] **Step 2: Add `UpdateDiagnostics` call after parsing**

In `MooCodeEditor.cs`, find the method that raises `ParsingComplete` (it receives the `List<ParseMessage>` error messages). After the existing `ParsingComplete?.Invoke(...)` call, add:

```csharp
UpdateDiagnostics(errorMessages);
```

Where `errorMessages` is the `List<ParseMessage>` available at that point. The exact variable name depends on what you find in Step 1.

- [ ] **Step 3: Verify `ISyntaxErrorGuide` property names match usage in MooCodeEditorRangeProvider**

Read `Org.Edgerunner.ANTLR4.Tools.Common/Grammar/Errors/ISyntaxErrorGuide.cs` and confirm the property names used in `OverlapsRange()` in Task 8 are correct. Fix if needed.

- [ ] **Step 4: Run all tests**

```bash
cd "D:\Projects\Moo Developer Tools"
dotnet test "Org.Edgerunner.Moo.Editor.Tests/Org.Edgerunner.Moo.Editor.Tests.csproj"
```

Expected: All tests pass.

- [ ] **Step 5: Build full solution**

```bash
cd "D:\Projects\Moo Developer Tools"
dotnet build "Moo Developer Tools.sln"
```

Expected: `Build succeeded. 0 Error(s).`

- [ ] **Step 6: Manual verification with Inspect.exe**

Install Windows SDK if not already present. Open **Inspect.exe** (found in `C:\Program Files (x86)\Windows Kits\10\bin\<version>\x64\`).

1. Launch Moo Udditor
2. Open Inspect.exe
3. Click on the `MooCodeEditor` control
4. In Inspect, verify:
   - `Control Type` = `Document`
   - `Patterns` includes `TextPattern`
   - `Live Setting` = `Polite`
5. Click on the `ConsoleWindowEmulator` control
6. Verify `Live Setting` = `Polite`
7. In Inspect's tree, select a line in the code editor and verify `Text` attribute shows the line content

- [ ] **Step 7: Commit**

```bash
cd "D:\Projects\Moo Developer Tools"
git add "Org.Edgerunner.Moo.Editor/Controls/MooCodeEditor.cs"
git commit -m "Wire MooCodeEditor.ParsingComplete to UpdateDiagnostics for screen reader announcements"
```

---

## Self-Review

**Spec coverage:**
- ✅ `ParseMessage.Severity` (Task 1)
- ✅ `UIAutomationProvider` NuGet (Task 2)
- ✅ `FctbAccessibleObject` with `ITextProvider` (Task 3)
- ✅ `FctbTextRangeProvider` — GetText, Clone, Compare, BoundingRects, Select, Scroll (Task 4)
- ✅ `FctbTextRangeProvider` — Move, Expand, MoveEndpoint (Task 5)
- ✅ `FctbTextRangeProvider` — GetAttributeValue, FindText (Task 6)
- ✅ `ConsoleAccessibleObject` with `LiveSetting.Polite` + LiveRegionChanged (Task 7)
- ✅ `MooCodeEditorAccessibleObject` + `BuildAnnouncementString` + annotation attributes (Task 8)
- ✅ 2-second debounce timer + announcement (Task 9)
- ✅ `MooCodeEditor.cs` wiring + Inspect.exe verification (Task 10)

**Deferred (out of scope per spec):**
- `IScrollProvider` pattern
- High-contrast theme
- Multi-selection UIA support
