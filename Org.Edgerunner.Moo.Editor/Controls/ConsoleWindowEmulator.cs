using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
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
      public AnsiManager AnsiManager { get; }
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
            AnsiManager.DefaultTextColor = _ConsoleForeColor;
         }
      }

      public Color ConsoleBackgroundColor
      {
         get => _ConsoleBackgroundColor;
         set
         {
            _ConsoleBackgroundColor = value;
            AnsiManager.DefaultTextBackgroundColor = _ConsoleBackgroundColor;
         }
      }

      public FontStyle ConsoleFontStyle
      {
         get => _ConsoleFontStyle;
         set
         {
            _ConsoleFontStyle = value;
            AnsiManager.DefaultFontTextStyle = _ConsoleFontStyle;
         }
      }

      /// <summary>
      /// Gets or sets a value indicating whether to enable ANSI color.
      /// </summary>
      /// <value><c>true</c> if [ANSI color enabled]; otherwise, <c>false</c>.</value>
      public bool AnsiColorEnabled { get; set; } = true;
      
      /// <summary>
      /// Gets or sets a value indicating whether to enable ascii bell.
      /// </summary>
      /// <value><c>true</c> if [bell enabled]; otherwise, <c>false</c>.</value>
      public bool AsciiBellEnabled { get; set; } = true;

      public ConsoleWindowEmulator()
      {
         InitializeComponent();
         AnsiManager = new AnsiManager(Color.White, Color.Black);
         AnsiManager.EchoEnabled = true;
         ConsoleForeColor = Color.WhiteSmoke;
         ConsoleBackgroundColor = Color.Black;
         ConsoleFontStyle = FontStyle.Regular;
         CurrentStyle = AnsiManager.CurrentStyle;
         LineInterval = 4;
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
         FoldingHighlightEnabled = false;
         HighlightFoldingIndicator = false;
         ShowFoldingLines = false;
         var testStyle = new TextStyle(new SolidBrush(Color.Red), null, FontStyle.Regular);
      }

      /// <summary>
      /// Echoes the text.
      /// </summary>
      /// <param name="text">The text.</param>
      public void EchoText(string text)
      {
         if (AnsiManager.EchoEnabled)
            Write(text, AnsiManager.EchoStyle);
      }

      /// <summary>
      /// Echoes the text line.
      /// </summary>
      /// <param name="text">The text.</param>
      public void EchoTextLine(string text)
      {
         if (AnsiManager.EchoEnabled)
            WriteLine(text, AnsiManager.EchoStyle);
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
         try
         {
            SuspendLayout();
            var match = Regex.Match(text, @"\e\[(?<codes>(\d+;)*\d+);*m");
            while (match.Captures.Count != 0)
            {
               var codes = match.Groups["codes"].Value;
               if (match.Index != 0)
               {
                  if (AnsiColorEnabled)
                  {
                     if (AnsiManager.Blinking)
                        WriteWithStyles(text[..(match.Index)], new Style[] { CurrentStyle, DefaultBlinkingStyle });
                     else
                        Write(text[..(match.Index)], CurrentStyle);
                  }
                  else
                     Write(text[..(match.Index)]);
               }
               CurrentStyle = AnsiManager.ProcessCodes(codes.Split(';').ToList().Select(int.Parse).ToList());
               text = match.Index + match.Length < text.Length ? text[(match.Index + match.Length)..] : string.Empty;
               match = Regex.Match(text, @"\e\[(?<codes>(\d+;)*\d+);*m");
            }

            if (!string.IsNullOrEmpty(text))
               if (AnsiColorEnabled)
               {
                  if (AnsiManager.Blinking)
                     WriteWithStyles(text, new Style[] { CurrentStyle, DefaultBlinkingStyle });
                  else
                     Write(text, CurrentStyle);
               }
               else
                  Write(text);
         }
         finally
         {
            ResumeLayout();
            Application.DoEvents();
         }
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
      /// <param name="style">The style to apply.</param>
      public void Write(string text, Style style)
      {
         var newText = text.Replace("\u0007", "");
         if (newText.Length != text.Length && AsciiBellEnabled)
            SystemSounds.Beep.Play();

         try
         {
            if (style == null)
               AppendText(newText);
            else
               AppendText(newText, style);
         }
         finally
         {
            ClearUndo();
         }

         ScrollToEnd();
      }

      /// <summary>
      /// Append line to end of text.
      /// </summary>
      /// <param name="text">The text.</param>
      /// <param name="styles">The styles to apply.</param>
      public void WriteWithStyles(string text, IEnumerable<Style> styles)
      {
         var newText = text.Replace("\u0007", "");
         if (newText.Length != text.Length && AsciiBellEnabled)
            SystemSounds.Beep.Play();

         try
         {
            if (styles == null)
               AppendText(newText);
            else
               AppendTextWithStyles(newText, styles);
         }
         finally
         {
            ClearUndo();
         }

         ScrollToEnd();
      }

      /// <summary>
      /// Append line to end of text.
      /// </summary>
      /// <param name="text">The text.</param>
      /// <param name="style">The style to apply.</param>
      public void WriteWithStyle(string text, Style style)
      {
         WriteWithStyles(text, new [] { style });
      }

      public void ScrollToEnd()
      {
         Selection.Start = new Place(TextSource[^1].Count, TextSource.Count - 1);
         Selection.End = Selection.Start;
      }
   }
}
