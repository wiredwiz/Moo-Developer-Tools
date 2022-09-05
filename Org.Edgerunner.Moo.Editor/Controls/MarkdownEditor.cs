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
using Org.Edgerunner.Moo.MooText;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace Org.Edgerunner.Moo.Editor.Controls
{
   public partial class MarkdownEditor : UserControl
   {
      private readonly MarkdownPipeline _MarkdownPipeline;
      private readonly MooTextPipeline _MooPipeline;
      private Color _PreviewPaneBackgroundColor;
      private Color _PreviewPaneForegroundColor;
      private int _LastLineNo;

      public MarkdownEditor()
      {
         InitializeComponent();
         _MarkdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
         _MooPipeline = new MooTextPipelineBuilder().ProcessColors().Build();
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

      [DefaultValue(false)]
      [DisplayName("Enable Markdown")]
      [Description("Enables processing of markdown in the text.")]
      public bool EnableMarkdownProcessing { get; set; }

      [DefaultValue(true)]
      [DisplayName("Enable Moo Text")]
      [Description("Enables processing of Moo Text codes.")]
      public bool EnableMooTextProcessing { get; set; } = true;

      [DisplayName("Preview Pane Color")]
      [Description("Determines the background color of the preview pane.")]
      public Color PreviewPaneBackgroundColor
      {
         get => _PreviewPaneBackgroundColor;
         set
         {
            _PreviewPaneBackgroundColor = value;
            webPanel.BackColor = value;
         }
      }

      [DisplayName("Preview Pane Text Color")]
      [Description("Determines the text color of the preview pane.")]
      public Color PreviewPaneForegroundColor
      {
         get => _PreviewPaneForegroundColor;
         set
         {
            _PreviewPaneForegroundColor = value;
            webPanel.ForeColor = value;
         }
      }

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
               SetPreviewPanelScroll();
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
         var working = TextInput.Text;

         if (EnableMooTextProcessing)
            working = MooText.MooText.ToHtml(working, _MooPipeline);

         if (EnableMarkdownProcessing)
            working = Markdown.ToHtml(working, _MarkdownPipeline);

         working = $"<!DOCTYPE html><html><body color=\"{ColorTranslator.ToHtml(PreviewPaneForegroundColor)}\">" + working + "</body></html>";
         var currentVerticalScroll = webPanel.VerticalScroll.Value;
         webPanel.Text = working;
         var newScroll = CalculateVerticalScroll(_LastLineNo);
         //while (webPanel.VerticalScroll.Value != newScroll)
            webPanel.VerticalScroll.Value = newScroll;
      }

      private void TextInput_Scroll(object sender, ScrollEventArgs e)
      {
         if (splitContainer.Panel2.Visible)
            SetPreviewPanelScroll();
      }

      private void SetPreviewPanelScroll()
      {
         if (webPanel.VerticalScroll.Visible)
         {
            double range = TextInput.VerticalScroll.Maximum - TextInput.ClientRectangle.Height;
            var percent = TextInput.VerticalScroll.Value / range;
            var newValue = (int)Math.Truncate(
                                              (webPanel.VerticalScroll.Maximum -
                                               webPanel.VerticalScroll.LargeChange +
                                               webPanel.VerticalScroll.SmallChange) * percent);
            webPanel.VerticalScroll.Value = Math.Max(Math.Min(newValue, webPanel.VerticalScroll.Maximum), 0);
         }
      }

      private void TextInput_SelectionChanged(object sender, EventArgs e)
      {
         if (TextInput.Lines.Count == 0)
            return;

         if (_LastLineNo == TextInput.Selection.Start.iLine + 1)
            return;

         _LastLineNo = TextInput.Selection.Start.iLine + 1;

         webPanel.VerticalScroll.Value = CalculateVerticalScroll(_LastLineNo);
         webPanel.PerformLayout();
      }

      private int CalculateVerticalScroll(int lineNo)
      {
         if (lineNo == 1)
            return 0;

         var max = webPanel.VerticalScroll.Maximum - webPanel.VerticalScroll.LargeChange + 1;
         var percentage = lineNo / (double)TextInput.Lines.Count;
         var newValue = (int)Math.Truncate(max * percentage);
         return Math.Max(Math.Min(newValue, max), 0);
      }
   }
}
