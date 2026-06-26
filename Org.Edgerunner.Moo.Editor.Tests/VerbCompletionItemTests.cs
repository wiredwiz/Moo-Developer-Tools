using FastColoredTextBoxNS.Types;
using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class VerbCompletionItemTests
{
   [Fact]
   public void Constructor_keeps_bare_name_as_text_and_sets_verb_icon()
   {
      var item = new VerbCompletionItem("tell", CompletionIconCategory.Verb);

      // The menu text / match text stays the bare verb name (no parens) so prefix matching works.
      item.Text.Should().Be("tell");
      item.ImageIndex.Should().Be((int)CompletionIconCategory.Verb);
   }

   [Fact]
   public void Constructor_supports_inherited_verb_icon()
   {
      var item = new VerbCompletionItem("look", CompletionIconCategory.VerbInherited);

      item.ImageIndex.Should().Be((int)CompletionIconCategory.VerbInherited);
   }

   [Theory]
   [InlineData("this:te", "this:tell(^)")]
   [InlineData("#123:", "#123:tell(^)")]
   [InlineData("$te", "$tell(^)")]
   public void GetTextForReplace_inserts_call_parens_with_caret_between(string fragment, string expected)
   {
      var item = new VerbCompletionItem("tell", CompletionIconCategory.Verb);

      item.Compare(fragment); // records the fragment prefix, as MemberCompletionItem does

      item.GetTextForReplace().Should().Be(expected);
   }

   [Fact]
   public void GetTextForReplace_places_caret_marker_strictly_between_the_parens()
   {
      var item = new VerbCompletionItem("tell", CompletionIconCategory.Verb);
      item.Compare("this:te");

      var text = item.GetTextForReplace();

      var open = text.IndexOf('(');
      var close = text.IndexOf(')');
      var caret = text.IndexOf('^');

      open.Should().BeGreaterThan(0);
      caret.Should().Be(open + 1, "the caret marker must sit immediately after the open paren");
      close.Should().Be(caret + 1, "the close paren must sit immediately after the caret marker");
   }

   [Fact]
   public void Compare_still_matches_typed_part_after_separator()
   {
      var item = new VerbCompletionItem("tell", CompletionIconCategory.Verb);

      item.Compare("this:te").Should().Be(CompareResult.VisibleAndSelected);
      item.Compare("this:xy").Should().Be(CompareResult.Hidden);
   }
}
