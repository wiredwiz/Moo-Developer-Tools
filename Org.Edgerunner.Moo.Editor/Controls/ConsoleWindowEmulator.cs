using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using FastColoredTextBoxNS.Types;

namespace Org.Edgerunner.Moo.Editor.Controls
{
   public partial class ConsoleWindowEmulator : FastColoredTextBox
   {
      private readonly ColorManager _ColorManager;
      private Color _ConsoleForeColor;
      private Color _ConsoleBackgroundColor;
      private FontStyle _ConsoleFontStyle;

      public TextStyle CurrentStyle { get; set; }

      public Color ConsoleForeColor
      {
         get => _ConsoleForeColor;
         set
         {
            _ConsoleForeColor = value;
            _ColorManager.DefaultTextColor = _ConsoleForeColor;
         }
      }

      public Color ConsoleBackgroundColor
      {
         get => _ConsoleBackgroundColor;
         set
         {
            _ConsoleBackgroundColor = value;
            _ColorManager.DefaultTextBackgroundColor = _ConsoleBackgroundColor;
         }
      }

      public FontStyle ConsoleFontStyle
      {
         get => _ConsoleFontStyle;
         set
         {
            _ConsoleFontStyle = value;
            _ColorManager.DefaultFontTextStyle = _ConsoleFontStyle;
         }
      }

      public ConsoleWindowEmulator()
      {
         InitializeComponent();
         _ColorManager = new ColorManager(Color.White, Color.Black);
         ConsoleForeColor = Color.WhiteSmoke;
         ConsoleBackgroundColor = Color.Black;
         ConsoleFontStyle = FontStyle.Regular;
         CurrentStyle = _ColorManager.CurrentStyle;
      }

      protected override void OnLoad(EventArgs e)
      {
         base.OnLoad(e);
         BackColor = ConsoleBackgroundColor;
         ForeColor = ConsoleForeColor;
         CaretColor = Color.Transparent;
         CaretBlinking = false;
         ShowLineNumbers = false;
         WordWrap = true;
         WordWrapMode = WordWrapMode.WordWrapControlWidth;
         PreferredLineWidth = 0;
         ReadOnly = true;
      }

      private static TextStyle GetStyle(Color foregroundColor, Color backgroundColor, FontStyle fontStyle)
      {
         var foreBrush = new SolidBrush(foregroundColor);
         var backBrush = new SolidBrush(backgroundColor);
         return new TextStyle(foreBrush, backBrush, fontStyle);
      }

      /// <summary>
      /// Append line to end of text.
      /// </summary>
      /// <param name="text"></param>
      public void WriteLine(string text)
      {
         Write(text, CurrentStyle);
         Write("\n");
      }

      /// <summary>
      /// Append line to end of text.
      /// </summary>
      /// <param name="text"></param>
      public void WriteAnsiLine(string text)
      {
         WriteAnsi(text);
         Write("\n");
      }

      /// <summary>
      /// Append line to end of text with a specific style.
      /// </summary>
      /// <param name="text">The text.</param>
      /// <param name="style">The text style.</param>
      public void WriteLine(string text, Style style)
      {
         Write(text, style);
         Write("\n");
      }

      /// <summary>
      /// Append line to end of text with Ansi colors replaced.
      /// </summary>
      /// <param name="text"></param>
      public void WriteAnsi(string text)
      {
         // Match Ansi color codes and process them
         var match = Regex.Match(text, @"\e\[(?<codes>(\d+;)*\d+);*m");
         while (match.Captures.Count != 0)
         {
            var codes = match.Groups["codes"].Value;
            if (match.Index != 0)
            {
               Write(text[..(match.Index)], CurrentStyle);
               Application.DoEvents();
            }
            CurrentStyle = _ColorManager.ProcessColors(codes.Split(';').ToList().Select(int.Parse).ToList());
            text = match.Index + match.Length < text.Length ? text[(match.Index + match.Length)..] : string.Empty;
            match = Regex.Match(text, @"\e\[(?<codes>(\d+;)*\d+);*m");
         }

         if (!string.IsNullOrEmpty(text))
            Write(text, CurrentStyle);
      }

      /// <summary>
      /// Append line to end of text.
      /// </summary>
      /// <param name="text"></param>
      public void Write(string text)
      {
         Write(text, null);
      }

      /// <summary>
      /// Append line to end of text.
      /// </summary>
      /// <param name="text">The text.</param>
      /// <param name="style">The style.</param>
      public void Write(string text, Style style)
      {
         try
         {
            if (style == null)
               AppendText(text);
            else
               AppendText(text, style);
         }
         finally
         {
            ClearUndo();
         }

         ScrollToEnd();
      }

      public void ScrollToEnd()
      {
         Selection.Start = new Place(TextSource[^1].Count, TextSource.Count - 1);
         Selection.End = Selection.Start;
      }
   }
}
