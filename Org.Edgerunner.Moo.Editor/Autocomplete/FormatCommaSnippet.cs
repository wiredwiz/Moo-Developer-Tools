using System.Text.RegularExpressions;
using FastColoredTextBoxNS.Types;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// Divides comma separated words/numbers with proper spacing: "123,456" -> "123, 456"
/// </summary>
public class FormatCommaSnippet : AutocompleteItem
{
   string pattern;

   public FormatCommaSnippet(string pattern):base("")
   {
      this.pattern = pattern;
   }

   public FormatCommaSnippet()
      : this(@"^(\d+)([a-zA-Z_]+)(\d*)$")
   {
   }

   public override CompareResult Compare(string fragmentText)
   {
      if (Regex.IsMatch(fragmentText, pattern))
      {
         Text = InsertSpaces(fragmentText);
         if(Text != fragmentText)
            return CompareResult.Visible;
      }
      return CompareResult.Hidden;
   }

   public string InsertSpaces(string fragment)
   {
      var m = Regex.Match(fragment, pattern);
      if (m.Groups[1].Value == "" && m.Groups[2].Value == "")
         return fragment;
      if (m.Groups[4].Captures.Count == 0)
         return fragment;

      var result = m.Groups[1].Value;
      for (int i = 0; i < m.Groups[4].Captures.Count; i++)
         result += ", " + m.Groups[4].Captures[i].Value;
      return result;
   }

   public override string ToolTipTitle
   {
      get
      {
         return Text;
      }
   }
}