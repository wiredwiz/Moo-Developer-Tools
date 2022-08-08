using System.Text.RegularExpressions;
using FastColoredTextBoxNS;

namespace Moo_Editor_Control
{
   public partial class MooEditor : FastColoredTextBox
   {
      public MooEditor()
      {
         InitializeComponent();
         LeftBracket = '(';
         LeftBracket2 = '{';
         LeftBracket3 = '[';
         RightBracket = ')';
         RightBracket2 = '}';
         RightBracket3 = ']';
         WordWrap = true;
         AutoIndentChars = false;
         WordWrapAutoIndent = true;
         WordWrapIndent = 2;
         TabLength = 2;
         AutoIndentNeeded += MooEditor_AutoIndentNeeded;
      }

      private void MooEditor_AutoIndentNeeded(object? sender, AutoIndentEventArgs e)
      {
         bool indent = Regex.IsMatch(e.PrevLineText, @"\b(if|while|fork|for|try|elseif|else|except|finally)\b");
         bool previousTerminates = Regex.IsMatch(e.PrevLineText, @"\b(endif|endfork|endfor|endwhile|endtry)\b");
         bool currentIndents = Regex.IsMatch(e.LineText, @"\b(if|fork|for|while|try)\b");
         bool unIndent = Regex.IsMatch(e.LineText,
            @"\b(elseif|else|except|finally|endif|endfork|endfor|endwhile|endtry)\b");

         // The previous line had something like "if (foo) bah; endif" and the current line contains something like "else"
         if (indent && previousTerminates && unIndent)
         {
            e.Shift = -e.TabLength;
            e.ShiftNextLines = -e.TabLength;
            return;
         }

         // The previous line had something like "if (foo) bah; endif" and our current line has nothing special
         if (indent && previousTerminates)
            return;

         // The previous line starting an indentation block and our current line does not contain something that would cause an un-indent
         if (indent && !unIndent)
         {
            e.Shift = e.TabLength;
            e.ShiftNextLines = e.TabLength;
            return;
         }

         // Our current line contains something like "if (foo) bah; endif"
         if (unIndent && currentIndents)
            return;

         // Lastly our current line contains something that would cause an un-indent and the previous line does not cause an indent
         if (unIndent && !indent)
         {
            e.Shift = -e.TabLength;
            e.ShiftNextLines = -e.TabLength;
         }
      }
   }
}