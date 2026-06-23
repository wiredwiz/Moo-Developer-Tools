using FluentAssertions;
using Org.Edgerunner.Moo.Editor;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class BuiltinFunctionDocsTests
{
   [Fact]
   public void Get_KnownBuiltin_ReturnsDocWithSignature()
   {
      var doc = BuiltinFunctionDocs.Get("tostr");

      doc.Should().NotBeNull();
      doc!.Signature.Should().Contain("tostr(");
   }

   [Fact]
   public void GetTooltipText_KnownBuiltin_StartsWithSignatureAndContainsNewline()
   {
      var doc = BuiltinFunctionDocs.Get("tostr");
      var tooltip = BuiltinFunctionDocs.GetTooltipText("tostr");

      tooltip.Should().NotBeNull();
      tooltip.Should().StartWith(doc!.Signature);
      tooltip.Should().Contain("\n");
   }

   [Fact]
   public void Get_UnknownBuiltin_ReturnsNull()
   {
      BuiltinFunctionDocs.Get("not_a_real_builtin_xyz").Should().BeNull();
      BuiltinFunctionDocs.GetTooltipText("not_a_real_builtin_xyz").Should().BeNull();
   }

   [Fact]
   public void EmbeddedResource_LoadsNonTrivialSet()
   {
      BuiltinFunctionDocs.Get("length").Should().NotBeNull();
      BuiltinFunctionDocs.Get("setremove").Should().NotBeNull();
   }
}
