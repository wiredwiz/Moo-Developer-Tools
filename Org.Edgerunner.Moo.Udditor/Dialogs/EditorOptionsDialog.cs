#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="EditorOptionsDialog.cs">
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

namespace Org.Edgerunner.Moo.Udditor.Dialogs
{
   /// <summary>
   /// Modal dialog for editing editor settings with a tabbed interface.
   /// </summary>
   /// <remarks>
   /// The dialog edits a clone of <see cref="Settings.Instance"/>. Changes are only persisted
   /// when the user clicks Apply or OK. Cancel discards all changes.
   /// </remarks>
   public partial class EditorOptionsDialog : KryptonForm
   {
      private readonly WindowManager _manager;
      private readonly string _configPath;
      private readonly Settings _working;

      /// <summary>
      /// Maps control names to their corresponding setting values.
      /// </summary>
      private readonly Dictionary<string, object> _controlValues = new();

      /// <summary>
      /// Initializes a new instance of the <see cref="EditorOptionsDialog"/> class.
      /// </summary>
      /// <param name="manager">The window manager used to apply settings to open editors.</param>
      /// <param name="configPath">The path the working settings are persisted to on Apply/OK.</param>
      public EditorOptionsDialog(WindowManager manager, string configPath)
      {
         _manager = manager;
         _configPath = configPath;
         _working = Settings.Instance.Clone();

         InitializeComponent();
         BuildTabs();
         LoadSettingsIntoControls();
      }

      /// <summary>
      /// Dynamically builds the tab pages with appropriate controls.
      /// </summary>
      private void BuildTabs()
      {
         BuildSyntaxHighlightingColorsTab();
         BuildFontStylesTab();
         BuildChromeColorsTab();
         BuildEditorFontTab();
         BuildIndentationTab();
         BuildCodeFeaturesTab();
         BuildThemeTab();
      }

      #region Tab Building Methods

      private void BuildSyntaxHighlightingColorsTab()
      {
         var panel = new TableLayoutPanel
         {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            ColumnCount = 1,
            Padding = new Padding(8)
         };
         panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

         var group = CreateGroupBox("Syntax Token Colors", CreateColorGrid(new[]
         {
            ("Default Word", nameof(Settings.DefaultWordColor), nameof(Settings.DefaultWordBackgroundColor)),
            ("Keyword", nameof(Settings.KeywordColor), nameof(Settings.KeywordBackgroundColor)),
            ("Comment", nameof(Settings.CommentColor), nameof(Settings.CommentBackgroundColor)),
            ("String", nameof(Settings.StringColor), nameof(Settings.StringBackgroundColor)),
            ("Literal", nameof(Settings.LiteralColor), nameof(Settings.LiteralBackgroundColor)),
            ("Symbol", nameof(Settings.SymbolColor), nameof(Settings.SymbolBackgroundColor)),
            ("Operator", nameof(Settings.OperatorColor), nameof(Settings.OperatorBackgroundColor)),
            ("Parenthesis", nameof(Settings.ParenthesisColor), nameof(Settings.ParenthesisBackgroundColor)),
            ("Bracket", nameof(Settings.BracketColor), nameof(Settings.BracketBackgroundColor)),
            ("Curly Brace", nameof(Settings.CurlyBraceColor), nameof(Settings.CurlyBraceBackgroundColor)),
            ("Object", nameof(Settings.ObjectColor), nameof(Settings.ObjectBackgroundColor)),
            ("Core Reference", nameof(Settings.CoreReferenceColor), nameof(Settings.CoreReferenceBackgroundColor)),
            ("Builtin Variable", nameof(Settings.BuiltinVariableColor), nameof(Settings.BuiltinVariableBackgroundColor)),
            ("Builtin Function", nameof(Settings.BuiltinFunctionColor), nameof(Settings.BuiltinFunctionBackgroundColor)),
            ("Verb", nameof(Settings.VerbColor), nameof(Settings.VerbBackgroundColor)),
            ("Property", nameof(Settings.PropertyColor), nameof(Settings.PropertyBackgroundColor))
         }));

         panel.Controls.Add(group);
         tabPageSyntaxColors.Controls.Add(panel);
      }

