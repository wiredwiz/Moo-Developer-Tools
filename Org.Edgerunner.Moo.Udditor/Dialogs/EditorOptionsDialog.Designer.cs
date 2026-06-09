namespace Org.Edgerunner.Moo.Udditor.Dialogs
{
   partial class EditorOptionsDialog
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         components = new System.ComponentModel.Container();
         var resources = new System.ComponentModel.ComponentResourceManager(typeof(EditorOptionsDialog));

         tabControl1 = new TabControl();
         tabPageSyntaxColors = new TabPage();
         tabPageFontStyles = new TabPage();
         tabPageChromeColors = new TabPage();
         tabPageEditorFont = new TabPage();
         tabPageIndentation = new TabPage();
         tabPageCodeFeatures = new TabPage();
         tabPageTheme = new TabPage();

         panelButtons = new Panel();
         buttonOk = new Button();
         buttonApply = new Button();
         buttonCancel = new Button();

         tabControl1.SuspendLayout();
         panelButtons.SuspendLayout();
         SuspendLayout();

         // tabControl1
         tabControl1.Controls.Add(tabPageSyntaxColors);
         tabControl1.Controls.Add(tabPageFontStyles);
         tabControl1.Controls.Add(tabPageChromeColors);
         tabControl1.Controls.Add(tabPageEditorFont);
         tabControl1.Controls.Add(tabPageIndentation);
         tabControl1.Controls.Add(tabPageCodeFeatures);
         tabControl1.Controls.Add(tabPageTheme);
         tabControl1.Dock = DockStyle.Fill;
         tabControl1.Location = new Point(0, 0);
         tabControl1.Name = "tabControl1";
         tabControl1.SelectedIndex = 0;
         tabControl1.Size = new Size(700, 500);
         tabControl1.TabIndex = 0;

         // tabPageSyntaxColors
         tabPageSyntaxColors.Location = new Point(4, 24);
         tabPageSyntaxColors.Name = "tabPageSyntaxColors";
         tabPageSyntaxColors.Padding = new Padding(3);
         tabPageSyntaxColors.Size = new Size(692, 472);
         tabPageSyntaxColors.TabIndex = 0;
         tabPageSyntaxColors.Text = "Syntax Highlighting Colors";
         tabPageSyntaxColors.UseVisualStyleBackColor = true;

         // tabPageFontStyles
         tabPageFontStyles.Location = new Point(4, 24);
         tabPageFontStyles.Name = "tabPageFontStyles";
         tabPageFontStyles.Padding = new Padding(3);
         tabPageFontStyles.Size = new Size(692, 472);
         tabPageFontStyles.TabIndex = 1;
         tabPageFontStyles.Text = "Font Styles";
         tabPageFontStyles.UseVisualStyleBackColor = true;

         // tabPageChromeColors
         tabPageChromeColors.Location = new Point(4, 24);
         tabPageChromeColors.Name = "tabPageChromeColors";
         tabPageChromeColors.Padding = new Padding(3);
         tabPageChromeColors.Size = new Size(692, 472);
         tabPageChromeColors.TabIndex = 2;
         tabPageChromeColors.Text = "Editor Chrome Colors";
         tabPageChromeColors.UseVisualStyleBackColor = true;

         // tabPageEditorFont
         tabPageEditorFont.Location = new Point(4, 24);
         tabPageEditorFont.Name = "tabPageEditorFont";
         tabPageEditorFont.Padding = new Padding(3);
         tabPageEditorFont.Size = new Size(692, 472);
         tabPageEditorFont.TabIndex = 3;
         tabPageEditorFont.Text = "Editor Font & Display";
         tabPageEditorFont.UseVisualStyleBackColor = true;

         // tabPageIndentation
         tabPageIndentation.Location = new Point(4, 24);
         tabPageIndentation.Name = "tabPageIndentation";
         tabPageIndentation.Padding = new Padding(3);
         tabPageIndentation.Size = new Size(692, 472);
         tabPageIndentation.TabIndex = 4;
         tabPageIndentation.Text = "Indentation & Wrapping";
         tabPageIndentation.UseVisualStyleBackColor = true;

         // tabPageCodeFeatures
         tabPageCodeFeatures.Location = new Point(4, 24);
         tabPageCodeFeatures.Name = "tabPageCodeFeatures";
         tabPageCodeFeatures.Padding = new Padding(3);
         tabPageCodeFeatures.Size = new Size(692, 472);
         tabPageCodeFeatures.TabIndex = 5;
         tabPageCodeFeatures.Text = "Code Features";
         tabPageCodeFeatures.UseVisualStyleBackColor = true;

         // tabPageTheme
         tabPageTheme.Location = new Point(4, 24);
         tabPageTheme.Name = "tabPageTheme";
         tabPageTheme.Padding = new Padding(3);
         tabPageTheme.Size = new Size(692, 472);
         tabPageTheme.TabIndex = 6;
         tabPageTheme.Text = "Theme & Grammar";
         tabPageTheme.UseVisualStyleBackColor = true;

         // panelButtons
         panelButtons.Controls.Add(buttonCancel);
         panelButtons.Controls.Add(buttonApply);
         panelButtons.Controls.Add(buttonOk);
         panelButtons.Dock = DockStyle.Bottom;
         panelButtons.Height = 50;
         panelButtons.Location = new Point(0, 500);
         panelButtons.Name = "panelButtons";
         panelButtons.Padding = new Padding(8);
         panelButtons.Size = new Size(700, 50);
         panelButtons.TabIndex = 1;

         // buttonOk
         buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
         buttonOk.DialogResult = DialogResult.OK;
         buttonOk.Location = new Point(460, 12);
         buttonOk.Name = "buttonOk";
         buttonOk.Size = new Size(75, 23);
         buttonOk.TabIndex = 0;
         buttonOk.Text = "OK";
         buttonOk.UseVisualStyleBackColor = true;
         buttonOk.Click += buttonOk_Click;

         // buttonApply
         buttonApply.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
         buttonApply.Location = new Point(541, 12);
         buttonApply.Name = "buttonApply";
         buttonApply.Size = new Size(75, 23);
         buttonApply.TabIndex = 1;
         buttonApply.Text = "Apply";
         buttonApply.UseVisualStyleBackColor = true;
         buttonApply.Click += buttonApply_Click;

         // buttonCancel
         buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
         buttonCancel.DialogResult = DialogResult.Cancel;
         buttonCancel.Location = new Point(622, 12);
         buttonCancel.Name = "buttonCancel";
         buttonCancel.Size = new Size(75, 23);
         buttonCancel.TabIndex = 2;
         buttonCancel.Text = "Cancel";
         buttonCancel.UseVisualStyleBackColor = true;
         buttonCancel.Click += buttonCancel_Click;

         // EditorOptionsDialog
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         ClientSize = new Size(700, 550);
         Controls.Add(tabControl1);
         Controls.Add(panelButtons);
         FormBorderStyle = FormBorderStyle.FixedDialog;
         MaximizeBox = false;
         MinimizeBox = false;
         Name = "EditorOptionsDialog";
         ShowInTaskbar = false;
         StartPosition = FormStartPosition.CenterParent;
         Text = "Editor Options";

         tabControl1.ResumeLayout(false);
         panelButtons.ResumeLayout(false);
         ResumeLayout(false);
      }

      #endregion

      private TabControl tabControl1;
      private TabPage tabPageSyntaxColors;
      private TabPage tabPageFontStyles;
      private TabPage tabPageChromeColors;
      private TabPage tabPageEditorFont;
      private TabPage tabPageIndentation;
      private TabPage tabPageCodeFeatures;
      private TabPage tabPageTheme;

      private Panel panelButtons;
      private Button buttonOk;
      private Button buttonApply;
      private Button buttonCancel;
   }
}
