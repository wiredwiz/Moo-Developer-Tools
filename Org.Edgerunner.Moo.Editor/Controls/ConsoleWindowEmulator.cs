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

      /// <summary>
      /// Appends a sequence of pre-colored segments as a single line in one batched operation: the
      /// concatenated text is inserted once and each segment's style is applied to its sub-range.
      /// </summary>
      /// <param name="segments">The ordered (text, style) segments that make up one line (no line breaks).</param>
      /// <remarks>
      /// This exists for performance: calling <see cref="WriteWithStyle"/> once per segment incurs a
      /// full AppendText (InsertTextCommand + Invalidate) plus <c>ClearUndo</c> and <c>ScrollToEnd</c>
      /// for every segment, which is dramatically slower when a line has many (e.g. syntax-highlighted
      /// code with one segment per token). Here the line is inserted once and those costs are paid once.
      /// </remarks>
      public void WriteStyledSegments(IReadOnlyList<(string Text, Style Style)> segments)
      {
         if (segments == null || segments.Count == 0)
            return;

         BeginUpdate();
         try
         {
            var lineIndex = TextSource.Count - 1;
            var column = TextSource[^1].Count;
            var builder = new System.Text.StringBuilder();
            var ranges = new List<(int Start, int Length, Style Style)>(segments.Count);
            foreach (var segment in segments)
            {
               var clean = (segment.Text ?? string.Empty).Replace("", "");
               if (clean.Length > 0)
                  ranges.Add((column, clean.Length, segment.Style));
               builder.Append(clean);
               column += clean.Length;
            }

            AppendText(builder.ToString());

            foreach (var (start, length, style) in ranges)
               if (style != null)
                  GetRange(new Place(start, lineIndex), new Place(start + length, lineIndex)).SetStyle(style);
         }
         finally
         {
            EndUpdate();
         }

         ClearUndo();
         ScrollToEnd();
      }

      /// <summary>
      /// Appends a whole block of pre-colored lines in one batched operation: the entire block text
      /// (every line's concatenated segment text, joined by line breaks plus a trailing line break) is
      /// inserted once, then each segment's style is applied to its sub-range at the correct (line, col).
      /// </summary>
      /// <param name="lines">
      /// The ordered lines; each line is an ordered list of (text, style) segments (no embedded line
      /// breaks). The block is terminated with a single trailing newline.
      /// </param>
      /// <remarks>
      /// This is the block-level analogue of <see cref="WriteStyledSegments"/>. For very large listings
      /// (e.g. a long verb body) it pays the AppendText / <c>ClearUndo</c> / <c>ScrollToEnd</c> costs once
      /// for the whole block instead of once per line, and triggers a single paint.
      /// </remarks>
      public void WriteStyledBlock(IReadOnlyList<IReadOnlyList<(string Text, Style Style)>> lines)
      {
         if (lines == null || lines.Count == 0)
            return;

         BeginUpdate();
         try
         {
            var startLine = TextSource.Count - 1;
            var startColumn = TextSource[^1].Count;
            var (blockText, ranges) = BuildStyledBlock(lines, startLine, startColumn);

            AppendText(blockText);

            foreach (var (lineIndex, start, length, style) in ranges)
               if (style != null)
                  GetRange(new Place(start, lineIndex), new Place(start + length, lineIndex)).SetStyle(style);
         }
         finally
         {
            EndUpdate();
         }

         ClearUndo();
         ScrollToEnd();
      }

      /// <summary>
      /// Pure helper that computes the concatenated block text and the per-segment style ranges for
      /// <see cref="WriteStyledBlock"/>. Extracted as a seam so the multi-line range math can be
      /// unit-tested without a live control. The bell character is stripped (matching the other write
      /// methods) and each line is terminated with a single <c>\n</c>.
      /// </summary>
      /// <param name="lines">The ordered lines of (text, style) segments.</param>
      /// <param name="startLine">The text-source line index the first line is appended to.</param>
      /// <param name="startColumn">The column the first line begins at (subsequent lines start at 0).</param>
      /// <returns>The full block text and the ordered (line, start, length, style) ranges to style.</returns>
      public static (string Text, IReadOnlyList<(int Line, int Start, int Length, Style Style)> Ranges) BuildStyledBlock(
         IReadOnlyList<IReadOnlyList<(string Text, Style Style)>> lines,
         int startLine,
         int startColumn)
      {
         var builder = new StringBuilder();
         var ranges = new List<(int Line, int Start, int Length, Style Style)>();

         for (var i = 0; i < lines.Count; i++)
         {
            var lineIndex = startLine + i;
            var column = i == 0 ? startColumn : 0;
            var segments = lines[i];
            if (segments != null)
               foreach (var segment in segments)
               {
                  var clean = (segment.Text ?? string.Empty).Replace("", "");
                  if (clean.Length > 0)
                     ranges.Add((lineIndex, column, clean.Length, segment.Style));
                  builder.Append(clean);
                  column += clean.Length;
               }

            builder.Append('\n');
         }

         return (builder.ToString(), ranges);
      }

      public void ScrollToEnd()
      {
         Selection.Start = new Place(TextSource[^1].Count, TextSource.Count - 1);
         Selection.End = Selection.Start;
      }
   }
}
