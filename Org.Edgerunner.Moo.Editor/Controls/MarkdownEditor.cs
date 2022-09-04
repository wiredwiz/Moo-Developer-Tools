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
      /// Gets the markdown render panel.
      /// </summary>
      /// <value>
      /// The markdown render panel.
      /// </value>
      public HtmlPanel MarkdownRenderPanel => webPanel;

      /// <summary>
      /// Gets the text editor.
      /// </summary>
      /// <value>
      /// The text editor.
      /// </value>
      public FastColoredTextBoxNS.FastColoredTextBox Editor => textEditor;

      private void textEditor_TextChangedDelayed(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
      {
         webPanel.Text = Markdown.ToHtml(textEditor.Text, _Pipeline);
      }
   }
}
