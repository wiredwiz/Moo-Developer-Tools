# MCP Dev-Info Query Package (udd-btl) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement both halves of the `edgerunner-org-moo-query` MCP 2.1 package — a Moo-code server package (dump file + install doc) and a client `IMcpPackage` backing a full-coverage `IMooWorldQueryProvider` registered at priority 200 — plus the normative protocol document.

**Architecture:** The client package participates in the standard MCP handshake; when the server confirms the package, it registers an `McpQueryProvider` with the connection's `QueryProviders`. Each query sends one tagged request message and awaits a tag-correlated JSON reply (multiline `data*` field), mapped to the typed `Org.Edgerunner.Mud.Common.Querying` models. The server half is a classic-LambdaMOO package object in the JHCore simpleedit style: one `handle_*` verb per request, all output JSON-encoded and chunked through a single `send_reply` verb.

**Tech Stack:** .NET 6 / C#, System.Text.Json, NLog 5.2.8, xunit + FluentAssertions + NSubstitute, classic LambdaMOO Moo code.

**Spec:** `docs/superpowers/specs/2026-06-10-mcp-query-package-design.md` (authoritative; MCP = MUD Client Protocol, NOT the LLM protocol).

---

## Ground rules for every task

- **Working directory:** `D:\Projects\Moo Developer Tools\.worktrees\mcp-query-package` (the `mcp-query-package` branch worktree). ALL file paths below are relative to it; ALL commands run from it.
- **NEVER run unfiltered `dotnet test`** anywhere in this solution — some test projects instantiate WinForms controls and crash the test host. Only run the exact filtered commands given in each step.
- **New C# files:** copy the `#region BSD 3-Clause License … #endregion` header block (lines 1–35) from `Org.Edgerunner.Mud.MCP/Packages/SimpleEditPackage.cs` to the top of each new `.cs` file, changing the `file="…"` attribute to the new file name and the copyright line to `Copyright (c) Thaddeus Ryker 2026`. The code listings below start at the `using` directives to save space — the header region is still required.
- The MCP project uses 3-space indentation, `ImplicitUsings`, and nullable enabled. Match it.
- Commit after every task with the exact `git add` paths listed.

---

## File map

| File | Responsibility |
|---|---|
| `docs/edgerunner-org-moo-query-protocol.md` (create) | Normative wire-protocol spec |
| `Org.Edgerunner.Mud.MCP/Org.Edgerunner.Mud.MCP.csproj` (modify) | Add NLog 5.2.8 |
| `Org.Edgerunner.Mud.MCP/Interfaces/IPackageNegotiationListener.cs` (create) | Negotiation-confirmation callback |
| `Org.Edgerunner.Mud.MCP/Packages/McpNegotiatePackage.cs` (modify) | Invoke the listener on package confirmation |
| `Org.Edgerunner.Mud.MCP/Exceptions/McpQueryErrorException.cs` (create) | Typed `-error` reply |
| `Org.Edgerunner.Mud.MCP/Packages/McpQueryCorrelator.cs` (create) | tag → TaskCompletionSource map |
| `Org.Edgerunner.Mud.MCP/Packages/McpQueryMapping.cs` (create) | JSON → typed-record mapping |
| `Org.Edgerunner.Mud.MCP/Packages/McpQueryProvider.cs` (create) | `IMooWorldQueryProvider` over MCP |
| `Org.Edgerunner.Mud.MCP/Packages/McpQueryPackage.cs` (create) | `IMcpPackage` + provider registration |
| `Org.Edgerunner.Moo.Udditor/WindowManager.cs` (modify) | Wire package into terminal pages |
| `Server Packages/edgerunner-org-moo-query.moo` (create) | Server package dump |
| `Server Packages/edgerunner-org-moo-query-INSTALL.md` (create) | Install/registration instructions |
| `Org.Edgerunner.Mud.MCP.Tests/NegotiationListenerTests.cs` (create) | Listener hook tests |
| `Org.Edgerunner.Mud.MCP.Tests/McpQueryCorrelatorTests.cs` (create) | Correlator tests |
| `Org.Edgerunner.Mud.MCP.Tests/McpQueryMappingTests.cs` (create) | Mapping tests |
| `Org.Edgerunner.Mud.MCP.Tests/FakeQueryTerminal.cs` (create) | Shared test fake |
| `Org.Edgerunner.Mud.MCP.Tests/McpQueryProviderTests.cs` (create) | Provider behavior tests |
| `Org.Edgerunner.Mud.MCP.Tests/McpQueryPackageTests.cs` (create) | Package + registration tests |

---

### Task 1: Protocol document

**Files:**
- Create: `docs/edgerunner-org-moo-query-protocol.md`

- [ ] **Step 1: Write the document**

Create `docs/edgerunner-org-moo-query-protocol.md` with exactly this content:

````markdown
# edgerunner-org-moo-query — MCP Package Protocol (v1.0)

**Status:** Normative. Both the server package (`Server Packages/edgerunner-org-moo-query.moo`)
and the client implementation (`Org.Edgerunner.Mud.MCP/Packages/McpQueryPackage.cs`) are written
against this document.

