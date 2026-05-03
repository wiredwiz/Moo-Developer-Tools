# UIA Provider Fix Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix UI Automation so screen readers see Document+TextPattern instead of Pane+LegacyIAccessible, enabling text navigation and live region announcements in both the console emulator and code editor windows.

**Architecture:** Three targeted changes: (1) replace the raw `UiaReturnRawElementProvider` P/Invoke in `FastColoredTextBox.cs` with the managed `AutomationInteropProvider.ReturnRawElementProvider` and remove the now-redundant P/Invoke declaration; (2) remove the `_liveRegionReady` guard in `ConsoleWindowEmulator` that blocks live region events until after first screen reader contact; (3) fix `MooCodeEditor.Accessibility.cs` so `UpdateDiagnostics` triggers the announcement timer on first parse, not just on subsequent text changes.

**Tech Stack:** .NET 6 / C# / WinForms, UIAutomationProvider NuGet (already referenced), Accessibility Insights for Windows for verification.

**Branch:** All work goes on the `Accessibility` branch. Use a git worktree.

**Worktree setup:**
```bash
cd "D:\Projects\Moo Developer Tools"
git stash  # stash any uncommitted changes first
git worktree add .worktrees/fix-uia-provider -b fix/uia-provider Accessibility
```

---

## Key Types Reference

```
FastColoredTextBox.Accessibility.cs  — partial FastColoredTextBox class with:
  WM_GETOBJECT = 0x003D              — Windows message for accessibility object queries
  OBJID_CLIENT = unchecked((int)0xFFFFFFFC)
  UiaReturnRawElementProvider P/Invoke   — REMOVE THIS (lines 20-22)
  FctbAccessibleObject               — implements ITextProvider, IRawElementProviderSimple
  FctbUiaProviderBridge              — non-FTM wrapper; passed to ReturnRawElementProvider

FastColoredTextBox.cs line 3109:
  m.Result = UiaReturnRawElementProvider(Handle, m.WParam, m.LParam, bridge);
  ↳ Change to: AutomationInteropProvider.ReturnRawElementProvider(...)

ConsoleWindowEmulator.Accessibility.cs:
  _liveRegionReady                   — bool field; guards live region events (REMOVE)
  OnHandleCreated                    — hooks TextChanged; has _liveRegionReady guard (REMOVE guard)
  CreateAccessibilityInstance        — sets _liveRegionReady = true (REMOVE that line)

MooCodeEditor.Accessibility.cs:
  UpdateDiagnostics(List<ParseMessage>) — updates error/warning lists; does NOT start timer
  _announcementTimer                 — initialized in InitializeTimer(); may be null if no reader connected
```

---

## File Map

| File | Action |
|---|---|
| `FastColoredTextBox/FastColoredTextBox.Accessibility.cs` | Remove `UiaReturnRawElementProvider` P/Invoke (lines 20-22) |
| `FastColoredTextBox/FastColoredTextBox.cs` | Change P/Invoke call to `AutomationInteropProvider.ReturnRawElementProvider` (line 3109) |
| `Org.Edgerunner.Moo.Editor/Controls/ConsoleWindowEmulator.Accessibility.cs` | Remove `_liveRegionReady` field and guard |
| `Org.Edgerunner.Moo.Editor/Controls/MooCodeEditor.Accessibility.cs` | Start announcement timer in `UpdateDiagnostics` |

---

## Task 1: Switch to Managed UIA Provider Registration

**Context:** `UiaReturnRawElementProvider` is a raw P/Invoke. `AutomationInteropProvider.ReturnRawElementProvider` is the managed equivalent already available via the `UIAutomationProvider` NuGet (already referenced). The `System.Windows.Automation.Provider` namespace is already imported in both affected files.

**Files:**
- Modify: `FastColoredTextBox/FastColoredTextBox.Accessibility.cs` — remove P/Invoke lines 20-22
- Modify: `FastColoredTextBox/FastColoredTextBox.cs` — change call at line 3109

