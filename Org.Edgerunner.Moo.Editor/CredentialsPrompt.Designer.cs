namespace Org.Edgerunner.Moo.Editor
{
   partial class CredentialsPrompt
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
         this.kryptonPanel1 = new Krypton.Toolkit.KryptonPanel();
         this.btnOk = new Krypton.Toolkit.KryptonButton();
         this.btnCancel = new Krypton.Toolkit.KryptonButton();
         this.kryptonLabel1 = new Krypton.Toolkit.KryptonLabel();
         this.txtName = new Krypton.Toolkit.KryptonTextBox();
         this.kryptonLabel2 = new Krypton.Toolkit.KryptonLabel();
         this.txtPassword = new Krypton.Toolkit.KryptonTextBox();
         ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).BeginInit();
         this.kryptonPanel1.SuspendLayout();
         this.SuspendLayout();
         // 
         // kryptonPanel1
         // 
         this.kryptonPanel1.Controls.Add(this.btnOk);
         this.kryptonPanel1.Controls.Add(this.btnCancel);
         this.kryptonPanel1.Controls.Add(this.kryptonLabel1);
         this.kryptonPanel1.Controls.Add(this.txtName);
         this.kryptonPanel1.Controls.Add(this.kryptonLabel2);
         this.kryptonPanel1.Controls.Add(this.txtPassword);
         this.kryptonPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.kryptonPanel1.Location = new System.Drawing.Point(0, 0);
         this.kryptonPanel1.Name = "kryptonPanel1";
         this.kryptonPanel1.Size = new System.Drawing.Size(282, 135);
         this.kryptonPanel1.TabIndex = 0;
         // 
         // btnOk
         // 
         this.btnOk.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
         this.btnOk.Enabled = false;
         this.btnOk.Location = new System.Drawing.Point(160, 95);
         this.btnOk.Name = "btnOk";
         this.btnOk.Size = new System.Drawing.Size(112, 29);
         this.btnOk.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.btnOk.TabIndex = 5;
         this.btnOk.Values.Text = "Ok";
         this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
         // 
         // btnCancel
         // 
         this.btnCancel.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
         this.btnCancel.Location = new System.Drawing.Point(12, 95);
         this.btnCancel.Name = "btnCancel";
         this.btnCancel.Size = new System.Drawing.Size(112, 29);
         this.btnCancel.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.btnCancel.TabIndex = 4;
         this.btnCancel.Values.Text = "Cancel";
         // 
         // kryptonLabel1
         // 
         this.kryptonLabel1.Location = new System.Drawing.Point(3, 11);
         this.kryptonLabel1.Name = "kryptonLabel1";
         this.kryptonLabel1.Size = new System.Drawing.Size(86, 20);
         this.kryptonLabel1.StateCommon.ShortText.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.kryptonLabel1.TabIndex = 0;
         this.kryptonLabel1.Values.Text = "User Name:";
         // 
         // txtName
         // 
         this.txtName.AccessibleName = "User Name";
         this.txtName.Location = new System.Drawing.Point(114, 9);
         this.txtName.Name = "txtName";
         this.txtName.Size = new System.Drawing.Size(158, 23);
         this.txtName.StateCommon.Content.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.txtName.TabIndex = 1;
         this.txtName.TextChanged += new System.EventHandler(this.Credentials_Changed);
         // 
         // kryptonLabel2
         // 
         this.kryptonLabel2.Location = new System.Drawing.Point(12, 51);
         this.kryptonLabel2.Name = "kryptonLabel2";
         this.kryptonLabel2.Size = new System.Drawing.Size(76, 20);
         this.kryptonLabel2.StateCommon.ShortText.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.kryptonLabel2.TabIndex = 2;
         this.kryptonLabel2.Values.Text = "Password:";
         // 
         // txtPassword
         // 
         this.txtPassword.AccessibleName = "User Password";
         this.txtPassword.Location = new System.Drawing.Point(114, 48);
         this.txtPassword.Name = "txtPassword";
         this.txtPassword.PasswordChar = '●';
         this.txtPassword.Size = new System.Drawing.Size(158, 23);
         this.txtPassword.StateCommon.Content.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.txtPassword.TabIndex = 3;
         this.txtPassword.UseSystemPasswordChar = true;
         this.txtPassword.TextChanged += new System.EventHandler(this.Credentials_Changed);
         // 
         // CredentialsPrompt
         // 
         this.AcceptButton = this.btnOk;
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.btnCancel;
         this.ClientSize = new System.Drawing.Size(282, 135);
         this.Controls.Add(this.kryptonPanel1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
         this.Name = "CredentialsPrompt";
         this.Text = "Enter Login Credentials";
         this.Load += new System.EventHandler(this.CredentialsPrompt_Load);
         ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).EndInit();
         this.kryptonPanel1.ResumeLayout(false);
         this.kryptonPanel1.PerformLayout();
         this.ResumeLayout(false);

      }

        #endregion

        private Krypton.Toolkit.KryptonPanel kryptonPanel1;
        private Krypton.Toolkit.KryptonButton btnOk;
        private Krypton.Toolkit.KryptonButton btnCancel;
        private Krypton.Toolkit.KryptonLabel kryptonLabel1;
        private Krypton.Toolkit.KryptonTextBox txtName;
        private Krypton.Toolkit.KryptonLabel kryptonLabel2;
        private Krypton.Toolkit.KryptonTextBox txtPassword;
    }
}