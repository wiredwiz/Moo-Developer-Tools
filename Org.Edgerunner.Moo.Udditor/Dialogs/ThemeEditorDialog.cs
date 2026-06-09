#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="ThemeEditorDialog.cs">
// Copyright (c) Thaddeus Ryker 2026
// </copyright>
//
// BSD 3-Clause License
//
// Copyright (c) 2026,
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

using Krypton.Toolkit;
using Org.Edgerunner.Moo.Editor.Configuration;
using Org.Edgerunner.Moo.Editor.Controls;

namespace Org.Edgerunner.Moo.Udditor.Dialogs
{
   /// <summary>
   /// Modal dialog that edits the editor color theme with an isolated live preview.
   /// </summary>
   /// <remarks>
   /// The dialog edits a clone of <see cref="Settings.Instance"/>. Open editors are untouched until
   /// the user clicks Apply or OK, at which point the working copy is persisted to the active config
   /// file, copied into the singleton, and applied to all open editors and the parser-message panel.
   /// </remarks>
   public partial class ThemeEditorDialog : KryptonForm
   {
      private const string PreviewResourceName = "ThemePreviewSample.moo";

      private readonly WindowManager _manager;
      private readonly string _configPath;

      /// <summary>The shared dark custom palette supplied by the main form, used for dialog-local and global dark preview.</summary>
      private readonly KryptonCustomPaletteBase _darkPalette;

      /// <summary>The application's global Krypton manager, committed to on Apply/OK.</summary>
      private readonly KryptonManager _kryptonManager;

      /// <summary>Suppresses the mode-dropdown handler while it is being populated programmatically.</summary>
      private bool _suppressModeChange;

      /// <summary>The working copy of the settings being edited.</summary>
      private readonly Settings _working;

      private MooCodeEditor _preview;

      /// <summary>Maps a background swatch control to a "make transparent" button so we can toggle state.</summary>
      private readonly Dictionary<Control, Action> _refreshers = new();

      /// <summary>
      /// Describes a single syntax-token row: its label and the property accessors it edits.
      /// </summary>
      private sealed class TokenRowDefinition
      {
         public string Label;
         public Func<Color> GetForeground;
         public Action<Color> SetForeground;
         public Func<Color> GetBackground;
         public Action<Color> SetBackground;
         public Func<FontStyle> GetFontStyle;
         public Action<FontStyle> SetFontStyle;
      }

      /// <summary>
      /// Describes a single editor-chrome row: a label and one color accessor.
      /// </summary>
      private sealed class ChromeRowDefinition
      {
         public string Label;
         public Func<Color> GetColor;
         public Action<Color> SetColor;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="ThemeEditorDialog"/> class.
      /// </summary>
      /// <param name="manager">The window manager used to apply the theme to open editors.</param>
      /// <param name="configPath">The path the working theme is persisted to on Apply/OK.</param>
      /// <param name="darkPalette">The shared dark custom palette used for the dialog-local and global dark preview.</param>
      /// <param name="kryptonManager">The application's global Krypton manager, committed to on Apply/OK.</param>
      public ThemeEditorDialog(WindowManager manager, string configPath, KryptonCustomPaletteBase darkPalette, KryptonManager kryptonManager)
      {
         _manager = manager;
         _configPath = configPath;
         _darkPalette = darkPalette;
         _kryptonManager = kryptonManager;
         _working = Settings.Instance.Clone();

         InitializeComponent();
         BuildPreview();
         BuildLeftControls();
         InitializeModeDropdown();
         ApplyDialogPalette();

         // Auto-indent selects the whole sample while the text loads. Collapse the selection once
         // the form is shown, deferred to the end of the message queue so it runs after any pending
         // auto-indent so the preview never opens with everything highlighted.
         Shown += (_, _) => BeginInvoke((Action)(() => _preview.CollapseSelectionToStart()));
      }

      private void BuildPreview()
      {
         _preview = new MooCodeEditor(_working.DefaultGrammarDialect, _working)
         {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            LineInterval = 4,
            BorderStyle = BorderStyle.Fixed3D
         };
         previewHostPanel.Controls.Add(_preview);
         _preview.Text = LoadPreviewSample();
         ApplyPreviewChrome();
         _preview.ShowCodeFolding = true;
         _preview.IsChanged = false;
         _preview.ClearUndo();
      }

