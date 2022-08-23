using System;
using System.Linq;
using System.Windows.Forms;
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
        /// Style bit mask
        /// </summary>
        /// <remarks>Bit 1 in position n means that this char will rendering by FastColoredTextBox.Styles[n]</remarks>
        public StyleIndex Style;

        /// <summary>
        /// An array of for this styled char
        /// </summary>
        public Style[] Styles;

        /// <summary>The index of the last style assigned</summary>
        /// <remarks>Returns -1 if there are no styles</remarks>
        public int LastStyle;

        private bool _ReadOnly;

        /// <summary>
        /// Gets a value indicating whether the character has a read only style.
        /// </summary>
        /// <value><c>true</c> if [read only]; otherwise, <c>false</c>.</value>
        public bool ReadOnly => _ReadOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="StyledChar"/> struct.
        /// </summary>
        /// <param name="c">The c.</param>
        public StyledChar(char c)
        {
            this.C = c;
            Style = StyleIndex.None;
            Styles = null;
            _ReadOnly = false;
            LastStyle = -1;
        }

        /// <summary>
        /// Adds the specified style to the character.
        /// </summary>
        /// <param name="style">The style.</param>
        /// <returns>The added Style.</returns>
        /// <exception cref="System.InvalidOperationException">You cannot add more than {LastStyle} styles to a character</exception>
        public Style AddStyle(Style style)
        {
            ++LastStyle;
            if (style is ReadOnlyStyle)
                _ReadOnly = true;
            if (Styles == null)
                Styles = new Style[2];
            else
            {
                if (Styles.Contains(style))
                    return style;
                if (LastStyle == Styles.Length)
                    if (LastStyle == int.MaxValue)
                        throw new InvalidOperationException($"You cannot add more than {LastStyle} styles to a character");
                    else
                        Array.Resize(ref Styles, Styles.Length > int.MaxValue / 2 ? int.MaxValue : Styles.Length * 2);

            }

            return Styles[LastStyle] = style;
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
            var index = GetStyleIndex(style);
            if (index == -1)
                return;

            for (int i = index; i < Styles.Length - 1; i++)
                Styles[i] = Styles[i + 1];
            Styles[^1] = null;
            _ReadOnly = false;
            for (int i = 0; i < LastStyle; i++)
                if (style is ReadOnlyStyle)
                    _ReadOnly = true;
            LastStyle--;
        }

        /// <summary>
        /// Clears all styles for the character.
        /// </summary>
        public void ClearStyles()
        {
            Styles = null;
            _ReadOnly = false;
            LastStyle = -1;
        }
    }
}