> MCP throughout = **MUD Client Protocol 2.1** (https://www.moo.mud.org/mcp/mcp2.html),
> NOT the LLM "Model Context Protocol".

## 1. Package identity

- Package name: `edgerunner-org-moo-query`
- Version: `1.0` (min = max = 1.0)
- Transport: standard MCP 2.1 messages over the negotiated session; no cords.

## 2. Negotiation

Support is settled during the initial MCP handshake: the `mcp` startup exchange followed by
`mcp-negotiate`, in which each side advertises its packages and versions via
`mcp-negotiate-can`. The client advertises `edgerunner-org-moo-query` with
`min-version: 1.0 max-version: 1.0`. When the server's `mcp-negotiate-can` for this package
overlaps that range, the package is in effect for the session. If the server never advertises
it, the client MUST NOT send any message of this package.

## 3. Message model

Every request is a single-line MCP message carrying the session auth key, a client-generated
`tag` field, and the parameters listed in §5. Every reply echoes the tag and carries the
payload in one `data*` multiline field containing **minified JSON**.

### 3.1 Correlation

- `tag` is an opaque client-generated string, unique per in-flight request (the reference
  client uses a monotonically increasing integer rendered as a string).
- The server MUST echo the request's tag verbatim on the reply (or error) message.
- The client matches replies to pending requests solely by tag. Replies bearing unknown tags
  are dropped.

### 3.2 Chunking (`data*` framing)

The reply JSON is one logical string. The server splits it into continuation lines of at most
**4000 characters** purely as transport framing:

```
#$#<reply-name> <authkey> tag: "<tag>" data*: "" _data-tag: <dtag>
#$#* <dtag> data: <chunk-1>
#$#* <dtag> data: <chunk-2>
#$#: <dtag>
```

The client concatenates all `data` chunks **verbatim with no separator**, then parses the
result as JSON once the closing line arrives. Because the JSON is minified and MOO strings
cannot contain newline characters, chunk boundaries never need escaping.

### 3.3 Encoding conventions

- Object numbers are **bare JSON ints** (no `#`, never quoted strings).
- Verb names stay as **raw MOO verb-names strings** (e.g. `"g*et put"`); consumers split on
  whitespace and interpret `*`.
- Envelope keys are single characters; list rows are positional arrays.
- `q` = queried object number; `r` = resolved (defining) object number. These appear ONLY on
  `-verb-info`, `-verb-doc`, and `-verb-code` replies.
- All JSON is minified (no insignificant whitespace).

### 3.4 Request parameter conventions

- `object` / `owner` values are object references in `#123` or `123` form.
- `verb` / `prop` values are plain names (a verb reference may be any alias).
- `owner` on `-owned` is always present on the wire; the **empty string** means "the
  connected player".

## 4. Permissions

Every server handler runs under `set_task_perms()` of the connected player. Visibility and
readability outcomes are exactly what the player's own MOO permissions yield; permission
failures surface as `-error` replies with code `E_PERM`.

## 5. Message catalog

All names below are suffixes of `edgerunner-org-moo-query`. Every request also carries `tag`.

| Request | Params | Reply | JSON payload |
|---|---|---|---|
| `-core-objects` | — | `-core-objects-reply` | `{"d":[[num,name,[aliases]],…]}` — one row per object referenced by a `#0` property (`$`-registered), deduped, valid objects only |
| `-children` | `object` | `-children-reply` | `{"d":[[num,name,[aliases]],…]}` — immediate children |
| `-owned` | `owner` | `-owned-reply` | `{"d":[[num,name,[aliases]],…]}` — from the target's `.owned_objects` bookkeeping; a core without that property answers `-error E_INVARG` (servers MUST NOT fall back to a DB walk) |
| `-parent` | `object` | `-parent-reply` | `{"p":num}`; `-1` = no parent |
| `-verbs` | `object` | `-verbs-reply` | `{"d":["g*et put","look_self",…]}` — raw verb-names strings, local + inherited (ancestor walk), deduped; unreadable ancestors contribute nothing |
| `-verb-info` | `object`, `verb` | `-verb-info-reply` | `{"q":num,"r":num,"a":"names","o":num,"p":"rxd","g":["this","none","this"]}` — `a` = raw names string, `o` = owner, `p` = permission flags, `g` = dobj/prep/iobj specs as returned by `verb_args()` |
| `-verb-doc` | `object`, `verb` | `-verb-doc-reply` | `{"q":num,"r":num,"l":[lines]}` — `l` = the leading string-literal lines of the verb code (unescaped) |
| `-verb-code` | `object`, `verb` | `-verb-code-reply` | `{"q":num,"r":num,"l":[lines]}` — `verb_code()` lines |
| `-props` | `object` | `-props-reply` | `{"d":["name",…]}` — property names only, local + inherited, deduped |
| `-prop-info` | `object`, `prop` | `-prop-info-reply` | `{"n":"name","o":num,"p":"rc","t":typecode,"v":"preview"}` — `t` = `typeof()` code, `v` = first 80 characters of `toliteral(value)` |
| `-prop-doc` | `object`, `prop` | `-prop-doc-reply` | `{"l":[lines]}` — `toliteral(value)` split into ≤78-char lines, capped at 50 lines |
| `-prop-value` | `object`, `prop` | `-prop-value-reply` | `{"t":typecode,"v":"literal"}` — full `toliteral(value)` |

Verb info/doc/code resolve the **defining ancestor**: the server walks up from the queried
object to the first ancestor whose `verb_info()` answers for the name; that ancestor is `r`.
No match anywhere on the chain → `-error E_VERBNF`.

### 5.1 Worked example

```
C→S: #$#edgerunner-org-moo-query-verbs K7% tag: 12 object: #123
S→C: #$#edgerunner-org-moo-query-verbs-reply K7% tag: "12" data*: "" _data-tag: 9911
     #$#* 9911 data: {"d":["g*et put","look_self"]}
     #$#: 9911
```

```
C→S: #$#edgerunner-org-moo-query-verb-info K7% tag: 13 object: #123 verb: "g*et"
S→C: #$#edgerunner-org-moo-query-verb-info-reply K7% tag: "13" data*: "" _data-tag: 9912
     #$#* 9912 data: {"q":123,"r":6,"a":"g*et put","o":2,"p":"rxd","g":["this","none","this"]}
     #$#: 9912
```

```
C→S: #$#edgerunner-org-moo-query-owned K7% tag: 14 owner: ""
S→C: #$#edgerunner-org-moo-query-owned-reply K7% tag: "14" data*: "" _data-tag: 9913
     #$#* 9913 data: {"d":[[101,"my room",["room"]],[102,"hat",[]]]}
     #$#: 9913
```

## 6. Errors

Shared single-line error reply:

```
#$#edgerunner-org-moo-query-error <authkey> tag: "<tag>" code: E_PERM message: "You can't read that"
```

- `code` — the MOO error constant name (`E_PERM`, `E_INVARG`, `E_VERBNF`, `E_PROPNF`, …).
- `message` — human-readable text; the server replaces embedded `"` with `'` so the value
  survives MCP quoting.

Client behavior on error: degrade to the `IMooWorldQueryProvider` contract value (`null` /
empty list) and log the event — never throw into editor consumers.

## 7. Type codes

`t` values are the MOO `typeof()` codes: 0 = INT, 1 = OBJ, 2 = STR, 3 = ERR, 4 = LIST,
9 = FLOAT (further codes per server family, e.g. ToastStunt MAP = 10, transmitted as-is).
````

- [ ] **Step 2: Commit**

```bash
git add docs/edgerunner-org-moo-query-protocol.md
git commit -m "Add normative edgerunner-org-moo-query protocol document (udd-btl)"
```

---

### Task 2: NLog reference + `IPackageNegotiationListener` + negotiation hook

**Files:**
- Modify: `Org.Edgerunner.Mud.MCP/Org.Edgerunner.Mud.MCP.csproj`
- Create: `Org.Edgerunner.Mud.MCP/Interfaces/IPackageNegotiationListener.cs`
- Modify: `Org.Edgerunner.Mud.MCP/Packages/McpNegotiatePackage.cs`
- Test: `Org.Edgerunner.Mud.MCP.Tests/NegotiationListenerTests.cs`

- [ ] **Step 1: Add the NLog package reference**

In `Org.Edgerunner.Mud.MCP/Org.Edgerunner.Mud.MCP.csproj`, replace:

```xml
  <ItemGroup>
    <ProjectReference Include="..\Org.Edgerunner.Mud.Common\Org.Edgerunner.Mud.Common.csproj" />
    <ProjectReference Include="..\Org.Edgerunner.Mud.Communication\Org.Edgerunner.Mud.Communication.csproj" />
  </ItemGroup>
```

with:

```xml
  <ItemGroup>
    <PackageReference Include="NLog" Version="5.2.8" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Org.Edgerunner.Mud.Common\Org.Edgerunner.Mud.Common.csproj" />
    <ProjectReference Include="..\Org.Edgerunner.Mud.Communication\Org.Edgerunner.Mud.Communication.csproj" />
  </ItemGroup>
```

- [ ] **Step 2: Write the failing tests**

Create `Org.Edgerunner.Mud.MCP.Tests/NegotiationListenerTests.cs`:

```csharp
using FluentAssertions;
using NSubstitute;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP;
using Org.Edgerunner.Mud.MCP.Interfaces;
using Org.Edgerunner.Mud.MCP.Packages;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class NegotiationListenerTests
{
   private sealed class ListeningPackage : IMcpPackage, IPackageNegotiationListener
   {
      public int SupportedCallCount { get; private set; }
      public IClientTerminal? LastClient { get; private set; }

      public string Name { get; set; } = "edgerunner-org-moo-query";
      public double MinimumVersion { get; set; } = 1.0;
      public double MaximumVersion { get; set; } = 1.0;
      public void SetSession(McpClientSession session) { }
      public bool CanHandleMessage(Message message) => false;
      public bool ProcessMessage(IClientTerminal client, Message message) => false;
      public void Reset() { }

      public void OnPackageSupported(IClientTerminal client)
      {
         SupportedCallCount++;
         LastClient = client;
      }
   }

   private sealed class PlainPackage : IMcpPackage
   {
      public string Name { get; set; } = "dns-org-mud-moo-simpleedit";
      public double MinimumVersion { get; set; } = 1.0;
      public double MaximumVersion { get; set; } = 1.0;
      public void SetSession(McpClientSession session) { }
      public bool CanHandleMessage(Message message) => false;
      public bool ProcessMessage(IClientTerminal client, Message message) => false;
      public void Reset() { }
   }

   private static McpClientSession CreateSession()
   {
      var manager = new McpClientSessionManager(new Version(2, 1), new Version(2, 1), new List<IMcpPackage>());
      return new McpClientSession(manager, "KEY123", new Version(2, 1));
   }

   private static Message NegotiateCan(string package, string min = "1.0", string max = "1.0") =>
      new("mcp-negotiate-can", "KEY123", new Dictionary<string, string>
      {
         ["package:"] = package,
         ["min-version:"] = min,
         ["max-version:"] = max
      });

   private static (McpNegotiatePackage Negotiate, McpClientSession Session) CreateNegotiator(params IMcpPackage[] packages)
   {
      var registry = packages.ToDictionary(p => p.Name.ToLowerInvariant(), p => p);
      var negotiate = new McpNegotiatePackage(registry);
      var session = CreateSession();
      negotiate.SetSession(session);
      return (negotiate, session);
   }

   [Fact]
   public void NegotiateCan_CompatiblePackage_NotifiesListenerWithClient()
   {
      var package = new ListeningPackage();
      var (negotiate, session) = CreateNegotiator(package);
      var client = Substitute.For<IClientTerminal>();

      negotiate.ProcessMessage(client, NegotiateCan(package.Name));

      package.SupportedCallCount.Should().Be(1);
      package.LastClient.Should().BeSameAs(client);
      session.SupportedPackages.Should().ContainKey(package.Name);
   }

   [Fact]
   public void NegotiateCan_IncompatibleVersions_DoesNotNotify()
   {
      var package = new ListeningPackage();
      var (negotiate, session) = CreateNegotiator(package);
      var client = Substitute.For<IClientTerminal>();

      negotiate.ProcessMessage(client, NegotiateCan(package.Name, "2.0", "3.0"));

      package.SupportedCallCount.Should().Be(0);
      session.SupportedPackages.Should().BeEmpty();
   }

   [Fact]
   public void NegotiateCan_UnregisteredPackage_DoesNotNotify()
   {
      var package = new ListeningPackage();
      var (negotiate, _) = CreateNegotiator(package);
      var client = Substitute.For<IClientTerminal>();

      negotiate.ProcessMessage(client, NegotiateCan("dns-org-mud-moo-somethingelse"));

      package.SupportedCallCount.Should().Be(0);
   }

   [Fact]
   public void NegotiateCan_PackageWithoutListenerInterface_StillRecordsSupport()
   {
      var package = new PlainPackage();
      var (negotiate, session) = CreateNegotiator(package);
      var client = Substitute.For<IClientTerminal>();

      negotiate.ProcessMessage(client, NegotiateCan(package.Name));

      session.SupportedPackages.Should().ContainKey(package.Name);
   }
}
```

- [ ] **Step 3: Run the tests to verify they fail**

Run: `dotnet test Org.Edgerunner.Mud.MCP.Tests --filter "FullyQualifiedName~NegotiationListener"`
Expected: build FAILURE — `IPackageNegotiationListener` does not exist yet.

- [ ] **Step 4: Create the interface**

Create `Org.Edgerunner.Mud.MCP/Interfaces/IPackageNegotiationListener.cs` (BSD header region first, per ground rules):

```csharp
using Org.Edgerunner.Mud.Communication.Interfaces;

namespace Org.Edgerunner.Mud.MCP.Interfaces;

/// <summary>
/// Optional companion interface for <see cref="IMcpPackage"/> implementations that need to know
/// when the server has confirmed support for the package during MCP negotiation.
/// </summary>
public interface IPackageNegotiationListener
{
   /// <summary>
   /// Called when the server's <c>mcp-negotiate-can</c> confirms this package at a compatible
   /// version. Invoked at most once per confirmation (a renegotiating session may invoke it again).
   /// </summary>
   /// <param name="client">The client terminal the negotiation belongs to.</param>
   void OnPackageSupported(IClientTerminal client);
}
```

- [ ] **Step 5: Hook the listener into `McpNegotiatePackage`**

In `Org.Edgerunner.Mud.MCP/Packages/McpNegotiatePackage.cs`, replace:

```csharp
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
```

with:

```csharp
   public bool ProcessMessage(IClientTerminal client, Message message)
   {
      return message.Name.ToLowerInvariant() switch
      {
         "mcp-negotiate-can" => ProcessNegotiateCan(client, message),
         "mcp-negotiate-end" => ProcessNegotiateEnd(),
         _ => false
      };
   }

   private bool ProcessNegotiateCan(IClientTerminal client, Message message)
   {
```

and replace:

```csharp
      _session.SupportedPackages[packageName.ToLowerInvariant()] = pkg;
      return true;
```

with:

```csharp
      _session.SupportedPackages[packageName.ToLowerInvariant()] = pkg;

      if (pkg is IPackageNegotiationListener listener)
         listener.OnPackageSupported(client);

      return true;
```

- [ ] **Step 6: Run the tests to verify they pass**

Run: `dotnet test Org.Edgerunner.Mud.MCP.Tests --filter "FullyQualifiedName~NegotiationListener"`
Expected: PASS (4 tests).

Also run the existing MCP suites to confirm nothing regressed:

Run: `dotnet test Org.Edgerunner.Mud.MCP.Tests --filter "FullyQualifiedName~SimpleEdit" --no-build`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add Org.Edgerunner.Mud.MCP/Org.Edgerunner.Mud.MCP.csproj Org.Edgerunner.Mud.MCP/Interfaces/IPackageNegotiationListener.cs Org.Edgerunner.Mud.MCP/Packages/McpNegotiatePackage.cs Org.Edgerunner.Mud.MCP.Tests/NegotiationListenerTests.cs
git commit -m "Add IPackageNegotiationListener hook to MCP negotiation (udd-btl)"
```

---

### Task 3: `McpQueryCorrelator` + `McpQueryErrorException`

**Files:**
- Create: `Org.Edgerunner.Mud.MCP/Exceptions/McpQueryErrorException.cs`
- Create: `Org.Edgerunner.Mud.MCP/Packages/McpQueryCorrelator.cs`
- Test: `Org.Edgerunner.Mud.MCP.Tests/McpQueryCorrelatorTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `Org.Edgerunner.Mud.MCP.Tests/McpQueryCorrelatorTests.cs`:

```csharp
using FluentAssertions;
using Org.Edgerunner.Mud.MCP.Exceptions;
using Org.Edgerunner.Mud.MCP.Packages;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpQueryCorrelatorTests
{
   [Fact]
   public void NextTag_ProducesUniqueSequentialTags()
   {
      var correlator = new McpQueryCorrelator();

      correlator.NextTag().Should().Be("1");
      correlator.NextTag().Should().Be("2");
      correlator.NextTag().Should().Be("3");
   }

   [Fact]
   public async Task Complete_ResolvesPendingTaskWithPayload()
   {
      var correlator = new McpQueryCorrelator();
      var pending = correlator.CreatePending("1");

      correlator.Complete("1", "{\"d\":[]}").Should().BeTrue();

      (await pending).Should().Be("{\"d\":[]}");
   }

   [Fact]
   public async Task CompleteError_FaultsPendingTaskWithTypedException()
   {
      var correlator = new McpQueryCorrelator();
      var pending = correlator.CreatePending("1");

      correlator.CompleteError("1", new McpQueryErrorException("E_PERM", "denied")).Should().BeTrue();

      var act = async () => await pending;
      var error = (await act.Should().ThrowAsync<McpQueryErrorException>()).Which;
      error.Code.Should().Be("E_PERM");
      error.Message.Should().Be("denied");
   }

   [Fact]
   public void Complete_UnknownTag_ReturnsFalse()
   {
      var correlator = new McpQueryCorrelator();

      correlator.Complete("99", "{}").Should().BeFalse();
      correlator.CompleteError("99", new McpQueryErrorException("E_INVARG", "x")).Should().BeFalse();
   }

   [Fact]
   public void Complete_AfterRemove_ReturnsFalse()
   {
      var correlator = new McpQueryCorrelator();
      correlator.CreatePending("1");
      correlator.Remove("1");

      correlator.Complete("1", "{}").Should().BeFalse();
   }

   [Fact]
   public void Complete_Twice_SecondReturnsFalse()
   {
      var correlator = new McpQueryCorrelator();
      correlator.CreatePending("1");

      correlator.Complete("1", "{}").Should().BeTrue();
      correlator.Complete("1", "{}").Should().BeFalse();
   }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test Org.Edgerunner.Mud.MCP.Tests --filter "FullyQualifiedName~McpQueryCorrelator"`
Expected: build FAILURE — types do not exist.

- [ ] **Step 3: Create the exception**

Create `Org.Edgerunner.Mud.MCP/Exceptions/McpQueryErrorException.cs` (BSD header region first):

```csharp
namespace Org.Edgerunner.Mud.MCP.Exceptions;

/// <summary>
/// Represents an <c>edgerunner-org-moo-query-error</c> reply from the server: a MOO error code
/// (e.g. <c>E_PERM</c>, <c>E_VERBNF</c>) plus the server's human-readable message.
/// </summary>
public class McpQueryErrorException : Exception
{
   /// <summary>
   /// Initializes a new instance of the <see cref="McpQueryErrorException"/> class.
   /// </summary>
   /// <param name="code">The MOO error constant name reported by the server.</param>
   /// <param name="message">The server's human-readable error message.</param>
   public McpQueryErrorException(string code, string message)
      : base(message)
   {
      Code = code;
   }

   /// <summary>
   /// Gets the MOO error constant name reported by the server (e.g. <c>E_PERM</c>).
   /// </summary>
   public string Code { get; }
}
```

- [ ] **Step 4: Create the correlator**

Create `Org.Edgerunner.Mud.MCP/Packages/McpQueryCorrelator.cs` (BSD header region first):

```csharp
using System.Collections.Concurrent;
using Org.Edgerunner.Mud.MCP.Exceptions;

namespace Org.Edgerunner.Mud.MCP.Packages;

/// <summary>
/// Thread-safe map of in-flight <c>edgerunner-org-moo-query</c> requests keyed by tag. Tags come
/// from an <see cref="Interlocked"/> counter and are unique by construction.
/// </summary>
public class McpQueryCorrelator
{
   private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pending = new();

   private int _tagCounter;

   /// <summary>
   /// Generates the next unique request tag.
   /// </summary>
   /// <returns>The tag.</returns>
   public string NextTag() => Interlocked.Increment(ref _tagCounter).ToString();

   /// <summary>
   /// Registers a pending request for the given tag.
   /// </summary>
   /// <param name="tag">The request tag.</param>
   /// <returns>A task that completes with the reply's JSON payload.</returns>
   public Task<string> CreatePending(string tag)
   {
      var source = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
      _pending[tag] = source;
      return source.Task;
   }

   /// <summary>
   /// Completes the pending request for the given tag with a JSON payload.
   /// </summary>
   /// <param name="tag">The reply tag.</param>
   /// <param name="payload">The reassembled JSON payload.</param>
   /// <returns><c>true</c> when a pending request was completed; <c>false</c> for unknown/stale tags.</returns>
   public bool Complete(string tag, string payload) =>
      _pending.TryRemove(tag, out var source) && source.TrySetResult(payload);

   /// <summary>
   /// Faults the pending request for the given tag with a server error.
   /// </summary>
   /// <param name="tag">The reply tag.</param>
   /// <param name="error">The typed server error.</param>
   /// <returns><c>true</c> when a pending request was faulted; <c>false</c> for unknown/stale tags.</returns>
   public bool CompleteError(string tag, McpQueryErrorException error) =>
      _pending.TryRemove(tag, out var source) && source.TrySetException(error);

   /// <summary>
   /// Removes the pending request for the given tag, if any.
   /// </summary>
   /// <param name="tag">The request tag.</param>
   public void Remove(string tag) => _pending.TryRemove(tag, out _);
}
```

- [ ] **Step 5: Run the tests to verify they pass**

Run: `dotnet test Org.Edgerunner.Mud.MCP.Tests --filter "FullyQualifiedName~McpQueryCorrelator"`
Expected: PASS (6 tests).

- [ ] **Step 6: Commit**

```bash
git add Org.Edgerunner.Mud.MCP/Exceptions/McpQueryErrorException.cs Org.Edgerunner.Mud.MCP/Packages/McpQueryCorrelator.cs Org.Edgerunner.Mud.MCP.Tests/McpQueryCorrelatorTests.cs
git commit -m "Add McpQueryCorrelator and McpQueryErrorException (udd-btl)"
```

---

### Task 4: `McpQueryMapping`

**Files:**
- Create: `Org.Edgerunner.Mud.MCP/Packages/McpQueryMapping.cs`
- Test: `Org.Edgerunner.Mud.MCP.Tests/McpQueryMappingTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `Org.Edgerunner.Mud.MCP.Tests/McpQueryMappingTests.cs`:

```csharp
using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
using Org.Edgerunner.Mud.MCP.Packages;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpQueryMappingTests
{
   private static readonly MooObjectId Queried = new(123);

   [Fact]
   public void MapObjectSummaries_ParsesRows()
   {
      var json = "{\"d\":[[0,\"System Object\",[\"sysobj\"]],[1,\"Root Class\",[]]]}";

      var result = McpQueryMapping.MapObjectSummaries(json);

      result.Should().HaveCount(2);
      result[0].Id.Should().Be(new MooObjectId(0));
      result[0].Name.Should().Be("System Object");
      result[0].Aliases.Should().Equal("sysobj");
      result[1].Id.Should().Be(new MooObjectId(1));
      result[1].Aliases.Should().BeEmpty();
   }

   [Fact]
   public void MapObjectSummaries_EmptyList_ReturnsEmpty()
   {
      McpQueryMapping.MapObjectSummaries("{\"d\":[]}").Should().BeEmpty();
   }

   [Fact]
   public void MapParent_PositiveNumber_ReturnsId()
   {
      McpQueryMapping.MapParent("{\"p\":1}").Should().Be(new MooObjectId(1));
   }

   [Fact]
   public void MapParent_MinusOne_ReturnsNull()
   {
      McpQueryMapping.MapParent("{\"p\":-1}").Should().BeNull();
   }

   [Fact]
   public void MapVerbSummaries_SplitsAliasesAndFillsDefiningObjectWithQueriedId()
   {
      var json = "{\"d\":[\"g*et put\",\"look_self\"]}";

      var result = McpQueryMapping.MapVerbSummaries(json, Queried);

      result.Should().HaveCount(2);
      result[0].Aliases.Should().Equal("g*et", "put");
      result[0].DefiningObject.Should().Be(Queried);
      result[1].Aliases.Should().Equal("look_self");
      result[1].DefiningObject.Should().Be(Queried);
   }

   [Fact]
   public void MapPropertySummaries_FillsDefiningObjectWithQueriedId()
   {
      var json = "{\"d\":[\"name\",\"aliases\"]}";

      var result = McpQueryMapping.MapPropertySummaries(json, Queried);

      result.Should().HaveCount(2);
      result[0].Name.Should().Be("name");
      result[0].DefiningObject.Should().Be(Queried);
   }

   [Fact]
   public void MapVerbInfo_ParsesAllFields()
   {
      var json = "{\"q\":123,\"r\":6,\"a\":\"g*et put\",\"o\":2,\"p\":\"rxd\",\"g\":[\"this\",\"none\",\"this\"]}";

      var result = McpQueryMapping.MapVerbInfo(json);

      result.QueriedObjectId.Should().Be(new MooObjectId(123));
      result.ResolvedObjectId.Should().Be(new MooObjectId(6));
      result.Aliases.Should().Equal("g*et", "put");
      result.Owner.Should().Be(new MooObjectId(2));
      result.Permissions.Should().Be(new VerbPermission(true, false, true, true));
      result.Args.Should().Be(new VerbArgs(DirectObject.This, Preposition.None, IndirectObject.This));
   }

   [Theory]
   [InlineData("none", Preposition.None)]
   [InlineData("any", Preposition.Any)]
   [InlineData("with/using", Preposition.With)]
   [InlineData("at/to", Preposition.At)]
   [InlineData("in front of", Preposition.InFrontOf)]
   [InlineData("in/inside/into", Preposition.In)]
   [InlineData("on top of/on/onto/upon", Preposition.OnTopOf)]
   [InlineData("out of/from inside/from", Preposition.OutOf)]
   [InlineData("over", Preposition.Over)]
   [InlineData("through", Preposition.Through)]
   [InlineData("under/underneath/beneath", Preposition.Under)]
   [InlineData("behind", Preposition.Behind)]
   [InlineData("beside", Preposition.Beside)]
   [InlineData("for/about", Preposition.For)]
   [InlineData("is", Preposition.Is)]
   [InlineData("as", Preposition.As)]
   [InlineData("off/off of", Preposition.Off)]
   [InlineData("garbage", Preposition.None)]
   public void ParsePreposition_ResolvesAliases(string spec, Preposition expected)
   {
      McpQueryMapping.ParsePreposition(spec).Should().Be(expected);
   }

   [Fact]
   public void MapVerbDocumentation_CarriesQueriedResolvedAndLines()
   {
      var json = "{\"q\":123,\"r\":6,\"l\":[\"Usage: foo\",\"Second line\"]}";

      var result = McpQueryMapping.MapVerbDocumentation(json);

      result.QueriedObjectId.Should().Be(new MooObjectId(123));
      result.ResolvedObjectId.Should().Be(new MooObjectId(6));
      result.Lines.Should().Equal("Usage: foo", "Second line");
   }

   [Fact]
   public void MapVerbCode_CarriesQueriedResolvedAndLines()
   {
      var json = "{\"q\":123,\"r\":6,\"l\":[\"return 1;\"]}";

      var result = McpQueryMapping.MapVerbCode(json);

      result.QueriedObjectId.Should().Be(new MooObjectId(123));
      result.ResolvedObjectId.Should().Be(new MooObjectId(6));
      result.Lines.Should().Equal("return 1;");
   }

   [Fact]
   public void MapPropertyInfo_FillsDefiningObjectWithQueriedId()
   {
      var json = "{\"n\":\"name\",\"o\":2,\"p\":\"rc\",\"t\":2,\"v\":\"\\\"Wizard\\\"\"}";

      var result = McpQueryMapping.MapPropertyInfo(json, Queried);

      result.Name.Should().Be("name");
      result.Owner.Should().Be(new MooObjectId(2));
      result.Permissions.Should().Be(new PropertyPermission(true, false, true));
      result.DefiningObject.Should().Be(Queried);
      result.ValueType.Should().Be(2);
      result.ValuePreview.Should().Be("\"Wizard\"");
   }

   [Fact]
   public void MapLines_ReturnsLines()
   {
      McpQueryMapping.MapLines("{\"l\":[\"a\",\"b\"]}").Should().Equal("a", "b");
   }

   [Fact]
   public void MapPropertyValue_ParsesTypeAndLiteral()
   {
      var result = McpQueryMapping.MapPropertyValue("{\"t\":4,\"v\":\"{1, 2}\"}");

      result.Type.Should().Be(4);
      result.Literal.Should().Be("{1, 2}");
   }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test Org.Edgerunner.Mud.MCP.Tests --filter "FullyQualifiedName~McpQueryMapping"`
Expected: build FAILURE — `McpQueryMapping` does not exist.

- [ ] **Step 3: Create the mapping class**

Create `Org.Edgerunner.Mud.MCP/Packages/McpQueryMapping.cs` (BSD header region first). It reuses
`SdwcMapping.ParseObjectId`/`SplitAliases` from the already-referenced Communication project:

```csharp
using System.Text.Json;
using Org.Edgerunner.Mud.Common.Querying;
using Org.Edgerunner.Mud.Communication.Sdwc;

namespace Org.Edgerunner.Mud.MCP.Packages;

/// <summary>
/// Pure, network-independent helpers that parse <c>edgerunner-org-moo-query</c> JSON payloads
/// (System.Text.Json) into the <see cref="Org.Edgerunner.Mud.Common.Querying"/> models. See
/// <c>docs/edgerunner-org-moo-query-protocol.md</c> for the payload schemas.
/// </summary>
/// <remarks>
/// Summary listings (<see cref="MapVerbSummaries"/>, <see cref="MapPropertySummaries"/>,
/// <see cref="MapPropertyInfo"/>) describe the queried object only; the caller supplies the queried
/// id and it is used as <c>DefiningObject</c>. Resolved-object semantics exist only on the
/// verb-info/doc/code payloads, which carry explicit <c>q</c>/<c>r</c> fields.
/// </remarks>
public static class McpQueryMapping
{
   private static readonly Dictionary<string, Preposition> PrepositionAliases = new(StringComparer.OrdinalIgnoreCase)
   {
      ["with"] = Preposition.With,
      ["using"] = Preposition.With,
      ["at"] = Preposition.At,
      ["to"] = Preposition.At,
      ["in front of"] = Preposition.InFrontOf,
      ["in"] = Preposition.In,
      ["inside"] = Preposition.In,
      ["into"] = Preposition.In,
      ["on top of"] = Preposition.OnTopOf,
      ["on"] = Preposition.OnTopOf,
      ["onto"] = Preposition.OnTopOf,
      ["upon"] = Preposition.OnTopOf,
      ["out of"] = Preposition.OutOf,
      ["from inside"] = Preposition.OutOf,
      ["from"] = Preposition.OutOf,
      ["over"] = Preposition.Over,
      ["through"] = Preposition.Through,
      ["under"] = Preposition.Under,
      ["underneath"] = Preposition.Under,
      ["beneath"] = Preposition.Under,
      ["behind"] = Preposition.Behind,
      ["beside"] = Preposition.Beside,
      ["for"] = Preposition.For,
      ["about"] = Preposition.For,
      ["is"] = Preposition.Is,
      ["as"] = Preposition.As,
      ["off"] = Preposition.Off,
      ["off of"] = Preposition.Off
   };

   /// <summary>
   /// Maps a <c>{"d":[[num,name,[aliases]],…]}</c> payload to object summaries.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <returns>A read-only list of <see cref="MooObjectSummary"/>.</returns>
   public static IReadOnlyList<MooObjectSummary> MapObjectSummaries(string json)
   {
      using var document = JsonDocument.Parse(json);
      var result = new List<MooObjectSummary>();
      if (document.RootElement.TryGetProperty("d", out var rows) && rows.ValueKind == JsonValueKind.Array)
         foreach (var row in rows.EnumerateArray())
         {
            if (row.ValueKind != JsonValueKind.Array || row.GetArrayLength() < 3)
               continue;

            var id = SdwcMapping.ParseObjectId(row[0]);
            var name = row[1].ValueKind == JsonValueKind.String ? row[1].GetString() ?? string.Empty : string.Empty;
            var aliases = new List<string>();
            if (row[2].ValueKind == JsonValueKind.Array)
               foreach (var alias in row[2].EnumerateArray())
                  if (alias.ValueKind == JsonValueKind.String)
                     aliases.Add(alias.GetString()!);

            result.Add(new MooObjectSummary(id, name, aliases));
         }

      return result;
   }

   /// <summary>
   /// Maps a <c>{"p":num}</c> payload to a parent id; a negative number means no parent.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <returns>The parent <see cref="MooObjectId"/>, or <c>null</c> when the object has no parent.</returns>
   public static MooObjectId? MapParent(string json)
   {
      using var document = JsonDocument.Parse(json);
      var number = document.RootElement.GetProperty("p").GetInt32();
      return number < 0 ? null : new MooObjectId(number);
   }

   /// <summary>
   /// Maps a <c>{"d":["g*et put",…]}</c> payload to verb summaries of the queried object.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <param name="queried">The queried object id, used as every row's <c>DefiningObject</c>.</param>
   /// <returns>A read-only list of <see cref="MooVerbSummary"/>.</returns>
   public static IReadOnlyList<MooVerbSummary> MapVerbSummaries(string json, MooObjectId queried)
   {
      using var document = JsonDocument.Parse(json);
      var result = new List<MooVerbSummary>();
      if (document.RootElement.TryGetProperty("d", out var rows) && rows.ValueKind == JsonValueKind.Array)
         foreach (var row in rows.EnumerateArray())
            if (row.ValueKind == JsonValueKind.String)
               result.Add(new MooVerbSummary(SdwcMapping.SplitAliases(row.GetString()), queried));

      return result;
   }

   /// <summary>
   /// Maps a <c>{"d":["name",…]}</c> payload to property summaries of the queried object.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <param name="queried">The queried object id, used as every row's <c>DefiningObject</c>.</param>
   /// <returns>A read-only list of <see cref="MooPropertySummary"/>.</returns>
   public static IReadOnlyList<MooPropertySummary> MapPropertySummaries(string json, MooObjectId queried)
   {
      using var document = JsonDocument.Parse(json);
      var result = new List<MooPropertySummary>();
      if (document.RootElement.TryGetProperty("d", out var rows) && rows.ValueKind == JsonValueKind.Array)
         foreach (var row in rows.EnumerateArray())
            if (row.ValueKind == JsonValueKind.String)
               result.Add(new MooPropertySummary(row.GetString()!, queried));

      return result;
   }

   /// <summary>
   /// Maps a <c>{"q","r","a","o","p","g"}</c> verb-info payload to a <see cref="MooVerbInfo"/>.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <returns>The parsed <see cref="MooVerbInfo"/>.</returns>
   public static MooVerbInfo MapVerbInfo(string json)
   {
      using var document = JsonDocument.Parse(json);
      var root = document.RootElement;

      var queried = new MooObjectId(root.GetProperty("q").GetInt32());
      var resolved = new MooObjectId(root.GetProperty("r").GetInt32());
      var aliases = SdwcMapping.SplitAliases(root.GetProperty("a").GetString());
      var owner = new MooObjectId(root.GetProperty("o").GetInt32());
      var permissions = ParseVerbPermissions(root.GetProperty("p").GetString() ?? string.Empty);

      var specs = root.GetProperty("g");
      var args = new VerbArgs(
         ParseDirectObject(specs[0].GetString() ?? string.Empty),
         ParsePreposition(specs[1].GetString() ?? string.Empty),
         ParseIndirectObject(specs[2].GetString() ?? string.Empty));

      return new MooVerbInfo(queried, resolved, aliases, owner, permissions, args);
   }

   /// <summary>
   /// Maps a <c>{"q","r","l"}</c> verb-doc payload to a <see cref="MooVerbDocumentation"/>.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <returns>The parsed <see cref="MooVerbDocumentation"/>.</returns>
   public static MooVerbDocumentation MapVerbDocumentation(string json)
   {
      using var document = JsonDocument.Parse(json);
      var root = document.RootElement;
      return new MooVerbDocumentation(
         new MooObjectId(root.GetProperty("q").GetInt32()),
         new MooObjectId(root.GetProperty("r").GetInt32()),
         ReadLines(root));
   }

   /// <summary>
   /// Maps a <c>{"q","r","l"}</c> verb-code payload to a <see cref="MooVerbCode"/>.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <returns>The parsed <see cref="MooVerbCode"/>.</returns>
   public static MooVerbCode MapVerbCode(string json)
   {
      using var document = JsonDocument.Parse(json);
      var root = document.RootElement;
      return new MooVerbCode(
         new MooObjectId(root.GetProperty("q").GetInt32()),
         new MooObjectId(root.GetProperty("r").GetInt32()),
         ReadLines(root));
   }

   /// <summary>
   /// Maps a <c>{"n","o","p","t","v"}</c> prop-info payload to a <see cref="MooPropertyInfo"/>.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <param name="queried">The queried object id, used as the <c>DefiningObject</c>.</param>
   /// <returns>The parsed <see cref="MooPropertyInfo"/>.</returns>
   public static MooPropertyInfo MapPropertyInfo(string json, MooObjectId queried)
   {
      using var document = JsonDocument.Parse(json);
      var root = document.RootElement;
      return new MooPropertyInfo(
         root.GetProperty("n").GetString() ?? string.Empty,
         new MooObjectId(root.GetProperty("o").GetInt32()),
         ParsePropertyPermissions(root.GetProperty("p").GetString() ?? string.Empty),
         queried,
         root.GetProperty("t").GetInt32(),
         root.GetProperty("v").GetString() ?? string.Empty);
   }

   /// <summary>
   /// Maps a <c>{"l":[lines]}</c> payload to its lines.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <returns>A read-only list of lines.</returns>
   public static IReadOnlyList<string> MapLines(string json)
   {
      using var document = JsonDocument.Parse(json);
      return ReadLines(document.RootElement);
   }

   /// <summary>
   /// Maps a <c>{"t","v"}</c> prop-value payload to a <see cref="MooPropertyValue"/>.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <returns>The parsed <see cref="MooPropertyValue"/>.</returns>
   public static MooPropertyValue MapPropertyValue(string json)
   {
      using var document = JsonDocument.Parse(json);
      var root = document.RootElement;
      return new MooPropertyValue(
         root.GetProperty("t").GetInt32(),
         root.GetProperty("v").GetString() ?? string.Empty);
   }

   /// <summary>
   /// Parses a MOO verb permission flag string (e.g. <c>"rxd"</c>) into a <see cref="VerbPermission"/>.
   /// </summary>
   /// <param name="flags">The flag string.</param>
   /// <returns>The parsed <see cref="VerbPermission"/>.</returns>
   public static VerbPermission ParseVerbPermissions(string flags) =>
      new(flags.Contains('r'), flags.Contains('w'), flags.Contains('x'), flags.Contains('d'));

   /// <summary>
   /// Parses a MOO property permission flag string (e.g. <c>"rc"</c>) into a <see cref="PropertyPermission"/>.
   /// </summary>
   /// <param name="flags">The flag string.</param>
   /// <returns>The parsed <see cref="PropertyPermission"/>.</returns>
   public static PropertyPermission ParsePropertyPermissions(string flags) =>
      new(flags.Contains('r'), flags.Contains('w'), flags.Contains('c'));

   /// <summary>
   /// Parses a MOO direct object specifier (<c>this</c>/<c>none</c>/<c>any</c>).
   /// </summary>
   /// <param name="spec">The specifier text.</param>
   /// <returns>The parsed <see cref="DirectObject"/>; unrecognized specs map to <see cref="DirectObject.None"/>.</returns>
   public static DirectObject ParseDirectObject(string spec) =>
      spec.Trim().ToLowerInvariant() switch
      {
         "this" => DirectObject.This,
         "any" => DirectObject.Any,
         _ => DirectObject.None
      };

   /// <summary>
   /// Parses a MOO indirect object specifier (<c>this</c>/<c>none</c>/<c>any</c>).
   /// </summary>
   /// <param name="spec">The specifier text.</param>
   /// <returns>The parsed <see cref="IndirectObject"/>; unrecognized specs map to <see cref="IndirectObject.None"/>.</returns>
   public static IndirectObject ParseIndirectObject(string spec) =>
      spec.Trim().ToLowerInvariant() switch
      {
         "this" => IndirectObject.This,
         "any" => IndirectObject.Any,
         _ => IndirectObject.None
      };

   /// <summary>
   /// Parses a MOO preposition specifier as returned by <c>verb_args()</c> (e.g. <c>"with/using"</c>,
   /// <c>"in front of"</c>); any slash-separated segment matching a known alias resolves the preposition.
   /// </summary>
   /// <param name="spec">The specifier text.</param>
   /// <returns>The parsed <see cref="Preposition"/>; unrecognized specs map to <see cref="Preposition.None"/>.</returns>
   public static Preposition ParsePreposition(string spec)
   {
      var trimmed = spec.Trim().ToLowerInvariant();
      if (trimmed.Length == 0 || trimmed == "none")
         return Preposition.None;
      if (trimmed == "any")
         return Preposition.Any;

      foreach (var segment in trimmed.Split('/'))
         if (PrepositionAliases.TryGetValue(segment.Trim(), out var preposition))
            return preposition;

      return Preposition.None;
   }

   private static IReadOnlyList<string> ReadLines(JsonElement root)
   {
      var lines = new List<string>();
      if (root.TryGetProperty("l", out var array) && array.ValueKind == JsonValueKind.Array)
         foreach (var line in array.EnumerateArray())
            if (line.ValueKind == JsonValueKind.String)
               lines.Add(line.GetString()!);

      return lines;
   }
}
```

- [ ] **Step 4: Run the tests to verify they pass**

Run: `dotnet test Org.Edgerunner.Mud.MCP.Tests --filter "FullyQualifiedName~McpQueryMapping"`
Expected: PASS (12 facts + 18 theory cases).

- [ ] **Step 5: Commit**

```bash
git add Org.Edgerunner.Mud.MCP/Packages/McpQueryMapping.cs Org.Edgerunner.Mud.MCP.Tests/McpQueryMappingTests.cs
git commit -m "Add McpQueryMapping payload parsers (udd-btl)"
```

---

### Task 5: `McpQueryProvider` + shared test fake

**Files:**
- Create: `Org.Edgerunner.Mud.MCP/Packages/McpQueryProvider.cs`
- Create: `Org.Edgerunner.Mud.MCP.Tests/FakeQueryTerminal.cs`
- Test: `Org.Edgerunner.Mud.MCP.Tests/McpQueryProviderTests.cs`

- [ ] **Step 1: Create the shared fake terminal**

Create `Org.Edgerunner.Mud.MCP.Tests/FakeQueryTerminal.cs`:

```csharp
using Org.Edgerunner.Mud.Communication.Interfaces;

namespace Org.Edgerunner.Mud.MCP.Tests;

/// <summary>A minimal fake terminal with a real query service, capturing single OOB lines.</summary>
public sealed class FakeQueryTerminal : IClientTerminal
{
   public List<string> SentOutOfBandLines { get; } = new();

   public Org.Edgerunner.Mud.Common.Querying.MooWorldQueryService QueryProviders { get; } = new();

   public bool IsConnected => true;

   public void SendOutOfBandLine(string text) => SentOutOfBandLines.Add(text);

   // Unused members.
   public System.Drawing.Color ConsoleForegroundColor { get; set; }
   public System.Drawing.Color ConsoleBackgroundColor { get; set; }
   public string Host => string.Empty;
   public int Port => 0;
   public string World => "TestWorld";
   public bool EchoEnabled { get; set; }
   public void SendTextLines(IEnumerable<string> lines) { }
   public void SendTextLine(string text) { }
   public void SendText(string text) { }
   public void SendOutOfBandLines(IEnumerable<string> lines) { }
   public void DisplayToConsole(string text) { }
   public void DisplayLineToConsole(string text) { }
   public void DisplayLinesToConsole(IEnumerable<string> lines) { }
   public void DisplayToConsole(string text, System.Drawing.Color foregroundColor) { }
   public void DisplayLineToConsole(string text, System.Drawing.Color foregroundColor) { }
   public void DisplayLinesToConsole(IEnumerable<string> lines, System.Drawing.Color foregroundColor) { }
   public void DisplayToConsole(string text, System.Drawing.Color foregroundColor, System.Drawing.Color backgroundColor) { }
   public void DisplayLineToConsole(string text, System.Drawing.Color foregroundColor, System.Drawing.Color backgroundColor) { }
   public void DisplayLinesToConsole(IEnumerable<string> lines, System.Drawing.Color foregroundColor, System.Drawing.Color backgroundColor) { }
}
```

(If `IClientTerminal` has members beyond these, stub them the same no-op way — mirror the
`CapturingTerminal` nested fake in `Org.Edgerunner.Mud.MCP.Tests/SimpleEditTests.cs:189-220`.)

- [ ] **Step 2: Write the failing tests**

Create `Org.Edgerunner.Mud.MCP.Tests/McpQueryProviderTests.cs`:

```csharp
using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
using Org.Edgerunner.Mud.MCP.Exceptions;
using Org.Edgerunner.Mud.MCP.Packages;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpQueryProviderTests
{
   private static readonly MooObjectId Target = new(123);

   private static (McpQueryProvider Provider, McpQueryCorrelator Correlator, FakeQueryTerminal Terminal) CreateProvider(TimeSpan? timeout = null)
   {
      var terminal = new FakeQueryTerminal();
      var correlator = new McpQueryCorrelator();
      var provider = new McpQueryProvider(terminal, "KEY123", correlator, timeout);
      return (provider, correlator, terminal);
   }

   [Fact]
   public async Task GetVerbsAsync_SendsWellFormedRequestAndMapsReply()
   {
      var (provider, correlator, terminal) = CreateProvider();

      var task = provider.GetVerbsAsync(Target, CancellationToken.None);

      terminal.SentOutOfBandLines.Should().ContainSingle()
         .Which.Should().Be("edgerunner-org-moo-query-verbs KEY123 tag: 1 object: #123");

      correlator.Complete("1", "{\"d\":[\"g*et put\"]}");
      var result = await task;

      result.Should().ContainSingle();
      result[0].Aliases.Should().Equal("g*et", "put");
      result[0].DefiningObject.Should().Be(Target);
   }

   [Fact]
   public async Task GetCoreObjectsAsync_SendsTagOnlyRequest()
   {
      var (provider, correlator, terminal) = CreateProvider();

      var task = provider.GetCoreObjectsAsync(CancellationToken.None);

      terminal.SentOutOfBandLines.Should().ContainSingle()
         .Which.Should().Be("edgerunner-org-moo-query-core-objects KEY123 tag: 1");

      correlator.Complete("1", "{\"d\":[[0,\"System Object\",[]]]}");
      (await task).Should().ContainSingle();
   }

   [Fact]
   public async Task GetOwnedObjectsAsync_Parameterless_SendsEmptyOwner()
   {
      var (provider, correlator, terminal) = CreateProvider();

      var task = provider.GetOwnedObjectsAsync(CancellationToken.None);

      terminal.SentOutOfBandLines.Should().ContainSingle()
         .Which.Should().Be("edgerunner-org-moo-query-owned KEY123 tag: 1 owner: \"\"");

      correlator.Complete("1", "{\"d\":[]}");
      (await task).Should().BeEmpty();
   }

   [Fact]
   public async Task GetOwnedObjectsAsync_WithOwner_SendsOwnerReference()
   {
      var (provider, correlator, terminal) = CreateProvider();

      var task = provider.GetOwnedObjectsAsync(new MooObjectId(7), CancellationToken.None);

      terminal.SentOutOfBandLines.Should().ContainSingle()
         .Which.Should().Be("edgerunner-org-moo-query-owned KEY123 tag: 1 owner: #7");

      correlator.Complete("1", "{\"d\":[]}");
      (await task).Should().BeEmpty();
   }

   [Fact]
   public async Task GetVerbInfoAsync_SendsVerbParameter()
   {
      var (provider, correlator, terminal) = CreateProvider();

      var task = provider.GetVerbInfoAsync(Target, "look", CancellationToken.None);

      terminal.SentOutOfBandLines.Should().ContainSingle()
         .Which.Should().Be("edgerunner-org-moo-query-verb-info KEY123 tag: 1 object: #123 verb: look");

      correlator.Complete("1", "{\"q\":123,\"r\":6,\"a\":\"look\",\"o\":2,\"p\":\"rxd\",\"g\":[\"this\",\"none\",\"this\"]}");
      var result = await task;

      result!.ResolvedObjectId.Should().Be(new MooObjectId(6));
   }

   [Fact]
   public async Task GetParentAsync_MapsMinusOneToNull()
   {
      var (provider, correlator, terminal) = CreateProvider();

      var task = provider.GetParentAsync(Target, CancellationToken.None);
      correlator.Complete("1", "{\"p\":-1}");

      (await task).Should().BeNull();
   }

   [Fact]
   public async Task ErrorReply_ListOperation_DegradesToEmptyList()
   {
      var (provider, correlator, _) = CreateProvider();

      var task = provider.GetVerbsAsync(Target, CancellationToken.None);
      correlator.CompleteError("1", new McpQueryErrorException("E_PERM", "denied"));

      (await task).Should().BeEmpty();
   }

   [Fact]
   public async Task ErrorReply_SingletonOperation_DegradesToNull()
   {
      var (provider, correlator, _) = CreateProvider();

      var task = provider.GetVerbInfoAsync(Target, "look", CancellationToken.None);
      correlator.CompleteError("1", new McpQueryErrorException("E_VERBNF", "no such verb"));

      (await task).Should().BeNull();
   }

   [Fact]
   public async Task ErrorReply_UnknownCode_StillDegrades()
   {
      var (provider, correlator, _) = CreateProvider();

      var task = provider.GetPropertyInfoAsync(Target, "name", CancellationToken.None);
      correlator.CompleteError("1", new McpQueryErrorException("E_BOGUS", "???"));

      (await task).Should().BeNull();
   }

   [Fact]
   public async Task MalformedJson_DegradesToContractDefault()
   {
      var (provider, correlator, _) = CreateProvider();

      var task = provider.GetPropertiesAsync(Target, CancellationToken.None);
      correlator.Complete("1", "this is not json");

      (await task).Should().BeEmpty();
   }

   [Fact]
   public async Task NoReply_ThrowsTimeoutException()
   {
      var (provider, _, _) = CreateProvider(TimeSpan.FromMilliseconds(50));

      var act = () => provider.GetVerbsAsync(Target, CancellationToken.None);

      await act.Should().ThrowAsync<TimeoutException>();
   }

   [Fact]
   public async Task CallerCancellation_Propagates()
   {
      var (provider, _, _) = CreateProvider();
      using var cancellation = new CancellationTokenSource();

      var task = provider.GetVerbsAsync(Target, cancellation.Token);
      cancellation.Cancel();

      var act = async () => await task;
      await act.Should().ThrowAsync<OperationCanceledException>();
   }

   [Fact]
   public async Task SequentialRequests_UseDistinctTags()
   {
      var (provider, correlator, terminal) = CreateProvider();

      var first = provider.GetPropertiesAsync(Target, CancellationToken.None);
      var second = provider.GetPropertiesAsync(Target, CancellationToken.None);

      terminal.SentOutOfBandLines[0].Should().Contain("tag: 1");
      terminal.SentOutOfBandLines[1].Should().Contain("tag: 2");

      correlator.Complete("2", "{\"d\":[\"b\"]}");
      correlator.Complete("1", "{\"d\":[\"a\"]}");

      (await first)[0].Name.Should().Be("a");
      (await second)[0].Name.Should().Be("b");
   }
}
```

- [ ] **Step 3: Run the tests to verify they fail**

Run: `dotnet test Org.Edgerunner.Mud.MCP.Tests --filter "FullyQualifiedName~McpQueryProvider"`
Expected: build FAILURE — `McpQueryProvider` does not exist.

- [ ] **Step 4: Create the provider**

Create `Org.Edgerunner.Mud.MCP/Packages/McpQueryProvider.cs` (BSD header region first):

```csharp
using System.Text.Json;
using NLog;
using Org.Edgerunner.Mud.Common.Querying;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP.Exceptions;

namespace Org.Edgerunner.Mud.MCP.Packages;

/// <summary>
/// An <see cref="IMooWorldQueryProvider"/> implemented over the <c>edgerunner-org-moo-query</c>
/// MCP 2.1 package. Covers all interface operations; see
/// <c>docs/edgerunner-org-moo-query-protocol.md</c> for the wire protocol.
/// </summary>
/// <remarks>
/// Each call registers a pending entry with the <see cref="McpQueryCorrelator"/> under a fresh tag,
/// sends a single-line MCP request, then awaits the tag-correlated JSON reply under the caller's
/// cancellation token linked to a bounded timeout. Server <c>-error</c> replies and unparseable
/// payloads degrade to the interface contract value (<c>null</c>/empty) but are always logged;
/// timeouts throw <see cref="TimeoutException"/>; cancellation propagates.
/// </remarks>
public sealed class McpQueryProvider : IMooWorldQueryProvider
{
   /// <summary>The MCP message-name prefix shared by every message of this package.</summary>
   public const string MessagePrefix = "edgerunner-org-moo-query";

   private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

   private static readonly HashSet<string> KnownErrorCodes = new(StringComparer.OrdinalIgnoreCase)
   {
      "E_NONE", "E_TYPE", "E_DIV", "E_PERM", "E_PROPNF", "E_VERBNF", "E_VARNF", "E_INVIND",
      "E_RECMOVE", "E_MAXREC", "E_RANGE", "E_ARGS", "E_NACC", "E_INVARG", "E_QUOTA", "E_FLOAT"
   };

   private readonly IClientTerminal _client;

   private readonly string _sessionKey;

   private readonly McpQueryCorrelator _correlator;

   private readonly TimeSpan _timeout;

   /// <summary>
   /// Initializes a new instance of the <see cref="McpQueryProvider"/> class.
   /// </summary>
   /// <param name="client">The client terminal used to send OOB requests.</param>
   /// <param name="sessionKey">The negotiated MCP session authentication key.</param>
   /// <param name="correlator">The correlator that matches replies to pending requests.</param>
   /// <param name="timeout">The bounded per-request timeout. Defaults to 10 seconds when <c>null</c>.</param>
   /// <exception cref="ArgumentNullException">Thrown when a required argument is <c>null</c>.</exception>
   public McpQueryProvider(IClientTerminal client, string sessionKey, McpQueryCorrelator correlator, TimeSpan? timeout = null)
   {
      _client = client ?? throw new ArgumentNullException(nameof(client));
      _sessionKey = sessionKey ?? throw new ArgumentNullException(nameof(sessionKey));
      _correlator = correlator ?? throw new ArgumentNullException(nameof(correlator));
      _timeout = timeout ?? TimeSpan.FromSeconds(10);
   }

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetCoreObjectsAsync(CancellationToken cancellationToken) =>
      QueryAsync<IReadOnlyList<MooObjectSummary>>(
         "core-objects",
         new Dictionary<string, string>(),
         McpQueryMapping.MapObjectSummaries,
         Array.Empty<MooObjectSummary>(),
         cancellationToken);

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetChildrenAsync(MooObjectId objectId, CancellationToken cancellationToken) =>
      QueryAsync<IReadOnlyList<MooObjectSummary>>(
         "children",
         new Dictionary<string, string> { ["object:"] = objectId.ToString() },
         McpQueryMapping.MapObjectSummaries,
         Array.Empty<MooObjectSummary>(),
         cancellationToken);

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(CancellationToken cancellationToken) =>
      QueryAsync<IReadOnlyList<MooObjectSummary>>(
         "owned",
         new Dictionary<string, string> { ["owner:"] = string.Empty },
         McpQueryMapping.MapObjectSummaries,
         Array.Empty<MooObjectSummary>(),
         cancellationToken);

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(MooObjectId owner, CancellationToken cancellationToken) =>
      QueryAsync<IReadOnlyList<MooObjectSummary>>(
         "owned",
         new Dictionary<string, string> { ["owner:"] = owner.ToString() },
         McpQueryMapping.MapObjectSummaries,
         Array.Empty<MooObjectSummary>(),
         cancellationToken);

   /// <inheritdoc/>
   public Task<MooObjectId?> GetParentAsync(MooObjectId objectId, CancellationToken cancellationToken) =>
      QueryAsync<MooObjectId?>(
         "parent",
         new Dictionary<string, string> { ["object:"] = objectId.ToString() },
         McpQueryMapping.MapParent,
         null,
         cancellationToken);

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooVerbSummary>> GetVerbsAsync(MooObjectId objectId, CancellationToken cancellationToken) =>
      QueryAsync<IReadOnlyList<MooVerbSummary>>(
         "verbs",
         new Dictionary<string, string> { ["object:"] = objectId.ToString() },
         json => McpQueryMapping.MapVerbSummaries(json, objectId),
         Array.Empty<MooVerbSummary>(),
         cancellationToken);

   /// <inheritdoc/>
   public Task<MooVerbInfo?> GetVerbInfoAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) =>
      QueryAsync<MooVerbInfo?>(
         "verb-info",
         new Dictionary<string, string> { ["object:"] = objectId.ToString(), ["verb:"] = verbName },
         McpQueryMapping.MapVerbInfo,
         null,
         cancellationToken);

   /// <inheritdoc/>
   public Task<MooVerbDocumentation?> GetVerbDocumentationAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) =>
      QueryAsync<MooVerbDocumentation?>(
         "verb-doc",
         new Dictionary<string, string> { ["object:"] = objectId.ToString(), ["verb:"] = verbName },
         McpQueryMapping.MapVerbDocumentation,
         null,
         cancellationToken);

   /// <inheritdoc/>
   public Task<MooVerbCode?> GetVerbCodeAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) =>
      QueryAsync<MooVerbCode?>(
         "verb-code",
         new Dictionary<string, string> { ["object:"] = objectId.ToString(), ["verb:"] = verbName },
         McpQueryMapping.MapVerbCode,
         null,
         cancellationToken);

   /// <inheritdoc/>
   public Task<IReadOnlyList<MooPropertySummary>> GetPropertiesAsync(MooObjectId objectId, CancellationToken cancellationToken) =>
      QueryAsync<IReadOnlyList<MooPropertySummary>>(
         "props",
         new Dictionary<string, string> { ["object:"] = objectId.ToString() },
         json => McpQueryMapping.MapPropertySummaries(json, objectId),
         Array.Empty<MooPropertySummary>(),
         cancellationToken);

   /// <inheritdoc/>
   public Task<MooPropertyInfo?> GetPropertyInfoAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) =>
      QueryAsync<MooPropertyInfo?>(
         "prop-info",
         new Dictionary<string, string> { ["object:"] = objectId.ToString(), ["prop:"] = propName },
         json => McpQueryMapping.MapPropertyInfo(json, objectId),
         null,
         cancellationToken);

   /// <inheritdoc/>
   public Task<IReadOnlyList<string>> GetPropertyDocumentationAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) =>
      QueryAsync<IReadOnlyList<string>>(
         "prop-doc",
         new Dictionary<string, string> { ["object:"] = objectId.ToString(), ["prop:"] = propName },
         McpQueryMapping.MapLines,
         Array.Empty<string>(),
         cancellationToken);

   /// <inheritdoc/>
   public Task<MooPropertyValue?> GetPropertyValueAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) =>
      QueryAsync<MooPropertyValue?>(
         "prop-value",
         new Dictionary<string, string> { ["object:"] = objectId.ToString(), ["prop:"] = propName },
         McpQueryMapping.MapPropertyValue,
         null,
         cancellationToken);

   /// <summary>
   /// Sends one request and awaits its mapped reply: register pending (fresh tag) → format and send →
   /// await under linked caller/timeout tokens → map. Server errors and unparseable payloads degrade
   /// to <paramref name="degraded"/> and are always logged; the pending entry is always removed.
   /// </summary>
   /// <typeparam name="T">The mapped result type.</typeparam>
   /// <param name="operation">The message-name suffix (e.g. <c>verbs</c>).</param>
   /// <param name="parameters">The request parameters (keys carry their trailing colon).</param>
   /// <param name="map">The payload mapper.</param>
   /// <param name="degraded">The contract value returned on server error or unparseable payload.</param>
   /// <param name="cancellationToken">The caller's cancellation token.</param>
   /// <returns>The mapped result or <paramref name="degraded"/>.</returns>
   /// <exception cref="TimeoutException">Thrown when no reply arrives within the bounded timeout.</exception>
   /// <exception cref="OperationCanceledException">Thrown when the caller cancels the operation.</exception>
   private async Task<T> QueryAsync<T>(
      string operation,
      Dictionary<string, string> parameters,
      Func<string, T> map,
      T degraded,
      CancellationToken cancellationToken)
   {
      var tag = _correlator.NextTag();
      var pending = _correlator.CreatePending(tag);

      var data = new Dictionary<string, string> { ["tag:"] = tag };
      foreach (var (keyword, value) in parameters)
         data[keyword] = value;

      try
      {
         _client.SendOutOfBandLine(McpUtils.FormatMessage($"{MessagePrefix}-{operation}", _sessionKey, data));

         using var timeoutSource = new CancellationTokenSource(_timeout);
         using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);

         var completed = await Task.WhenAny(pending, Task.Delay(Timeout.Infinite, linked.Token)).ConfigureAwait(false);
         if (completed != pending)
         {
            // The delay won: distinguish a caller cancellation from a bounded timeout.
            if (cancellationToken.IsCancellationRequested)
               throw new OperationCanceledException(cancellationToken);

            Logger.Debug("MCP query '{0}' (tag {1}, {2}) timed out after {3}.", operation, tag, Describe(parameters), _timeout);
            throw new TimeoutException($"MCP query '{operation}' timed out after {_timeout}.");
         }

         string json;
         try
         {
            json = await pending.ConfigureAwait(false);
         }
         catch (McpQueryErrorException error)
         {
            if (KnownErrorCodes.Contains(error.Code))
               Logger.Debug(
                  "MCP query '{0}' (tag {1}, {2}) answered {3}: {4}",
                  operation, tag, Describe(parameters), error.Code, error.Message);
            else
               Logger.Warn(
                  "MCP query '{0}' (tag {1}, {2}) answered unrecognized error code {3}: {4}",
                  operation, tag, Describe(parameters), error.Code, error.Message);

            return degraded;
         }

         try
         {
            return map(json);
         }
         catch (Exception ex) when (ex is JsonException or FormatException or KeyNotFoundException or InvalidOperationException or IndexOutOfRangeException)
         {
            Logger.Warn(ex, "MCP query '{0}' (tag {1}) returned an unparseable payload ({2} chars).", operation, tag, json.Length);
            Logger.Trace("MCP query '{0}' (tag {1}) payload: {2}", operation, tag, json);
            return degraded;
         }
      }
      finally
      {
         _correlator.Remove(tag);
      }
   }

   private static string Describe(Dictionary<string, string> parameters) =>
      parameters.Count == 0 ? "no params" : string.Join(" ", parameters.Select(p => $"{p.Key} {p.Value}"));
}
```

- [ ] **Step 5: Run the tests to verify they pass**

Run: `dotnet test Org.Edgerunner.Mud.MCP.Tests --filter "FullyQualifiedName~McpQueryProvider"`
Expected: PASS (13 tests).

- [ ] **Step 6: Commit**

```bash
git add Org.Edgerunner.Mud.MCP/Packages/McpQueryProvider.cs Org.Edgerunner.Mud.MCP.Tests/FakeQueryTerminal.cs Org.Edgerunner.Mud.MCP.Tests/McpQueryProviderTests.cs
git commit -m "Add McpQueryProvider implementing the full IMooWorldQueryProvider contract (udd-btl)"
```

---

### Task 6: `McpQueryPackage`

**Files:**
- Create: `Org.Edgerunner.Mud.MCP/Packages/McpQueryPackage.cs`
- Test: `Org.Edgerunner.Mud.MCP.Tests/McpQueryPackageTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `Org.Edgerunner.Mud.MCP.Tests/McpQueryPackageTests.cs`:

