namespace Org.Edgerunner.Moo.Editor
{
   partial class WorldManager
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
         this.btnConnect = new Krypton.Toolkit.KryptonButton();
         this.btnEdit = new Krypton.Toolkit.KryptonButton();
         this.btnDelete = new Krypton.Toolkit.KryptonButton();
         this.btnMoveUp = new Krypton.Toolkit.KryptonButton();
         this.btnMoveDown = new Krypton.Toolkit.KryptonButton();
         this.btnNew = new Krypton.Toolkit.KryptonButton();
         this.lstWorlds = new Krypton.Toolkit.KryptonListBox();
         this.kryptonPanel1 = new Krypton.Toolkit.KryptonPanel();
         ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).BeginInit();
         this.kryptonPanel1.SuspendLayout();
         this.SuspendLayout();
         // 
         // btnConnect
         // 
         this.btnConnect.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
         this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.btnConnect.Location = new System.Drawing.Point(251, 253);
         this.btnConnect.Name = "btnConnect";
         this.btnConnect.Size = new System.Drawing.Size(130, 36);
         this.btnConnect.StateCommon.Content.LongText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.btnConnect.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.btnConnect.TabIndex = 6;
         this.btnConnect.Values.Text = "Connect";
         this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
         // 
         // btnEdit
         // 
         this.btnEdit.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
         this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.btnEdit.Location = new System.Drawing.Point(251, 152);
         this.btnEdit.Name = "btnEdit";
         this.btnEdit.Size = new System.Drawing.Size(130, 36);
         this.btnEdit.StateCommon.Content.LongText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.btnEdit.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.btnEdit.TabIndex = 4;
         this.btnEdit.Values.Text = "Edit";
         this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
         // 
         // btnDelete
         // 
         this.btnDelete.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
         this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.btnDelete.Location = new System.Drawing.Point(251, 194);
         this.btnDelete.Name = "btnDelete";
         this.btnDelete.Size = new System.Drawing.Size(130, 36);
         this.btnDelete.StateCommon.Content.LongText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.btnDelete.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.btnDelete.TabIndex = 5;
         this.btnDelete.Values.Text = "Delete";
         this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
         // 
         // btnMoveUp
         // 
         this.btnMoveUp.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
         this.btnMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.btnMoveUp.Location = new System.Drawing.Point(251, 12);
         this.btnMoveUp.Name = "btnMoveUp";
         this.btnMoveUp.Size = new System.Drawing.Size(130, 36);
         this.btnMoveUp.StateCommon.Content.LongText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.btnMoveUp.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.btnMoveUp.TabIndex = 1;
         this.btnMoveUp.Values.Text = "Move Up";
         this.btnMoveUp.Click += new System.EventHandler(this.btnMoveUp_Click);
         // 
         // btnMoveDown
         // 
         this.btnMoveDown.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
         this.btnMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.btnMoveDown.Location = new System.Drawing.Point(251, 54);
         this.btnMoveDown.Name = "btnMoveDown";
         this.btnMoveDown.Size = new System.Drawing.Size(130, 36);
         this.btnMoveDown.StateCommon.Content.LongText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.btnMoveDown.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.btnMoveDown.TabIndex = 2;
         this.btnMoveDown.Values.Text = "Move Down";
         this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);
         // 
         // btnNew
         // 
         this.btnNew.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
         this.btnNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.btnNew.Location = new System.Drawing.Point(251, 110);
         this.btnNew.Name = "btnNew";
         this.btnNew.Size = new System.Drawing.Size(130, 36);
         this.btnNew.StateCommon.Content.LongText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.btnNew.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.btnNew.TabIndex = 3;
         this.btnNew.Values.Text = "New";
         this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
         // 
         // lstWorlds
         // 
         this.lstWorlds.AccessibleRole = System.Windows.Forms.AccessibleRole.List;
         this.lstWorlds.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.lstWorlds.DisplayMember = "Name";
         this.lstWorlds.Location = new System.Drawing.Point(12, 12);
         this.lstWorlds.Name = "lstWorlds";
         this.lstWorlds.Size = new System.Drawing.Size(221, 277);
         this.lstWorlds.StateCommon.Item.Content.LongText.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.lstWorlds.StateCommon.Item.Content.ShortText.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.lstWorlds.TabIndex = 0;
         this.lstWorlds.ValueMember = "Name";
         // 
         // kryptonPanel1
         // 
         this.kryptonPanel1.Controls.Add(this.lstWorlds);
         this.kryptonPanel1.Controls.Add(this.btnMoveUp);
         this.kryptonPanel1.Controls.Add(this.btnConnect);
         this.kryptonPanel1.Controls.Add(this.btnDelete);
         this.kryptonPanel1.Controls.Add(this.btnNew);
         this.kryptonPanel1.Controls.Add(this.btnEdit);
         this.kryptonPanel1.Controls.Add(this.btnMoveDown);
         this.kryptonPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.kryptonPanel1.Location = new System.Drawing.Point(0, 0);
         this.kryptonPanel1.Name = "kryptonPanel1";
         this.kryptonPanel1.Size = new System.Drawing.Size(393, 303);
         this.kryptonPanel1.TabIndex = 7;
         // 
         // WorldManager
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(393, 303);
         this.Controls.Add(this.kryptonPanel1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
         this.MinimumSize = new System.Drawing.Size(343, 342);
         this.Name = "WorldManager";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "World Manager";
         ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).EndInit();
         this.kryptonPanel1.ResumeLayout(false);
         this.ResumeLayout(false);

      }

        #endregion
        private Krypton.Toolkit.KryptonButton btnConnect;
        private Krypton.Toolkit.KryptonButton btnEdit;
        private Krypton.Toolkit.KryptonButton btnDelete;
        private Krypton.Toolkit.KryptonButton btnMoveUp;
        private Krypton.Toolkit.KryptonButton btnMoveDown;
        private Krypton.Toolkit.KryptonButton btnNew;
        private Krypton.Toolkit.KryptonListBox lstWorlds;
        private Krypton.Toolkit.KryptonPanel kryptonPanel1;
    }
}