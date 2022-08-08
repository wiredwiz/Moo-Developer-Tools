using FastColoredTextBoxNS.Types;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FastColoredTextBoxNS {
	public partial class FindForm : Form {
		readonly FastColoredTextBox tb;

		public FindForm(FastColoredTextBox tb) {
			InitializeComponent();
			this.tb = tb;
		}

		private bool SearchRange(string pattern, TextSelectionRange range, RegexOptions opt) {
			foreach (var r in range.GetRangesByLines(pattern, opt)) {
				tb.Selection = r;
				tb.DoSelectionVisible();
				tb.Invalidate();
				return true;
			}

			return false;
		}

		private bool SearchRangeReversed(string pattern, TextSelectionRange range, RegexOptions opt) {
			foreach (var r in range.GetRangesByLinesReversed(pattern, opt)) {
				tb.Selection = r;
				tb.DoSelectionVisible();
				tb.Invalidate();
				return true;
			}

			return false;
		}

		public virtual void FindNext(string pattern) {
			try {
				RegexOptions opt = cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
				if (!cbRegex.Checked)
					pattern = Regex.Escape(pattern);
				if (cbWholeWord.Checked)
					pattern = "\\b" + pattern + "\\b";

				TextSelectionRange selectedRange = tb.Selection.Clone();
				selectedRange.Normalize();

				// Search range after selection
				TextSelectionRange searchRange = new(tb) {
					Start = selectedRange.End,
					End = new Place(tb.GetLineLength(tb.LinesCount - 1), tb.LinesCount - 1)
				};
				if (SearchRange(pattern, searchRange, opt)) { return; }

				// Search range before selection
				searchRange.Start = new Place(0, 0);
				searchRange.End = selectedRange.Start;
				if (SearchRange(pattern, searchRange, opt)) { return; }

				MessageBox.Show("Not found");
			} catch (Exception ex) { MessageBox.Show(ex.Message); }
		}

		public virtual void FindPrev(string pattern) {
			try {
				RegexOptions opt = cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
				if (!cbRegex.Checked)
					pattern = Regex.Escape(pattern);
				if (cbWholeWord.Checked)
					pattern = "\\b" + pattern + "\\b";

				TextSelectionRange selectedRange = tb.Selection.Clone();
				selectedRange.Normalize();

				// Search range before selection
				TextSelectionRange searchRange = new(tb) {
					Start = new Place(0, 0),
					End = selectedRange.Start
				};
				if (SearchRangeReversed(pattern, searchRange, opt)) { return; }

				// Search range after selection
				searchRange.Start = selectedRange.End;
				searchRange.End = new Place(tb.GetLineLength(tb.LinesCount - 1), tb.LinesCount - 1);
				if (SearchRangeReversed(pattern, searchRange, opt)) { return; }

				MessageBox.Show("Not found");
			} catch (Exception ex) { MessageBox.Show(ex.Message); }
		}

		private void TbFind_KeyPress(object sender, KeyPressEventArgs e) {
			switch (e.KeyChar) {
				case '\r':
					btFindNext.PerformClick();
					e.Handled = true;
					return;
				case '\x1b':
					Hide();
					e.Handled = true;
					return;
			}
		}

		private void FindForm_FormClosing(object sender, FormClosingEventArgs e) {
			if (e.CloseReason == CloseReason.UserClosing) {
				e.Cancel = true;
				Hide();
			}
			tb.Focus();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
			switch (keyData) {
				case Keys.Escape:
					Close();
					return true;
				default:
					return base.ProcessCmdKey(ref msg, keyData);
			}
		}

		private void BtClose_Click(object sender, EventArgs e) => Close();
		private void BtFindNext_Click(object sender, EventArgs e) => FindNext(tbFind.Text);
		private void BtnFindPrev_Click(object sender, EventArgs e) => FindPrev(tbFind.Text);
		protected override void OnActivated(EventArgs e) => tbFind.Focus();
	}
}