```csharp
using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
using Org.Edgerunner.Mud.MCP.Interfaces;
using Org.Edgerunner.Mud.MCP.Packages;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpQueryPackageTests
{
   private static McpClientSession CreateSession(string key = "KEY123")
   {
      var manager = new McpClientSessionManager(new Version(2, 1), new Version(2, 1), new List<IMcpPackage>());
      return new McpClientSession(manager, key, new Version(2, 1));
   }

   private static (McpQueryPackage Package, FakeQueryTerminal Terminal) CreateRegisteredPackage()
   {
      var package = new McpQueryPackage();
      var terminal = new FakeQueryTerminal();
      package.SetSession(CreateSession());
      package.OnPackageSupported(terminal);
      return (package, terminal);
   }

   private static Message Reply(string suffix, string tag, string data) =>
      new($"edgerunner-org-moo-query-{suffix}", "KEY123", new Dictionary<string, string>
      {
         ["tag:"] = tag,
         ["data:"] = data
      });

   [Theory]
   [InlineData("edgerunner-org-moo-query-verbs-reply", true)]
   [InlineData("EDGERUNNER-ORG-MOO-QUERY-PROPS-REPLY", true)]
   [InlineData("edgerunner-org-moo-query-error", true)]
   [InlineData("edgerunner-org-moo-query-verbs", false)]
   [InlineData("mcp-negotiate-can", false)]
   [InlineData("dns-org-mud-moo-simpleedit-content", false)]
   public void CanHandleMessage_MatchesRepliesAndErrorOnly(string name, bool expected)
   {
      var package = new McpQueryPackage();

      package.CanHandleMessage(new Message(name, "KEY123", new Dictionary<string, string>()))
         .Should().Be(expected);
   }

   [Fact]
   public void Package_AdvertisesNameAndVersion()
   {
      var package = new McpQueryPackage();

      package.Name.Should().Be("edgerunner-org-moo-query");
      package.MinimumVersion.Should().Be(1.0);
      package.MaximumVersion.Should().Be(1.0);
   }

   [Fact]
   public void OnPackageSupported_RegistersProviderExactlyOnce()
   {
      var package = new McpQueryPackage();
      var terminal = new FakeQueryTerminal();
      package.SetSession(CreateSession());

      var registrations = 0;
      terminal.QueryProviders.ProvidersChanged += (_, _) => registrations++;

      package.OnPackageSupported(terminal);
      package.OnPackageSupported(terminal);

      registrations.Should().Be(1);
   }

   [Fact]
   public void OnPackageSupported_WithoutSession_DoesNotRegister()
   {
      var package = new McpQueryPackage();
      var terminal = new FakeQueryTerminal();

      var registrations = 0;
      terminal.QueryProviders.ProvidersChanged += (_, _) => registrations++;

      package.OnPackageSupported(terminal);

      registrations.Should().Be(0);
   }

   [Fact]
   public async Task RoundTrip_QueryThroughRegistry_ReplyCompletesRequest()
   {
      var (package, terminal) = CreateRegisteredPackage();

      var task = terminal.QueryProviders.Query.GetVerbsAsync(new MooObjectId(123), CancellationToken.None);

      terminal.SentOutOfBandLines.Should().ContainSingle()
         .Which.Should().Be("edgerunner-org-moo-query-verbs KEY123 tag: 1 object: #123");

      var handled = package.ProcessMessage(terminal, Reply("verbs-reply", "1", "{\"d\":[\"look\"]}"));

      handled.Should().BeTrue();
      var result = await task;
      result.Should().ContainSingle();
      result[0].Aliases.Should().Equal("look");
   }

   [Fact]
   public async Task RoundTrip_MultilineChunks_AreReassembledWithoutSeparator()
   {
      var (package, terminal) = CreateRegisteredPackage();

      var task = terminal.QueryProviders.Query.GetVerbsAsync(new MooObjectId(123), CancellationToken.None);

      // The parser joins multiline 'data' continuation lines with '\n'; the package must strip them.
      package.ProcessMessage(terminal, Reply("verbs-reply", "1", "{\"d\":[\"lo\nok\"]}"));

      var result = await task;
      result[0].Aliases.Should().Equal("look");
   }

   [Fact]
   public async Task ErrorReply_CompletesRequestWithDegradedResult()
   {
      var (package, terminal) = CreateRegisteredPackage();

      var task = terminal.QueryProviders.Query.GetVerbInfoAsync(new MooObjectId(123), "look", CancellationToken.None);

      var error = new Message("edgerunner-org-moo-query-error", "KEY123", new Dictionary<string, string>
      {
         ["tag:"] = "1",
         ["code:"] = "E_VERBNF",
         ["message:"] = "no such verb"
      });

      package.ProcessMessage(terminal, error).Should().BeTrue();
      (await task).Should().BeNull();
   }

   [Fact]
   public void StaleTagReply_IsHandledAndDropped()
   {
      var (package, terminal) = CreateRegisteredPackage();

      package.ProcessMessage(terminal, Reply("verbs-reply", "99", "{\"d\":[]}")).Should().BeTrue();
   }

   [Fact]
   public void NonMatchingMessage_IsNotHandled()
   {
      var (package, terminal) = CreateRegisteredPackage();

      var message = new Message("dns-org-mud-moo-simpleedit-content", "KEY123", new Dictionary<string, string>());

      package.ProcessMessage(terminal, message).Should().BeFalse();
   }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test Org.Edgerunner.Mud.MCP.Tests --filter "FullyQualifiedName~McpQueryPackage"`
