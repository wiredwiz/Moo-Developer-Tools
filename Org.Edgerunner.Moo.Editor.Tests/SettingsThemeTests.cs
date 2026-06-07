using System.Drawing;
using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Configuration;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class SettingsThemeTests
{
   private static Settings CreatePopulatedSettings()
   {
      var settings = new Settings();
      settings.LoadDefaults();

      // Customize a representative spread of values, including a Transparent background
      // and a combined Bold;Italic font style, to exercise round-tripping.
      settings.KeywordColor = Color.Blue;
      settings.KeywordBackgroundColor = Color.Transparent;
      settings.KeywordFontStyle = FontStyle.Bold | FontStyle.Italic;
      settings.CommentColor = Color.Green;
      settings.CommentBackgroundColor = Color.LightYellow;
      settings.CommentFontStyle = FontStyle.Italic;
      settings.StringColor = Color.FromArgb(255, 12, 34, 56);
      settings.StringBackgroundColor = Color.Transparent;
      settings.StringFontStyle = FontStyle.Regular;
      settings.EditorBackgroundColor = Color.White;
      settings.EditorTextColor = Color.Black;
      settings.EditorCurrentLineColor = Color.Transparent;
      settings.ErrorIndicatorColor = Color.Red;
      settings.EditorTabLength = 4;
      settings.EditorWordWrap = false;
      settings.EditorAutocompleteDelay = 123;
      settings.DefaultGrammarDialect = GrammarDialect.ToastStunt;

      return settings;
   }

   [Fact]
   public void Clone_ProducesEqualButIndependentCopy()
   {
      var source = CreatePopulatedSettings();

      var clone = source.Clone();

      // Every relevant property should match the source.
      clone.KeywordColor.Should().Be(source.KeywordColor);
      clone.KeywordBackgroundColor.Should().Be(source.KeywordBackgroundColor);
      clone.KeywordFontStyle.Should().Be(source.KeywordFontStyle);
      clone.CommentColor.Should().Be(source.CommentColor);
      clone.CommentBackgroundColor.Should().Be(source.CommentBackgroundColor);
      clone.CommentFontStyle.Should().Be(source.CommentFontStyle);
      clone.StringColor.Should().Be(source.StringColor);
      clone.EditorBackgroundColor.Should().Be(source.EditorBackgroundColor);
      clone.EditorTextColor.Should().Be(source.EditorTextColor);
      clone.EditorCurrentLineColor.Should().Be(source.EditorCurrentLineColor);
      clone.ErrorIndicatorColor.Should().Be(source.ErrorIndicatorColor);
      clone.EditorTabLength.Should().Be(source.EditorTabLength);
      clone.EditorWordWrap.Should().Be(source.EditorWordWrap);
      clone.EditorAutocompleteDelay.Should().Be(source.EditorAutocompleteDelay);
      clone.DefaultGrammarDialect.Should().Be(source.DefaultGrammarDialect);
   }

   [Fact]
   public void Clone_MutatingCloneDoesNotAffectSource()
   {
      var source = CreatePopulatedSettings();
      var originalKeyword = source.KeywordColor;
      var originalStyle = source.KeywordFontStyle;
      var originalTab = source.EditorTabLength;

      var clone = source.Clone();
      clone.KeywordColor = Color.Magenta;
      clone.KeywordFontStyle = FontStyle.Underline;
      clone.EditorTabLength = 99;
      clone.DefaultGrammarDialect = GrammarDialect.LambdaMoo;

      source.KeywordColor.Should().Be(originalKeyword);
      source.KeywordFontStyle.Should().Be(originalStyle);
      source.EditorTabLength.Should().Be(originalTab);
      source.DefaultGrammarDialect.Should().Be(GrammarDialect.ToastStunt);
   }

   [Fact]
   public void SaveTo_ThenLoadFrom_RoundTripsAllValues()
   {
      var source = CreatePopulatedSettings();
      var tempFile = Path.Combine(Path.GetTempPath(), $"moo-theme-test-{Guid.NewGuid():N}.config");

      try
      {
         source.SaveTo(tempFile);

         var loaded = new Settings();
         loaded.LoadFrom(tempFile);

         // Colors, including the Transparent backgrounds.
         loaded.KeywordColor.ToArgb().Should().Be(source.KeywordColor.ToArgb());
         loaded.KeywordBackgroundColor.ToArgb().Should().Be(Color.Transparent.ToArgb());
         loaded.StringBackgroundColor.ToArgb().Should().Be(Color.Transparent.ToArgb());
         loaded.CommentColor.ToArgb().Should().Be(source.CommentColor.ToArgb());
         loaded.CommentBackgroundColor.ToArgb().Should().Be(source.CommentBackgroundColor.ToArgb());
         loaded.StringColor.ToArgb().Should().Be(source.StringColor.ToArgb());
         loaded.EditorBackgroundColor.ToArgb().Should().Be(source.EditorBackgroundColor.ToArgb());
         loaded.EditorTextColor.ToArgb().Should().Be(source.EditorTextColor.ToArgb());
         loaded.EditorCurrentLineColor.ToArgb().Should().Be(Color.Transparent.ToArgb());
         loaded.ErrorIndicatorColor.ToArgb().Should().Be(source.ErrorIndicatorColor.ToArgb());

         // Font styles, including the combined Bold;Italic.
         loaded.KeywordFontStyle.Should().Be(FontStyle.Bold | FontStyle.Italic);
         loaded.CommentFontStyle.Should().Be(FontStyle.Italic);
         loaded.StringFontStyle.Should().Be(FontStyle.Regular);

         // Behavior and dialect values.
         loaded.EditorTabLength.Should().Be(source.EditorTabLength);
         loaded.EditorWordWrap.Should().Be(source.EditorWordWrap);
         loaded.EditorAutocompleteDelay.Should().Be(source.EditorAutocompleteDelay);
         loaded.DefaultGrammarDialect.Should().Be(source.DefaultGrammarDialect);
      }
      finally
      {
         if (File.Exists(tempFile))
            File.Delete(tempFile);
      }
   }
}
