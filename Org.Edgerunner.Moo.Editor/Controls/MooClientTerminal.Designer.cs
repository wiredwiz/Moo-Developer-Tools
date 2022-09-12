namespace Org.Edgerunner.Moo.Editor.Controls
{
   partial class MooClientTerminal
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

      #region Component Designer generated code

      /// <summary> 
      /// Required method for Designer support - do not modify 
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.components = new System.ComponentModel.Container();
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MooClientTerminal));
         this.splitContainer1 = new System.Windows.Forms.SplitContainer();
         this.consoleSim = new Org.Edgerunner.Moo.Editor.Controls.ConsoleWindowEmulator();
         this.txtInput = new System.Windows.Forms.TextBox();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
         this.splitContainer1.Panel1.SuspendLayout();
         this.splitContainer1.Panel2.SuspendLayout();
         this.splitContainer1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.consoleSim)).BeginInit();
         this.SuspendLayout();
         // 
         // splitContainer1
         // 
         this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
         this.splitContainer1.Location = new System.Drawing.Point(0, 0);
         this.splitContainer1.Name = "splitContainer1";
         this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
         // 
         // splitContainer1.Panel1
         // 
         this.splitContainer1.Panel1.Controls.Add(this.consoleSim);
         // 
         // splitContainer1.Panel2
         // 
         this.splitContainer1.Panel2.Controls.Add(this.txtInput);
         this.splitContainer1.Size = new System.Drawing.Size(354, 267);
         this.splitContainer1.SplitterDistance = 238;
         this.splitContainer1.TabIndex = 0;
         // 
         // consoleSim
         // 
         this.consoleSim.AccessibleDescription = "Textbox control";
         this.consoleSim.AccessibleName = "Fast Colored Text Box";
         this.consoleSim.AccessibleRole = System.Windows.Forms.AccessibleRole.Text;
         this.consoleSim.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
         this.consoleSim.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\r\n^\\s*(case|default)\\s*[^:]*" +
    "(?<range>:)\\s*(?<range>[^;]+);";
         this.consoleSim.AutoScrollMinSize = new System.Drawing.Size(0, 22);
         this.consoleSim.BackBrush = null;
         this.consoleSim.BackColor = System.Drawing.Color.Black;
         this.consoleSim.CaretBlinking = false;
         this.consoleSim.CaretColor = System.Drawing.Color.Transparent;
         this.consoleSim.CharHeight = 22;
         this.consoleSim.CharWidth = 12;
         this.consoleSim.ConsoleBackgroundColor = System.Drawing.Color.Black;
         this.consoleSim.ConsoleFontStyle = System.Drawing.FontStyle.Regular;
         this.consoleSim.ConsoleForeColor = System.Drawing.Color.WhiteSmoke;
         this.consoleSim.DefaultMarkerSize = 8;
         this.consoleSim.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
         this.consoleSim.Dock = System.Windows.Forms.DockStyle.Fill;
         this.consoleSim.FindForm = null;
         this.consoleSim.FoldingHighlightColor = System.Drawing.Color.LightGray;
         this.consoleSim.FoldingHighlightEnabled = false;
         this.consoleSim.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.consoleSim.ForeColor = System.Drawing.Color.WhiteSmoke;
         this.consoleSim.GoToForm = null;
         this.consoleSim.HighlightFoldingIndicator = false;
         this.consoleSim.Hotkeys = resources.GetString("consoleSim.Hotkeys");
         this.consoleSim.IsReplaceMode = false;
         this.consoleSim.Location = new System.Drawing.Point(0, 0);
         this.consoleSim.Margin = new System.Windows.Forms.Padding(3, 3, 3, 20);
         this.consoleSim.Name = "consoleSim";
         this.consoleSim.Paddings = new System.Windows.Forms.Padding(0);
         this.consoleSim.ReadOnly = true;
         this.consoleSim.ReplaceForm = null;
         this.consoleSim.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
         this.consoleSim.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("consoleSim.ServiceColors")));
         this.consoleSim.ShowLineNumbers = false;
         this.consoleSim.Size = new System.Drawing.Size(354, 238);
         this.consoleSim.TabIndex = 1;
         this.consoleSim.Text = "consoleWindowEmulator1";
         this.consoleSim.ToolTipDelay = 100;
         this.consoleSim.WordWrap = true;
         this.consoleSim.Zoom = 100;
         // 
         // txtInput
         // 
         this.txtInput.Dock = System.Windows.Forms.DockStyle.Fill;
         this.txtInput.Location = new System.Drawing.Point(0, 0);
         this.txtInput.Multiline = true;
         this.txtInput.Name = "txtInput";
         this.txtInput.PasswordChar = '*';
         this.txtInput.Size = new System.Drawing.Size(354, 25);
         this.txtInput.TabIndex = 0;
         this.txtInput.UseSystemPasswordChar = true;
         this.txtInput.TextChanged += new System.EventHandler(this.txtInput_TextChanged);
         this.txtInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyDown);
         // 
         // MooClientTerminal
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.splitContainer1);
         this.Name = "MooClientTerminal";
         this.Size = new System.Drawing.Size(354, 267);
         this.Enter += new System.EventHandler(this.MooClientTerminal_Enter);
         this.splitContainer1.Panel1.ResumeLayout(false);
         this.splitContainer1.Panel2.ResumeLayout(false);
         this.splitContainer1.Panel2.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
         this.splitContainer1.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.consoleSim)).EndInit();
         this.ResumeLayout(false);

      }

      #endregion

      private SplitContainer splitContainer1;
      private ConsoleWindowEmulator consoleSim;
        private TextBox txtInput;
    }
}
