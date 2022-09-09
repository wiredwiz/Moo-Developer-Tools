namespace Org.Edgerunner.Moo.Udditor;

partial class Setup
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Setup));
         this.kryptonTableLayoutPanel1 = new Krypton.Toolkit.KryptonTableLayoutPanel();
         this.kryptonWrapLabel1 = new Krypton.Toolkit.KryptonWrapLabel();
         this.txtPassword = new Krypton.Toolkit.KryptonTextBox();
         this.kryptonLabel1 = new Krypton.Toolkit.KryptonLabel();
         this.btnOk = new Krypton.Toolkit.KryptonButton();
         this.btnCancel = new Krypton.Toolkit.KryptonButton();
         this.kryptonTableLayoutPanel1.SuspendLayout();
         this.SuspendLayout();
         // 
         // kryptonTableLayoutPanel1
         // 
         this.kryptonTableLayoutPanel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("kryptonTableLayoutPanel1.BackgroundImage")));
         this.kryptonTableLayoutPanel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
         this.kryptonTableLayoutPanel1.ColumnCount = 3;
         this.kryptonTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 138F));
         this.kryptonTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 113F));
         this.kryptonTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.kryptonTableLayoutPanel1.Controls.Add(this.kryptonWrapLabel1, 0, 0);
         this.kryptonTableLayoutPanel1.Controls.Add(this.txtPassword, 1, 1);
         this.kryptonTableLayoutPanel1.Controls.Add(this.kryptonLabel1, 0, 1);
         this.kryptonTableLayoutPanel1.Controls.Add(this.btnOk, 2, 2);
         this.kryptonTableLayoutPanel1.Controls.Add(this.btnCancel, 1, 2);
         this.kryptonTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.kryptonTableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
         this.kryptonTableLayoutPanel1.Name = "kryptonTableLayoutPanel1";
         this.kryptonTableLayoutPanel1.RowCount = 3;
         this.kryptonTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.kryptonTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
         this.kryptonTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
         this.kryptonTableLayoutPanel1.Size = new System.Drawing.Size(366, 224);
         this.kryptonTableLayoutPanel1.TabIndex = 0;
         // 
         // kryptonWrapLabel1
         // 
         this.kryptonTableLayoutPanel1.SetColumnSpan(this.kryptonWrapLabel1, 3);
         this.kryptonWrapLabel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.kryptonWrapLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(57)))), ((int)(((byte)(91)))));
         this.kryptonWrapLabel1.LabelStyle = Krypton.Toolkit.LabelStyle.NormalControl;
         this.kryptonWrapLabel1.Location = new System.Drawing.Point(3, 0);
         this.kryptonWrapLabel1.Name = "kryptonWrapLabel1";
         this.kryptonWrapLabel1.Size = new System.Drawing.Size(353, 120);
         this.kryptonWrapLabel1.Text = resources.GetString("kryptonWrapLabel1.Text");
         // 
         // txtPassword
         // 
         this.txtPassword.AccessibleName = "Enter master password";
         this.kryptonTableLayoutPanel1.SetColumnSpan(this.txtPassword, 2);
         this.txtPassword.Dock = System.Windows.Forms.DockStyle.Fill;
         this.txtPassword.Location = new System.Drawing.Point(141, 137);
         this.txtPassword.Name = "txtPassword";
         this.txtPassword.PasswordChar = '●';
         this.txtPassword.Size = new System.Drawing.Size(222, 27);
         this.txtPassword.TabIndex = 1;
         this.txtPassword.UseSystemPasswordChar = true;
         this.txtPassword.Validated += new System.EventHandler(this.txtPassword_Validated);
         // 
         // kryptonLabel1
         // 
         this.kryptonLabel1.Location = new System.Drawing.Point(3, 137);
         this.kryptonLabel1.Name = "kryptonLabel1";
         this.kryptonLabel1.Size = new System.Drawing.Size(130, 24);
         this.kryptonLabel1.TabIndex = 2;
         this.kryptonLabel1.Values.Text = "Master Password:";
         // 
         // btnOk
         // 
         this.btnOk.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
         this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnOk.Enabled = false;
         this.btnOk.Location = new System.Drawing.Point(254, 190);
         this.btnOk.Name = "btnOk";
         this.btnOk.Size = new System.Drawing.Size(109, 31);
         this.btnOk.TabIndex = 4;
         this.btnOk.Values.Text = "Ok";
         this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
         // 
         // btnCancel
         // 
         this.btnCancel.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
         this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnCancel.Location = new System.Drawing.Point(141, 190);
         this.btnCancel.Name = "btnCancel";
         this.btnCancel.Size = new System.Drawing.Size(107, 31);
         this.btnCancel.TabIndex = 3;
         this.btnCancel.Values.Text = "Cancel";
         // 
         // Setup
         // 
         this.AcceptButton = this.btnOk;
         this.AccessibleName = "Setup - Enter a master password you can remember.";
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.btnCancel;
         this.ClientSize = new System.Drawing.Size(366, 224);
         this.Controls.Add(this.kryptonTableLayoutPanel1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
         this.Name = "Setup";
         this.Text = "Initial Setup";
         this.kryptonTableLayoutPanel1.ResumeLayout(false);
         this.kryptonTableLayoutPanel1.PerformLayout();
         this.ResumeLayout(false);

   }

    #endregion

    private Krypton.Toolkit.KryptonTableLayoutPanel kryptonTableLayoutPanel1;
    private Krypton.Toolkit.KryptonWrapLabel kryptonWrapLabel1;
    private Krypton.Toolkit.KryptonTextBox txtPassword;
    private Krypton.Toolkit.KryptonLabel kryptonLabel1;
    private Krypton.Toolkit.KryptonButton btnOk;
    private Krypton.Toolkit.KryptonButton btnCancel;
}