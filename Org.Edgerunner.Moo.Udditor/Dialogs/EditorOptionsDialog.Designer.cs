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

         tabControl1 = new Krypton.Navigator.KryptonNavigator();
         tabPageEditorFont = new Krypton.Navigator.KryptonPage();
         tabPageIndentation = new Krypton.Navigator.KryptonPage();
         tabPageCodeFeatures = new Krypton.Navigator.KryptonPage();
         tabPageGrammar = new Krypton.Navigator.KryptonPage();

         panelButtons = new Krypton.Toolkit.KryptonPanel();
         buttonOk = new Krypton.Toolkit.KryptonButton();
         buttonApply = new Krypton.Toolkit.KryptonButton();
         buttonCancel = new Krypton.Toolkit.KryptonButton();

         ((System.ComponentModel.ISupportInitialize)(tabControl1)).BeginInit();
         tabControl1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(tabPageEditorFont)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(tabPageIndentation)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(tabPageCodeFeatures)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(tabPageGrammar)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(panelButtons)).BeginInit();
         panelButtons.SuspendLayout();
         SuspendLayout();

         // tabControl1
         tabControl1.Pages.Add(tabPageEditorFont);
         tabControl1.Pages.Add(tabPageIndentation);
         tabControl1.Pages.Add(tabPageCodeFeatures);
         tabControl1.Pages.Add(tabPageGrammar);
         tabControl1.NavigatorMode = Krypton.Navigator.NavigatorMode.BarTabGroup;
         // The options tabs are fixed; users must not be able to close them. Hide the tab-bar close (X).
         tabControl1.Button.CloseButtonDisplay = Krypton.Navigator.ButtonDisplay.Hide;
         tabControl1.Dock = DockStyle.Fill;
         tabControl1.Location = new Point(0, 0);
         tabControl1.Name = "tabControl1";
         tabControl1.SelectedIndex = 0;
         tabControl1.Size = new Size(700, 500);
         tabControl1.TabIndex = 0;

         // tabPageEditorFont
         tabPageEditorFont.Name = "tabPageEditorFont";
         tabPageEditorFont.Size = new Size(692, 472);
         tabPageEditorFont.Text = "Editor Font & Display";
         tabPageEditorFont.TextTitle = "Editor Font & Display";

         // tabPageIndentation
         tabPageIndentation.Name = "tabPageIndentation";
         tabPageIndentation.Size = new Size(692, 472);
         tabPageIndentation.Text = "Indentation & Wrapping";
         tabPageIndentation.TextTitle = "Indentation & Wrapping";

         // tabPageCodeFeatures
         tabPageCodeFeatures.Name = "tabPageCodeFeatures";
         tabPageCodeFeatures.Size = new Size(692, 472);
         tabPageCodeFeatures.Text = "Code Features";
         tabPageCodeFeatures.TextTitle = "Code Features";

         // tabPageGrammar
         tabPageGrammar.Name = "tabPageGrammar";
         tabPageGrammar.Size = new Size(692, 472);
         tabPageGrammar.Text = "Grammar";
         tabPageGrammar.TextTitle = "Grammar";

         // panelButtons
         panelButtons.Controls.Add(buttonCancel);
         panelButtons.Controls.Add(buttonApply);
         panelButtons.Controls.Add(buttonOk);
         panelButtons.Dock = DockStyle.Bottom;
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
         buttonOk.Values.Text = "OK";
         buttonOk.Click += buttonOk_Click;

         // buttonApply
         buttonApply.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
         buttonApply.Location = new Point(541, 12);
         buttonApply.Name = "buttonApply";
         buttonApply.Size = new Size(75, 23);
         buttonApply.TabIndex = 1;
         buttonApply.Values.Text = "Apply";
         buttonApply.Click += buttonApply_Click;

         // buttonCancel
         buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
         buttonCancel.DialogResult = DialogResult.Cancel;
         buttonCancel.Location = new Point(622, 12);
         buttonCancel.Name = "buttonCancel";
         buttonCancel.Size = new Size(75, 23);
         buttonCancel.TabIndex = 2;
         buttonCancel.Values.Text = "Cancel";
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

         ((System.ComponentModel.ISupportInitialize)(tabPageEditorFont)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(tabPageIndentation)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(tabPageCodeFeatures)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(tabPageGrammar)).EndInit();
         tabControl1.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(tabControl1)).EndInit();
         panelButtons.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(panelButtons)).EndInit();
         ResumeLayout(false);
      }

      #endregion

      private Krypton.Navigator.KryptonNavigator tabControl1;
      private Krypton.Navigator.KryptonPage tabPageEditorFont;
      private Krypton.Navigator.KryptonPage tabPageIndentation;
      private Krypton.Navigator.KryptonPage tabPageCodeFeatures;
      private Krypton.Navigator.KryptonPage tabPageGrammar;

      private Krypton.Toolkit.KryptonPanel panelButtons;
      private Krypton.Toolkit.KryptonButton buttonOk;
      private Krypton.Toolkit.KryptonButton buttonApply;
      private Krypton.Toolkit.KryptonButton buttonCancel;
   }
}
