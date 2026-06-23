using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Org.Edgerunner.Mud.Common.Querying;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class MemberOperandResolverTests
{
   [Fact]
   public void Resolve_core_reference_returns_object_zero()
   {
      var context = new MemberCompletionContext(MemberContextKind.CoreReference, string.Empty);

      var result = MemberOperandResolver.Resolve(context, null);

      result.Should().Be(new MooObjectId(0));
   }

   [Theory]
   [InlineData("#123", 123)]
   [InlineData("#0", 0)]
   [InlineData("#-1", -1)]
   public void Resolve_object_literal_returns_its_number(string operand, int expected)
   {
      var context = new MemberCompletionContext(MemberContextKind.Verb, operand);

      var result = MemberOperandResolver.Resolve(context, null);

      result.Should().Be(new MooObjectId(expected));
   }

   [Fact]
   public void Resolve_this_returns_the_context_object()
   {
      var context = new MemberCompletionContext(MemberContextKind.Property, "this");

      var result = MemberOperandResolver.Resolve(context, new MooObjectId(42));

      result.Should().Be(new MooObjectId(42));
   }

   [Fact]
   public void Resolve_this_returns_null_without_a_context_object()
   {
      var context = new MemberCompletionContext(MemberContextKind.Verb, "this");

      var result = MemberOperandResolver.Resolve(context, null);

      result.Should().BeNull();
   }

   [Theory]
   [InlineData("me")]       // deferred: no player-id source yet
   [InlineData("player")]   // deferred: no player-id source yet
   [InlineData("foo")]      // bareword
   [InlineData("$foo")]     // core-ref operand (value unknown client-side)
   public void Resolve_unresolvable_operands_return_null(string operand)
   {
      var context = new MemberCompletionContext(MemberContextKind.Verb, operand);

      var result = MemberOperandResolver.Resolve(context, new MooObjectId(42));

      result.Should().BeNull();
   }

   [Fact]
   public void Resolve_none_context_returns_null()
   {
      var result = MemberOperandResolver.Resolve(MemberCompletionContext.None, new MooObjectId(42));

      result.Should().BeNull();
   }

   // TryGetCoreName tests

   [Theory]
   [InlineData(MemberContextKind.Verb, "$network", "network")]
   [InlineData(MemberContextKind.Property, "$network", "network")]
   [InlineData(MemberContextKind.Verb, "$_private", "_private")]
   [InlineData(MemberContextKind.Verb, "$a", "a")]
   public void TryGetCoreName_returns_true_for_dollar_identifier_in_verb_or_property_context(
      MemberContextKind kind, string operand, string expectedName)
   {
      var context = new MemberCompletionContext(kind, operand);

      var result = MemberOperandResolver.TryGetCoreName(context, out var name);

      result.Should().BeTrue();
      name.Should().Be(expectedName);
   }

   [Theory]
   [InlineData(MemberContextKind.Verb, "this")]
   [InlineData(MemberContextKind.Verb, "#5")]
   [InlineData(MemberContextKind.Verb, "foo")]
   [InlineData(MemberContextKind.CoreReference, "")]
   [InlineData(MemberContextKind.None, "")]
   public void TryGetCoreName_returns_false_for_non_core_name_operands(MemberContextKind kind, string operand)
   {
      var context = new MemberCompletionContext(kind, operand);

      var result = MemberOperandResolver.TryGetCoreName(context, out var name);

      result.Should().BeFalse();
      name.Should().BeEmpty();
   }

   // TryGetCurrentPlayerOperand tests

   [Theory]
   [InlineData(MemberContextKind.Verb, "player")]
   [InlineData(MemberContextKind.Verb, "caller")]
   [InlineData(MemberContextKind.Property, "player")]
   [InlineData(MemberContextKind.Property, "caller")]
   public void TryGetCurrentPlayerOperand_returns_true_for_player_or_caller_in_member_context(
      MemberContextKind kind, string operand)
   {
      var context = new MemberCompletionContext(kind, operand);

      MemberOperandResolver.TryGetCurrentPlayerOperand(context).Should().BeTrue();
   }

   [Theory]
   [InlineData(MemberContextKind.Verb, "this")]
   [InlineData(MemberContextKind.Verb, "me")]
   [InlineData(MemberContextKind.Verb, "foo")]
   [InlineData(MemberContextKind.Verb, "$player")]
   [InlineData(MemberContextKind.Verb, "#5")]
   [InlineData(MemberContextKind.CoreReference, "player")]
   [InlineData(MemberContextKind.None, "player")]
   public void TryGetCurrentPlayerOperand_returns_false_otherwise(MemberContextKind kind, string operand)
   {
      var context = new MemberCompletionContext(kind, operand);

      MemberOperandResolver.TryGetCurrentPlayerOperand(context).Should().BeFalse();
   }

   [Fact]
   public void Resolve_player_and_caller_still_return_null_for_async_resolution()
   {
      var playerContext = new MemberCompletionContext(MemberContextKind.Verb, "player");
      var callerContext = new MemberCompletionContext(MemberContextKind.Property, "caller");

      MemberOperandResolver.Resolve(playerContext, new MooObjectId(42)).Should().BeNull();
      MemberOperandResolver.Resolve(callerContext, new MooObjectId(42)).Should().BeNull();
   }

   [Fact]
   public void TryGetCoreName_returns_false_for_dollar_with_leading_digit()
   {
      // $1abc is not a valid identifier so TryGetCoreName must reject it
      var context = new MemberCompletionContext(MemberContextKind.Verb, "$1abc");

      var result = MemberOperandResolver.TryGetCoreName(context, out var name);

      result.Should().BeFalse();
      name.Should().BeEmpty();
   }
}
