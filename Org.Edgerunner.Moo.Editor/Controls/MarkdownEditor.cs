using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Markdig;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar.Errors;
using Org.Edgerunner.ANTLR4.Tools.Common;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace Org.Edgerunner.Moo.Editor.Controls
{
   public partial class MarkdownEditor : UserControl
   {
      private readonly MarkdownPipeline _Pipeline;

      public MarkdownEditor()
      {
         InitializeComponent();
         _Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
      }

      /// <summary>
      /// Gets the markdown preview panel.
      /// </summary>
      /// <value>
      /// The markdown preview panel.
      /// </value>
      public HtmlPanel MarkdownPreviewPanel => webPanel;

      /// <summary>
      /// Gets the text editor.
      /// </summary>
      /// <value>
      /// The text editor.
      /// </value>
      public FastColoredTextBoxNS.FastColoredTextBox Input => TextInput;

      [Browsable(false)]
      public DocumentInfo Document { get; set; }

      /// <summary>
      /// Allows previewing of markdown text.
      /// </summary>
      [DefaultValue(false)]
      [Description("Allows previewing of markdown text.")]
      public bool EnablePreview
      {
         get => !splitContainer.Panel2Collapsed && splitContainer.Panel2.Visible;
         set
         {
            if (value)
            {
               splitContainer.Panel2Collapsed = false;
               splitContainer.Panel2.Show();
            }
            else
            {
               splitContainer.Panel2Collapsed = true;
               splitContainer.Panel2.Hide();
            }
         }
      }

      private void textEditor_TextChangedDelayed(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
      {
         webPanel.Text = Markdown.ToHtml(TextInput.Text, _Pipeline);
      }
   }
}
