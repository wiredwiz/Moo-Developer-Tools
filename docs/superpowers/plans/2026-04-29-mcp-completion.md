# MCP Completion Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Complete the MUD Client Protocol (MCP) 2.1 implementation by adding `McpMessageParser`, `McpMessageDispatcher`, `McpOobHandler`, `McpNegotiatePackage`, `McpCordPackage`, and wiring them into the existing `OutOfBandMessageProcessor` pipeline.

**Architecture:** MCP processing stays at the terminal level as a registered `IOutOfBandMessageHandler`. A new `McpOobHandler` feeds raw OOB lines into a stateful `McpMessageParser`, which emits complete `Message` objects to `McpMessageDispatcher` for routing to `IMcpPackage` implementations. The existing `McpClientSessionManager`, `McpClientSession`, `Message`, and `McpUtils.SplitMessageIntoWords` are all preserved and reused.

**Tech Stack:** .NET 6 / C#, xUnit 2.7, FluentAssertions 6.12, NSubstitute 5.1

---

## File Map

| File | Action | Purpose |
|---|---|---|
| `Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj` | Create | xUnit test project |
| `Org.Edgerunner.Mud.MCP.Tests/McpMessageParserTests.cs` | Create | Parser unit tests |
| `Org.Edgerunner.Mud.MCP.Tests/McpClientSessionManagerTests.cs` | Create | Session manager unit tests |
| `Org.Edgerunner.Mud.MCP.Tests/McpMessageDispatcherTests.cs` | Create | Dispatcher unit tests |
| `Org.Edgerunner.Mud.MCP.Tests/McpNegotiatePackageTests.cs` | Create | Negotiate package unit tests |
| `Org.Edgerunner.Mud.MCP.Tests/McpCordPackageTests.cs` | Create | Cord package unit tests |
| `Org.Edgerunner.Mud.MCP/Interfaces/IMcpProtocolHandler.cs` | Modify | Add `IClientTerminal` param to `ProcessMessage` |
| `Org.Edgerunner.Mud.MCP/Interfaces/IMCPPackage.cs` | Modify | Add `SetSession(McpClientSession)` |
| `Org.Edgerunner.Mud.MCP/Interfaces/IMCPSession.cs` | Modify | Add `IsNegotiationComplete` property |
| `Org.Edgerunner.Mud.MCP/McpClientSession.cs` | Modify | Implement `IsNegotiationComplete`; remove unused session manager field from terminal |
| `Org.Edgerunner.Mud.MCP/McpUtils.cs` | Modify | Remove `ParseMessage`; add `FormatMessage` |
| `Org.Edgerunner.Mud.MCP/McpMessageParser.cs` | Create | Stateful single/multiline MCP message parser |
| `Org.Edgerunner.Mud.MCP/Packages/McpNegotiatePackage.cs` | Create | `mcp-negotiate` package |
| `Org.Edgerunner.Mud.MCP/Packages/McpCord.cs` | Create | Cord state model |
| `Org.Edgerunner.Mud.MCP/Packages/McpCordPackage.cs` | Create | `mcp-cord` package |
| `Org.Edgerunner.Mud.MCP/McpMessageDispatcher.cs` | Create | Routes assembled messages to packages |
| `Org.Edgerunner.Mud.MCP/McpOobHandler.cs` | Create | `IOutOfBandMessageHandler` coordinator |
| `Org.Edgerunner.Moo.Editor/Controls/MooClientTerminal.cs` | Modify | Remove unused `McpSessionManager` property |
| `Org.Edgerunner.Moo.Udditor/WindowManager.cs` | Modify | Register `McpOobHandler` in `CreateTerminalPage` |
| `Moo Developer Tools.sln` | Modify | Add test project |

---

## Task 1: Create Test Project

**Files:**
- Create: `Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj`
- Modify: `Moo Developer Tools.sln`

- [ ] **Step 1: Create the project file**

Create `Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <Platforms>AnyCPU;x64;x86</Platforms>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
    <PackageReference Include="xunit" Version="2.7.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.7">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="coverlet.collector" Version="6.0.0">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="NSubstitute" Version="5.1.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Org.Edgerunner.Mud.MCP\Org.Edgerunner.Mud.MCP.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Add to solution**

```bash
cd "D:\Projects\Moo Developer Tools"
dotnet sln "Moo Developer Tools.sln" add "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj"
```

Expected: `Project ... added to the solution.`

- [ ] **Step 3: Verify build**

```bash
dotnet build "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj"
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add "Org.Edgerunner.Mud.MCP.Tests/" "Moo Developer Tools.sln"
git commit -m "Add Org.Edgerunner.Mud.MCP.Tests xUnit project"
```

---

## Task 2: Update Interfaces and McpClientSession

**Files:**
- Modify: `Org.Edgerunner.Mud.MCP/Interfaces/IMcpProtocolHandler.cs`
- Modify: `Org.Edgerunner.Mud.MCP/Interfaces/IMCPPackage.cs`
- Modify: `Org.Edgerunner.Mud.MCP/Interfaces/IMCPSession.cs`
- Modify: `Org.Edgerunner.Mud.MCP/McpClientSession.cs`

No tests for pure interface changes — verified by build success.

- [ ] **Step 1: Update `IMcpProtocolHandler.cs`**

Replace the entire file body (keep the BSD license header):

```csharp
using Org.Edgerunner.Mud.Communication;
using Org.Edgerunner.Mud.Communication.Interfaces;

namespace Org.Edgerunner.Mud.MCP.Interfaces;

public interface IMcpProtocolHandler
{
    public bool CanHandleMessage(Message message);
    public bool ProcessMessage(IClientTerminal client, Message message);
}
```

> Note: `Message` here is `Org.Edgerunner.Mud.MCP.Message`, not a Communication type. The `using Org.Edgerunner.Mud.Communication` import is needed for `IClientTerminal`.

- [ ] **Step 2: Update `IMCPPackage.cs`**

Replace the interface body (keep BSD header):

```csharp
namespace Org.Edgerunner.Mud.MCP.Interfaces;

public interface IMcpPackage : IMcpProtocolHandler
{
    string Name { get; set; }
    double MinimumVersion { get; set; }
    double MaximumVersion { get; set; }
    void SetSession(McpClientSession session);
}
```

- [ ] **Step 3: Update `IMCPSession.cs`**

Add `IsNegotiationComplete` to the interface body (keep BSD header):

```csharp
namespace Org.Edgerunner.Mud.MCP.Interfaces;

