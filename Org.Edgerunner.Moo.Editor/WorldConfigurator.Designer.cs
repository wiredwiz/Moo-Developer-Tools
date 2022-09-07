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
         this.chkUseTLS = new Krypton.Toolkit.KryptonCheckBox();
         this.SuspendLayout();
         // 
         // kryptonLabel1
         // 
         this.kryptonLabel1.AutoSize = false;
         this.kryptonLabel1.Location = new System.Drawing.Point(12, 191);
         this.kryptonLabel1.Name = "kryptonLabel1";
         this.kryptonLabel1.Size = new System.Drawing.Size(95, 62);
         this.kryptonLabel1.TabIndex = 11;
         this.kryptonLabel1.Values.Text = "Connection\r\nString";
         // 
         // kryptonLabel2
         // 
         this.kryptonLabel2.Location = new System.Drawing.Point(12, 22);
         this.kryptonLabel2.Name = "kryptonLabel2";
         this.kryptonLabel2.Size = new System.Drawing.Size(52, 24);
         this.kryptonLabel2.TabIndex = 0;
         this.kryptonLabel2.Values.Text = "Name";
         // 
         // kryptonLabel3
         // 
         this.kryptonLabel3.Location = new System.Drawing.Point(12, 56);
         this.kryptonLabel3.Name = "kryptonLabel3";
         this.kryptonLabel3.Size = new System.Drawing.Size(43, 24);
         this.kryptonLabel3.TabIndex = 2;
         this.kryptonLabel3.Values.Text = "Host";
         // 
         // kryptonLabel4
         // 
         this.kryptonLabel4.Location = new System.Drawing.Point(12, 89);
         this.kryptonLabel4.Name = "kryptonLabel4";
         this.kryptonLabel4.Size = new System.Drawing.Size(40, 24);
         this.kryptonLabel4.TabIndex = 4;
         this.kryptonLabel4.Values.Text = "Port";
         // 
         // kryptonLabel5
         // 
         this.kryptonLabel5.Location = new System.Drawing.Point(12, 134);
         this.kryptonLabel5.Name = "kryptonLabel5";
         this.kryptonLabel5.Size = new System.Drawing.Size(42, 24);
         this.kryptonLabel5.TabIndex = 6;
         this.kryptonLabel5.Values.Text = "User";
         // 
         // kryptonLabel6
         // 
         this.kryptonLabel6.Location = new System.Drawing.Point(12, 167);
         this.kryptonLabel6.Name = "kryptonLabel6";
         this.kryptonLabel6.Size = new System.Drawing.Size(76, 24);
         this.kryptonLabel6.TabIndex = 8;
         this.kryptonLabel6.Values.Text = "Password";
         // 
         // txtConnection
         // 
         this.txtConnection.Location = new System.Drawing.Point(104, 198);
         this.txtConnection.Multiline = true;
         this.txtConnection.Name = "txtConnection";
         this.txtConnection.Size = new System.Drawing.Size(156, 55);
         this.txtConnection.TabIndex = 12;
         this.txtConnection.Text = "kryptonTextBox1";
         // 
         // txtName
         // 
         this.txtName.Location = new System.Drawing.Point(104, 19);
         this.txtName.Name = "txtName";
         this.txtName.Size = new System.Drawing.Size(156, 27);
         this.txtName.TabIndex = 1;
         this.txtName.Validated += new System.EventHandler(this.Text_Validated);
         // 
         // txtHost
         // 
         this.txtHost.Location = new System.Drawing.Point(104, 53);
         this.txtHost.Name = "txtHost";
         this.txtHost.Size = new System.Drawing.Size(156, 27);
         this.txtHost.TabIndex = 3;
         this.txtHost.Validated += new System.EventHandler(this.Text_Validated);
         // 
         // txtPort
         // 
         this.txtPort.Location = new System.Drawing.Point(104, 86);
         this.txtPort.Name = "txtPort";
         this.txtPort.Size = new System.Drawing.Size(156, 27);
         this.txtPort.TabIndex = 5;
         this.txtPort.Validating += new System.ComponentModel.CancelEventHandler(this.txtPort_Validating);
         this.txtPort.Validated += new System.EventHandler(this.Text_Validated);
         // 
         // txtUser
         // 
         this.txtUser.Location = new System.Drawing.Point(104, 131);
         this.txtUser.Name = "txtUser";
         this.txtUser.Size = new System.Drawing.Size(156, 27);
         this.txtUser.TabIndex = 7;
         // 
         // txtPassword
         // 
         this.txtPassword.Location = new System.Drawing.Point(104, 164);
         this.txtPassword.Name = "txtPassword";
         this.txtPassword.PasswordChar = '●';
         this.txtPassword.Size = new System.Drawing.Size(156, 27);
         this.txtPassword.TabIndex = 9;
         this.txtPassword.UseSystemPasswordChar = true;
         // 
         // chkMcp
         // 
         this.chkMcp.Location = new System.Drawing.Point(51, 418);
         this.chkMcp.Name = "chkMcp";
         this.chkMcp.Size = new System.Drawing.Size(106, 24);
         this.chkMcp.TabIndex = 18;
         this.chkMcp.Values.Text = "Enable MCP";
         // 
         // chkColor
         // 
         this.chkColor.Location = new System.Drawing.Point(51, 448);
         this.chkColor.Name = "chkColor";
         this.chkColor.Size = new System.Drawing.Size(111, 24);
         this.chkColor.TabIndex = 19;
         this.chkColor.Values.Text = "Enable Color";
         // 
         // chkShowOnMenu
         // 
         this.chkShowOnMenu.Location = new System.Drawing.Point(51, 479);
         this.chkShowOnMenu.Name = "chkShowOnMenu";
         this.chkShowOnMenu.Size = new System.Drawing.Size(188, 24);
         this.chkShowOnMenu.TabIndex = 20;
         this.chkShowOnMenu.Values.Text = "Show As Menu Shortcut";
         // 
         // chkLocalEdit
         // 
         this.chkLocalEdit.Location = new System.Drawing.Point(51, 387);
         this.chkLocalEdit.Name = "chkLocalEdit";
         this.chkLocalEdit.Size = new System.Drawing.Size(205, 24);
         this.chkLocalEdit.TabIndex = 17;
         this.chkLocalEdit.Values.Text = "Enable Old Style Local Edit";
         // 
         // btnCancel
         // 
         this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.btnCancel.Location = new System.Drawing.Point(38, 529);
         this.btnCancel.Name = "btnCancel";
         this.btnCancel.Size = new System.Drawing.Size(90, 26);
         this.btnCancel.TabIndex = 21;
         this.btnCancel.Values.Text = "&Cancel";
         // 
         // btnOk
         // 
         this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.btnOk.Location = new System.Drawing.Point(167, 529);
         this.btnOk.Name = "btnOk";
         this.btnOk.Size = new System.Drawing.Size(90, 26);
         this.btnOk.TabIndex = 22;
         this.btnOk.Values.Text = "Ok";
         this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
         // 
         // btnShowPassword
         // 
         this.btnShowPassword.AccessibleDescription = "Displays or hides the password text";
         this.btnShowPassword.AccessibleName = "Show password button";
         this.btnShowPassword.Checked = true;
         this.btnShowPassword.Location = new System.Drawing.Point(266, 165);
         this.btnShowPassword.Name = "btnShowPassword";
         this.btnShowPassword.Size = new System.Drawing.Size(30, 26);
         this.btnShowPassword.TabIndex = 10;
         this.btnShowPassword.ToolTipValues.Description = "Show password";
         this.btnShowPassword.ToolTipValues.EnableToolTips = true;
         this.btnShowPassword.Values.Text = "*";
         this.btnShowPassword.CheckedChanged += new System.EventHandler(this.btnShowPassword_CheckedChanged);
         // 
         // chkEnableEcho
         // 
         this.chkEnableEcho.Location = new System.Drawing.Point(51, 356);
         this.chkEnableEcho.Name = "chkEnableEcho";
         this.chkEnableEcho.Size = new System.Drawing.Size(166, 24);
         this.chkEnableEcho.TabIndex = 16;
         this.chkEnableEcho.Values.Text = "Enable Console Echo";
         // 
         // chkAutoLogin
         // 
         this.chkAutoLogin.Location = new System.Drawing.Point(51, 295);
         this.chkAutoLogin.Name = "chkAutoLogin";
         this.chkAutoLogin.Size = new System.Drawing.Size(160, 24);
         this.chkAutoLogin.TabIndex = 14;
         this.chkAutoLogin.Values.Text = "Automatically Login";
         // 
         // chkPrompt
         // 
         this.chkPrompt.Location = new System.Drawing.Point(51, 326);
         this.chkPrompt.Name = "chkPrompt";
         this.chkPrompt.Size = new System.Drawing.Size(182, 24);
         this.chkPrompt.TabIndex = 15;
         this.chkPrompt.Values.Text = "Prompt For Credentials";
         // 
         // chkUseTLS
         // 
         this.chkUseTLS.Location = new System.Drawing.Point(51, 265);
         this.chkUseTLS.Name = "chkUseTLS";
         this.chkUseTLS.Size = new System.Drawing.Size(78, 24);
         this.chkUseTLS.TabIndex = 13;
         this.chkUseTLS.Values.Text = "Use TLS";
         // 
         // WorldConfigurator
         // 
         this.AccessibleDescription = "Form for configurating world information";
         this.AccessibleName = "World configuration page";
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.btnCancel;
         this.ClientSize = new System.Drawing.Size(322, 568);
         this.Controls.Add(this.chkUseTLS);
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
        private Krypton.Toolkit.KryptonCheckBox chkUseTLS;
    }
}