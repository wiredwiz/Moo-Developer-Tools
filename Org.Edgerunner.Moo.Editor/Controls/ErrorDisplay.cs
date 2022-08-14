using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Org.Edgerunner.ANTLR4.Tools.Common;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar.Errors;
using Org.Edgerunner.Moo.Editor.Configuration;

namespace Org.Edgerunner.Moo.Editor.Controls
{
   public partial class ErrorDisplay : ListView
   {
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
            var error = new ListViewItem(data);
            Items.Add(error);
         }
         Refresh();
      }

      private void ConfigureDisplay()
      {
         var font = new Font(Settings.Instance.EditorFontFamily, Settings.Instance.EditorFontSize);
         Font = font;
      }
   }
}
