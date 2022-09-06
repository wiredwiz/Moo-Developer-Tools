namespace Org.Edgerunner.Moo.Editor.Controls
{
   partial class MooDocumentEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MooDocumentEditor));
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.TextInput = new FastColoredTextBoxNS.FastColoredTextBox();
            this.webPanel = new TheArtOfDev.HtmlRenderer.WinForms.HtmlPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TextInput)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.TextInput);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.webPanel);
            this.splitContainer.Size = new System.Drawing.Size(306, 256);
            this.splitContainer.SplitterDistance = 168;
            this.splitContainer.TabIndex = 0;
            // 
            // TextInput
            // 
            this.TextInput.AccessibleDescription = "Textbox control";
            this.TextInput.AccessibleName = "Fast Colored Text Box";
            this.TextInput.AccessibleRole = System.Windows.Forms.AccessibleRole.Text;
            this.TextInput.AutoCompleteBracketsList = new char[] {
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
            this.TextInput.AutoIndentChars = false;
            this.TextInput.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\r\n^\\s*(case|default)\\s*[^:]*" +
    "(?<range>:)\\s*(?<range>[^;]+);";
            this.TextInput.AutoScrollMinSize = new System.Drawing.Size(0, 18);
            this.TextInput.BackBrush = null;
            this.TextInput.ChangedLineColor = System.Drawing.Color.Yellow;
            this.TextInput.CharHeight = 18;
            this.TextInput.CharWidth = 10;
            this.TextInput.DefaultMarkerSize = 8;
            this.TextInput.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.TextInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextInput.FindForm = null;
            this.TextInput.GoToForm = null;
            this.TextInput.Hotkeys = resources.GetString("TextInput.Hotkeys");
            this.TextInput.IsReplaceMode = false;
            this.TextInput.Location = new System.Drawing.Point(0, 0);
            this.TextInput.Name = "TextInput";
            this.TextInput.Paddings = new System.Windows.Forms.Padding(0);
            this.TextInput.ReplaceForm = null;
            this.TextInput.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.TextInput.ServiceColors = null;
            this.TextInput.Size = new System.Drawing.Size(168, 256);
            this.TextInput.TabIndex = 0;
            this.TextInput.TabLength = 2;
            this.TextInput.ToolTipDelay = 100;
            this.TextInput.WordWrap = true;
            this.TextInput.Zoom = 100;
            this.TextInput.SelectionChanged += new System.EventHandler(this.TextInput_SelectionChanged);
            this.TextInput.TextChangedDelayed += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.textEditor_TextChangedDelayed);
            this.TextInput.Scroll += new System.Windows.Forms.ScrollEventHandler(this.TextInput_Scroll);
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
            // MooDocumentEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Name = "MooDocumentEditor";
            this.Size = new System.Drawing.Size(306, 256);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TextInput)).EndInit();
            this.ResumeLayout(false);

      }

        #endregion

        private SplitContainer splitContainer;
        private FastColoredTextBoxNS.FastColoredTextBox TextInput;
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlPanel webPanel;
    }
}