Expected: build FAILURE — `McpQueryPackage` does not exist.

- [ ] **Step 3: Create the package**

Create `Org.Edgerunner.Mud.MCP/Packages/McpQueryPackage.cs` (BSD header region first):

```csharp
using NLog;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP.Exceptions;
using Org.Edgerunner.Mud.MCP.Interfaces;

namespace Org.Edgerunner.Mud.MCP.Packages;

/// <summary>
/// Implements client support for the <c>edgerunner-org-moo-query</c> MCP package (v1.0): receives
/// tagged <c>…-reply</c>/<c>…-error</c> messages and completes the matching pending requests of its
/// <see cref="McpQueryProvider"/>. When MCP negotiation confirms the server supports the package, the
/// provider is registered with the terminal's query service at priority
/// <see cref="ProviderPriority"/>. See <c>docs/edgerunner-org-moo-query-protocol.md</c>.
/// </summary>
/// <seealso cref="IMcpPackage"/>
/// <seealso cref="IPackageNegotiationListener"/>
public class McpQueryPackage : IMcpPackage, IPackageNegotiationListener
{
   /// <summary>The MCP package name.</summary>
   public const string PackageName = "edgerunner-org-moo-query";

   /// <summary>The shared error reply message name.</summary>
   public const string ErrorMessageName = PackageName + "-error";

   /// <summary>The registry priority of the query provider (above SDWC's 100).</summary>
   public const int ProviderPriority = 200;

   private const string ReplySuffix = "-reply";

   private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

   private readonly McpQueryCorrelator _correlator = new();

   private readonly TimeSpan? _timeout;

   private readonly object _registrationLock = new();

   private McpClientSession? _session;

   private McpQueryProvider? _provider;

   /// <summary>
   /// Initializes a new instance of the <see cref="McpQueryPackage"/> class.
   /// </summary>
   /// <param name="timeout">An optional per-request timeout override for the provider (testing hook).</param>
   public McpQueryPackage(TimeSpan? timeout = null)
   {
      _timeout = timeout;
   }

   /// <inheritdoc/>
   public string Name { get; set; } = PackageName;

   /// <inheritdoc/>
   public double MinimumVersion { get; set; } = 1.0;

   /// <inheritdoc/>
   public double MaximumVersion { get; set; } = 1.0;

   /// <inheritdoc/>
   public void SetSession(McpClientSession session) => _session = session;

   /// <inheritdoc/>
   public bool CanHandleMessage(Message message)
   {
      var name = message.Name.ToLowerInvariant();
      if (!name.StartsWith(PackageName + "-", StringComparison.Ordinal))
         return false;

      return name == ErrorMessageName || name.EndsWith(ReplySuffix, StringComparison.Ordinal);
   }

   /// <inheritdoc/>
   public bool ProcessMessage(IClientTerminal client, Message message)
   {
      if (!CanHandleMessage(message))
         return false;

      if (!message.Data.TryGetValue("tag:", out var tag) || string.IsNullOrEmpty(tag))
      {
         Logger.Trace("Dropping MCP query reply '{0}' with no tag.", message.Name);
         return true;
      }

      bool completed;
      if (message.Name.Equals(ErrorMessageName, StringComparison.OrdinalIgnoreCase))
      {
         message.Data.TryGetValue("code:", out var code);
         message.Data.TryGetValue("message:", out var errorMessage);
         completed = _correlator.CompleteError(tag, new McpQueryErrorException(code ?? string.Empty, errorMessage ?? string.Empty));
      }
      else
      {
         // The parser joins multiline 'data' continuation lines with '\n'. The payload is minified
         // JSON and MOO strings cannot contain literal newlines, so stripping them reassembles the
         // transport chunks verbatim with no separator (protocol §3.2).
         message.Data.TryGetValue("data:", out var data);
         completed = _correlator.Complete(tag, (data ?? string.Empty).Replace("\n", string.Empty));
      }

      if (!completed)
         Logger.Trace("Dropping stale MCP query reply '{0}' (tag {1}).", message.Name, tag);

      return true;
   }

   /// <inheritdoc/>
   public void OnPackageSupported(IClientTerminal client)
   {
      lock (_registrationLock)
      {
         if (_provider != null)
            return;

         if (_session == null)
            return;

         _provider = new McpQueryProvider(client, _session.Key, _correlator, _timeout);
         client.QueryProviders.Register(_provider, ProviderPriority);
      }
   }

   /// <inheritdoc/>
   public void Reset() { }
}
```

