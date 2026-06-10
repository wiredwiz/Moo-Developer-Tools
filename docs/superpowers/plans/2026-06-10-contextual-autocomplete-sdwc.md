# Contextual SDWC Autocomplete (udd-7g2) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** When the user types `$`, `:` or `.` in a connected verb editor, query the world via SDWC and offer verb/property/core-reference completion items with their proper icons (purple Verb, cyan Property, green CoreReference).

**Architecture:** Pure, headless-testable units in `Org.Edgerunner.Moo.Editor/Autocomplete` (context detector, operand resolver, completion item, async controller, dynamic item source) plumbed into `MooCodeEditorPage.BuildAutocompleteMenu`, plus a small object-reference parser in `Org.Edgerunner.Mud.Common`. The forked FastColoredTextBox `AutocompleteMenu` re-enumerates its `sourceItems` (`IEnumerable<AutocompleteItem>`) on every `DoAutocomplete` call, so a lazy enumerable that injects current member items in front of the static items is the supported insertion point — no FCTB changes needed.

**Tech Stack:** .NET 6 Windows (`net6.0-windows`), WinForms, xunit + FluentAssertions for tests, SDWC `IMooWorldQueryProvider` backbone (already merged and testable).

**Spec:** `docs/superpowers/specs/2026-06-09-contextual-autocomplete-sdwc-design.md`

---

## Critical constraints (read first)

1. **NEVER run a test that instantiates a WinForms control** (e.g. `MooCodeEditor`, `FastColoredTextBox`, `AutocompleteMenu`). It spawns a GUI test host that crashes/hangs. All new tests are pure-logic only. Always run tests with the exact `--filter` arguments given in each step — never bare `dotnet test` on a whole project.
2. **WinForms/GUI layers get build-only verification.** The user smoke-tests live.
3. **Every new `.cs` file gets the BSD 3-Clause header block.** Copy the `#region BSD 3-Clause License … #endregion` block verbatim from a sibling file in the same project (e.g. `Org.Edgerunner.Mud.Common/Querying/MooObjectId.cs`), changing only the `file="…"` attribute to the new file name.
4. **Indentation:** `Org.Edgerunner.Mud.Common` and `Org.Edgerunner.Moo.Editor` use **3-space** indents. `Org.Edgerunner.Moo.Udditor` pages use **4-space** indents. Match the file you are in.
5. **Commit after every task.** Work happens in the dedicated worktree branch.
6. **Beads:** the bead `udd-7g2` is already claimed. Do not close it mid-plan; the final task closes it BEFORE the final commit so the `.beads/issues.jsonl` change lands in the same commit.

## Key reference facts (verified against the codebase)

- `AutocompleteMenu.Items` is an `AutocompleteListView`; `Items.SetAutocompleteItems(IEnumerable<AutocompleteItem>)` (`FastColoredTextBox/AutocompleteMenu.cs:810`) stores the enumerable and `DoAutocomplete` re-enumerates it on every popup refresh (`AutocompleteMenu.cs:469`). Lazy enumerables work.
- `AutocompleteMenu.Show(bool forced)` is public and re-runs `DoAutocomplete` (`AutocompleteMenu.cs:141`).
- `AutocompleteItem` (`FastColoredTextBox/Types/AutocompleteItem.cs`) is a plain class (NOT a Control — safe in headless tests). Relevant members: `Text` (public field), `ImageIndex` (public field), ctor `(string text, int imageIndex)`, virtual `Compare(string fragmentText)`, virtual `GetTextForReplace()`. The upstream `MethodAutocompleteItem` (same file, line 175) stores the fragment prefix during `Compare` and reuses it in `GetTextForReplace` — we follow that exact pattern.
- The popup fragment is taken from `SearchPattern` char class; current pattern in `MooCodeEditorPage.BuildAutocompleteMenu` is `@"[\w\.:=!<>+-/*%&|^]"` (`MooCodeEditorPage.cs:277`). It lacks `$` and `#`, which we must add so fragments like `$fo` and `#123:te` reach `Compare`.
- `MooObjectId` is a readonly struct with `int Number`, ctor `(int number)`, `ToString()` → `#n` (`Org.Edgerunner.Mud.Common/Querying/MooObjectId.cs`).
- `MooVerbSummary(IReadOnlyList<string> Aliases, MooObjectId DefiningObject)` and `MooPropertySummary(string Name, MooObjectId DefiningObject)` records (`Org.Edgerunner.Mud.Common/Querying/QueryModels.cs:52,59`).
- `IClientTerminal.QueryProviders` → `MooWorldQueryService`; `.Query` → the caching `IMooWorldQueryProvider` (`MooWorldQueryService.cs:71`).
- `MooCodeEditorPage.QueryProvider` (nullable hook) already exists at `MooCodeEditorPage.cs:63`.
- `CompletionIconCategory`: `CoreReference = 2`, `Verb = 3`, `Property = 4`; enum value == ImageIndex (`Org.Edgerunner.Moo.Editor/Autocomplete/CompletionIconCategory.cs`).
- `FastColoredTextBox.GetLineText(int iLine)` exists (`FastColoredTextBox.cs:6580`); caret is `editor.Selection.Start` (a `Place` with `iChar`/`iLine`).
- `Org.Edgerunner.Moo.Editor` does **not** reference NLog — the controller swallows fetch failures silently (best-effort completion), no logging dependency added.
- Test projects use xunit 2.7 + FluentAssertions 6.12. `Org.Edgerunner.Mud.Common.Tests` has a `Querying/` folder; `Org.Edgerunner.Moo.Editor.Tests` has flat test files.

---

### Task 1: Object-reference parser (`MooObjectReferenceParser`)

Extracts the first `#<n>` object id from an upload command (`@program #123:verbname`) or simpleedit reference. Lives in Mud.Common so it is testable and reusable by both Udditor wiring points.

**Files:**
- Create: `Org.Edgerunner.Mud.Common/Querying/MooObjectReferenceParser.cs`
- Test: `Org.Edgerunner.Mud.Common.Tests/Querying/MooObjectReferenceParserTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `Org.Edgerunner.Mud.Common.Tests/Querying/MooObjectReferenceParserTests.cs` (3-space indent; look at an existing file in that folder and match its using/namespace style):

```csharp
using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
using Xunit;

namespace Org.Edgerunner.Mud.Common.Tests.Querying;

public class MooObjectReferenceParserTests
{
   [Theory]
   [InlineData("@program #123:verbname", 123)]
   [InlineData("#0:tell", 0)]
   [InlineData("@program #-1:foo", -1)]
   [InlineData("prefix text #42:bar suffix #99", 42)]
   public void FindFirstObjectId_returns_first_object_number(string text, int expected)
   {
      var result = MooObjectReferenceParser.FindFirstObjectId(text);

      result.Should().Be(new MooObjectId(expected));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("@edit foo:bar")]
   [InlineData("# 5 (space after hash)")]
   [InlineData("no references here")]
   public void FindFirstObjectId_returns_null_when_no_object_reference_present(string? text)
   {
      var result = MooObjectReferenceParser.FindFirstObjectId(text);

      result.Should().BeNull();
   }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test Org.Edgerunner.Mud.Common.Tests --filter "FullyQualifiedName~MooObjectReferenceParserTests"`
Expected: FAIL to compile — `MooObjectReferenceParser` does not exist.

- [ ] **Step 3: Write the implementation**

Create `Org.Edgerunner.Mud.Common/Querying/MooObjectReferenceParser.cs` (BSD header from `MooObjectId.cs`, 3-space indent):

```csharp
using System.Text.RegularExpressions;

namespace Org.Edgerunner.Mud.Common.Querying;

/// <summary>
/// Extracts MOO object references (<c>#n</c>) from free-form text such as local-edit upload
/// commands (<c>@program #123:verbname</c>) or simpleedit references.
/// </summary>
public static class MooObjectReferenceParser
{
   private static readonly Regex ObjectIdPattern = new(@"#(-?\d+)", RegexOptions.Compiled);

