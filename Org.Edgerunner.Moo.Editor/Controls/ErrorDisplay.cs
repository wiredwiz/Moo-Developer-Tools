using System;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar.Errors;

namespace Org.Edgerunner.Moo.Editor.Controls
{
   /// <summary>
   /// Class that displays a list of lexer/parser feedback messages.
   /// Implements the <see cref="System.Windows.Forms.ListView" />
   /// </summary>
   /// <seealso cref="System.Windows.Forms.ListView" />
   public partial class ErrorDisplay : ListView
   {
      /// <summary>Initializes a new instance of the <see cref="T:Org.Edgerunner.Moo.Editor.Controls.ErrorDisplay" /> class.</summary>
      public ErrorDisplay()
      {
         InitializeComponent();
         FullRowSelect = true;
         GridLines = true;
         View = View.Details;
         MultiSelect = false;
         HeaderStyle = ColumnHeaderStyle.Nonclickable;
         Columns.Add("colId", "id", 0, HorizontalAlignment.Left, 0);
         Columns.Add("colSource", "Source", 200, HorizontalAlignment.Left, 0);
         Columns.Add("colLineNumber", "Line", 60, HorizontalAlignment.Right, 0);
         Columns.Add("colColumn", "Column", 80, HorizontalAlignment.Right, 0);
         Columns.Add("colMessage", "Message", 600, HorizontalAlignment.Left, 0);
         //ConfigureDisplay();
      }

      /// <summary>
      /// Populates the the display with the specified errors.
      /// </summary>
      /// <param name="errorMessages">The error messages to display.</param>
      public void PopulateErrors(List<ParseMessage> errorMessages)
      {
         Items.Clear();
         foreach (var msg in errorMessages)
         {
            var data = new string[5];
            data[0] = msg.DocumentId;
            data[1] = msg.DocumentName;
            data[2] = msg.LineNumber.ToString();
            data[3] = msg.Column.ToString();
            data[4] = msg.Message;
            var error = new ListViewItem(data)
                        {
                           Tag = msg.Guide
                        };
            Items.Add(error);
         }
         Refresh();
      }
   }
}
