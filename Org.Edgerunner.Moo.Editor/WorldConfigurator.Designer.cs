namespace Org.Edgerunner.Moo.Editor
{
   partial class WorldConfigurator
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
         this.kryptonLabel1 = new Krypton.Toolkit.KryptonLabel();
         this.kryptonLabel2 = new Krypton.Toolkit.KryptonLabel();
         this.kryptonLabel3 = new Krypton.Toolkit.KryptonLabel();
         this.kryptonLabel4 = new Krypton.Toolkit.KryptonLabel();
         this.kryptonLabel5 = new Krypton.Toolkit.KryptonLabel();
         this.kryptonLabel6 = new Krypton.Toolkit.KryptonLabel();
         this.txtConnection = new Krypton.Toolkit.KryptonTextBox();
         this.txtName = new Krypton.Toolkit.KryptonTextBox();
         this.txtHost = new Krypton.Toolkit.KryptonTextBox();
         this.txtPort = new Krypton.Toolkit.KryptonTextBox();
         this.txtUser = new Krypton.Toolkit.KryptonTextBox();
         this.txtPassword = new Krypton.Toolkit.KryptonTextBox();
         this.chkMcp = new Krypton.Toolkit.KryptonCheckBox();
         this.chkColor = new Krypton.Toolkit.KryptonCheckBox();
         this.chkShowOnMenu = new Krypton.Toolkit.KryptonCheckBox();
         this.chkLocalEdit = new Krypton.Toolkit.KryptonCheckBox();
         this.btnCancel = new Krypton.Toolkit.KryptonButton();
         this.btnOk = new Krypton.Toolkit.KryptonButton();
         this.btnShowPassword = new Krypton.Toolkit.KryptonCheckButton();
         this.chkEnableEcho = new Krypton.Toolkit.KryptonCheckBox();
         this.chkAutoLogin = new Krypton.Toolkit.KryptonCheckBox();
         this.chkPrompt = new Krypton.Toolkit.KryptonCheckBox();
         this.SuspendLayout();
         // 
         // kryptonLabel1
         // 
         this.kryptonLabel1.AutoSize = false;
         this.kryptonLabel1.Location = new System.Drawing.Point(12, 181);
         this.kryptonLabel1.Name = "kryptonLabel1";
         this.kryptonLabel1.Size = new System.Drawing.Size(86, 59);
         this.kryptonLabel1.TabIndex = 11;
         this.kryptonLabel1.Values.Text = "Connection\r\nString";
         // 
         // kryptonLabel2
         // 
         this.kryptonLabel2.Location = new System.Drawing.Point(12, 21);
         this.kryptonLabel2.Name = "kryptonLabel2";
         this.kryptonLabel2.Size = new System.Drawing.Size(49, 23);
         this.kryptonLabel2.TabIndex = 0;
         this.kryptonLabel2.Values.Text = "Name";
         // 
         // kryptonLabel3
         // 
         this.kryptonLabel3.Location = new System.Drawing.Point(12, 53);
         this.kryptonLabel3.Name = "kryptonLabel3";
         this.kryptonLabel3.Size = new System.Drawing.Size(41, 23);
         this.kryptonLabel3.TabIndex = 2;
         this.kryptonLabel3.Values.Text = "Host";
         // 
         // kryptonLabel4
         // 
         this.kryptonLabel4.Location = new System.Drawing.Point(12, 85);
         this.kryptonLabel4.Name = "kryptonLabel4";
         this.kryptonLabel4.Size = new System.Drawing.Size(38, 23);
         this.kryptonLabel4.TabIndex = 4;
         this.kryptonLabel4.Values.Text = "Port";
         // 
         // kryptonLabel5
         // 
         this.kryptonLabel5.Location = new System.Drawing.Point(12, 127);
         this.kryptonLabel5.Name = "kryptonLabel5";
         this.kryptonLabel5.Size = new System.Drawing.Size(40, 23);
         this.kryptonLabel5.TabIndex = 6;
         this.kryptonLabel5.Values.Text = "User";
         // 
         // kryptonLabel6
         // 
         this.kryptonLabel6.Location = new System.Drawing.Point(12, 159);
         this.kryptonLabel6.Name = "kryptonLabel6";
         this.kryptonLabel6.Size = new System.Drawing.Size(71, 23);
         this.kryptonLabel6.TabIndex = 8;
         this.kryptonLabel6.Values.Text = "Password";
         // 
         // txtConnection
         // 
         this.txtConnection.Location = new System.Drawing.Point(99, 188);
         this.txtConnection.Multiline = true;
         this.txtConnection.Name = "txtConnection";
         this.txtConnection.Size = new System.Drawing.Size(142, 52);
         this.txtConnection.TabIndex = 12;
         this.txtConnection.Text = "kryptonTextBox1";
         // 
         // txtName
         // 
         this.txtName.Location = new System.Drawing.Point(99, 18);
         this.txtName.Name = "txtName";
         this.txtName.Size = new System.Drawing.Size(142, 26);
         this.txtName.TabIndex = 1;
         this.txtName.Validated += new System.EventHandler(this.txtName_Validated);
         // 
         // txtHost
         // 
         this.txtHost.Location = new System.Drawing.Point(99, 50);
         this.txtHost.Name = "txtHost";
         this.txtHost.Size = new System.Drawing.Size(142, 26);
         this.txtHost.TabIndex = 3;
         this.txtHost.Validated += new System.EventHandler(this.txtHost_Validated);
         // 
         // txtPort
         // 
         this.txtPort.Location = new System.Drawing.Point(99, 82);
         this.txtPort.Name = "txtPort";
         this.txtPort.Size = new System.Drawing.Size(142, 26);
         this.txtPort.TabIndex = 5;
         this.txtPort.Validating += new System.ComponentModel.CancelEventHandler(this.txtPort_Validating);
         this.txtPort.Validated += new System.EventHandler(this.txtPort_Validated);
         // 
         // txtUser
         // 
         this.txtUser.Location = new System.Drawing.Point(99, 124);
         this.txtUser.Name = "txtUser";
         this.txtUser.Size = new System.Drawing.Size(142, 26);
         this.txtUser.TabIndex = 7;
         // 
         // txtPassword
         // 
         this.txtPassword.Location = new System.Drawing.Point(99, 156);
         this.txtPassword.Name = "txtPassword";
         this.txtPassword.PasswordChar = '●';
         this.txtPassword.Size = new System.Drawing.Size(142, 26);
         this.txtPassword.TabIndex = 9;
         this.txtPassword.UseSystemPasswordChar = true;
         // 
         // chkMcp
         // 
         this.chkMcp.Location = new System.Drawing.Point(28, 371);
         this.chkMcp.Name = "chkMcp";
         this.chkMcp.Size = new System.Drawing.Size(100, 23);
         this.chkMcp.TabIndex = 17;
         this.chkMcp.Values.Text = "Enable MCP";
         // 
         // chkColor
         // 
         this.chkColor.Location = new System.Drawing.Point(28, 400);
         this.chkColor.Name = "chkColor";
         this.chkColor.Size = new System.Drawing.Size(105, 23);
         this.chkColor.TabIndex = 18;
         this.chkColor.Values.Text = "Enable Color";
         // 
         // chkShowOnMenu
         // 
         this.chkShowOnMenu.Location = new System.Drawing.Point(28, 429);
         this.chkShowOnMenu.Name = "chkShowOnMenu";
         this.chkShowOnMenu.Size = new System.Drawing.Size(177, 23);
         this.chkShowOnMenu.TabIndex = 19;
         this.chkShowOnMenu.Values.Text = "Show As Menu Shortcut";
         // 
         // chkLocalEdit
         // 
         this.chkLocalEdit.Location = new System.Drawing.Point(28, 342);
         this.chkLocalEdit.Name = "chkLocalEdit";
         this.chkLocalEdit.Size = new System.Drawing.Size(193, 23);
         this.chkLocalEdit.TabIndex = 16;
         this.chkLocalEdit.Values.Text = "Enable Old Style Local Edit";
         // 
         // btnCancel
         // 
         this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.btnCancel.Location = new System.Drawing.Point(38, 487);
         this.btnCancel.Name = "btnCancel";
         this.btnCancel.Size = new System.Drawing.Size(90, 25);
         this.btnCancel.TabIndex = 20;
         this.btnCancel.Values.Text = "&Cancel";
         // 
         // btnOk
         // 
         this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.btnOk.Location = new System.Drawing.Point(167, 487);
         this.btnOk.Name = "btnOk";
         this.btnOk.Size = new System.Drawing.Size(90, 25);
         this.btnOk.TabIndex = 21;
         this.btnOk.Values.Text = "Ok";
         this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
         // 
         // btnShowPassword
         // 
         this.btnShowPassword.AccessibleDescription = "Displays or hides the password text";
         this.btnShowPassword.AccessibleName = "Show password button";
         this.btnShowPassword.Checked = true;
         this.btnShowPassword.Location = new System.Drawing.Point(247, 157);
         this.btnShowPassword.Name = "btnShowPassword";
         this.btnShowPassword.Size = new System.Drawing.Size(30, 25);
         this.btnShowPassword.TabIndex = 10;
         this.btnShowPassword.Values.Text = "Show Password";
         // 
         // chkEnableEcho
         // 
         this.chkEnableEcho.Location = new System.Drawing.Point(28, 313);
         this.chkEnableEcho.Name = "chkEnableEcho";
         this.chkEnableEcho.Size = new System.Drawing.Size(156, 23);
         this.chkEnableEcho.TabIndex = 15;
         this.chkEnableEcho.Values.Text = "Enable Console Echo";
         // 
         // chkAutoLogin
         // 
         this.chkAutoLogin.Location = new System.Drawing.Point(28, 255);
         this.chkAutoLogin.Name = "chkAutoLogin";
         this.chkAutoLogin.Size = new System.Drawing.Size(150, 23);
         this.chkAutoLogin.TabIndex = 13;
         this.chkAutoLogin.Values.Text = "Automatically Login";
         // 
         // chkPrompt
         // 
         this.chkPrompt.Location = new System.Drawing.Point(28, 284);
         this.chkPrompt.Name = "chkPrompt";
         this.chkPrompt.Size = new System.Drawing.Size(171, 23);
         this.chkPrompt.TabIndex = 14;
         this.chkPrompt.Values.Text = "Prompt For Credentials";
         // 
         // WorldConfigurator
         // 
         this.AccessibleDescription = "Form for configurating world information";
         this.AccessibleName = "World configuration page";
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.btnCancel;
         this.ClientSize = new System.Drawing.Size(295, 524);
         this.Controls.Add(this.chkPrompt);
         this.Controls.Add(this.chkAutoLogin);
         this.Controls.Add(this.chkEnableEcho);
         this.Controls.Add(this.btnShowPassword);
         this.Controls.Add(this.btnOk);
         this.Controls.Add(this.btnCancel);
         this.Controls.Add(this.chkLocalEdit);
         this.Controls.Add(this.chkShowOnMenu);
         this.Controls.Add(this.chkColor);
         this.Controls.Add(this.chkMcp);
         this.Controls.Add(this.txtPassword);
         this.Controls.Add(this.txtUser);
         this.Controls.Add(this.txtPort);
         this.Controls.Add(this.txtHost);
         this.Controls.Add(this.txtName);
         this.Controls.Add(this.txtConnection);
         this.Controls.Add(this.kryptonLabel6);
         this.Controls.Add(this.kryptonLabel5);
         this.Controls.Add(this.kryptonLabel4);
         this.Controls.Add(this.kryptonLabel3);
         this.Controls.Add(this.kryptonLabel2);
         this.Controls.Add(this.kryptonLabel1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
         this.Name = "WorldConfigurator";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "World Configuration";
         this.Load += new System.EventHandler(this.WorldConfigurator_Load);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

        #endregion
        private Krypton.Toolkit.KryptonLabel kryptonLabel1;
        private Krypton.Toolkit.KryptonLabel kryptonLabel2;
        private Krypton.Toolkit.KryptonLabel kryptonLabel3;
        private Krypton.Toolkit.KryptonLabel kryptonLabel4;
        private Krypton.Toolkit.KryptonLabel kryptonLabel5;
        private Krypton.Toolkit.KryptonLabel kryptonLabel6;
        private Krypton.Toolkit.KryptonTextBox txtConnection;
        private Krypton.Toolkit.KryptonTextBox txtName;
        private Krypton.Toolkit.KryptonTextBox txtHost;
        private Krypton.Toolkit.KryptonTextBox txtPort;
        private Krypton.Toolkit.KryptonTextBox txtUser;
        private Krypton.Toolkit.KryptonTextBox txtPassword;
        private Krypton.Toolkit.KryptonCheckBox chkMcp;
        private Krypton.Toolkit.KryptonCheckBox chkColor;
        private Krypton.Toolkit.KryptonCheckBox chkShowOnMenu;
        private Krypton.Toolkit.KryptonCheckBox chkLocalEdit;
        private Krypton.Toolkit.KryptonButton btnCancel;
        private Krypton.Toolkit.KryptonButton btnOk;
        private Krypton.Toolkit.KryptonCheckButton btnShowPassword;
        private Krypton.Toolkit.KryptonCheckBox chkEnableEcho;
        private Krypton.Toolkit.KryptonCheckBox chkAutoLogin;
        private Krypton.Toolkit.KryptonCheckBox chkPrompt;
    }
}