using FastColoredTextBoxNS;
using FastColoredTextBoxNS.Types;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

public class AutoIndentingSnippet : AutocompleteItem
{
   public AutoIndentingSnippet(string snippet)
      : base(snippet)
   {
   }

   public override void OnSelected(AutocompleteMenu popupMenu, SelectedEventArgs e)
   {
      base.OnSelected(popupMenu, e);
      if (Parent.Fragment.tb.AutoIndent)
      {
         // Fix our auto-indentation and remove the text selection
         Parent.Fragment.tb.DoAutoIndent();
         Parent.Fragment.tb.Selection =
            new TextSelectionRange(
               Parent.Fragment.tb,
               new Place(Parent.Fragment.tb.Selection.End.iChar, Parent.Fragment.tb.Selection.End.iLine),
               new Place(Parent.Fragment.tb.Selection.End.iChar, Parent.Fragment.tb.Selection.End.iLine));
      }
   }
}