- [ ] **Step 1: Remove the `UiaReturnRawElementProvider` P/Invoke from FastColoredTextBox.Accessibility.cs**

Read the file, locate lines 20-22:
```csharp
[DllImport("UIAutomationCore.dll", SetLastError = false)]
private static extern IntPtr UiaReturnRawElementProvider(
   IntPtr hwnd, IntPtr wParam, IntPtr lParam, IRawElementProviderSimple el);
```
Delete those three lines. The `WM_GETOBJECT` and `OBJID_CLIENT` constants on lines 17-18 stay — they are still used by `FastColoredTextBox.cs`. The `UiaHostProviderFromHwnd` P/Invoke in `FctbAccessibleObject` (lines 94-95) also stays — it is a different function still needed.

The `using System.Runtime.InteropServices;` at the top of the file stays — still needed for the `UiaHostProviderFromHwnd` DllImport inside `FctbAccessibleObject`.

- [ ] **Step 2: Update the call in FastColoredTextBox.cs**

In `FastColoredTextBox.cs` at line 3109, change:
```csharp
m.Result = UiaReturnRawElementProvider(Handle, m.WParam, m.LParam, bridge);
```
to:
```csharp
m.Result = AutomationInteropProvider.ReturnRawElementProvider(Handle, m.WParam, m.LParam, bridge);
```

`AutomationInteropProvider` lives in `System.Windows.Automation.Provider`. Confirm that namespace is already imported at the top of `FastColoredTextBox.cs`. If not, add:
```csharp
using System.Windows.Automation.Provider;
```

The surrounding WndProc block (lines 3097-3112) should now look like:
```csharp
protected override void WndProc(ref Message m)
{
   // Return our UIA provider for WM_GETOBJECT, bypassing WinForms's
   // SupportsUiaProviders gate (which is false for custom UserControls).
   // FctbAccessibleObject inherits StandardOleMarshalObject (FTM via AccessibleObject),
   // which causes UIAutomationCore to discard the provider during cross-process setup.
   // We pass FctbUiaProviderBridge — a plain non-FTM wrapper — instead.
   if (m.Msg == WM_GETOBJECT && (int)(long)m.LParam == OBJID_CLIENT && IsHandleCreated)
   {
      if (AccessibilityObject is FctbAccessibleObject fctbProvider)
      {
         var bridge = new FctbUiaProviderBridge(fctbProvider);
         m.Result = AutomationInteropProvider.ReturnRawElementProvider(Handle, m.WParam, m.LParam, bridge);
         return;
      }
   }
   // ... rest of WndProc unchanged
```

- [ ] **Step 3: Build to verify no errors**

```bash
cd "D:\Projects\Moo Developer Tools\.worktrees\fix-uia-provider"
dotnet build "Moo Developer Tools.sln" 2>&1 | grep -E "error CS|Error\b|succeeded|failed" | grep -v warning
```
Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 4: Verify with Accessibility Insights for Windows**

Run the app from the worktree's bin directory:
```
.worktrees\fix-uia-provider\Org.Edgerunner.Moo.Udditor\bin\Debug\net6.0-windows\Moo Udditor.exe
```

Open Accessibility Insights → hover tool → click directly on the console output area or code editor text area.

**Expected after this fix:**
- Control Type: **Document (50006)**
- Patterns: **TextPattern** (or TextEditPattern for the code editor), ScrollPattern, LegacyIAccessiblePattern

**If still Pane:** AIW may be selecting a Krypton container rather than the FCTB itself. In AIW, use the element tree (left panel) to drill down: find the window → navigate into nested containers → locate the element whose Name matches the editor/console content.

- [ ] **Step 5: Commit**

```bash
cd "D:\Projects\Moo Developer Tools\.worktrees\fix-uia-provider"
git add FastColoredTextBox/FastColoredTextBox.Accessibility.cs FastColoredTextBox/FastColoredTextBox.cs
git commit -m "Switch UiaReturnRawElementProvider P/Invoke to managed AutomationInteropProvider

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>"
```