- [ ] **Step 4: Run the tests to verify they pass**

Run: `dotnet test Org.Edgerunner.Mud.MCP.Tests --filter "FullyQualifiedName~McpQueryPackage"`
Expected: PASS (9 facts + 6 theory cases).

- [ ] **Step 5: Run the full MCP test sweep**

Run: `dotnet test Org.Edgerunner.Mud.MCP.Tests --filter "FullyQualifiedName~McpQuery|FullyQualifiedName~NegotiationListener|FullyQualifiedName~SimpleEdit" --no-build`
Expected: PASS, no failures.

- [ ] **Step 6: Commit**

```bash
git add Org.Edgerunner.Mud.MCP/Packages/McpQueryPackage.cs Org.Edgerunner.Mud.MCP.Tests/McpQueryPackageTests.cs
git commit -m "Add McpQueryPackage with negotiation-driven provider registration (udd-btl)"
```

---

### Task 7: Wire the package into terminal pages

**Files:**
- Modify: `Org.Edgerunner.Moo.Udditor/WindowManager.cs` (in `CreateTerminalPage`, around lines 334–369)

- [ ] **Step 1: Add the package to the MCP handler's package array**

In `Org.Edgerunner.Moo.Udditor/WindowManager.cs`, inside `CreateTerminalPage`, replace:

