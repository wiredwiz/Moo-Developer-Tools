﻿using FastColoredTextBoxNS;
using FastColoredTextBoxNS.Types;

namespace Org.Edgerunner.Moo.Editor.Autocomplete;

/// <summary>
/// Inserts line break after '}'
/// </summary>
public class InsertEnterSnippet : AutocompleteItem
{
   Place enterPlace = Place.Empty;

   public InsertEnterSnippet()
      : base("[Line break]")
   {
   }

   public override CompareResult Compare(string fragmentText)
   {
      var r = Parent.Fragment.Clone();
      while (r.Start.iChar > 0)
      {
         if (r.CharBeforeStart == '}')
         {
            enterPlace = r.Start;
            return CompareResult.Visible;
         }

         r.GoLeftThroughFolded();
      }

      return CompareResult.Hidden;
   }

   public override string GetTextForReplace()
   {
      //extend range
      TextSelectionRange r = Parent.Fragment;
      Place end = r.End;
      r.Start = enterPlace;
      r.End = r.End;
      //insert line break
      return Environment.NewLine + r.Text;
   }

   public override void OnSelected(AutocompleteMenu popupMenu, SelectedEventArgs e)
   {
      base.OnSelected(popupMenu, e);
      if (Parent.Fragment.tb.AutoIndent)
         Parent.Fragment.tb.DoAutoIndent();
   }

   public override string ToolTipTitle
   {
      get { return "Insert line break after '}'"; }
   }
}