---

## Task 2: Remove _liveRegionReady Guard from ConsoleWindowEmulator

**Context:** `_liveRegionReady` was added to prevent live region events from firing before a screen reader connects. But once UIA is working (Task 1), the screen reader subscribes to events at connection time — not at focus time. The guard has the opposite effect: it silences events until the user has already manually focused the console with a screen reader, which may never happen for a read-only output pane. Removing it means `FireLiveRegionChangedEvent()` is called on every `TextChanged`, which is a no-op if no reader is listening.

**File:**
- Modify: `Org.Edgerunner.Moo.Editor/Controls/ConsoleWindowEmulator.Accessibility.cs`

Current file content of the `ConsoleWindowEmulator` partial class section:
```csharp
public partial class ConsoleWindowEmulator
{
   private bool _liveRegionReady;

   private static readonly MethodInfo _raiseAutomationEventMethod = ...

   protected override void OnHandleCreated(EventArgs e)
   {
      base.OnHandleCreated(e);
      TextChanged += (_, _) =>
      {
         if (_liveRegionReady)
            FireLiveRegionChangedEvent();
      };
   }

   protected override AccessibleObject CreateAccessibilityInstance()
   {
      EnsureUiaEventHooks();
      _liveRegionReady = true;
      return new ConsoleAccessibleObject(this);
   }
   ...
```

- [ ] **Step 1: Remove `_liveRegionReady` field and all references**

Replace the partial class section with:
```csharp
public partial class ConsoleWindowEmulator
{
   private static readonly MethodInfo _raiseAutomationEventMethod =
      typeof(AccessibleObject).GetMethod("RaiseAutomationEvent",
         BindingFlags.NonPublic | BindingFlags.Instance);

   protected override void OnHandleCreated(EventArgs e)
   {
      base.OnHandleCreated(e);
      TextChanged += (_, _) => FireLiveRegionChangedEvent();
   }

   protected override AccessibleObject CreateAccessibilityInstance()
   {
      EnsureUiaEventHooks();
      return new ConsoleAccessibleObject(this);
   }

   internal void FireLiveRegionChangedEvent()
   {
      var ao = AccessibilityObject;
      if (_raiseAutomationEventMethod == null || ao == null) return;
      try
      {
         var param = _raiseAutomationEventMethod.GetParameters()[0];
         var enumVal = Enum.ToObject(param.ParameterType, 19996); // UIA_LiveRegionChangedEventId
         _raiseAutomationEventMethod.Invoke(ao, new object[] { enumVal });
      }
      catch (Exception) { /* best effort */ }
   }
}
```

- [ ] **Step 2: Build to verify**

```bash
cd "D:\Projects\Moo Developer Tools\.worktrees\fix-uia-provider"
dotnet build "Moo Developer Tools.sln" 2>&1 | grep -E "error CS|Error\b|succeeded|failed" | grep -v warning
```
Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 3: Verify live region with NVDA or Narrator**

Run the app, connect to a MUD server. With NVDA or Narrator running:
1. Focus the application window.
2. Incoming server text should be announced automatically without needing to navigate to the console area first.

- [ ] **Step 4: Commit**

```bash
git add Org.Edgerunner.Moo.Editor/Controls/ConsoleWindowEmulator.Accessibility.cs
git commit -m "Remove _liveRegionReady guard: fire live region events as soon as text arrives

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>"
```

---

## Task 3: Fix MooCodeEditor Diagnostic Announcement on First Parse

**Context:** `InitializeTimer()` hooks `TextChanged` to restart the 2-second debounce timer. But when an editor opens with pre-existing verb code (the common case for local-edit), there is no `TextChanged` event — only a `ParsingComplete` event fires. `UpdateDiagnostics` receives the parse results but never starts the timer. The announcement never fires. Fix: start the timer inside `UpdateDiagnostics` so a fresh parse result also triggers an announcement.

