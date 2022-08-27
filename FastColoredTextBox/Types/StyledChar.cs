using System;
using System.Linq;
// ReSharper disable ExceptionNotDocumented

namespace FastColoredTextBoxNS.Types
{
   /// <summary>
   /// Char and style
   /// </summary>
   public struct StyledChar
   {

      /// <summary>
      /// Unicode character
      /// </summary>
      public char C;

      /// <summary>
      /// An array of the styles for this char
      /// </summary>
      public Style[] Styles;

      /// <summary>The index of the last style assigned</summary>
      /// <remarks>Returns -1 if there are no styles</remarks>
      public int LastStyleIndex;

      private bool _ReadOnly;

      /// <summary>
      /// Gets a value indicating whether the character has a read only style.
      /// </summary>
      /// <value><c>true</c> if [read only]; otherwise, <c>false</c>.</value>
      public bool ReadOnly => _ReadOnly;

      private bool _Blinking;

      /// <summary>
      /// Gets a value indicating whether this <see cref="StyledChar"/> is blinking.
      /// </summary>
      /// <value>
      ///   <c>true</c> if blinking; otherwise, <c>false</c>.
      /// </value>
      public bool Blinking => _Blinking;

      /// <summary>
      /// Initializes a new instance of the <see cref="StyledChar"/> struct.
      /// </summary>
      /// <param name="c">The c.</param>
      public StyledChar(char c)
      {
         this.C = c;
         Styles = null;
         _ReadOnly = false;
         _Blinking = false;
         LastStyleIndex = -1;
      }

      /// <summary>
      /// Adds the specified style to the character.
      /// </summary>
      /// <param name="style">The style.</param>
      /// <returns>The added Style.</returns>
      /// <exception cref="System.InvalidOperationException">You cannot add more than {LastStyleIndex} styles to a character</exception>
      public Style AddStyle(Style style)
      {
         ++LastStyleIndex;

         // Update Readonly status if needed
         if (style is ReadOnlyStyle)
            _ReadOnly = true;

         // Update Blinking status if needed
         if (style is BlinkingStyle)
            _Blinking = true;

         // Initialize storage, fetch existing style or expand the storage array if needed
         if (Styles == null)
            Styles = new Style[2];
         else if (LastStyleIndex != 0)
         {
            if (Styles.Contains(style))
               return style;
            if (LastStyleIndex == Styles.Length)
               if (LastStyleIndex == int.MaxValue)
                  throw new InvalidOperationException($"You cannot add more than {LastStyleIndex} styles to a character");
               else
                  Array.Resize(ref Styles, Styles.Length > int.MaxValue / 2 ? int.MaxValue : Styles.Length * 2);

         }

         return Styles[LastStyleIndex] = style;
      }

      /// <summary>
      /// Gets the index of the specified style.
      /// </summary>
      /// <param name="style">The style.</param>
      /// <returns>The <see cref="int"/> index value.</returns>
      public int GetStyleIndex(Style style)
      {
         if (Styles == null)
            return -1;

         return Array.IndexOf(Styles, style);
      }

      /// <summary>
      /// Removes the specified style.
      /// </summary>
      /// <param name="style">The style.</param>
      public void RemoveStyle(Style style)
      {
         _ReadOnly = false;
         _Blinking = false;
         var index = GetStyleIndex(style);
         if (index == -1)
            return;

         for (int i = index; i < Styles.Length - 1; i++)
         {
            Styles[i] = Styles[i + 1];

            if (Styles[i] is ReadOnlyStyle)
               _ReadOnly = true;

            if (Styles[i] is BlinkingStyle)
               _Blinking = true;

            // If we have no more valid styles, no reason to keep clearing
            if (Styles[i] == null)
               break;
         }

         Styles[^1] = null;
         if (!_ReadOnly || !_Blinking)
            for (var i = 0; i < LastStyleIndex; i++)
            {
               if (Styles[i] is ReadOnlyStyle)
                  _ReadOnly = true;
               if (Styles[i] is BlinkingStyle)
                  _Blinking = true;
            }

         LastStyleIndex--;
      }

      /// <summary>
      /// Clears all styles for the character.
      /// </summary>
      public void ClearStyles()
      {
         // We clear the array rather than release it,
         // because it is quite possible we will be using it again momentarily.
         // Also it is unlikely to eat that much extra memory.
         if (Styles != null)
            Array.Clear(Styles, 0, Styles.Length);
         _ReadOnly = false;
         _Blinking = false;
         LastStyleIndex = -1;
      }
   }
}
