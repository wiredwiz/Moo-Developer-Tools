using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
using Xunit;

namespace Org.Edgerunner.Mud.Common.Tests.Querying;

public class MooObjectIdTests
{
    [Fact]
    public void Nothing_Is_NegativeOne()
    {
        MooObjectId.Nothing.Number.Should().Be(-1);
        MooObjectId.Nothing.ToString().Should().Be("#-1");
    }

    [Fact]
    public void AmbiguousMatch_Is_NegativeTwo()
    {
        MooObjectId.AmbiguousMatch.Number.Should().Be(-2);
        MooObjectId.AmbiguousMatch.ToString().Should().Be("#-2");
    }

    [Fact]
    public void FailedMatch_Is_NegativeThree()
    {
        MooObjectId.FailedMatch.Number.Should().Be(-3);
        MooObjectId.FailedMatch.ToString().Should().Be("#-3");
    }

    [Fact]
    public void ToString_Formats_AsHashNumber()
    {
        new MooObjectId(123).ToString().Should().Be("#123");
    }

    [Fact]
    public void Equality_CompareByValue()
    {
        var a = new MooObjectId(42);
        var b = new MooObjectId(42);
        var c = new MooObjectId(7);

        (a == b).Should().BeTrue();
        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
        (a != c).Should().BeTrue();
        (a == c).Should().BeFalse();
    }
}