**File:**
- Modify: `Org.Edgerunner.Moo.Editor/Controls/MooCodeEditor.Accessibility.cs`

Current `UpdateDiagnostics` method:
```csharp
internal void UpdateDiagnostics(List<ParseMessage> messages)
{
   _errors   = messages.Where(m => m.Severity == ParseMessageSeverity.Error).ToList();
   _warnings = messages.Where(m => m.Severity == ParseMessageSeverity.Warning).ToList();
}
```

- [ ] **Step 1: Update `UpdateDiagnostics` to start the announcement timer**

Replace with:
```csharp
internal void UpdateDiagnostics(List<ParseMessage> messages)
{
   _errors   = messages.Where(m => m.Severity == ParseMessageSeverity.Error).ToList();
   _warnings = messages.Where(m => m.Severity == ParseMessageSeverity.Warning).ToList();
   // Trigger announcement for first-load parses where TextChanged never fires.
   // Null-guard: timer is only created when a screen reader connects.
   if (_announcementTimer != null)
   {
      _announcementTimer.Stop();
      _announcementTimer.Start();
   }
}
```

- [ ] **Step 2: Build to verify**

```bash
cd "D:\Projects\Moo Developer Tools\.worktrees\fix-uia-provider"
dotnet build "Moo Developer Tools.sln" 2>&1 | grep -E "error CS|Error\b|succeeded|failed" | grep -v warning
```
Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 3: Verify with NVDA or Narrator**

Run the app. With NVDA or Narrator running, open a verb for local edit (triggers `CreateMooCodeEditorPage` with source code). Within ~2 seconds of the editor opening, the screen reader should announce the diagnostic summary (e.g. "No errors" or "3 syntax errors").

- [ ] **Step 4: Commit**

```bash
git add Org.Edgerunner.Moo.Editor/Controls/MooCodeEditor.Accessibility.cs
git commit -m "Trigger diagnostic announcement timer on first parse, not only on TextChanged

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>"
```

---

## Task 4: Merge to Accessibility Branch and Push

- [ ] **Step 1: Merge worktree branch into Accessibility**

```bash
cd "D:\Projects\Moo Developer Tools"
git checkout Accessibility
git merge fix/uia-provider --no-ff -m "Fix UIA provider registration, live regions, and first-load diagnostics

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>"
```

- [ ] **Step 2: Remove worktree and branch**

```bash
git worktree remove .worktrees/fix-uia-provider
git branch -d fix/uia-provider
```

- [ ] **Step 3: Push Accessibility branch**

```bash
git push origin Accessibility
```

- [ ] **Step 4: Final AIW verification**

With the app running from the merged Accessibility branch build:
1. Open AIW hover tool → click on console output area → confirm **Control Type: Document, Patterns: TextPattern**
2. Open AIW hover tool → click on code editor area → confirm **Control Type: Document, Patterns: TextEditPattern** (or TextPattern)
3. With NVDA running: use arrow keys in a code editor → screen reader should read each line/character
4. With NVDA running: receive server text → screen reader should announce it automatically

---

## Self-Review Notes

- Task 1 removes the P/Invoke but keeps `WM_GETOBJECT`/`OBJID_CLIENT` constants — these are still used in `FastColoredTextBox.cs` WndProc via the same partial class ✓
- `UiaHostProviderFromHwnd` P/Invoke in `FctbAccessibleObject` is separate and untouched ✓
- `_announcementTimer` null-guard in Task 3 is necessary: timer is only created when `CreateAccessibilityInstance` runs, which only happens when a screen reader connects ✓
- All work stays on the `Accessibility` branch — no changes to `master` ✓
- `using System.Runtime.InteropServices` stays in `FastColoredTextBox.Accessibility.cs` even after P/Invoke removal, because `FctbAccessibleObject.UiaHostProviderFromHwnd` still needs `[DllImport]` ✓
