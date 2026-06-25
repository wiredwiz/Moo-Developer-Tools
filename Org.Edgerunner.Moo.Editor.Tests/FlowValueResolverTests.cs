using Antlr4.Runtime;
using FluentAssertions;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar;
using Org.Edgerunner.Moo.Editor;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class FlowValueResolverTests
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

   // Resolves `name`'s flow value with the caret at the absolute offset of the first occurrence of `caretMarker`.
   private static FlowValue ResolveAt(string buffer, string name, string caretMarker)
   {
      var tree = Parse(buffer);
      var offset = buffer.IndexOf(caretMarker, System.StringComparison.Ordinal);
      offset.Should().BeGreaterThanOrEqualTo(0, "the caret marker should exist in the buffer");
      return FlowValueResolver.ResolveCurrentValue(name, tree, offset);
   }

   private static FlowValue ResolveAtEnd(string buffer, string name)
   {
      var tree = Parse(buffer);
      return FlowValueResolver.ResolveCurrentValue(name, tree, buffer.Length);
   }

   [Fact]
   public void Definite_top_level_assignment_to_object_literal()
   {
      var fv = ResolveAtEnd("player = #10;\nplayer:", "player");

      fv.Kind.Should().Be(FlowValueKind.Reassigned);
      fv.Chain!.Base.Kind.Should().Be(ChainBaseKind.ObjectLiteral);
      fv.Chain.Base.Text.Should().Be("10");
      fv.Chain.Steps.Should().BeEmpty();
   }

   [Fact]
   public void Conditional_assignment_poisons_keyword_to_use_default()
   {
      // The assignment lives in an if-branch the caret (after endif) is not in.
      var fv = ResolveAtEnd("if (foo)\n  player = #20;\nendif\nplayer:", "player");

      fv.Kind.Should().Be(FlowValueKind.UseDefault);
      fv.Chain.Should().BeNull();
   }

   [Fact]
   public void Later_definite_overrides_prior_conditional()
   {
      var fv = ResolveAtEnd("if (foo)\n  player = #20;\nendif\nplayer = #10;\nplayer:", "player");

      fv.Kind.Should().Be(FlowValueKind.Reassigned);
      fv.Chain!.Base.Text.Should().Be("10");
   }

   [Fact]
   public void Caret_inside_branch_sees_definite_assignment_in_that_branch()
   {
      // Caret marker sits right after the in-branch assignment, inside the if-body.
      var buffer = "if (foo)\n  player = #20;\n  player:CARET\nendif";
      var fv = ResolveAt(buffer, "player", "CARET");

      fv.Kind.Should().Be(FlowValueKind.Reassigned);
      fv.Chain!.Base.Text.Should().Be("20");
   }

   [Fact]
   public void Unmodified_keyword_uses_default()
   {
      var fv = ResolveAtEnd("x = #1;\nplayer:", "player");

      fv.Kind.Should().Be(FlowValueKind.UseDefault);
   }

   [Fact]
   public void Unassigned_local_is_unknown()
   {
      var fv = ResolveAtEnd("y = #1;\nx:", "x");

      fv.Kind.Should().Be(FlowValueKind.Unknown);
   }

   [Fact]
   public void Ambiguous_local_is_unknown()
   {
      var fv = ResolveAtEnd("if (foo)\n  x = #5;\nendif\nx:", "x");

      fv.Kind.Should().Be(FlowValueKind.Unknown);
   }

   [Fact]
   public void Non_chain_rhs_definite_is_unknown()
   {
      var fv = ResolveAtEnd("x = this:create();\nx:", "x");

      fv.Kind.Should().Be(FlowValueKind.Unknown);
   }

   [Fact]
   public void Arithmetic_rhs_definite_is_unknown()
   {
      var fv = ResolveAtEnd("x = 1 + 2;\nx:", "x");

      fv.Kind.Should().Be(FlowValueKind.Unknown);
   }

   [Fact]
   public void Offset_correct_nested_snapshot_resolves_rhs_as_of_assignment()
   {
      // x = player at O1 must capture player's DEFAULT value, NOT the #5 assigned later at O2.
      var fv = ResolveAtEnd("x = player;\nplayer = #5;\nx:", "x");

      // x's value is player reduced as of O1 -> player is unmodified there -> keyword default Player base.
      fv.Kind.Should().Be(FlowValueKind.Reassigned);
      fv.Chain!.Base.Kind.Should().Be(ChainBaseKind.Player);
      fv.Chain.Steps.Should().BeEmpty();
   }

   [Fact]
   public void Keyword_rhs_is_reduced_as_of_the_assignment_offset()
   {
      // player = #9 (O0); x = player (O1) must capture player's value AT O1 (which is #9), NOT the
      // default player and NOT the later #5. A subsequent player = #5 must not leak back into x.
      var fv = ResolveAtEnd("player = #9;\nx = player;\nplayer = #5;\nx:", "x");

      fv.Kind.Should().Be(FlowValueKind.Reassigned);
      fv.Chain!.Base.Kind.Should().Be(ChainBaseKind.ObjectLiteral);
      fv.Chain.Base.Text.Should().Be("9");
   }

   [Fact]
   public void Unmodified_keyword_rhs_reduces_to_default_keyword_base()
   {
      // x = player where player is unmodified before O1 -> x is the keyword default (Player base),
      // which downstream falls back to the connected player. (Regression: unchanged behavior.)
      var fv = ResolveAtEnd("x = player;\nx:", "x");

      fv.Kind.Should().Be(FlowValueKind.Reassigned);
      fv.Chain!.Base.Kind.Should().Be(ChainBaseKind.Player);
      fv.Chain.Steps.Should().BeEmpty();
   }

   [Fact]
   public void Keyword_self_assignment_terminates_and_reduces_to_default()
   {
      // player = player; player: -> reducing player's RHS resolves player AS OF that assignment's offset,
      // where player is still unmodified, so it grounds to the keyword default (Player base). The recursion
      // must TERMINATE (the depth budget + reducing-set guard prevent any infinite loop) and must not
      // fabricate a bogus ground object. (Default player == player's actual value at that line.)
      var fv = ResolveAtEnd("player = player;\nplayer:", "player");

      fv.Kind.Should().Be(FlowValueKind.Reassigned);
      fv.Chain!.Base.Kind.Should().Be(ChainBaseKind.Player);
      fv.Chain.Steps.Should().BeEmpty();
   }

   [Fact]
   public void Keyword_reassigned_then_self_referenced_grounds_to_prior_value()
   {
      // player = #9; player = player; player: -> the second assignment reduces `player` as of ITS offset,
      // where player is #9 -> grounds to #9 (terminates; no infinite loop through the cycle).
      var fv = ResolveAtEnd("player = #9;\nplayer = player;\nplayer:", "player");

      fv.Kind.Should().Be(FlowValueKind.Reassigned);
      fv.Chain!.Base.Kind.Should().Be(ChainBaseKind.ObjectLiteral);
      fv.Chain.Base.Text.Should().Be("9");
   }

   [Fact]
   public void Keyword_rhs_with_property_step_grounds_as_of_offset()
   {
      // player = #9 (O0); x = player.home (O1) -> x grounds to { base #9, steps [home] } as of O1.
      var fv = ResolveAtEnd("player = #9;\nx = player.home;\nplayer = #5;\nx:", "x");

      fv.Kind.Should().Be(FlowValueKind.Reassigned);
      fv.Chain!.Base.Kind.Should().Be(ChainBaseKind.ObjectLiteral);
      fv.Chain.Base.Text.Should().Be("9");
      fv.Chain.Steps.Should().Equal("home");
   }

   [Fact]
   public void Local_assigned_to_multi_step_chain_grounds_with_steps()
   {
      var fv = ResolveAtEnd("pack = $mcp.package;\npack:", "pack");

      fv.Kind.Should().Be(FlowValueKind.Reassigned);
      fv.Chain!.Base.Kind.Should().Be(ChainBaseKind.CoreName);
      fv.Chain.Base.Text.Should().Be("mcp");
      fv.Chain.Steps.Should().Equal("package");
   }

   [Fact]
   public void Multi_hop_reduction_honors_offsets()
   {
      // a = b (O1); b = #5 (O2); a: -> a reduces to b-as-of-O1, but b is unmodified before O1 -> Unknown.
      var fv = ResolveAtEnd("a = b;\nb = #5;\na:", "a");

      fv.Kind.Should().Be(FlowValueKind.Unknown);
   }

   [Fact]
   public void Multi_hop_reduction_resolves_when_source_precedes()
   {
      // b = #5 (O1); a = b (O2); a: -> a reduces to b-as-of-O2 -> #5.
      var fv = ResolveAtEnd("b = #5;\na = b;\na:", "a");

      fv.Kind.Should().Be(FlowValueKind.Reassigned);
      fv.Chain!.Base.Kind.Should().Be(ChainBaseKind.ObjectLiteral);
      fv.Chain.Base.Text.Should().Be("5");
   }

   [Fact]
   public void Loop_back_edge_poisons_keyword_to_default()
   {
      // Caret inside the loop body; a later (post-caret) assignment runs on the back-edge.
      var buffer = "while (cond)\n  player:CARET\n  player = #5;\nendwhile";
      var fv = ResolveAt(buffer, "player", "CARET");

      fv.Kind.Should().Be(FlowValueKind.UseDefault);
   }

   [Fact]
   public void Loop_sole_dominating_assignment_is_definite()
   {
      // The only assignment in the loop body precedes the caret and dominates it.
      var buffer = "while (cond)\n  player = #5;\n  player:CARET\nendwhile";
      var fv = ResolveAt(buffer, "player", "CARET");

      fv.Kind.Should().Be(FlowValueKind.Reassigned);
      fv.Chain!.Base.Text.Should().Be("5");
   }

   [Fact]
   public void Cycle_guard_yields_unknown()
   {
      // x = y; y = x; both before a caret using x -> reduction cycles -> Unknown.
      var fv = ResolveAtEnd("y = x;\nx = y;\nx:", "x");

      fv.Kind.Should().Be(FlowValueKind.Unknown);
   }

   [Fact]
   public void Compound_assignment_poisons_with_unknown()
   {
      var fv = ResolveAtEnd("x = #5;\nx += 1;\nx:", "x");

      fv.Kind.Should().Be(FlowValueKind.Unknown);
   }

   [Fact]
   public void Compound_assignment_on_keyword_is_unknown()
   {
      // player += 1 is a DEFINITE assignment whose value is an arithmetic result (not a resolvable
      // chain), so player is definitely-unknown here, not ambiguous -> Unknown (not the keyword default).
      var fv = ResolveAtEnd("player = #5;\nplayer += 1;\nplayer:", "player");

      fv.Kind.Should().Be(FlowValueKind.Unknown);
   }

   // ---- Scatter ----

   [Fact]
   public void Scatter_direct_list_literal_resolves_positional_object()
   {
      var fv = ResolveAtEnd("{a, b} = {#1, #2};\nb:", "b");

      fv.Kind.Should().Be(FlowValueKind.Reassigned);
      fv.Chain!.Base.Kind.Should().Be(ChainBaseKind.ObjectLiteral);
      fv.Chain.Base.Text.Should().Be("2");
   }

   [Fact]
   public void Scatter_first_position_resolves()
   {
      var fv = ResolveAtEnd("{a, b} = {#1, #2};\na:", "a");

      fv.Kind.Should().Be(FlowValueKind.Reassigned);
      fv.Chain!.Base.Text.Should().Be("1");
   }

   [Fact]
   public void Scatter_indirected_rhs_resolves_object_element()
   {
      var fv = ResolveAtEnd("a = {$mcp, 5};\n{mcp, count} = a;\nmcp:", "mcp");

      fv.Kind.Should().Be(FlowValueKind.Reassigned);
      fv.Chain!.Base.Kind.Should().Be(ChainBaseKind.CoreName);
      fv.Chain.Base.Text.Should().Be("mcp");
   }

   [Fact]
   public void Scatter_indirected_non_object_element_is_unknown()
   {
      var fv = ResolveAtEnd("a = {$mcp, 5};\n{mcp, count} = a;\ncount:", "count");

      fv.Kind.Should().Be(FlowValueKind.Unknown);
   }

   [Fact]
   public void Scatter_non_object_element_is_unknown()
   {
      var fv = ResolveAtEnd("{a, b} = {5, \"x\"};\na:", "a");

      fv.Kind.Should().Be(FlowValueKind.Unknown);
   }

   [Fact]
   public void Scatter_with_splat_makes_positions_unknown()
   {
      var fv = ResolveAtEnd("{a, b} = {@stuff, #2};\nb:", "b");

      fv.Kind.Should().Be(FlowValueKind.Unknown);
   }

   [Fact]
   public void Scatter_optional_target_is_unknown()
   {
      var fv = ResolveAtEnd("{a, ?b} = {#1, #2};\nb:", "b");

      fv.Kind.Should().Be(FlowValueKind.Unknown);
   }

   [Fact]
   public void Scatter_rest_target_is_unknown()
   {
      var fv = ResolveAtEnd("{a, @rest} = {#1, #2};\nrest:", "rest");

      fv.Kind.Should().Be(FlowValueKind.Unknown);
   }

   [Fact]
   public void Scatter_target_after_optional_is_unknown()
   {
      // An optional target before a plain target shifts the plain target's position -> Unknown.
      var fv = ResolveAtEnd("{?a, b} = {#1, #2};\nb:", "b");

      fv.Kind.Should().Be(FlowValueKind.Unknown);
   }

   [Fact]
   public void Scatter_out_of_range_position_is_unknown()
   {
      var fv = ResolveAtEnd("{a, b, c} = {#1, #2};\nc:", "c");

      fv.Kind.Should().Be(FlowValueKind.Unknown);
   }

   [Fact]
   public void Scatter_non_list_literal_rhs_is_unknown()
   {
      var fv = ResolveAtEnd("{a, b} = this:stuff();\na:", "a");

      fv.Kind.Should().Be(FlowValueKind.Unknown);
   }

   [Fact]
   public void Scatter_poisons_target_even_when_value_unknown()
   {
      // First a definite #9; then a scatter binds a with a non-object element -> a is poisoned to Unknown.
      var fv = ResolveAtEnd("a = #9;\n{a, b} = {5, #2};\na:", "a");

      fv.Kind.Should().Be(FlowValueKind.Unknown);
   }
}
