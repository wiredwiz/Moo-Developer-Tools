using System.Windows.Forms;
using FastColoredTextBoxNS.Input;
using FluentAssertions;
using Xunit;

namespace FastColoredTextBox.Tests;

public class HotkeysMappingTests
{
   [Fact]
   public void Parse_normalizes_legacy_key_abbreviations()
   {
      // .NET Framework's KeysConverter accepted Ins/Del/PgUp/PgDn; .NET 8's does not. Parse must
      // normalize them (the default hotkeys + designer .resx strings still use the abbreviations).
      var map = HotkeysMapping.Parse(
         "Ins=ReplaceMode, Del=DeleteCharRight, PgUp=GoPageUp, PgDn=GoPageDown, " +
         "Shift+Ins=Paste, Ctrl+Del=ClearWordRight, Ctrl+Ins=Copy");

      map[Keys.Insert].Should().Be(FCTBAction.ReplaceMode);
      map[Keys.Delete].Should().Be(FCTBAction.DeleteCharRight);
      map[Keys.PageUp].Should().Be(FCTBAction.GoPageUp);
      map[Keys.PageDown].Should().Be(FCTBAction.GoPageDown);
      map[Keys.Shift | Keys.Insert].Should().Be(FCTBAction.Paste);
      map[Keys.Control | Keys.Delete].Should().Be(FCTBAction.ClearWordRight);
      map[Keys.Control | Keys.Insert].Should().Be(FCTBAction.Copy);
   }

   [Fact]
   public void Parse_still_accepts_full_key_names()
   {
      var map = HotkeysMapping.Parse("Insert=ReplaceMode, Ctrl+PageDown=GoPageDown");

      map[Keys.Insert].Should().Be(FCTBAction.ReplaceMode);
      map[Keys.Control | Keys.PageDown].Should().Be(FCTBAction.GoPageDown);
   }
}
