using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
using Org.Edgerunner.Mud.Communication;
using Org.Edgerunner.Mud.Communication.OutOfBand;
using Org.Edgerunner.Mud.Communication.Sdwc;
using Org.Edgerunner.Mud.MCP.Interfaces;
using Org.Edgerunner.Mud.MCP.Packages;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

/// <summary>
/// End-to-end cascade tests (udd-bju): a disconnect signal driven through the full
/// <see cref="RootMessageProcessor"/> → <see cref="OutOfBandMessageProcessor"/> → handler chain tears
/// down both the MCP and SDWC query sources, faulting their in-flight requests, and is idempotent.
/// </summary>
public class DisconnectTeardownCascadeTests
{
   private const string Prefix = "#$#";

   private static (RootMessageProcessor Processor, FakeQueryTerminal Terminal, SdwcOobHandler Sdwc)
      BuildChain()
   {
      var terminal = new FakeQueryTerminal();
      var oob = new OutOfBandMessageProcessor();

      var queryPackage = new McpQueryPackage(TimeSpan.FromSeconds(30));
      oob.RegisterHandler(new McpOobHandler(new Version(2, 1), new Version(2, 1),
         new IMcpPackage[] { queryPackage }));
      var sdwc = new SdwcOobHandler(requestTimeout: TimeSpan.FromSeconds(30));
      oob.RegisterHandler(sdwc);

      var processor = new RootMessageProcessor(Prefix, oob) { OutOfBandMessagingTimeout = 500000 };
      return (processor, terminal, sdwc);
   }

   /// <summary>
   /// Registers the SDWC provider by signalling the handler directly. The MCP OOB handler (registered
   /// first) otherwise consumes the bare <c>dome-client-user</c> line, so the cascade tests drive SDWC
   /// registration on the handler instance while still triggering teardown via the processor.
   /// </summary>
   private static void RegisterSdwc(SdwcOobHandler sdwc, FakeQueryTerminal terminal)
   {
      var state = new MessageProcessingState();
      sdwc.ProcessMessage(terminal, " dome-client-user", ref state);
   }

   /// <summary>Drives the MCP handshake + negotiation on the wire so the query provider registers.</summary>
   private static void NegotiateMcpQuery(RootMessageProcessor processor, FakeQueryTerminal terminal)
   {
      processor.ProcessMessage(terminal, $"{Prefix}mcp version: 2.1 to: 2.1\n");

      // The handshake reply carries the client-chosen authentication key; reuse it for negotiation.
      var handshake = terminal.SentOutOfBandLines.First(l => l.StartsWith("mcp authentication-key:"));
      var key = handshake.Split(' ')[2];

      processor.ProcessMessage(terminal,
         $"{Prefix}mcp-negotiate-can {key} package: {McpQueryPackage.PackageName} min-version: 1.0 max-version: 1.0\n");
      processor.ProcessMessage(terminal, $"{Prefix}mcp-negotiate-end {key}\n");
   }

   [Fact]
   public async Task OnDisconnected_CascadesToMcpSource_FaultingInFlightQuery()
   {
      var (processor, terminal, _) = BuildChain();

      NegotiateMcpQuery(processor, terminal);

      // A verbs query routes to the MCP provider. The disconnect cascade through the processor must
      // reach the MCP source and fault this request.
      var mcpInFlight = terminal.QueryProviders.Query.GetVerbsAsync(new MooObjectId(1), CancellationToken.None);

      processor.OnDisconnected();

      var act = async () => await mcpInFlight;
      await act.Should().ThrowAsync<QueryConnectionClosedException>();
   }

   [Fact]
   public async Task OnDisconnected_CascadesToSdwcSource_FaultingInFlightQuery()
   {
      var (processor, terminal, sdwc) = BuildChain();

      RegisterSdwc(sdwc, terminal);

      var sdwcInFlight = terminal.QueryProviders.Query.GetVerbsAsync(new MooObjectId(73), CancellationToken.None);

      // Teardown is triggered through the processor, proving the cascade reaches the SDWC handler.
      processor.OnDisconnected();

      var act = async () => await sdwcInFlight;
      await act.Should().ThrowAsync<QueryConnectionClosedException>();
   }

   [Fact]
   public void OnDisconnected_CalledTwice_IsIdempotent()
   {
      var (processor, terminal, sdwc) = BuildChain();
      NegotiateMcpQuery(processor, terminal);
      RegisterSdwc(sdwc, terminal);

      processor.OnDisconnected();
      Action act = () => processor.OnDisconnected();
      act.Should().NotThrow();
   }

   [Fact]
   public void OnDisconnected_LeavesMcpSourceReusable_ReRegisterOnReconnect()
   {
      var (processor, terminal, _) = BuildChain();
      NegotiateMcpQuery(processor, terminal);

      processor.OnDisconnected();
      terminal.SentOutOfBandLines.Clear();

      // Reconnect: re-negotiate MCP; the provider must register afresh and serve real requests again.
      NegotiateMcpQuery(processor, terminal);

      var query = terminal.QueryProviders.Query.GetVerbsAsync(new MooObjectId(5), CancellationToken.None);
      query.IsFaulted.Should().BeFalse(); // a real request is in flight again, not an immediate fault
      terminal.SentOutOfBandLines.Any(l => l.Contains("edgerunner-org-moo-query-verbs")).Should().BeTrue();

      // Tear down so the in-flight task does not dangle.
      processor.OnDisconnected();
   }

   [Fact]
   public async Task Dispose_CascadesThroughChain_FaultingInFlightQuery()
   {
      var (processor, terminal, sdwc) = BuildChain();
      RegisterSdwc(sdwc, terminal);

      var inFlight = terminal.QueryProviders.Query.GetVerbsAsync(new MooObjectId(73), CancellationToken.None);

      processor.Dispose();

      var act = async () => await inFlight;
      await act.Should().ThrowAsync<QueryConnectionClosedException>();
   }

   [Fact]
   public void Dispose_CalledTwice_DoesNotThrow()
   {
      var (processor, terminal, sdwc) = BuildChain();
      RegisterSdwc(sdwc, terminal);

      processor.Dispose();
      Action act = () => processor.Dispose();
      act.Should().NotThrow();
   }
}