      private void BuildFontStylesTab()
      {
         var panel = new TableLayoutPanel
         {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            ColumnCount = 1,
            Padding = new Padding(8)
         };
         panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

         var group = CreateGroupBox("Token Font Styles", CreateFontStyleGrid(new[]
         {
            ("Default Word", nameof(Settings.DefaultWordFontStyle)),
            ("Keyword", nameof(Settings.KeywordFontStyle)),
            ("Comment", nameof(Settings.CommentFontStyle)),
            ("String", nameof(Settings.StringFontStyle)),
            ("Literal", nameof(Settings.LiteralFontStyle)),
            ("Symbol", nameof(Settings.SymbolFontStyle)),
            ("Operator", nameof(Settings.OperatorFontStyle)),
            ("Parenthesis", nameof(Settings.ParenthesisFontStyle)),
            ("Bracket", nameof(Settings.BracketFontStyle)),
            ("Curly Brace", nameof(Settings.CurlyBraceFontStyle)),
            ("Object", nameof(Settings.ObjectFontStyle)),
            ("Core Reference", nameof(Settings.CoreReferenceFontStyle))
         }));

         panel.Controls.Add(group);
         tabPageFontStyles.Controls.Add(panel);
      }

      private void BuildChromeColorsTab()
      {
         var panel = new TableLayoutPanel
         {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            ColumnCount = 1,
            Padding = new Padding(8)
         };
         panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

         var group = CreateGroupBox("Editor Chrome", CreateChromeColorGrid(new[]
         {
            ("Text Color", nameof(Settings.EditorTextColor)),
            ("Background Color", nameof(Settings.EditorBackgroundColor)),
            ("Caret Color", nameof(Settings.EditorCaretColor)),
            ("Line Number Color", nameof(Settings.EditorLineNumberColor)),
            ("Current Line Color", nameof(Settings.EditorCurrentLineColor)),
            ("Selection Color", nameof(Settings.EditorTextSelectionColor)),
            ("Folding Indicator Color", nameof(Settings.EditorFoldingIndicatorColor)),
            ("Changed Line Color", nameof(Settings.EditorChangedLineColor)),
            ("Indent Background Color", nameof(Settings.EditorIndentBackColor)),
            ("Bookmark Color", nameof(Settings.EditorBookmarkColor)),
            ("Service Line Color", nameof(Settings.EditorServiceLineColor)),
            ("Folding Highlight Color", nameof(Settings.EditorFoldingHighlightColor)),
            ("Error Indicator Color", nameof(Settings.ErrorIndicatorColor))
         }));

         panel.Controls.Add(group);
         tabPageChromeColors.Controls.Add(panel);
      }

      private void BuildEditorFontTab()
      {
         var panel = new TableLayoutPanel
         {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Padding = new Padding(8),
            RowCount = 5
         };
         panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150f));
         panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

         // Font Family
         var lblFont = new Label { Text = "Font Family:", AutoSize = true };
         var cmbFont = new ComboBox { Name = nameof(Settings.EditorFontFamily), DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
         PopulateFontCombo(cmbFont);
         panel.Controls.Add(lblFont);
         panel.Controls.Add(cmbFont);

         // Font Size
         var lblSize = new Label { Text = "Font Size:", AutoSize = true };
         var nudSize = new NumericUpDown { Name = nameof(Settings.EditorFontSize), Dock = DockStyle.Fill, Minimum = 1, Maximum = 72, DecimalPlaces = 1 };
         panel.Controls.Add(lblSize);
         panel.Controls.Add(nudSize);

         // Zoom Factor
         var lblZoom = new Label { Text = "Zoom Factor:", AutoSize = true };
         var nudZoom = new NumericUpDown { Name = nameof(Settings.EditorZoomFactor), Dock = DockStyle.Fill, Minimum = -100, Maximum = 500 };
         panel.Controls.Add(lblZoom);
         panel.Controls.Add(nudZoom);

         // Tab Length
         var lblTab = new Label { Text = "Tab Length:", AutoSize = true };
         var nudTab = new NumericUpDown { Name = nameof(Settings.EditorTabLength), Dock = DockStyle.Fill, Minimum = 1, Maximum = 16 };
         panel.Controls.Add(lblTab);
         panel.Controls.Add(nudTab);

         // Autocomplete Delay
         var lblAutoComplete = new Label { Text = "Autocomplete Delay (ms):", AutoSize = true };
         var nudAutoComplete = new NumericUpDown { Name = nameof(Settings.EditorAutocompleteDelay), Dock = DockStyle.Fill, Minimum = 0, Maximum = 5000 };
         panel.Controls.Add(lblAutoComplete);
         panel.Controls.Add(nudAutoComplete);

         tabPageEditorFont.Controls.Add(panel);
      }

