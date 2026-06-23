using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
using Org.Edgerunner.Mud.Communication;
using Org.Edgerunner.Mud.Communication.Sdwc;
using Xunit;

namespace Org.Edgerunner.Mud.Communication.Tests.Sdwc;

public class SdwcOobHandlerTests
{
   private static bool Feed(SdwcOobHandler handler, FakeSdwcTerminal terminal, string line)
   {
      var state = new MessageProcessingState();
      return handler.ProcessMessage(terminal, line, ref state);
   }

   [Fact]
   public void DomeClientUser_RegistersProviderExactlyOnce()
   {
      var terminal = new FakeSdwcTerminal();
      var registrations = 0;
      terminal.QueryProviders.ProvidersChanged += (_, _) => registrations++;
      var handler = new SdwcOobHandler();

      // Leading space (wire form after #$# strip) plus repeated broadcasts.
      Feed(handler, terminal, " dome-client-user").Should().BeTrue();
      Feed(handler, terminal, " dome-client-user").Should().BeTrue();
      Feed(handler, terminal, "dome-client-user").Should().BeTrue();

      registrations.Should().Be(1);
      handler.Provider.Should().NotBeNull();
   }

   [Fact]
   public void NoWrapControlLines_AreConsumed()
   {
      var terminal = new FakeSdwcTerminal();
      var handler = new SdwcOobHandler();

      Feed(handler, terminal, " SDWC-START-NOWRAP").Should().BeTrue();
      Feed(handler, terminal, " SDWC-END-NOWRAP").Should().BeTrue();
   }

   [Fact]
   public void UnrecognizedLine_ReturnsFalse()
   {
      var terminal = new FakeSdwcTerminal();
      var handler = new SdwcOobHandler();

      Feed(handler, terminal, " some other oob line").Should().BeFalse();
   }

   [Fact]
   public void StrayResponse_ForUnknownKey_IsDropped()
   {
      var terminal = new FakeSdwcTerminal();
      var correlator = new SdwcCorrelator();
      var handler = new SdwcOobHandler(correlator);

      // No pending request; handled (true) but nothing to complete.
      Feed(handler, terminal, " SDWC%%VERBS%%{ \"object\": \"#73\", \"verbs\": [] }").Should().BeTrue();
      correlator.PendingCount.Should().Be(0);
   }

   [Fact]
   public void MalformedJsonResponse_IsDroppedWithoutThrowing()
   {
      var terminal = new FakeSdwcTerminal();
      var handler = new SdwcOobHandler();

      Feed(handler, terminal, " SDWC%%VERBS%%{ not json").Should().BeTrue();
   }

   [Fact]
   public void Response_CompletesMatchingPendingRequest()
   {
      var terminal = new FakeSdwcTerminal();
      var correlator = new SdwcCorrelator();
      var handler = new SdwcOobHandler(correlator);
      var pending = correlator.CreatePending(new SdwcCorrelationKey("VERBS", new MooObjectId(73), null));

      Feed(handler, terminal, " SDWC%%VERBS%%{ \"object\": \"#73\", \"verbs\": [] }");

      pending.IsCompletedSuccessfully.Should().BeTrue();
      correlator.PendingCount.Should().Be(0);
   }

   [Fact]
   public void OverlayResponse_CorrelatesOnObjectAndName()
   {
      var terminal = new FakeSdwcTerminal();
      var correlator = new SdwcCorrelator();
      var handler = new SdwcOobHandler(correlator);
      var wrongName = correlator.CreatePending(new SdwcCorrelationKey("VERB-OVERLAY", new MooObjectId(73), "other"));
      var rightName = correlator.CreatePending(new SdwcCorrelationKey("VERB-OVERLAY", new MooObjectId(73), "look"));

      Feed(handler, terminal, " SDWC%%VERB-OVERLAY%%{ \"object\": \"#73\", \"verb\": \"look\", \"value\": \"x\" }");

      rightName.IsCompletedSuccessfully.Should().BeTrue();
      wrongName.IsCompleted.Should().BeFalse();
   }

   // udd-bju: disconnect teardown faults in-flight queries and unregisters the dead provider.
   [Fact]
   public async Task OnDisconnected_FaultsInFlightQueryWithQueryConnectionClosedException()
   {
      var terminal = new FakeSdwcTerminal();
      // Generous timeout so a fault — not a timeout — is what completes the query.
      var handler = new SdwcOobHandler(requestTimeout: TimeSpan.FromSeconds(30));
      Feed(handler, terminal, " dome-client-user");

      var inFlight = terminal.QueryProviders.Query.GetVerbsAsync(new MooObjectId(73), CancellationToken.None);
      terminal.SentOutOfBandLines.Should().ContainSingle(); // request went out

      handler.OnDisconnected();

      var act = async () => await inFlight;
      await act.Should().ThrowAsync<QueryConnectionClosedException>();
   }

   [Fact]
   public async Task OnDisconnected_UnregistersProvider_SoRegistryNoLongerRoutesToIt()
   {
      var terminal = new FakeSdwcTerminal();
      var handler = new SdwcOobHandler();
      Feed(handler, terminal, " dome-client-user");

      handler.OnDisconnected();
      terminal.SentOutOfBandLines.Clear();

      // With no provider registered, the registry returns the empty fallback without sending a request.
      var result = await terminal.QueryProviders.Query.GetVerbsAsync(new MooObjectId(73), CancellationToken.None);

      result.Should().BeEmpty();
      terminal.SentOutOfBandLines.Should().BeEmpty();
   }

   [Fact]
   public void OnDisconnected_ThenCapabilitySignal_ReRegistersFreshProvider()
   {
      var terminal = new FakeSdwcTerminal();
      var handler = new SdwcOobHandler();
      Feed(handler, terminal, " dome-client-user");

      handler.OnDisconnected();
      handler.Provider.Should().BeNull();

      var registrations = 0;
      terminal.QueryProviders.ProvidersChanged += (_, _) => registrations++;

      // Reconnect: the next capability signal must register a fresh provider again.
      Feed(handler, terminal, " dome-client-user");

      registrations.Should().Be(1);
      handler.Provider.Should().NotBeNull();
   }

   [Fact]
   public void OnDisconnected_CalledTwice_DoesNotThrow()
   {
      var terminal = new FakeSdwcTerminal();
      var handler = new SdwcOobHandler();
      Feed(handler, terminal, " dome-client-user");

      handler.OnDisconnected();
      Action act = () => handler.OnDisconnected();
      act.Should().NotThrow();
   }

   [Fact]
   public async Task Dispose_FaultsInFlightQueryAndUnregistersProvider()
   {
      var terminal = new FakeSdwcTerminal();
      var handler = new SdwcOobHandler(requestTimeout: TimeSpan.FromSeconds(30));
      Feed(handler, terminal, " dome-client-user");

      var inFlight = terminal.QueryProviders.Query.GetVerbsAsync(new MooObjectId(73), CancellationToken.None);

      handler.Dispose();

      var act = async () => await inFlight;
      await act.Should().ThrowAsync<QueryConnectionClosedException>();

      terminal.SentOutOfBandLines.Clear();
      var result = await terminal.QueryProviders.Query.GetVerbsAsync(new MooObjectId(73), CancellationToken.None);
      result.Should().BeEmpty();
      terminal.SentOutOfBandLines.Should().BeEmpty();
   }
}
