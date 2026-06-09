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
        _parser.FeedLine("mcp-edit-set abc123 content*: \"\" _data-tag: dt42");

        _parser.Result.Should().BeNull();
    }

    [Fact]
    public void FeedLine_MultilineHeader_ReturnsInProgress()
    {
        var result = _parser.FeedLine("mcp-edit-set abc123 name: foo content*: \"\" _data-tag: dt42");

        result.Should().Be(McpParseState.InProgress);
        _parser.Result.Should().BeNull();
    }

    [Fact]
    public void FeedLine_MultilineComplete_AssemblesFieldsFromContinuationLines()
    {
        _parser.FeedLine("mcp-edit-set abc123 name: foo content*: \"\" _data-tag: dt42");
        _parser.FeedLine("* dt42 content: first line");
        _parser.FeedLine("* dt42 content: second line");
        var result = _parser.FeedLine(": dt42");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Name.Should().Be("mcp-edit-set");
        _parser.Result.Key.Should().Be("abc123");
        _parser.Result.Data["name:"].Should().Be("foo");
        _parser.Result.Data["content:"].Should().Be("first line\nsecond line");
        _parser.Result.Data.Should().NotContainKey("_data-tag:");
    }

    [Fact]
    public void FeedLine_MultilineWithNoContentLines_AssemblesEmptyField()
    {
        _parser.FeedLine("mcp-edit-set abc123 content*: \"\" _data-tag: dt42");
        var result = _parser.FeedLine(": dt42");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Data["content:"].Should().Be(string.Empty);
    }

    [Fact]
    public void FeedLine_CanonicalMultilineExample_PreservesLiteralContent()
    {
        _parser.FeedLine("spam 12345 from: Biff text*: \"\" _data-tag: 9b76");
        _parser.FeedLine("* 9b76 text: This is some sample text.");
        _parser.FeedLine("* 9b76 text:");
        _parser.FeedLine("* 9b76 text:     leading spaces and \"quotes\" and : colons are literal");
        var result = _parser.FeedLine(": 9b76");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Name.Should().Be("spam");
        _parser.Result.Key.Should().Be("12345");
        _parser.Result.Data["from:"].Should().Be("Biff");
        _parser.Result.Data["text:"].Should().Be(
            "This is some sample text.\n\n    leading spaces and \"quotes\" and : colons are literal");
        _parser.Result.Data.Should().NotContainKey("_data-tag:");
    }

    [Fact]
    public void FeedLine_MultilineInlineValue_IsIgnored()
    {
        _parser.FeedLine("mcp-edit-set abc123 text*: ignoreme _data-tag: 9b76");
        _parser.FeedLine("* 9b76 text: real content");
        var result = _parser.FeedLine(": 9b76");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Data["text:"].Should().Be("real content");
    }

    [Fact]
    public void FeedLine_TwoMultilineFieldsSharingOneDataTag_RoutesByKeyword()
    {
        _parser.FeedLine("mcp-edit-set abc123 name*: \"\" body*: \"\" _data-tag: dt7");
        _parser.FeedLine("* dt7 name: My Verb");
        _parser.FeedLine("* dt7 body: line one");
        _parser.FeedLine("* dt7 body: line two");
        var result = _parser.FeedLine(": dt7");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Data["name:"].Should().Be("My Verb");
        _parser.Result.Data["body:"].Should().Be("line one\nline two");
    }

    [Fact]
    public void FeedLine_MultilineFieldWithoutDataTag_ReturnsError()
    {
        var result = _parser.FeedLine("mcp-edit-set abc123 content*: \"\"");

        result.Should().Be(McpParseState.Error);
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
    public void FeedLine_ContinuationLinesWithTrailingNewline_DoNotDoubleSpaceContent()
    {
        // Mirrors production: the OOB transport delivers each line with a trailing "\n".
        _parser.FeedLine("dns-org-mud-moo-simpleedit-content KEY reference: \"#2:rport\" name: \"x\" type: moo-code content*: \"\" _data-tag: TAG\n");
        _parser.FeedLine("* TAG content: if (!args)\n");
        _parser.FeedLine("* TAG content:   return x;\n");
        _parser.FeedLine("* TAG content:     deeply indented\n");
        _parser.FeedLine("* TAG content: \n");
        _parser.FeedLine("* TAG content: tail line\n");
        var result = _parser.FeedLine(": TAG\n");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Name.Should().Be("dns-org-mud-moo-simpleedit-content");
        _parser.Result.Key.Should().Be("KEY");
        // Lines joined by a SINGLE "\n" with no doubled/blank lines, indentation intact,
        // and the empty content line preserved as a single blank line.
        _parser.Result.Data["content:"].Should().Be(
            "if (!args)\n  return x;\n    deeply indented\n\ntail line");
        _parser.Result.Data.Should().NotContainKey("_data-tag:");
    }

    [Fact]
    public void FeedLine_ContinuationLinesWithCrLfTerminator_CollapseCorrectly()
    {
        _parser.FeedLine("mcp-edit-set abc123 content*: \"\" _data-tag: dt42\r\n");
        _parser.FeedLine("* dt42 content: first line\r\n");
        _parser.FeedLine("* dt42 content:   second indented\r\n");
        var result = _parser.FeedLine(": dt42\r\n");

        result.Should().Be(McpParseState.Complete);
        _parser.Result!.Data["content:"].Should().Be("first line\n  second indented");
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
