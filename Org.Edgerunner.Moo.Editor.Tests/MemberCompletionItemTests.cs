using FastColoredTextBoxNS.Types;
using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class MemberCompletionItemTests
{
   [Fact]
   public void Constructor_sets_image_index_from_category()
   {
      var item = new MemberCompletionItem("tell", CompletionIconCategory.Verb);

      item.ImageIndex.Should().Be((int)CompletionIconCategory.Verb);
      item.Text.Should().Be("tell");
   }

   [Theory]
   [InlineData("this:te", CompareResult.VisibleAndSelected)]   // prefix match on typed part
   [InlineData("this:TE", CompareResult.VisibleAndSelected)]   // case-insensitive
   [InlineData("this:", CompareResult.Visible)]                // nothing typed yet: visible, unselected
   [InlineData("obj.te", CompareResult.VisibleAndSelected)]    // property separator works too
   [InlineData("$te", CompareResult.VisibleAndSelected)]       // core-reference separator
   [InlineData("this:xy", CompareResult.Hidden)]               // typed part does not match
   [InlineData("tell", CompareResult.Hidden)]                  // no separator: members hidden in plain fragments
   public void Compare_matches_typed_part_after_last_separator(string fragment, CompareResult expected)
   {
      var item = new MemberCompletionItem("tell", CompletionIconCategory.Verb);

      item.Compare(fragment).Should().Be(expected);
   }

   [Theory]
   [InlineData("this:te", "this:tell")]
   [InlineData("#123:", "#123:tell")]
   [InlineData("obj.te", "obj.tell")]
   [InlineData("$te", "$tell")]
   public void GetTextForReplace_prepends_the_fragment_prefix(string fragment, string expected)
   {
      var item = new MemberCompletionItem("tell", CompletionIconCategory.Verb);

      item.Compare(fragment);   // Compare records the prefix, as MethodAutocompleteItem does upstream

      item.GetTextForReplace().Should().Be(expected);
   }
}
