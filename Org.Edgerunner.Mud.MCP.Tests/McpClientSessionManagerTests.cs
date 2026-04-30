using FluentAssertions;
using Org.Edgerunner.Mud.MCP;
using Org.Edgerunner.Mud.MCP.Interfaces;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpClientSessionManagerTests
{
    private static Message ParseHandshake(string line)
    {
        var parser = new McpMessageParser();
        parser.FeedLine(line);
        return parser.Result!;
    }

    [Fact]
    public void NegotiationMcpSession_OverlappingVersions_ReturnsSession()
    {
        var manager = new McpClientSessionManager(new Version(2, 1), new Version(2, 1), new List<IMcpPackage>());
        var msg = ParseHandshake("mcp version: 2.1 to: 2.1");

        var session = manager.NegotiationMcpSession(msg);

        session.Should().NotBeNull();
        session!.ProtocolVersion.Should().Be(new Version(2, 1));
        session.Key.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NegotiationMcpSession_NonOverlappingVersions_ReturnsNull()
    {
        var manager = new McpClientSessionManager(new Version(2, 1), new Version(2, 1), new List<IMcpPackage>());
        var msg = ParseHandshake("mcp version: 1.0 to: 1.0");

        var session = manager.NegotiationMcpSession(msg);

        session.Should().BeNull();
    }

    [Fact]
    public void NegotiationMcpSession_GeneratesUniqueKeyPerSession()
    {
        var manager = new McpClientSessionManager(new Version(2, 1), new Version(2, 1), new List<IMcpPackage>());

        var session1 = manager.NegotiationMcpSession(ParseHandshake("mcp version: 2.1 to: 2.1"));
        var session2 = manager.NegotiationMcpSession(ParseHandshake("mcp version: 2.1 to: 2.1"));

        session1!.Key.Should().NotBe(session2!.Key);
    }
}