      private static string LoadPreviewSample()
      {
         try
         {
            using var stream = typeof(ThemeEditorDialog).Assembly.GetManifestResourceStream(PreviewResourceName);
            if (stream == null)
               return "\"Theme preview sample could not be loaded.\";";

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
         }
         catch (Exception)
         {
            return "\"Theme preview sample could not be loaded.\";";
         }
      }

      private void ApplyPreviewChrome()
      {
         _preview.Font = new Font(_working.EditorFontFamily, _working.EditorFontSize);
         _preview.ForeColor = _working.EditorTextColor;
         _preview.BackColor = _working.EditorBackgroundColor;
         _preview.CaretColor = _working.EditorCaretColor;
         _preview.CurrentLineColor = _working.EditorCurrentLineColor;
         _preview.LineNumberColor = _working.EditorLineNumberColor;
         _preview.SelectionColor = _working.EditorTextSelectionColor;
         _preview.ChangedLineColor = _working.EditorChangedLineColor;
         _preview.FoldingIndicatorColor = _working.EditorFoldingIndicatorColor;
         _preview.IndentBackColor = _working.EditorIndentBackColor;
         _preview.BookmarkColor = _working.EditorBookmarkColor;
         _preview.ServiceLinesColor = _working.EditorServiceLineColor;
         _preview.FoldingHighlightColor = _working.EditorFoldingHighlightColor;
      }

      /// <summary>
      /// Re-applies the working theme to the preview editor after any color or style change.
      /// </summary>
      private void RefreshPreview()
      {
         ApplyPreviewChrome();
         _preview.RefreshTheme();
         _preview.Invalidate();
      }

      private void BuildLeftControls()
      {
         var tokenRows = BuildTokenRowDefinitions();
         var chromeRows = BuildChromeRowDefinitions();

         var layout = new KryptonTableLayoutPanel
         {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            Padding = new Padding(8),
            GrowStyle = TableLayoutPanelGrowStyle.AddRows
         };
         layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

         layout.Controls.Add(BuildSyntaxGroup(tokenRows));
         layout.Controls.Add(BuildChromeGroup(chromeRows));

         leftScrollPanel.Controls.Add(layout);
      }

      private static KryptonLabel HeaderLabel(string text)
      {
         var label = new KryptonLabel { AutoSize = true, Anchor = AnchorStyles.Left };
         label.Values.Text = text;
         return label;
      }

      private static KryptonLabel RowLabel(string text)
      {
         var label = new KryptonLabel { AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 6, 3, 3) };
         label.Values.Text = text;
         return label;
      }

      private KryptonGroupBox BuildSyntaxGroup(IList<TokenRowDefinition> rows)
      {
         var table = new KryptonTableLayoutPanel
         {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 4,
            Padding = new Padding(6),
            GrowStyle = TableLayoutPanelGrowStyle.AddRows
         };
         table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130f));
         table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
         table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
         table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

         // Header row
         table.Controls.Add(HeaderLabel("Token"));
         table.Controls.Add(HeaderLabel("Foreground"));
         table.Controls.Add(HeaderLabel("Background"));
         table.Controls.Add(HeaderLabel("Style"));

         foreach (var row in rows)
         {
            table.Controls.Add(RowLabel(row.Label));
            table.Controls.Add(CreateForegroundSwatch(row.GetForeground, row.SetForeground));
            table.Controls.Add(CreateBackgroundSwatch(row.GetBackground, row.SetBackground));
            table.Controls.Add(CreateFontStyleControl(row.GetFontStyle, row.SetFontStyle));
         }

