using FluentAssertions;
using NSubstitute;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpMessageDispatcherTests
{
    private static Message ParseLine(string line)
    {
        var parser = new McpMessageParser();
        parser.FeedLine(line);
        return parser.Result!;
    }

    [Fact]
    public void Dispatch_Handshake_SendsHandshakeReply()
    {
        var client = Substitute.For<IClientTerminal>();
        var dispatcher = new McpMessageDispatcher(new Version(2, 1), new Version(2, 1));

        dispatcher.Dispatch(client, ParseLine("mcp version: 2.1 to: 2.1"));

        client.Received().SendOutOfBandLine(Arg.Is<string>(s => s.StartsWith("mcp authentication-key:")));
    }

    [Fact]
    public void Dispatch_Handshake_SendsNegotiateCanForEachPackage()
    {
        var client = Substitute.For<IClientTerminal>();
        var dispatcher = new McpMessageDispatcher(new Version(2, 1), new Version(2, 1));

        dispatcher.Dispatch(client, ParseLine("mcp version: 2.1 to: 2.1"));

        client.Received().SendOutOfBandLine(Arg.Is<string>(s =>
            s.Contains("mcp-negotiate-can") && s.Contains("package: mcp-negotiate")));
        client.Received().SendOutOfBandLine(Arg.Is<string>(s =>
            s.Contains("mcp-negotiate-can") && s.Contains("package: mcp-cord")));
    }

    [Fact]
    public void Dispatch_Handshake_SendsNegotiateEnd()
    {
        var client = Substitute.For<IClientTerminal>();
        var dispatcher = new McpMessageDispatcher(new Version(2, 1), new Version(2, 1));

        dispatcher.Dispatch(client, ParseLine("mcp version: 2.1 to: 2.1"));

        client.Received().SendOutOfBandLine(Arg.Is<string>(s => s.Contains("mcp-negotiate-end")));
    }

    [Fact]
    public void Dispatch_MessageBeforeHandshake_DropsMessage()
    {
        var client = Substitute.For<IClientTerminal>();
        var dispatcher = new McpMessageDispatcher(new Version(2, 1), new Version(2, 1));

        dispatcher.Dispatch(client, new Message("mcp-negotiate-end", "somekey",
            new Dictionary<string, string>()));

        client.DidNotReceive().SendOutOfBandLine(Arg.Any<string>());
    }

    [Fact]
    public void Dispatch_WrongAuthKey_DropsMessage()
    {
        var client = Substitute.For<IClientTerminal>();
        var dispatcher = new McpMessageDispatcher(new Version(2, 1), new Version(2, 1));
        dispatcher.Dispatch(client, ParseLine("mcp version: 2.1 to: 2.1"));
        client.ClearReceivedCalls();

        dispatcher.Dispatch(client, new Message("mcp-negotiate-end", "WRONGKEY",
            new Dictionary<string, string>()));

        client.DidNotReceive().SendOutOfBandLine(Arg.Any<string>());
    }

    [Fact]
    public void Dispatch_IncompatibleVersions_SessionRemainsNull()
    {
        var client = Substitute.For<IClientTerminal>();
        var dispatcher = new McpMessageDispatcher(new Version(2, 1), new Version(2, 1));

        dispatcher.Dispatch(client, ParseLine("mcp version: 1.0 to: 1.0"));

        dispatcher.Session.Should().BeNull();
        client.DidNotReceive().SendOutOfBandLine(Arg.Any<string>());
    }

    [Fact]
    public void Dispatch_UnknownPackage_DropsMessageSilently()
    {
        var client = Substitute.For<IClientTerminal>();
        var dispatcher = new McpMessageDispatcher(new Version(2, 1), new Version(2, 1));
        dispatcher.Dispatch(client, ParseLine("mcp version: 2.1 to: 2.1"));
        var key = dispatcher.Session!.Key;

        var act = () => dispatcher.Dispatch(client,
            new Message("unknown-thing-do", key, new Dictionary<string, string>()));

        act.Should().NotThrow();
    }
}
