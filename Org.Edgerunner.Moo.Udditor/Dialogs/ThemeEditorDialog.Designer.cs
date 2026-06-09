#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="ThemeEditorDialog.Designer.cs">
// Copyright (c) Thaddeus Ryker 2026
// </copyright>
//
// BSD 3-Clause License
//
// Copyright (c) 2026,
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

namespace Org.Edgerunner.Moo.Udditor.Dialogs
{
   partial class ThemeEditorDialog
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
            components.Dispose();

         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.splitContainer = new Krypton.Toolkit.KryptonSplitContainer();
         this.leftScrollPanel = new Krypton.Toolkit.KryptonPanel();
         this.previewHostPanel = new Krypton.Toolkit.KryptonPanel();
         this.headerPanel = new Krypton.Toolkit.KryptonPanel();
         this.lblMode = new Krypton.Toolkit.KryptonLabel();
         this.cboMode = new Krypton.Toolkit.KryptonComboBox();
         this.btnImport = new Krypton.Toolkit.KryptonButton();
         this.btnExport = new Krypton.Toolkit.KryptonButton();
         this.buttonPanel = new Krypton.Toolkit.KryptonPanel();
         this.btnOk = new Krypton.Toolkit.KryptonButton();
         this.btnApply = new Krypton.Toolkit.KryptonButton();
         this.btnCancel = new Krypton.Toolkit.KryptonButton();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer.Panel1)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer.Panel2)).BeginInit();
         this.splitContainer.Panel1.SuspendLayout();
         this.splitContainer.Panel2.SuspendLayout();
         this.splitContainer.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.leftScrollPanel)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.previewHostPanel)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.headerPanel)).BeginInit();
         this.headerPanel.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.cboMode)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.buttonPanel)).BeginInit();
         this.buttonPanel.SuspendLayout();
         this.SuspendLayout();
         //
         // splitContainer
         //
         this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
         this.splitContainer.Location = new System.Drawing.Point(0, 40);
         this.splitContainer.Name = "splitContainer";
         this.splitContainer.Orientation = System.Windows.Forms.Orientation.Vertical;
         this.splitContainer.Panel1.Controls.Add(this.leftScrollPanel);
         this.splitContainer.Panel2.Controls.Add(this.previewHostPanel);
         this.splitContainer.Size = new System.Drawing.Size(1260, 560);
         this.splitContainer.SplitterDistance = 560;
         this.splitContainer.TabIndex = 1;
         //
         // leftScrollPanel
         //
         this.leftScrollPanel.AutoScroll = true;
         this.leftScrollPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.leftScrollPanel.Location = new System.Drawing.Point(0, 0);
         this.leftScrollPanel.Name = "leftScrollPanel";
         this.leftScrollPanel.Size = new System.Drawing.Size(430, 560);
         this.leftScrollPanel.TabIndex = 0;
         //
         // previewHostPanel
         //
         this.previewHostPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.previewHostPanel.Location = new System.Drawing.Point(0, 0);
         this.previewHostPanel.Name = "previewHostPanel";
         this.previewHostPanel.Size = new System.Drawing.Size(466, 560);
         this.previewHostPanel.TabIndex = 0;
         //
         // headerPanel
         //
         this.headerPanel.Controls.Add(this.lblMode);
         this.headerPanel.Controls.Add(this.cboMode);
         this.headerPanel.Controls.Add(this.btnImport);
         this.headerPanel.Controls.Add(this.btnExport);
         this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
         this.headerPanel.Location = new System.Drawing.Point(0, 0);
         this.headerPanel.Name = "headerPanel";
         this.headerPanel.Size = new System.Drawing.Size(1260, 40);
         this.headerPanel.TabIndex = 0;
         //
         // lblMode
         //
         this.lblMode.Location = new System.Drawing.Point(8, 8);
         this.lblMode.Name = "lblMode";
         this.lblMode.Size = new System.Drawing.Size(45, 23);
         this.lblMode.TabIndex = 0;
         this.lblMode.Values.Text = "Mode:";
         //
         // cboMode
         //
         this.cboMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cboMode.Location = new System.Drawing.Point(59, 7);
         this.cboMode.Name = "cboMode";
         this.cboMode.Size = new System.Drawing.Size(120, 22);
         this.cboMode.TabIndex = 1;
         this.cboMode.SelectedIndexChanged += new System.EventHandler(this.cboMode_SelectedIndexChanged);
         //
         // btnImport
         //
         this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.btnImport.Location = new System.Drawing.Point(1058, 6);
         this.btnImport.Name = "btnImport";
         this.btnImport.Size = new System.Drawing.Size(90, 25);
         this.btnImport.TabIndex = 2;
         this.btnImport.Values.Text = "Import…";
         this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
         //
         // btnExport
         //
         this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.btnExport.Location = new System.Drawing.Point(1154, 6);
         this.btnExport.Name = "btnExport";
         this.btnExport.Size = new System.Drawing.Size(90, 25);
         this.btnExport.TabIndex = 3;
         this.btnExport.Values.Text = "Export…";
         this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
         //
         // buttonPanel
         //
         this.buttonPanel.Controls.Add(this.btnOk);
         this.buttonPanel.Controls.Add(this.btnApply);
         this.buttonPanel.Controls.Add(this.btnCancel);
         this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
         this.buttonPanel.Location = new System.Drawing.Point(0, 600);
         this.buttonPanel.Name = "buttonPanel";
         this.buttonPanel.Size = new System.Drawing.Size(1260, 48);
         this.buttonPanel.TabIndex = 2;
         //
         // btnOk
         //
         this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Right;
         this.btnOk.Location = new System.Drawing.Point(987, 10);
         this.btnOk.Name = "btnOk";
         this.btnOk.Size = new System.Drawing.Size(85, 28);
         this.btnOk.TabIndex = 0;
         this.btnOk.Values.Text = "OK";
         this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
         //
         // btnApply
         //
         this.btnApply.Anchor = System.Windows.Forms.AnchorStyles.Right;
         this.btnApply.Location = new System.Drawing.Point(1078, 10);
         this.btnApply.Name = "btnApply";
         this.btnApply.Size = new System.Drawing.Size(85, 28);
         this.btnApply.TabIndex = 1;
         this.btnApply.Values.Text = "Apply";
         this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
         //
         // btnCancel
         //
         this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
         this.btnCancel.Location = new System.Drawing.Point(1169, 10);
         this.btnCancel.Name = "btnCancel";
         this.btnCancel.Size = new System.Drawing.Size(85, 28);
         this.btnCancel.TabIndex = 2;
         this.btnCancel.Values.Text = "Cancel";
         this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
         //
         // ThemeEditorDialog
         //
         this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(1260, 648);
         this.Controls.Add(this.splitContainer);
         this.Controls.Add(this.headerPanel);
         this.Controls.Add(this.buttonPanel);
         this.MinimumSize = new System.Drawing.Size(980, 450);
         this.Name = "ThemeEditorDialog";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Editor Theme";
         this.splitContainer.Panel1.ResumeLayout(false);
         this.splitContainer.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer.Panel1)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer.Panel2)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
         this.splitContainer.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.leftScrollPanel)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.previewHostPanel)).EndInit();
         this.headerPanel.ResumeLayout(false);
         this.headerPanel.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.headerPanel)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.cboMode)).EndInit();
         this.buttonPanel.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.buttonPanel)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();
      }

      #endregion

      private Krypton.Toolkit.KryptonSplitContainer splitContainer;
      private Krypton.Toolkit.KryptonPanel leftScrollPanel;
      private Krypton.Toolkit.KryptonPanel previewHostPanel;
      private Krypton.Toolkit.KryptonPanel headerPanel;
      private Krypton.Toolkit.KryptonLabel lblMode;
      private Krypton.Toolkit.KryptonComboBox cboMode;
      private Krypton.Toolkit.KryptonButton btnImport;
      private Krypton.Toolkit.KryptonButton btnExport;
      private Krypton.Toolkit.KryptonPanel buttonPanel;
      private Krypton.Toolkit.KryptonButton btnOk;
      private Krypton.Toolkit.KryptonButton btnApply;
      private Krypton.Toolkit.KryptonButton btnCancel;
   }
}