         var group = new KryptonGroupBox
         {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(3)
         };
         group.Values.Heading = "Syntax";
         group.Panel.Controls.Add(table);
         return group;
      }

      private KryptonGroupBox BuildChromeGroup(IList<ChromeRowDefinition> rows)
      {
         var table = new KryptonTableLayoutPanel
         {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Padding = new Padding(6),
            GrowStyle = TableLayoutPanelGrowStyle.AddRows
         };
         table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200f));
         table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

         foreach (var row in rows)
         {
            table.Controls.Add(RowLabel(row.Label));
            table.Controls.Add(CreateForegroundSwatch(row.GetColor, row.SetColor));
         }

         var group = new KryptonGroupBox
         {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(3)
         };
         group.Values.Heading = "Editor chrome";
         group.Panel.Controls.Add(table);
         return group;
      }

      /// <summary>
      /// Creates a swatch button that opens a <see cref="ColorDialog"/> on click (opaque colors only).
      /// </summary>
      private Control CreateForegroundSwatch(Func<Color> get, Action<Color> set)
      {
         var swatch = CreateColorChip();

         void Refresh() => SetChipColor(swatch, get());
         _refreshers[swatch] = Refresh;
         Refresh();

         swatch.Click += (_, _) =>
         {
            using var dialog = new ColorDialog { Color = get(), FullOpen = true, AnyColor = true };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
               set(dialog.Color);
               Refresh();
               RefreshPreview();
            }
         };

         return swatch;
      }

      /// <summary>
      /// Creates a flat <see cref="KryptonButton"/> color chip that paints a solid color regardless of palette.
      /// </summary>
      private static KryptonButton CreateColorChip()
      {
         var swatch = new KryptonButton
         {
            Width = 60,
            Height = 24,
            Margin = new Padding(3)
         };
         swatch.StateCommon.Back.ColorStyle = Krypton.Toolkit.PaletteColorStyle.Solid;
         return swatch;
      }

      /// <summary>
      /// Paints a color chip with the supplied color (solid draw style) and a neutral fill when transparent.
      /// </summary>
      private static void SetChipColor(KryptonButton swatch, Color color)
      {
         var isTransparent = color.A == 0;
         var fill = isTransparent ? SystemColors.Control : color;
         swatch.StateCommon.Back.Color1 = fill;
         swatch.StateCommon.Back.Color2 = fill;
         swatch.Values.Text = isTransparent ? "(none)" : string.Empty;
      }

      /// <summary>
      /// Creates a background swatch with both a color picker and a "Transparent" affordance,
      /// since <see cref="ColorDialog"/> cannot express transparency.
      /// </summary>
      private Control CreateBackgroundSwatch(Func<Color> get, Action<Color> set)
      {
         // A small transparent-background KryptonPanel holds the chip + checkbox so the themed parent
         // shows through (KryptonPanel has no flow layout, so the children are positioned manually).
         var container = new KryptonPanel
         {
            Size = new Size(200, 30),
            Margin = new Padding(0)
         };
         container.StateCommon.Color1 = Color.Transparent;
         container.StateCommon.Color2 = Color.Transparent;

         var swatch = CreateColorChip();
         swatch.Location = new Point(3, 3);

         var transparentBox = new KryptonCheckBox
         {
            AutoSize = true,
            Location = new Point(swatch.Right + 6, 5)
         };
         transparentBox.Values.Text = "Transparent";

         void Refresh()
         {
            var color = get();
            var isTransparent = color.A == 0;
            transparentBox.Checked = isTransparent;
            // Show a neutral fill when transparent so the user sees "no color".
            SetChipColor(swatch, color);
         }

         _refreshers[swatch] = Refresh;

         swatch.Click += (_, _) =>
         {
            var current = get();
            using var dialog = new ColorDialog
            {
               Color = current.A == 0 ? Color.White : current,
               FullOpen = true,
               AnyColor = true
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
               set(dialog.Color);
               Refresh();
               RefreshPreview();
            }
         };

         transparentBox.CheckedChanged += (_, _) =>
         {
            if (transparentBox.Checked && get().A != 0)
            {
               set(Color.Transparent);
               Refresh();
               RefreshPreview();
            }
            else if (!transparentBox.Checked && get().A == 0)
            {
               // Restore an opaque default when un-checking transparent.
               set(_working.EditorBackgroundColor);
               Refresh();
               RefreshPreview();
            }
         };

         Refresh();
         container.Controls.Add(swatch);
         container.Controls.Add(transparentBox);
         return container;
      }

      /// <summary>
      /// Creates a combo box for choosing the per-token font style (Regular/Bold/Italic/Bold+Italic).
      /// </summary>
      private Control CreateFontStyleControl(Func<FontStyle> get, Action<FontStyle> set)
      {
         var combo = new KryptonComboBox
         {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 110,
            Margin = new Padding(3)
         };
         combo.Items.AddRange(new object[] { "Regular", "Bold", "Italic", "Bold + Italic" });
         combo.SelectedIndex = FontStyleToIndex(get());

         combo.SelectedIndexChanged += (_, _) =>
         {
            set(IndexToFontStyle(combo.SelectedIndex));
            RefreshPreview();
         };

         return combo;
      }

      private static int FontStyleToIndex(FontStyle style)
      {
         var bold = style.HasFlag(FontStyle.Bold);
         var italic = style.HasFlag(FontStyle.Italic);
         if (bold && italic) return 3;
         if (italic) return 2;
         if (bold) return 1;
         return 0;
      }

      private static FontStyle IndexToFontStyle(int index)
      {
         return index switch
         {
            1 => FontStyle.Bold,
            2 => FontStyle.Italic,
            3 => FontStyle.Bold | FontStyle.Italic,
            _ => FontStyle.Regular
         };
      }

      private IList<TokenRowDefinition> BuildTokenRowDefinitions()
      {
         var s = _working;
         return new List<TokenRowDefinition>
         {
            Token("Default", () => s.DefaultWordColor, c => s.DefaultWordColor = c, () => s.DefaultWordBackgroundColor, c => s.DefaultWordBackgroundColor = c, () => s.DefaultWordFontStyle, v => s.DefaultWordFontStyle = v),
            Token("Keyword", () => s.KeywordColor, c => s.KeywordColor = c, () => s.KeywordBackgroundColor, c => s.KeywordBackgroundColor = c, () => s.KeywordFontStyle, v => s.KeywordFontStyle = v),
            Token("Comment", () => s.CommentColor, c => s.CommentColor = c, () => s.CommentBackgroundColor, c => s.CommentBackgroundColor = c, () => s.CommentFontStyle, v => s.CommentFontStyle = v),
            Token("Literal", () => s.LiteralColor, c => s.LiteralColor = c, () => s.LiteralBackgroundColor, c => s.LiteralBackgroundColor = c, () => s.LiteralFontStyle, v => s.LiteralFontStyle = v),
            Token("String", () => s.StringColor, c => s.StringColor = c, () => s.StringBackgroundColor, c => s.StringBackgroundColor = c, () => s.StringFontStyle, v => s.StringFontStyle = v),
            Token("Symbol", () => s.SymbolColor, c => s.SymbolColor = c, () => s.SymbolBackgroundColor, c => s.SymbolBackgroundColor = c, () => s.SymbolFontStyle, v => s.SymbolFontStyle = v),
            Token("Operator", () => s.OperatorColor, c => s.OperatorColor = c, () => s.OperatorBackgroundColor, c => s.OperatorBackgroundColor = c, () => s.OperatorFontStyle, v => s.OperatorFontStyle = v),
            Token("Parenthesis", () => s.ParenthesisColor, c => s.ParenthesisColor = c, () => s.ParenthesisBackgroundColor, c => s.ParenthesisBackgroundColor = c, () => s.ParenthesisFontStyle, v => s.ParenthesisFontStyle = v),
            Token("Bracket", () => s.BracketColor, c => s.BracketColor = c, () => s.BracketBackgroundColor, c => s.BracketBackgroundColor = c, () => s.BracketFontStyle, v => s.BracketFontStyle = v),
            Token("Curly Brace", () => s.CurlyBraceColor, c => s.CurlyBraceColor = c, () => s.CurlyBraceBackgroundColor, c => s.CurlyBraceBackgroundColor = c, () => s.CurlyBraceFontStyle, v => s.CurlyBraceFontStyle = v),
            Token("Object", () => s.ObjectColor, c => s.ObjectColor = c, () => s.ObjectBackgroundColor, c => s.ObjectBackgroundColor = c, () => s.ObjectFontStyle, v => s.ObjectFontStyle = v),
            Token("Core Reference", () => s.CoreReferenceColor, c => s.CoreReferenceColor = c, () => s.CoreReferenceBackgroundColor, c => s.CoreReferenceBackgroundColor = c, () => s.CoreReferenceFontStyle, v => s.CoreReferenceFontStyle = v),
            Token("Builtin Variable", () => s.BuiltinVariableColor, c => s.BuiltinVariableColor = c, () => s.BuiltinVariableBackgroundColor, c => s.BuiltinVariableBackgroundColor = c, () => s.BuiltinVariableFontStyle, v => s.BuiltinVariableFontStyle = v),
            Token("Builtin Function", () => s.BuiltinFunctionColor, c => s.BuiltinFunctionColor = c, () => s.BuiltinFunctionBackgroundColor, c => s.BuiltinFunctionBackgroundColor = c, () => s.BuiltinFunctionFontStyle, v => s.BuiltinFunctionFontStyle = v),
            Token("Verb", () => s.VerbColor, c => s.VerbColor = c, () => s.VerbBackgroundColor, c => s.VerbBackgroundColor = c, () => s.VerbFontStyle, v => s.VerbFontStyle = v),
            Token("Property", () => s.PropertyColor, c => s.PropertyColor = c, () => s.PropertyBackgroundColor, c => s.PropertyBackgroundColor = c, () => s.PropertyFontStyle, v => s.PropertyFontStyle = v)
         };
      }

      private static TokenRowDefinition Token(string label, Func<Color> getFg, Action<Color> setFg, Func<Color> getBg, Action<Color> setBg, Func<FontStyle> getStyle, Action<FontStyle> setStyle)
      {
         return new TokenRowDefinition
         {
            Label = label,
            GetForeground = getFg,
            SetForeground = setFg,
            GetBackground = getBg,
            SetBackground = setBg,
            GetFontStyle = getStyle,
            SetFontStyle = setStyle
         };
      }

      private IList<ChromeRowDefinition> BuildChromeRowDefinitions()
      {
         var s = _working;
         return new List<ChromeRowDefinition>
         {
            Chrome("Background", () => s.EditorBackgroundColor, c => s.EditorBackgroundColor = c),
            Chrome("Text", () => s.EditorTextColor, c => s.EditorTextColor = c),
            Chrome("Caret", () => s.EditorCaretColor, c => s.EditorCaretColor = c),
            Chrome("Line Number", () => s.EditorLineNumberColor, c => s.EditorLineNumberColor = c),
            Chrome("Current Line", () => s.EditorCurrentLineColor, c => s.EditorCurrentLineColor = c),
            Chrome("Text Selection", () => s.EditorTextSelectionColor, c => s.EditorTextSelectionColor = c),
            Chrome("Changed Line", () => s.EditorChangedLineColor, c => s.EditorChangedLineColor = c),
            Chrome("Folding Indicator", () => s.EditorFoldingIndicatorColor, c => s.EditorFoldingIndicatorColor = c),
            Chrome("Folding Highlight", () => s.EditorFoldingHighlightColor, c => s.EditorFoldingHighlightColor = c),
            Chrome("Indent Background", () => s.EditorIndentBackColor, c => s.EditorIndentBackColor = c),
            Chrome("Bookmark", () => s.EditorBookmarkColor, c => s.EditorBookmarkColor = c),
            Chrome("Service Line", () => s.EditorServiceLineColor, c => s.EditorServiceLineColor = c),
            Chrome("Error Indicator", () => s.ErrorIndicatorColor, c => s.ErrorIndicatorColor = c)
         };
      }

      private static ChromeRowDefinition Chrome(string label, Func<Color> get, Action<Color> set)
      {
         return new ChromeRowDefinition { Label = label, GetColor = get, SetColor = set };
      }

      /// <summary>
      /// Persists the working theme, copies it into the singleton, and applies it to open editors.
      /// </summary>
      /// <returns><c>true</c> if the apply succeeded; otherwise <c>false</c>.</returns>
      private bool ApplyTheme()
      {
         try
         {
            _working.SaveTo(_configPath);
         }
         catch (Exception ex)
         {
            MessageBox.Show(this,
               $"The theme could not be saved:{Environment.NewLine}{ex.Message}",
               "Theme Save Error",
               MessageBoxButtons.OK,
               MessageBoxIcon.Error);
            return false;
         }

         Settings.Instance.CopyFrom(_working);
         _manager?.ApplyThemeToOpenEditors();

         // Commit the global Krypton chrome to match the selected mode so the whole app updates live.
         if (_kryptonManager != null)
         {
            if (_working.EditorDarkTheme && _darkPalette != null)
            {
               _kryptonManager.GlobalPalette = _darkPalette;
            }
            else
            {
               // Mirror the application default for light mode (the designer leaves the manager at
               // its builtin Microsoft365Blue palette mode).
               _kryptonManager.GlobalPaletteMode = PaletteMode.Microsoft365Blue;
            }
         }

         return true;
      }

      /// <summary>
      /// Populates the dark/light mode dropdown and selects the entry matching the working theme.
      /// </summary>
      private void InitializeModeDropdown()
      {
         _suppressModeChange = true;
         try
         {
            cboMode.Items.Clear();
            cboMode.Items.AddRange(new object[] { "Light", "Dark" });
            cboMode.SelectedIndex = _working.EditorDarkTheme ? 1 : 0;
         }
         finally
         {
            _suppressModeChange = false;
         }
      }

      /// <summary>
      /// Applies the dialog-local Krypton palette to match the working dark/light flag. Affects this dialog only.
      /// </summary>
      private void ApplyDialogPalette()
      {
         var mode = _working.EditorDarkTheme
            ? PaletteMode.Microsoft365BlackDarkMode
            : PaletteMode.Microsoft365Blue;
         ApplyPaletteModeRecursive(this, mode);
      }

      /// <summary>
      /// Applies a local <see cref="PaletteMode"/> to <paramref name="control"/> and all descendant
      /// Krypton controls (any control exposing a settable <c>PaletteMode</c> property), so the dialog
      /// previews the chosen chrome without altering the global palette. Non-Krypton controls are skipped.
      /// </summary>
      private static void ApplyPaletteModeRecursive(Control control, PaletteMode mode)
      {
         var prop = control.GetType().GetProperty("PaletteMode", typeof(PaletteMode));
         if (prop != null && prop.CanWrite)
            prop.SetValue(control, mode);

         foreach (Control child in control.Controls)
            ApplyPaletteModeRecursive(child, mode);
      }

      private void cboMode_SelectedIndexChanged(object sender, EventArgs e)
      {
         if (_suppressModeChange)
            return;

         _working.EditorDarkTheme = cboMode.SelectedIndex == 1;
         ApplyDialogPalette();
      }

      /// <summary>
      /// Clears and rebuilds the left-pane controls so they reflect the current working settings.
      /// </summary>
      private void RebuildLeftControls()
      {
         leftScrollPanel.Controls.Clear();
         _refreshers.Clear();
         BuildLeftControls();
      }

      private void btnExport_Click(object sender, EventArgs e)
      {
         using var dialog = new SaveFileDialog
         {
            Filter = "Moo theme (*.mood)|*.mood",
            DefaultExt = "mood",
            AddExtension = true
         };

         if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

         try
         {
            _working.ExportThemeToJson(dialog.FileName);
         }
         catch (Exception ex)
         {
            MessageBox.Show(this,
               $"The theme could not be exported:{Environment.NewLine}{ex.Message}",
               "Theme Export Error",
               MessageBoxButtons.OK,
               MessageBoxIcon.Error);
         }
      }

      private void btnImport_Click(object sender, EventArgs e)
      {
         using var dialog = new OpenFileDialog
         {
            Filter = "Moo theme (*.mood)|*.mood",
            DefaultExt = "mood",
            CheckFileExists = true
         };

         if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

         try
         {
            var result = _working.ImportThemeFromJson(dialog.FileName);

            // Rebuild the UI to reflect the imported values.
            RebuildLeftControls();
            InitializeModeDropdown();
            ApplyDialogPalette();
            RefreshPreview();

            if (result?.MissingFontName != null)
            {
               MessageBox.Show(this,
                  $"Theme font '{result.MissingFontName}' isn't installed; using a monospace fallback.",
                  "Theme Font Unavailable",
                  MessageBoxButtons.OK,
                  MessageBoxIcon.Information);
            }
         }
         catch (Exception ex)
         {
            // Import is atomic, so the working copy and controls are unchanged on failure.
            MessageBox.Show(this,
               $"The theme could not be imported:{Environment.NewLine}{ex.Message}",
               "Theme Import Error",
               MessageBoxButtons.OK,
               MessageBoxIcon.Error);
         }
      }

      private void btnApply_Click(object sender, EventArgs e)
      {
         ApplyTheme();
      }

      private void btnOk_Click(object sender, EventArgs e)
      {
         if (ApplyTheme())
         {
            DialogResult = DialogResult.OK;
            Close();
         }
      }

      private void btnCancel_Click(object sender, EventArgs e)
      {
         DialogResult = DialogResult.Cancel;
         Close();
      }
   }
}
