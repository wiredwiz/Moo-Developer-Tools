namespace Org.Edgerunner.Moo.Udditor;

partial class Form1
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
         this.components = new System.ComponentModel.Container();
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
         this.splitContainer1 = new System.Windows.Forms.SplitContainer();
         this.panel1 = new System.Windows.Forms.Panel();
         this.consoleSim = new Org.Edgerunner.Moo.Editor.Controls.ConsoleWindowEmulator();
         this.txtInput = new System.Windows.Forms.TextBox();
         this.menuStrip1 = new System.Windows.Forms.MenuStrip();
         this.networkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
         this.splitContainer1.Panel1.SuspendLayout();
         this.splitContainer1.Panel2.SuspendLayout();
         this.splitContainer1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.consoleSim)).BeginInit();
         this.menuStrip1.SuspendLayout();
         this.SuspendLayout();
         // 
         // splitContainer1
         // 
         this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.splitContainer1.IsSplitterFixed = true;
         this.splitContainer1.Location = new System.Drawing.Point(0, 28);
         this.splitContainer1.Name = "splitContainer1";
         this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
         // 
         // splitContainer1.Panel1
         // 
         this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.Control;
         this.splitContainer1.Panel1.Controls.Add(this.consoleSim);
         this.splitContainer1.Panel1.Controls.Add(this.panel1);
         // 
         // splitContainer1.Panel2
         // 
         this.splitContainer1.Panel2.Controls.Add(this.txtInput);
         this.splitContainer1.Size = new System.Drawing.Size(561, 450);
         this.splitContainer1.SplitterDistance = 387;
         this.splitContainer1.SplitterWidth = 12;
         this.splitContainer1.TabIndex = 0;
         // 
         // panel1
         // 
         this.panel1.BackColor = System.Drawing.Color.Black;
         this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
         this.panel1.Location = new System.Drawing.Point(0, 361);
         this.panel1.Name = "panel1";
         this.panel1.Size = new System.Drawing.Size(561, 26);
         this.panel1.TabIndex = 1;
         // 
         // consoleSim
         // 
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
         this.consoleSim.AutoScrollMinSize = new System.Drawing.Size(0, 18);
         this.consoleSim.BackBrush = null;
         this.consoleSim.BackColor = System.Drawing.Color.Black;
         this.consoleSim.CaretBlinking = false;
         this.consoleSim.CaretColor = System.Drawing.Color.Transparent;
         this.consoleSim.CharHeight = 18;
         this.consoleSim.CharWidth = 10;
         this.consoleSim.DefaultMarkerSize = 8;
         this.consoleSim.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
         this.consoleSim.Dock = System.Windows.Forms.DockStyle.Fill;
         this.consoleSim.FindForm = null;
         this.consoleSim.ForeColor = System.Drawing.Color.WhiteSmoke;
         this.consoleSim.GoToForm = null;
         this.consoleSim.Hotkeys = resources.GetString("consoleSim.Hotkeys");
         this.consoleSim.IsReplaceMode = false;
         this.consoleSim.Location = new System.Drawing.Point(0, 0);
         this.consoleSim.Name = "consoleSim";
         this.consoleSim.Paddings = new System.Windows.Forms.Padding(0);
         this.consoleSim.ReadOnly = true;
         this.consoleSim.ReplaceForm = null;
         this.consoleSim.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
         this.consoleSim.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("consoleSim.ServiceColors")));
         this.consoleSim.ShowLineNumbers = false;
         this.consoleSim.Size = new System.Drawing.Size(561, 361);
         this.consoleSim.TabIndex = 0;
         this.consoleSim.WordWrap = true;
         this.consoleSim.Zoom = 100;
         // 
         // txtInput
         // 
         this.txtInput.Dock = System.Windows.Forms.DockStyle.Fill;
         this.txtInput.Location = new System.Drawing.Point(0, 0);
         this.txtInput.Multiline = true;
         this.txtInput.Name = "txtInput";
         this.txtInput.Size = new System.Drawing.Size(561, 51);
         this.txtInput.TabIndex = 0;
         this.txtInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyDown);
         // 
         // menuStrip1
         // 
         this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
         this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.networkToolStripMenuItem});
         this.menuStrip1.Location = new System.Drawing.Point(0, 0);
         this.menuStrip1.Name = "menuStrip1";
         this.menuStrip1.Size = new System.Drawing.Size(561, 28);
         this.menuStrip1.TabIndex = 1;
         this.menuStrip1.Text = "menuStrip1";
         // 
         // networkToolStripMenuItem
         // 
         this.networkToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.closeToolStripMenuItem});
         this.networkToolStripMenuItem.Name = "networkToolStripMenuItem";
         this.networkToolStripMenuItem.Size = new System.Drawing.Size(79, 24);
         this.networkToolStripMenuItem.Text = "Network";
         // 
         // openToolStripMenuItem
         // 
         this.openToolStripMenuItem.Name = "openToolStripMenuItem";
         this.openToolStripMenuItem.Size = new System.Drawing.Size(128, 26);
         this.openToolStripMenuItem.Text = "Open";
         this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
         // 
         // closeToolStripMenuItem
         // 
         this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
         this.closeToolStripMenuItem.Size = new System.Drawing.Size(128, 26);
         this.closeToolStripMenuItem.Text = "Close";
         this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
         // 
         // Form1
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(561, 478);
         this.Controls.Add(this.splitContainer1);
         this.Controls.Add(this.menuStrip1);
         this.MainMenuStrip = this.menuStrip1;
         this.Name = "Form1";
         this.Text = "Form1";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
         this.Load += new System.EventHandler(this.Form1_Load);
         this.splitContainer1.Panel1.ResumeLayout(false);
         this.splitContainer1.Panel2.ResumeLayout(false);
         this.splitContainer1.Panel2.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
         this.splitContainer1.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.consoleSim)).EndInit();
         this.menuStrip1.ResumeLayout(false);
         this.menuStrip1.PerformLayout();
         this.ResumeLayout(false);
         this.PerformLayout();

    }

    #endregion

    private SplitContainer splitContainer1;
    private TextBox txtInput;
    private Moo.Editor.Controls.ConsoleWindowEmulator consoleSim;
    private MenuStrip menuStrip1;
    private ToolStripMenuItem networkToolStripMenuItem;
    private ToolStripMenuItem openToolStripMenuItem;
    private ToolStripMenuItem closeToolStripMenuItem;
    private Panel panel1;
}
