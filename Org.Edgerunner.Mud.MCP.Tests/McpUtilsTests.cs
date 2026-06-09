using FluentAssertions;
using Org.Edgerunner.Mud.MCP;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpUtilsTests
{
    [Fact]
    public void FormatMessage_NameAndKeyNoData_FormatsCorrectly()
    {
        var result = McpUtils.FormatMessage("mcp-negotiate-end", "abc123", new Dictionary<string, string>());

        result.Should().Be("mcp-negotiate-end abc123");
    }

    [Fact]
    public void FormatMessage_NoKey_OmitsKeySegment()
    {
        var result = McpUtils.FormatMessage("mcp", string.Empty, new Dictionary<string, string>
        {
            ["version:"] = "2.1",
            ["to:"] = "2.1"
        });

        result.Should().Be("mcp version: 2.1 to: 2.1");
    }

    [Fact]
    public void FormatMessage_ValueWithSpaces_QuotesValue()
    {
        var result = McpUtils.FormatMessage("mcp-edit-set", "abc123", new Dictionary<string, string>
        {
            ["name:"] = "My Object verb"
        });

        result.Should().Be("mcp-edit-set abc123 name: \"My Object verb\"");
    }

    [Fact]
    public void FormatMessage_SimpleValue_DoesNotQuote()
    {
        var result = McpUtils.FormatMessage("mcp-negotiate-can", "abc123", new Dictionary<string, string>
        {
            ["package:"] = "mcp-edit",
            ["min-version:"] = "1.0",
            ["max-version:"] = "1.0"
        });

        result.Should().Be("mcp-negotiate-can abc123 package: mcp-edit min-version: 1.0 max-version: 1.0");
    }

    [Fact]
    public void FormatMessage_ValueWithColon_QuotesValue()
    {
        var result = McpUtils.FormatMessage("mcp-negotiate-can", "abc123", new Dictionary<string, string>
        {
            ["package:"] = "a:b"
        });

        result.Should().Be("mcp-negotiate-can abc123 package: \"a:b\"");
    }

    [Fact]
    public void FormatMessage_ValueWithAsterisk_QuotesValue()
    {
        var result = McpUtils.FormatMessage("mcp-negotiate-can", "abc123", new Dictionary<string, string>
        {
            ["package:"] = "a*b"
        });

        result.Should().Be("mcp-negotiate-can abc123 package: \"a*b\"");
    }

    [Fact]
    public void FormatMessage_ValueWithQuote_QuotesValue()
    {
        var result = McpUtils.FormatMessage("mcp-negotiate-can", "abc123", new Dictionary<string, string>
        {
            ["package:"] = "a\"b"
        });

        result.Should().Be("mcp-negotiate-can abc123 package: \"a\"b\"");
    }

    [Fact]
    public void FormatMessage_PlainValue_DoesNotQuote()
    {
        var result = McpUtils.FormatMessage("mcp-negotiate-can", "abc123", new Dictionary<string, string>
        {
            ["package:"] = "foo"
        });

        result.Should().Be("mcp-negotiate-can abc123 package: foo");
    }
}
