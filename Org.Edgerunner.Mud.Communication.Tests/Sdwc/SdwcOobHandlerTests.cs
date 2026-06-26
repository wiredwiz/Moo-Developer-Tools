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
   public void DomeClientUser_NowReturnsFalse_AndDoesNotRegister()
   {
      var terminal = new FakeSdwcTerminal();
      var registrations = 0;
      terminal.QueryProviders.ProvidersChanged += (_, _) => registrations++;
      var handler = new SdwcOobHandler();

      // The dome-client-user hack is gone: such lines are unhandled now.
      Feed(handler, terminal, " dome-client-user").Should().BeFalse();
      Feed(handler, terminal, "dome-client-user").Should().BeFalse();

      registrations.Should().Be(0);
      handler.Provider.Should().BeNull();
   }

   [Fact]
   public void SupportBroadcast_WithQueryableAbility_StoresCapsAndRegistersProviderOnce()
   {
      var terminal = new FakeSdwcTerminal();
      var registrations = 0;
      terminal.QueryProviders.ProvidersChanged += (_, _) => registrations++;
      var handler = new SdwcOobHandler();

      Feed(handler, terminal, " SDWC%%SUPPORT%%verbs|props|PROP-OVERLAY|VERB-OVERLAY|SUPPORT").Should().BeTrue();

      handler.ServerCapabilities.Should().NotBeNull();
      handler.ServerCapabilities!.SupportsVerbs.Should().BeTrue();
      handler.ServerCapabilities.SupportsProps.Should().BeTrue();
      handler.ServerCapabilities.SupportsVerbOverlay.Should().BeTrue();
      handler.ServerCapabilities.SupportsPropOverlay.Should().BeTrue();
      handler.ServerCapabilities.RawTokens.Should().Contain("SUPPORT");
      handler.Provider.Should().NotBeNull();
      registrations.Should().Be(1);
   }

   [Fact]
   public void SupportBroadcast_TrimsWhitespaceAroundTokens()
   {
      var terminal = new FakeSdwcTerminal();
      var handler = new SdwcOobHandler();

      Feed(handler, terminal, " SDWC%%SUPPORT%% verbs | props ").Should().BeTrue();

      handler.ServerCapabilities!.SupportsVerbs.Should().BeTrue();
      handler.ServerCapabilities.SupportsProps.Should().BeTrue();
   }

   [Fact]
   public void SupportBroadcast_WithNoQueryableAbility_StoresCapsButDoesNotRegister()
   {
      var terminal = new FakeSdwcTerminal();
      var registrations = 0;
      terminal.QueryProviders.ProvidersChanged += (_, _) => registrations++;
      var handler = new SdwcOobHandler();

      Feed(handler, terminal, " SDWC%%SUPPORT%%SUPPORT").Should().BeTrue();

      handler.ServerCapabilities.Should().NotBeNull();
      handler.ServerCapabilities!.HasAnyQueryableAbility.Should().BeFalse();
      handler.Provider.Should().BeNull();
      registrations.Should().Be(0);
   }

   [Fact]
   public void SupportBroadcast_EmptyPayload_StoresEmptyCapsAndStillSendsDeclaration()
   {
      var terminal = new FakeSdwcTerminal();
      var handler = new SdwcOobHandler();

      Feed(handler, terminal, " SDWC%%SUPPORT%%").Should().BeTrue();

      handler.ServerCapabilities.Should().NotBeNull();
      handler.ServerCapabilities!.RawTokens.Should().BeEmpty();
      handler.Provider.Should().BeNull();
      terminal.SentOutOfBandLines.Should().ContainSingle()
         .Which.Should().Be(" SDWC%%SUPPORT%%");
   }

   [Fact]
   public void SupportBroadcast_SendsDeclarationExactlyOnce_AcrossMultipleBroadcasts()
   {
      var terminal = new FakeSdwcTerminal();
      var handler = new SdwcOobHandler();

      Feed(handler, terminal, " SDWC%%SUPPORT%%verbs");
      Feed(handler, terminal, " SDWC%%SUPPORT%%verbs|props");

      terminal.SentOutOfBandLines.Should().ContainSingle()
         .Which.Should().Be(" SDWC%%SUPPORT%%");
   }

   [Fact]
   public void SupportBroadcast_DeclarationProducesExactWireLine()
   {
      var terminal = new FakeSdwcTerminal();
      var handler = new SdwcOobHandler();

      Feed(handler, terminal, " SDWC%%SUPPORT%%verbs");

      // SendOutOfBandLine prepends the OOB prefix "#$#" with no trailing space, so the value
      // passed carries the leading space to produce the exact wire line "#$# SDWC%%SUPPORT%%".
      terminal.SentOutOfBandLines.Single().Should().Be(" SDWC%%SUPPORT%%");
   }

   [Fact]
   public void SupportBroadcast_SecondBroadcast_RefreshesCapsWithoutResending()
   {
      var terminal = new FakeSdwcTerminal();
      var handler = new SdwcOobHandler();

      Feed(handler, terminal, " SDWC%%SUPPORT%%verbs");
      handler.ServerCapabilities!.SupportsProps.Should().BeFalse();

      Feed(handler, terminal, " SDWC%%SUPPORT%%verbs|props");
      handler.ServerCapabilities!.SupportsProps.Should().BeTrue();

      terminal.SentOutOfBandLines.Should().ContainSingle();
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
      Feed(handler, terminal, " SDWC%%SUPPORT%%verbs");
      terminal.SentOutOfBandLines.Clear(); // drop the outbound SUPPORT declaration

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
      Feed(handler, terminal, " SDWC%%SUPPORT%%verbs");

      handler.OnDisconnected();
      terminal.SentOutOfBandLines.Clear();

      // With no provider registered, the registry returns the empty fallback without sending a request.
      var result = await terminal.QueryProviders.Query.GetVerbsAsync(new MooObjectId(73), CancellationToken.None);

      result.Should().BeEmpty();
      terminal.SentOutOfBandLines.Should().BeEmpty();
   }

   [Fact]
   public void OnDisconnected_ResetsCapabilitiesAndDeclarationFlag_ThenReHandshakes()
   {
      var terminal = new FakeSdwcTerminal();
      var handler = new SdwcOobHandler();
      Feed(handler, terminal, " SDWC%%SUPPORT%%verbs");
      handler.ServerCapabilities.Should().NotBeNull();
      terminal.SentOutOfBandLines.Should().ContainSingle();

      handler.OnDisconnected();
      handler.Provider.Should().BeNull();
      handler.ServerCapabilities.Should().BeNull();

      var registrations = 0;
      terminal.QueryProviders.ProvidersChanged += (_, _) => registrations++;

      // Reconnect: the next SUPPORT broadcast must register a fresh provider and re-send the declaration.
      Feed(handler, terminal, " SDWC%%SUPPORT%%verbs");

      registrations.Should().Be(1);
      handler.Provider.Should().NotBeNull();
      handler.ServerCapabilities.Should().NotBeNull();
      terminal.SentOutOfBandLines.Should().HaveCount(2); // declaration re-sent after reconnect
   }

   [Fact]
   public void OnDisconnected_CalledTwice_DoesNotThrow()
   {
      var terminal = new FakeSdwcTerminal();
      var handler = new SdwcOobHandler();
      Feed(handler, terminal, " SDWC%%SUPPORT%%verbs");

      handler.OnDisconnected();
      Action act = () => handler.OnDisconnected();
      act.Should().NotThrow();
   }

   [Fact]
   public async Task Dispose_FaultsInFlightQueryAndUnregistersProvider()
   {
      var terminal = new FakeSdwcTerminal();
      var handler = new SdwcOobHandler(requestTimeout: TimeSpan.FromSeconds(30));
      Feed(handler, terminal, " SDWC%%SUPPORT%%verbs");

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
