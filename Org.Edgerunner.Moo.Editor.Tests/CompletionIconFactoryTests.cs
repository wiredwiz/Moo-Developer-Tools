using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class CompletionIconFactoryTests
{
   [Fact]
   public void CreateImageList_ReturnsEightSixtyFourBySixtyFourImages()
   {
      var imageList = CompletionIconFactory.CreateImageList();

      imageList.Should().NotBeNull();
      imageList.Images.Count.Should().Be(8);
      imageList.ImageSize.Width.Should().Be(64);
      imageList.ImageSize.Height.Should().Be(64);

      for (var i = 0; i < imageList.Images.Count; i++)
      {
         var image = imageList.Images[i];
         image.Should().NotBeNull();
         image.Width.Should().Be(64);
         image.Height.Should().Be(64);
      }
   }

   [Fact]
   public void CreateImageList_ReturnsTheSameCachedInstance()
   {
      var first = CompletionIconFactory.CreateImageList();
      var second = CompletionIconFactory.CreateImageList();

      second.Should().BeSameAs(first);
   }

   [Fact]
   public void GetIcon_ReturnsNonNull64x64ForEveryCategory()
   {
      foreach (CompletionIconCategory category in Enum.GetValues(typeof(CompletionIconCategory)))
      {
         using var icon = CompletionIconFactory.GetIcon(category);
         icon.Should().NotBeNull($"category {category} should have an icon");
         icon.Width.Should().Be(64);
         icon.Height.Should().Be(64);
      }
   }

   [Theory]
   [InlineData("E_PERM", CompletionIconCategory.Constant)]
   [InlineData("true", CompletionIconCategory.Constant)]
   [InlineData("OBJ", CompletionIconCategory.Constant)]
   [InlineData("player", CompletionIconCategory.Variable)]
   [InlineData("args", CompletionIconCategory.Variable)]
   [InlineData("while", CompletionIconCategory.Keyword)]
   [InlineData("if", CompletionIconCategory.Keyword)]
   [InlineData("endtry", CompletionIconCategory.Keyword)]
   public void ClassifyKeyword_ReturnsExpectedCategory(string word, CompletionIconCategory expected)
   {
      Editor.Moo.ClassifyKeyword(word).Should().Be(expected);
   }

   [Fact]
   public void ClassifyKeyword_UnknownWord_DefaultsToKeyword()
   {
      Editor.Moo.ClassifyKeyword("not_a_real_keyword").Should().Be(CompletionIconCategory.Keyword);
   }

   [Fact]
   public void EveryKeyword_ClassifiesIntoExactlyOneOfConstantVariableKeyword()
   {
      var allowed = new[]
      {
         CompletionIconCategory.Constant,
         CompletionIconCategory.Variable,
         CompletionIconCategory.Keyword
      };

      foreach (var keyword in Editor.Moo.Keywords)
      {
         var category = Editor.Moo.ClassifyKeyword(keyword);
         allowed.Should().Contain(category, $"keyword '{keyword}' must classify into a wired category");
      }
   }

   [Fact]
   public void CategorySets_AreDisjoint()
   {
      var constants = new HashSet<string>(Editor.Moo.Constants);
      var variables = new HashSet<string>(Editor.Moo.BuiltinVariables);
      var keywords = new HashSet<string>(Editor.Moo.ControlFlowKeywords);

      constants.Overlaps(variables).Should().BeFalse();
      constants.Overlaps(keywords).Should().BeFalse();
      variables.Overlaps(keywords).Should().BeFalse();
   }

   [Fact]
   public void CategorySets_UnionEqualsKeywordsSet()
   {
      var union = new HashSet<string>(Editor.Moo.Constants);
      union.UnionWith(Editor.Moo.BuiltinVariables);
      union.UnionWith(Editor.Moo.ControlFlowKeywords);

      var keywords = new HashSet<string>(Editor.Moo.Keywords);

      union.SetEquals(keywords).Should().BeTrue();
   }
}
