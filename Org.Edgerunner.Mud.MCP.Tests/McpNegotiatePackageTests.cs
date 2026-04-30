using FluentAssertions;
using NSubstitute;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP;
using Org.Edgerunner.Mud.MCP.Interfaces;
using Org.Edgerunner.Mud.MCP.Packages;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpNegotiatePackageTests
{
    private static McpClientSession CreateSession()
    {
        var manager = new McpClientSessionManager(new Version(2, 1), new Version(2, 1), new List<IMcpPackage>());
        var parser = new McpMessageParser();
        parser.FeedLine("mcp version: 2.1 to: 2.1");
        return (McpClientSession)manager.NegotiationMcpSession(parser.Result!)!;
    }

    [Fact]
    public void ProcessMessage_NegotiateCan_MatchingVersions_RecordsPackage()
    {
        var client = Substitute.For<IClientTerminal>();
        var mockPackage = Substitute.For<IMcpPackage>();
        mockPackage.Name.Returns("mcp-edit");
        mockPackage.MinimumVersion.Returns(1.0);
        mockPackage.MaximumVersion.Returns(1.0);

        var registry = new Dictionary<string, IMcpPackage> { ["mcp-edit"] = mockPackage };
        var package = new McpNegotiatePackage(registry);
        var session = CreateSession();
        package.SetSession(session);

        var msg = new Message("mcp-negotiate-can", session.Key, new Dictionary<string, string>
        {
            ["package:"] = "mcp-edit",
            ["min-version:"] = "1.0",
            ["max-version:"] = "1.0"
        });

        package.ProcessMessage(client, msg);

        session.SupportedPackages.Should().ContainKey("mcp-edit");
    }

    [Fact]
    public void ProcessMessage_NegotiateCan_VersionMismatch_DoesNotRecord()
    {
        var client = Substitute.For<IClientTerminal>();
        var mockPackage = Substitute.For<IMcpPackage>();
        mockPackage.Name.Returns("mcp-edit");
        mockPackage.MinimumVersion.Returns(2.0);
        mockPackage.MaximumVersion.Returns(2.0);

        var registry = new Dictionary<string, IMcpPackage> { ["mcp-edit"] = mockPackage };
        var package = new McpNegotiatePackage(registry);
        var session = CreateSession();
        package.SetSession(session);

        var msg = new Message("mcp-negotiate-can", session.Key, new Dictionary<string, string>
        {
            ["package:"] = "mcp-edit",
            ["min-version:"] = "1.0",
            ["max-version:"] = "1.0"
        });

        package.ProcessMessage(client, msg);

        session.SupportedPackages.Should().NotContainKey("mcp-edit");
    }

    [Fact]
    public void ProcessMessage_NegotiateCan_UnknownPackage_DoesNotThrow()
    {
        var client = Substitute.For<IClientTerminal>();
        var package = new McpNegotiatePackage(new Dictionary<string, IMcpPackage>());
        package.SetSession(CreateSession());

        var msg = new Message("mcp-negotiate-can", "key", new Dictionary<string, string>
        {
            ["package:"] = "mcp-unknown",
            ["min-version:"] = "1.0",
            ["max-version:"] = "1.0"
        });

        var act = () => package.ProcessMessage(client, msg);
        act.Should().NotThrow();
    }

    [Fact]
    public void ProcessMessage_NegotiateEnd_SetsIsNegotiationComplete()
    {
        var client = Substitute.For<IClientTerminal>();
        var package = new McpNegotiatePackage(new Dictionary<string, IMcpPackage>());
        var session = CreateSession();
        package.SetSession(session);

        package.ProcessMessage(client, new Message("mcp-negotiate-end", session.Key, new Dictionary<string, string>()));

        session.IsNegotiationComplete.Should().BeTrue();
    }

    [Fact]
    public void CanHandleMessage_NegotiateCan_ReturnsTrue()
    {
        var package = new McpNegotiatePackage(new Dictionary<string, IMcpPackage>());
        var msg = new Message("mcp-negotiate-can", "key", new Dictionary<string, string>());

        package.CanHandleMessage(msg).Should().BeTrue();
    }

    [Fact]
    public void CanHandleMessage_NegotiateEnd_ReturnsTrue()
    {
        var package = new McpNegotiatePackage(new Dictionary<string, IMcpPackage>());
        var msg = new Message("mcp-negotiate-end", "key", new Dictionary<string, string>());

        package.CanHandleMessage(msg).Should().BeTrue();
    }
}
