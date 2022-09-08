namespace Org.Edgerunner.Moo.Udditor;

partial class AboutBox
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
         this.tableLayoutPanel = new Krypton.Toolkit.KryptonTableLayoutPanel();
         this.logoPictureBox = new System.Windows.Forms.PictureBox();
         this.labelVersion = new Krypton.Toolkit.KryptonLabel();
         this.labelCopyright = new Krypton.Toolkit.KryptonLabel();
         this.labelCompanyName = new Krypton.Toolkit.KryptonLabel();
         this.textBoxDescription = new Krypton.Toolkit.KryptonTextBox();
         this.okButton = new Krypton.Toolkit.KryptonButton();
         this.labelProductName = new Krypton.Toolkit.KryptonLabel();
         this.lblProject = new Krypton.Toolkit.KryptonLabel();
         this.kryptonPanel1 = new Krypton.Toolkit.KryptonPanel();
         this.tableLayoutPanel.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).BeginInit();
         this.kryptonPanel1.SuspendLayout();
         this.SuspendLayout();
         // 
         // tableLayoutPanel
         // 
         this.tableLayoutPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tableLayoutPanel.BackgroundImage")));
         this.tableLayoutPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
         this.tableLayoutPanel.ColumnCount = 2;
         this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
         this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67F));
         this.tableLayoutPanel.Controls.Add(this.logoPictureBox, 0, 1);
         this.tableLayoutPanel.Controls.Add(this.labelVersion, 1, 2);
         this.tableLayoutPanel.Controls.Add(this.labelCopyright, 1, 3);
         this.tableLayoutPanel.Controls.Add(this.labelCompanyName, 1, 4);
         this.tableLayoutPanel.Controls.Add(this.textBoxDescription, 1, 5);
         this.tableLayoutPanel.Controls.Add(this.okButton, 1, 6);
         this.tableLayoutPanel.Controls.Add(this.labelProductName, 1, 0);
         this.tableLayoutPanel.Controls.Add(this.lblProject, 1, 1);
         this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.tableLayoutPanel.Location = new System.Drawing.Point(12, 13);
         this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.tableLayoutPanel.Name = "tableLayoutPanel";
         this.tableLayoutPanel.RowCount = 7;
         this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
         this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
         this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
         this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
         this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
         this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
         this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
         this.tableLayoutPanel.Size = new System.Drawing.Size(556, 387);
         this.tableLayoutPanel.TabIndex = 0;
         // 
         // logoPictureBox
         // 
         this.logoPictureBox.BackColor = System.Drawing.Color.Transparent;
         this.logoPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
         this.logoPictureBox.Image = global::Org.Edgerunner.Moo.Udditor.Properties.Resources.cartoon_cow_clipart_xl;
         this.logoPictureBox.Location = new System.Drawing.Point(4, 43);
         this.logoPictureBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.logoPictureBox.Name = "logoPictureBox";
         this.tableLayoutPanel.SetRowSpan(this.logoPictureBox, 6);
         this.logoPictureBox.Size = new System.Drawing.Size(175, 339);
         this.logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
         this.logoPictureBox.TabIndex = 12;
         this.logoPictureBox.TabStop = false;
         // 
         // labelVersion
         // 
         this.labelVersion.Dock = System.Windows.Forms.DockStyle.Fill;
         this.labelVersion.Location = new System.Drawing.Point(191, 76);
         this.labelVersion.Margin = new System.Windows.Forms.Padding(8, 0, 4, 0);
         this.labelVersion.MaximumSize = new System.Drawing.Size(0, 25);
         this.labelVersion.Name = "labelVersion";
         this.labelVersion.Size = new System.Drawing.Size(361, 25);
         this.labelVersion.TabIndex = 0;
         this.labelVersion.Values.Text = "Version";
         // 
         // labelCopyright
         // 
         this.labelCopyright.Dock = System.Windows.Forms.DockStyle.Fill;
         this.labelCopyright.Location = new System.Drawing.Point(191, 114);
         this.labelCopyright.Margin = new System.Windows.Forms.Padding(8, 0, 4, 0);
         this.labelCopyright.MaximumSize = new System.Drawing.Size(0, 25);
         this.labelCopyright.Name = "labelCopyright";
         this.labelCopyright.Size = new System.Drawing.Size(361, 25);
         this.labelCopyright.TabIndex = 21;
         this.labelCopyright.Values.Text = "Copyright";
         // 
         // labelCompanyName
         // 
         this.labelCompanyName.Dock = System.Windows.Forms.DockStyle.Fill;
         this.labelCompanyName.Location = new System.Drawing.Point(191, 152);
         this.labelCompanyName.Margin = new System.Windows.Forms.Padding(8, 0, 4, 0);
         this.labelCompanyName.MaximumSize = new System.Drawing.Size(0, 25);
         this.labelCompanyName.Name = "labelCompanyName";
         this.labelCompanyName.Size = new System.Drawing.Size(361, 25);
         this.labelCompanyName.TabIndex = 22;
         this.labelCompanyName.Values.Text = "Company Name";
         // 
         // textBoxDescription
         // 
         this.textBoxDescription.Dock = System.Windows.Forms.DockStyle.Fill;
         this.textBoxDescription.Location = new System.Drawing.Point(191, 195);
         this.textBoxDescription.Margin = new System.Windows.Forms.Padding(8, 5, 4, 5);
         this.textBoxDescription.Multiline = true;
         this.textBoxDescription.Name = "textBoxDescription";
         this.textBoxDescription.ReadOnly = true;
         this.textBoxDescription.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.textBoxDescription.Size = new System.Drawing.Size(361, 144);
         this.textBoxDescription.TabIndex = 23;
         this.textBoxDescription.TabStop = false;
         this.textBoxDescription.Text = "Description";
         // 
         // okButton
         // 
         this.okButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
         this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.okButton.Location = new System.Drawing.Point(452, 350);
         this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.okButton.Name = "okButton";
         this.okButton.Size = new System.Drawing.Size(100, 32);
         this.okButton.TabIndex = 24;
         this.okButton.Values.Text = "&OK";
         // 
         // labelProductName
         // 
         this.labelProductName.Dock = System.Windows.Forms.DockStyle.Fill;
         this.labelProductName.Location = new System.Drawing.Point(191, 0);
         this.labelProductName.Margin = new System.Windows.Forms.Padding(8, 0, 4, 0);
         this.labelProductName.MaximumSize = new System.Drawing.Size(0, 25);
         this.labelProductName.Name = "labelProductName";
         this.labelProductName.Size = new System.Drawing.Size(361, 25);
         this.labelProductName.TabIndex = 19;
         this.labelProductName.Values.Text = "Product Name";
         // 
         // lblProject
         // 
         this.lblProject.Cursor = System.Windows.Forms.Cursors.Hand;
         this.lblProject.Location = new System.Drawing.Point(191, 42);
         this.lblProject.Margin = new System.Windows.Forms.Padding(8, 4, 4, 0);
         this.lblProject.Name = "lblProject";
         this.lblProject.Size = new System.Drawing.Size(73, 19);
         this.lblProject.StateCommon.ShortText.Color1 = System.Drawing.Color.Blue;
         this.lblProject.StateCommon.ShortText.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point);
         this.lblProject.TabIndex = 25;
         this.lblProject.Values.Text = "Project Url";
         this.lblProject.Click += new System.EventHandler(this.lnkProject_LinkClicked);
         // 
         // kryptonPanel1
         // 
         this.kryptonPanel1.Controls.Add(this.tableLayoutPanel);
         this.kryptonPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.kryptonPanel1.Location = new System.Drawing.Point(0, 0);
         this.kryptonPanel1.Name = "kryptonPanel1";
         this.kryptonPanel1.Padding = new System.Windows.Forms.Padding(12, 13, 12, 13);
         this.kryptonPanel1.Size = new System.Drawing.Size(580, 413);
         this.kryptonPanel1.TabIndex = 1;
         // 
         // AboutBox
         // 
         this.AcceptButton = this.okButton;
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.okButton;
         this.ClientSize = new System.Drawing.Size(580, 413);
         this.Controls.Add(this.kryptonPanel1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "AboutBox";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "AboutBox";
         this.tableLayoutPanel.ResumeLayout(false);
         this.tableLayoutPanel.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).EndInit();
         this.kryptonPanel1.ResumeLayout(false);
         this.ResumeLayout(false);

    }

    #endregion

    private Krypton.Toolkit.KryptonTableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.PictureBox logoPictureBox;
    private Krypton.Toolkit.KryptonLabel labelProductName;
    private Krypton.Toolkit.KryptonLabel labelVersion;
    private Krypton.Toolkit.KryptonLabel labelCopyright;
    private Krypton.Toolkit.KryptonLabel labelCompanyName;
    private Krypton.Toolkit.KryptonTextBox textBoxDescription;
    private Krypton.Toolkit.KryptonButton okButton;
   private Krypton.Toolkit.KryptonPanel kryptonPanel1;
    private Krypton.Toolkit.KryptonLabel lblProject;
}
