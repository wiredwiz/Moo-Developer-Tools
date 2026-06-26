using System.Linq;
using Antlr4.Runtime;
using FluentAssertions;
using FastColoredTextBoxNS.Types;
using Org.Edgerunner.Moo.Editor;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class VariableReferenceCollectorTests
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

   // Collects with no caret exclusion (caret offset -1).
   private static System.Collections.Generic.IReadOnlyCollection<string> Collect(string buffer)
   {
      return VariableReferenceCollector.CollectVariableNames(Parse(buffer), -1);
   }

   // Collects with the caret at the absolute offset of the first occurrence of `caretMarker`.
   private static System.Collections.Generic.IReadOnlyCollection<string> CollectAt(string buffer, string caretMarker)
   {
      var tree = Parse(buffer);
      var offset = buffer.IndexOf(caretMarker, System.StringComparison.Ordinal);
      offset.Should().BeGreaterThanOrEqualTo(0, "the caret marker should exist in the buffer");
      return VariableReferenceCollector.CollectVariableNames(tree, offset);
   }

   [Fact]
   public void Null_tree_yields_empty()
   {
      VariableReferenceCollector.CollectVariableNames(null, 0).Should().BeEmpty();
   }

   [Fact]
   public void Plain_assignment_target_is_collected()
   {
      Collect("foo = 1;").Should().Contain("foo");
   }

   [Fact]
   public void Compound_assignment_target_is_collected()
   {
      Collect("counter = 0;\ncounter += 1;").Should().Contain("counter");
   }

   [Fact]
   public void Scatter_assignment_targets_are_collected()
   {
      var names = Collect("{alpha, beta, gamma} = source;");

      names.Should().Contain(new[] { "alpha", "beta", "gamma", "source" });
   }

   [Fact]
   public void Scatter_optional_and_rest_targets_are_collected()
   {
      var names = Collect("{first, ?second, @rest} = source;");

      names.Should().Contain(new[] { "first", "second", "rest" });
   }

   [Fact]
   public void For_loop_variable_is_collected()
   {
      var names = Collect("for item in (collection)\n  x = item;\nendfor");

      names.Should().Contain(new[] { "item", "collection", "x" });
   }

   [Fact]
   public void Except_error_variable_is_collected()
   {
      var names = Collect("try\n  z = 1;\nexcept err (ANY)\n  w = err;\nendtry");

      names.Should().Contain(new[] { "err", "z", "w" });
   }

   [Fact]
   public void Bare_read_reference_with_no_assignment_is_collected()
   {
      // `orphan` is never assigned, only read — it still counts (flat lexical harvest).
      Collect("x = orphan;").Should().Contain("orphan");
   }

   [Fact]
   public void Chain_base_identifier_is_collected_but_member_name_is_excluded()
   {
      // `widget` is a variable (chain base); `height` is a property name (right of '.') and excluded.
      var names = Collect("y = widget.height;");

      names.Should().Contain("widget");
      names.Should().NotContain("height");
   }

   [Fact]
   public void Verb_call_member_name_is_excluded()
   {
      // `target` is the chain base variable; `frobnicate` is the verb name (right of ':') and excluded.
      var names = Collect("z = target:frobnicate(arg);");

      names.Should().Contain(new[] { "target", "arg" });
      names.Should().NotContain("frobnicate");
   }

   [Fact]
   public void Builtin_function_call_name_is_excluded()
   {
      // `notify` is the function name (a bare call terminal), not a variable; only `player` (an arg).
      var names = Collect("notify(recipient, \"hi\");");

      names.Should().Contain("recipient");
      names.Should().NotContain("notify");
   }

   [Fact]
   public void Name_used_as_member_elsewhere_is_still_collected_as_variable()
   {
      // `count` appears once as a property name (excluded there) and once as a variable target (kept).
      var names = Collect("a = obj.count;\ncount = 5;");

      names.Should().Contain("count");
   }

   [Fact]
   public void Names_are_deduplicated_case_insensitively_preserving_first_casing()
   {
      var names = Collect("Total = 1;\ntotal = Total + total;").ToList();

      names.Count(n => string.Equals(n, "total", System.StringComparison.OrdinalIgnoreCase)).Should().Be(1);
      names.Should().Contain("Total");
   }

   [Fact]
   public void Caret_occurrence_dropped_when_name_used_nowhere_else()
   {
      // `newName` exists only at the caret (the in-progress identifier) -> dropped entirely.
      var names = CollectAt("newName", "newName");

      names.Should().NotContain("newName");
   }

   [Fact]
   public void Caret_occurrence_dropped_but_name_kept_when_used_elsewhere()
   {
      // `count` is assigned earlier and is being re-typed at CARET — the typed occurrence drops, the
      // earlier one keeps the name.
      var buffer = "count = 0;\nx = countCARET";
      var names = CollectAt(buffer, "countCARET");

      names.Should().Contain("count");
   }

   // ---- Asymmetric dedup (wiring: DynamicCompletionSource.BuildVariableItems) ----

   [Fact]
   public void BuildVariableItems_suppresses_names_matching_keywords()
   {
      // `player` is a built-in variable (Moo.Keywords) — suppressed; `myLocal` survives.
      var items = DynamicCompletionSource.BuildVariableItems(new[] { "player", "myLocal" });

      items.Select(i => i.ToString()).Should().NotContain("player");
      items.Select(i => i.ToString()).Should().Contain("myLocal");
   }

   [Fact]
   public void BuildVariableItems_keeps_names_matching_builtin_functions()
   {
      // `read` is a built-in function (Moo.Builtins) — the variable entry is still emitted (both appear).
      var items = DynamicCompletionSource.BuildVariableItems(new[] { "read" });

      items.Should().ContainSingle();
      items[0].ToString().Should().Be("read");
      items[0].ImageIndex.Should().Be((int)CompletionIconCategory.Variable);
   }

   [Fact]
   public void BuildVariableItems_wraps_survivors_with_variable_icon()
   {
      var items = DynamicCompletionSource.BuildVariableItems(new[] { "alpha", "beta" });

      items.Should().OnlyContain(i => i.ImageIndex == (int)CompletionIconCategory.Variable);
   }
}
