namespace Org.Edgerunner.Moo.Editor.Controls
{
   /// <summary>
   /// Pure decision logic for whitespace-tolerant, escape-aware auto-delete of bracket and quote pairs on
   /// backspace. Kept free of any WinForms types so it can be unit-tested without a live control.
   /// </summary>
   public static class SmartBracketDeletion
   {
      /// <summary>
      /// Given the current line and caret char index, determines whether backspacing the char immediately
      /// before the caret (expected to be <paramref name="opener"/>) should also remove a matching
      /// <paramref name="closer"/> that is the first non-whitespace char ahead (spaces and tabs skipped).
      /// </summary>
      /// <param name="lineText">The full text of the current line.</param>
      /// <param name="caretIndex">The caret's char index within the line.</param>
      /// <param name="opener">The opening char expected immediately before the caret.</param>
      /// <param name="closer">The matching closing char to look for ahead.</param>
      /// <returns>
      /// The line index of the matching closer to also delete, or <c>-1</c> for a normal backspace.
      /// </returns>
      public static int FindMatchingCloserAhead(string lineText, int caretIndex, char opener, char closer)
      {
         if (lineText == null || caretIndex <= 0 || caretIndex > lineText.Length)
            return -1;
         if (lineText[caretIndex - 1] != opener)
            return -1;

         int i = caretIndex;
         while (i < lineText.Length && (lineText[i] == ' ' || lineText[i] == '\t'))
            i++;

         if (i < lineText.Length && lineText[i] == closer)
            return i;

         return -1;
      }

      /// <summary>
      /// Determines whether the <c>"</c> at <paramref name="quoteIndex"/> is an unescaped delimiter, using
      /// backslash parity: it is a delimiter when an even number (including zero) of consecutive <c>\</c>
      /// immediately precede it, and escaped string content when the run is odd.
      /// </summary>
      /// <param name="lineText">The full text of the current line.</param>
      /// <param name="quoteIndex">The line index of the <c>"</c> to test.</param>
      public static bool IsUnescapedQuoteDelimiter(string lineText, int quoteIndex)
      {
         if (lineText == null || quoteIndex < 0 || quoteIndex >= lineText.Length)
            return false;

         int backslashes = 0;
         int j = quoteIndex - 1;
         while (j >= 0 && lineText[j] == '\\')
         {
            backslashes++;
            j--;
         }

         return (backslashes % 2) == 0;
      }
   }
}
