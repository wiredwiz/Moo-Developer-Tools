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
      var resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
      tableLayoutPanel = new Krypton.Toolkit.KryptonTableLayoutPanel();
      logoPictureBox = new PictureBox();
      labelVersion = new Krypton.Toolkit.KryptonLabel();
      labelCopyright = new Krypton.Toolkit.KryptonLabel();
      labelCompanyName = new Krypton.Toolkit.KryptonLabel();
      okButton = new Krypton.Toolkit.KryptonButton();
      labelProductName = new Krypton.Toolkit.KryptonLabel();
      lblProject = new Krypton.Toolkit.KryptonLabel();
      lblDescription = new Krypton.Toolkit.KryptonWrapLabel();
      kryptonPanel1 = new Krypton.Toolkit.KryptonPanel();
      tableLayoutPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
      ((System.ComponentModel.ISupportInitialize)kryptonPanel1).BeginInit();
      kryptonPanel1.SuspendLayout();
      SuspendLayout();
      // 
      // tableLayoutPanel
      // 
      tableLayoutPanel.BackgroundImage = (Image)resources.GetObject("tableLayoutPanel.BackgroundImage");
      tableLayoutPanel.BackgroundImageLayout = ImageLayout.None;
      tableLayoutPanel.ColumnCount = 2;
      tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
      tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 67F));
      tableLayoutPanel.Controls.Add(logoPictureBox, 0, 1);
      tableLayoutPanel.Controls.Add(labelVersion, 1, 2);
      tableLayoutPanel.Controls.Add(labelCopyright, 1, 3);
      tableLayoutPanel.Controls.Add(labelCompanyName, 1, 4);
      tableLayoutPanel.Controls.Add(okButton, 1, 6);
      tableLayoutPanel.Controls.Add(labelProductName, 1, 0);
      tableLayoutPanel.Controls.Add(lblProject, 1, 1);
      tableLayoutPanel.Controls.Add(lblDescription, 1, 5);
      tableLayoutPanel.Dock = DockStyle.Fill;
      tableLayoutPanel.Location = new Point(10, 10);
      tableLayoutPanel.Margin = new Padding(4);
      tableLayoutPanel.Name = "tableLayoutPanel";
      tableLayoutPanel.RowCount = 7;
      tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
      tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
      tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
      tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
      tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
      tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
      tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
      tableLayoutPanel.Size = new Size(488, 306);
      tableLayoutPanel.TabIndex = 0;
      // 
      // logoPictureBox
      // 
      logoPictureBox.BackColor = Color.Transparent;
      logoPictureBox.Dock = DockStyle.Fill;
      logoPictureBox.Image = Properties.Resources.cartoon_cow_clipart_xl;
      logoPictureBox.Location = new Point(4, 34);
      logoPictureBox.Margin = new Padding(4);
      logoPictureBox.Name = "logoPictureBox";
      tableLayoutPanel.SetRowSpan(logoPictureBox, 6);
      logoPictureBox.Size = new Size(153, 268);
      logoPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
      logoPictureBox.TabIndex = 12;
      logoPictureBox.TabStop = false;
      // 
      // labelVersion
      // 
      labelVersion.Dock = DockStyle.Fill;
      labelVersion.Location = new Point(168, 60);
      labelVersion.Margin = new Padding(7, 0, 4, 0);
      labelVersion.MaximumSize = new Size(0, 20);
      labelVersion.Name = "labelVersion";
      labelVersion.Size = new Size(316, 20);
      labelVersion.StateCommon.ShortText.Font = new Font("Microsoft Sans Serif", 12F);
      labelVersion.TabIndex = 0;
      labelVersion.Values.Text = "Version";
      // 
      // labelCopyright
      // 
      labelCopyright.Dock = DockStyle.Fill;
      labelCopyright.Location = new Point(168, 90);
      labelCopyright.Margin = new Padding(7, 0, 4, 0);
      labelCopyright.MaximumSize = new Size(0, 20);
      labelCopyright.Name = "labelCopyright";
      labelCopyright.Size = new Size(316, 20);
      labelCopyright.StateCommon.ShortText.Font = new Font("Microsoft Sans Serif", 12F);
      labelCopyright.TabIndex = 21;
      labelCopyright.Values.Text = "Copyright";
      // 
      // labelCompanyName
      // 
      labelCompanyName.Dock = DockStyle.Fill;
      labelCompanyName.Location = new Point(168, 120);
      labelCompanyName.Margin = new Padding(7, 0, 4, 0);
      labelCompanyName.MaximumSize = new Size(0, 20);
      labelCompanyName.Name = "labelCompanyName";
      labelCompanyName.Size = new Size(316, 20);
      labelCompanyName.StateCommon.ShortText.Font = new Font("Microsoft Sans Serif", 12F);
      labelCompanyName.TabIndex = 22;
      labelCompanyName.Values.Text = "Company Name";
      // 
      // okButton
      // 
      okButton.AccessibleDescription = "";
      okButton.AccessibleRole = AccessibleRole.PushButton;
      okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      okButton.DialogResult = DialogResult.Cancel;
      okButton.Location = new Point(396, 276);
      okButton.Margin = new Padding(4);
      okButton.Name = "okButton";
      okButton.Size = new Size(88, 26);
      okButton.TabIndex = 24;
      okButton.Values.Text = "&OK";
      // 
      // labelProductName
      // 
      labelProductName.Dock = DockStyle.Fill;
      labelProductName.Location = new Point(168, 0);
      labelProductName.Margin = new Padding(7, 0, 4, 0);
      labelProductName.MaximumSize = new Size(0, 20);
      labelProductName.Name = "labelProductName";
      labelProductName.Size = new Size(316, 20);
      labelProductName.StateCommon.ShortText.Font = new Font("Microsoft Sans Serif", 12F);
      labelProductName.TabIndex = 19;
      labelProductName.Values.Text = "Product Name";
      // 
      // lblProject
      // 
      lblProject.Cursor = Cursors.Hand;
      lblProject.Location = new Point(168, 33);
      lblProject.Margin = new Padding(7, 3, 4, 0);
      lblProject.Name = "lblProject";
      lblProject.Size = new Size(60, 16);
      lblProject.StateCommon.ShortText.Color1 = Color.Blue;
      lblProject.StateCommon.ShortText.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Underline);
      lblProject.TabIndex = 25;
      lblProject.Values.Text = "Project Url";
      lblProject.Click += lnkProject_LinkClicked;
      // 
      // lblDescription
      // 
      lblDescription.Font = new Font("Segoe UI", 9F);
      lblDescription.ForeColor = Color.FromArgb(30, 57, 91);
      lblDescription.LabelStyle = Krypton.Toolkit.LabelStyle.NormalControl;
      lblDescription.Location = new Point(164, 150);
      lblDescription.Name = "lblDescription";
      lblDescription.Size = new Size(67, 15);
      lblDescription.Text = "Description";
      // 
      // kryptonPanel1
      // 
      kryptonPanel1.Controls.Add(tableLayoutPanel);
      kryptonPanel1.Dock = DockStyle.Fill;
      kryptonPanel1.Location = new Point(0, 0);
      kryptonPanel1.Margin = new Padding(3, 2, 3, 2);
      kryptonPanel1.Name = "kryptonPanel1";
      kryptonPanel1.Padding = new Padding(10);
      kryptonPanel1.Size = new Size(508, 326);
      kryptonPanel1.TabIndex = 1;
      // 
      // AboutBox
      // 
      AcceptButton = okButton;
      AccessibleDescription = "";
      AccessibleName = resources.GetString("$this.AccessibleName");
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      CancelButton = okButton;
      ClientSize = new Size(508, 326);
      Controls.Add(kryptonPanel1);
      FormBorderStyle = FormBorderStyle.FixedDialog;
      Margin = new Padding(4);
      MaximizeBox = false;
      MinimizeBox = false;
      Name = "AboutBox";
      ShowIcon = false;
      ShowInTaskbar = false;
      StartPosition = FormStartPosition.CenterParent;
      Text = "AboutBox";
      tableLayoutPanel.ResumeLayout(false);
      tableLayoutPanel.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)logoPictureBox).EndInit();
      ((System.ComponentModel.ISupportInitialize)kryptonPanel1).EndInit();
      kryptonPanel1.ResumeLayout(false);
      ResumeLayout(false);

   }

   #endregion

   private Krypton.Toolkit.KryptonTableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.PictureBox logoPictureBox;
    private Krypton.Toolkit.KryptonLabel labelProductName;
    private Krypton.Toolkit.KryptonLabel labelVersion;
    private Krypton.Toolkit.KryptonLabel labelCopyright;
    private Krypton.Toolkit.KryptonLabel labelCompanyName;
    private Krypton.Toolkit.KryptonButton okButton;
   private Krypton.Toolkit.KryptonPanel kryptonPanel1;
    private Krypton.Toolkit.KryptonLabel lblProject;
    private Krypton.Toolkit.KryptonWrapLabel lblDescription;
}
