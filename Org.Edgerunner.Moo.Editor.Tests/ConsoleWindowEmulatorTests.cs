using System.Drawing;
using FastColoredTextBoxNS.Types;
using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Controls;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

/// <summary>
/// Unit tests for the pure block-layout seam used by
/// <see cref="ConsoleWindowEmulator.WriteStyledBlock"/>. The multi-line range math is exercised here
/// without a live FastColoredTextBox control.
/// </summary>
public class ConsoleWindowEmulatorTests
{
   private static Style NewStyle() => new TextStyle(null, null, FontStyle.Regular);

   [Fact]
   public void BuildStyledBlock_Concatenates_Lines_With_Trailing_Newline_And_Maps_Ranges()
   {
      var s1 = NewStyle();
      var s2 = NewStyle();
      var s3 = NewStyle();
      var lines = new List<IReadOnlyList<(string Text, Style Style)>>
      {
         new List<(string Text, Style Style)> { ("ab", s1), ("cd", s2) },
         new List<(string Text, Style Style)> { ("ef", s3) },
      };

      var (text, ranges) = ConsoleWindowEmulator.BuildStyledBlock(lines, 0, 0);

      text.Should().Be("abcd\nef\n");
      ranges.Should().HaveCount(3);
      ranges[0].Should().Be((0, 0, 2, s1));
      ranges[1].Should().Be((0, 2, 2, s2));
      ranges[2].Should().Be((1, 0, 2, s3));
   }

   [Fact]
   public void BuildStyledBlock_Applies_Start_Line_And_Column_To_First_Line_Only()
   {
      var s1 = NewStyle();
      var s2 = NewStyle();
      var lines = new List<IReadOnlyList<(string Text, Style Style)>>
      {
         new List<(string Text, Style Style)> { ("hi", s1) },
         new List<(string Text, Style Style)> { ("yo", s2) },
      };

      var (text, ranges) = ConsoleWindowEmulator.BuildStyledBlock(lines, 5, 3);

      text.Should().Be("hi\nyo\n");
      ranges[0].Should().Be((5, 3, 2, s1)); // first line continues at startLine/startColumn
      ranges[1].Should().Be((6, 0, 2, s2)); // subsequent lines start at column 0
   }

   [Fact]
   public void BuildStyledBlock_Strips_Bell_And_Skips_Empty_Segments()
   {
      var s1 = NewStyle();
      var withBell = "a" + (char)7 + "b"; // embedded bell should be stripped
      var lines = new List<IReadOnlyList<(string Text, Style Style)>>
      {
         new List<(string Text, Style Style)> { (withBell, s1), (string.Empty, NewStyle()) },
      };

      var (text, ranges) = ConsoleWindowEmulator.BuildStyledBlock(lines, 0, 0);

      text.Should().Be("ab\n");
      ranges.Should().ContainSingle();
      ranges[0].Should().Be((0, 0, 2, s1)); // length excludes the stripped bell; empty segment yields no range
   }
}