      private void BuildIndentationTab()
      {
         var panel = new TableLayoutPanel
         {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Padding = new Padding(8),
            RowCount = 5
         };
         panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200f));
         panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

         // Word Wrap
         var chkWordWrap = new CheckBox { Text = "Enable Word Wrap", Name = nameof(Settings.EditorWordWrap), AutoSize = true };
         panel.Controls.Add(chkWordWrap);
         panel.Controls.Add(new Label());

         // Word Wrap Auto Indent
         var chkWordWrapIndent = new CheckBox { Text = "Word Wrap Auto Indent", Name = nameof(Settings.EditorWordWrapAutoIndent), AutoSize = true };
         panel.Controls.Add(chkWordWrapIndent);
         panel.Controls.Add(new Label());

         // Word Wrap Indent
         var lblWrapIndent = new Label { Text = "Word Wrap Indent:", AutoSize = true };
         var nudWrapIndent = new NumericUpDown { Name = nameof(Settings.EditorWordWrapIndent), Dock = DockStyle.Fill, Minimum = 0, Maximum = 100 };
         panel.Controls.Add(lblWrapIndent);
         panel.Controls.Add(nudWrapIndent);

         // Auto Indent
         var chkAutoIndent = new CheckBox { Text = "Enable Auto Indent", Name = nameof(Settings.EditorAutoIndent), AutoSize = true };
         panel.Controls.Add(chkAutoIndent);
         panel.Controls.Add(new Label());

         // Show Text Indent Guides
         var chkIndentGuides = new CheckBox { Text = "Show Text Indent Guides", Name = nameof(Settings.EditorShowTextIndentGuides), AutoSize = true };
         panel.Controls.Add(chkIndentGuides);
         panel.Controls.Add(new Label());

         tabPageIndentation.Controls.Add(panel);
      }

      private void BuildCodeFeaturesTab()
      {
         var panel = new TableLayoutPanel
         {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Padding = new Padding(8),
            RowCount = 5
         };
         panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250f));
         panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

         // Show Code Folding
         var chkCodeFolding = new CheckBox { Text = "Enable Code Folding", Name = nameof(Settings.EditorShowCodeFolding), AutoSize = true };
         panel.Controls.Add(chkCodeFolding);
         panel.Controls.Add(new Label());

         // Show Folding Block Highlights
         var chkFoldHighlight = new CheckBox { Text = "Show Folding Block Highlights", Name = nameof(Settings.EditorShowFoldingBlockHighlights), AutoSize = true };
         panel.Controls.Add(chkFoldHighlight);
         panel.Controls.Add(new Label());

         // Auto Brackets
         var chkAutoBrackets = new CheckBox { Text = "Enable Auto Brackets", Name = nameof(Settings.EditorAutoBrackets), AutoSize = true };
         panel.Controls.Add(chkAutoBrackets);
         panel.Controls.Add(new Label());

         // Parser Message Font Family
         var lblParserFont = new Label { Text = "Parser Message Font:", AutoSize = true };
         var cmbParserFont = new ComboBox { Name = nameof(Settings.ParserMessageFontFamily), DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
         PopulateFontCombo(cmbParserFont);
         panel.Controls.Add(lblParserFont);
         panel.Controls.Add(cmbParserFont);

         // Parser Message Font Size
         var lblParserSize = new Label { Text = "Parser Message Font Size:", AutoSize = true };
         var nudParserSize = new NumericUpDown { Name = nameof(Settings.ParserMessageFontSize), Dock = DockStyle.Fill, Minimum = 1, Maximum = 72, DecimalPlaces = 1 };
         panel.Controls.Add(lblParserSize);
         panel.Controls.Add(nudParserSize);

         tabPageCodeFeatures.Controls.Add(panel);
      }

      private void BuildThemeTab()
      {
         var panel = new TableLayoutPanel
         {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Padding = new Padding(8),
            RowCount = 2
         };
         panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200f));
         panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

         // Dark Theme
         var chkDarkTheme = new CheckBox { Text = "Use Dark Theme", Name = nameof(Settings.EditorDarkTheme), AutoSize = true };
         panel.Controls.Add(chkDarkTheme);
         panel.Controls.Add(new Label());

         // Default Dialect
         var lblDialect = new Label { Text = "Default Grammar Dialect:", AutoSize = true };
         var cmbDialect = new ComboBox { Name = nameof(Settings.DefaultGrammarDialect), DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
         cmbDialect.Items.AddRange(new object[] { "LambdaMoo", "ToastStunt", "Edgerunner" });
         panel.Controls.Add(lblDialect);
         panel.Controls.Add(cmbDialect);

         tabPageTheme.Controls.Add(panel);
      }

      #endregion

      #region Helper Methods for Tab Building

      private GroupBox CreateGroupBox(string title, Control contentControl)
      {
         var group = new GroupBox
         {
            Text = title,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(8)
         };
         group.Controls.Add(contentControl);
         return group;
      }

      private Control CreateColorGrid((string label, string fgProperty, string bgProperty)[] rows)
      {
         var table = new TableLayoutPanel
         {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 3,
            Padding = new Padding(4),
            GrowStyle = TableLayoutPanelGrowStyle.AddRows
         };
         table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130f));
         table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80f));
         table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80f));

         // Header
         table.Controls.Add(new Label { Text = "Token", AutoSize = true });
         table.Controls.Add(new Label { Text = "Foreground", AutoSize = true });
         table.Controls.Add(new Label { Text = "Background", AutoSize = true });

         foreach (var (label, fgProp, bgProp) in rows)
         {
            table.Controls.Add(new Label { Text = label, AutoSize = true });
            var fgBtn = CreateColorButton(fgProp);
            var bgBtn = CreateColorButton(bgProp);
            table.Controls.Add(fgBtn);
            table.Controls.Add(bgBtn);
         }

         return table;
      }

      private Control CreateFontStyleGrid((string label, string property)[] rows)
      {
         var table = new TableLayoutPanel
         {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Padding = new Padding(4),
            GrowStyle = TableLayoutPanelGrowStyle.AddRows
         };
         table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130f));
         table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150f));

         // Header
         table.Controls.Add(new Label { Text = "Token", AutoSize = true });
         table.Controls.Add(new Label { Text = "Font Style", AutoSize = true });

         foreach (var (label, property) in rows)
         {
            table.Controls.Add(new Label { Text = label, AutoSize = true });
            var styleCombo = CreateFontStyleCombo(property);
            table.Controls.Add(styleCombo);
         }

         return table;
      }

      private Control CreateChromeColorGrid((string label, string property)[] rows)
      {
         var table = new TableLayoutPanel
         {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Padding = new Padding(4),
            GrowStyle = TableLayoutPanelGrowStyle.AddRows
         };
         table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180f));
         table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80f));

         // Header
         table.Controls.Add(new Label { Text = "Element", AutoSize = true });
         table.Controls.Add(new Label { Text = "Color", AutoSize = true });

         foreach (var (label, property) in rows)
         {
            table.Controls.Add(new Label { Text = label, AutoSize = true });
            var colorBtn = CreateColorButton(property);
            table.Controls.Add(colorBtn);
         }

         return table;
      }

      private Button CreateColorButton(string propertyName)
      {
         var btn = new Button
         {
            Name = propertyName,
            Width = 60,
            Height = 24,
            FlatStyle = FlatStyle.Flat,
            Tag = propertyName
         };

         // Get current color from settings
         var prop = typeof(Settings).GetProperty(propertyName);
         if (prop != null && prop.GetValue(_working) is Color color)
            btn.BackColor = color;

         btn.FlatAppearance.BorderColor = Color.Gray;
         btn.FlatAppearance.BorderSize = 1;

         btn.Click += (_, _) => OnColorButtonClick(btn, propertyName);

         return btn;
      }

      private ComboBox CreateFontStyleCombo(string propertyName)
      {
         var combo = new ComboBox
         {
            Name = propertyName,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 140,
            Tag = propertyName
         };

         combo.Items.AddRange(new object[] { "Regular", "Bold", "Italic", "Bold + Italic", "Underline", "Strikeout" });

         // Set current font style
         var prop = typeof(Settings).GetProperty(propertyName);
         if (prop != null && prop.GetValue(_working) is FontStyle style)
            combo.SelectedIndex = FontStyleToIndex(style);
         else
            combo.SelectedIndex = 0;

         return combo;
      }

      private void PopulateFontCombo(ComboBox combo)
      {
         foreach (var family in FontFamily.Families)
            combo.Items.Add(family.Name);

         // Set current selection
         var prop = typeof(Settings).GetProperty(combo.Name);
         if (prop != null && prop.GetValue(_working) is FontFamily fontFamily)
            combo.SelectedItem = fontFamily.Name;
         else if (combo.Items.Count > 0)
            combo.SelectedIndex = 0;
      }

      private void OnColorButtonClick(Button btn, string propertyName)
      {
         using var dialog = new ColorDialog
         {
            Color = btn.BackColor,
            FullOpen = true,
            AnyColor = true
         };

         if (dialog.ShowDialog(this) == DialogResult.OK)
         {
            btn.BackColor = dialog.Color;
            var prop = typeof(Settings).GetProperty(propertyName);
            if (prop != null)
               prop.SetValue(_working, dialog.Color);
         }
      }

      private static int FontStyleToIndex(FontStyle style)
      {
         if (style.HasFlag(FontStyle.Bold) && style.HasFlag(FontStyle.Italic))
            return 3;
         if (style.HasFlag(FontStyle.Italic))
            return 2;
         if (style.HasFlag(FontStyle.Bold))
            return 1;
         if (style.HasFlag(FontStyle.Underline))
            return 4;
         if (style.HasFlag(FontStyle.Strikeout))
            return 5;
         return 0;
      }

      private static FontStyle IndexToFontStyle(int index)
      {
         return index switch
         {
            1 => FontStyle.Bold,
            2 => FontStyle.Italic,
            3 => FontStyle.Bold | FontStyle.Italic,
            4 => FontStyle.Underline,
            5 => FontStyle.Strikeout,
            _ => FontStyle.Regular
         };
      }

      #endregion

      /// <summary>
      /// Loads all settings from the working copy into the dialog controls.
      /// </summary>
      private void LoadSettingsIntoControls()
      {
         // Load all controls with their corresponding settings values
         foreach (Control control in GetAllControls(this))
         {
            if (string.IsNullOrEmpty(control.Name) || control.Name.StartsWith("_"))
               continue;

            var prop = typeof(Settings).GetProperty(control.Name);
            if (prop == null)
               continue;

            var value = prop.GetValue(_working);

            if (control is Button btn && value is Color color)
               btn.BackColor = color;
            else if (control is CheckBox chk && value is bool boolValue)
               chk.Checked = boolValue;
            else if (control is NumericUpDown nud && value != null)
            {
               if (value is int intValue)
                  nud.Value = intValue;
               else if (value is float floatValue)
                  nud.Value = (decimal)floatValue;
            }
            else if (control is ComboBox combo && value != null)
            {
               if (value is FontFamily ff)
                  combo.SelectedItem = ff.Name;
               else if (value is Enum enumValue)
                  combo.SelectedIndex = (int)(object)enumValue;
               else
                  combo.SelectedItem = value.ToString();
            }
         }
      }

      /// <summary>
      /// Saves all dialog control values back to the working settings copy.
      /// </summary>
      private void SaveSettingsFromControls()
      {
         foreach (Control control in GetAllControls(this))
         {
            if (string.IsNullOrEmpty(control.Name) || control.Name.StartsWith("_"))
               continue;

            var prop = typeof(Settings).GetProperty(control.Name);
            if (prop == null || !prop.CanWrite)
               continue;

            object value = null;

            if (control is Button btn)
               value = btn.BackColor;
            else if (control is CheckBox chk)
               value = chk.Checked;
            else if (control is NumericUpDown nud)
            {
               if (prop.PropertyType == typeof(int))
                  value = (int)nud.Value;
               else if (prop.PropertyType == typeof(float))
                  value = (float)nud.Value;
            }
            else if (control is ComboBox combo)
            {
               if (prop.PropertyType == typeof(FontFamily))
               {
                  if (combo.SelectedItem is string fontName)
                     value = new FontFamily(fontName);
               }
               else if (prop.PropertyType.IsEnum)
                  value = Enum.ToObject(prop.PropertyType, combo.SelectedIndex);
               else if (combo.SelectedItem != null)
                  value = combo.SelectedItem.ToString();
            }

            if (value != null)
               prop.SetValue(_working, value);
         }
      }

      /// <summary>
      /// Gets all controls recursively from the specified container.
      /// </summary>
      private static IEnumerable<Control> GetAllControls(Control container)
      {
         foreach (Control control in container.Controls)
         {
            yield return control;
            foreach (var child in GetAllControls(control))
               yield return child;
         }
      }

      /// <summary>
      /// Applies the working settings to the singleton and persists to disk.
      /// </summary>
      private void ApplySettings()
      {
         try
         {
            SaveSettingsFromControls();
            _working.SaveTo(_configPath);
            Settings.Instance.CopyFrom(_working);
            _manager.ApplyThemeToOpenEditors();
         }
         catch (Exception ex)
         {
            MessageBox.Show($"Error saving settings: {ex.Message}", "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      private void buttonOk_Click(object sender, EventArgs e)
      {
         ApplySettings();
         DialogResult = DialogResult.OK;
         Close();
      }

      private void buttonApply_Click(object sender, EventArgs e)
      {
         ApplySettings();
      }

      private void buttonCancel_Click(object sender, EventArgs e)
      {
         DialogResult = DialogResult.Cancel;
         Close();
      }
   }
}
