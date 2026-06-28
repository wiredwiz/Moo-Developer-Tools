using System.Drawing;
using FluentAssertions;
using Org.Edgerunner.Moo.Editor;
using Org.Edgerunner.Moo.Editor.Configuration;
using Org.Edgerunner.Moo.Editor.SyntaxHighlighting;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class MooCodeColorizerTests
{
   private const GrammarDialect Dialect = GrammarDialect.Edgerunner;

   /// <summary>
   /// Builds a settings instance with distinct, easily-asserted colors for each token category.
   /// </summary>
   private static Settings CreateColorSettings()
   {
      var settings = new Settings();
      settings.LoadDefaults();
      settings.DefaultWordColor = Color.FromArgb(255, 1, 1, 1);
      settings.VerbColor = Color.FromArgb(255, 2, 2, 2);
      settings.BuiltinFunctionColor = Color.FromArgb(255, 3, 3, 3);
      settings.PropertyColor = Color.FromArgb(255, 4, 4, 4);
      settings.BuiltinVariableColor = Color.FromArgb(255, 5, 5, 5);
      settings.LiteralColor = Color.FromArgb(255, 6, 6, 6);
      settings.SymbolColor = Color.FromArgb(255, 7, 7, 7);
      settings.ParenthesisColor = Color.FromArgb(255, 8, 8, 8);
      return settings;
   }

   private static Color ColorOf(IReadOnlyList<(string Text, Color Color)> segments, string tokenText)
   {
      var match = segments.FirstOrDefault(s => s.Text == tokenText);
      match.Text.Should().Be(tokenText, "expected a segment with text '{0}'", tokenText);
      return match.Color;
   }

   [Fact]
   public void Concatenated_Segments_Reproduce_The_Stripped_Input()
   {
      var settings = CreateColorSettings();
      const string code = "  this:tell(\"hi\");";

      var segments = MooCodeColorizer.GetColoredSegments(code, Dialect, settings);

      string.Concat(segments.Select(s => s.Text)).Should().Be(code);
   }

   [Fact]
   public void Strips_Ansi_Before_Lexing()
   {
      var settings = CreateColorSettings();
      // Embed ANSI color codes around the verb name.
      var code = "foo:[31mbar[0m";

      var segments = MooCodeColorizer.GetColoredSegments(code, Dialect, settings);

      string.Concat(segments.Select(s => s.Text)).Should().Be("foo:bar");
      ColorOf(segments, "bar").Should().Be(settings.VerbColor);
   }

   [Fact]
   public void Verb_After_Colon_Uses_Verb_Color()
   {
      var settings = CreateColorSettings();

      var segments = MooCodeColorizer.GetColoredSegments("foo:bar", Dialect, settings);

      ColorOf(segments, "bar").Should().Be(settings.VerbColor);
   }

   [Fact]
   public void Builtin_Function_Call_Uses_Function_Color()
   {
      var settings = CreateColorSettings();

      var segments = MooCodeColorizer.GetColoredSegments("length(x)", Dialect, settings);

      ColorOf(segments, "length").Should().Be(settings.BuiltinFunctionColor);
   }

   [Fact]
   public void Property_After_Dot_Uses_Property_Color()
   {
      var settings = CreateColorSettings();

      var segments = MooCodeColorizer.GetColoredSegments("x.name", Dialect, settings);

      ColorOf(segments, "name").Should().Be(settings.PropertyColor);
   }

   [Theory]
   [InlineData("this")]
   [InlineData("NUM")]
   [InlineData("player")]
   public void Builtin_Variables_Use_Builtin_Variable_Color(string variable)
   {
      var settings = CreateColorSettings();

      var segments = MooCodeColorizer.GetColoredSegments($"x = {variable};", Dialect, settings);

      ColorOf(segments, variable).Should().Be(settings.BuiltinVariableColor);
   }

   [Fact]
   public void Dynamic_Call_Does_Not_Color_As_Verb_And_Inner_Variable_Is_Plain()
   {
      var settings = CreateColorSettings();

      // obj:(expr) — after ':' comes '(' (not an identifier), so no verb match. The variable inside
      // is a plain identifier colored with the default word color.
      var segments = MooCodeColorizer.GetColoredSegments("obj:(expr)", Dialect, settings);

      // No segment should be colored as a verb.
      segments.Should().NotContain(s => s.Color == settings.VerbColor);
      ColorOf(segments, "expr").Should().Be(settings.DefaultWordColor);
   }

   [Fact]
   public void Empty_Input_Returns_No_Segments()
   {
      var settings = CreateColorSettings();

      MooCodeColorizer.GetColoredSegments(string.Empty, Dialect, settings).Should().BeEmpty();
   }
}
