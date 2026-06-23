using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
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

   [Fact]
   public async Task FaultAll_FaultsEveryPendingTaskWithGivenException()
   {
      var correlator = new McpQueryCorrelator();
      var first = correlator.CreatePending("1");
      var second = correlator.CreatePending("2");

      var boom = new InvalidOperationException("boom");
      correlator.FaultAll(boom);

      var firstAct = async () => await first;
      var secondAct = async () => await second;
      (await firstAct.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(boom);
      (await secondAct.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(boom);
   }

   [Fact]
   public async Task FaultAll_LeavesCorrelatorReusable()
   {
      var correlator = new McpQueryCorrelator();
      correlator.CreatePending("1");

      correlator.FaultAll(new QueryConnectionClosedException());

      // Still usable: a fresh pending registers and completes normally.
      var next = correlator.CreatePending("2");
      correlator.Complete("2", "{\"d\":[]}").Should().BeTrue();
      (await next).Should().Be("{\"d\":[]}");
   }

   [Fact]
   public async Task Dispose_FaultsPendingTasksWithQueryConnectionClosedException()
   {
      var correlator = new McpQueryCorrelator();
      var pending = correlator.CreatePending("1");

      correlator.Dispose();

      var act = async () => await pending;
      await act.Should().ThrowAsync<QueryConnectionClosedException>();
   }

   [Fact]
   public async Task CreatePending_AfterDispose_ReturnsAlreadyFaultedTask()
   {
      var correlator = new McpQueryCorrelator();
      correlator.Dispose();

      var pending = correlator.CreatePending("1");

      pending.IsFaulted.Should().BeTrue();
      var act = async () => await pending;
      await act.Should().ThrowAsync<QueryConnectionClosedException>();
   }

   [Fact]
   public void CompleteAndRemove_AfterDispose_AreSafeNoOps()
   {
      var correlator = new McpQueryCorrelator();
      correlator.Dispose();

      correlator.Complete("1", "{}").Should().BeFalse();
      correlator.CompleteError("1", new McpQueryErrorException("E_PERM", "x")).Should().BeFalse();
      var act = () => correlator.Remove("1");
      act.Should().NotThrow();
   }

   [Fact]
   public void Dispose_Twice_DoesNotThrow()
   {
      var correlator = new McpQueryCorrelator();
      correlator.Dispose();

      var act = () => correlator.Dispose();
      act.Should().NotThrow();
   }
}
