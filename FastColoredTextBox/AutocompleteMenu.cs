using FastColoredTextBoxNS.Input;
using FastColoredTextBoxNS.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FastColoredTextBoxNS {
	/// <summary>
	/// Popup menu for autocomplete
	/// </summary>
	[Browsable(false)]
	public class AutocompleteMenu : ToolStripDropDown, IDisposable {
		readonly AutocompleteListView listView;
		public ToolStripControlHost host;
		public TextSelectionRange Fragment { get; internal set; }

		/// <summary>
		/// Regex pattern for serach fragment around caret
		/// </summary>
		public string SearchPattern { get; set; }
		/// <summary>
		/// Minimum fragment length for popup
		/// </summary>
		public int MinFragmentLength { get; set; }
		/// <summary>
		/// User selects item
		/// </summary>
		public event EventHandler<SelectingEventArgs> Selecting;
		/// <summary>
		/// It fires after item inserting
		/// </summary>
		public event EventHandler<SelectedEventArgs> Selected;
		/// <summary>
		/// Occurs when popup menu is opening
		/// </summary>
		public new event EventHandler<CancelEventArgs> Opening;
		/// <summary>
		/// Allow TAB for select menu item
		/// </summary>
		public bool AllowTabKey { get { return listView.AllowTabKey; } set { listView.AllowTabKey = value; } }
		/// <summary>
		/// Interval of menu appear (ms)
		/// </summary>
		public int AppearInterval { get { return listView.AppearInterval; } set { listView.AppearInterval = value; } }
		/// <summary>
		/// Sets the max tooltip window size
		/// </summary>
		public Size MaxTooltipSize { get { return listView.MaxToolTipSize; } set { listView.MaxToolTipSize = value; } }
		/// <summary>
		/// Tooltip will perm show and duration will be ignored
		/// </summary>
		public bool AlwaysShowTooltip { get { return listView.AlwaysShowTooltip; } set { listView.AlwaysShowTooltip = value; } }

		/// <summary>
		/// Back color of selected item
		/// </summary>
		[DefaultValue(typeof(Color), "Orange")]
		public Color SelectedColor {
			get { return listView.SelectedColor; }
			set { listView.SelectedColor = value; }
		}

		/// <summary>
		/// Border color of hovered item
		/// </summary>
		[DefaultValue(typeof(Color), "Red")]
		public Color HoveredColor {
			get { return listView.HoveredColor; }
			set { listView.HoveredColor = value; }
		}

		public AutocompleteMenu(FastColoredTextBox tb) {
			// create a new popup and add the list view to it 
			AutoClose = false;
			AutoSize = false;
			Margin = Padding.Empty;
			Padding = Padding.Empty;
			BackColor = Color.White;
			listView = new AutocompleteListView(tb);
			host = new ToolStripControlHost(listView) {
				Margin = new Padding(2, 2, 2, 2),
				Padding = Padding.Empty,
				AutoSize = false,
				AutoToolTip = false
			};
			CalcSize();
			base.Items.Add(host);
			listView.Parent = this;
			SearchPattern = @"[\w\.]";
			MinFragmentLength = 2;

		}

		public new Font Font {
			get { return listView.Font; }
			set { listView.Font = value; }
		}

		new internal void OnOpening(CancelEventArgs args) {
			Opening?.Invoke(this, args);
		}

		public new void Close() {
			listView.toolTip.Hide(listView);
			base.Close();
		}

		internal void CalcSize() {
			host.Size = listView.Size;
			Size = new System.Drawing.Size(listView.Size.Width + 4, listView.Size.Height + 4);
		}

		public virtual void OnSelecting() {
			listView.OnSelecting();
		}

		public void SelectNext(int shift) {
			listView.SelectNext(shift);
		}

		internal void OnSelecting(SelectingEventArgs args) {
			Selecting?.Invoke(this, args);
		}

		public void OnSelected(SelectedEventArgs args) {
			Selected?.Invoke(this, args);
		}

		public new AutocompleteListView Items {
			get { return listView; }
		}

		/// <summary>
		/// Shows popup menu immediately
		/// </summary>
		/// <param name="forced">If True - MinFragmentLength will be ignored</param>
		public void Show(bool forced) {
			Items.DoAutocomplete(forced);
		}

		/// <summary>
		/// Minimal size of menu
		/// </summary>
		public new Size MinimumSize {
			get { return Items.MinimumSize; }
			set { Items.MinimumSize = value; }
		}

		/// <summary>
		/// Image list of menu
		/// </summary>
		public new ImageList ImageList {
			get { return Items.ImageList; }
			set { Items.ImageList = value; }
		}

		/// <summary>
		/// Tooltip duration (ms)
		/// </summary>
		public int ToolTipDuration {
			get { return Items.ToolTipDuration; }
			set { Items.ToolTipDuration = value; }
		}

		/// <summary>
		/// Tooltip
		/// </summary>
		public ToolTip ToolTip {
			get { return Items.toolTip; }
			set { Items.toolTip = value; }
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			if (listView != null && !listView.IsDisposed)
				listView.Dispose();
		}
	}

	[System.ComponentModel.ToolboxItem(false)]
	public class AutocompleteListView : UserControl, IDisposable {
		public event EventHandler FocussedItemIndexChanged;

		internal List<AutocompleteItem> visibleItems;
		IEnumerable<AutocompleteItem> sourceItems = new List<AutocompleteItem>();
		int focussedItemIndex = 0;
		readonly int hoveredItemIndex = -1;

		private int ItemHeight {
			get {
				try {
					return Font.Height + 2;
				} catch (ArgumentException) {
					//defensive: a popup font hiccup must never crash the host editor; fall back to an
					//approximate row height derived from the baseline point size (≈1.3 px per point).
					return (int)Math.Ceiling(baseFontSizeInPoints * 1.3f) + 2;
				}
			}
		}

		AutocompleteMenu Menu { get { return Parent as AutocompleteMenu; } }
		int oldItemCount = 0;
		readonly FastColoredTextBox tb;
		internal ToolTip toolTip = new();
		readonly Timer timer = new();

		/// <summary>
		/// The font assigned to the list view (the 100% zoom baseline). Kept separate from the
		/// effective <see cref="Control.Font"/>, which is the baseline scaled by the editor zoom.
		/// </summary>
		Font baseFont;

		// Immutable copies of the baseline font's identity, captured whenever baseFont is assigned.
		// The effective (zoomed) font is rebuilt from the family NAME, not from baseFont.FontFamily:
		// when the ToolStripControlHost pushes its own Font onto this control (through the Font
		// setter) and WinForms later disposes that pushed font, anything sharing its FontFamily is
		// invalidated and Font.Height throws "Parameter is not valid". Building from the name gives
		// each effective font its own family, immune to that disposal.
		string baseFontFamilyName = FontFamily.GenericSansSerif.Name;
		float baseFontSizeInPoints = 9f;
		FontStyle baseFontStyle = FontStyle.Regular;

		internal bool AllowTabKey { get; set; }
		public ImageList ImageList { get; set; }
		internal int AppearInterval { get { return timer.Interval; } set { timer.Interval = value; } }
		internal int ToolTipDuration { get; set; }
		internal Size MaxToolTipSize { get; set; }
		internal bool AlwaysShowTooltip {
			get { return toolTip.ShowAlways; }
			set { toolTip.ShowAlways = value; }
		}

		public Color SelectedColor { get; set; }
		public Color HoveredColor { get; set; }
		public int FocussedItemIndex {
			get { return focussedItemIndex; }
			set {
				if (focussedItemIndex != value) {
					focussedItemIndex = value;
					FocussedItemIndexChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public AutocompleteItem FocussedItem {
			get {
				if (FocussedItemIndex >= 0 && focussedItemIndex < visibleItems.Count)
					return visibleItems[focussedItemIndex];
				return null;
			}
			set {
				FocussedItemIndex = visibleItems.IndexOf(value);
			}
		}

		internal AutocompleteListView(FastColoredTextBox tb) {
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
			baseFont = new Font(FontFamily.GenericSansSerif, 9);
			CaptureBaseFontIdentity();
			base.Font = baseFont;
			visibleItems = new List<AutocompleteItem>();
			VerticalScroll.SmallChange = ItemHeight;
			MaximumSize = new Size(Size.Width, AutocompletePopupMetrics.BaseMaxHeight);
			toolTip.ShowAlways = false;
			AppearInterval = 500;
			timer.Tick += new EventHandler(Timer_Tick);
			SelectedColor = Color.Orange;
			HoveredColor = Color.Red;
			ToolTipDuration = 3000;
			toolTip.Popup += ToolTip_Popup;

			this.tb = tb;

			tb.KeyDown += new KeyEventHandler(Tb_KeyDown);
			tb.SelectionChanged += new EventHandler(Tb_SelectionChanged);
			tb.KeyPressed += new KeyPressEventHandler(Tb_KeyPressed);
			tb.ZoomChanged += new EventHandler(Tb_ZoomChanged);

			//apply the editor's current zoom to the freshly-built effective font
			ApplyZoom();

			Form form = tb.FindForm();
			if (form != null) {
				form.LocationChanged += delegate { SafetyClose(); };
				form.ResizeBegin += delegate { SafetyClose(); };
				form.FormClosing += delegate { SafetyClose(); };
				form.LostFocus += delegate { SafetyClose(); };
			}

			tb.LostFocus += (o, e) => {
				if (Menu != null && !Menu.IsDisposed)
					if (!Menu.Focused)
						SafetyClose();
			};

			tb.Scroll += delegate { SafetyClose(); };

			this.VisibleChanged += (o, e) => {
				if (this.Visible)
					DoSelectedVisible();
			};
		}

		/// <summary>
		/// The font assigned to the popup. Setting it updates the 100% baseline; the effective font
		/// shown is this scaled by the current editor zoom (see <see cref="ApplyZoom"/>).
		/// </summary>
		public override Font Font {
			get { return base.Font; }
			set {
				baseFont = value;
				CaptureBaseFontIdentity();
				ApplyZoom();
			}
		}

		/// <summary>
		/// Snapshots the baseline font's family name, point size and style into immutable fields
		/// while <see cref="baseFont"/> is freshly assigned (and still valid), so the effective font
		/// can later be rebuilt without touching a font WinForms may have disposed.
		/// </summary>
		void CaptureBaseFontIdentity() {
			if (baseFont == null)
				return;
			baseFontFamilyName = baseFont.FontFamily.Name;
			baseFontSizeInPoints = baseFont.SizeInPoints;
			baseFontStyle = baseFont.Style;
		}

		void Tb_ZoomChanged(object sender, EventArgs e) {
			ApplyZoom();
		}

		/// <summary>
		/// Recomputes the effective font from the base font and the editor's current zoom, then,
		/// if the menu is visible, recalculates size and repositions so it tracks the zoom live.
		/// </summary>
		void ApplyZoom() {
			var metrics = AutocompletePopupMetrics.Compute(baseFontSizeInPoints, tb.Zoom);

			Font newFont;
			try {
				//build from the family NAME so the effective font owns its own family and survives
				//WinForms disposing whatever font it pushed onto us as the baseline (see field notes)
				newFont = new Font(baseFontFamilyName, metrics.FontSizeInPoints, baseFontStyle);
			} catch (ArgumentException) {
				//never let popup font sizing crash the host editor
				return;
			}

			//Control.Font compares by VALUE: assigning a font equal to the current one is a no-op and
			//leaves the existing object in place. The previous code then disposed that still-live object
			//(it differs by reference from baseFont), so the next paint drew with a disposed font and
			//threw "Parameter is not valid". Only swap+dispose when the new font actually differs.
			var oldFont = base.Font;
			if (newFont.Equals(oldFont)) {
				newFont.Dispose(); //redundant; keep the font already in use
			} else {
				base.Font = newFont;
				//do not dispose the baseline font (it backs the 100% baseline)
				if (oldFont != null && oldFont != baseFont)
					oldFont.Dispose();
			}

			//scale the max-height cap so a comparable number of the (now taller) rows stays visible
			MaximumSize = new Size(MaximumSize.Width, metrics.MaxHeight);

			VerticalScroll.SmallChange = ItemHeight;

			if (Menu != null && !Menu.IsDisposed && Menu.Visible) {
				oldItemCount = -1; //force AdjustScroll to recompute the height at the new item size
				AdjustScroll();
				Invalidate();
			}
		}

		private void ToolTip_Popup(object sender, PopupEventArgs e) {
			if (MaxToolTipSize.Height > 0 && MaxToolTipSize.Width > 0)
				e.ToolTipSize = MaxToolTipSize;
		}

		protected override void Dispose(bool disposing) {
			if (toolTip != null) {
				toolTip.Popup -= ToolTip_Popup;
				toolTip.Dispose();
			}
			if (tb != null) {
				tb.KeyDown -= Tb_KeyDown;
				tb.KeyPressed -= Tb_KeyPressed;
				tb.SelectionChanged -= Tb_SelectionChanged;
				tb.ZoomChanged -= Tb_ZoomChanged;
			}

			if (timer != null) {
				timer.Stop();
				timer.Tick -= Timer_Tick;
				timer.Dispose();
			}

			base.Dispose(disposing);
		}

		void SafetyClose() {
			if (Menu != null && !Menu.IsDisposed)
				Menu.Close();
		}

		void Tb_KeyPressed(object sender, KeyPressEventArgs e) {
			bool backspaceORdel = e.KeyChar == '\b' || e.KeyChar == 0xff;

			/*
            if (backspaceORdel)
                prevSelection = tb.Selection.Start;*/

			if (Menu.Visible && !backspaceORdel)
				DoAutocomplete(false);
			else
				ResetTimer(timer);
		}

		void Timer_Tick(object sender, EventArgs e) {
			timer.Stop();
			DoAutocomplete(false);
		}

		static void ResetTimer(Timer timer) {
			timer.Stop();
			timer.Start();
		}

		internal void DoAutocomplete() {
			DoAutocomplete(false);
		}

		internal void DoAutocomplete(bool forced) {
			if (!Menu.Enabled) {
				Menu.Close();
				return;
			}

			//re-apply zoom in case it changed while the menu was closed
			ApplyZoom();

			visibleItems.Clear();
			FocussedItemIndex = 0;
			VerticalScroll.Value = 0;
			//some magic for update scrolls
			AutoScrollMinSize -= new Size(1, 0);
			AutoScrollMinSize += new Size(1, 0);
			//get fragment around caret
			TextSelectionRange fragment = tb.Selection.GetFragment(Menu.SearchPattern);
			string text = fragment.Text;
			//calc screen point for popup menu
			Point point = tb.PlaceToPoint(fragment.End);
			point.Offset(2, tb.CharHeight);
			//
			if (forced || (text.Length >= Menu.MinFragmentLength
				&& tb.Selection.IsEmpty /*pops up only if selected range is empty*/
				&& (tb.Selection.Start > fragment.Start || text.Length == 0/*pops up only if caret is after first letter*/))) {
				Menu.Fragment = fragment;
				//build popup menu, ranking the focused item: exact match first, then alphabetically-first (the list
				//is already alphabetically sorted upstream), then source order
				var selectableTexts = new List<string>();
				var selectableVisibleIndexes = new List<int>();
				foreach (var item in sourceItems) {
					item.Parent = Menu;
					CompareResult res = item.Compare(text);
					if (res == CompareResult.Hidden)
						continue;
					visibleItems.Add(item);
					if (res == CompareResult.VisibleAndSelected) {
						selectableVisibleIndexes.Add(visibleItems.Count - 1);
						selectableTexts.Add(item.Text);
					}
				}
				int pick = AutocompleteMatchRanker.SelectBestMatchIndex(selectableTexts, text);
				if (pick >= 0)
					FocussedItemIndex = selectableVisibleIndexes[pick];

				//if nothing offers more than what is already typed, do not show the menu
				if (visibleItems.Count > 0) {
					bool nothingToComplete = true;
					foreach (var item in visibleItems)
						if (!string.Equals(item.GetTextForReplace(), text, StringComparison.InvariantCultureIgnoreCase)) {
							nothingToComplete = false;
							break;
						}
					if (nothingToComplete && !forced) {
						Menu.Close();
						return;
					}
				}
			}

			//show popup menu
			if (Count > 0) {
				if (!Menu.Visible) {
					CancelEventArgs args = new();
					Menu.OnOpening(args);
					if (!args.Cancel)
						Menu.Show(tb, point);
				}

				DoSelectedVisible();
				Invalidate();
			} else
				Menu.Close();
		}

		void Tb_SelectionChanged(object sender, EventArgs e) {
			/*
            FastColoredTextBox tb = sender as FastColoredTextBox;
            
            if (Math.Abs(prevSelection.iChar - tb.Selection.Start.iChar) > 1 ||
                        prevSelection.iLine != tb.Selection.Start.iLine)
                Menu.Close();
            prevSelection = tb.Selection.Start;*/
			if (Menu.Visible) {
				bool needClose = false;

				if (!tb.Selection.IsEmpty)
					needClose = true;
				else
					if (!Menu.Fragment.Contains(tb.Selection.Start)) {
					if (tb.Selection.Start.iLine == Menu.Fragment.End.iLine && tb.Selection.Start.iChar == Menu.Fragment.End.iChar + 1) {
						//user press key at end of fragment
						char c = tb.Selection.CharBeforeStart;
						if (!Regex.IsMatch(c.ToString(), Menu.SearchPattern))//check char
							needClose = true;
					} else
						needClose = true;
				}

				if (needClose)
					Menu.Close();
			}

		}

		void Tb_KeyDown(object sender, KeyEventArgs e) {
			var tb = sender as FastColoredTextBox;

			if (Menu.Visible)
				if (ProcessKey(e.KeyCode, e.Modifiers))
					e.Handled = true;

			if (!Menu.Visible) {
				if (tb.HotkeysMapping.ContainsKey(e.KeyData) && tb.HotkeysMapping[e.KeyData] == FCTBAction.AutocompleteMenu) {
					DoAutocomplete();
					e.Handled = true;
				} else {
					if (e.KeyCode == Keys.Escape && timer.Enabled)
						timer.Stop();
				}
			}
		}

		void AdjustScroll() {
			if (oldItemCount == visibleItems.Count)
				return;

			int needHeight = ItemHeight * visibleItems.Count + 1;
			Height = Math.Min(needHeight, MaximumSize.Height);
			Menu.CalcSize();

			AutoScrollMinSize = new Size(0, needHeight);
			oldItemCount = visibleItems.Count;
		}

		protected override void OnPaint(PaintEventArgs e) {
			AdjustScroll();

			var itemHeight = ItemHeight;
			int startI = VerticalScroll.Value / itemHeight - 1;
			int finishI = (VerticalScroll.Value + ClientSize.Height) / itemHeight + 1;
			startI = Math.Max(startI, 0);
			finishI = Math.Min(finishI, visibleItems.Count);
			//scale the icon and the text/icon gutter with the editor zoom
			var metrics = AutocompletePopupMetrics.Compute(baseFontSizeInPoints, tb.Zoom);
			int iconSize = metrics.IconSize;
			int leftPadding = metrics.Gutter;
			//downscale the 64px source icons crisply
			e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			for (int i = startI; i < finishI; i++) {
				int y = i * itemHeight - VerticalScroll.Value;

				var item = visibleItems[i];

				if (item.BackColor != Color.Transparent)
					using (var brush = new SolidBrush(item.BackColor))
						e.Graphics.FillRectangle(brush, 1, y, ClientSize.Width - 1 - 1, itemHeight - 1);

				if (ImageList != null && visibleItems[i].ImageIndex >= 0)
					e.Graphics.DrawImage(ImageList.Images[item.ImageIndex], new Rectangle(1, y + (itemHeight - iconSize) / 2, iconSize, iconSize));

				if (i == FocussedItemIndex)
					using (var selectedBrush = new LinearGradientBrush(new Point(0, y - 3), new Point(0, y + itemHeight), Color.Transparent, SelectedColor))
					using (var pen = new Pen(SelectedColor)) {
						e.Graphics.FillRectangle(selectedBrush, leftPadding, y, ClientSize.Width - 1 - leftPadding, itemHeight - 1);
						e.Graphics.DrawRectangle(pen, leftPadding, y, ClientSize.Width - 1 - leftPadding, itemHeight - 1);
					}

				if (i == hoveredItemIndex)
					using (var pen = new Pen(HoveredColor))
						e.Graphics.DrawRectangle(pen, leftPadding, y, ClientSize.Width - 1 - leftPadding, itemHeight - 1);

				using (var brush = new SolidBrush(item.ForeColor != Color.Transparent ? item.ForeColor : ForeColor))
					e.Graphics.DrawString(item.ToString(), Font, brush, leftPadding, y);
			}
		}

		protected override void OnScroll(ScrollEventArgs se) {
			base.OnScroll(se);
			Invalidate();
		}

		protected override void OnMouseClick(MouseEventArgs e) {
			base.OnMouseClick(e);

			if (e.Button == System.Windows.Forms.MouseButtons.Left) {
				FocussedItemIndex = PointToItemIndex(e.Location);
				DoSelectedVisible();
				Invalidate();
			}
		}

		protected override void OnMouseDoubleClick(MouseEventArgs e) {
			base.OnMouseDoubleClick(e);
			FocussedItemIndex = PointToItemIndex(e.Location);
			Invalidate();
			OnSelecting();
		}

		internal virtual void OnSelecting() {
			if (FocussedItemIndex < 0 || FocussedItemIndex >= visibleItems.Count)
				return;
			tb.TextSource.Manager.BeginAutoUndoCommands();
			try {
				AutocompleteItem item = FocussedItem;
				SelectingEventArgs args = new() {
					Item = item,
					SelectedIndex = FocussedItemIndex
				};

				Menu.OnSelecting(args);

				if (args.Cancel) {
					FocussedItemIndex = args.SelectedIndex;
					Invalidate();
					return;
				}

				if (!args.Handled) {
					var fragment = Menu.Fragment;
					DoAutocomplete(item, fragment);
				}

				Menu.Close();
				//
				SelectedEventArgs args2 = new() {
					Item = item,
					Tb = Menu.Fragment.tb
				};
				item.OnSelected(Menu, args2);
				Menu.OnSelected(args2);
			} finally {
				tb.TextSource.Manager.EndAutoUndoCommands();
			}
		}

		private static void DoAutocomplete(AutocompleteItem item, TextSelectionRange fragment) {
			string newText = item.GetTextForReplace();

			//replace text of fragment
			var tb = fragment.tb;

			tb.BeginAutoUndo();
			tb.TextSource.Manager.ExecuteCommand(new SelectCommand(tb.TextSource));
			if (tb.Selection.ColumnSelectionMode) {
				var start = tb.Selection.Start;
				var end = tb.Selection.End;
				start.iChar = fragment.Start.iChar;
				end.iChar = fragment.End.iChar;
				tb.Selection.Start = start;
				tb.Selection.End = end;
			} else {
				tb.Selection.Start = fragment.Start;
				tb.Selection.End = fragment.End;
			}
			tb.InsertText(newText);
			tb.TextSource.Manager.ExecuteCommand(new SelectCommand(tb.TextSource));
			tb.EndAutoUndo();
			tb.Focus();
		}

		int PointToItemIndex(Point p) {
			return (p.Y + VerticalScroll.Value) / ItemHeight;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
			ProcessKey(keyData, Keys.None);

			return base.ProcessCmdKey(ref msg, keyData);
		}

		private bool ProcessKey(Keys keyData, Keys keyModifiers) {
			if (keyModifiers == Keys.None)
				switch (keyData) {
					case Keys.Down:
						SelectNext(+1);
						return true;
					case Keys.PageDown:
						SelectNext(+10);
						return true;
					case Keys.Up:
						SelectNext(-1);
						return true;
					case Keys.PageUp:
						SelectNext(-10);
						return true;
					case Keys.Enter:
						OnSelecting();
						return true;
					case Keys.Tab:
						if (!AllowTabKey)
							break;
						OnSelecting();
						return true;
					case Keys.Escape:
						Menu.Close();
						return true;
				}

			return false;
		}

		public void SelectNext(int shift) {
			FocussedItemIndex = Math.Max(0, Math.Min(FocussedItemIndex + shift, visibleItems.Count - 1));
			DoSelectedVisible();
			//
			Invalidate();
		}

		private void DoSelectedVisible() {
			if (FocussedItem != null)
				SetToolTip(FocussedItem);

			var y = FocussedItemIndex * ItemHeight - VerticalScroll.Value;
			if (y < 0)
				VerticalScroll.Value = FocussedItemIndex * ItemHeight;
			if (y > ClientSize.Height - ItemHeight)
				VerticalScroll.Value = Math.Min(VerticalScroll.Maximum, FocussedItemIndex * ItemHeight - ClientSize.Height + ItemHeight);
			//some magic for update scrolls
			AutoScrollMinSize -= new Size(1, 0);
			AutoScrollMinSize += new Size(1, 0);
		}

		private void SetToolTip(AutocompleteItem autocompleteItem) {
			var title = autocompleteItem.ToolTipTitle;
			var text = autocompleteItem.ToolTipText;

			if (string.IsNullOrEmpty(title)) {
				toolTip.ToolTipTitle = null;
				toolTip.SetToolTip(this, null);
				return;
			}

			if (this.Parent != null) {
				IWin32Window window = this.Parent ?? this;
				Point location;

				if ((this.PointToScreen(this.Location).X + MaxToolTipSize.Width + 105) < Screen.FromControl(this.Parent).WorkingArea.Right)
					location = new Point(Right + 5, 0);
				else
					location = new Point(Left - 105 - MaximumSize.Width, 0);

				if (string.IsNullOrEmpty(text)) {
					toolTip.ToolTipTitle = null;
					toolTip.Show(title, window, location.X, location.Y, ToolTipDuration);
				} else {
					toolTip.ToolTipTitle = title;
					toolTip.Show(text, window, location.X, location.Y, ToolTipDuration);
				}
			}
		}

		public int Count {
			get { return visibleItems.Count; }
		}

		public void SetAutocompleteItems(ICollection<string> items) {
			List<AutocompleteItem> list = new(items.Count);
			foreach (var item in items)
				list.Add(new AutocompleteItem(item));
			SetAutocompleteItems(list);
		}

		public void SetAutocompleteItems(IEnumerable<AutocompleteItem> items) {
			sourceItems = items;
		}
	}

	public class SelectingEventArgs : EventArgs {
		public AutocompleteItem Item { get; internal set; }
		public bool Cancel { get; set; }
		public int SelectedIndex { get; set; }
		public bool Handled { get; set; }
	}

	public class SelectedEventArgs : EventArgs {
		public AutocompleteItem Item { get; internal set; }
		public FastColoredTextBox Tb { get; set; }
	}
}
