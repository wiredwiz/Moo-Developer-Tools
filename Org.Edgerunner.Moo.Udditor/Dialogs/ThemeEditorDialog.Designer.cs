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
         this.splitContainer = new System.Windows.Forms.SplitContainer();
         this.leftScrollPanel = new System.Windows.Forms.Panel();
         this.previewHostPanel = new System.Windows.Forms.Panel();
         this.buttonPanel = new System.Windows.Forms.Panel();
         this.btnOk = new System.Windows.Forms.Button();
         this.btnApply = new System.Windows.Forms.Button();
         this.btnCancel = new System.Windows.Forms.Button();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
         this.splitContainer.Panel1.SuspendLayout();
         this.splitContainer.Panel2.SuspendLayout();
         this.splitContainer.SuspendLayout();
         this.buttonPanel.SuspendLayout();
         this.SuspendLayout();
         //
         // splitContainer
         //
         this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
         this.splitContainer.Location = new System.Drawing.Point(0, 0);
         this.splitContainer.Name = "splitContainer";
         this.splitContainer.Orientation = System.Windows.Forms.Orientation.Vertical;
         this.splitContainer.Panel1.Controls.Add(this.leftScrollPanel);
         this.splitContainer.Panel2.Controls.Add(this.previewHostPanel);
         this.splitContainer.Size = new System.Drawing.Size(1260, 600);
         this.splitContainer.SplitterDistance = 430;
         this.splitContainer.TabIndex = 0;
         //
         // leftScrollPanel
         //
         this.leftScrollPanel.AutoScroll = true;
         this.leftScrollPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.leftScrollPanel.Location = new System.Drawing.Point(0, 0);
         this.leftScrollPanel.Name = "leftScrollPanel";
         this.leftScrollPanel.Size = new System.Drawing.Size(430, 600);
         this.leftScrollPanel.TabIndex = 0;
         //
         // previewHostPanel
         //
         this.previewHostPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.previewHostPanel.Location = new System.Drawing.Point(0, 0);
         this.previewHostPanel.Name = "previewHostPanel";
         this.previewHostPanel.Size = new System.Drawing.Size(466, 600);
         this.previewHostPanel.TabIndex = 0;
         //
         // buttonPanel
         //
         this.buttonPanel.Controls.Add(this.btnOk);
         this.buttonPanel.Controls.Add(this.btnApply);
         this.buttonPanel.Controls.Add(this.btnCancel);
         this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
         this.buttonPanel.Location = new System.Drawing.Point(0, 600);
         this.buttonPanel.Name = "buttonPanel";
         this.buttonPanel.Size = new System.Drawing.Size(900, 48);
         this.buttonPanel.TabIndex = 1;
         //
         // btnOk
         //
         this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Right;
         this.btnOk.Location = new System.Drawing.Point(627, 10);
         this.btnOk.Name = "btnOk";
         this.btnOk.Size = new System.Drawing.Size(85, 28);
         this.btnOk.TabIndex = 0;
         this.btnOk.Text = "OK";
         this.btnOk.UseVisualStyleBackColor = true;
         this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
         //
         // btnApply
         //
         this.btnApply.Anchor = System.Windows.Forms.AnchorStyles.Right;
         this.btnApply.Location = new System.Drawing.Point(718, 10);
         this.btnApply.Name = "btnApply";
         this.btnApply.Size = new System.Drawing.Size(85, 28);
         this.btnApply.TabIndex = 1;
         this.btnApply.Text = "Apply";
         this.btnApply.UseVisualStyleBackColor = true;
         this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
         //
         // btnCancel
         //
         this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
         this.btnCancel.Location = new System.Drawing.Point(809, 10);
         this.btnCancel.Name = "btnCancel";
         this.btnCancel.Size = new System.Drawing.Size(85, 28);
         this.btnCancel.TabIndex = 2;
         this.btnCancel.Text = "Cancel";
         this.btnCancel.UseVisualStyleBackColor = true;
         this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
         //
         // ThemeEditorDialog
         //
         this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(1260, 648);
         this.Controls.Add(this.splitContainer);
         this.Controls.Add(this.buttonPanel);
         this.MinimumSize = new System.Drawing.Size(980, 450);
         this.Name = "ThemeEditorDialog";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Editor Theme";
         this.splitContainer.Panel1.ResumeLayout(false);
         this.splitContainer.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
         this.splitContainer.ResumeLayout(false);
         this.buttonPanel.ResumeLayout(false);
         this.ResumeLayout(false);
      }

      #endregion

      private System.Windows.Forms.SplitContainer splitContainer;
      private System.Windows.Forms.Panel leftScrollPanel;
      private System.Windows.Forms.Panel previewHostPanel;
      private System.Windows.Forms.Panel buttonPanel;
      private System.Windows.Forms.Button btnOk;
      private System.Windows.Forms.Button btnApply;
      private System.Windows.Forms.Button btnCancel;
   }
}
