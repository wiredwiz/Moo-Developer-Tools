using System;
using FastColoredTextBoxNS.Types;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FastColoredTextBoxNS.Text {
	/// <summary>
	/// Exports colored text as HTML
	/// </summary>
	/// <remarks>At this time only TextStyle renderer is supported. Other styles is not exported.</remarks>
	public class ExportToHTML {
		public string LineNumbersCSS = "<style type=\"text/css\"> .lineNumber{font-family : monospace; font-size : small; font-style : normal; font-weight : normal; color : Teal; background-color : ThreedFace;} </style>";

		/// <summary>
		/// Use nbsp; instead space
		/// </summary>
		public bool UseNbsp { get; set; }
		/// <summary>
		/// Use nbsp; instead space in beginning of line
		/// </summary>
		public bool UseForwardNbsp { get; set; }
		/// <summary>
		/// Use original font
		/// </summary>
		public bool UseOriginalFont { get; set; }
		/// <summary>
		/// Use style tag instead style attribute
		/// </summary>
		public bool UseStyleTag { get; set; }
		/// <summary>
		/// Use 'br' tag instead of '\n'
		/// </summary>
		public bool UseBr { get; set; }
		/// <summary>
		/// Includes line numbers
		/// </summary>
		public bool IncludeLineNumbers { get; set; }

		FastColoredTextBox tb;

		public ExportToHTML() {
			UseNbsp = true;
			UseOriginalFont = true;
			UseStyleTag = true;
			UseBr = true;
		}

		public string GetHtml(FastColoredTextBox tb) {
			this.tb = tb;
			TextSelectionRange sel = new(tb);
			sel.SelectAll();
			return GetHtml(sel);
		}

		public string GetHtml(TextSelectionRange r) {
         tb = r.tb;
         var sb = new StringBuilder();
         var tempSB = new StringBuilder();
         var currentStyles = new Style[]{};
         var styles = new HashSet<Style>();
			r.Normalize();
			int currentLine = r.Start.iLine;
			//
			if (UseOriginalFont)
				sb.AppendFormat("<font style=\"font-family: {0}, monospace; font-size: {1}pt; line-height: {2}px;\">",
												r.tb.Font.Name, r.tb.Font.SizeInPoints, r.tb.CharHeight);

			//
			if (IncludeLineNumbers)
				tempSB.AppendFormat("<span class=lineNumber>{0}</span>  ", currentLine + 1);
			//
			bool hasNonSpace = false;
			foreach (Place p in r) {
				StyledChar c = r.tb[p.iLine][p.iChar];
            if ((currentStyles == null && c.Styles != null) ||
                (currentStyles != null && c.Styles == null) ||
                (currentStyles != null && !currentStyles.SequenceEqual(c.Styles)))
            {
               Flush(sb, tempSB, currentStyles);
               currentStyles = c.Styles;
					if (currentStyles != null)
                  foreach (var currentStyle in currentStyles)
                     styles.Add(currentStyle);
            }

				if (p.iLine != currentLine) {
					for (int i = currentLine; i < p.iLine; i++) {
						tempSB.Append(UseBr ? "<br>" : "\r\n");
						if (IncludeLineNumbers)
							tempSB.AppendFormat("<span class=lineNumber>{0}</span>  ", i + 2);
					}
					currentLine = p.iLine;
					hasNonSpace = false;
				}
				switch (c.C) {
					case ' ':
						if ((hasNonSpace || !UseForwardNbsp) && !UseNbsp)
							goto default;

						tempSB.Append("&nbsp;");
						break;
					case '<':
						tempSB.Append("&lt;");
						break;
					case '>':
						tempSB.Append("&gt;");
						break;
					case '&':
						tempSB.Append("&amp;");
						break;
					default:
						hasNonSpace = true;
						tempSB.Append(c.C);
						break;
				}
			}
			Flush(sb, tempSB, currentStyles);

			if (UseOriginalFont)
				sb.Append("</font>");

			//build styles
			if (UseStyleTag) {
				tempSB.Length = 0;
				tempSB.Append("<style type=\"text/css\">");
				foreach (var style in styles)
					tempSB.AppendFormat(".fctb{0}{{ {1} }}\r\n", GetStyleName(style), GetCss(style));
				tempSB.Append("</style>");

				sb.Insert(0, tempSB.ToString());
			}

			if (IncludeLineNumbers)
				sb.Insert(0, LineNumbersCSS);

			return sb.ToString();
		}

		private string GetCss(IEnumerable<Style> styles) {
			TextStyle textStyle = null;
			bool hasTextStyle = false;

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
            //draw by default renderer if css style is not found
            !hasTextStyle ? tb.DefaultStyle.GetCSS() : textStyle.GetCSS();

         return result;
      }

      private string GetCss(Style style) {
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
            !hasTextStyle ? tb.DefaultStyle.GetCSS() : textStyle.GetCSS();

         return result;
      }

		public static string GetColorAsString(Color color) {
			if (color == Color.Transparent)
				return "";
			return string.Format("#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
		}

		static string GetStyleName(IEnumerable<Style> styles)
      {
         var names = new List<string>();
			foreach (var style in styles)
			{
				if (style == null)
					break;
            names.Add(style.GetType().Name);
			}
			return string.Join(',', names).Replace(" ", "").Replace(",", "");
		}

      static string GetStyleName(Style style)
      {
         return style?.GetType().Name.Replace(" ", "").Replace(",", "") ?? string.Empty;
      }

		private void Flush(StringBuilder sb, StringBuilder tempSB, IEnumerable<Style> currentStyles) {
			//find textRenderer
			if (tempSB.Length == 0)
				return;
			if (UseStyleTag)
				sb.AppendFormat("<font class=fctb{0}>{1}</font>", GetStyleName(currentStyles), tempSB);
			else {
				string css = GetCss(currentStyles);
				if (css != "")
					sb.AppendFormat("<font style=\"{0}\">", css);
				sb.Append(tempSB);
				if (css != "")
					sb.Append("</font>");
			}
			tempSB.Length = 0;
		}
	}
}
