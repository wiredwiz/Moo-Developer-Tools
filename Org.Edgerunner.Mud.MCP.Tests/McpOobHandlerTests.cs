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
