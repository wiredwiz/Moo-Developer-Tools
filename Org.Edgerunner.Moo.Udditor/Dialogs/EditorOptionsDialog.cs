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
         BuildEditorFontTab();
         BuildIndentationTab();
         BuildCodeFeaturesTab();
         BuildGrammarTab();
      }

      #region Tab Building Methods

      private void BuildEditorFontTab()
      {
         var panel = new KryptonTableLayoutPanel
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
         var lblFont = new KryptonLabel { AutoSize = true };
         lblFont.Values.Text = "Font Family:";
         var cmbFont = new KryptonComboBox { Name = nameof(Settings.EditorFontFamily), DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
         PopulateFontCombo(cmbFont);
         panel.Controls.Add(lblFont);
         panel.Controls.Add(cmbFont);

         // Font Size
         var lblSize = new KryptonLabel { AutoSize = true };
         lblSize.Values.Text = "Font Size:";
         var nudSize = new KryptonNumericUpDown { Name = nameof(Settings.EditorFontSize), Dock = DockStyle.Fill, Minimum = 1, Maximum = 72, DecimalPlaces = 1 };
         panel.Controls.Add(lblSize);
         panel.Controls.Add(nudSize);

         // Zoom Factor
         var lblZoom = new KryptonLabel { AutoSize = true };
         lblZoom.Values.Text = "Zoom Factor:";
         var nudZoom = new KryptonNumericUpDown { Name = nameof(Settings.EditorZoomFactor), Dock = DockStyle.Fill, Minimum = -100, Maximum = 500 };
         panel.Controls.Add(lblZoom);
         panel.Controls.Add(nudZoom);

         // Tab Length
         var lblTab = new KryptonLabel { AutoSize = true };
         lblTab.Values.Text = "Tab Length:";
         var nudTab = new KryptonNumericUpDown { Name = nameof(Settings.EditorTabLength), Dock = DockStyle.Fill, Minimum = 1, Maximum = 16 };
         panel.Controls.Add(lblTab);
         panel.Controls.Add(nudTab);

         // Autocomplete Delay
         var lblAutoComplete = new KryptonLabel { AutoSize = true };
         lblAutoComplete.Values.Text = "Autocomplete Delay (ms):";
         var nudAutoComplete = new KryptonNumericUpDown { Name = nameof(Settings.EditorAutocompleteDelay), Dock = DockStyle.Fill, Minimum = 0, Maximum = 5000 };
         panel.Controls.Add(lblAutoComplete);
         panel.Controls.Add(nudAutoComplete);

         tabPageEditorFont.Controls.Add(panel);
      }

      private void BuildIndentationTab()
      {
         var panel = new KryptonTableLayoutPanel
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
         var chkWordWrap = new KryptonCheckBox { Name = nameof(Settings.EditorWordWrap), AutoSize = true };
         chkWordWrap.Values.Text = "Enable Word Wrap";
         panel.Controls.Add(chkWordWrap);
         panel.Controls.Add(new KryptonLabel());

         // Word Wrap Auto Indent
         var chkWordWrapIndent = new KryptonCheckBox { Name = nameof(Settings.EditorWordWrapAutoIndent), AutoSize = true };
         chkWordWrapIndent.Values.Text = "Word Wrap Auto Indent";
         panel.Controls.Add(chkWordWrapIndent);
         panel.Controls.Add(new KryptonLabel());

         // Word Wrap Indent
         var lblWrapIndent = new KryptonLabel { AutoSize = true };
         lblWrapIndent.Values.Text = "Word Wrap Indent:";
         var nudWrapIndent = new KryptonNumericUpDown { Name = nameof(Settings.EditorWordWrapIndent), Dock = DockStyle.Fill, Minimum = 0, Maximum = 100 };
         panel.Controls.Add(lblWrapIndent);
         panel.Controls.Add(nudWrapIndent);

         // Auto Indent
         var chkAutoIndent = new KryptonCheckBox { Name = nameof(Settings.EditorAutoIndent), AutoSize = true };
         chkAutoIndent.Values.Text = "Enable Auto Indent";
         panel.Controls.Add(chkAutoIndent);
         panel.Controls.Add(new KryptonLabel());

         // Show Text Indent Guides
         var chkIndentGuides = new KryptonCheckBox { Name = nameof(Settings.EditorShowTextIndentGuides), AutoSize = true };
         chkIndentGuides.Values.Text = "Show Text Indent Guides";
         panel.Controls.Add(chkIndentGuides);
         panel.Controls.Add(new KryptonLabel());

         tabPageIndentation.Controls.Add(panel);
      }

      private void BuildCodeFeaturesTab()
      {
         var panel = new KryptonTableLayoutPanel
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
         var chkCodeFolding = new KryptonCheckBox { Name = nameof(Settings.EditorShowCodeFolding), AutoSize = true };
         chkCodeFolding.Values.Text = "Enable Code Folding";
         panel.Controls.Add(chkCodeFolding);
         panel.Controls.Add(new KryptonLabel());

         // Show Folding Block Highlights
         var chkFoldHighlight = new KryptonCheckBox { Name = nameof(Settings.EditorShowFoldingBlockHighlights), AutoSize = true };
         chkFoldHighlight.Values.Text = "Show Folding Block Highlights";
         panel.Controls.Add(chkFoldHighlight);
         panel.Controls.Add(new KryptonLabel());

         // Auto Brackets
         var chkAutoBrackets = new KryptonCheckBox { Name = nameof(Settings.EditorAutoBrackets), AutoSize = true };
         chkAutoBrackets.Values.Text = "Enable Auto Brackets";
         panel.Controls.Add(chkAutoBrackets);
         panel.Controls.Add(new KryptonLabel());

         // Parser Message Font Family
         var lblParserFont = new KryptonLabel { AutoSize = true };
         lblParserFont.Values.Text = "Parser Message Font:";
         var cmbParserFont = new KryptonComboBox { Name = nameof(Settings.ParserMessageFontFamily), DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
         PopulateFontCombo(cmbParserFont);
         panel.Controls.Add(lblParserFont);
         panel.Controls.Add(cmbParserFont);

         // Parser Message Font Size
         var lblParserSize = new KryptonLabel { AutoSize = true };
         lblParserSize.Values.Text = "Parser Message Font Size:";
         var nudParserSize = new KryptonNumericUpDown { Name = nameof(Settings.ParserMessageFontSize), Dock = DockStyle.Fill, Minimum = 1, Maximum = 72, DecimalPlaces = 1 };
         panel.Controls.Add(lblParserSize);
         panel.Controls.Add(nudParserSize);

         tabPageCodeFeatures.Controls.Add(panel);
      }

      private void BuildGrammarTab()
      {
         var panel = new KryptonTableLayoutPanel
         {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Padding = new Padding(8),
            RowCount = 1
         };
         panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200f));
         panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

         // Default Dialect
         var lblDialect = new KryptonLabel { AutoSize = true };
         lblDialect.Values.Text = "Default Grammar Dialect:";
         var cmbDialect = new KryptonComboBox { Name = nameof(Settings.DefaultGrammarDialect), DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
         cmbDialect.Items.AddRange(new object[] { "LambdaMoo", "ToastStunt", "Edgerunner" });
         panel.Controls.Add(lblDialect);
         panel.Controls.Add(cmbDialect);

         tabPageGrammar.Controls.Add(panel);
      }

      #endregion

      #region Helper Methods for Tab Building

      private void PopulateFontCombo(KryptonComboBox combo)
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

            if (control is KryptonCheckBox chk && value is bool boolValue)
               chk.Checked = boolValue;
            else if (control is KryptonNumericUpDown nud && value != null)
            {
               if (value is int intValue)
                  nud.Value = intValue;
               else if (value is float floatValue)
                  nud.Value = (decimal)floatValue;
            }
            else if (control is KryptonComboBox combo && value != null)
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

            if (control is KryptonCheckBox chk)
               value = chk.Checked;
            else if (control is KryptonNumericUpDown nud)
            {
               if (prop.PropertyType == typeof(int))
                  value = (int)nud.Value;
               else if (prop.PropertyType == typeof(float))
                  value = (float)nud.Value;
            }
            else if (control is KryptonComboBox combo)
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

         if (container is Krypton.Navigator.KryptonNavigator nav)
            foreach (Krypton.Navigator.KryptonPage page in nav.Pages)
            {
               yield return page;
               foreach (var child in GetAllControls(page))
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