   /// <summary>
   /// Finds the first <c>#n</c> object reference in the supplied text.
   /// </summary>
   /// <param name="text">The text to scan. May be <c>null</c> or empty.</param>
   /// <returns>The first object id found, or <c>null</c> when the text contains none.</returns>
   public static MooObjectId? FindFirstObjectId(string? text)
   {
      if (string.IsNullOrEmpty(text))
         return null;

      var match = ObjectIdPattern.Match(text);
      return match.Success && int.TryParse(match.Groups[1].Value, out var number)
                ? new MooObjectId(number)
                : null;
   }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test Org.Edgerunner.Mud.Common.Tests --filter "FullyQualifiedName~MooObjectReferenceParserTests"`
Expected: PASS (9 tests).

- [ ] **Step 5: Commit**

```bash
git add Org.Edgerunner.Mud.Common/Querying/MooObjectReferenceParser.cs Org.Edgerunner.Mud.Common.Tests/Querying/MooObjectReferenceParserTests.cs
git commit -m "feat: add MooObjectReferenceParser for #n extraction (udd-7g2)"
```

---

### Task 2: Member-completion context detector

Pure classifier: given the text left of the caret on the current line, decide whether we are in a `$` (core reference), `:` (verb) or `.` (property) completion context, and extract the operand.

**Files:**
- Create: `Org.Edgerunner.Moo.Editor/Autocomplete/MemberCompletionContext.cs`
- Create: `Org.Edgerunner.Moo.Editor/Autocomplete/MemberCompletionContextDetector.cs`
- Test: `Org.Edgerunner.Moo.Editor.Tests/MemberCompletionContextDetectorTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `Org.Edgerunner.Moo.Editor.Tests/MemberCompletionContextDetectorTests.cs`:

```csharp
using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class MemberCompletionContextDetectorTests
{
   [Theory]
   [InlineData("$", "")]
   [InlineData("$fo", "")]
   [InlineData("x = $roo", "")]
   public void Detect_classifies_core_reference_context(string linePrefix, string expectedOperand)
   {
      var context = MemberCompletionContextDetector.Detect(linePrefix);

      context.Kind.Should().Be(MemberContextKind.CoreReference);
      context.Operand.Should().Be(expectedOperand);
   }

   [Theory]
   [InlineData("this:", "this")]
   [InlineData("this:te", "this")]
   [InlineData("#123:tell", "#123")]
   [InlineData("x = obj:fr", "obj")]
   [InlineData("$foo:bar", "$foo")]
   public void Detect_classifies_verb_context(string linePrefix, string expectedOperand)
   {
      var context = MemberCompletionContextDetector.Detect(linePrefix);

      context.Kind.Should().Be(MemberContextKind.Verb);
      context.Operand.Should().Be(expectedOperand);
   }

   [Theory]
   [InlineData("this.", "this")]
   [InlineData("this.loc", "this")]
   [InlineData("#0.na", "#0")]
   [InlineData("foo.bar", "foo")]
   [InlineData("this.location.na", "location")]
   public void Detect_classifies_property_context(string linePrefix, string expectedOperand)
   {
      var context = MemberCompletionContextDetector.Detect(linePrefix);

      context.Kind.Should().Be(MemberContextKind.Property);
      context.Operand.Should().Be(expectedOperand);
   }

   [Theory]
   [InlineData("")]
   [InlineData("x = 5")]
   [InlineData("for x in [1..5")]    // range operator, not property access
   [InlineData("1.5")]               // float literal
   [InlineData("x = \"a string with $foo")]   // inside a string
   [InlineData("x = \"obj:verb")]              // inside a string
   [InlineData("player:tell(\"this.")]         // inside a string argument
   public void Detect_returns_none_for_non_member_contexts(string linePrefix)
   {
      var context = MemberCompletionContextDetector.Detect(linePrefix);

      context.Kind.Should().Be(MemberContextKind.None);
   }

   [Fact]
   public void Detect_treats_escaped_quotes_as_string_content()
   {
      // The string "say \"hi\" to" is still open: the $ is inside it.
      var context = MemberCompletionContextDetector.Detect("x = \"say \\\"hi\\\" to $fo");

      context.Kind.Should().Be(MemberContextKind.None);
   }

   [Fact]
   public void Detect_recognizes_member_context_after_closed_string()
   {
      var context = MemberCompletionContextDetector.Detect("x = \"done\"; this:te");

      context.Kind.Should().Be(MemberContextKind.Verb);
      context.Operand.Should().Be("this");
   }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test Org.Edgerunner.Moo.Editor.Tests --filter "FullyQualifiedName~MemberCompletionContextDetectorTests"`
Expected: FAIL to compile — types do not exist.

- [ ] **Step 3: Write the context types**

Create `Org.Edgerunner.Moo.Editor/Autocomplete/MemberCompletionContext.cs` (BSD header; 3-space indent):

```csharp
namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// The kind of member completion context detected at the caret.
/// </summary>
public enum MemberContextKind
{
   /// <summary>Not a member completion position; only static completion applies.</summary>
   None,

   /// <summary>A core reference (<c>$foo</c>): completes properties of object <c>#0</c>.</summary>
   CoreReference,

   /// <summary>A verb call (<c>obj:verb</c>): completes verbs of the operand object.</summary>
   Verb,

   /// <summary>A property access (<c>obj.prop</c>): completes properties of the operand object.</summary>
   Property
}

/// <summary>
/// The member completion context detected from the text left of the caret.
/// </summary>
/// <param name="Kind">The context kind.</param>
/// <param name="Operand">The operand text left of the trigger character (empty for core references).</param>
public readonly record struct MemberCompletionContext(MemberContextKind Kind, string Operand)
{
   /// <summary>Gets the "no member context" value.</summary>
   public static MemberCompletionContext None => new(MemberContextKind.None, string.Empty);
}
```

- [ ] **Step 4: Write the detector**

Create `Org.Edgerunner.Moo.Editor/Autocomplete/MemberCompletionContextDetector.cs` (BSD header; 3-space indent):

```csharp
using System.Text.RegularExpressions;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// Classifies the member completion context from the text left of the caret on the current line.
/// </summary>
/// <remarks>
/// Only syntactically adjacent operands are recognized (<c>obj:</c>, <c>#123.</c>, <c>$frag</c>).
/// Chained expressions resolve to their last segment (a bareword, which the resolver will reject),
/// range operators (<c>..</c>) and float literals never match, and positions inside an open string
/// are never member contexts. MOO has no comment syntax inside verbs, so only strings are tracked.
/// </remarks>
public static class MemberCompletionContextDetector
{
   // <operand><separator><partial-member> anchored at the caret. Operand forms: bareword/keyword
   // (this, foo), core reference ($foo) or object literal (#123 / #-1).
   private static readonly Regex MemberPattern =
      new(@"(\$?[A-Za-z_]\w*|#-?\d+)([:.])\w*$", RegexOptions.Compiled);

   // A core-reference fragment ($ or $partialname) anchored at the caret.
   private static readonly Regex CoreRefPattern = new(@"\$\w*$", RegexOptions.Compiled);

   /// <summary>
   /// Detects the member completion context for the supplied line prefix.
   /// </summary>
   /// <param name="linePrefix">The text on the caret line, from column 0 up to the caret.</param>
   /// <returns>The detected context; <see cref="MemberCompletionContext.None"/> when not a member position.</returns>
   public static MemberCompletionContext Detect(string linePrefix)
   {
      if (string.IsNullOrEmpty(linePrefix) || IsInsideString(linePrefix))
         return MemberCompletionContext.None;

      var member = MemberPattern.Match(linePrefix);
      if (member.Success)
      {
         var kind = member.Groups[2].Value == ":" ? MemberContextKind.Verb : MemberContextKind.Property;
         return new MemberCompletionContext(kind, member.Groups[1].Value);
      }

      if (CoreRefPattern.IsMatch(linePrefix))
         return new MemberCompletionContext(MemberContextKind.CoreReference, string.Empty);

      return MemberCompletionContext.None;
   }

   private static bool IsInsideString(string linePrefix)
   {
      var inString = false;
      for (var i = 0; i < linePrefix.Length; i++)
      {
         var ch = linePrefix[i];
         if (inString && ch == '\\')
         {
            i++; // skip the escaped character
            continue;
         }

         if (ch == '"')
            inString = !inString;
      }

      return inString;
   }
}
```

- [ ] **Step 5: Run tests to verify they pass**

Run: `dotnet test Org.Edgerunner.Moo.Editor.Tests --filter "FullyQualifiedName~MemberCompletionContextDetectorTests"`
Expected: PASS (all theory cases).

Note: the `"for x in [1..5"` case passes because `1` cannot match the operand pattern (barewords must start with a letter/underscore; object literals need `#`), so `..`/float text falls through to `None`.

- [ ] **Step 6: Commit**

```bash
git add Org.Edgerunner.Moo.Editor/Autocomplete/MemberCompletionContext.cs Org.Edgerunner.Moo.Editor/Autocomplete/MemberCompletionContextDetector.cs Org.Edgerunner.Moo.Editor.Tests/MemberCompletionContextDetectorTests.cs
git commit -m "feat: add member completion context detector (udd-7g2)"
```

---

### Task 3: Operand object resolver

Pure mapping from (context, page's edited-object id) to a queryable `MooObjectId?`.

**Files:**
- Create: `Org.Edgerunner.Moo.Editor/Autocomplete/MemberOperandResolver.cs`
- Test: `Org.Edgerunner.Moo.Editor.Tests/MemberOperandResolverTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `Org.Edgerunner.Moo.Editor.Tests/MemberOperandResolverTests.cs`:

```csharp
using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Org.Edgerunner.Mud.Common.Querying;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class MemberOperandResolverTests
{
   [Fact]
   public void Resolve_core_reference_returns_object_zero()
   {
      var context = new MemberCompletionContext(MemberContextKind.CoreReference, string.Empty);

      var result = MemberOperandResolver.Resolve(context, null);

      result.Should().Be(new MooObjectId(0));
   }

   [Theory]
   [InlineData("#123", 123)]
   [InlineData("#0", 0)]
   [InlineData("#-1", -1)]
   public void Resolve_object_literal_returns_its_number(string operand, int expected)
   {
      var context = new MemberCompletionContext(MemberContextKind.Verb, operand);

      var result = MemberOperandResolver.Resolve(context, null);

      result.Should().Be(new MooObjectId(expected));
   }

   [Fact]
   public void Resolve_this_returns_the_context_object()
   {
      var context = new MemberCompletionContext(MemberContextKind.Property, "this");

      var result = MemberOperandResolver.Resolve(context, new MooObjectId(42));

      result.Should().Be(new MooObjectId(42));
   }

   [Fact]
   public void Resolve_this_returns_null_without_a_context_object()
   {
      var context = new MemberCompletionContext(MemberContextKind.Verb, "this");

      var result = MemberOperandResolver.Resolve(context, null);

      result.Should().BeNull();
   }

   [Theory]
   [InlineData("me")]       // deferred: no player-id source yet
   [InlineData("player")]   // deferred: no player-id source yet
   [InlineData("foo")]      // bareword
   [InlineData("$foo")]     // core-ref operand (value unknown client-side)
   public void Resolve_unresolvable_operands_return_null(string operand)
   {
      var context = new MemberCompletionContext(MemberContextKind.Verb, operand);

      var result = MemberOperandResolver.Resolve(context, new MooObjectId(42));

      result.Should().BeNull();
   }

   [Fact]
   public void Resolve_none_context_returns_null()
   {
      var result = MemberOperandResolver.Resolve(MemberCompletionContext.None, new MooObjectId(42));

      result.Should().BeNull();
   }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test Org.Edgerunner.Moo.Editor.Tests --filter "FullyQualifiedName~MemberOperandResolverTests"`
Expected: FAIL to compile — `MemberOperandResolver` does not exist.

- [ ] **Step 3: Write the implementation**

Create `Org.Edgerunner.Moo.Editor/Autocomplete/MemberOperandResolver.cs` (BSD header; 3-space indent):

```csharp
using Org.Edgerunner.Mud.Common.Querying;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// Resolves a member-completion operand to the object that should be queried for members.
/// </summary>
/// <remarks>
/// Resolution is deliberately conservative: only operands whose object identity is knowable
/// client-side resolve. <c>me</c>/<c>player</c> are deferred until a player-object-id source
/// exists; barewords and core-reference operands are unresolvable and yield <c>null</c>,
/// which silently skips member completion.
/// </remarks>
public static class MemberOperandResolver
{
   /// <summary>
   /// Resolves the operand of the supplied context to an object id.
   /// </summary>
   /// <param name="context">The detected member completion context.</param>
   /// <param name="contextObjectId">The object the edited verb lives on (the meaning of <c>this</c>), when known.</param>
   /// <returns>The object to query, or <c>null</c> when the operand cannot be resolved.</returns>
   public static MooObjectId? Resolve(MemberCompletionContext context, MooObjectId? contextObjectId)
   {
      switch (context.Kind)
      {
         case MemberContextKind.CoreReference:
            return new MooObjectId(0);
         case MemberContextKind.Verb:
         case MemberContextKind.Property:
            var operand = context.Operand;
            if (operand.StartsWith('#') && int.TryParse(operand[1..], out var number))
               return new MooObjectId(number);
            if (operand == "this")
               return contextObjectId;
            return null;
         default:
            return null;
      }
   }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test Org.Edgerunner.Moo.Editor.Tests --filter "FullyQualifiedName~MemberOperandResolverTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Org.Edgerunner.Moo.Editor/Autocomplete/MemberOperandResolver.cs Org.Edgerunner.Moo.Editor.Tests/MemberOperandResolverTests.cs
git commit -m "feat: add member operand resolver (udd-7g2)"
```

---

### Task 4: `MemberCompletionItem`

An `AutocompleteItem` that matches only the typed part after the last `:`/`.`/`$` and replaces the whole fragment with prefix + member name (same pattern as upstream `MethodAutocompleteItem`, which stores the prefix during `Compare`).

**Files:**
- Create: `Org.Edgerunner.Moo.Editor/Autocomplete/MemberCompletionItem.cs`
- Test: `Org.Edgerunner.Moo.Editor.Tests/MemberCompletionItemTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `Org.Edgerunner.Moo.Editor.Tests/MemberCompletionItemTests.cs`. `AutocompleteItem` is a plain class — instantiating `MemberCompletionItem` is headless-safe. Do NOT construct an `AutocompleteMenu` (that needs a real textbox).

```csharp
using FastColoredTextBoxNS.Types;
using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class MemberCompletionItemTests
{
   [Fact]
   public void Constructor_sets_image_index_from_category()
   {
      var item = new MemberCompletionItem("tell", CompletionIconCategory.Verb);

      item.ImageIndex.Should().Be((int)CompletionIconCategory.Verb);
      item.Text.Should().Be("tell");
   }

   [Theory]
   [InlineData("this:te", CompareResult.VisibleAndSelected)]   // prefix match on typed part
   [InlineData("this:TE", CompareResult.VisibleAndSelected)]   // case-insensitive
   [InlineData("this:", CompareResult.Visible)]                // nothing typed yet: visible, unselected
   [InlineData("obj.te", CompareResult.VisibleAndSelected)]    // property separator works too
   [InlineData("$te", CompareResult.VisibleAndSelected)]       // core-reference separator
   [InlineData("this:xy", CompareResult.Hidden)]               // typed part does not match
   [InlineData("tell", CompareResult.Hidden)]                  // no separator: members hidden in plain fragments
   public void Compare_matches_typed_part_after_last_separator(string fragment, CompareResult expected)
   {
      var item = new MemberCompletionItem("tell", CompletionIconCategory.Verb);

      item.Compare(fragment).Should().Be(expected);
   }

   [Theory]
   [InlineData("this:te", "this:tell")]
   [InlineData("#123:", "#123:tell")]
   [InlineData("obj.te", "obj.tell")]
   [InlineData("$te", "$tell")]
   public void GetTextForReplace_prepends_the_fragment_prefix(string fragment, string expected)
   {
      var item = new MemberCompletionItem("tell", CompletionIconCategory.Verb);

      item.Compare(fragment);   // Compare records the prefix, as MethodAutocompleteItem does upstream

      item.GetTextForReplace().Should().Be(expected);
   }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test Org.Edgerunner.Moo.Editor.Tests --filter "FullyQualifiedName~MemberCompletionItemTests"`
Expected: FAIL to compile — `MemberCompletionItem` does not exist.

- [ ] **Step 3: Write the implementation**

Create `Org.Edgerunner.Moo.Editor/Autocomplete/MemberCompletionItem.cs` (BSD header; 3-space indent):

```csharp
using System;
using FastColoredTextBoxNS.Types;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// An autocomplete item for a world-queried member (verb, property or core reference).
/// Matches only the typed part after the last member separator and replaces the whole
/// fragment with the original prefix plus the member name.
/// </summary>
/// <remarks>
/// The popup fragment includes the operand (for example <c>this:te</c>), because the menu's
/// search pattern treats <c>:</c>, <c>.</c> and <c>$</c> as fragment characters. Like the
/// upstream <see cref="MethodAutocompleteItem"/>, <see cref="Compare"/> records the fragment
/// prefix so <see cref="GetTextForReplace"/> can reproduce it.
/// </remarks>
public class MemberCompletionItem : AutocompleteItem
{
   private static readonly char[] SeparatorChars = { ':', '.', '$' };

   private string _replacementPrefix = string.Empty;

   /// <summary>
   /// Initializes a new instance of the <see cref="MemberCompletionItem"/> class.
   /// </summary>
   /// <param name="memberName">The member name offered for completion.</param>
   /// <param name="category">The icon category (verb, property or core reference).</param>
   public MemberCompletionItem(string memberName, CompletionIconCategory category)
      : base(memberName, (int)category)
   {
   }

   /// <inheritdoc/>
   public override CompareResult Compare(string fragmentText)
   {
      var index = fragmentText.LastIndexOfAny(SeparatorChars);
      if (index < 0)
         return CompareResult.Hidden;

      _replacementPrefix = fragmentText[..(index + 1)];
      var typed = fragmentText[(index + 1)..];
      if (typed.Length == 0)
         return CompareResult.Visible;

      return Text.StartsWith(typed, StringComparison.OrdinalIgnoreCase)
                ? CompareResult.VisibleAndSelected
                : CompareResult.Hidden;
   }

   /// <inheritdoc/>
   public override string GetTextForReplace() => _replacementPrefix + Text;
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test Org.Edgerunner.Moo.Editor.Tests --filter "FullyQualifiedName~MemberCompletionItemTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Org.Edgerunner.Moo.Editor/Autocomplete/MemberCompletionItem.cs Org.Edgerunner.Moo.Editor.Tests/MemberCompletionItemTests.cs
git commit -m "feat: add MemberCompletionItem with verb/property/core-ref icons (udd-7g2)"
```

---

### Task 5: `MemberCompletionController` (async query + cache)

The bridge between the synchronous popup enumeration and the async provider. Called synchronously during item enumeration; returns cached items or kicks off a background fetch and returns nothing. All state mutation is funneled through the `uiMarshal` callback so the controller is single-threaded from its own point of view (tests pass an immediate-invoke marshal).

**Files:**
- Create: `Org.Edgerunner.Moo.Editor/Autocomplete/MemberCompletionController.cs`
- Test: `Org.Edgerunner.Moo.Editor.Tests/MemberCompletionControllerTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `Org.Edgerunner.Moo.Editor.Tests/MemberCompletionControllerTests.cs`. The fake provider implements only the two used methods; everything else throws.

```csharp
using FastColoredTextBoxNS.Types;
using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Org.Edgerunner.Mud.Common.Querying;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class MemberCompletionControllerTests
{
   private sealed class FakeQueryProvider : IMooWorldQueryProvider
   {
      public List<MooVerbSummary> Verbs { get; } = new();
      public List<MooPropertySummary> Properties { get; } = new();
      public int VerbCalls;
      public int PropertyCalls;
      public TaskCompletionSource? Gate;
      public Exception? ThrowOnVerbs;

      public async Task<IReadOnlyList<MooVerbSummary>> GetVerbsAsync(MooObjectId objectId, CancellationToken cancellationToken)
      {
         Interlocked.Increment(ref VerbCalls);
         if (Gate is not null)
            await Gate.Task.WaitAsync(cancellationToken);
         if (ThrowOnVerbs is not null)
            throw ThrowOnVerbs;
         return Verbs;
      }

      public async Task<IReadOnlyList<MooPropertySummary>> GetPropertiesAsync(MooObjectId objectId, CancellationToken cancellationToken)
      {
         Interlocked.Increment(ref PropertyCalls);
         if (Gate is not null)
            await Gate.Task.WaitAsync(cancellationToken);
         return Properties;
      }

      public Task<IReadOnlyList<MooObjectSummary>> GetObjectsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetChildrenAsync(MooObjectId objectId, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(MooObjectId owner, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooObjectId?> GetParentAsync(MooObjectId objectId, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbInfo?> GetVerbInfoAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooPropertyInfo?> GetPropertyInfoAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbCode?> GetVerbCodeAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooPropertyValue?> GetPropertyValueAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbDocumentation?> GetVerbDocumentationAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<string>> GetPropertyDocumentationAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
   }

   private static MemberCompletionController CreateController(
      FakeQueryProvider provider,
      MooObjectId? contextObject = null,
      Action? refresh = null)
   {
      return new MemberCompletionController(
         () => provider,
         () => contextObject,
         action => action(),          // immediate marshal: deterministic single-threaded tests
         refresh ?? (() => { }));
   }

   private static void WaitForCache(MemberCompletionController controller, string linePrefix)
   {
      // The fetch completes asynchronously; poll briefly until the marshalled cache write lands.
      SpinWait.SpinUntil(() => controller.GetMemberItems(linePrefix).Count > 0, TimeSpan.FromSeconds(5))
              .Should().BeTrue("the fetched members should land in the cache");
   }

   [Fact]
   public void GetMemberItems_returns_empty_for_non_member_context()
   {
      var provider = new FakeQueryProvider();
      using var controller = CreateController(provider);

      controller.GetMemberItems("x = 5").Should().BeEmpty();
      provider.VerbCalls.Should().Be(0);
      provider.PropertyCalls.Should().Be(0);
   }

   [Fact]
   public void GetMemberItems_returns_empty_when_operand_unresolved()
   {
      var provider = new FakeQueryProvider();
      using var controller = CreateController(provider, contextObject: null);

      controller.GetMemberItems("this:te").Should().BeEmpty();
      provider.VerbCalls.Should().Be(0);
   }

   [Fact]
   public void GetMemberItems_returns_empty_without_a_provider()
   {
      using var controller = new MemberCompletionController(
         () => null, () => new MooObjectId(5), action => action(), () => { });

      controller.GetMemberItems("this:te").Should().BeEmpty();
   }

   [Fact]
   public void Verb_context_fetches_verbs_and_caches_flattened_aliases()
   {
      var provider = new FakeQueryProvider();
      provider.Verbs.Add(new MooVerbSummary(new[] { "tell", "g*et" }, new MooObjectId(1)));
      provider.Verbs.Add(new MooVerbSummary(new[] { "drop" }, new MooObjectId(1)));
      using var controller = CreateController(provider, contextObject: new MooObjectId(5));

      controller.GetMemberItems("this:").Should().BeEmpty("first call only starts the fetch");
      WaitForCache(controller, "this:");

      var items = controller.GetMemberItems("this:");
      items.Select(i => i.Text).Should().BeEquivalentTo("drop", "get", "tell");
      items.Should().AllSatisfy(i => i.ImageIndex.Should().Be((int)CompletionIconCategory.Verb));
      provider.VerbCalls.Should().Be(1, "subsequent calls must be served from the cache");
   }

   [Fact]
   public void Property_context_fetches_properties_with_property_icon()
   {
      var provider = new FakeQueryProvider();
      provider.Properties.Add(new MooPropertySummary("name", new MooObjectId(1)));
      using var controller = CreateController(provider, contextObject: new MooObjectId(5));

      controller.GetMemberItems("this.");
      WaitForCache(controller, "this.");

      var items = controller.GetMemberItems("this.");
      items.Single().Text.Should().Be("name");
      items.Single().ImageIndex.Should().Be((int)CompletionIconCategory.Property);
   }

   [Fact]
   public void Core_reference_context_fetches_properties_of_object_zero_with_core_icon()
   {
      var provider = new FakeQueryProvider();
      provider.Properties.Add(new MooPropertySummary("room", new MooObjectId(0)));
      using var controller = CreateController(provider);

      controller.GetMemberItems("$ro");
      WaitForCache(controller, "$ro");

      var items = controller.GetMemberItems("$ro");
      items.Single().Text.Should().Be("room");
      items.Single().ImageIndex.Should().Be((int)CompletionIconCategory.CoreReference);
   }

   [Fact]
   public void Completed_fetch_invokes_the_menu_refresh_callback()
   {
      var provider = new FakeQueryProvider();
      provider.Properties.Add(new MooPropertySummary("name", new MooObjectId(1)));
      var refreshed = 0;
      using var controller = CreateController(provider, new MooObjectId(5), () => Interlocked.Increment(ref refreshed));

      controller.GetMemberItems("this.");
      WaitForCache(controller, "this.");

      refreshed.Should().BeGreaterThan(0);
   }

   [Fact]
   public void Provider_failure_is_swallowed_and_yields_no_items()
   {
      var provider = new FakeQueryProvider { ThrowOnVerbs = new TimeoutException("SDWC timed out") };
      using var controller = CreateController(provider, contextObject: new MooObjectId(5));

      controller.GetMemberItems("this:");
      Thread.Sleep(250); // allow the faulted fetch to finish

      controller.GetMemberItems("this:x").Should().BeEmpty();
   }

   [Fact]
   public void New_context_cancels_the_inflight_fetch()
   {
      var provider = new FakeQueryProvider { Gate = new TaskCompletionSource() };
      provider.Properties.Add(new MooPropertySummary("name", new MooObjectId(1)));
      using var controller = CreateController(provider, contextObject: new MooObjectId(5));

      controller.GetMemberItems("this:");    // starts verb fetch, parked on the gate
      controller.GetMemberItems("#7.");      // different key: must cancel the verb fetch and start this one
      provider.Gate.SetResult();             // release both

      SpinWait.SpinUntil(() => controller.GetMemberItems("#7.").Count > 0, TimeSpan.FromSeconds(5))
              .Should().BeTrue();
      controller.GetMemberItems("this:").Should().BeEmpty("the cancelled verb fetch must not populate the cache");
   }

   [Fact]
   public void Repeated_trigger_for_same_key_does_not_start_a_second_fetch()
   {
      var provider = new FakeQueryProvider { Gate = new TaskCompletionSource() };
      using var controller = CreateController(provider, contextObject: new MooObjectId(5));

      controller.GetMemberItems("this:");
      controller.GetMemberItems("this:t");
      controller.GetMemberItems("this:te");

      provider.VerbCalls.Should().Be(1);
      provider.Gate.SetResult();
   }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test Org.Edgerunner.Moo.Editor.Tests --filter "FullyQualifiedName~MemberCompletionControllerTests"`
Expected: FAIL to compile — `MemberCompletionController` does not exist.

- [ ] **Step 3: Write the implementation**

Create `Org.Edgerunner.Moo.Editor/Autocomplete/MemberCompletionController.cs` (BSD header; 3-space indent):

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastColoredTextBoxNS.Types;
using Org.Edgerunner.Mud.Common.Querying;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// Supplies world-queried member completion items (verbs, properties, core references) to the
/// autocomplete popup. Lookups are synchronous against a local cache; misses start a single
/// background fetch and return nothing, and the menu is refreshed when results arrive.
/// </summary>
/// <remarks>
/// In production all state mutation happens on the UI thread (popup enumeration plus actions the
/// owner marshals there), but a lock guards the cache and in-flight state anyway so that hosts
/// (and tests) with an immediate marshal are also safe. Member completion is best-effort: provider
/// failures (timeout, cancellation, disconnect, protocol errors) are swallowed and simply leave
/// the static completion list in place.
/// </remarks>
public sealed class MemberCompletionController : IDisposable
{
   /// <summary>The default lifetime of a cached member list.</summary>
   public static readonly TimeSpan DefaultCacheTimeToLive = TimeSpan.FromSeconds(30);

   private readonly Func<IMooWorldQueryProvider?> _providerAccessor;

   private readonly Func<MooObjectId?> _contextObjectAccessor;

   private readonly Action<Action> _uiMarshal;

   private readonly Action _menuRefresh;

   private readonly TimeSpan _cacheTimeToLive;

   private readonly object _stateLock = new();

   private readonly Dictionary<(MemberContextKind Kind, int ObjectNumber), CacheEntry> _cache = new();

   private (MemberContextKind Kind, int ObjectNumber)? _inflightKey;

   private CancellationTokenSource? _fetchCancellation;

   private bool _disposed;

   private sealed record CacheEntry(IReadOnlyList<AutocompleteItem> Items, DateTime CreatedUtc);

   /// <summary>
   /// Initializes a new instance of the <see cref="MemberCompletionController"/> class.
   /// </summary>
   /// <param name="providerAccessor">Returns the current query provider, or <c>null</c> when none is attached.</param>
   /// <param name="contextObjectAccessor">Returns the object the edited verb lives on (the meaning of <c>this</c>), when known.</param>
   /// <param name="uiMarshal">Runs the supplied action on the UI thread (tests may invoke immediately).</param>
   /// <param name="menuRefresh">Asks the owner to refresh the autocomplete popup if it is open.</param>
   /// <param name="cacheTimeToLive">Cache entry lifetime; defaults to <see cref="DefaultCacheTimeToLive"/>.</param>
   /// <exception cref="ArgumentNullException">Thrown when any callback is <c>null</c>.</exception>
   public MemberCompletionController(
      Func<IMooWorldQueryProvider?> providerAccessor,
      Func<MooObjectId?> contextObjectAccessor,
      Action<Action> uiMarshal,
      Action menuRefresh,
      TimeSpan? cacheTimeToLive = null)
   {
      _providerAccessor = providerAccessor ?? throw new ArgumentNullException(nameof(providerAccessor));
      _contextObjectAccessor = contextObjectAccessor ?? throw new ArgumentNullException(nameof(contextObjectAccessor));
      _uiMarshal = uiMarshal ?? throw new ArgumentNullException(nameof(uiMarshal));
      _menuRefresh = menuRefresh ?? throw new ArgumentNullException(nameof(menuRefresh));
      _cacheTimeToLive = cacheTimeToLive ?? DefaultCacheTimeToLive;
   }

   /// <summary>
   /// Gets the member completion items for the caret position described by <paramref name="linePrefix"/>.
   /// Returns an empty list outside member contexts, for unresolved operands, without a provider,
   /// or while a fetch is still in flight (the menu is refreshed when it completes).
   /// </summary>
   /// <param name="linePrefix">The text on the caret line, from column 0 up to the caret.</param>
   /// <returns>The items to offer; never <c>null</c>.</returns>
   public IReadOnlyList<AutocompleteItem> GetMemberItems(string linePrefix)
   {
      var context = MemberCompletionContextDetector.Detect(linePrefix);
      if (context.Kind == MemberContextKind.None)
         return Array.Empty<AutocompleteItem>();

      var objectId = MemberOperandResolver.Resolve(context, _contextObjectAccessor());
      if (objectId is null)
         return Array.Empty<AutocompleteItem>();

      var key = (context.Kind, objectId.Value.Number);
      lock (_stateLock)
      {
         if (_disposed)
            return Array.Empty<AutocompleteItem>();

         if (_cache.TryGetValue(key, out var entry))
         {
            if (DateTime.UtcNow - entry.CreatedUtc < _cacheTimeToLive)
               return entry.Items;

            _cache.Remove(key);
         }

         var provider = _providerAccessor();
         if (provider is null || _inflightKey == key)
            return Array.Empty<AutocompleteItem>();

         _fetchCancellation?.Cancel();
         _fetchCancellation = new CancellationTokenSource();
         _inflightKey = key;
         _ = FetchAsync(provider, context.Kind, objectId.Value, key, _fetchCancellation.Token);
      }

      return Array.Empty<AutocompleteItem>();
   }

   /// <inheritdoc/>
   public void Dispose()
   {
      lock (_stateLock)
      {
         if (_disposed)
            return;

         _disposed = true;
         _fetchCancellation?.Cancel();
      }
   }

   private async Task FetchAsync(
      IMooWorldQueryProvider provider,
      MemberContextKind kind,
      MooObjectId objectId,
      (MemberContextKind Kind, int ObjectNumber) key,
      CancellationToken cancellationToken)
   {
      try
      {
         IReadOnlyList<AutocompleteItem> items;
         if (kind == MemberContextKind.Verb)
            items = BuildVerbItems(await provider.GetVerbsAsync(objectId, cancellationToken).ConfigureAwait(false));
         else
            items = BuildPropertyItems(await provider.GetPropertiesAsync(objectId, cancellationToken).ConfigureAwait(false), kind);

         _uiMarshal(() =>
         {
            lock (_stateLock)
            {
               if (_disposed || cancellationToken.IsCancellationRequested)
                  return;

               _cache[key] = new CacheEntry(items, DateTime.UtcNow);
               if (_inflightKey == key)
                  _inflightKey = null;
            }

            _menuRefresh();
         });
      }
      catch (Exception)
      {
         // Best-effort completion: any failure leaves only the static list in place.
         _uiMarshal(() =>
         {
            lock (_stateLock)
            {
               if (!_disposed && _inflightKey == key)
                  _inflightKey = null;
            }
         });
      }
   }

   private static IReadOnlyList<AutocompleteItem> BuildVerbItems(IReadOnlyList<MooVerbSummary> verbs)
   {
      // Flatten aliases, strip MOO prefix-match stars ("g*et" => "get"), de-duplicate and sort.
      var names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
      foreach (var verb in verbs)
         foreach (var alias in verb.Aliases)
         {
            var name = alias.Replace("*", string.Empty);
            if (name.Length > 0)
               names.Add(name);
         }

      return names.Select(name => (AutocompleteItem)new MemberCompletionItem(name, CompletionIconCategory.Verb)).ToList();
   }

   private static IReadOnlyList<AutocompleteItem> BuildPropertyItems(IReadOnlyList<MooPropertySummary> properties, MemberContextKind kind)
   {
      var category = kind == MemberContextKind.CoreReference
                        ? CompletionIconCategory.CoreReference
                        : CompletionIconCategory.Property;
      var names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
      foreach (var property in properties)
         if (!string.IsNullOrEmpty(property.Name))
            names.Add(property.Name);

      return names.Select(name => (AutocompleteItem)new MemberCompletionItem(name, category)).ToList();
   }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test Org.Edgerunner.Moo.Editor.Tests --filter "FullyQualifiedName~MemberCompletionControllerTests"`
Expected: PASS (11 tests). If `New_context_cancels_the_inflight_fetch` is flaky, the cancellation check inside the marshalled action is the part to inspect — the cancelled fetch's `cancellationToken.IsCancellationRequested` must be true by the time its marshalled action runs (it is: the second `GetMemberItems` call cancelled the CTS synchronously before the gate opened).

- [ ] **Step 5: Commit**

```bash
git add Org.Edgerunner.Moo.Editor/Autocomplete/MemberCompletionController.cs Org.Edgerunner.Moo.Editor.Tests/MemberCompletionControllerTests.cs
git commit -m "feat: add async member completion controller with caching (udd-7g2)"
```

---

### Task 6: `DynamicCompletionSource` enumerable

The lazy `IEnumerable<AutocompleteItem>` handed to `SetAutocompleteItems`: yields current member items first, then the static items.

**Files:**
- Create: `Org.Edgerunner.Moo.Editor/Autocomplete/DynamicCompletionSource.cs`
- Test: `Org.Edgerunner.Moo.Editor.Tests/DynamicCompletionSourceTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `Org.Edgerunner.Moo.Editor.Tests/DynamicCompletionSourceTests.cs`:

```csharp
using FastColoredTextBoxNS.Types;
using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Org.Edgerunner.Mud.Common.Querying;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class DynamicCompletionSourceTests
{
   private static MemberCompletionController CreateInertController()
   {
      // No provider: the controller always yields empty member lists.
      return new MemberCompletionController(() => null, () => null, action => action(), () => { });
   }

   [Fact]
   public void Enumeration_yields_static_items_when_no_member_context()
   {
      var statics = new List<AutocompleteItem> { new("if"), new("while") };
      using var controller = CreateInertController();
      var source = new DynamicCompletionSource(statics, controller, () => "x = 5");

      source.Should().Equal(statics);
   }

   [Fact]
   public void Enumeration_yields_member_items_before_static_items()
   {
      var statics = new List<AutocompleteItem> { new("if") };
      var fakeProvider = new ImmediatePropertyProvider("name");
      using var controller = new MemberCompletionController(
         () => fakeProvider, () => new MooObjectId(5), action => action(), () => { });
      var source = new DynamicCompletionSource(statics, controller, () => "this.");

      source.ToList();                                       // first pass starts the fetch
      SpinWait.SpinUntil(() => source.Count() == 2, TimeSpan.FromSeconds(5)).Should().BeTrue();

      var items = source.ToList();
      items[0].Should().BeOfType<MemberCompletionItem>();
      items[0].Text.Should().Be("name");
      items[1].Text.Should().Be("if");
   }

   [Fact]
   public void Line_prefix_is_evaluated_freshly_on_every_enumeration()
   {
      var prefixes = new Queue<string>(new[] { "x = 5", "x = 5" });
      using var controller = CreateInertController();
      var source = new DynamicCompletionSource(new List<AutocompleteItem>(), controller, prefixes.Dequeue);

      source.ToList();
      source.ToList();

      prefixes.Should().BeEmpty("each enumeration must request the current line prefix");
   }

   /// <summary>A provider whose property query completes synchronously.</summary>
   private sealed class ImmediatePropertyProvider : IMooWorldQueryProvider
   {
      private readonly string _propertyName;

      public ImmediatePropertyProvider(string propertyName) => _propertyName = propertyName;

      public Task<IReadOnlyList<MooPropertySummary>> GetPropertiesAsync(MooObjectId objectId, CancellationToken cancellationToken) =>
         Task.FromResult<IReadOnlyList<MooPropertySummary>>(new[] { new MooPropertySummary(_propertyName, objectId) });

      public Task<IReadOnlyList<MooVerbSummary>> GetVerbsAsync(MooObjectId objectId, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetObjectsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetChildrenAsync(MooObjectId objectId, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(MooObjectId owner, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooObjectId?> GetParentAsync(MooObjectId objectId, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbInfo?> GetVerbInfoAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooPropertyInfo?> GetPropertyInfoAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbCode?> GetVerbCodeAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooPropertyValue?> GetPropertyValueAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbDocumentation?> GetVerbDocumentationAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<string>> GetPropertyDocumentationAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
   }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test Org.Edgerunner.Moo.Editor.Tests --filter "FullyQualifiedName~DynamicCompletionSourceTests"`
Expected: FAIL to compile — `DynamicCompletionSource` does not exist.

- [ ] **Step 3: Write the implementation**

Create `Org.Edgerunner.Moo.Editor/Autocomplete/DynamicCompletionSource.cs` (BSD header; 3-space indent):

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using FastColoredTextBoxNS.Types;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// The autocomplete item source handed to the popup menu. The menu re-enumerates its source on
/// every refresh, so each enumeration injects the current world-queried member items (if any)
/// ahead of the static keyword/builtin/snippet items.
/// </summary>
public sealed class DynamicCompletionSource : IEnumerable<AutocompleteItem>
{
   private readonly IReadOnlyList<AutocompleteItem> _staticItems;

   private readonly MemberCompletionController _controller;

   private readonly Func<string> _linePrefixProvider;

   /// <summary>
   /// Initializes a new instance of the <see cref="DynamicCompletionSource"/> class.
   /// </summary>
   /// <param name="staticItems">The static completion items (keywords, builtins, snippets).</param>
   /// <param name="controller">The member completion controller.</param>
   /// <param name="linePrefixProvider">Returns the caret line text from column 0 up to the caret.</param>
   /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
   public DynamicCompletionSource(
      IReadOnlyList<AutocompleteItem> staticItems,
      MemberCompletionController controller,
      Func<string> linePrefixProvider)
   {
      _staticItems = staticItems ?? throw new ArgumentNullException(nameof(staticItems));
      _controller = controller ?? throw new ArgumentNullException(nameof(controller));
      _linePrefixProvider = linePrefixProvider ?? throw new ArgumentNullException(nameof(linePrefixProvider));
   }

   /// <inheritdoc/>
   public IEnumerator<AutocompleteItem> GetEnumerator()
   {
      foreach (var item in _controller.GetMemberItems(_linePrefixProvider()))
         yield return item;

      foreach (var item in _staticItems)
         yield return item;
   }

   /// <inheritdoc/>
   IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test Org.Edgerunner.Moo.Editor.Tests --filter "FullyQualifiedName~DynamicCompletionSourceTests"`
Expected: PASS (3 tests).

- [ ] **Step 5: Commit**

```bash
git add Org.Edgerunner.Moo.Editor/Autocomplete/DynamicCompletionSource.cs Org.Edgerunner.Moo.Editor.Tests/DynamicCompletionSourceTests.cs
git commit -m "feat: add dynamic completion source enumerable (udd-7g2)"
```

---

### Task 7: Wire into `MooCodeEditorPage` (build-only verification)

**Files:**
- Modify: `Org.Edgerunner.Moo.Udditor/Pages/MooCodeEditorPage.cs` (the `QueryProvider` property block ~line 51-63, and `BuildAutocompleteMenu` ~line 272-300)

- [ ] **Step 1: Add the `ContextObjectId` property and controller field**

In `MooCodeEditorPage.cs`, directly after the existing `QueryProvider` property (line 63), add (4-space indent in this file):

```csharp
    /// <summary>
    /// Gets or sets the object the edited verb lives on (the meaning of <c>this</c> in the
    /// edited code), parsed from the local-edit upload command or simpleedit reference.
    /// </summary>
    /// <value>
    /// The context object id, or <c>null</c> for file-based or otherwise unattributed edits.
    /// </value>
    public MooObjectId? ContextObjectId { get; set; }

    private MemberCompletionController? _memberCompletionController;
```

- [ ] **Step 2: Rework `BuildAutocompleteMenu`**

Replace the body of `BuildAutocompleteMenu` (currently lines 272-300) with:

```csharp
    private void BuildAutocompleteMenu(MooCodeEditor codeEditor)
    {
        codeEditor.AutocompleteMenu = new AutocompleteMenu(codeEditor);

        codeEditor.AutocompleteMenu.ImageList = CompletionIconFactory.CreateImageList();
        // $ and # are fragment characters so member contexts ($foo, #123:verb) reach the items.
        codeEditor.AutocompleteMenu.SearchPattern = @"[\w\.:=!<>+-/*%&|^$#]";
        codeEditor.AutocompleteMenu.AllowTabKey = true;
        codeEditor.AutocompleteMenu.MinFragmentLength = 1;

        List<AutocompleteItem> items = new List<AutocompleteItem>();

        foreach (var item in Snippets.LoadSnippets(ApplicationPaths.ResolveDataFile("Snippets.txt")))
            items.Add(new SnippetAutocompleteItem(item) { ImageIndex = (int)CompletionIconCategory.Snippet });
        foreach (var item in Moo.Editor.Moo.Keywords)
            items.Add(new AutoIndentingSnippet(item) { ImageIndex = (int)Moo.Editor.Moo.ClassifyKeyword(item) });
        foreach (var builtin in Moo.Editor.Moo.Builtins.Values)
            items.Add(new SnippetAutocompleteItem(builtin) { ImageIndex = (int)CompletionIconCategory.Function });

        // display the completion items alphabetically by their menu text
        items.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.CurrentCultureIgnoreCase));

        items.Add(new InsertSpaceSnippet());
        items.Add(new InsertSpaceSnippet(@"^(\w+)([=<>!&|%-+*/]+)(\w+)$"));
        items.Add(new FormatCommaSnippet(@"^(\w+)(([,]+)(\w+))+$"));

        // member completion: world-queried verbs/properties/core references injected ahead of
        // the static items whenever the caret sits in a member context on a connected world.
        _memberCompletionController?.Dispose();
        _memberCompletionController = new MemberCompletionController(
            () => QueryProvider,
            () => ContextObjectId,
            action =>
            {
                if (!codeEditor.IsDisposed && codeEditor.IsHandleCreated)
                    codeEditor.BeginInvoke(action);
            },
            () =>
            {
                var menu = codeEditor.AutocompleteMenu;
                if (menu is { Visible: true })
                    menu.Show(false);
            });

        //set as autocomplete source
        codeEditor.AutocompleteMenu.Items.SetAutocompleteItems(
            new DynamicCompletionSource(items, _memberCompletionController, () => GetCaretLinePrefix(codeEditor)));
        codeEditor.AutocompleteMenu.AppearInterval = Settings.Instance.EditorAutocompleteDelay;
    }

    private static string GetCaretLinePrefix(MooCodeEditor codeEditor)
    {
        var place = codeEditor.Selection.Start;
        if (place.iLine < 0 || place.iLine >= codeEditor.LinesCount || place.iChar <= 0)
            return string.Empty;

        var line = codeEditor.GetLineText(place.iLine);
        return line.Substring(0, Math.Min(place.iChar, line.Length));
    }
```

No new `using` directives should be needed — the file already imports `Org.Edgerunner.Moo.Editor.Autocomplete`, `Org.Edgerunner.Mud.Common.Querying`, `FastColoredTextBoxNS`, and `System`. Verify and add any the compiler reports missing.

- [ ] **Step 3: Build**

Run: `dotnet build "Moo Developer Tools.sln" -c Debug`
Expected: Build succeeded, 0 errors. (Warnings pre-exist; do not chase them.)

- [ ] **Step 4: Commit**

```bash
git add Org.Edgerunner.Moo.Udditor/Pages/MooCodeEditorPage.cs
git commit -m "feat: wire dynamic member completion into the code editor page (udd-7g2)"
```

---

### Task 8: Wire both local-edit creation paths (build-only verification)

**Files:**
- Modify: `Org.Edgerunner.Moo.Udditor/Communication/OutOfBand/LocalEditHandler.cs` (~line 115-127)
- Modify: `Org.Edgerunner.Moo.Udditor/Communication/OutOfBand/WindowManagerSimpleEditConsumer.cs` (~line 68-91)

- [ ] **Step 1: Wire `LocalEditHandler`**

In `LocalEditHandler.ProcessMessage`, the code-editor branch currently reads (3-space indent in this file):

```csharp
            if (DocumentName.Contains(":"))
            {
               Logger.Trace("Opening code editor");
               page = _WindowManager.CreateMooCodeEditorPage(DocumentName,
                                                      client.World,
                                                      Settings.Instance.DefaultGrammarDialect,
                                                      DocumentSource.ToString().Trim());
            }
```

Change it to:

```csharp
            if (DocumentName.Contains(":"))
            {
               Logger.Trace("Opening code editor");
               var codePage = _WindowManager.CreateMooCodeEditorPage(DocumentName,
                                                      client.World,
                                                      Settings.Instance.DefaultGrammarDialect,
                                                      DocumentSource.ToString().Trim());
               // Attach the world query provider and edited-object identity so the editor can
               // offer contextual member completion (verbs/properties/core references).
               codePage.QueryProvider = client.QueryProviders.Query;
               codePage.ContextObjectId = MooObjectReferenceParser.FindFirstObjectId(UploadCommand);
               page = codePage;
            }
```

Add `using Org.Edgerunner.Mud.Common.Querying;` to the file's using block if not already present.

- [ ] **Step 2: Wire `WindowManagerSimpleEditConsumer`**

In `WindowManagerSimpleEditConsumer.PresentEdit`, the moo-code branch currently reads (3-space indent):

```csharp
      if (string.Equals(request.EditType, "moo-code", StringComparison.OrdinalIgnoreCase))
      {
         Logger.Trace("SimpleEdit: opening code editor");
         page = _WindowManager.CreateMooCodeEditorPage(
            request.Name,
            world,
            Settings.Instance.DefaultGrammarDialect,
            request.Content);
      }
```

Change it to:

```csharp
      if (string.Equals(request.EditType, "moo-code", StringComparison.OrdinalIgnoreCase))
      {
         Logger.Trace("SimpleEdit: opening code editor");
         var codePage = _WindowManager.CreateMooCodeEditorPage(
            request.Name,
            world,
            Settings.Instance.DefaultGrammarDialect,
            request.Content);
         // Attach the world query provider and edited-object identity so the editor can
         // offer contextual member completion (verbs/properties/core references).
         codePage.QueryProvider = uploader.ClientTerminal.QueryProviders.Query;
         codePage.ContextObjectId = MooObjectReferenceParser.FindFirstObjectId(request.Reference);
         page = codePage;
      }
```

Add `using Org.Edgerunner.Mud.Common.Querying;` if not already present.

- [ ] **Step 3: Build**

Run: `dotnet build "Moo Developer Tools.sln" -c Debug`
Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
git add Org.Edgerunner.Moo.Udditor/Communication/OutOfBand/LocalEditHandler.cs Org.Edgerunner.Moo.Udditor/Communication/OutOfBand/WindowManagerSimpleEditConsumer.cs
git commit -m "feat: attach query provider and context object to local-edit pages (udd-7g2)"
```

---

### Task 9: Final verification

- [ ] **Step 1: Full solution build**

Run: `dotnet build "Moo Developer Tools.sln" -c Debug`
Expected: Build succeeded, 0 errors.

- [ ] **Step 2: Run all new tests (filtered — never bare `dotnet test`)**

```bash
dotnet test Org.Edgerunner.Mud.Common.Tests --filter "FullyQualifiedName~MooObjectReferenceParserTests" --no-build
dotnet test Org.Edgerunner.Moo.Editor.Tests --filter "FullyQualifiedName~MemberCompletionContextDetectorTests|FullyQualifiedName~MemberOperandResolverTests|FullyQualifiedName~MemberCompletionItemTests|FullyQualifiedName~MemberCompletionControllerTests|FullyQualifiedName~DynamicCompletionSourceTests" --no-build
```

Expected: all PASS, zero failures, and no GUI window appears at any point.

- [ ] **Step 3: Commit anything outstanding**

```bash
git status
```

Expected: clean (every task committed as it went). If anything is left, commit it now with an appropriate message.

**Do NOT close udd-7g2 or merge here.** Bead closure and merge happen in the main session after the user's live smoke test (typing `$`/`this:`/`#123.` in a verb editor against an SDWC world and seeing green/purple/cyan items), per the finishing-a-development-branch skill — and the bead must be closed BEFORE the merge commit so the `.beads` change rides with it.
