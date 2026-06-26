using System;

namespace FastColoredTextBoxNS
{
   /// <summary>
   /// Pure decision logic for "smart" auto-close of bracket pairs. Kept free of any WinForms types so it
   /// can be unit-tested without a live control.
   /// </summary>
   public static class SmartBracketDecisions
   {
      /// <summary>
      /// Scans the line from <paramref name="caretIndex"/> to end-of-line tracking bracket depth
      /// (<c>+1</c> for each <paramref name="opener"/>, <c>-1</c> for each <paramref name="closer"/>).
      /// Returns <c>true</c> if the depth ever goes negative — i.e. there is an unmatched closer ahead
      /// that a freshly typed opener would pair with, so no auto-closer should be inserted.
      /// </summary>
      /// <param name="lineText">The full text of the current line.</param>
      /// <param name="caretIndex">The caret's char index within the line (chars at/after this are scanned).</param>
      /// <param name="opener">The opening bracket char being typed.</param>
      /// <param name="closer">The matching closing bracket char.</param>
      public static bool HasUnmatchedCloserAhead(string lineText, int caretIndex, char opener, char closer)
      {
         if (string.IsNullOrEmpty(lineText))
            return false;

         int depth = 0;
         for (int i = Math.Max(0, caretIndex); i < lineText.Length; i++)
         {
            char ch = lineText[i];
            if (ch == opener)
               depth++;
            else if (ch == closer)
            {
               depth--;
               if (depth < 0)
                  return true;
            }
         }

         return false;
      }
   }
}
