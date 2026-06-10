using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
using Xunit;

namespace Org.Edgerunner.Mud.Common.Tests.Querying;

public class MooObjectReferenceParserTests
{
    [Theory]
    [InlineData("@program #123:verbname", 123)]
    [InlineData("#0:tell", 0)]
    [InlineData("@program #-1:foo", -1)]
    [InlineData("prefix text #42:bar suffix #99", 42)]
    public void FindFirstObjectId_returns_first_object_number(string text, int expected)
    {
        var result = MooObjectReferenceParser.FindFirstObjectId(text);

        result.Should().Be(new MooObjectId(expected));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("@edit foo:bar")]
    [InlineData("# 5 (space after hash)")]
    [InlineData("no references here")]
    public void FindFirstObjectId_returns_null_when_no_object_reference_present(string? text)
    {
        var result = MooObjectReferenceParser.FindFirstObjectId(text);

        result.Should().BeNull();
    }
}
