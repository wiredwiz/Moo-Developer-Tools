using System;
using System.Collections.Generic;

namespace FastColoredTextBoxNS {
	/// <summary>
	/// Pure ranking logic for the autocomplete popup's focused item. Given the alphabetically-sorted
	/// list of selectable (prefix-matching) item texts and the text the user has typed, it picks the
	/// index of the item to focus: an exact case-insensitive match wins; otherwise the
	/// alphabetically-first item by Text is chosen (the list is already alphabetically sorted upstream).
	/// Kept free of any WinForms types so it can be unit-tested without showing a window.
	/// </summary>
	public static class AutocompleteMatchRanker {
		/// <summary>
		/// Picks the index of the match to focus among the selectable (prefix-matching) items. An exact
		/// case-insensitive match to <paramref name="typedText"/> wins; otherwise the alphabetically-first
		/// item by Text is chosen (the list is already alphabetically sorted upstream); equal texts keep the
		/// earlier (source) order. Returns -1 when there are no selectable items.
		/// </summary>
		/// <param name="selectableTexts">The texts of the selectable prefix-matching items, in source order.</param>
		/// <param name="typedText">The text the user has typed so far.</param>
		/// <returns>The index into <paramref name="selectableTexts"/> of the item to focus, or -1 if empty.</returns>
		public static int SelectBestMatchIndex(IReadOnlyList<string> selectableTexts, string typedText) {
			int best = -1;
			bool bestExact = false;
			string bestText = null;
			for (int i = 0; i < selectableTexts.Count; i++) {
				string t = selectableTexts[i] ?? string.Empty;
				bool exact = string.Equals(t, typedText, StringComparison.InvariantCultureIgnoreCase);
				if (best < 0
					|| (exact && !bestExact)
					|| (exact == bestExact && string.Compare(t, bestText, StringComparison.InvariantCultureIgnoreCase) < 0)) {
					best = i;
					bestExact = exact;
					bestText = t;
				}
			}
			return best;
		}
	}
}