public interface IMcpSession
{
    McpClientSessionManager Manager { get; }
    string Key { get; }
    Version ProtocolVersion { get; }
    Dictionary<string, IMcpPackage> SupportedPackages { get; }
    bool IsNegotiationComplete { get; set; }
    string Handshake();
}
```

- [ ] **Step 4: Update `McpClientSession.cs`**

Add the `IsNegotiationComplete` property after the existing `SupportedPackages` property:

```csharp
public bool IsNegotiationComplete { get; set; }
```

- [ ] **Step 5: Verify build**

```bash
dotnet build "Org.Edgerunner.Mud.MCP/Org.Edgerunner.Mud.MCP.csproj"
```

Expected: `Build succeeded.` (No implementations exist yet so no cascade failures.)

- [ ] **Step 6: Commit**

```bash
git add Org.Edgerunner.Mud.MCP/Interfaces/ Org.Edgerunner.Mud.MCP/McpClientSession.cs
git commit -m "Update MCP interfaces: ProcessMessage adds IClientTerminal, IMcpPackage adds SetSession, IMcpSession adds IsNegotiationComplete"
```

---

## Task 3: McpMessageParser — Single-Line Parsing

**Files:**
- Create: `Org.Edgerunner.Mud.MCP/McpMessageParser.cs`
- Create: `Org.Edgerunner.Mud.MCP.Tests/McpMessageParserTests.cs`

**Background:** `McpUtils.SplitMessageIntoWords` (internal static, already exists) is the tokeniser. It handles quoted strings and colon-terminated keywords. When it processes `package: mcp-edit`, it returns `["package:", "mcp-edit"]`. Keywords always end with `:` in the token stream.

- [ ] **Step 1: Write failing tests for single-line parsing**

Create `Org.Edgerunner.Mud.MCP.Tests/McpMessageParserTests.cs`:

```csharp
using FluentAssertions;
using Org.Edgerunner.Mud.MCP;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpMessageParserTests
{
    private readonly McpMessageParser _parser = new();

    [Fact]
    public void FeedLine_MessageNameOnly_ReturnsComplete()
    {
        var result = _parser.FeedLine("mcp");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Name.Should().Be("mcp");
        _parser.Result.Key.Should().BeEmpty();
        _parser.Result.Data.Should().BeEmpty();
    }

    [Fact]
    public void FeedLine_HandshakeNoAuthKey_ExtractsVersionFields()
    {
        var result = _parser.FeedLine("mcp version: 2.1 to: 2.1");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Name.Should().Be("mcp");
        _parser.Result.Key.Should().BeEmpty();
        _parser.Result.Data["version:"].Should().Be("2.1");
        _parser.Result.Data["to:"].Should().Be("2.1");
    }

    [Fact]
    public void FeedLine_MessageWithAuthKey_ExtractsKey()
    {
        var result = _parser.FeedLine("mcp-negotiate-can abc123 package: mcp-edit min-version: 1.0 max-version: 1.0");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Name.Should().Be("mcp-negotiate-can");
        _parser.Result.Key.Should().Be("abc123");
        _parser.Result.Data["package:"].Should().Be("mcp-edit");
        _parser.Result.Data["min-version:"].Should().Be("1.0");
        _parser.Result.Data["max-version:"].Should().Be("1.0");
    }

    [Fact]
    public void FeedLine_QuotedValue_UnquotesValue()
    {
        var result = _parser.FeedLine("mcp-edit-set abc123 name: \"My Object:verb\"");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Data["name:"].Should().Be("My Object:verb");
    }

    [Fact]
    public void FeedLine_ResultIsNullBeforeComplete()
    {
        _parser.FeedLine("mcp-edit-set abc123 content*: dt42");

        _parser.Result.Should().BeNull();
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

```bash
dotnet test "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj" --filter "FullyQualifiedName~McpMessageParserTests"
```

Expected: Build error — `McpMessageParser` and `McpParseState` do not exist yet.

- [ ] **Step 3: Create `McpMessageParser.cs` with single-line support**

Create `Org.Edgerunner.Mud.MCP/McpMessageParser.cs`:

```csharp
namespace Org.Edgerunner.Mud.MCP;

public enum McpParseState { InProgress, Complete, Error }

public class McpMessageParser
{
   private enum InternalState { Normal, InMultiline }

   private InternalState _state = InternalState.Normal;
   private string _name = string.Empty;
   private string _key = string.Empty;
   private Dictionary<string, string> _simpleFields = new();
   private Dictionary<string, string> _dataTagToKeyword = new();
   private Dictionary<string, List<string>> _multilineBuffers = new();

   public Message? Result { get; private set; }

   public McpParseState FeedLine(string line)
   {
      Result = null;
      return _state == InternalState.Normal
         ? ProcessNormalLine(line)
         : ProcessMultilineLine(line);
   }

   private McpParseState ProcessNormalLine(string line)
   {
      if (string.IsNullOrWhiteSpace(line))
         return McpParseState.Error;

      if (line.StartsWith("* ") || line == "*" || line.StartsWith(": ") || line == ":")
         return McpParseState.Error;

      try
      {
         var words = McpUtils.SplitMessageIntoWords(line);
         if (words.Count == 0)
            return McpParseState.Error;

         _name = words[0];
         words.RemoveAt(0);

         _key = string.Empty;
         if (words.Count > 0 && !words[0].EndsWith(':'))
         {
            _key = words[0];
            words.RemoveAt(0);
         }

         _simpleFields = new Dictionary<string, string>();
         _dataTagToKeyword = new Dictionary<string, string>();
         _multilineBuffers = new Dictionary<string, List<string>>();

         bool hasMultiline = false;
         string currentKey = string.Empty;

         foreach (var word in words)
         {
            if (word.EndsWith(':'))
            {
               currentKey = word;
            }
            else if (!string.IsNullOrEmpty(currentKey))
            {
               if (currentKey.EndsWith("*:"))
               {
                  var fieldName = currentKey[..^2];
                  _dataTagToKeyword[word] = fieldName;
                  _multilineBuffers[word] = new List<string>();
                  hasMultiline = true;
               }
               else
               {
                  _simpleFields[currentKey.ToLowerInvariant()] = word;
               }
               currentKey = string.Empty;
            }
         }

         if (hasMultiline)
         {
            _state = InternalState.InMultiline;
            return McpParseState.InProgress;
         }

         Result = new Message(_name, _key, _simpleFields);
         return McpParseState.Complete;
      }
      catch
      {
         return McpParseState.Error;
      }
   }

   private McpParseState ProcessMultilineLine(string line)
   {
      // Placeholder — multiline support added in Task 4
      return McpParseState.Error;
   }

   public void Reset()
   {
      _state = InternalState.Normal;
      _name = string.Empty;
      _key = string.Empty;
      _simpleFields = new Dictionary<string, string>();
      _dataTagToKeyword = new Dictionary<string, string>();
      _multilineBuffers = new Dictionary<string, List<string>>();
      Result = null;
   }
}
```

- [ ] **Step 4: Run single-line tests to confirm they pass**

```bash
dotnet test "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj" --filter "FullyQualifiedName~McpMessageParserTests"
```

Expected: 5 passed, 0 failed. (`FeedLine_ResultIsNullBeforeComplete` passes because multiline header returns `InProgress` and `Result` stays null.)

- [ ] **Step 5: Commit**

```bash
git add Org.Edgerunner.Mud.MCP/McpMessageParser.cs Org.Edgerunner.Mud.MCP.Tests/McpMessageParserTests.cs
git commit -m "Add McpMessageParser single-line parsing with McpParseState enum"
```

---

## Task 4: McpMessageParser — Multiline Parsing

**Files:**
- Modify: `Org.Edgerunner.Mud.MCP/McpMessageParser.cs`
- Modify: `Org.Edgerunner.Mud.MCP.Tests/McpMessageParserTests.cs`

**Background:** Multiline MCP messages use a data-tag to correlate continuation lines. The header line has `keyword*: <data-tag>`. Continuation lines are `* <data-tag> keyword: value`. The close tag is `: <data-tag>`. All arrive with `#$#` already stripped by `RootMessageProcessor`. `SplitMessageIntoWords` on `* dt42 keyword: value` returns `["*", "dt42", "keyword:", "value"]`. On `: dt42` it returns `[":", "dt42"]`.

- [ ] **Step 1: Add multiline tests**

Append to `McpMessageParserTests.cs`:

```csharp
[Fact]
public void FeedLine_MultilineHeader_ReturnsInProgress()
{
    var result = _parser.FeedLine("mcp-edit-set abc123 name: foo content*: dt42");

    result.Should().Be(McpParseState.InProgress);
    _parser.Result.Should().BeNull();
}

[Fact]
public void FeedLine_MultilineComplete_AssemblesFieldsFromContinuationLines()
{
    _parser.FeedLine("mcp-edit-set abc123 name: foo content*: dt42");
    _parser.FeedLine("* dt42 content: first line");
    _parser.FeedLine("* dt42 content: second line");
    var result = _parser.FeedLine(": dt42");

    result.Should().Be(McpParseState.Complete);
    _parser.Result!.Name.Should().Be("mcp-edit-set");
    _parser.Result.Key.Should().Be("abc123");
    _parser.Result.Data["name:"].Should().Be("foo");
    _parser.Result.Data["content:"].Should().Be("first line\nsecond line");
}

[Fact]
public void FeedLine_MultilineWithNoContentLines_AssemblesEmptyField()
{
    _parser.FeedLine("mcp-edit-set abc123 content*: dt42");
    var result = _parser.FeedLine(": dt42");

    result.Should().Be(McpParseState.Complete);
    _parser.Result!.Data["content:"].Should().Be(string.Empty);
}

[Fact]
public void FeedLine_UnexpectedContinuationInNormalState_ReturnsError()
{
    var result = _parser.FeedLine("* dt42 content: value");

    result.Should().Be(McpParseState.Error);
}

[Fact]
public void FeedLine_UnexpectedCloseInNormalState_ReturnsError()
{
    var result = _parser.FeedLine(": dt42");

    result.Should().Be(McpParseState.Error);
}

[Fact]
public void Reset_AfterMultilineInProgress_AllowsFreshParse()
{
    _parser.FeedLine("mcp-edit-set abc123 content*: dt42");
    _parser.Reset();

    var result = _parser.FeedLine("mcp version: 2.1 to: 2.1");

    result.Should().Be(McpParseState.Complete);
    _parser.Result!.Name.Should().Be("mcp");
}
```

- [ ] **Step 2: Run new tests to confirm they fail**

```bash
dotnet test "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj" --filter "FullyQualifiedName~McpMessageParserTests"
```

Expected: The 4 new multiline tests fail; the 5 single-line tests still pass.

- [ ] **Step 3: Implement `ProcessMultilineLine`**

Replace the `ProcessMultilineLine` placeholder in `McpMessageParser.cs`:

```csharp
private McpParseState ProcessMultilineLine(string line)
{
   var words = McpUtils.SplitMessageIntoWords(line);

   if (words.Count >= 2 && words[0] == "*")
   {
      var tag = words[1];
      var value = words.Count >= 4 ? words[3] : string.Empty;

      if (_multilineBuffers.TryGetValue(tag, out var buffer))
         buffer.Add(value);

      return McpParseState.InProgress;
   }

   if (words.Count >= 1 && words[0] == ":")
   {
      var data = new Dictionary<string, string>(_simpleFields);
      foreach (var (tag, lines) in _multilineBuffers)
      {
         if (_dataTagToKeyword.TryGetValue(tag, out var fieldName))
            data[fieldName.ToLowerInvariant() + ":"] = string.Join("\n", lines);
      }

      Result = new Message(_name, _key, data);
      _state = InternalState.Normal;
      return McpParseState.Complete;
   }

   return McpParseState.Error;
}
```

- [ ] **Step 4: Run all parser tests to confirm they pass**

```bash
dotnet test "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj" --filter "FullyQualifiedName~McpMessageParserTests"
```

Expected: 11 passed, 0 failed.

- [ ] **Step 5: Commit**

```bash
git add Org.Edgerunner.Mud.MCP/McpMessageParser.cs Org.Edgerunner.Mud.MCP.Tests/McpMessageParserTests.cs
git commit -m "Add McpMessageParser multiline support"
```

---

## Task 5: McpUtils — Add FormatMessage, Remove ParseMessage

**Files:**
- Modify: `Org.Edgerunner.Mud.MCP/McpUtils.cs`
- Create: `Org.Edgerunner.Mud.MCP.Tests/McpUtilsTests.cs`

**Background:** `FormatMessage` builds outbound MCP wire strings (without the `#$#` prefix — `IClientTerminal.SendOutOfBandLine` prepends it). Values containing spaces or tabs must be double-quoted. Keywords already include their trailing `:` as part of their key string (e.g., `"version:"`, `"package:"`).

- [ ] **Step 1: Write failing tests**

Create `Org.Edgerunner.Mud.MCP.Tests/McpUtilsTests.cs`:

```csharp
using FluentAssertions;
using Org.Edgerunner.Mud.MCP;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpUtilsTests
{
    [Fact]
    public void FormatMessage_NameAndKeyNoData_FormatsCorrectly()
    {
        var result = McpUtils.FormatMessage("mcp-negotiate-end", "abc123", new Dictionary<string, string>());

        result.Should().Be("mcp-negotiate-end abc123");
    }

    [Fact]
    public void FormatMessage_NoKey_OmitsKeySegment()
    {
        var result = McpUtils.FormatMessage("mcp", string.Empty, new Dictionary<string, string>
        {
            ["version:"] = "2.1",
            ["to:"] = "2.1"
        });

        result.Should().Be("mcp version: 2.1 to: 2.1");
    }

    [Fact]
    public void FormatMessage_ValueWithSpaces_QuotesValue()
    {
        var result = McpUtils.FormatMessage("mcp-edit-set", "abc123", new Dictionary<string, string>
        {
            ["name:"] = "My Object verb"
        });

        result.Should().Be("mcp-edit-set abc123 name: \"My Object verb\"");
    }

    [Fact]
    public void FormatMessage_SimpleValue_DoesNotQuote()
    {
        var result = McpUtils.FormatMessage("mcp-negotiate-can", "abc123", new Dictionary<string, string>
        {
            ["package:"] = "mcp-edit",
            ["min-version:"] = "1.0",
            ["max-version:"] = "1.0"
        });

        result.Should().Be("mcp-negotiate-can abc123 package: mcp-edit min-version: 1.0 max-version: 1.0");
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

```bash
dotnet test "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj" --filter "FullyQualifiedName~McpUtilsTests"
```

Expected: Build error — `McpUtils.FormatMessage` does not exist yet.

- [ ] **Step 3: Add `FormatMessage` to `McpUtils.cs` and remove `ParseMessage`**

In `McpUtils.cs`:
1. Delete the entire `ParseMessage` method (lines 53–111 in the current file).
2. Add `FormatMessage` and its helper before `GenerateSessionKey`:

```csharp
public static string FormatMessage(string name, string key, Dictionary<string, string> data)
{
   var sb = new StringBuilder();
   sb.Append(name);

   if (!string.IsNullOrEmpty(key))
   {
      sb.Append(' ');
      sb.Append(key);
   }

   foreach (var (k, v) in data)
   {
      sb.Append(' ');
      sb.Append(k);
      sb.Append(' ');
      if (NeedsQuoting(v))
      {
         sb.Append('"');
         sb.Append(v);
         sb.Append('"');
      }
      else
         sb.Append(v);
   }

   return sb.ToString();
}

private static bool NeedsQuoting(string value) =>
   string.IsNullOrEmpty(value) || value.Any(c => c == ' ' || c == '\t');
```

- [ ] **Step 4: Run FormatMessage tests to confirm they pass**

```bash
dotnet test "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj" --filter "FullyQualifiedName~McpUtilsTests"
```

Expected: 4 passed, 0 failed.

- [ ] **Step 5: Verify full solution still builds**

```bash
dotnet build "Moo Developer Tools.sln"
```

Expected: `Build succeeded.` (`ParseMessage` had no callers so no cascade failures.)

- [ ] **Step 6: Commit**

```bash
git add Org.Edgerunner.Mud.MCP/McpUtils.cs Org.Edgerunner.Mud.MCP.Tests/McpUtilsTests.cs
git commit -m "Add McpUtils.FormatMessage; remove unused ParseMessage"
```

---

## Task 6: McpClientSessionManager Tests

**Files:**
- Create: `Org.Edgerunner.Mud.MCP.Tests/McpClientSessionManagerTests.cs`

These tests cover the existing negotiation logic using the new `McpMessageParser`.

- [ ] **Step 1: Write tests**

Create `Org.Edgerunner.Mud.MCP.Tests/McpClientSessionManagerTests.cs`:

```csharp
using FluentAssertions;
using Org.Edgerunner.Mud.MCP;
using Org.Edgerunner.Mud.MCP.Interfaces;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpClientSessionManagerTests
{
    private static Message ParseHandshake(string line)
    {
        var parser = new McpMessageParser();
        parser.FeedLine(line);
        return parser.Result!;
    }

    [Fact]
    public void NegotiationMcpSession_OverlappingVersions_ReturnsSession()
    {
        var manager = new McpClientSessionManager(new Version(2, 1), new Version(2, 1), new List<IMcpPackage>());
        var msg = ParseHandshake("mcp version: 2.1 to: 2.1");

        var session = manager.NegotiationMcpSession(msg);

        session.Should().NotBeNull();
        session!.ProtocolVersion.Should().Be(new Version(2, 1));
        session.Key.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NegotiationMcpSession_NonOverlappingVersions_ReturnsNull()
    {
        var manager = new McpClientSessionManager(new Version(2, 1), new Version(2, 1), new List<IMcpPackage>());
        var msg = ParseHandshake("mcp version: 1.0 to: 1.0");

        var session = manager.NegotiationMcpSession(msg);

        session.Should().BeNull();
    }

    [Fact]
    public void NegotiationMcpSession_GeneratesUniqueKeyPerSession()
    {
        var manager = new McpClientSessionManager(new Version(2, 1), new Version(2, 1), new List<IMcpPackage>());

        var session1 = manager.NegotiationMcpSession(ParseHandshake("mcp version: 2.1 to: 2.1"));
        var session2 = manager.NegotiationMcpSession(ParseHandshake("mcp version: 2.1 to: 2.1"));

        session1!.Key.Should().NotBe(session2!.Key);
    }
}
```

- [ ] **Step 2: Run tests**

```bash
dotnet test "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj" --filter "FullyQualifiedName~McpClientSessionManagerTests"
```

Expected: 3 passed, 0 failed.

- [ ] **Step 3: Commit**

```bash
git add Org.Edgerunner.Mud.MCP.Tests/McpClientSessionManagerTests.cs
git commit -m "Add McpClientSessionManager tests"
```

---

## Task 7: McpNegotiatePackage

**Files:**
- Create: `Org.Edgerunner.Mud.MCP/Packages/McpNegotiatePackage.cs`
- Create: `Org.Edgerunner.Mud.MCP.Tests/McpNegotiatePackageTests.cs`

**Background:** `McpNegotiatePackage` receives `mcp-negotiate-can` messages from the server advertising its supported packages. It checks whether the same package is in the client's registry (`_registeredPackages`) and whether their version ranges overlap. If so, the package is added to `session.SupportedPackages`. `mcp-negotiate-end` marks negotiation as complete.

- [ ] **Step 1: Write failing tests**

Create `Org.Edgerunner.Mud.MCP.Tests/McpNegotiatePackageTests.cs`:

```csharp
using FluentAssertions;
using NSubstitute;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP;
using Org.Edgerunner.Mud.MCP.Interfaces;
using Org.Edgerunner.Mud.MCP.Packages;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpNegotiatePackageTests
{
    private static McpClientSession CreateSession()
    {
        var manager = new McpClientSessionManager(new Version(2, 1), new Version(2, 1), new List<IMcpPackage>());
        var parser = new McpMessageParser();
        parser.FeedLine("mcp version: 2.1 to: 2.1");
        return (McpClientSession)manager.NegotiationMcpSession(parser.Result!)!;
    }

    [Fact]
    public void ProcessMessage_NegotiateCan_MatchingVersions_RecordsPackage()
    {
        var client = Substitute.For<IClientTerminal>();
        var mockPackage = Substitute.For<IMcpPackage>();
        mockPackage.Name.Returns("mcp-edit");
        mockPackage.MinimumVersion.Returns(1.0);
        mockPackage.MaximumVersion.Returns(1.0);

        var registry = new Dictionary<string, IMcpPackage> { ["mcp-edit"] = mockPackage };
        var package = new McpNegotiatePackage(registry);
        var session = CreateSession();
        package.SetSession(session);

        var msg = new Message("mcp-negotiate-can", session.Key, new Dictionary<string, string>
        {
            ["package:"] = "mcp-edit",
            ["min-version:"] = "1.0",
            ["max-version:"] = "1.0"
        });

        package.ProcessMessage(client, msg);

        session.SupportedPackages.Should().ContainKey("mcp-edit");
    }

    [Fact]
    public void ProcessMessage_NegotiateCan_VersionMismatch_DoesNotRecord()
    {
        var client = Substitute.For<IClientTerminal>();
        var mockPackage = Substitute.For<IMcpPackage>();
        mockPackage.Name.Returns("mcp-edit");
        mockPackage.MinimumVersion.Returns(2.0);
        mockPackage.MaximumVersion.Returns(2.0);

        var registry = new Dictionary<string, IMcpPackage> { ["mcp-edit"] = mockPackage };
        var package = new McpNegotiatePackage(registry);
        var session = CreateSession();
        package.SetSession(session);

        var msg = new Message("mcp-negotiate-can", session.Key, new Dictionary<string, string>
        {
            ["package:"] = "mcp-edit",
            ["min-version:"] = "1.0",
            ["max-version:"] = "1.0"
        });

        package.ProcessMessage(client, msg);

        session.SupportedPackages.Should().NotContainKey("mcp-edit");
    }

    [Fact]
    public void ProcessMessage_NegotiateCan_UnknownPackage_DoesNotThrow()
    {
        var client = Substitute.For<IClientTerminal>();
        var package = new McpNegotiatePackage(new Dictionary<string, IMcpPackage>());
        package.SetSession(CreateSession());

        var msg = new Message("mcp-negotiate-can", "key", new Dictionary<string, string>
        {
            ["package:"] = "mcp-unknown",
            ["min-version:"] = "1.0",
            ["max-version:"] = "1.0"
        });

        var act = () => package.ProcessMessage(client, msg);
        act.Should().NotThrow();
    }

    [Fact]
    public void ProcessMessage_NegotiateEnd_SetsIsNegotiationComplete()
    {
        var client = Substitute.For<IClientTerminal>();
        var package = new McpNegotiatePackage(new Dictionary<string, IMcpPackage>());
        var session = CreateSession();
        package.SetSession(session);

        package.ProcessMessage(client, new Message("mcp-negotiate-end", session.Key, new Dictionary<string, string>()));

        session.IsNegotiationComplete.Should().BeTrue();
    }

    [Fact]
    public void CanHandleMessage_NegotiateCan_ReturnsTrue()
    {
        var package = new McpNegotiatePackage(new Dictionary<string, IMcpPackage>());
        var msg = new Message("mcp-negotiate-can", "key", new Dictionary<string, string>());

        package.CanHandleMessage(msg).Should().BeTrue();
    }

    [Fact]
    public void CanHandleMessage_NegotiateEnd_ReturnsTrue()
    {
        var package = new McpNegotiatePackage(new Dictionary<string, IMcpPackage>());
        var msg = new Message("mcp-negotiate-end", "key", new Dictionary<string, string>());

        package.CanHandleMessage(msg).Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

```bash
dotnet test "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj" --filter "FullyQualifiedName~McpNegotiatePackageTests"
```

Expected: Build error — `McpNegotiatePackage` does not exist.

- [ ] **Step 3: Create `Packages/` folder and `McpNegotiatePackage.cs`**

Create `Org.Edgerunner.Mud.MCP/Packages/McpNegotiatePackage.cs`:

```csharp
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP.Interfaces;

namespace Org.Edgerunner.Mud.MCP.Packages;

public class McpNegotiatePackage : IMcpPackage
{
   private McpClientSession? _session;
   private readonly Dictionary<string, IMcpPackage> _registeredPackages;

   public McpNegotiatePackage(Dictionary<string, IMcpPackage> registeredPackages)
   {
      _registeredPackages = registeredPackages;
   }

   public string Name { get; set; } = "mcp-negotiate";
   public double MinimumVersion { get; set; } = 1.0;
   public double MaximumVersion { get; set; } = 2.0;

   public void SetSession(McpClientSession session) => _session = session;

   public bool CanHandleMessage(Message message)
   {
      var name = message.Name.ToLowerInvariant();
      return name is "mcp-negotiate-can" or "mcp-negotiate-end";
   }

   public bool ProcessMessage(IClientTerminal client, Message message)
   {
      return message.Name.ToLowerInvariant() switch
      {
         "mcp-negotiate-can" => ProcessNegotiateCan(message),
         "mcp-negotiate-end" => ProcessNegotiateEnd(),
         _ => false
      };
   }

   private bool ProcessNegotiateCan(Message message)
   {
      if (_session == null) return false;

      if (!message.Data.TryGetValue("package:", out var packageName)) return true;
      if (!message.Data.TryGetValue("min-version:", out var minStr)) return true;
      if (!message.Data.TryGetValue("max-version:", out var maxStr)) return true;

      if (!double.TryParse(minStr, out var serverMin) ||
          !double.TryParse(maxStr, out var serverMax))
         return true;

      if (!_registeredPackages.TryGetValue(packageName.ToLowerInvariant(), out var pkg))
         return true;

      if (pkg.MaximumVersion < serverMin || serverMax < pkg.MinimumVersion)
         return true;

      _session.SupportedPackages[packageName.ToLowerInvariant()] = pkg;
      return true;
   }

   private bool ProcessNegotiateEnd()
   {
      if (_session != null)
         _session.IsNegotiationComplete = true;
      return true;
   }

   public void Reset() { }
}
```

- [ ] **Step 4: Run tests to confirm they pass**

```bash
dotnet test "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj" --filter "FullyQualifiedName~McpNegotiatePackageTests"
```

Expected: 6 passed, 0 failed.

- [ ] **Step 5: Commit**

```bash
git add Org.Edgerunner.Mud.MCP/Packages/ Org.Edgerunner.Mud.MCP.Tests/McpNegotiatePackageTests.cs
git commit -m "Add McpNegotiatePackage with mcp-negotiate-can and mcp-negotiate-end handling"
```

---

## Task 8: McpCord and McpCordPackage

**Files:**
- Create: `Org.Edgerunner.Mud.MCP/Packages/McpCord.cs`
- Create: `Org.Edgerunner.Mud.MCP/Packages/McpCordPackage.cs`
- Create: `Org.Edgerunner.Mud.MCP.Tests/McpCordPackageTests.cs`

**Background:** Cords are server-initiated (ID prefix `I`) or client-initiated (prefix `R`) multiplexed channels. `mcp-cord-open` creates one, `mcp-cord` sends a message through it (routed to a handler registered for the cord's type), `mcp-cord-closed` tears it down. The `_id:` and `_type:` fields use underscore-prefixed names.

- [ ] **Step 1: Write failing tests**

Create `Org.Edgerunner.Mud.MCP.Tests/McpCordPackageTests.cs`:

```csharp
using FluentAssertions;
using NSubstitute;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP;
using Org.Edgerunner.Mud.MCP.Interfaces;
using Org.Edgerunner.Mud.MCP.Packages;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpCordPackageTests
{
    private static Message CordOpenMsg(string id = "I12345", string type = "whiteboard") =>
        new("mcp-cord-open", "key123", new Dictionary<string, string>
        {
            ["_id:"] = id,
            ["_type:"] = type
        });

    [Fact]
    public void ProcessMessage_CordOpen_ReturnsTrueAndCreatesCord()
    {
        var client = Substitute.For<IClientTerminal>();
        var package = new McpCordPackage(new Dictionary<string, IMcpPackage>());

        var result = package.ProcessMessage(client, CordOpenMsg());

        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessMessage_CordClosed_ReturnsTrueAndRemovesCord()
    {
        var client = Substitute.For<IClientTerminal>();
        var package = new McpCordPackage(new Dictionary<string, IMcpPackage>());
        package.ProcessMessage(client, CordOpenMsg());

        var closeMsg = new Message("mcp-cord-closed", "key123", new Dictionary<string, string>
        {
            ["_id:"] = "I12345"
        });
        var result = package.ProcessMessage(client, closeMsg);

        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessMessage_CordMessage_RoutesToRegisteredHandler()
    {
        var client = Substitute.For<IClientTerminal>();
        var handler = Substitute.For<IMcpPackage>();
        handler.ProcessMessage(Arg.Any<IClientTerminal>(), Arg.Any<Message>()).Returns(true);

        var package = new McpCordPackage(new Dictionary<string, IMcpPackage>
        {
            ["whiteboard"] = handler
        });

        package.ProcessMessage(client, CordOpenMsg());

        package.ProcessMessage(client, new Message("mcp-cord", "key123", new Dictionary<string, string>
        {
            ["_id:"] = "I12345",
            ["_message:"] = "draw",
            ["color:"] = "red"
        }));

        handler.Received(1).ProcessMessage(client, Arg.Is<Message>(m => m.Name == "draw"));
    }

    [Fact]
    public void ProcessMessage_CordMessage_NoHandlerRegistered_DoesNotThrow()
    {
        var client = Substitute.For<IClientTerminal>();
        var package = new McpCordPackage(new Dictionary<string, IMcpPackage>());
        package.ProcessMessage(client, CordOpenMsg());

        var act = () => package.ProcessMessage(client, new Message("mcp-cord", "key123",
            new Dictionary<string, string> { ["_id:"] = "I12345", ["_message:"] = "draw" }));

        act.Should().NotThrow();
    }

    [Fact]
    public void ProcessMessage_CordMessage_ClosedCord_ReturnsFalse()
    {
        var client = Substitute.For<IClientTerminal>();
        var package = new McpCordPackage(new Dictionary<string, IMcpPackage>());
        package.ProcessMessage(client, CordOpenMsg());
        package.ProcessMessage(client, new Message("mcp-cord-closed", "key123",
            new Dictionary<string, string> { ["_id:"] = "I12345" }));

        var result = package.ProcessMessage(client, new Message("mcp-cord", "key123",
            new Dictionary<string, string> { ["_id:"] = "I12345", ["_message:"] = "draw" }));

        result.Should().BeFalse();
    }

    [Fact]
    public void CanHandleMessage_CordMessages_ReturnsTrue()
    {
        var package = new McpCordPackage(new Dictionary<string, IMcpPackage>());

        package.CanHandleMessage(new Message("mcp-cord-open", "key", new Dictionary<string, string>())).Should().BeTrue();
        package.CanHandleMessage(new Message("mcp-cord", "key", new Dictionary<string, string>())).Should().BeTrue();
        package.CanHandleMessage(new Message("mcp-cord-closed", "key", new Dictionary<string, string>())).Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

```bash
dotnet test "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj" --filter "FullyQualifiedName~McpCordPackageTests"
```

Expected: Build error — `McpCord` and `McpCordPackage` do not exist.

- [ ] **Step 3: Create `McpCord.cs`**

Create `Org.Edgerunner.Mud.MCP/Packages/McpCord.cs`:

```csharp
namespace Org.Edgerunner.Mud.MCP.Packages;

public class McpCord
{
   public McpCord(string id, string type)
   {
      Id = id;
      Type = type;
      IsOpen = true;
   }

   public string Id { get; }
   public string Type { get; }
   public bool IsOpen { get; set; }
}
```

- [ ] **Step 4: Create `McpCordPackage.cs`**

Create `Org.Edgerunner.Mud.MCP/Packages/McpCordPackage.cs`:

```csharp
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP.Interfaces;

namespace Org.Edgerunner.Mud.MCP.Packages;

public class McpCordPackage : IMcpPackage
{
   private McpClientSession? _session;
   private readonly Dictionary<string, McpCord> _cords = new();
   private readonly Dictionary<string, IMcpPackage> _cordTypeHandlers;

   public McpCordPackage(Dictionary<string, IMcpPackage> cordTypeHandlers)
   {
      _cordTypeHandlers = cordTypeHandlers;
   }

   public string Name { get; set; } = "mcp-cord";
   public double MinimumVersion { get; set; } = 1.0;
   public double MaximumVersion { get; set; } = 1.0;

   public void SetSession(McpClientSession session) => _session = session;

   public bool CanHandleMessage(Message message)
   {
      var name = message.Name.ToLowerInvariant();
      return name is "mcp-cord-open" or "mcp-cord" or "mcp-cord-closed";
   }

   public bool ProcessMessage(IClientTerminal client, Message message)
   {
      return message.Name.ToLowerInvariant() switch
      {
         "mcp-cord-open"   => ProcessCordOpen(message),
         "mcp-cord"        => ProcessCord(client, message),
         "mcp-cord-closed" => ProcessCordClosed(message),
         _ => false
      };
   }

   private bool ProcessCordOpen(Message message)
   {
      if (!message.Data.TryGetValue("_id:", out var id)) return false;
      if (!message.Data.TryGetValue("_type:", out var type)) return false;

      _cords[id] = new McpCord(id, type);
      return true;
   }

   private bool ProcessCord(IClientTerminal client, Message message)
   {
      if (!message.Data.TryGetValue("_id:", out var id)) return false;
      if (!_cords.TryGetValue(id, out var cord) || !cord.IsOpen) return false;
      if (!message.Data.TryGetValue("_message:", out var cordMessage)) return false;

      if (_cordTypeHandlers.TryGetValue(cord.Type.ToLowerInvariant(), out var handler))
      {
         var innerData = message.Data
            .Where(kvp => kvp.Key != "_id:" && kvp.Key != "_message:")
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
         handler.ProcessMessage(client, new Message(cordMessage, message.Key, innerData));
      }

      return true;
   }

   private bool ProcessCordClosed(Message message)
   {
      if (!message.Data.TryGetValue("_id:", out var id)) return false;

      if (_cords.TryGetValue(id, out var cord))
      {
         cord.IsOpen = false;
         _cords.Remove(id);
      }

      return true;
   }

   public void Reset() => _cords.Clear();
}
```

- [ ] **Step 5: Run tests to confirm they pass**

```bash
dotnet test "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj" --filter "FullyQualifiedName~McpCordPackageTests"
```

Expected: 6 passed, 0 failed.

- [ ] **Step 6: Commit**

```bash
git add Org.Edgerunner.Mud.MCP/Packages/ Org.Edgerunner.Mud.MCP.Tests/McpCordPackageTests.cs
git commit -m "Add McpCord model and McpCordPackage with cord lifecycle handling"
```

---

## Task 9: McpMessageDispatcher

**Files:**
- Create: `Org.Edgerunner.Mud.MCP/McpMessageDispatcher.cs`
- Create: `Org.Edgerunner.Mud.MCP.Tests/McpMessageDispatcherTests.cs`

**Background:** The dispatcher owns the package registry and the session manager. On receiving the initial `mcp` handshake (name `"mcp"`, empty key), it creates a session and advertises all registered packages via `mcp-negotiate-can` + `mcp-negotiate-end`. For all other messages it validates the auth key and routes by longest-matching package name prefix. `SendOutOfBandLine` sends messages without the `#$#` prefix (the terminal's `SendOutOfBandLine` adds it).

- [ ] **Step 1: Write failing tests**

Create `Org.Edgerunner.Mud.MCP.Tests/McpMessageDispatcherTests.cs`:

```csharp
using FluentAssertions;
using NSubstitute;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpMessageDispatcherTests
{
    private static Message ParseLine(string line)
    {
        var parser = new McpMessageParser();
        parser.FeedLine(line);
        return parser.Result!;
    }

    [Fact]
    public void Dispatch_Handshake_SendsHandshakeReply()
    {
        var client = Substitute.For<IClientTerminal>();
        var dispatcher = new McpMessageDispatcher(new Version(2, 1), new Version(2, 1));

        dispatcher.Dispatch(client, ParseLine("mcp version: 2.1 to: 2.1"));

        client.Received().SendOutOfBandLine(Arg.Is<string>(s => s.StartsWith("mcp authentication-key:")));
    }

    [Fact]
    public void Dispatch_Handshake_SendsNegotiateCanForEachPackage()
    {
        var client = Substitute.For<IClientTerminal>();
        var dispatcher = new McpMessageDispatcher(new Version(2, 1), new Version(2, 1));

        dispatcher.Dispatch(client, ParseLine("mcp version: 2.1 to: 2.1"));

        client.Received().SendOutOfBandLine(Arg.Is<string>(s =>
            s.Contains("mcp-negotiate-can") && s.Contains("package: mcp-negotiate")));
        client.Received().SendOutOfBandLine(Arg.Is<string>(s =>
            s.Contains("mcp-negotiate-can") && s.Contains("package: mcp-cord")));
    }

    [Fact]
    public void Dispatch_Handshake_SendsNegotiateEnd()
    {
        var client = Substitute.For<IClientTerminal>();
        var dispatcher = new McpMessageDispatcher(new Version(2, 1), new Version(2, 1));

        dispatcher.Dispatch(client, ParseLine("mcp version: 2.1 to: 2.1"));

        client.Received().SendOutOfBandLine(Arg.Is<string>(s => s.Contains("mcp-negotiate-end")));
    }

    [Fact]
    public void Dispatch_MessageBeforeHandshake_DropsMessage()
    {
        var client = Substitute.For<IClientTerminal>();
        var dispatcher = new McpMessageDispatcher(new Version(2, 1), new Version(2, 1));

        dispatcher.Dispatch(client, new Message("mcp-negotiate-end", "somekey",
            new Dictionary<string, string>()));

        client.DidNotReceive().SendOutOfBandLine(Arg.Any<string>());
    }

    [Fact]
    public void Dispatch_WrongAuthKey_DropsMessage()
    {
        var client = Substitute.For<IClientTerminal>();
        var dispatcher = new McpMessageDispatcher(new Version(2, 1), new Version(2, 1));
        dispatcher.Dispatch(client, ParseLine("mcp version: 2.1 to: 2.1"));
        client.ClearReceivedCalls();

        dispatcher.Dispatch(client, new Message("mcp-negotiate-end", "WRONGKEY",
            new Dictionary<string, string>()));

        client.DidNotReceive().SendOutOfBandLine(Arg.Any<string>());
    }

    [Fact]
    public void Dispatch_IncompatibleVersions_SessionRemainsNull()
    {
        var client = Substitute.For<IClientTerminal>();
        var dispatcher = new McpMessageDispatcher(new Version(2, 1), new Version(2, 1));

        dispatcher.Dispatch(client, ParseLine("mcp version: 1.0 to: 1.0"));

        dispatcher.Session.Should().BeNull();
        client.DidNotReceive().SendOutOfBandLine(Arg.Any<string>());
    }

    [Fact]
    public void Dispatch_UnknownPackage_DropsMessageSilently()
    {
        var client = Substitute.For<IClientTerminal>();
        var dispatcher = new McpMessageDispatcher(new Version(2, 1), new Version(2, 1));
        dispatcher.Dispatch(client, ParseLine("mcp version: 2.1 to: 2.1"));
        var key = dispatcher.Session!.Key;

        var act = () => dispatcher.Dispatch(client,
            new Message("unknown-thing-do", key, new Dictionary<string, string>()));

        act.Should().NotThrow();
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

```bash
dotnet test "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj" --filter "FullyQualifiedName~McpMessageDispatcherTests"
```

Expected: Build error — `McpMessageDispatcher` does not exist.

- [ ] **Step 3: Create `McpMessageDispatcher.cs`**

Create `Org.Edgerunner.Mud.MCP/McpMessageDispatcher.cs`:

```csharp
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP.Interfaces;
using Org.Edgerunner.Mud.MCP.Packages;

namespace Org.Edgerunner.Mud.MCP;

public class McpMessageDispatcher
{
   private readonly McpClientSessionManager _sessionManager;
   private readonly Dictionary<string, IMcpPackage> _packages = new();

   public McpClientSession? Session { get; private set; }

   public McpMessageDispatcher(Version minVersion, Version maxVersion)
   {
      _sessionManager = new McpClientSessionManager(minVersion, maxVersion, new List<IMcpPackage>());

      var negotiatePackage = new McpNegotiatePackage(_packages);
      var cordPackage = new McpCordPackage(new Dictionary<string, IMcpPackage>());

      _packages["mcp-negotiate"] = negotiatePackage;
      _packages["mcp-cord"] = cordPackage;
   }

   public void RegisterPackage(IMcpPackage package)
   {
      _packages[package.Name.ToLowerInvariant()] = package;
   }

   public void Dispatch(IClientTerminal client, Message message)
   {
      if (message.Name.ToLowerInvariant() == "mcp" && string.IsNullOrEmpty(message.Key))
      {
         ProcessHandshake(client, message);
         return;
      }

      if (Session == null) return;
      if (message.Key != Session.Key) return;

      var packageName = _packages.Keys
         .Where(k => message.Name.ToLowerInvariant().StartsWith(k))
         .OrderByDescending(k => k.Length)
         .FirstOrDefault();

      if (packageName == null) return;

      _packages[packageName].ProcessMessage(client, message);
   }

   private void ProcessHandshake(IClientTerminal client, Message message)
   {
      var session = _sessionManager.NegotiationMcpSession(message);
      if (session == null) return;

      Session = (McpClientSession)session;

      client.SendOutOfBandLine(Session.Handshake());

      foreach (var pkg in _packages.Values)
         pkg.SetSession(Session);

      foreach (var pkg in _packages.Values)
      {
         client.SendOutOfBandLine(McpUtils.FormatMessage(
            "mcp-negotiate-can",
            Session.Key,
            new Dictionary<string, string>
            {
               ["package:"] = pkg.Name,
               ["min-version:"] = pkg.MinimumVersion.ToString("F1"),
               ["max-version:"] = pkg.MaximumVersion.ToString("F1")
            }));
      }

      client.SendOutOfBandLine(McpUtils.FormatMessage(
         "mcp-negotiate-end", Session.Key, new Dictionary<string, string>()));
   }
}
```

- [ ] **Step 4: Run tests to confirm they pass**

```bash
dotnet test "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj" --filter "FullyQualifiedName~McpMessageDispatcherTests"
```

Expected: 7 passed, 0 failed.

- [ ] **Step 5: Commit**

```bash
git add Org.Edgerunner.Mud.MCP/McpMessageDispatcher.cs Org.Edgerunner.Mud.MCP.Tests/McpMessageDispatcherTests.cs
git commit -m "Add McpMessageDispatcher with handshake, auth key validation, and package routing"
```

---

## Task 10: McpOobHandler

**Files:**
- Create: `Org.Edgerunner.Mud.MCP/McpOobHandler.cs`
- Create: `Org.Edgerunner.Mud.MCP.Tests/McpOobHandlerTests.cs`

**Background:** `McpOobHandler` is the `IOutOfBandMessageHandler` that the `OutOfBandMessageProcessor` calls. Lines arrive with `#$#` already stripped by `RootMessageProcessor`. The `MessageProcessingState.CurrentProcessor` mechanism routes multiline continuation lines directly back here, bypassing the full OOB dispatch loop. Setting `state.Finished = true` after a complete message tells `RootMessageProcessor` to do a clean reset before the next message.

- [ ] **Step 1: Write failing tests**

Create `Org.Edgerunner.Mud.MCP.Tests/McpOobHandlerTests.cs`:

```csharp
using FluentAssertions;
using NSubstitute;
using Org.Edgerunner.Mud.Communication;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpOobHandlerTests
{
    [Fact]
    public void ProcessMessage_SingleLineMcpMessage_ReturnsTrue()
    {
        var client = Substitute.For<IClientTerminal>();
        var state = new MessageProcessingState();
        var handler = new McpOobHandler(new Version(2, 1), new Version(2, 1));

        var result = handler.ProcessMessage(client, "mcp version: 2.1 to: 2.1", ref state);

        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessMessage_SingleLineMcpMessage_SetsFinished()
    {
        var client = Substitute.For<IClientTerminal>();
        var state = new MessageProcessingState();
        var handler = new McpOobHandler(new Version(2, 1), new Version(2, 1));

        handler.ProcessMessage(client, "mcp version: 2.1 to: 2.1", ref state);

        state.Finished.Should().BeTrue();
        state.CurrentProcessor.Should().BeNull();
    }

    [Fact]
    public void ProcessMessage_MultilineHeader_SetsCurrentProcessorToSelf()
    {
        var client = Substitute.For<IClientTerminal>();
        var state = new MessageProcessingState();
        var handler = new McpOobHandler(new Version(2, 1), new Version(2, 1));

        handler.ProcessMessage(client, "mcp-edit-set abc123 content*: dt42", ref state);

        state.CurrentProcessor.Should().Be(handler);
        state.Finished.Should().BeFalse();
    }

    [Fact]
    public void ProcessMessage_MultilineContinuationThenClose_SetsFinished()
    {
        var client = Substitute.For<IClientTerminal>();
        var state = new MessageProcessingState();
        var handler = new McpOobHandler(new Version(2, 1), new Version(2, 1));

        handler.ProcessMessage(client, "mcp-edit-set abc123 content*: dt42", ref state);
        handler.ProcessMessage(client, "* dt42 content: hello", ref state);
        handler.ProcessMessage(client, ": dt42", ref state);

        state.Finished.Should().BeTrue();
        state.CurrentProcessor.Should().BeNull();
    }

    [Fact]
    public void ProcessMessage_MalformedLine_ReturnsTrueAndSetsFinished()
    {
        var client = Substitute.For<IClientTerminal>();
        var state = new MessageProcessingState();
        var handler = new McpOobHandler(new Version(2, 1), new Version(2, 1));

        var result = handler.ProcessMessage(client, string.Empty, ref state);

        result.Should().BeTrue();
        state.Finished.Should().BeTrue();
    }

    [Fact]
    public void Reset_ClearsParserState()
    {
        var client = Substitute.For<IClientTerminal>();
        var state = new MessageProcessingState();
        var handler = new McpOobHandler(new Version(2, 1), new Version(2, 1));

        handler.ProcessMessage(client, "mcp-edit-set abc123 content*: dt42", ref state);
        handler.Reset();

        // After reset, a complete single-line message should parse cleanly
        handler.ProcessMessage(client, "mcp version: 2.1 to: 2.1", ref state);
        state.Finished.Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

```bash
dotnet test "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj" --filter "FullyQualifiedName~McpOobHandlerTests"
```

Expected: Build error — `McpOobHandler` does not exist.

- [ ] **Step 3: Create `McpOobHandler.cs`**

Create `Org.Edgerunner.Mud.MCP/McpOobHandler.cs`:

```csharp
using Org.Edgerunner.Mud.Communication;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.Communication.OutOfBand;

namespace Org.Edgerunner.Mud.MCP;

public class McpOobHandler : IOutOfBandMessageHandler
{
   private readonly McpMessageParser _parser = new();
   private readonly McpMessageDispatcher _dispatcher;

   public McpOobHandler(Version minVersion, Version maxVersion)
   {
      _dispatcher = new McpMessageDispatcher(minVersion, maxVersion);
   }

   public bool ProcessMessage(IClientTerminal client, string line, ref MessageProcessingState state)
   {
      var result = _parser.FeedLine(line);

      switch (result)
      {
         case McpParseState.Complete:
            _dispatcher.Dispatch(client, _parser.Result!);
            _parser.Reset();
            state.CurrentProcessor = null;
            state.Finished = true;
            return true;

         case McpParseState.InProgress:
            state.CurrentProcessor = this;
            return true;

         default:
            _parser.Reset();
            state.CurrentProcessor = null;
            state.Finished = true;
            return true;
      }
   }

   public void Reset() => _parser.Reset();
}
```

- [ ] **Step 4: Run tests to confirm they pass**

```bash
dotnet test "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj" --filter "FullyQualifiedName~McpOobHandlerTests"
```

Expected: 6 passed, 0 failed.

- [ ] **Step 5: Run all tests**

```bash
dotnet test "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj"
```

Expected: All tests pass.

- [ ] **Step 6: Commit**

```bash
git add Org.Edgerunner.Mud.MCP/McpOobHandler.cs Org.Edgerunner.Mud.MCP.Tests/McpOobHandlerTests.cs
git commit -m "Add McpOobHandler wiring McpMessageParser to McpMessageDispatcher"
```

---

## Task 11: Wire Into WindowManager and Clean Up Terminal

**Files:**
- Modify: `Org.Edgerunner.Moo.Udditor/WindowManager.cs`
- Modify: `Org.Edgerunner.Moo.Editor/Controls/MooClientTerminal.cs`

No new tests — verified by build success and manual smoke test. This task has no unit tests because it touches WinForms UI code that cannot be exercised in a headless test.

- [ ] **Step 1: Register `McpOobHandler` in `WindowManager.CreateTerminalPage`**

In `WindowManager.cs`, locate the `CreateTerminalPage` method. The existing code is:

```csharp
var oobPrefix = "#$#";
var oobHandler = new OutOfBandMessageProcessor();
oobHandler.RegisterHandler(new LocalEditHandler(this));
var processor = new RootMessageProcessor(oobPrefix, oobHandler);
```

Add the MCP handler registration immediately after the `LocalEditHandler` line:

```csharp
var oobPrefix = "#$#";
var oobHandler = new OutOfBandMessageProcessor();
oobHandler.RegisterHandler(new LocalEditHandler(this));
oobHandler.RegisterHandler(new McpOobHandler(new Version(2, 1), new Version(2, 1)));
var processor = new RootMessageProcessor(oobPrefix, oobHandler);
```

Add the required using directive at the top of `WindowManager.cs` if not already present:

```csharp
using Org.Edgerunner.Mud.MCP;
```

- [ ] **Step 2: Remove the unused `McpSessionManager` from `MooClientTerminal.cs`**

In `MooClientTerminal.cs`:

1. Remove the property declaration:
```csharp
protected McpClientSessionManager McpSessionManager { get; set; }
```

2. Remove its initialization in the constructor:
```csharp
McpSessionManager = new McpClientSessionManager(new Version(2,1), new Version(2,1), new List<IMcpPackage>());
```

3. Remove the now-unused using directives (if no longer referenced):
```csharp
using Org.Edgerunner.Mud.MCP;
using Org.Edgerunner.Mud.MCP.Exceptions;
using Org.Edgerunner.Mud.MCP.Interfaces;
```

- [ ] **Step 3: Build the full solution**

```bash
dotnet build "Moo Developer Tools.sln"
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 4: Run all tests one final time**

```bash
dotnet test "Org.Edgerunner.Mud.MCP.Tests/Org.Edgerunner.Mud.MCP.Tests.csproj"
```

Expected: All tests pass.

- [ ] **Step 5: Commit**

```bash
git add Org.Edgerunner.Moo.Udditor/WindowManager.cs Org.Edgerunner.Moo.Editor/Controls/MooClientTerminal.cs
git commit -m "Wire McpOobHandler into WindowManager; remove unused McpSessionManager from MooClientTerminal"
```

---

## Self-Review Notes

**Spec coverage check:**
- ✅ `McpMessageParser` replaces `McpUtils.ParseMessage` (Tasks 3, 4)
- ✅ `McpUtils.FormatMessage` added (Task 5)
- ✅ `mcp-negotiate` package (Task 7)
- ✅ `mcp-cord` + `McpCord` (Task 8)
- ✅ `McpMessageDispatcher` with handshake, auth validation, routing (Task 9)
- ✅ `McpOobHandler` wiring parser to dispatcher (Task 10)
- ✅ Terminal wiring in `WindowManager` (Task 11)
- ✅ `IMcpProtocolHandler.ProcessMessage` signature updated (Task 2)
- ✅ `IMcpPackage.SetSession` added (Task 2)
- ✅ `IMcpSession.IsNegotiationComplete` added (Task 2)
- ✅ xUnit test project with FluentAssertions + NSubstitute (Task 1)
- ✅ `McpSessionManager` removed from `MooClientTerminal` (Task 11)
- ✅ Deferred: `IMcpConfiguration` concrete implementation (out of scope per spec)
- ✅ Deferred: Client-initiated cord creation (out of scope per spec)
