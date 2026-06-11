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
