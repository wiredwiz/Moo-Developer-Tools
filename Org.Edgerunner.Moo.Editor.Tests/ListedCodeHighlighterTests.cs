using System.Drawing;
using FluentAssertions;
using Org.Edgerunner.Moo.Editor;
using Org.Edgerunner.Moo.Editor.Configuration;
using Org.Edgerunner.Moo.Editor.Controls;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class ListedCodeHighlighterTests
{
   private const string Header = "#106:\"tell\"  this none this";

   private static Settings CreateSettings()
   {
      var settings = new Settings();
      settings.LoadDefaults();
      settings.DefaultGrammarDialect = GrammarDialect.Edgerunner;
      return settings;
   }

   private sealed class Capture
   {
      public List<string> PassThrough { get; } = new();
      public List<IReadOnlyList<(string Text, Color Color)>> Styled { get; } = new();

      public string StyledText(int index) => string.Concat(Styled[index].Select(s => s.Text));
   }

   private static ListedCodeHighlighter NewHighlighter(Settings settings = null)
   {
      settings ??= CreateSettings();
      return new ListedCodeHighlighter(() => GrammarDialect.Edgerunner, settings);
   }

   private static bool Handle(ListedCodeHighlighter h, Capture c, string line, DateTime when)
   {
      return h.TryHandle(line, when, s => c.PassThrough.Add(s), seg => c.Styled.Add(seg));
   }

   // ----- Header regex -----

   [Theory]
   [InlineData("#106:\"tell\"  this none this")]
   [InlineData("#106:tell this none this")]
   [InlineData("#106:\"get take\"  any in front of this")]
   [InlineData("#2:look this none this Wizard (#2), rxd")]
   public void Header_Regex_Matches_Standard_Forms(string header)
   {
      ListedCodeHighlighter.IsHeader(header).Should().BeTrue();
   }

   [Theory]
   [InlineData("You see nothing special.")]
   [InlineData("The wizard says, \"hello\"")]
   [InlineData("1:  player:tell(\"hi\");")]
   [InlineData("")]
   public void Header_Regex_Rejects_Ordinary_Lines(string line)
   {
      ListedCodeHighlighter.IsHeader(line).Should().BeFalse();
   }

   [Fact]
   public void Header_Is_Stripped_Of_Ansi_Before_Matching()
   {
      var c = new Capture();
      var h = NewHighlighter();
      var ansiHeader = "[36m#106:\"tell\"  this none this[0m\n";

      var consumed = Handle(h, c, ansiHeader, DateTime.UtcNow);

      consumed.Should().BeTrue();
      c.PassThrough.Should().ContainSingle().Which.Should().Be(ansiHeader);
   }

   // ----- Numbered listings -----

   [Fact]
   public void Numbered_Listing_Captures_Until_First_Non_Numbered_Line()
   {
      var c = new Capture();
      var h = NewHighlighter();
      var t = DateTime.UtcNow;

      Handle(h, c, Header + "\n", t).Should().BeTrue();
      Handle(h, c, "1:  player:tell(\"hi\");\n", t).Should().BeTrue();
      Handle(h, c, "2:  return 1;\n", t).Should().BeTrue();
      // First non-numbered line terminates and is passed through normally (not consumed).
      Handle(h, c, "Verb programmed.\n", t).Should().BeFalse();

      c.Styled.Should().HaveCount(2);
      c.StyledText(0).Should().Be("1:  player:tell(\"hi\");");
      c.StyledText(1).Should().Be("2:  return 1;");
   }

   [Fact]
   public void Numbered_Listing_Renders_Number_Prefix_Plainly()
   {
      var settings = CreateSettings();
      settings.DefaultWordColor = Color.FromArgb(255, 9, 9, 9);
      var c = new Capture();
      var h = NewHighlighter(settings);
      var t = DateTime.UtcNow;

      Handle(h, c, Header + "\n", t);
      Handle(h, c, "1:  return 1;\n", t).Should().BeTrue();

      var first = c.Styled[0][0];
      first.Text.Should().Be("1:");
      first.Color.Should().Be(settings.DefaultWordColor);
   }

   [Fact]
   public void Numbered_Listing_Numbering_May_Start_Above_One()
   {
      var c = new Capture();
      var h = NewHighlighter();
      var t = DateTime.UtcNow;

      Handle(h, c, Header + "\n", t);
      Handle(h, c, "17:  return 1;\n", t).Should().BeTrue();

      c.StyledText(0).Should().Be("17:  return 1;");
   }

   // ----- Unnumbered listings -----

   [Fact]
   public void Unnumbered_Listing_Captures_Contiguous_Lines()
   {
      var c = new Capture();
      var h = NewHighlighter();
      var t = DateTime.UtcNow;

      Handle(h, c, Header + "\n", t).Should().BeTrue();
      Handle(h, c, "player:tell(\"hi\");\n", t).Should().BeTrue();
      Handle(h, c, "return 1;\n", t.AddMilliseconds(50)).Should().BeTrue();

      c.Styled.Should().HaveCount(2);
      c.StyledText(0).Should().Be("player:tell(\"hi\");");
      c.StyledText(1).Should().Be("return 1;");
   }

   [Fact]
   public void Unnumbered_Listing_Terminates_On_Timing_Gap()
   {
      var c = new Capture();
      var h = NewHighlighter();
      var t = DateTime.UtcNow;

      Handle(h, c, Header + "\n", t);
      Handle(h, c, "player:tell(\"hi\");\n", t).Should().BeTrue();
      // A line arriving >= 500 ms after the previous captured line ends the listing.
      Handle(h, c, "Something unrelated\n", t.AddMilliseconds(500)).Should().BeFalse();

      c.Styled.Should().HaveCount(1);
   }

   [Fact]
   public void Unnumbered_Listing_Keeps_Capturing_Just_Under_The_Gap()
   {
      var c = new Capture();
      var h = NewHighlighter();
      var t = DateTime.UtcNow;

      Handle(h, c, Header + "\n", t);
      Handle(h, c, "player:tell(\"hi\");\n", t).Should().BeTrue();
      Handle(h, c, "return 1;\n", t.AddMilliseconds(499)).Should().BeTrue();

      c.Styled.Should().HaveCount(2);
   }

   [Fact]
   public void Unnumbered_Listing_Terminates_On_Blank_Line()
   {
      var c = new Capture();
      var h = NewHighlighter();
      var t = DateTime.UtcNow;

      Handle(h, c, Header + "\n", t);
      Handle(h, c, "player:tell(\"hi\");\n", t).Should().BeTrue();
      Handle(h, c, "\n", t.AddMilliseconds(10)).Should().BeFalse();

      c.Styled.Should().HaveCount(1);
   }

   // ----- Multi-verb / reset -----

   [Fact]
   public void New_Header_Mid_Capture_Ends_Block_And_Starts_New_One()
   {
      var c = new Capture();
      var h = NewHighlighter();
      var t = DateTime.UtcNow;

      Handle(h, c, Header + "\n", t);
      Handle(h, c, "1:  return 1;\n", t).Should().BeTrue();
      // A new header restarts a new listing block.
      Handle(h, c, "#107:\"look\"  this none this\n", t).Should().BeTrue();
      Handle(h, c, "1:  return 2;\n", t).Should().BeTrue();

      c.PassThrough.Should().HaveCount(2); // both headers
      c.Styled.Should().HaveCount(2);
      c.StyledText(1).Should().Be("1:  return 2;");
   }

   [Fact]
   public void Reset_Abandons_In_Progress_Capture()
   {
      var c = new Capture();
      var h = NewHighlighter();
      var t = DateTime.UtcNow;

      Handle(h, c, Header + "\n", t);
      h.Reset();
      // After reset, a numbered line is no longer treated as code.
      Handle(h, c, "1:  return 1;\n", t).Should().BeFalse();
      c.Styled.Should().BeEmpty();
   }

   [Fact]
   public void Ordinary_Lines_Are_Not_Consumed_When_Idle()
   {
      var c = new Capture();
      var h = NewHighlighter();

      Handle(h, c, "You see nothing special.\n", DateTime.UtcNow).Should().BeFalse();
      c.Styled.Should().BeEmpty();
      c.PassThrough.Should().BeEmpty();
   }
}