```csharp
      var simpleEdit = new SimpleEditPackage(new WindowManagerSimpleEditConsumer(this));
      oobHandler.RegisterHandler(new McpOobHandler(new Version(2, 1), new Version(2, 1),
         new IMcpPackage[] { simpleEdit }));
```

with:

```csharp
      var simpleEdit = new SimpleEditPackage(new WindowManagerSimpleEditConsumer(this));
      var queryPackage = new McpQueryPackage();
      oobHandler.RegisterHandler(new McpOobHandler(new Version(2, 1), new Version(2, 1),
         new IMcpPackage[] { simpleEdit, queryPackage }));
```

(If the actual code differs slightly in whitespace, match what is there; the change is solely
constructing `McpQueryPackage` and adding it to the array. The `using` for
`Org.Edgerunner.Mud.MCP.Packages` already exists in this file because `SimpleEditPackage` comes
from the same namespace.)

- [ ] **Step 2: Build the full solution (verification for this task is build-only)**

Run: `dotnet build "Moo Developer Tools.sln"`
Expected: Build succeeded, 0 errors (~1237 pre-existing warnings are normal).

- [ ] **Step 3: Commit**

```bash
git add Org.Edgerunner.Moo.Udditor/WindowManager.cs
git commit -m "Wire McpQueryPackage into terminal page MCP negotiation (udd-btl)"
```

