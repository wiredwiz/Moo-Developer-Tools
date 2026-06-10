using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
using Org.Edgerunner.Mud.Communication.Sdwc;
using Xunit;

namespace Org.Edgerunner.Mud.Communication.Tests.Sdwc;

public class SdwcMappingTests
{
   [Theory]
   [InlineData("\"#123\"", 123)]
   [InlineData("123", 123)]
   [InlineData("\"#-1\"", -1)]
   [InlineData("-1", -1)]
   public void ParseObjectId_AcceptsStringOrBareNumber(string jsonValue, int expected)
   {
      using var document = System.Text.Json.JsonDocument.Parse(jsonValue);
      var id = SdwcMapping.ParseObjectId(document.RootElement);
      id.Should().Be(new MooObjectId(expected));
   }

   [Fact]
   public void ParseObjectId_Invalid_Throws()
   {
      Action act = () => SdwcMapping.ParseObjectId("not-an-id");
      act.Should().Throw<FormatException>();
   }

   [Fact]
   public void MapVerbs_SplitsWhitespaceAliases_AndUsesObjectAsDefining()
   {
      const string json = "{ \"object\": \"#73\", \"owner\": \"#2\", \"name\": \"player\", " +
                          "\"verbs\": [ { \"owner\": \"#2\", \"permissions\": \"rxd\", \"name\": \"look l examine\", \"args\": \"this none this\" } ] }";

      var verbs = SdwcMapping.MapVerbs(json);

      verbs.Should().ContainSingle();
      verbs[0].DefiningObject.Should().Be(new MooObjectId(73));
      verbs[0].Aliases.Should().Equal("look", "l", "examine");
   }

   [Fact]
   public void MapVerbs_AcceptsBareNumberObjectId()
   {
      const string json = "{ \"object\": 73, \"verbs\": [ { \"name\": \"foo\" } ] }";

      var verbs = SdwcMapping.MapVerbs(json);

      verbs.Should().ContainSingle();
      verbs[0].DefiningObject.Should().Be(new MooObjectId(73));
      verbs[0].Aliases.Should().Equal("foo");
   }

   [Fact]
   public void MapProperties_MapsKeysToSummaries()
   {
      const string json = "{ \"object\": \"#73\", \"name\": \"thing\", \"parent\": \"#1\", \"owner\": \"#2\", " +
                          "\"flags\": \"r\", \"props\": { \"description\": { \"clear\": false, \"owner\": \"#2\", \"permissions\": \"rc\" }, " +
                          "\"weight\": { \"clear\": true, \"owner\": \"#2\", \"permissions\": \"r\" } } }";

      var props = SdwcMapping.MapProperties(json);

      props.Select(p => p.Name).Should().BeEquivalentTo(new[] { "description", "weight" });
      props.Should().OnlyContain(p => p.DefiningObject == new MooObjectId(73));
   }

   [Fact]
   public void MapVerbOverlay_CarriesQueriedAndResolvedIds_AndSplitsLines()
   {
      const string json = "{ \"object\": \"#73\", \"resolved_object\": \"#1\", \"verb\": \"look\", \"value\": \"line one\\nline two\" }";

      var doc = SdwcMapping.MapVerbOverlay(json);

      doc.QueriedObjectId.Should().Be(new MooObjectId(73));
      doc.ResolvedObjectId.Should().Be(new MooObjectId(1));
      doc.Lines.Should().Equal("line one", "line two");
   }

   [Fact]
   public void MapVerbOverlay_DefaultsResolvedToQueriedWhenAbsent()
   {
      const string json = "{ \"object\": \"#73\", \"verb\": \"look\", \"value\": \"only\" }";

      var doc = SdwcMapping.MapVerbOverlay(json);

      doc.QueriedObjectId.Should().Be(new MooObjectId(73));
      doc.ResolvedObjectId.Should().Be(new MooObjectId(73));
      doc.Lines.Should().Equal("only");
   }

   [Fact]
   public void MapPropOverlay_ReturnsValuePreviewLines()
   {
      const string json = "{ \"object\": \"#73\", \"property\": \"description\", \"value\": \"A long\\r\\ndescription\" }";

      var lines = SdwcMapping.MapPropOverlay(json);

      lines.Should().Equal("A long", "description");
   }
}
