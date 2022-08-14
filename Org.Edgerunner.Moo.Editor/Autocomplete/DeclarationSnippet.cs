﻿using System.Text.RegularExpressions;
using FastColoredTextBoxNS.Types;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// This item appears when any part of snippet text is typed
/// </summary>
public class DeclarationSnippet : SnippetAutocompleteItem
{
   public DeclarationSnippet(string snippet)
      : base(snippet)
   {
   }

   public override CompareResult Compare(string fragmentText)
   {
      var pattern = Regex.Escape(fragmentText);
      if (Regex.IsMatch(Text, "\\b" + pattern, RegexOptions.IgnoreCase))
         return CompareResult.Visible;
      return CompareResult.Hidden;
   }
}