---

### Task 8: Server package dump

**Files:**
- Create: `Server Packages/edgerunner-org-moo-query.moo`

This is classic-LambdaMOO Moo code in the dump format of
`docs/reference/dns-org-mud-moo-simpleedit.moo` (the JHCore simpleedit package): `;;`-property
lines, then `@args`/`@chown`/`@program` verb blocks ending with a lone `.`, finishing with
`"***finished***`. `#XXX` is a placeholder for the created object number (the install doc tells
the user to search-replace it). There is nothing to compile or test on this machine — verification
is careful self-review plus the user's live load.

Server-side conventions proven from the simpleedit reference:
- Handlers are called `(session, @declared-params)`; `session.connection` is the connected player.
- Every handler: `caller == this` guard → `set_task_perms(session.connection)` → `try/except v (ANY)`.
- All payload structures are built with `tonum()` ints (never raw objnums) before JSON encoding,
  because ToastStunt's `generate_json()` would otherwise encode objnums as `"#123"` strings.
- `generate_json()` is probed via `function_info()` and invoked through `call_function()` so the
  dump still compiles on servers without the builtin.

- [ ] **Step 1: Write the dump file**

Create `Server Packages/edgerunner-org-moo-query.moo` with exactly this content:

````
;;#XXX.("use_generate_json") = -1
;;#XXX.("version_range") = {"1.0", "1.0"}
;;#XXX.("messages_in") = {{"core-objects", {"tag"}}, {"children", {"tag", "object"}}, {"owned", {"tag", "owner"}}, {"parent", {"tag", "object"}}, {"verbs", {"tag", "object"}}, {"verb-info", {"tag", "object", "verb"}}, {"verb-doc", {"tag", "object", "verb"}}, {"verb-code", {"tag", "object", "verb"}}, {"props", {"tag", "object"}}, {"prop-info", {"tag", "object", "prop"}}, {"prop-doc", {"tag", "object", "prop"}}, {"prop-value", {"tag", "object", "prop"}}}
;;#XXX.("messages_out") = {{"core-objects-reply", {"tag", "data"}}, {"children-reply", {"tag", "data"}}, {"owned-reply", {"tag", "data"}}, {"parent-reply", {"tag", "data"}}, {"verbs-reply", {"tag", "data"}}, {"verb-info-reply", {"tag", "data"}}, {"verb-doc-reply", {"tag", "data"}}, {"verb-code-reply", {"tag", "data"}}, {"props-reply", {"tag", "data"}}, {"prop-info-reply", {"tag", "data"}}, {"prop-doc-reply", {"tag", "data"}}, {"prop-value-reply", {"tag", "data"}}, {"error", {"tag", "code", "message"}}}
;;#XXX.("aliases") = {"edgerunner-org-moo-query"}
;;#XXX.("description") = {"Developer-information query package for MCP 2.1 clients (v1.0).", "", "Each C->S request carries a client-generated tag; each S->C reply echoes the", "tag and carries one data* multiline field holding minified JSON.", "Object numbers are bare JSON ints; verb names are raw MOO names strings.", "", "Requests (params besides tag):", " -core-objects ()            -> {\"d\":[[num,name,[aliases]],...]}", " -children (object)          -> {\"d\":[[num,name,[aliases]],...]}", " -owned (owner)              -> {\"d\":[[num,name,[aliases]],...]}  owner \"\" = player", " -parent (object)            -> {\"p\":num}  -1 = no parent", " -verbs (object)             -> {\"d\":[\"g*et put\",...]}  local+inherited, deduped", " -verb-info (object, verb)   -> {\"q\",\"r\",\"a\",\"o\",\"p\",\"g\"}", " -verb-doc (object, verb)    -> {\"q\",\"r\",\"l\":[lines]}", " -verb-code (object, verb)   -> {\"q\",\"r\",\"l\":[lines]}", " -props (object)             -> {\"d\":[\"name\",...]}  local+inherited, deduped", " -prop-info (object, prop)   -> {\"n\",\"o\",\"p\",\"t\",\"v\"}  v = 80-char preview", " -prop-doc (object, prop)    -> {\"l\":[lines]}  toliteral split <=78 chars, max 50", " -prop-value (object, prop)  -> {\"t\",\"v\"}  full toliteral", "", "Shared error reply: -error (tag, code, message) where code is the MOO error", "name (E_PERM, E_INVARG, E_VERBNF, E_PROPNF, ...).", "", "Every handler runs under set_task_perms() of the connected player; normal MOO", "read rules decide visibility.", "", "Normative protocol: docs/edgerunner-org-moo-query-protocol.md in the", "Moo Developer Tools repository (https://github.com/.../Moo-Developer-Tools)."}

