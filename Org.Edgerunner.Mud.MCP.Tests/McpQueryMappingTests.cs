using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
using Org.Edgerunner.Mud.MCP.Packages;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpQueryMappingTests
{
   private static readonly MooObjectId Queried = new(123);

   [Fact]
   public void MapObjectSummaries_ParsesRows()
   {
      var json = "{\"d\":[[0,\"System Object\",[\"sysobj\"]],[1,\"Root Class\",[]]]}";

      var result = McpQueryMapping.MapObjectSummaries(json);

      result.Should().HaveCount(2);
      result[0].Id.Should().Be(new MooObjectId(0));
      result[0].Name.Should().Be("System Object");
      result[0].Aliases.Should().Equal("sysobj");
      result[1].Id.Should().Be(new MooObjectId(1));
      result[1].Aliases.Should().BeEmpty();
   }

   [Fact]
   public void MapObjectSummaries_EmptyList_ReturnsEmpty()
   {
      McpQueryMapping.MapObjectSummaries("{\"d\":[]}").Should().BeEmpty();
   }

   [Fact]
   public void MapParent_PositiveNumber_ReturnsId()
   {
      McpQueryMapping.MapParent("{\"p\":1}").Should().Be(new MooObjectId(1));
   }

   [Fact]
   public void MapParent_MinusOne_ReturnsNull()
   {
      McpQueryMapping.MapParent("{\"p\":-1}").Should().BeNull();
   }

   [Fact]
   public void MapVerbSummaries_PairRows_SetLocalAndInheritedOrigin()
   {
      var json = "{\"d\":[[\"g*et put\",1],[\"look_self\",0]]}";

      var result = McpQueryMapping.MapVerbSummaries(json, Queried);

      result.Should().HaveCount(2);
      result[0].Aliases.Should().Equal("g*et", "put");
      result[0].DefiningObject.Should().Be(Queried);
      result[0].Origin.Should().Be(MemberOrigin.Local);
      result[1].Aliases.Should().Equal("look_self");
      result[1].Origin.Should().Be(MemberOrigin.Inherited);
   }

   [Fact]
   public void MapVerbSummaries_LegacyStringRows_OriginUnknown()
   {
      var json = "{\"d\":[\"g*et put\",\"look_self\"]}";

      var result = McpQueryMapping.MapVerbSummaries(json, Queried);

      result.Should().HaveCount(2);
      result[0].Aliases.Should().Equal("g*et", "put");
      result[0].DefiningObject.Should().Be(Queried);
      result.Should().OnlyContain(v => v.Origin == MemberOrigin.Unknown);
   }

   [Fact]
   public void MapVerbSummaries_MalformedRows_DegradeToUnknownWithoutThrowing()
   {
      // A bare number row, an empty array, and a non-string first element are all malformed.
      var json = "{\"d\":[42,[],[7,1],[\"good\",0]]}";

      IReadOnlyList<MooVerbSummary> result = null!;
      var act = () => result = McpQueryMapping.MapVerbSummaries(json, Queried);

      act.Should().NotThrow();
      result.Should().ContainSingle();
      result[0].Aliases.Should().Equal("good");
      result[0].Origin.Should().Be(MemberOrigin.Inherited);
   }

   [Fact]
   public void MapVerbSummaries_PairRowWithoutNumericFlag_OriginUnknown()
   {
      var json = "{\"d\":[[\"g*et put\",\"oops\"]]}";

      var result = McpQueryMapping.MapVerbSummaries(json, Queried);

      result.Should().ContainSingle();
      result[0].Aliases.Should().Equal("g*et", "put");
      result[0].Origin.Should().Be(MemberOrigin.Unknown);
   }

   [Fact]
   public void MapPropertySummaries_PairRows_SetLocalAndInheritedOrigin()
   {
      var json = "{\"d\":[[\"name\",1],[\"aliases\",0]]}";

      var result = McpQueryMapping.MapPropertySummaries(json, Queried);

      result.Should().HaveCount(2);
      result[0].Name.Should().Be("name");
      result[0].DefiningObject.Should().Be(Queried);
      result[0].Origin.Should().Be(MemberOrigin.Local);
      result[1].Name.Should().Be("aliases");
      result[1].Origin.Should().Be(MemberOrigin.Inherited);
   }

   [Fact]
   public void MapPropertySummaries_LegacyStringRows_OriginUnknown()
   {
      var json = "{\"d\":[\"name\",\"aliases\"]}";

      var result = McpQueryMapping.MapPropertySummaries(json, Queried);

      result.Should().HaveCount(2);
      result[0].Name.Should().Be("name");
      result[0].DefiningObject.Should().Be(Queried);
      result.Should().OnlyContain(p => p.Origin == MemberOrigin.Unknown);
   }

   [Fact]
   public void MapPropertySummaries_MalformedRows_DegradeToUnknownWithoutThrowing()
   {
      var json = "{\"d\":[42,[],[7,1],[\"good\",1]]}";

      IReadOnlyList<MooPropertySummary> result = null!;
      var act = () => result = McpQueryMapping.MapPropertySummaries(json, Queried);

      act.Should().NotThrow();
      result.Should().ContainSingle();
      result[0].Name.Should().Be("good");
      result[0].Origin.Should().Be(MemberOrigin.Local);
   }

   [Fact]
   public void MapVerbInfo_ParsesAllFields()
   {
      var json = "{\"q\":123,\"r\":6,\"a\":\"g*et put\",\"o\":2,\"p\":\"rxd\",\"g\":[\"this\",\"none\",\"this\"]}";

      var result = McpQueryMapping.MapVerbInfo(json);

      result.QueriedObjectId.Should().Be(new MooObjectId(123));
      result.ResolvedObjectId.Should().Be(new MooObjectId(6));
      result.Aliases.Should().Equal("g*et", "put");
      result.Owner.Should().Be(new MooObjectId(2));
      result.Permissions.Should().Be(new VerbPermission(true, false, true, true));
      result.Args.Should().Be(new VerbArgs(DirectObject.This, Preposition.None, IndirectObject.This));
   }

   [Theory]
   [InlineData("none", Preposition.None)]
   [InlineData("any", Preposition.Any)]
   [InlineData("with/using", Preposition.With)]
   [InlineData("at/to", Preposition.At)]
   [InlineData("in front of", Preposition.InFrontOf)]
   [InlineData("in/inside/into", Preposition.In)]
   [InlineData("on top of/on/onto/upon", Preposition.OnTopOf)]
   [InlineData("out of/from inside/from", Preposition.OutOf)]
   [InlineData("over", Preposition.Over)]
   [InlineData("through", Preposition.Through)]
   [InlineData("under/underneath/beneath", Preposition.Under)]
   [InlineData("behind", Preposition.Behind)]
   [InlineData("beside", Preposition.Beside)]
   [InlineData("for/about", Preposition.For)]
   [InlineData("is", Preposition.Is)]
   [InlineData("as", Preposition.As)]
   [InlineData("off/off of", Preposition.Off)]
   [InlineData("garbage", Preposition.None)]
   public void ParsePreposition_ResolvesAliases(string spec, Preposition expected)
   {
      McpQueryMapping.ParsePreposition(spec).Should().Be(expected);
   }

   [Fact]
   public void MapVerbDocumentation_CarriesQueriedResolvedAndLines()
   {
      var json = "{\"q\":123,\"r\":6,\"l\":[\"Usage: foo\",\"Second line\"]}";

      var result = McpQueryMapping.MapVerbDocumentation(json);

      result.QueriedObjectId.Should().Be(new MooObjectId(123));
      result.ResolvedObjectId.Should().Be(new MooObjectId(6));
      result.Lines.Should().Equal("Usage: foo", "Second line");
   }

   [Fact]
   public void MapVerbCode_CarriesQueriedResolvedAndLines()
   {
      var json = "{\"q\":123,\"r\":6,\"l\":[\"return 1;\"]}";

      var result = McpQueryMapping.MapVerbCode(json);

      result.QueriedObjectId.Should().Be(new MooObjectId(123));
      result.ResolvedObjectId.Should().Be(new MooObjectId(6));
      result.Lines.Should().Equal("return 1;");
   }

   [Fact]
   public void MapPropertyInfo_FillsDefiningObjectWithQueriedId()
   {
      var json = "{\"n\":\"name\",\"o\":2,\"p\":\"rc\",\"t\":2,\"v\":\"\\\"Wizard\\\"\"}";

      var result = McpQueryMapping.MapPropertyInfo(json, Queried);

      result.Name.Should().Be("name");
      result.Owner.Should().Be(new MooObjectId(2));
      result.Permissions.Should().Be(new PropertyPermission(true, false, true));
      result.DefiningObject.Should().Be(Queried);
      result.ValueType.Should().Be(2);
      result.ValuePreview.Should().Be("\"Wizard\"");
   }

   [Fact]
   public void MapLines_ReturnsLines()
   {
      McpQueryMapping.MapLines("{\"l\":[\"a\",\"b\"]}").Should().Equal("a", "b");
   }

   [Fact]
   public void MapPropertyValue_ParsesTypeAndLiteral()
   {
      var result = McpQueryMapping.MapPropertyValue("{\"t\":4,\"v\":\"{1, 2}\"}");

      result.Type.Should().Be(4);
      result.Literal.Should().Be("{1, 2}");
   }
}
