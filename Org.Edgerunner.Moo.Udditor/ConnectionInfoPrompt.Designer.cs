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
         this.label1 = new System.Windows.Forms.Label();
         this.txtHost = new System.Windows.Forms.TextBox();
         this.label2 = new System.Windows.Forms.Label();
         this.btnCancel = new System.Windows.Forms.Button();
         this.btnConnect = new System.Windows.Forms.Button();
         this.txtPort = new System.Windows.Forms.MaskedTextBox();
         this.chkTls = new System.Windows.Forms.CheckBox();
         this.SuspendLayout();
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(12, 21);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(40, 20);
         this.label1.TabIndex = 0;
         this.label1.Text = "Host";
         // 
         // txtHost
         // 
         this.txtHost.Location = new System.Drawing.Point(58, 18);
         this.txtHost.Name = "txtHost";
         this.txtHost.Size = new System.Drawing.Size(198, 27);
         this.txtHost.TabIndex = 1;
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(273, 21);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(35, 20);
         this.label2.TabIndex = 2;
         this.label2.Text = "Port";
         // 
         // btnCancel
         // 
         this.btnCancel.Location = new System.Drawing.Point(154, 69);
         this.btnCancel.Name = "btnCancel";
         this.btnCancel.Size = new System.Drawing.Size(94, 29);
         this.btnCancel.TabIndex = 5;
         this.btnCancel.Text = "Cancel";
         this.btnCancel.UseVisualStyleBackColor = true;
         // 
         // btnConnect
         // 
         this.btnConnect.Location = new System.Drawing.Point(273, 69);
         this.btnConnect.Name = "btnConnect";
         this.btnConnect.Size = new System.Drawing.Size(94, 29);
         this.btnConnect.TabIndex = 6;
         this.btnConnect.Text = "Connect";
         this.btnConnect.UseVisualStyleBackColor = true;
         this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
         // 
         // txtPort
         // 
         this.txtPort.Location = new System.Drawing.Point(314, 18);
         this.txtPort.Mask = "00000";
         this.txtPort.Name = "txtPort";
         this.txtPort.Size = new System.Drawing.Size(53, 27);
         this.txtPort.TabIndex = 3;
         this.txtPort.ValidatingType = typeof(int);
         // 
         // chkTls
         // 
         this.chkTls.AutoSize = true;
         this.chkTls.Location = new System.Drawing.Point(12, 72);
         this.chkTls.Name = "chkTls";
         this.chkTls.Size = new System.Drawing.Size(82, 24);
         this.chkTls.TabIndex = 4;
         this.chkTls.Text = "Use TLS";
         this.chkTls.UseVisualStyleBackColor = true;
         // 
         // ConnectionInfoPrompt
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.btnCancel;
         this.ClientSize = new System.Drawing.Size(386, 110);
         this.Controls.Add(this.chkTls);
         this.Controls.Add(this.txtPort);
         this.Controls.Add(this.btnConnect);
         this.Controls.Add(this.btnCancel);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.txtHost);
         this.Controls.Add(this.label1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
         this.Name = "ConnectionInfoPrompt";
         this.Text = "Enter Connection Info";
         this.ResumeLayout(false);
         this.PerformLayout();

    }

    #endregion

    private Label label1;
    private TextBox txtHost;
    private Label label2;
    private Button btnCancel;
    private Button btnConnect;
    private MaskedTextBox txtPort;
    private CheckBox chkTls;
}