@args #XXX:"handle_core_objects" this none this
@chown #XXX:handle_core_objects #2
@program #XXX:handle_core_objects
"Usage: :handle_core_objects(session, tag)";
{session, tag} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  objs = {};
  for pname in (properties(#0))
    value = `#0.(pname) ! ANY => #-1';
    if (typeof(value) == OBJ && valid(value) && !(value in objs))
      objs = {@objs, value};
    endif
  endfor
  this:send_reply(session, "core-objects-reply", tag, this:summary_json(objs));
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_children" this none this
@chown #XXX:handle_children #2
@program #XXX:handle_children
"Usage: :handle_children(session, tag, object)";
{session, tag, object} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  this:send_reply(session, "children-reply", tag, this:summary_json(children(o)));
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_owned" this none this
@chown #XXX:handle_owned #2
@program #XXX:handle_owned
"Usage: :handle_owned(session, tag, owner) -- owner \"\" or absent means the connected player";
{session, tag, ?owner = ""} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  if (owner == "" || owner == 0)
    target = session.connection;
  else
    target = toobj(owner);
  endif
  if (typeof(target) != OBJ || !valid(target))
    raise(E_INVARG);
  endif
  owned = `target.owned_objects ! E_PROPNF';
  if (typeof(owned) == ERR)
    raise(E_INVARG, "This core has no owned_objects bookkeeping");
  endif
  if (typeof(owned) != LIST)
    owned = {};
  endif
  objs = {};
  for o in (owned)
    if (typeof(o) == OBJ && valid(o))
      objs = {@objs, o};
    endif
  endfor
  this:send_reply(session, "owned-reply", tag, this:summary_json(objs));
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_parent" this none this
@chown #XXX:handle_parent #2
@program #XXX:handle_parent
"Usage: :handle_parent(session, tag, object)";
{session, tag, object} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  this:send_reply(session, "parent-reply", tag, tostr("{\"p\":", tonum(parent(o)), "}"));
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_verbs" this none this
@chown #XXX:handle_verbs #2
@program #XXX:handle_verbs
"Usage: :handle_verbs(session, tag, object) -- local + inherited verb-names strings, deduped";
{session, tag, object} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  names = {};
  what = o;
  while (valid(what))
    for vname in (`verbs(what) ! E_PERM => {}')
      if (!(vname in names))
        names = {@names, vname};
      endif
    endfor
    what = parent(what);
  endwhile
  this:send_reply(session, "verbs-reply", tag, tostr("{\"d\":", this:json_encode(names), "}"));
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_verb_info" this none this
@chown #XXX:handle_verb_info #2
@program #XXX:handle_verb_info
"Usage: :handle_verb_info(session, tag, object, verb)";
{session, tag, object, vname} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  r = this:find_verb_definer(o, vname);
  info = verb_info(r, vname);
  vargs = verb_args(r, vname);
  json = tostr("{\"q\":", tonum(o), ",\"r\":", tonum(r), ",\"a\":", this:json_encode(info[3]), ",\"o\":", tonum(info[1]), ",\"p\":", this:json_encode(info[2]), ",\"g\":", this:json_encode(vargs), "}");
  this:send_reply(session, "verb-info-reply", tag, json);
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_verb_doc" this none this
@chown #XXX:handle_verb_doc #2
@program #XXX:handle_verb_doc
"Usage: :handle_verb_doc(session, tag, object, verb) -- leading string-literal lines of the code";
{session, tag, object, vname} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  r = this:find_verb_definer(o, vname);
  code = verb_code(r, vname);
  docs = {};
  for line in (code)
    mat = match(line, "^ *\"%(.*%)\"; *$");
    if (mat)
      inner = substitute("%1", mat);
      inner = strsub(strsub(inner, "\\\"", "\""), "\\\\", "\\");
      docs = {@docs, inner};
    else
      break;
    endif
  endfor
  json = tostr("{\"q\":", tonum(o), ",\"r\":", tonum(r), ",\"l\":", this:json_encode(docs), "}");
  this:send_reply(session, "verb-doc-reply", tag, json);
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_verb_code" this none this
@chown #XXX:handle_verb_code #2
@program #XXX:handle_verb_code
"Usage: :handle_verb_code(session, tag, object, verb)";
{session, tag, object, vname} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  r = this:find_verb_definer(o, vname);
  lines = verb_code(r, vname);
  json = tostr("{\"q\":", tonum(o), ",\"r\":", tonum(r), ",\"l\":", this:json_encode(lines), "}");
  this:send_reply(session, "verb-code-reply", tag, json);
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_props" this none this
@chown #XXX:handle_props #2
@program #XXX:handle_props
"Usage: :handle_props(session, tag, object) -- local + inherited property names, deduped";
{session, tag, object} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  names = {};
  what = o;
  while (valid(what))
    for pname in (`properties(what) ! E_PERM => {}')
      if (!(pname in names))
        names = {@names, pname};
      endif
    endfor
    what = parent(what);
  endwhile
  this:send_reply(session, "props-reply", tag, tostr("{\"d\":", this:json_encode(names), "}"));
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_prop_info" this none this
@chown #XXX:handle_prop_info #2
@program #XXX:handle_prop_info
"Usage: :handle_prop_info(session, tag, object, prop)";
{session, tag, object, pname} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  info = property_info(o, pname);
  value = o.(pname);
  lit = toliteral(value);
  preview = lit[1..min(80, length(lit))];
  json = tostr("{\"n\":", this:json_encode(pname), ",\"o\":", tonum(info[1]), ",\"p\":", this:json_encode(info[2]), ",\"t\":", typeof(value), ",\"v\":", this:json_encode(preview), "}");
  this:send_reply(session, "prop-info-reply", tag, json);
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_prop_doc" this none this
@chown #XXX:handle_prop_doc #2
@program #XXX:handle_prop_doc
"Usage: :handle_prop_doc(session, tag, object, prop) -- toliteral split into <=78-char lines, max 50";
{session, tag, object, pname} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  lit = toliteral(o.(pname));
  lines = {};
  start = 1;
  len = length(lit);
  while (start <= len && length(lines) < 50)
    finish = min(start + 77, len);
    lines = {@lines, lit[start..finish]};
    start = finish + 1;
  endwhile
  this:send_reply(session, "prop-doc-reply", tag, tostr("{\"l\":", this:json_encode(lines), "}"));
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_prop_value" this none this
@chown #XXX:handle_prop_value #2
@program #XXX:handle_prop_value
"Usage: :handle_prop_value(session, tag, object, prop)";
{session, tag, object, pname} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  value = o.(pname);
  json = tostr("{\"t\":", typeof(value), ",\"v\":", this:json_encode(toliteral(value)), "}");
  this:send_reply(session, "prop-value-reply", tag, json);
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"find_verb_definer" this none this
@chown #XXX:find_verb_definer #2
@program #XXX:find_verb_definer
"Usage: :find_verb_definer(object, verbname) => first ancestor whose verb_info answers; raises E_VERBNF";
{o, vname} = args;
what = o;
while (valid(what))
  if (`verb_info(what, vname) ! E_VERBNF => 0')
    return what;
  endif
  what = parent(what);
endwhile
raise(E_VERBNF);
.

@args #XXX:"summary_json" this none this
@chown #XXX:summary_json #2
@program #XXX:summary_json
"Usage: :summary_json(list of objects) => '{\"d\":[[num,name,[aliases]],...]}'";
"Object numbers are converted with tonum() BEFORE encoding so generate_json never sees objnums.";
{objs} = args;
rows = {};
for o in (objs)
  name = `o.name ! ANY => ""';
  if (typeof(name) != STR)
    name = tostr(name);
  endif
  aliases = `o.aliases ! ANY => {}';
  if (typeof(aliases) != LIST)
    aliases = {};
  endif
  strs = {};
  for a in (aliases)
    if (typeof(a) == STR)
      strs = {@strs, a};
    endif
  endfor
  rows = {@rows, {tonum(o), name, strs}};
endfor
return tostr("{\"d\":", this:json_encode(rows), "}");
.

@args #XXX:"json_encode" this none this
@chown #XXX:json_encode #2
@program #XXX:json_encode
"Usage: :json_encode(value) => minified JSON for strings, numbers, floats, objnums (bare ints), lists";
"Probes the generate_json() builtin once (cached in .use_generate_json: -1 unknown, 1 yes, 0 no).";
"Callers must convert objnums with tonum() first; the OBJ branch below is only a safety net for";
"the fallback encoder (ToastStunt generate_json would encode objnums as \"#123\" strings).";
{value} = args;
use = this.use_generate_json;
if (use == -1)
  use = (`function_info("generate_json") ! ANY => 0') ? 1 | 0;
  `this.use_generate_json = use ! E_PERM';
endif
if (use == 1)
  return call_function("generate_json", value);
endif
t = typeof(value);
if (t == STR)
  return tostr("\"", strsub(strsub(value, "\\", "\\\\"), "\"", "\\\""), "\"");
elseif (t == OBJ)
  return tostr(tonum(value));
elseif (t == LIST)
  parts = "";
  for item in (value)
    parts = tostr(parts, parts == "" ? "" | ",", this:json_encode(item));
  endfor
  return tostr("[", parts, "]");
elseif (t == ERR)
  return this:json_encode(tostr(value));
else
  return tostr(value);
endif
.

@args #XXX:"send_reply" this none this
@chown #XXX:send_reply #2
@program #XXX:send_reply
"Usage: :send_reply(session, reply-suffix, tag, json)";
"Single adaptation point for all reply traffic: emits the raw MCP multiline block, chunking the";
"JSON at <=4000 chars per data line. If your core exposes the session key or connection under";
"different property names, fix them HERE (and in send_error) only.";
{session, suffix, tag, json} = args;
if (caller != this)
  raise(E_PERM);
endif
conn = session.connection;
name = tostr("edgerunner-org-moo-query-", suffix);
dtag = tostr(random(100000), random(100000));
notify(conn, tostr("#$#", name, " ", session.key, " tag: \"", tag, "\" data*: \"\" _data-tag: ", dtag));
start = 1;
len = length(json);
while (start <= len)
  finish = min(start + 3999, len);
  notify(conn, tostr("#$#* ", dtag, " data: ", json[start..finish]));
  start = finish + 1;
endwhile
notify(conn, tostr("#$#: ", dtag));
.

@args #XXX:"send_error" this none this
@chown #XXX:send_error #2
@program #XXX:send_error
"Usage: :send_error(session, tag, error-list-from-except)";
"code = the MOO error name via toliteral (tostr would give the human message instead).";
{session, tag, v} = args;
if (caller != this)
  raise(E_PERM);
endif
code = toliteral(v[1]);
msg = strsub(tostr(v[2]), "\"", "'");
notify(session.connection, tostr("#$#edgerunner-org-moo-query-error ", session.key, " tag: \"", tag, "\" code: ", code, " message: \"", msg, "\""));
.

"***finished***
````

- [ ] **Step 2: Self-review the dump against the protocol doc**

Read the file back and verify each point; fix anything that fails:
1. Twelve `handle_*` verbs exist and their scatter lists match `messages_in` order
   (`session` first, then the declared params in order).
2. Every handler has the `caller != this` guard, `set_task_perms(session.connection)`, and a
   `try … except v (ANY)` wrapping that calls `this:send_error(session, tag, v)`.
3. Every JSON envelope matches the protocol doc catalog (keys, shapes, bare-int object numbers).
4. No raw objnum value is ever passed to `json_encode` (everything goes through `tonum()` or
   `summary_json`).
5. The dump uses only classic-LambdaMOO syntax: no `+=`, no maps, no ToastStunt-only builtins
   outside the `call_function` probe path.

- [ ] **Step 3: Commit**

```bash
git add "Server Packages/edgerunner-org-moo-query.moo"
git commit -m "Add edgerunner-org-moo-query server package dump (udd-btl)"
```

---

### Task 9: Install instructions

**Files:**
- Create: `Server Packages/edgerunner-org-moo-query-INSTALL.md`

- [ ] **Step 1: Write the document**

Create `Server Packages/edgerunner-org-moo-query-INSTALL.md` with exactly this content:

````markdown
# Installing the edgerunner-org-moo-query server package

This package answers the developer-information queries used by Moo Udditor (object browser,
contextual autocomplete, verb/property inspection). It targets cores with a JHCore-style MCP 2.1
implementation — the same framework that hosts `dns-org-mud-moo-simpleedit`.

Protocol reference: `docs/edgerunner-org-moo-query-protocol.md` in the Moo Developer Tools
repository. The package object's `description` property carries a condensed copy.

## Prerequisites

- A working server-side MCP 2.1 framework with package dispatch (handler verbs named
  `handle_<message>` called as `(session, @params)`), as used by your simpleedit package.
- A wizard character.

## Steps

1. **Create the package object** as a child of your core's generic MCP package parent (the same
   parent your simpleedit package object uses):

   ```
   @create <mcp-package-parent> named edgerunner-org-moo-query
   ```

   Note the object number it reports (e.g. `#231`).

2. **Replace the placeholder.** In `edgerunner-org-moo-query.moo`, search-replace every
   occurrence of `#XXX` with your object number. Also review the two `@chown … #2` lines per
   verb: `#2` assumes your archwizard is `#2`; adjust if not.

3. **Add the properties.** The framework may already define some metadata properties on the
   parent. For each property the dump assigns (`use_generate_json`, `version_range`,
   `messages_in`, `messages_out`, `aliases`, `description`), create it on your object if the
   parent doesn't provide it:

   ```
   @property #231.use_generate_json -1
   ```

   (repeat for the others, or rely on inherited definitions).

4. **Load the dump.** Paste the edited file into your wizard connection (or use your usual
   dump-loading mechanism). The `;;` lines set the properties; the `@args`/`@program` blocks
   create the verbs.

5. **Adapt the session accessors if needed.** All wire output is isolated in two verbs:
   `send_reply` and `send_error`. They assume the session object exposes:
   - `session.key` — the MCP session authentication key, and
   - `session.connection` — the connected player object.

   If your core's MCP session object uses different property names, fix them in those two verbs
   (and the `set_task_perms(session.connection)` line at the top of each handler) — nothing else
   touches the session.

6. **Register the package** with your core's MCP package registry, exactly the way your
   simpleedit package is registered (e.g. adding the object to the MCP registry's package list —
   consult how `dns-org-mud-moo-simpleedit` was installed on your core).

7. **Verify.** Connect with Moo Udditor. The client advertises `edgerunner-org-moo-query 1.0`
   during the MCP handshake; once the server's `mcp-negotiate-can` confirms it, Udditor's query
   features go live. Quick manual check from a raw client:

   ```
   #$#mcp authentication-key: TEST version: 2.1 to: 2.1
   #$#mcp-negotiate-can TEST package: edgerunner-org-moo-query min-version: "1.0" max-version: "1.0"
   #$#mcp-negotiate-end TEST
   #$#edgerunner-org-moo-query-parent TEST tag: 1 object: #1
   ```

   Expect a `-parent-reply` multiline block whose data is `{"p":<n>}`.

## Notes

- Every handler runs under `set_task_perms()` of the connected player; players see exactly what
  their MOO permissions allow. Failures come back as `-error` replies (`E_PERM`, `E_VERBNF`, …).
- `-owned` relies on the core's `.owned_objects` bookkeeping (maintained by `@create`/`@recycle`
  in LambdaCore lineage). Cores without it answer `-error E_INVARG`; the package never walks the
  whole database.
- On ToastStunt the JSON encoder uses the `generate_json()` builtin (probed once, cached in
  `.use_generate_json`); on classic LambdaMOO it falls back to a hand-rolled encoder. Reset the
  probe with `;#231.use_generate_json = -1` after a server-family change.
````

- [ ] **Step 2: Commit**

```bash
git add "Server Packages/edgerunner-org-moo-query-INSTALL.md"
git commit -m "Add edgerunner-org-moo-query install instructions (udd-btl)"
```

---

### Task 10: Final verification sweep

**Files:** none (verification only)

- [ ] **Step 1: Full solution build**

Run: `dotnet build "Moo Developer Tools.sln"`
Expected: Build succeeded, 0 errors.

- [ ] **Step 2: Run every affected test suite (filtered — never unfiltered)**

```bash
dotnet test Org.Edgerunner.Mud.MCP.Tests --filter "FullyQualifiedName~McpQuery|FullyQualifiedName~NegotiationListener|FullyQualifiedName~SimpleEdit" --no-build
dotnet test Org.Edgerunner.Mud.Common.Tests --filter "FullyQualifiedName~Querying" --no-build
dotnet test Org.Edgerunner.Mud.Communication.Tests --filter "FullyQualifiedName~Sdwc" --no-build
dotnet test Org.Edgerunner.Moo.Editor.Tests --filter "FullyQualifiedName~Completion" --no-build
```

Expected: all PASS (the last three suites were passing on master at 62/31/62 tests; they must
still pass untouched).

- [ ] **Step 3: Confirm a clean tree**

Run: `git status`
Expected: nothing to commit, working tree clean (every task committed its files).

> Merging, bead closure (`bd close udd-btl`), worktree cleanup, and `git push` are handled by the
> controlling session via superpowers:finishing-a-development-branch — not by this task.

---

## Self-review record

- **Spec coverage:** protocol doc (Task 1) ✓; negotiation hook = spec risk #1 (Task 2) ✓;
  correlator (Task 3) ✓; mapping incl. `DefiningObject`-=-queried rule, `p:-1`→null, `rxd`/`rc`
  flags, preposition aliases (Task 4) ✓; all-13-operations provider with degrade-but-always-log
  table, 10 s timeout, owner-empty-string convention (Task 5) ✓; package with reply/error
  routing, multiline reassembly (spec risk #2), idempotent priority-200 registration (Task 6) ✓;
  WindowManager wiring (Task 7) ✓; server dump with simpleedit dispatch convention (spec risk
  #3), per-op semantics incl. 80-char preview / 78-char×50-line prop-doc / no-DB-walk `-owned`,
  generate_json probe + tonum rule, ≤4000-char chunking (Task 8) ✓; install doc incl. `#XXX`
  replacement and session-accessor adaptation point (Task 9) ✓; full regression sweep (Task 10) ✓.
- **Placeholders:** none — every step carries complete code/content. `#XXX` in Task 8/9 is a
  deliberate spec-mandated artifact of the deliverable, not a plan placeholder.
- **Type consistency:** `McpQueryCorrelator` API (`NextTag`/`CreatePending`/`Complete`/
  `CompleteError`/`Remove`) is used identically in Tasks 3, 5, 6; `McpQueryErrorException(code,
  message)` with `.Code` matches Tasks 3, 5, 6; `McpQueryMapping` method names in Task 4 match
  every call in Task 5; `OnPackageSupported(IClientTerminal)` matches Tasks 2 and 6;
  `MooObjectId.ToString()` = `#n` matches the wire assertions in Tasks 5 and 6.
