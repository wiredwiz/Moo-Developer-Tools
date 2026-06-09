using System;

namespace FastColoredTextBoxNS {
	/// <summary>
	/// Pure sizing math for the zoom-aware autocomplete popup. Given the base font size (the
	/// 100% baseline) and the editor zoom percentage, it yields the effective font size plus the
	/// scaled icon size, gutter (text x-offset) and max-height cap, applying the minimum floors.
	/// Kept free of any WinForms drawing so it can be unit-tested without showing a window.
	/// </summary>
	public readonly struct AutocompletePopupMetrics {
		/// <summary>Nominal icon edge length (px) at 100% zoom.</summary>
		public const int BaseIconSize = 16;
		/// <summary>Max popup height cap (px) at 100% zoom.</summary>
		public const int BaseMaxHeight = 180;
		/// <summary>Effective font never drops below this (pt).</summary>
		public const float MinFontSizeInPoints = 6f;
		/// <summary>
		/// Effective font never exceeds this (pt). Mirrors the editor's own zoom cap
		/// (<see cref="FastColoredTextBox.DoZoom"/> rejects sizes &gt; 300pt) so GDI+ is never
		/// asked for a degenerate font: <see cref="FastColoredTextBox.Zoom"/> stores its raw value
		/// even when the editor refuses an out-of-range zoom, so the popup must clamp independently.
		/// </summary>
		public const float MaxFontSizeInPoints = 300f;
		/// <summary>Effective icon never drops below this (px).</summary>
		public const int MinIconSize = 8;

		/// <summary>The effective font size, in points, after scaling and the font floor.</summary>
		public float FontSizeInPoints { get; }
		/// <summary>The effective icon edge length, in pixels, after scaling and the icon floor.</summary>
		public int IconSize { get; }
		/// <summary>The text x-offset / icon gutter, in pixels (icon size + 2).</summary>
		public int Gutter { get; }
		/// <summary>The scaled max-height cap, in pixels.</summary>
		public int MaxHeight { get; }

		private AutocompletePopupMetrics(float fontSizeInPoints, int iconSize, int gutter, int maxHeight) {
			FontSizeInPoints = fontSizeInPoints;
			IconSize = iconSize;
			Gutter = gutter;
			MaxHeight = maxHeight;
		}

		/// <summary>
		/// Computes the popup metrics for a given base font size and zoom percentage.
		/// </summary>
		/// <param name="baseFontSizeInPoints">The 100% baseline font size, in points.</param>
		/// <param name="zoomPercent">The editor zoom, in percent (100 == no zoom).</param>
		/// <returns>The effective metrics with floors applied.</returns>
		public static AutocompletePopupMetrics Compute(float baseFontSizeInPoints, int zoomPercent) {
			float scale = zoomPercent / 100f;

			float fontSize = Math.Clamp(baseFontSizeInPoints * scale, MinFontSizeInPoints, MaxFontSizeInPoints);
			int iconSize = Math.Max((int)Math.Round(BaseIconSize * scale), MinIconSize);
			int gutter = iconSize + 2;
			int maxHeight = (int)Math.Round(BaseMaxHeight * scale);

			return new AutocompletePopupMetrics(fontSize, iconSize, gutter, maxHeight);
		}
	}
}
