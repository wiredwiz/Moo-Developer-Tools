using System.Drawing;
using FastColoredTextBoxNS;
using FluentAssertions;
using Xunit;

namespace FastColoredTextBox.Tests;

/// <summary>
/// Reproduces the GDI+ font lifecycle the autocomplete popup relies on. The popup rebuilds its
/// effective font on every zoom/keystroke and disposes the previous one; these tests pin down
/// whether sharing a <see cref="FontFamily"/> across those fonts is safe.
/// </summary>
public class FontFamilyLifecycleTests {
	// Mirrors the OLD AutocompleteListView.ApplyZoom pattern: every effective font is built from
	// the SAME baseFont.FontFamily instance, and prior effective fonts are disposed.
	[Fact]
	public void SharedFamily_DisposingSibling_ThenReadingHeight_DoesNotThrow() {
		using var baseFont = new Font(FontFamily.GenericSansSerif, 9);

		var n1 = new Font(baseFont.FontFamily, 12, baseFont.Style);
		var n2 = new Font(baseFont.FontFamily, 14, baseFont.Style);

		n1.Dispose(); // popup disposes the previous effective font

		// next effective font built from the same shared family, exactly as ApplyZoom does
		var n3 = new Font(baseFont.FontFamily, 16, baseFont.Style);

		float h2 = 0, h3 = 0;
		System.Action read = () => { h2 = n2.Height; h3 = n3.Height; };
		read.Should().NotThrow("fonts sharing a FontFamily must survive a sibling being disposed");

		h2.Should().BeGreaterThan(0);
		h3.Should().BeGreaterThan(0);

		n2.Dispose();
		n3.Dispose();
	}

	// Replays AutocompleteListView.ApplyZoom's exact loop: each effective font is built from
	// baseFont.FontFamily, swapped in, and the previous effective font disposed; ItemHeight reads
	// Font.Height right after. Crash on "any zoom change" means an iteration's Height throws.
	[Fact]
	public void ApplyZoomLoop_SharedFamily_ReadsHeightEveryIteration() {
		using var baseFont = new Font(FontFamily.GenericSansSerif, 9);
		Font current = baseFont;

		System.Action loop = () => {
			for (int i = 1; i <= 60; i++) {
				var newFont = new Font(baseFont.FontFamily, 9f + i * 0.25f, baseFont.Style);
				var oldFont = current;
				current = newFont;
				if (oldFont != baseFont)
					oldFont.Dispose();
				_ = current.Height; // ItemHeight
			}
		};

		loop.Should().NotThrow();
		if (current != baseFont)
			current.Dispose();
	}

	// Same loop, but each effective font is built from the family NAME (the fix pattern).
	[Fact]
	public void ApplyZoomLoop_NameBuilt_ReadsHeightEveryIteration() {
		using var baseFont = new Font(FontFamily.GenericSansSerif, 9);
		string familyName = baseFont.FontFamily.Name;
		Font current = baseFont;

		System.Action loop = () => {
			for (int i = 1; i <= 60; i++) {
				var newFont = new Font(familyName, 9f + i * 0.25f, baseFont.Style);
				var oldFont = current;
				current = newFont;
				if (oldFont != baseFont)
					oldFont.Dispose();
				_ = current.Height;
			}
		};

		loop.Should().NotThrow();
		if (current != baseFont)
			current.Dispose();
	}

	// Real code path: a live FastColoredTextBox + AutocompleteMenu, zoomed up and down. This drives
	// the actual AutocompleteListView.ApplyZoom through Control.Font, which the bare-Font tests above
	// do not. The reported crash is "any zoom change throws Parameter is not valid".
	[Fact]
	public void ZoomingTextbox_DoesNotCrashAutocompletePopup() {
		using var tb = new FastColoredTextBoxNS.FastColoredTextBox { Text = "verb foo\n  return 1;\nendverb" };
		var menu = new AutocompleteMenu(tb); // wires ZoomChanged + calls ApplyZoom

		System.Action zoom = () => {
			tb.Zoom = 110;
			tb.Zoom = 150;
			tb.Zoom = 90;
			tb.Zoom = 100;
		};

		zoom.Should().NotThrow();
		menu.Dispose();
	}

	// Full reproduction: real Form so the ToolStripDropDown/ToolStripControlHost machinery runs.
	// Showing the autocomplete menu lets the host push its Font onto the listView (whose overridden
	// Font setter resets baseFont); then a zoom change + re-show drives DoAutocomplete -> ApplyZoom.
	[Fact]
	public void ShowMenu_ThenZoom_ThenReshow_DoesNotCrash() {
		RunSta(() => {
			using var form = new System.Windows.Forms.Form();
			using var tb = new FastColoredTextBoxNS.FastColoredTextBox { Text = "verb foo\n  retu", Dock = System.Windows.Forms.DockStyle.Fill };
			form.Controls.Add(tb);
			var menu = new AutocompleteMenu(tb);
			menu.Items.SetAutocompleteItems(new[] { "return", "retry", "result" });
			menu.MinFragmentLength = 1;

			form.Show();
			try {
				tb.GoEnd(); // caret after "retu"

				menu.Show(true);          // first popup; host pushes its font onto the list view
				tb.Zoom = 130;            // any zoom change
				menu.Show(true);          // DoAutocomplete -> ApplyZoom at the new zoom
				tb.Zoom = 80;
				menu.Show(true);
			} finally {
				menu.Dispose();
				form.Close();
			}
		});
	}

	static void RunSta(System.Action action) {
		System.Exception captured = null;
		var t = new System.Threading.Thread(() => {
			try { action(); } catch (System.Exception ex) { captured = ex; }
		});
		t.SetApartmentState(System.Threading.ApartmentState.STA);
		t.Start();
		t.Join();
		if (captured != null)
			throw new Xunit.Sdk.XunitException("Repro threw: " + captured);
	}

	// The FIX pattern: build each effective font from the family NAME so each owns its own family.
	[Fact]
	public void NameBuiltFonts_DisposingSibling_ThenReadingHeight_DoesNotThrow() {
		using var baseFont = new Font(FontFamily.GenericSansSerif, 9);
		string familyName = baseFont.FontFamily.Name;

		var n1 = new Font(familyName, 12, baseFont.Style);
		var n2 = new Font(familyName, 14, baseFont.Style);

		n1.Dispose();

		var n3 = new Font(familyName, 16, baseFont.Style);

		float h2 = 0, h3 = 0;
		System.Action read = () => { h2 = n2.Height; h3 = n3.Height; };
		read.Should().NotThrow();

		h2.Should().BeGreaterThan(0);
		h3.Should().BeGreaterThan(0);

		n2.Dispose();
		n3.Dispose();
	}
}
