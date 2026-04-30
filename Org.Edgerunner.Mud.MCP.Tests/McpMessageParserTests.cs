using FluentAssertions;
using Org.Edgerunner.Mud.MCP;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpMessageParserTests
{
    private readonly McpMessageParser _parser = new();

    [Fact]
    public void FeedLine_MessageNameOnly_ReturnsComplete()
    {
        var result = _parser.FeedLine("mcp");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Name.Should().Be("mcp");
        _parser.Result.Key.Should().BeEmpty();
        _parser.Result.Data.Should().BeEmpty();
    }

    [Fact]
    public void FeedLine_HandshakeNoAuthKey_ExtractsVersionFields()
    {
        var result = _parser.FeedLine("mcp version: 2.1 to: 2.1");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Name.Should().Be("mcp");
        _parser.Result.Key.Should().BeEmpty();
        _parser.Result.Data["version:"].Should().Be("2.1");
        _parser.Result.Data["to:"].Should().Be("2.1");
    }

    [Fact]
    public void FeedLine_MessageWithAuthKey_ExtractsKey()
    {
        var result = _parser.FeedLine("mcp-negotiate-can abc123 package: mcp-edit min-version: 1.0 max-version: 1.0");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Name.Should().Be("mcp-negotiate-can");
        _parser.Result.Key.Should().Be("abc123");
        _parser.Result.Data["package:"].Should().Be("mcp-edit");
        _parser.Result.Data["min-version:"].Should().Be("1.0");
        _parser.Result.Data["max-version:"].Should().Be("1.0");
    }

    [Fact]
    public void FeedLine_QuotedValue_UnquotesValue()
    {
        var result = _parser.FeedLine("mcp-edit-set abc123 name: \"My Object:verb\"");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Data["name:"].Should().Be("My Object:verb");
    }

    [Fact]
    public void FeedLine_ResultIsNullBeforeComplete()
    {
        _parser.FeedLine("mcp-edit-set abc123 content*: dt42");

        _parser.Result.Should().BeNull();
    }

    [Fact]
    public void FeedLine_MultilineHeader_ReturnsInProgress()
    {
        var result = _parser.FeedLine("mcp-edit-set abc123 name: foo content*: dt42");

        result.Should().Be(McpParseState.InProgress);
        _parser.Result.Should().BeNull();
    }

    [Fact]
    public void FeedLine_MultilineComplete_AssemblesFieldsFromContinuationLines()
    {
        _parser.FeedLine("mcp-edit-set abc123 name: foo content*: dt42");
        _parser.FeedLine("* dt42 content: first line");
        _parser.FeedLine("* dt42 content: second line");
        var result = _parser.FeedLine(": dt42");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Name.Should().Be("mcp-edit-set");
        _parser.Result.Key.Should().Be("abc123");
        _parser.Result.Data["name:"].Should().Be("foo");
        _parser.Result.Data["content:"].Should().Be("first line\nsecond line");
    }

    [Fact]
    public void FeedLine_MultilineWithNoContentLines_AssemblesEmptyField()
    {
        _parser.FeedLine("mcp-edit-set abc123 content*: dt42");
        var result = _parser.FeedLine(": dt42");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Data["content:"].Should().Be(string.Empty);
    }

    [Fact]
    public void FeedLine_UnexpectedContinuationInNormalState_ReturnsError()
    {
        var result = _parser.FeedLine("* dt42 content: value");

        result.Should().Be(McpParseState.Error);
    }

    [Fact]
    public void FeedLine_UnexpectedCloseInNormalState_ReturnsError()
    {
        var result = _parser.FeedLine(": dt42");

        result.Should().Be(McpParseState.Error);
    }

    [Fact]
    public void Reset_AfterMultilineInProgress_AllowsFreshParse()
    {
        _parser.FeedLine("mcp-edit-set abc123 content*: dt42");
        _parser.Reset();

        var result = _parser.FeedLine("mcp version: 2.1 to: 2.1");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Name.Should().Be("mcp");
    }
}
