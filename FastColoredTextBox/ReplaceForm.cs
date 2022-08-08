using FastColoredTextBoxNS.Input;
using FastColoredTextBoxNS.Types;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FastColoredTextBoxNS {
	public partial class ReplaceForm : Form {
		private readonly FastColoredTextBox tb;
		bool firstSearch = true;
		Place startPlace;

		public ReplaceForm(FastColoredTextBox tb) {
			InitializeComponent();
			this.tb = tb;
		}

		private void BtFindNext_Click(object sender, EventArgs e) {
			try {
				if (!Find(tbFind.Text))
					MessageBox.Show("Not found");
			} catch (Exception ex) { MessageBox.Show(ex.Message); }
		}

		public List<TextSelectionRange> FindAll(string pattern) {
			var opt = cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
			if (!cbRegex.Checked)
				pattern = Regex.Escape(pattern);
			if (cbWholeWord.Checked)
				pattern = "\\b" + pattern + "\\b";
			//
			var range = tb.Selection.IsEmpty ? tb.Range.Clone() : tb.Selection.Clone();
			//
			var list = new List<TextSelectionRange>();
			foreach (var r in range.GetRangesByLines(pattern, opt))
				list.Add(r);

			return list;
		}

		public bool Find(string pattern) {
			RegexOptions opt = cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
			if (!cbRegex.Checked)
				pattern = Regex.Escape(pattern);
			if (cbWholeWord.Checked)
				pattern = "\\b" + pattern + "\\b";
			//
			TextSelectionRange range = tb.Selection.Clone();
			range.Normalize();
			//
			if (firstSearch) {
				startPlace = range.Start;
				firstSearch = false;
			}
			//
			range.Start = range.End;
			if (range.Start >= startPlace)
				tb.Selection.SetStartAndEnd(new Place(0, 0));
			else
				range.End = startPlace;
			//
			foreach (var r in range.GetRangesByLines(pattern, opt)) {
				tb.Selection.Start = r.Start;
				tb.Selection.End = r.End;
				tb.DoSelectionVisible();
				tb.Invalidate();
				return true;
			}
			if (range.Start >= startPlace && startPlace > Place.Empty) {
				tb.Selection.Start = new Place(0, 0);
				return Find(pattern);
			}
			return false;
		}

		private void TbFind_KeyPress(object sender, KeyPressEventArgs e) {
			switch (e.KeyChar) {
				case '\r':
					BtFindNext_Click(sender, null);
					break;
				case '\x1b':
					Hide();
					break;
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
			if (keyData == Keys.Escape) {
				Close();
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void ReplaceForm_FormClosing(object sender, FormClosingEventArgs e) {
			if (e.CloseReason == CloseReason.UserClosing) {
				e.Cancel = true;
				Hide();
			}
			tb.Focus();
		}

		private void BtReplace_Click(object sender, EventArgs e) {
			try {
				if (tb.SelectionLength != 0)
					if (!tb.Selection.ReadOnly)
						tb.InsertText(tbReplace.Text);
				BtFindNext_Click(sender, null);
			} catch (Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}

		private void BtReplaceAll_Click(object sender, EventArgs e) {
			try {
				tb.Selection.BeginUpdate();

				//search
				var ranges = FindAll(tbFind.Text);
				//check readonly
				var ro = false;
				foreach (var r in ranges)
					if (r.ReadOnly) {
						ro = true;
						break;
					}
				//replace
				if (!ro)
					if (ranges.Count > 0) {
						tb.TextSource.Manager.ExecuteCommand(new ReplaceTextCommand(tb.TextSource, ranges, tbReplace.Text));
						tb.Selection.SetStartAndEnd(new Place(0, 0));
					}
				//
				tb.Invalidate();
				MessageBox.Show(ranges.Count + " occurrence(s) replaced");
			} catch (Exception ex) { MessageBox.Show(ex.Message); }
			tb.Selection.EndUpdate();
		}

		protected override void OnActivated(EventArgs e) {
			tbFind.Focus();
			ResetSerach();
		}

		void ResetSerach() => firstSearch = true;
		private void CbMatchCase_CheckedChanged(object sender, EventArgs e) => ResetSerach();
		private void BtClose_Click(object sender, EventArgs e) => Close();
	}
}