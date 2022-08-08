namespace FastColoredTextBoxNS
{
    partial class FindForm
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
			this.btClose = new System.Windows.Forms.Button();
			this.btFindNext = new System.Windows.Forms.Button();
			this.tbFind = new System.Windows.Forms.TextBox();
			this.cbRegex = new System.Windows.Forms.CheckBox();
			this.cbMatchCase = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.cbWholeWord = new System.Windows.Forms.CheckBox();
			this.btnFindPrev = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btClose
			// 
			this.btClose.Location = new System.Drawing.Point(410, 112);
			this.btClose.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btClose.Name = "btClose";
			this.btClose.Size = new System.Drawing.Size(112, 35);
			this.btClose.TabIndex = 5;
			this.btClose.Text = "Close";
			this.btClose.UseVisualStyleBackColor = true;
			this.btClose.Click += new System.EventHandler(this.BtClose_Click);
			// 
			// btFindNext
			// 
			this.btFindNext.Location = new System.Drawing.Point(288, 112);
			this.btFindNext.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btFindNext.Name = "btFindNext";
			this.btFindNext.Size = new System.Drawing.Size(112, 35);
			this.btFindNext.TabIndex = 4;
			this.btFindNext.Text = "Next";
			this.btFindNext.UseVisualStyleBackColor = true;
			this.btFindNext.Click += new System.EventHandler(this.BtFindNext_Click);
			// 
			// tbFind
			// 
			this.tbFind.Location = new System.Drawing.Point(63, 18);
			this.tbFind.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tbFind.Name = "tbFind";
			this.tbFind.Size = new System.Drawing.Size(457, 26);
			this.tbFind.TabIndex = 0;
			this.tbFind.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TbFind_KeyPress);
			// 
			// cbRegex
			// 
			this.cbRegex.AutoSize = true;
			this.cbRegex.Location = new System.Drawing.Point(374, 58);
			this.cbRegex.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cbRegex.Name = "cbRegex";
			this.cbRegex.Size = new System.Drawing.Size(81, 24);
			this.cbRegex.TabIndex = 3;
			this.cbRegex.Text = "Regex";
			this.cbRegex.UseVisualStyleBackColor = true;
			// 
			// cbMatchCase
			// 
			this.cbMatchCase.AutoSize = true;
			this.cbMatchCase.Location = new System.Drawing.Point(63, 58);
			this.cbMatchCase.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cbMatchCase.Name = "cbMatchCase";
			this.cbMatchCase.Size = new System.Drawing.Size(117, 24);
			this.cbMatchCase.TabIndex = 1;
			this.cbMatchCase.Text = "Match case";
			this.cbMatchCase.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 23);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(48, 20);
			this.label1.TabIndex = 5;
			this.label1.Text = "Find: ";
			// 
			// cbWholeWord
			// 
			this.cbWholeWord.AutoSize = true;
			this.cbWholeWord.Location = new System.Drawing.Point(195, 58);
			this.cbWholeWord.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cbWholeWord.Name = "cbWholeWord";
			this.cbWholeWord.Size = new System.Drawing.Size(162, 24);
			this.cbWholeWord.TabIndex = 2;
			this.cbWholeWord.Text = "Match whole word";
			this.cbWholeWord.UseVisualStyleBackColor = true;
			// 
			// btnFindPrev
			// 
			this.btnFindPrev.Location = new System.Drawing.Point(168, 112);
			this.btnFindPrev.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnFindPrev.Name = "btnFindPrev";
			this.btnFindPrev.Size = new System.Drawing.Size(112, 35);
			this.btnFindPrev.TabIndex = 6;
			this.btnFindPrev.Text = "Previous";
			this.btnFindPrev.UseVisualStyleBackColor = true;
			this.btnFindPrev.Click += new System.EventHandler(this.BtnFindPrev_Click);
			// 
			// FindForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(540, 166);
			this.Controls.Add(this.btnFindPrev);
			this.Controls.Add(this.cbWholeWord);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cbMatchCase);
			this.Controls.Add(this.cbRegex);
			this.Controls.Add(this.tbFind);
			this.Controls.Add(this.btFindNext);
			this.Controls.Add(this.btClose);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "FindForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Find";
			this.TopMost = true;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FindForm_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox cbMatchCase;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbWholeWord;
		private System.Windows.Forms.CheckBox cbRegex;
		public System.Windows.Forms.Button btFindNext;
		public System.Windows.Forms.Button btClose;
		public System.Windows.Forms.Button btnFindPrev;
		public System.Windows.Forms.TextBox tbFind;
	}
}