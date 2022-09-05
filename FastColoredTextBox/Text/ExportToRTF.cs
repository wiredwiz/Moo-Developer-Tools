﻿using FastColoredTextBoxNS.Types;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FastColoredTextBoxNS.Text {
	/// <summary>
	/// Exports colored text as RTF
	/// </summary>
	/// <remarks>At this time only TextStyle renderer is supported. Other styles are not exported.</remarks>
	public class ExportToRTF {
		/// <summary>
		/// Includes line numbers
		/// </summary>
		public bool IncludeLineNumbers { get; set; }
		/// <summary>
		/// Use original font
		/// </summary>
		public bool UseOriginalFont { get; set; }

		FastColoredTextBox tb;
		readonly Dictionary<Color, int> colorTable = new();

		public ExportToRTF() => UseOriginalFont = true;

		public string GetRtf(FastColoredTextBox tb) {
			this.tb = tb;
			TextSelectionRange sel = new(tb);
			sel.SelectAll();
			return GetRtf(sel);
		}

		public string GetRtf(TextSelectionRange r) {
			tb = r.tb;
			var sb = new StringBuilder();
			var tempSB = new StringBuilder();
			var currentStyles = new Style[]{};
			r.Normalize();
			int currentLine = r.Start.iLine;
			colorTable.Clear();
			//
			var lineNumberColor = GetColorTableNumber(r.tb.LineNumberColor);

			if (IncludeLineNumbers)
				tempSB.AppendFormat(@"{{\cf{1} {0}}}\tab", currentLine + 1, lineNumberColor);
			//
			foreach (Place p in r) {
				StyledChar c = r.tb[p.iLine][p.iChar];
				if ((c.Styles == null && currentStyles != null) ||
					 (currentStyles == null && c.Styles != null) ||
					 (currentStyles != null &&
				    !currentStyles.SequenceEqual(c.Styles))) {
					Flush(sb, tempSB, currentStyles);
					currentStyles = c.Styles;
				}

				if (p.iLine != currentLine) {
					for (int i = currentLine; i < p.iLine; i++) {
						tempSB.AppendLine(@"\line");
						if (IncludeLineNumbers)
							tempSB.AppendFormat(@"{{\cf{1} {0}}}\tab", i + 2, lineNumberColor);
					}
					currentLine = p.iLine;
				}
				switch (c.C) {
					case '\\':
						tempSB.Append(@"\\");
						break;
					case '{':
						tempSB.Append(@"\{");
						break;
					case '}':
						tempSB.Append(@"\}");
						break;
					default:
						var ch = c.C;
						var code = (int)ch;
						if (code < 128)
							tempSB.Append(c.C);
						else
							tempSB.AppendFormat(@"{{\u{0}}}", code);
						break;
				}
			}
			Flush(sb, tempSB, currentStyles);

			//build color table
			var list = new SortedList<int, Color>();
			foreach (var pair in colorTable)
				list.Add(pair.Value, pair.Key);

			tempSB.Length = 0;
			tempSB.AppendFormat(@"{{\colortbl;");

			foreach (var pair in list)
				tempSB.Append(GetColorAsString(pair.Value) + ";");
			tempSB.AppendLine("}");

			//
			if (UseOriginalFont) {
				_ = sb.Insert(0, string.Format(@"{{\fonttbl{{\f0\fmodern {0};}}}}{{\fs{1} ",
								tb.Font.Name, (int)(2 * tb.Font.SizeInPoints), tb.CharHeight));
				sb.AppendLine(@"}");
			}

			sb.Insert(0, tempSB.ToString());

			sb.Insert(0, @"{\rtf1\ud\deff0");
			sb.AppendLine(@"}");

			return sb.ToString();
		}

		private RTFStyleDescriptor GetRtfDescriptor(IEnumerable<Style> styles) {
			//find text renderer
			TextStyle textStyle = null;
			var hasTextStyle = false;

         var intersect = tb.StyleManager.GetStyles().Intersect(styles);
         foreach (var style in intersect)
         {
            var isTextStyle = style is TextStyle;
						if (isTextStyle)
							if (!hasTextStyle || tb.AllowSeveralTextStyleDrawing) {
								hasTextStyle = true;
								textStyle = style as TextStyle;
							}
         }

			//add TextStyle css
         var result =
            //draw by default renderer if rtf style is not found
            !hasTextStyle ? tb.DefaultStyle.GetRTF() : textStyle.GetRTF();

			return result;
		}

      private RTFStyleDescriptor GetRtfDescriptor(Style style) {
         //find text renderer
         TextStyle textStyle = null;
         var hasTextStyle = false;

         if (tb.StyleManager.IsManaged(style))
         {
            var isTextStyle = style is TextStyle;
            if (isTextStyle)
            {
               hasTextStyle = true;
               textStyle = (TextStyle)style;
            }
         }

         //add TextStyle css
         var result =
            //draw by default renderer if rtf style is not found
            !hasTextStyle ? tb.DefaultStyle.GetRTF() : textStyle.GetRTF();

         return result;
      }

		public static string GetColorAsString(Color color) {
			if (color == Color.Transparent)
				return "";
			return string.Format(@"\red{0}\green{1}\blue{2}", color.R, color.G, color.B);
		}

		private void Flush(StringBuilder sb, StringBuilder tempSB, IEnumerable<Style> currentStyles) {
			//find textRenderer
			if (tempSB.Length == 0)
				return;

			var desc = GetRtfDescriptor(currentStyles);
			var cf = GetColorTableNumber(desc.ForeColor);
			var cb = GetColorTableNumber(desc.BackColor);
			var tags = new StringBuilder();
			if (cf >= 0)
				tags.AppendFormat(@"\cf{0}", cf);
			if (cb >= 0)
				tags.AppendFormat(@"\highlight{0}", cb);
			if (!string.IsNullOrEmpty(desc.AdditionalTags))
				tags.Append(desc.AdditionalTags.Trim());

			if (tags.Length > 0)
				sb.AppendFormat(@"{{{0} {1}}}", tags, tempSB.ToString());
			else
				sb.Append(tempSB);
			tempSB.Length = 0;
		}

		private int GetColorTableNumber(Color color) {
			if (color.A == 0)
				return -1;

			if (!colorTable.ContainsKey(color))
				colorTable[color] = colorTable.Count + 1;

			return colorTable[color];
		}
	}

	public class RTFStyleDescriptor {
		public Color ForeColor { get; set; }
		public Color BackColor { get; set; }
		public string AdditionalTags { get; set; }
	}
}
