using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class MemberCompletionContextDetectorTests
{
   [Theory]
   [InlineData("$", "")]
   [InlineData("$fo", "")]
   [InlineData("x = $roo", "")]
   public void Detect_classifies_core_reference_context(string linePrefix, string expectedOperand)
   {
      var context = MemberCompletionContextDetector.Detect(linePrefix);

      context.Kind.Should().Be(MemberContextKind.CoreReference);
      context.Operand.Should().Be(expectedOperand);
   }

   [Theory]
   [InlineData("this:", "this")]
   [InlineData("this:te", "this")]
   [InlineData("#123:tell", "#123")]
   [InlineData("x = obj:fr", "obj")]
   [InlineData("$foo:bar", "$foo")]
   public void Detect_classifies_verb_context(string linePrefix, string expectedOperand)
   {
      var context = MemberCompletionContextDetector.Detect(linePrefix);

      context.Kind.Should().Be(MemberContextKind.Verb);
      context.Operand.Should().Be(expectedOperand);
   }

   [Theory]
   [InlineData("this.", "this")]
   [InlineData("this.loc", "this")]
   [InlineData("#0.na", "#0")]
   [InlineData("foo.bar", "foo")]
   [InlineData("this.location.na", "location")]
   public void Detect_classifies_property_context(string linePrefix, string expectedOperand)
   {
      var context = MemberCompletionContextDetector.Detect(linePrefix);

      context.Kind.Should().Be(MemberContextKind.Property);
      context.Operand.Should().Be(expectedOperand);
   }

   [Theory]
   [InlineData("")]
   [InlineData("x = 5")]
   [InlineData("for x in [1..5")]    // range operator, not property access
   [InlineData("1.5")]               // float literal
   [InlineData("x = \"a string with $foo")]   // inside a string
   [InlineData("x = \"obj:verb")]              // inside a string
   [InlineData("player:tell(\"this.")]         // inside a string argument
   public void Detect_returns_none_for_non_member_contexts(string linePrefix)
   {
      var context = MemberCompletionContextDetector.Detect(linePrefix);

      context.Kind.Should().Be(MemberContextKind.None);
   }

   [Fact]
   public void Detect_treats_escaped_quotes_as_string_content()
   {
      // The string "say \"hi\" to" is still open: the $ is inside it.
      var context = MemberCompletionContextDetector.Detect("x = \"say \\\"hi\\\" to $fo");

      context.Kind.Should().Be(MemberContextKind.None);
   }

   [Fact]
   public void Detect_recognizes_member_context_after_closed_string()
   {
      var context = MemberCompletionContextDetector.Detect("x = \"done\"; this:te");

      context.Kind.Should().Be(MemberContextKind.Verb);
      context.Operand.Should().Be("this");
   }
}
