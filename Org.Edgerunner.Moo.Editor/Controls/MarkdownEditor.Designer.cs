﻿namespace Org.Edgerunner.Moo.Editor.Controls
{
   partial class MarkdownEditor
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

      #region Component Designer generated code

      /// <summary> 
      /// Required method for Designer support - do not modify 
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.components = new System.ComponentModel.Container();
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MarkdownEditor));
         this.splitContainer1 = new System.Windows.Forms.SplitContainer();
         this.textEditor = new FastColoredTextBoxNS.FastColoredTextBox();
         this.webPanel = new TheArtOfDev.HtmlRenderer.WinForms.HtmlPanel();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
         this.splitContainer1.Panel1.SuspendLayout();
         this.splitContainer1.Panel2.SuspendLayout();
         this.splitContainer1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.textEditor)).BeginInit();
         this.SuspendLayout();
         // 
         // splitContainer1
         // 
         this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.splitContainer1.Location = new System.Drawing.Point(0, 0);
         this.splitContainer1.Name = "splitContainer1";
         // 
         // splitContainer1.Panel1
         // 
         this.splitContainer1.Panel1.Controls.Add(this.textEditor);
         // 
         // splitContainer1.Panel2
         // 
         this.splitContainer1.Panel2.Controls.Add(this.webPanel);
         this.splitContainer1.Size = new System.Drawing.Size(306, 256);
         this.splitContainer1.SplitterDistance = 168;
         this.splitContainer1.TabIndex = 0;
         // 
         // textEditor
         // 
         this.textEditor.AccessibleDescription = "Textbox control";
         this.textEditor.AccessibleName = "Fast Colored Text Box";
         this.textEditor.AccessibleRole = System.Windows.Forms.AccessibleRole.Text;
         this.textEditor.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
         this.textEditor.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\r\n^\\s*(case|default)\\s*[^:]*" +
    "(?<range>:)\\s*(?<range>[^;]+);";
         this.textEditor.AutoScrollMinSize = new System.Drawing.Size(31, 18);
         this.textEditor.BackBrush = null;
         this.textEditor.CharHeight = 18;
         this.textEditor.CharWidth = 10;
         this.textEditor.DefaultMarkerSize = 8;
         this.textEditor.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
         this.textEditor.Dock = System.Windows.Forms.DockStyle.Fill;
         this.textEditor.FindForm = null;
         this.textEditor.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.textEditor.GoToForm = null;
         this.textEditor.Hotkeys = resources.GetString("textEditor.Hotkeys");
         this.textEditor.IsReplaceMode = false;
         this.textEditor.Location = new System.Drawing.Point(0, 0);
         this.textEditor.Name = "textEditor";
         this.textEditor.Paddings = new System.Windows.Forms.Padding(0);
         this.textEditor.ReplaceForm = null;
         this.textEditor.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
         this.textEditor.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("textEditor.ServiceColors")));
         this.textEditor.Size = new System.Drawing.Size(168, 256);
         this.textEditor.TabIndex = 0;
         this.textEditor.ToolTipDelay = 100;
         this.textEditor.Zoom = 100;
         this.textEditor.TextChangedDelayed += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.textEditor_TextChangedDelayed);
         // 
         // webPanel
         // 
         this.webPanel.AutoScroll = true;
         this.webPanel.BackColor = System.Drawing.SystemColors.Window;
         this.webPanel.BaseStylesheet = null;
         this.webPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.webPanel.Location = new System.Drawing.Point(0, 0);
         this.webPanel.Name = "webPanel";
         this.webPanel.Size = new System.Drawing.Size(134, 256);
         this.webPanel.TabIndex = 0;
         this.webPanel.Text = null;
         // 
         // MarkdownEditor
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.splitContainer1);
         this.Name = "MarkdownEditor";
         this.Size = new System.Drawing.Size(306, 256);
         this.splitContainer1.Panel1.ResumeLayout(false);
         this.splitContainer1.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
         this.splitContainer1.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.textEditor)).EndInit();
         this.ResumeLayout(false);

      }

        #endregion

        private SplitContainer splitContainer1;
        private FastColoredTextBoxNS.FastColoredTextBox textEditor;
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlPanel webPanel;
    }
}
