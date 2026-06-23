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
   public async Task GetCurrentPlayerAsync_SendsTagOnlyRequest_AndMapsReply()
   {
      var (provider, correlator, terminal) = CreateProvider();

      var task = provider.GetCurrentPlayerAsync(CancellationToken.None);

      terminal.SentOutOfBandLines.Should().ContainSingle()
         .Which.Should().Be("edgerunner-org-moo-query-player KEY123 tag: 1");

      correlator.Complete("1", "{\"p\":62}");
      var result = await task;

      result.Should().Be(new MooObjectId(62));
   }

   [Fact]
   public async Task GetCurrentPlayerAsync_MapsNegativeToNull()
   {
      var (provider, correlator, _) = CreateProvider();

      var task = provider.GetCurrentPlayerAsync(CancellationToken.None);
      correlator.Complete("1", "{\"p\":-1}");

      (await task).Should().BeNull();
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

   [Fact]
   public async Task GetChildrenAsync_SendsObjectParameter()
   {
      var (provider, correlator, terminal) = CreateProvider();

      var task = provider.GetChildrenAsync(Target, CancellationToken.None);

      terminal.SentOutOfBandLines.Should().ContainSingle()
         .Which.Should().Be("edgerunner-org-moo-query-children KEY123 tag: 1 object: #123");

      correlator.Complete("1", "{\"d\":[]}");
      (await task).Should().BeEmpty();
   }

   [Fact]
   public async Task GetVerbDocumentationAsync_SendsVerbParameter()
   {
      var (provider, correlator, terminal) = CreateProvider();

      var task = provider.GetVerbDocumentationAsync(Target, "look", CancellationToken.None);

      terminal.SentOutOfBandLines.Should().ContainSingle()
         .Which.Should().Be("edgerunner-org-moo-query-verb-doc KEY123 tag: 1 object: #123 verb: look");

      correlator.Complete("1", "{\"q\":123,\"r\":6,\"l\":[\"Usage: look\"]}");
      var result = await task;

      result!.Lines.Should().Equal("Usage: look");
   }

   [Fact]
   public async Task GetVerbCodeAsync_SendsVerbParameter()
   {
      var (provider, correlator, terminal) = CreateProvider();

      var task = provider.GetVerbCodeAsync(Target, "look", CancellationToken.None);

      terminal.SentOutOfBandLines.Should().ContainSingle()
         .Which.Should().Be("edgerunner-org-moo-query-verb-code KEY123 tag: 1 object: #123 verb: look");

      correlator.Complete("1", "{\"q\":123,\"r\":6,\"l\":[\"return 1;\"]}");
      var result = await task;

      result!.Lines.Should().Equal("return 1;");
   }

   [Fact]
   public async Task GetPropertyDocumentationAsync_SendsPropParameter()
   {
      var (provider, correlator, terminal) = CreateProvider();

      var task = provider.GetPropertyDocumentationAsync(Target, "name", CancellationToken.None);

      terminal.SentOutOfBandLines.Should().ContainSingle()
         .Which.Should().Be("edgerunner-org-moo-query-prop-doc KEY123 tag: 1 object: #123 prop: name");

      correlator.Complete("1", "{\"l\":[\"\\\"Wizard\\\"\"]}");
      (await task).Should().Equal("\"Wizard\"");
   }

   [Fact]
   public async Task GetPropertyValueAsync_SendsPropParameter()
   {
      var (provider, correlator, terminal) = CreateProvider();

      var task = provider.GetPropertyValueAsync(Target, "name", CancellationToken.None);

      terminal.SentOutOfBandLines.Should().ContainSingle()
         .Which.Should().Be("edgerunner-org-moo-query-prop-value KEY123 tag: 1 object: #123 prop: name");

      correlator.Complete("1", "{\"t\":2,\"v\":\"\\\"Wizard\\\"\"}");
      var result = await task;

      result!.Type.Should().Be(2);
      result.Literal.Should().Be("\"Wizard\"");
   }
}
