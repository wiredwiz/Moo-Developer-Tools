using FluentAssertions;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class BuiltinConstantDocsTests
{
   [Theory]
   [InlineData("INT", "INT => 0")]
   [InlineData("NUM", "NUM => 0")]
   [InlineData("OBJ", "OBJ => 1")]
   [InlineData("STR", "STR => 2")]
   [InlineData("ERR", "ERR => 3")]
   [InlineData("LIST", "LIST => 4")]
   [InlineData("FLOAT", "FLOAT => 9")]
   [InlineData("MAP", "MAP => 10")]
   [InlineData("ANON", "ANON => 12")]
   [InlineData("WAIF", "WAIF => 13")]
   [InlineData("BOOL", "BOOL => 14")]
   public void GetTooltipText_returns_typeof_code_for_type_constants(string name, string expected)
   {
      BuiltinConstantDocs.GetTooltipText(name).Should().Be(expected);
   }

   [Theory]
   [InlineData("E_NONE", "E_NONE => No error")]
   [InlineData("E_TYPE", "E_TYPE => Type mismatch")]
   [InlineData("E_DIV", "E_DIV => Division by zero")]
   [InlineData("E_PERM", "E_PERM => Permission denied")]
   [InlineData("E_PROPNF", "E_PROPNF => Property not found")]
   [InlineData("E_VERBNF", "E_VERBNF => Verb not found")]
   [InlineData("E_VARNF", "E_VARNF => Variable not found")]
   [InlineData("E_INVIND", "E_INVIND => Invalid indirection")]
   [InlineData("E_RECMOVE", "E_RECMOVE => Recursive move")]
   [InlineData("E_MAXREC", "E_MAXREC => Too many verb calls")]
   [InlineData("E_RANGE", "E_RANGE => Range error")]
   [InlineData("E_ARGS", "E_ARGS => Incorrect number of arguments")]
   [InlineData("E_NACC", "E_NACC => Move refused by destination")]
   [InlineData("E_INVARG", "E_INVARG => Invalid argument")]
   [InlineData("E_QUOTA", "E_QUOTA => Resource limit exceeded")]
   [InlineData("E_FLOAT", "E_FLOAT => Floating-point arithmetic error")]
   [InlineData("E_FILE", "E_FILE => File system error")]
   [InlineData("E_EXEC", "E_EXEC => Exec error")]
   [InlineData("E_INTRPT", "E_INTRPT => Interrupted")]
   public void GetTooltipText_returns_message_for_error_constants(string name, string expected)
   {
      BuiltinConstantDocs.GetTooltipText(name).Should().Be(expected);
   }

   [Theory]
   [InlineData("true", "true => true")]
   [InlineData("false", "false => false")]
   public void GetTooltipText_returns_literal_for_bool_constants(string name, string expected)
   {
      BuiltinConstantDocs.GetTooltipText(name).Should().Be(expected);
   }

   [Theory]
   [InlineData("not_a_constant")]
   [InlineData("E_BOGUS")]
   [InlineData("")]
   [InlineData(null)]
   public void GetTooltipText_returns_null_for_unknown(string name)
   {
      BuiltinConstantDocs.GetTooltipText(name).Should().BeNull();
   }

   [Theory]
   [InlineData("INT", "type", "0")]
   [InlineData("E_PERM", "error", "Permission denied")]
   [InlineData("true", "bool", "true")]
   public void Get_returns_kind_and_display(string name, string kind, string display)
   {
      var doc = BuiltinConstantDocs.Get(name);
      doc.Should().NotBeNull();
      doc!.Kind.Should().Be(kind);
      doc.Display.Should().Be(display);
   }
}

public class MooIsConstantTests
{
   [Theory]
   [InlineData("INT")]
   [InlineData("NUM")]
   [InlineData("BOOL")]
   [InlineData("E_PERM")]
   [InlineData("E_INTRPT")]
   [InlineData("true")]
   [InlineData("false")]
   public void IsConstant_true_for_constants(string name)
   {
      Moo.IsConstant(name).Should().BeTrue();
   }

   [Theory]
   [InlineData("player")]   // built-in variable, not a constant
   [InlineData("if")]       // keyword
   [InlineData("typeof")]   // built-in function
   [InlineData("foobar")]
   [InlineData("")]
   [InlineData(null)]
   public void IsConstant_false_for_non_constants(string name)
   {
      Moo.IsConstant(name).Should().BeFalse();
   }
}

public class ConstantHoverResolverTests
{
   [Theory]
   [InlineData("type", "5", "0", "5")]            // live value wins
   [InlineData("error", "Live msg", "Baked", "Live msg")]
   public void ResolveConstantDisplay_prefers_live_value(string kind, string live, string baked, string expected)
   {
      ConstantHoverResolver.ResolveConstantDisplay(kind, live, baked).Should().Be(expected);
   }

   [Theory]
   [InlineData("type", null, "0", "0")]           // null live -> baked
   [InlineData("type", "", "0", "0")]             // empty live -> baked
   [InlineData("error", null, "Type mismatch", "Type mismatch")]
   public void ResolveConstantDisplay_falls_back_to_baked(string kind, string live, string baked, string expected)
   {
      ConstantHoverResolver.ResolveConstantDisplay(kind, live, baked).Should().Be(expected);
   }

   [Theory]
   [InlineData("bool", "anything", "true", "true")]   // booleans always use the baked literal
   [InlineData("bool", null, "false", "false")]
   public void ResolveConstantDisplay_bool_uses_baked(string kind, string live, string baked, string expected)
   {
      ConstantHoverResolver.ResolveConstantDisplay(kind, live, baked).Should().Be(expected);
   }

   [Fact]
   public void ResolveConstantDisplay_null_both_returns_null()
   {
      ConstantHoverResolver.ResolveConstantDisplay("type", null, null).Should().BeNull();
   }
}
