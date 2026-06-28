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

      /// <summary>Every flushed block, in flush order; each block is a list of lines of (text,color) segments.</summary>
      public List<IReadOnlyList<IReadOnlyList<(string Text, Color Color)>>> Blocks { get; } = new();

      /// <summary>The concatenated text of a single line within a flushed block.</summary>
      public string LineText(int blockIndex, int lineIndex) =>
         string.Concat(Blocks[blockIndex][lineIndex].Select(s => s.Text));
   }

   private static ListedCodeHighlighter NewHighlighter(
      Settings settings = null,
      Action armFlush = null,
      Action cancelFlush = null)
   {
      settings ??= CreateSettings();
      return new ListedCodeHighlighter(() => GrammarDialect.Edgerunner, settings, armFlush, cancelFlush);
   }

   private static bool Handle(ListedCodeHighlighter h, Capture c, string line)
   {
      return h.TryHandle(line, s => c.PassThrough.Add(s), b => c.Blocks.Add(b));
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
   public void Numbered_IdleFlush_RendersBatch_ButKeepsCapturing()
   {
      var c = new Capture();
      var h = NewHighlighter();

      Handle(h, c, Header).Should().BeTrue();
      Handle(h, c, "1:  if (x)").Should().BeTrue();
      Handle(h, c, "2:    return 1;").Should().BeTrue();

      // The idle timer fires mid-listing (a >IdleFlushMilliseconds streaming gap on a long verb).
      h.FlushPending();
      c.Blocks.Should().HaveCount(1, "the buffered-so-far lines are rendered as a batch");
      c.Blocks[0].Should().HaveCount(2);

      // Numbered listings are NOT terminated by the idle flush: subsequent numbered lines must still
      // be captured (consumed), not fall through as normal unhighlighted text.
      Handle(h, c, "3:  endif").Should().BeTrue();
      Handle(h, c, "4:  return 0;").Should().BeTrue();

      // Only the first non-numbered line ends the listing, flushing the remaining buffer.
      Handle(h, c, "You see nothing special.").Should().BeFalse();
      c.Blocks.Should().HaveCount(2);
      c.Blocks[1].Should().HaveCount(2, "lines 3 and 4 were captured after the mid-stream idle flush");
   }

   [Fact]
   public void Header_Is_Stripped_Of_Ansi_Before_Matching()
   {
      var c = new Capture();
      var h = NewHighlighter();
      var ansiHeader = "[36m#106:\"tell\"  this none this[0m\n";

      var consumed = Handle(h, c, ansiHeader);

      consumed.Should().BeTrue();
      c.PassThrough.Should().ContainSingle().Which.Should().Be(ansiHeader);
   }

   // ----- Numbered listings -----

   [Fact]
   public void Numbered_Listing_Buffers_Until_First_Non_Numbered_Line_Then_Flushes_One_Block()
   {
      var c = new Capture();
      var h = NewHighlighter();

      Handle(h, c, Header + "\n").Should().BeTrue();
      Handle(h, c, "1:  player:tell(\"hi\");\n").Should().BeTrue();
      Handle(h, c, "2:  return 1;\n").Should().BeTrue();
      // Nothing flushed yet: lines are buffered.
      c.Blocks.Should().BeEmpty();
      // First non-numbered line terminates: it flushes the block and is passed through normally.
      Handle(h, c, "Verb programmed.\n").Should().BeFalse();

      c.Blocks.Should().ContainSingle();
      c.Blocks[0].Should().HaveCount(2);
      c.LineText(0, 0).Should().Be("1:  player:tell(\"hi\");");
      c.LineText(0, 1).Should().Be("2:  return 1;");
   }

   [Fact]
   public void Numbered_Listing_Renders_Number_Prefix_Plainly()
   {
      var settings = CreateSettings();
      settings.DefaultWordColor = Color.FromArgb(255, 9, 9, 9);
      var c = new Capture();
      var h = NewHighlighter(settings);

      Handle(h, c, Header + "\n");
      Handle(h, c, "1:  return 1;\n").Should().BeTrue();
      h.FlushPending();

      var first = c.Blocks[0][0][0];
      first.Text.Should().Be("1:");
      first.Color.Should().Be(settings.DefaultWordColor);
   }

   [Fact]
   public void Numbered_Listing_Numbering_May_Start_Above_One()
   {
      var c = new Capture();
      var h = NewHighlighter();

      Handle(h, c, Header + "\n");
      Handle(h, c, "17:  return 1;\n").Should().BeTrue();
      h.FlushPending();

      c.LineText(0, 0).Should().Be("17:  return 1;");
   }

   // ----- Unnumbered listings -----

   [Fact]
   public void Unnumbered_Listing_Buffers_Contiguous_Lines_Into_One_Block()
   {
      var c = new Capture();
      var h = NewHighlighter();

      Handle(h, c, Header + "\n").Should().BeTrue();
      Handle(h, c, "player:tell(\"hi\");\n").Should().BeTrue();
      Handle(h, c, "return 1;\n").Should().BeTrue();
      c.Blocks.Should().BeEmpty();

      h.FlushPending();

      c.Blocks.Should().ContainSingle();
      c.Blocks[0].Should().HaveCount(2);
      c.LineText(0, 0).Should().Be("player:tell(\"hi\");");
      c.LineText(0, 1).Should().Be("return 1;");
   }

   [Fact]
   public void Unnumbered_Listing_Terminates_On_Blank_Line()
   {
      var c = new Capture();
      var h = NewHighlighter();

      Handle(h, c, Header + "\n");
      Handle(h, c, "player:tell(\"hi\");\n").Should().BeTrue();
      // A blank line ends the listing: flush, then render the blank normally.
      Handle(h, c, "\n").Should().BeFalse();

      c.Blocks.Should().ContainSingle();
      c.Blocks[0].Should().HaveCount(1);
   }

   // ----- Idle flush (timer stand-in) -----

   [Fact]
   public void FlushPending_Emits_Buffered_Block_Once_When_Listing_Is_Last_Output()
   {
      var c = new Capture();
      var h = NewHighlighter();

      Handle(h, c, Header + "\n");
      Handle(h, c, "player:tell(\"hi\");\n").Should().BeTrue();
      Handle(h, c, "return 1;\n").Should().BeTrue();

      h.FlushPending();
      c.Blocks.Should().ContainSingle();
      c.Blocks[0].Should().HaveCount(2);

      // A second flush is a no-op (buffer already drained).
      h.FlushPending();
      c.Blocks.Should().ContainSingle();
   }

   // ----- Multi-verb / reset -----

   [Fact]
   public void New_Header_Mid_Capture_Flushes_Block_And_Starts_New_One()
   {
      var c = new Capture();
      var h = NewHighlighter();

      Handle(h, c, Header + "\n");
      Handle(h, c, "1:  return 1;\n").Should().BeTrue();
      // A new header flushes the first block and restarts a new listing.
      Handle(h, c, "#107:\"look\"  this none this\n").Should().BeTrue();
      Handle(h, c, "1:  return 2;\n").Should().BeTrue();
      h.FlushPending();

      c.PassThrough.Should().HaveCount(2); // both headers
      c.Blocks.Should().HaveCount(2);
      c.LineText(0, 0).Should().Be("1:  return 1;");
      c.LineText(1, 0).Should().Be("1:  return 2;");
   }

   [Fact]
   public void Reset_Abandons_Buffered_Code_Without_Emitting()
   {
      var c = new Capture();
      var h = NewHighlighter();

      Handle(h, c, Header + "\n");
      Handle(h, c, "1:  return 1;\n").Should().BeTrue();
      h.Reset();

      // The buffered block is abandoned (never flushed).
      h.FlushPending();
      c.Blocks.Should().BeEmpty();

      // After reset, a numbered line is no longer treated as code.
      Handle(h, c, "1:  return 1;\n").Should().BeFalse();
      c.Blocks.Should().BeEmpty();
   }

   [Fact]
   public void Ordinary_Lines_Are_Not_Consumed_When_Idle()
   {
      var c = new Capture();
      var h = NewHighlighter();

      Handle(h, c, "You see nothing special.\n").Should().BeFalse();
      c.Blocks.Should().BeEmpty();
      c.PassThrough.Should().BeEmpty();
   }

   // ----- Injected flush scheduler -----

   [Fact]
   public void Flush_Scheduler_Is_Armed_Per_Buffered_Line_And_Cancelled_On_Flush()
   {
      var arms = 0;
      var cancels = 0;
      var c = new Capture();
      var h = NewHighlighter(armFlush: () => arms++, cancelFlush: () => cancels++);

      Handle(h, c, Header + "\n");
      Handle(h, c, "1:  a;\n").Should().BeTrue();
      Handle(h, c, "2:  b;\n").Should().BeTrue();
      arms.Should().Be(2); // armed once per buffered code line

      // Terminator flushes the block and cancels the pending flush.
      Handle(h, c, "Verb programmed.\n").Should().BeFalse();
      cancels.Should().BeGreaterThanOrEqualTo(1);
      c.Blocks.Should().ContainSingle();
   }

   [Fact]
   public void Reset_Cancels_Pending_Flush()
   {
      var cancels = 0;
      var c = new Capture();
      var h = NewHighlighter(cancelFlush: () => cancels++);

      Handle(h, c, Header + "\n");
      Handle(h, c, "1:  a;\n").Should().BeTrue();
      h.Reset();

      cancels.Should().BeGreaterThanOrEqualTo(1);
   }
}
