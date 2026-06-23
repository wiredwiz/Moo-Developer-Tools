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

   // Fix 1: re-register the provider when the session key changes on renegotiation.
   [Fact]
   public async Task OnPackageSupported_SessionKeyChange_ReRegistersProviderWithNewKey()
   {
      var package = new McpQueryPackage(TimeSpan.FromSeconds(10));
      var terminal = new FakeQueryTerminal();

      // Confirm support with key A.
      package.SetSession(CreateSession("KEY-A"));
      package.OnPackageSupported(terminal);

      // Fire-and-forget a query; it should use KEY-A on the wire.
      _ = terminal.QueryProviders.Query.GetVerbsAsync(new MooObjectId(1), CancellationToken.None);
      terminal.SentOutOfBandLines.Should().ContainSingle()
         .Which.Should().Contain("KEY-A");

      terminal.SentOutOfBandLines.Clear();

      // Renegotiation: new session with key B.
      var registrations = 0;
      terminal.QueryProviders.ProvidersChanged += (_, _) => registrations++;

      package.SetSession(CreateSession("KEY-B"));
      package.OnPackageSupported(terminal);

      // ProvidersChanged fired for Unregister + Register = 2 events.
      registrations.Should().Be(2);

      // New query must use KEY-B on the wire.
      _ = terminal.QueryProviders.Query.GetVerbsAsync(new MooObjectId(2), CancellationToken.None);
      terminal.SentOutOfBandLines.Should().ContainSingle()
         .Which.Should().Contain("KEY-B");

      await Task.CompletedTask; // satisfy async signature
   }

   [Fact]
   public void OnPackageSupported_SameKeyCalledTwice_DoesNotReRegister()
   {
      var package = new McpQueryPackage();
      var terminal = new FakeQueryTerminal();
      package.SetSession(CreateSession("KEY123"));

      var registrations = 0;
      terminal.QueryProviders.ProvidersChanged += (_, _) => registrations++;

      package.OnPackageSupported(terminal);
      package.OnPackageSupported(terminal); // idempotent — same key, must not re-register

      registrations.Should().Be(1);
   }

   // Fix 2: drop path for a reply message with no tag.
   [Fact]
   public void ProcessMessage_ReplyWithNoTag_IsHandledAndDropped()
   {
      var (package, terminal) = CreateRegisteredPackage();

      var noTagReply = new Message("edgerunner-org-moo-query-verbs-reply", "KEY123",
         new Dictionary<string, string> { ["data:"] = "{\"d\":[]}" }); // no "tag:" key

      package.ProcessMessage(terminal, noTagReply).Should().BeTrue();
   }

   [Fact]
   public void ProcessMessage_ReplyWithEmptyTag_IsHandledAndDropped()
   {
      var (package, terminal) = CreateRegisteredPackage();

      var emptyTagReply = new Message("edgerunner-org-moo-query-verbs-reply", "KEY123",
         new Dictionary<string, string> { ["tag:"] = string.Empty, ["data:"] = "{\"d\":[]}" });

      package.ProcessMessage(terminal, emptyTagReply).Should().BeTrue();
   }

   // udd-bju: disconnect teardown faults in-flight queries and unregisters the dead provider.
   [Fact]
   public async Task OnDisconnected_FaultsInFlightQueryWithQueryConnectionClosedException()
   {
      // Generous timeout so a fault — not a timeout — is what completes the query.
      var package = new McpQueryPackage(TimeSpan.FromSeconds(30));
      var terminal = new FakeQueryTerminal();
      package.SetSession(CreateSession());
      package.OnPackageSupported(terminal);

      var inFlight = terminal.QueryProviders.Query.GetVerbsAsync(new MooObjectId(123), CancellationToken.None);
      terminal.SentOutOfBandLines.Should().ContainSingle(); // request went out

      package.OnDisconnected();

      var act = async () => await inFlight;
      await act.Should().ThrowAsync<QueryConnectionClosedException>();
   }

   [Fact]
   public async Task OnDisconnected_UnregistersProvider_SoRegistryNoLongerRoutesToIt()
   {
      var (package, terminal) = CreateRegisteredPackage();

      package.OnDisconnected();
      terminal.SentOutOfBandLines.Clear();

      // With no provider registered, the registry returns the empty fallback without sending a request.
      var result = await terminal.QueryProviders.Query.GetVerbsAsync(new MooObjectId(123), CancellationToken.None);

      result.Should().BeEmpty();
      terminal.SentOutOfBandLines.Should().BeEmpty();
   }

   [Fact]
   public void OnDisconnected_ThenReNegotiate_ReRegistersFreshProvider()
   {
      var package = new McpQueryPackage();
      var terminal = new FakeQueryTerminal();
      package.SetSession(CreateSession("KEY-A"));
      package.OnPackageSupported(terminal);

      package.OnDisconnected();

      var registrations = 0;
      terminal.QueryProviders.ProvidersChanged += (_, _) => registrations++;

      // Reconnect / re-negotiation: a fresh provider must register again.
      package.SetSession(CreateSession("KEY-B"));
      package.OnPackageSupported(terminal);

      registrations.Should().Be(1);
   }

   [Fact]
   public void OnDisconnected_CalledTwice_DoesNotThrow()
   {
      var (package, _) = CreateRegisteredPackage();

      package.OnDisconnected();
      Action act = () => package.OnDisconnected();
      act.Should().NotThrow();
   }

   [Fact]
   public async Task Dispose_FaultsInFlightQueryAndUnregistersProvider()
   {
      var package = new McpQueryPackage(TimeSpan.FromSeconds(30));
      var terminal = new FakeQueryTerminal();
      package.SetSession(CreateSession());
      package.OnPackageSupported(terminal);

      var inFlight = terminal.QueryProviders.Query.GetVerbsAsync(new MooObjectId(123), CancellationToken.None);

      package.Dispose();

      var act = async () => await inFlight;
      await act.Should().ThrowAsync<QueryConnectionClosedException>();

      terminal.SentOutOfBandLines.Clear();
      var result = await terminal.QueryProviders.Query.GetVerbsAsync(new MooObjectId(123), CancellationToken.None);
      result.Should().BeEmpty();
      terminal.SentOutOfBandLines.Should().BeEmpty();
   }
}
