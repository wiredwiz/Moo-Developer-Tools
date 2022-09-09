namespace Org.Edgerunner.Moo.Udditor;

partial class ConnectionInfoPrompt
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
         this.txtPort = new Krypton.Toolkit.KryptonTextBox();
         this.txtHost = new Krypton.Toolkit.KryptonTextBox();
         this.chkTls = new Krypton.Toolkit.KryptonCheckBox();
         this.kryptonPanel1 = new Krypton.Toolkit.KryptonPanel();
         this.btnConnect = new Krypton.Toolkit.KryptonButton();
         this.btnCancel = new Krypton.Toolkit.KryptonButton();
         ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).BeginInit();
         this.kryptonPanel1.SuspendLayout();
         this.SuspendLayout();
         // 
         // kryptonLabel1
         // 
         this.kryptonLabel1.Location = new System.Drawing.Point(11, 21);
         this.kryptonLabel1.Name = "kryptonLabel1";
         this.kryptonLabel1.Size = new System.Drawing.Size(43, 24);
         this.kryptonLabel1.TabIndex = 0;
         this.kryptonLabel1.Values.Text = "Host";
         // 
         // kryptonLabel2
         // 
         this.kryptonLabel2.Location = new System.Drawing.Point(273, 21);
         this.kryptonLabel2.Name = "kryptonLabel2";
         this.kryptonLabel2.Size = new System.Drawing.Size(40, 24);
         this.kryptonLabel2.TabIndex = 2;
         this.kryptonLabel2.Values.Text = "Port";
         // 
         // txtPort
         // 
         this.txtPort.AccessibleName = "Port Number";
         this.txtPort.Location = new System.Drawing.Point(317, 18);
         this.txtPort.Name = "txtPort";
         this.txtPort.Size = new System.Drawing.Size(46, 27);
         this.txtPort.TabIndex = 3;
         this.txtPort.Validating += new System.ComponentModel.CancelEventHandler(this.ConnectInfo_Validating);
         // 
         // txtHost
         // 
         this.txtHost.AccessibleName = "Host Address";
         this.txtHost.Location = new System.Drawing.Point(58, 18);
         this.txtHost.Name = "txtHost";
         this.txtHost.Size = new System.Drawing.Size(209, 27);
         this.txtHost.TabIndex = 1;
         this.txtHost.Validating += new System.ComponentModel.CancelEventHandler(this.ConnectInfo_Validating);
         // 
         // chkTls
         // 
         this.chkTls.AccessibleRole = System.Windows.Forms.AccessibleRole.CheckButton;
         this.chkTls.Location = new System.Drawing.Point(12, 71);
         this.chkTls.Name = "chkTls";
         this.chkTls.Size = new System.Drawing.Size(79, 21);
         this.chkTls.TabIndex = 4;
         this.chkTls.Values.Text = "Use TLS";
         // 
         // kryptonPanel1
         // 
         this.kryptonPanel1.Controls.Add(this.btnConnect);
         this.kryptonPanel1.Controls.Add(this.btnCancel);
         this.kryptonPanel1.Controls.Add(this.chkTls);
         this.kryptonPanel1.Controls.Add(this.kryptonLabel1);
         this.kryptonPanel1.Controls.Add(this.txtHost);
         this.kryptonPanel1.Controls.Add(this.kryptonLabel2);
         this.kryptonPanel1.Controls.Add(this.txtPort);
         this.kryptonPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.kryptonPanel1.Location = new System.Drawing.Point(0, 0);
         this.kryptonPanel1.Name = "kryptonPanel1";
         this.kryptonPanel1.Size = new System.Drawing.Size(386, 109);
         this.kryptonPanel1.TabIndex = 0;
         // 
         // btnConnect
         // 
         this.btnConnect.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
         this.btnConnect.Enabled = false;
         this.btnConnect.Location = new System.Drawing.Point(274, 68);
         this.btnConnect.Name = "btnConnect";
         this.btnConnect.Size = new System.Drawing.Size(89, 38);
         this.btnConnect.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.btnConnect.TabIndex = 6;
         this.btnConnect.Values.Text = "Connect";
         this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
         // 
         // btnCancel
         // 
         this.btnCancel.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
         this.btnCancel.Location = new System.Drawing.Point(163, 68);
         this.btnCancel.Name = "btnCancel";
         this.btnCancel.Size = new System.Drawing.Size(89, 38);
         this.btnCancel.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.btnCancel.TabIndex = 5;
         this.btnCancel.Values.Text = "Cancel";
         // 
         // ConnectionInfoPrompt
         // 
         this.AcceptButton = this.btnConnect;
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.btnCancel;
         this.ClientSize = new System.Drawing.Size(386, 109);
         this.Controls.Add(this.kryptonPanel1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
         this.Name = "ConnectionInfoPrompt";
         this.Text = "Enter Connection Info";
         ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).EndInit();
         this.kryptonPanel1.ResumeLayout(false);
         this.kryptonPanel1.PerformLayout();
         this.ResumeLayout(false);

    }

    #endregion
    private Krypton.Toolkit.KryptonLabel kryptonLabel1;
    private Krypton.Toolkit.KryptonLabel kryptonLabel2;
    private Krypton.Toolkit.KryptonTextBox txtPort;
    private Krypton.Toolkit.KryptonTextBox txtHost;
    private Krypton.Toolkit.KryptonCheckBox chkTls;
    private Krypton.Toolkit.KryptonPanel kryptonPanel1;
    private Krypton.Toolkit.KryptonButton btnCancel;
    private Krypton.Toolkit.KryptonButton btnConnect;
}
