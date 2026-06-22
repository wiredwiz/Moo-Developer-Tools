using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
using Xunit;

namespace Org.Edgerunner.Mud.Common.Tests.Querying;

public class QueryModelsTests
{
   [Fact]
   public void MooVerbSummary_DefaultsOriginToUnknown()
   {
      var summary = new MooVerbSummary(new[] { "look" }, new MooObjectId(73));

      summary.Origin.Should().Be(MemberOrigin.Unknown);
   }

   [Fact]
   public void MooPropertySummary_DefaultsOriginToUnknown()
   {
      var summary = new MooPropertySummary("description", new MooObjectId(73));

      summary.Origin.Should().Be(MemberOrigin.Unknown);
   }

   [Fact]
   public void MooVerbSummary_RecordEquality_IncludesOrigin()
   {
      var local = new MooVerbSummary(new[] { "look" }, new MooObjectId(73), MemberOrigin.Local);
      var inherited = local with { Origin = MemberOrigin.Inherited };

      inherited.Origin.Should().Be(MemberOrigin.Inherited);
      inherited.Should().NotBe(local);
   }

   [Fact]
   public void MooPropertySummary_RecordEquality_IncludesOrigin()
   {
      var local = new MooPropertySummary("description", new MooObjectId(73), MemberOrigin.Local);
      var inherited = local with { Origin = MemberOrigin.Inherited };

      inherited.Origin.Should().Be(MemberOrigin.Inherited);
      inherited.Should().NotBe(local);
   }
}
