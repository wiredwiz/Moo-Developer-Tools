using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using FluentAssertions;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar;
using Org.Edgerunner.Moo.Editor;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class ChainExtractorTests
{
   // Parses a buffer the way the editor does and returns the parse tree plus the detailed token list.
   private static (ParserRuleContext Tree, IList<DetailedToken> Tokens) Parse(string buffer)
   {
      var inputStream = new AntlrInputStream(buffer);
      var lexer = Org.Edgerunner.Moo.Editor.Moo.GetLexer(GrammarDialect.Edgerunner, inputStream);
      lexer.TokenFactory = DetailedTokenFactory.Instance;
      var stream = new CommonTokenStream(lexer);
      stream.Fill();
      var tokens = stream.GetTokens().Cast<DetailedToken>().ToList();
      var parser = Org.Edgerunner.Moo.Editor.Moo.GetParser(GrammarDialect.Edgerunner, stream);
      parser.RemoveErrorListeners();
      var tree = (ParserRuleContext)parser.GetType().GetMethod("code")!.Invoke(parser, null)!;
      return (tree, tokens);
   }

   // Caret position: 1-based line, 1-based column matching the editor's iChar+1 convention.
   // We express the buffer up to the caret so the caret sits at end-of-buffer for these tests.
   private static ChainDescriptor? Extract(string buffer)
   {
      var (tree, tokens) = Parse(buffer);
      // Caret at end of buffer.
      var lines = buffer.Split('\n');
      var caretLine = lines.Length;             // 1-based
      var caretColumn = lines[^1].Length + 1;   // 1-based, one past last char
      return ChainExtractor.Extract(buffer, caretLine, caretColumn, tokens, tree);
   }

   [Fact]
   public void Extract_resolves_core_property_chain_for_verb_completion()
   {
      var descriptor = Extract("$Mcp.package:");

      descriptor.Should().NotBeNull();
      descriptor!.MemberKind.Should().Be(MemberContextKind.Verb);
      descriptor.Base.Kind.Should().Be(ChainBaseKind.CoreName);
      descriptor.Base.Text.Should().Be("Mcp");
      descriptor.Steps.Should().Equal("package");
      descriptor.PartialFragment.Should().BeEmpty();
   }

   [Fact]
   public void Extract_resolves_deep_object_literal_chain_for_property_completion()
   {
      var descriptor = Extract("#10.owner.name.");

      descriptor.Should().NotBeNull();
      descriptor!.MemberKind.Should().Be(MemberContextKind.Property);
      descriptor.Base.Kind.Should().Be(ChainBaseKind.ObjectLiteral);
      descriptor.Base.Text.Should().Be("10");
      descriptor.Steps.Should().Equal("owner", "name");
      descriptor.PartialFragment.Should().BeEmpty();
   }

   [Fact]
   public void Extract_resolves_variable_chain_for_verb_completion()
   {
      var descriptor = Extract("x.bar:");

      descriptor.Should().NotBeNull();
      descriptor!.MemberKind.Should().Be(MemberContextKind.Verb);
      descriptor.Base.Kind.Should().Be(ChainBaseKind.Variable);
      descriptor.Base.Text.Should().Be("x");
      descriptor.Steps.Should().Equal("bar");
   }

   [Fact]
   public void Extract_handles_dangling_dot_with_token_fallback()
   {
      var descriptor = Extract("$Mcp.");

      descriptor.Should().NotBeNull();
      descriptor!.MemberKind.Should().Be(MemberContextKind.Property);
      descriptor.Base.Kind.Should().Be(ChainBaseKind.CoreName);
      descriptor.Base.Text.Should().Be("Mcp");
      descriptor.Steps.Should().BeEmpty();
      descriptor.PartialFragment.Should().BeEmpty();
   }

   [Fact]
   public void Extract_captures_partial_member_fragment()
   {
      var descriptor = Extract("$Mcp.package:fo");

      descriptor.Should().NotBeNull();
      descriptor!.MemberKind.Should().Be(MemberContextKind.Verb);
      descriptor.Base.Text.Should().Be("Mcp");
      descriptor.Steps.Should().Equal("package");
      descriptor.PartialFragment.Should().Be("fo");
   }

   [Fact]
   public void Extract_single_atom_this_is_a_length_one_chain()
   {
      var descriptor = Extract("this:");

      descriptor.Should().NotBeNull();
      descriptor!.MemberKind.Should().Be(MemberContextKind.Verb);
      descriptor.Base.Kind.Should().Be(ChainBaseKind.This);
      descriptor.Steps.Should().BeEmpty();
   }

   [Theory]
   [InlineData("player.", ChainBaseKind.Player)]
   [InlineData("caller:", ChainBaseKind.Caller)]
   public void Extract_recognizes_player_and_caller_bases(string buffer, ChainBaseKind expectedKind)
   {
      var descriptor = Extract(buffer);

      descriptor.Should().NotBeNull();
      descriptor!.Base.Kind.Should().Be(expectedKind);
      descriptor.Steps.Should().BeEmpty();
   }

   [Fact]
   public void Extract_single_atom_object_literal_is_a_length_one_chain()
   {
      var descriptor = Extract("#123.");

      descriptor.Should().NotBeNull();
      descriptor!.MemberKind.Should().Be(MemberContextKind.Property);
      descriptor.Base.Kind.Should().Be(ChainBaseKind.ObjectLiteral);
      descriptor.Base.Text.Should().Be("123");
      descriptor.Steps.Should().BeEmpty();
   }

   [Fact]
   public void Extract_stops_at_operator_boundary()
   {
      // foo() + $bar.baz: -> the chain base is $bar, NOT the whole left operand.
      var descriptor = Extract("foo() + $bar.baz:");

      descriptor.Should().NotBeNull();
      descriptor!.Base.Kind.Should().Be(ChainBaseKind.CoreName);
      descriptor.Base.Text.Should().Be("bar");
      descriptor.Steps.Should().Equal("baz");
   }

   [Fact]
   public void Extract_returns_null_when_not_a_member_context()
   {
      Extract("x = 5").Should().BeNull();
   }

   [Fact]
   public void Extract_at_start_of_buffer_beginning_with_a_newline_is_not_an_internal_error()
   {
      // A file starting with a blank line, caret at line 1 column 1 (absolute offset 0). This is a
      // normal caret position, so it must yield no source WITHOUT raising an (logged) internal error.
      var (tree, tokens) = Parse("\n$Mcp.package:");
      var diagnosed = false;

      var result = ChainExtractor.Extract(
         "\n$Mcp.package:", caretLine: 1, caretColumn: 1, tokens, tree,
         diagnostic: (_, _) => diagnosed = true);

      result.Should().BeNull("the caret is on an empty first line, not a member position");
      diagnosed.Should().BeFalse("an empty-line caret is an expected non-resolution, not an internal error");
   }

   [Fact]
   public void Extract_returns_null_inside_a_string_literal()
   {
      // The separator sits inside an open string literal, so it is not a member position.
      Extract("\"foo.bar:").Should().BeNull();
   }

   // Line-prefix-only entry point (no parse tree) must subsume the legacy single-atom detector.
   [Theory]
   [InlineData("this:te", ChainBaseKind.This, MemberContextKind.Verb, "te")]
   [InlineData("#123.na", ChainBaseKind.ObjectLiteral, MemberContextKind.Property, "na")]
   [InlineData("player.", ChainBaseKind.Player, MemberContextKind.Property, "")]
   public void ExtractFromLinePrefix_produces_single_atom_chains(
      string linePrefix, ChainBaseKind expectedBase, MemberContextKind expectedKind, string expectedFragment)
   {
      var descriptor = ChainExtractor.ExtractFromLinePrefix(linePrefix);

      descriptor.Should().NotBeNull();
      descriptor!.Base.Kind.Should().Be(expectedBase);
      descriptor.MemberKind.Should().Be(expectedKind);
      descriptor.Steps.Should().BeEmpty();
      descriptor.PartialFragment.Should().Be(expectedFragment);
   }

   [Fact]
   public void ExtractFromLinePrefix_core_reference_fragment_maps_to_core_reference_kind()
   {
      var descriptor = ChainExtractor.ExtractFromLinePrefix("$ro");

      descriptor.Should().NotBeNull();
      descriptor!.MemberKind.Should().Be(MemberContextKind.CoreReference);
      descriptor.Base.Kind.Should().Be(ChainBaseKind.ObjectLiteral);
      descriptor.Base.Text.Should().Be("0");
      descriptor.PartialFragment.Should().Be("ro");
   }

   [Fact]
   public void ExtractFromLinePrefix_returns_null_for_non_member_context()
   {
      ChainExtractor.ExtractFromLinePrefix("x = 5").Should().BeNull();
   }
}
