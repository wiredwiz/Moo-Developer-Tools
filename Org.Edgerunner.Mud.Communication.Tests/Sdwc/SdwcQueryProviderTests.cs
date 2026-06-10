using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
using Org.Edgerunner.Mud.Communication;
using Org.Edgerunner.Mud.Communication.Sdwc;
using Xunit;

namespace Org.Edgerunner.Mud.Communication.Tests.Sdwc;

public class SdwcQueryProviderTests
{
   private static void Feed(SdwcOobHandler handler, FakeSdwcTerminal terminal, string line)
   {
      var state = new MessageProcessingState();
      handler.ProcessMessage(terminal, line, ref state);
   }

   // Wires a handler + provider against a fake terminal; the terminal echoes a scripted response for
   // each outbound request via the supplied responder so awaits complete synchronously.
   private static SdwcQueryProvider WireProvider(
      FakeSdwcTerminal terminal,
      SdwcCorrelator correlator,
      Func<string, string?> responder)
   {
      var handler = new SdwcOobHandler(correlator);
      terminal.OnOutOfBandLineSent = sent =>
      {
         var response = responder(sent);
         if (response != null)
            Feed(handler, terminal, response);
      };
      return new SdwcQueryProvider(terminal, correlator);
   }

   [Fact]
   public async Task GetVerbs_FormatsRequest_AndMapsResponse()
   {
      var terminal = new FakeSdwcTerminal();
      var correlator = new SdwcCorrelator();
      var provider = WireProvider(terminal, correlator,
         _ => " SDWC%%VERBS%%{ \"object\": \"#73\", \"verbs\": [ { \"name\": \"look l\" } ] }");

      var verbs = await provider.GetVerbsAsync(new MooObjectId(73), CancellationToken.None);

      terminal.SentOutOfBandLines.Should().ContainSingle().Which.Should().Be(" SDWC%%VERBS%%#73");
      verbs.Should().ContainSingle();
      verbs[0].Aliases.Should().Equal("look", "l");
      verbs[0].DefiningObject.Should().Be(new MooObjectId(73));
   }

   [Fact]
   public async Task GetProperties_FormatsRequest_AndMapsResponse()
   {
      var terminal = new FakeSdwcTerminal();
      var correlator = new SdwcCorrelator();
      var provider = WireProvider(terminal, correlator,
         _ => " SDWC%%PROPS%%{ \"object\": \"#73\", \"props\": { \"description\": {}, \"weight\": {} } }");

      var props = await provider.GetPropertiesAsync(new MooObjectId(73), CancellationToken.None);

      terminal.SentOutOfBandLines.Should().ContainSingle().Which.Should().Be(" SDWC%%PROPS%%#73");
      props.Select(p => p.Name).Should().BeEquivalentTo(new[] { "description", "weight" });
   }

   [Fact]
   public async Task GetVerbDocumentation_FormatsRequest_AndMapsOverlay()
   {
      var terminal = new FakeSdwcTerminal();
      var correlator = new SdwcCorrelator();
      var provider = WireProvider(terminal, correlator,
         _ => " SDWC%%VERB-OVERLAY%%{ \"object\": \"#73\", \"resolved_object\": \"#1\", \"verb\": \"look\", \"value\": \"a\\nb\" }");

      var doc = await provider.GetVerbDocumentationAsync(new MooObjectId(73), "look", CancellationToken.None);

      terminal.SentOutOfBandLines.Should().ContainSingle().Which.Should().Be(" SDWC%%VERB-OVERLAY%%#73%%look");
      doc.Should().NotBeNull();
      doc!.QueriedObjectId.Should().Be(new MooObjectId(73));
      doc.ResolvedObjectId.Should().Be(new MooObjectId(1));
      doc.Lines.Should().Equal("a", "b");
   }

   [Fact]
   public async Task GetPropertyDocumentation_FormatsRequest_AndMapsOverlay()
   {
      var terminal = new FakeSdwcTerminal();
      var correlator = new SdwcCorrelator();
      var provider = WireProvider(terminal, correlator,
         _ => " SDWC%%PROP-OVERLAY%%{ \"object\": \"#73\", \"property\": \"description\", \"value\": \"preview line 1\\npreview line 2\" }");

      var lines = await provider.GetPropertyDocumentationAsync(new MooObjectId(73), "description", CancellationToken.None);

      terminal.SentOutOfBandLines.Should().ContainSingle().Which.Should().Be(" SDWC%%PROP-OVERLAY%%#73%%description");
      lines.Should().Equal("preview line 1", "preview line 2");
   }

   [Fact]
   public async Task Timeout_Throws_AndClearsPendingEntry()
   {
      var terminal = new FakeSdwcTerminal(); // no responder: nothing answers
      var correlator = new SdwcCorrelator();
      var provider = new SdwcQueryProvider(terminal, correlator, TimeSpan.FromMilliseconds(50));

      var act = async () => await provider.GetVerbsAsync(new MooObjectId(73), CancellationToken.None);

      await act.Should().ThrowAsync<TimeoutException>();
      correlator.PendingCount.Should().Be(0);
   }

   [Fact]
   public async Task Cancellation_Propagates_AndClearsPendingEntry()
   {
      var terminal = new FakeSdwcTerminal();
      var correlator = new SdwcCorrelator();
      var provider = new SdwcQueryProvider(terminal, correlator, TimeSpan.FromSeconds(30));
      using var cts = new CancellationTokenSource();

      var task = provider.GetVerbsAsync(new MooObjectId(73), cts.Token);
      cts.Cancel();

      var act = async () => await task;
      await act.Should().ThrowAsync<OperationCanceledException>();
      correlator.PendingCount.Should().Be(0);
   }

   [Fact]
   public async Task UnsupportedMethods_ThrowNotImplementedException()
   {
      var terminal = new FakeSdwcTerminal();
      var provider = new SdwcQueryProvider(terminal, new SdwcCorrelator());
      var id = new MooObjectId(1);
      var ct = CancellationToken.None;

      await ((Func<Task>)(() => provider.GetObjectsAsync(ct))).Should().ThrowAsync<NotImplementedException>();
      await ((Func<Task>)(() => provider.GetChildrenAsync(id, ct))).Should().ThrowAsync<NotImplementedException>();
      await ((Func<Task>)(() => provider.GetOwnedObjectsAsync(ct))).Should().ThrowAsync<NotImplementedException>();
      await ((Func<Task>)(() => provider.GetOwnedObjectsAsync(id, ct))).Should().ThrowAsync<NotImplementedException>();
      await ((Func<Task>)(() => provider.GetParentAsync(id, ct))).Should().ThrowAsync<NotImplementedException>();
      await ((Func<Task>)(() => provider.GetVerbInfoAsync(id, "v", ct))).Should().ThrowAsync<NotImplementedException>();
      await ((Func<Task>)(() => provider.GetPropertyInfoAsync(id, "p", ct))).Should().ThrowAsync<NotImplementedException>();
      await ((Func<Task>)(() => provider.GetVerbCodeAsync(id, "v", ct))).Should().ThrowAsync<NotImplementedException>();
      await ((Func<Task>)(() => provider.GetPropertyValueAsync(id, "p", ct))).Should().ThrowAsync<NotImplementedException>();
   }
}
