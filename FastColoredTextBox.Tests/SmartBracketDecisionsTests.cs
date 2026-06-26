using FastColoredTextBoxNS;
using FluentAssertions;
using Xunit;

namespace FastColoredTextBox.Tests;

public class SmartBracketDecisionsTests
{
   [Fact]
   public void NoCloserAhead_DoesNotSuppress()
   {
      // "foo|" type '(' -> pair should be inserted
      SmartBracketDecisions.HasUnmatchedCloserAhead("foo", 3, '(', ')').Should().BeFalse();
   }

   [Fact]
   public void UnmatchedCloserImmediatelyAhead_Suppresses()
   {
      // "|)" type '(' -> only '(' inserted
      SmartBracketDecisions.HasUnmatchedCloserAhead(")", 0, '(', ')').Should().BeTrue();
   }

   [Fact]
   public void UnmatchedCloserAfterContent_Suppresses()
   {
      // "(a, |)" type '(' -> ')' ahead is unmatched -> only '(' inserted
      SmartBracketDecisions.HasUnmatchedCloserAhead("(a, )", 4, '(', ')').Should().BeTrue();
   }

   [Fact]
   public void BalancedPairAhead_DoesNotSuppress()
   {
      // "|()" -> the closer ahead is matched by its own opener, depth never negative
      SmartBracketDecisions.HasUnmatchedCloserAhead("()", 0, '(', ')').Should().BeFalse();
   }

   [Fact]
   public void EmptyLine_DoesNotSuppress()
   {
      SmartBracketDecisions.HasUnmatchedCloserAhead("", 0, '(', ')').Should().BeFalse();
   }

   [Fact]
   public void CaretBeyondTheCloser_DoesNotSuppress()
   {
      // ")|" caret after the closer -> nothing ahead
      SmartBracketDecisions.HasUnmatchedCloserAhead(")", 1, '(', ')').Should().BeFalse();
   }

   [Fact]
   public void WorksForSquareBrackets()
   {
      SmartBracketDecisions.HasUnmatchedCloserAhead("a, ]", 3, '[', ']').Should().BeTrue();
   }

   [Fact]
   public void WorksForCurlyBraces_UnmatchedAhead_Suppresses()
   {
      SmartBracketDecisions.HasUnmatchedCloserAhead("x}", 0, '{', '}').Should().BeTrue();
   }

   [Fact]
   public void WorksForCurlyBraces_Balanced_DoesNotSuppress()
   {
      SmartBracketDecisions.HasUnmatchedCloserAhead("{}", 0, '{', '}').Should().BeFalse();
   }
}
