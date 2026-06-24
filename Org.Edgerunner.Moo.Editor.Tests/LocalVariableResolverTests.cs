using System.Linq;
using Antlr4.Runtime;
using FluentAssertions;
using Org.Edgerunner.Moo.Editor;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class LocalVariableResolverTests
{
   private static ParserRuleContext Parse(string buffer)
   {
      var inputStream = new AntlrInputStream(buffer);
      var lexer = Org.Edgerunner.Moo.Editor.Moo.GetLexer(GrammarDialect.Edgerunner, inputStream);
      var stream = new CommonTokenStream(lexer);
      var parser = Org.Edgerunner.Moo.Editor.Moo.GetParser(GrammarDialect.Edgerunner, stream);
      parser.RemoveErrorListeners();
      return (ParserRuleContext)parser.GetType().GetMethod("code")!.Invoke(parser, null)!;
   }

   // Resolves a variable's RHS chain, treating the caret as sitting at the very end of the buffer.
   private static ChainDescriptor? Resolve(string buffer, string variable)
   {
      var tree = Parse(buffer);
      return LocalVariableResolver.ResolveAssignmentChain(variable, tree, caretOffset: buffer.Length);
   }

   [Fact]
   public void Resolves_top_level_assignment_to_core_reference()
   {
      var rhs = Resolve("x = $foo;\nx.bar:", "x");

      rhs.Should().NotBeNull();
      rhs!.Base.Kind.Should().Be(ChainBaseKind.CoreName);
      rhs.Base.Text.Should().Be("foo");
      rhs.Steps.Should().BeEmpty();
   }

   [Fact]
   public void Resolves_rhs_chain_expression()
   {
      var rhs = Resolve("x = $foo.bar;\nx:", "x");

      rhs.Should().NotBeNull();
      rhs!.Base.Kind.Should().Be(ChainBaseKind.CoreName);
      rhs.Base.Text.Should().Be("foo");
      rhs.Steps.Should().Equal("bar");
   }

   [Fact]
   public void Resolves_object_literal_rhs()
   {
      var rhs = Resolve("o = #42;\no:", "o");

      rhs.Should().NotBeNull();
      rhs!.Base.Kind.Should().Be(ChainBaseKind.ObjectLiteral);
      rhs.Base.Text.Should().Be("42");
   }

   [Fact]
   public void Resolves_variable_to_variable_assignment()
   {
      var rhs = Resolve("y = $foo;\nx = y;\nx:", "x");

      rhs.Should().NotBeNull();
      rhs!.Base.Kind.Should().Be(ChainBaseKind.Variable);
      rhs.Base.Text.Should().Be("y");
   }

   [Fact]
   public void Takes_the_nearest_preceding_assignment()
   {
      // Two assignments to x; the nearest preceding the caret wins.
      var rhs = Resolve("x = $foo;\nx = #7;\nx:", "x");

      rhs.Should().NotBeNull();
      rhs!.Base.Kind.Should().Be(ChainBaseKind.ObjectLiteral);
      rhs.Base.Text.Should().Be("7");
   }

   [Fact]
   public void Finds_assignment_nested_in_if_block()
   {
      var buffer = "if (1)\n  x = $foo;\nendif\nx.bar:";
      var rhs = Resolve(buffer, "x");

      rhs.Should().NotBeNull();
      rhs!.Base.Kind.Should().Be(ChainBaseKind.CoreName);
      rhs.Base.Text.Should().Be("foo");
   }

   [Fact]
   public void Finds_assignment_nested_in_for_block()
   {
      var buffer = "for o in (things)\n  x = #5;\nendfor\nx:";
      var rhs = Resolve(buffer, "x");

      rhs.Should().NotBeNull();
      rhs!.Base.Kind.Should().Be(ChainBaseKind.ObjectLiteral);
      rhs.Base.Text.Should().Be("5");
   }

   [Fact]
   public void Ignores_assignment_that_follows_the_caret()
   {
      // Caret is placed right after the first "x:" use; the later x = #9 must be ignored.
      var buffer = "x = $foo;\nx:";
      var rhs = LocalVariableResolver.ResolveAssignmentChain(
         "x", Parse(buffer + "\nx = #9;"), caretOffset: buffer.Length);

      rhs.Should().NotBeNull();
      rhs!.Base.Kind.Should().Be(ChainBaseKind.CoreName);
      rhs.Base.Text.Should().Be("foo");
   }

   [Fact]
   public void Returns_null_for_non_chain_rhs_verb_call()
   {
      var rhs = Resolve("x = this:create();\nx:", "x");

      rhs.Should().BeNull();
   }

   [Fact]
   public void Returns_null_for_non_chain_rhs_function_call()
   {
      var rhs = Resolve("x = create(#1);\nx:", "x");

      rhs.Should().BeNull();
   }

   [Fact]
   public void Returns_null_for_arithmetic_rhs()
   {
      var rhs = Resolve("x = 1 + 2;\nx:", "x");

      rhs.Should().BeNull();
   }

   [Fact]
   public void Returns_null_for_list_literal_rhs()
   {
      var rhs = Resolve("x = {1, 2, 3};\nx:", "x");

      rhs.Should().BeNull();
   }

   [Fact]
   public void Returns_null_when_variable_never_assigned()
   {
      var rhs = Resolve("y = $foo;\nx:", "x");

      rhs.Should().BeNull();
   }
}
