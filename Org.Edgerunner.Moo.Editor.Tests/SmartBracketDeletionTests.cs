using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Controls;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class SmartBracketDeletionTests
{
   // ---- Refinement 2: whitespace-tolerant bracket delete ----

   [Fact]
   public void Bracket_AdjacentCloser_ReturnsCloserIndex()
   {
      // "(|)" caret at 1 -> closer at index 1
      SmartBracketDeletion.FindMatchingCloserAhead("()", 1, '(', ')').Should().Be(1);
   }

   [Fact]
   public void Bracket_WhitespaceThenCloser_ReturnsCloserIndex()
   {
      // "(|   )" caret at 1 -> closer at index 4 (3 spaces skipped)
      SmartBracketDeletion.FindMatchingCloserAhead("(   )", 1, '(', ')').Should().Be(4);
   }

   [Fact]
   public void Bracket_TabThenCloser_ReturnsCloserIndex()
   {
      SmartBracketDeletion.FindMatchingCloserAhead("(\t)", 1, '(', ')').Should().Be(2);
   }

   [Fact]
   public void Bracket_NonWhitespaceBeforeCloser_ReturnsMinusOne()
   {
      // "(|  x)" first non-ws is 'x' -> normal backspace
      SmartBracketDeletion.FindMatchingCloserAhead("(  x)", 1, '(', ')').Should().Be(-1);
   }

   [Fact]
   public void Bracket_OpenerNotBeforeCaret_ReturnsMinusOne()
   {
      // "x|" -> char before caret is not '('
      SmartBracketDeletion.FindMatchingCloserAhead("x", 1, '(', ')').Should().Be(-1);
   }

   [Fact]
   public void Bracket_NoCloserAhead_ReturnsMinusOne()
   {
      SmartBracketDeletion.FindMatchingCloserAhead("(   ", 1, '(', ')').Should().Be(-1);
   }

   [Fact]
   public void Bracket_SquareAndCurly_Work()
   {
      SmartBracketDeletion.FindMatchingCloserAhead("[ ]", 1, '[', ']').Should().Be(2);
      SmartBracketDeletion.FindMatchingCloserAhead("{ }", 1, '{', '}').Should().Be(2);
   }

   // ---- Refinement 3: escape-aware quote delete ----

   [Fact]
   public void Quote_AdjacentMatching_ReturnsCloserIndex()
   {
      // "\"|\"" caret at 1 -> matching quote at index 1
      SmartBracketDeletion.FindMatchingCloserAhead("\"\"", 1, '"', '"').Should().Be(1);
   }

   [Fact]
   public void Quote_WhitespaceThenMatching_ReturnsCloserIndex()
   {
      // "\"|   \"" -> matching quote at index 4
      SmartBracketDeletion.FindMatchingCloserAhead("\"   \"", 1, '"', '"').Should().Be(4);
   }

   [Fact]
   public void Quote_ContentBeforeMatching_ReturnsMinusOne()
   {
      // "\"| hello \"" first non-ws is 'h' -> only deleted quote removed
      SmartBracketDeletion.FindMatchingCloserAhead("\" hello \"", 1, '"', '"').Should().Be(-1);
   }

   [Fact]
   public void IsUnescapedQuoteDelimiter_NoBackslash_IsDelimiter()
   {
      // "\"" -> index 0, zero backslashes (even) -> delimiter
      SmartBracketDeletion.IsUnescapedQuoteDelimiter("\"", 0).Should().BeTrue();
   }

   [Fact]
   public void IsUnescapedQuoteDelimiter_SingleBackslash_IsEscaped()
   {
      // "\\\"" -> the quote at index 1 preceded by one '\' (odd) -> escaped content
      SmartBracketDeletion.IsUnescapedQuoteDelimiter("\\\"", 1).Should().BeFalse();
   }

   [Fact]
   public void IsUnescapedQuoteDelimiter_DoubleBackslash_IsDelimiter()
   {
      // "\\\\\"" -> quote at index 2 preceded by two '\' (even) -> delimiter
      SmartBracketDeletion.IsUnescapedQuoteDelimiter("\\\\\"", 2).Should().BeTrue();
   }

   [Fact]
   public void IsUnescapedQuoteDelimiter_TripleBackslash_IsEscaped()
   {
      // three '\' (odd) -> escaped content
      SmartBracketDeletion.IsUnescapedQuoteDelimiter("\\\\\\\"", 3).Should().BeFalse();
   }
}
