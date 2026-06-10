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
}
