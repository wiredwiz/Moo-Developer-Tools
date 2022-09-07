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
         this.kryptonPanel1 = new Krypton.Toolkit.KryptonPanel();
         ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).BeginInit();
         this.kryptonPanel1.SuspendLayout();
         this.SuspendLayout();
         // 
         // kryptonLabel1
         // 
         this.kryptonLabel1.AutoSize = false;
         this.kryptonLabel1.Location = new System.Drawing.Point(12, 182);
         this.kryptonLabel1.Name = "kryptonLabel1";
         this.kryptonLabel1.Size = new System.Drawing.Size(86, 52);
         this.kryptonLabel1.TabIndex = 11;
         this.kryptonLabel1.Values.Text = "Connection\r\nString";
         // 
         // kryptonLabel2
         // 
         this.kryptonLabel2.Location = new System.Drawing.Point(12, 15);
         this.kryptonLabel2.Name = "kryptonLabel2";
         this.kryptonLabel2.Size = new System.Drawing.Size(49, 23);
         this.kryptonLabel2.TabIndex = 0;
         this.kryptonLabel2.Values.Text = "Name";
         // 
         // kryptonLabel3
         // 
         this.kryptonLabel3.Location = new System.Drawing.Point(12, 47);
         this.kryptonLabel3.Name = "kryptonLabel3";
         this.kryptonLabel3.Size = new System.Drawing.Size(41, 23);
         this.kryptonLabel3.TabIndex = 2;
         this.kryptonLabel3.Values.Text = "Host";
         // 
         // kryptonLabel4
         // 
         this.kryptonLabel4.Location = new System.Drawing.Point(12, 79);
         this.kryptonLabel4.Name = "kryptonLabel4";
         this.kryptonLabel4.Size = new System.Drawing.Size(38, 23);
         this.kryptonLabel4.TabIndex = 4;
         this.kryptonLabel4.Values.Text = "Port";
         // 
         // kryptonLabel5
         // 
         this.kryptonLabel5.Location = new System.Drawing.Point(12, 121);
         this.kryptonLabel5.Name = "kryptonLabel5";
         this.kryptonLabel5.Size = new System.Drawing.Size(40, 23);
         this.kryptonLabel5.TabIndex = 6;
         this.kryptonLabel5.Values.Text = "User";
         // 
         // kryptonLabel6
         // 
         this.kryptonLabel6.Location = new System.Drawing.Point(12, 153);
         this.kryptonLabel6.Name = "kryptonLabel6";
         this.kryptonLabel6.Size = new System.Drawing.Size(71, 23);
         this.kryptonLabel6.TabIndex = 8;
         this.kryptonLabel6.Values.Text = "Password";
         // 
         // txtConnection
         // 
         this.txtConnection.AccessibleName = "Connection string";
         this.txtConnection.Location = new System.Drawing.Point(104, 182);
         this.txtConnection.Multiline = true;
         this.txtConnection.Name = "txtConnection";
         this.txtConnection.Size = new System.Drawing.Size(156, 52);
         this.txtConnection.TabIndex = 12;
         this.txtConnection.Text = "kryptonTextBox1";
         // 
         // txtName
         // 
         this.txtName.AccessibleName = "World Name";
         this.txtName.Location = new System.Drawing.Point(104, 12);
         this.txtName.Name = "txtName";
         this.txtName.Size = new System.Drawing.Size(156, 26);
         this.txtName.TabIndex = 1;
         this.txtName.Validated += new System.EventHandler(this.Text_Validated);
         // 
         // txtHost
         // 
         this.txtHost.AccessibleName = "Host Address";
         this.txtHost.Location = new System.Drawing.Point(104, 44);
         this.txtHost.Name = "txtHost";
         this.txtHost.Size = new System.Drawing.Size(156, 26);
         this.txtHost.TabIndex = 3;
         this.txtHost.Validated += new System.EventHandler(this.Text_Validated);
         // 
         // txtPort
         // 
         this.txtPort.AccessibleName = "Port Number";
         this.txtPort.Location = new System.Drawing.Point(104, 76);
         this.txtPort.Name = "txtPort";
         this.txtPort.Size = new System.Drawing.Size(156, 26);
         this.txtPort.TabIndex = 5;
         this.txtPort.Validating += new System.ComponentModel.CancelEventHandler(this.txtPort_Validating);
         this.txtPort.Validated += new System.EventHandler(this.Text_Validated);
         // 
         // txtUser
         // 
         this.txtUser.AccessibleName = "Use Name";
         this.txtUser.Location = new System.Drawing.Point(104, 118);
         this.txtUser.Name = "txtUser";
         this.txtUser.Size = new System.Drawing.Size(156, 26);
         this.txtUser.TabIndex = 7;
         // 
         // txtPassword
         // 
         this.txtPassword.AccessibleName = "User Password";
         this.txtPassword.Location = new System.Drawing.Point(104, 150);
         this.txtPassword.Name = "txtPassword";
         this.txtPassword.PasswordChar = '●';
         this.txtPassword.Size = new System.Drawing.Size(156, 26);
         this.txtPassword.TabIndex = 9;
         this.txtPassword.UseSystemPasswordChar = true;
         // 
         // chkMcp
         // 
         this.chkMcp.AccessibleRole = System.Windows.Forms.AccessibleRole.CheckButton;
         this.chkMcp.Location = new System.Drawing.Point(51, 397);
         this.chkMcp.Name = "chkMcp";
         this.chkMcp.Size = new System.Drawing.Size(100, 23);
         this.chkMcp.TabIndex = 18;
         this.chkMcp.Values.Text = "Enable MCP";
         // 
         // chkColor
         // 
         this.chkColor.AccessibleRole = System.Windows.Forms.AccessibleRole.CheckButton;
         this.chkColor.Location = new System.Drawing.Point(51, 426);
         this.chkColor.Name = "chkColor";
         this.chkColor.Size = new System.Drawing.Size(105, 23);
         this.chkColor.TabIndex = 19;
         this.chkColor.Values.Text = "Enable Color";
         // 
         // chkShowOnMenu
         // 
         this.chkShowOnMenu.AccessibleRole = System.Windows.Forms.AccessibleRole.CheckButton;
         this.chkShowOnMenu.Location = new System.Drawing.Point(51, 455);
         this.chkShowOnMenu.Name = "chkShowOnMenu";
         this.chkShowOnMenu.Size = new System.Drawing.Size(177, 23);
         this.chkShowOnMenu.TabIndex = 20;
         this.chkShowOnMenu.Values.Text = "Show As Menu Shortcut";
         // 
         // chkLocalEdit
         // 
         this.chkLocalEdit.AccessibleRole = System.Windows.Forms.AccessibleRole.CheckButton;
         this.chkLocalEdit.Location = new System.Drawing.Point(51, 368);
         this.chkLocalEdit.Name = "chkLocalEdit";
         this.chkLocalEdit.Size = new System.Drawing.Size(193, 23);
         this.chkLocalEdit.TabIndex = 17;
         this.chkLocalEdit.Values.Text = "Enable Old Style Local Edit";
         // 
         // btnCancel
         // 
         this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.btnCancel.Location = new System.Drawing.Point(35, 503);
         this.btnCancel.Name = "btnCancel";
         this.btnCancel.Size = new System.Drawing.Size(90, 25);
         this.btnCancel.TabIndex = 21;
         this.btnCancel.Values.Text = "&Cancel";
         // 
         // btnOk
         // 
         this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.btnOk.Location = new System.Drawing.Point(170, 503);
         this.btnOk.Name = "btnOk";
         this.btnOk.Size = new System.Drawing.Size(90, 25);
         this.btnOk.TabIndex = 22;
         this.btnOk.Values.Text = "Ok";
         this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
         // 
         // btnShowPassword
         // 
         this.btnShowPassword.AccessibleDescription = "Displays or hides the password text";
         this.btnShowPassword.AccessibleName = "Show password button";
         this.btnShowPassword.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
         this.btnShowPassword.Checked = true;
         this.btnShowPassword.Location = new System.Drawing.Point(266, 151);
         this.btnShowPassword.Name = "btnShowPassword";
         this.btnShowPassword.Size = new System.Drawing.Size(30, 25);
         this.btnShowPassword.TabIndex = 10;
         this.btnShowPassword.ToolTipValues.Description = "Show password";
         this.btnShowPassword.ToolTipValues.EnableToolTips = true;
         this.btnShowPassword.Values.Text = "*";
         this.btnShowPassword.CheckedChanged += new System.EventHandler(this.btnShowPassword_CheckedChanged);
         // 
         // chkEnableEcho
         // 
         this.chkEnableEcho.AccessibleRole = System.Windows.Forms.AccessibleRole.CheckButton;
         this.chkEnableEcho.Location = new System.Drawing.Point(51, 338);
         this.chkEnableEcho.Name = "chkEnableEcho";
         this.chkEnableEcho.Size = new System.Drawing.Size(156, 23);
         this.chkEnableEcho.TabIndex = 16;
         this.chkEnableEcho.Values.Text = "Enable Console Echo";
         // 
         // chkAutoLogin
         // 
         this.chkAutoLogin.AccessibleRole = System.Windows.Forms.AccessibleRole.CheckButton;
         this.chkAutoLogin.Location = new System.Drawing.Point(51, 280);
         this.chkAutoLogin.Name = "chkAutoLogin";
         this.chkAutoLogin.Size = new System.Drawing.Size(150, 23);
         this.chkAutoLogin.TabIndex = 14;
         this.chkAutoLogin.Values.Text = "Automatically Login";
         // 
         // chkPrompt
         // 
         this.chkPrompt.AccessibleRole = System.Windows.Forms.AccessibleRole.CheckButton;
         this.chkPrompt.Location = new System.Drawing.Point(51, 309);
         this.chkPrompt.Name = "chkPrompt";
         this.chkPrompt.Size = new System.Drawing.Size(171, 23);
         this.chkPrompt.TabIndex = 15;
         this.chkPrompt.Values.Text = "Prompt For Credentials";
         // 
         // chkUseTLS
         // 
         this.chkUseTLS.AccessibleRole = System.Windows.Forms.AccessibleRole.CheckButton;
         this.chkUseTLS.Location = new System.Drawing.Point(51, 251);
         this.chkUseTLS.Name = "chkUseTLS";
         this.chkUseTLS.Size = new System.Drawing.Size(74, 23);
         this.chkUseTLS.TabIndex = 13;
         this.chkUseTLS.Values.Text = "Use TLS";
         // 
         // kryptonPanel1
         // 
         this.kryptonPanel1.Controls.Add(this.btnShowPassword);
         this.kryptonPanel1.Controls.Add(this.chkEnableEcho);
         this.kryptonPanel1.Controls.Add(this.txtPassword);
         this.kryptonPanel1.Controls.Add(this.btnCancel);
         this.kryptonPanel1.Controls.Add(this.txtUser);
         this.kryptonPanel1.Controls.Add(this.btnOk);
         this.kryptonPanel1.Controls.Add(this.txtPort);
         this.kryptonPanel1.Controls.Add(this.chkPrompt);
         this.kryptonPanel1.Controls.Add(this.txtHost);
         this.kryptonPanel1.Controls.Add(this.chkUseTLS);
         this.kryptonPanel1.Controls.Add(this.txtName);
         this.kryptonPanel1.Controls.Add(this.txtConnection);
         this.kryptonPanel1.Controls.Add(this.chkAutoLogin);
         this.kryptonPanel1.Controls.Add(this.kryptonLabel6);
         this.kryptonPanel1.Controls.Add(this.chkLocalEdit);
         this.kryptonPanel1.Controls.Add(this.kryptonLabel5);
         this.kryptonPanel1.Controls.Add(this.chkMcp);
         this.kryptonPanel1.Controls.Add(this.kryptonLabel4);
         this.kryptonPanel1.Controls.Add(this.chkShowOnMenu);
         this.kryptonPanel1.Controls.Add(this.kryptonLabel3);
         this.kryptonPanel1.Controls.Add(this.chkColor);
         this.kryptonPanel1.Controls.Add(this.kryptonLabel2);
         this.kryptonPanel1.Controls.Add(this.kryptonLabel1);
         this.kryptonPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.kryptonPanel1.Location = new System.Drawing.Point(0, 0);
         this.kryptonPanel1.Name = "kryptonPanel1";
         this.kryptonPanel1.Size = new System.Drawing.Size(322, 540);
         this.kryptonPanel1.TabIndex = 0;
         // 
         // WorldConfigurator
         // 
         this.AccessibleDescription = "Form for configurating world information";
         this.AccessibleName = "World configuration page";
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.btnCancel;
         this.ClientSize = new System.Drawing.Size(322, 540);
         this.Controls.Add(this.kryptonPanel1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
         this.Name = "WorldConfigurator";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "World Configuration";
         this.Load += new System.EventHandler(this.WorldConfigurator_Load);
         ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).EndInit();
         this.kryptonPanel1.ResumeLayout(false);
         this.kryptonPanel1.PerformLayout();
         this.ResumeLayout(false);

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
        private Krypton.Toolkit.KryptonPanel kryptonPanel1;
    }
}