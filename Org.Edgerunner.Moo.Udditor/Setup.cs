using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Krypton.Toolkit;

namespace Org.Edgerunner.Moo.Udditor;
public partial class Setup : KryptonForm
{
   public Setup()
   {
      InitializeComponent();
   }

   /// <summary>
   /// Gets or sets the password.
   /// </summary>
   /// <value>
   /// The password.
   /// </value>
   public string Password
   {
      get => txtPassword.Text;
      set => txtPassword.Text = value;
   }

   private void btnOk_Click(object sender, EventArgs e)
   {
      DialogResult = DialogResult.OK;
   }

    private void txtPassword_Validated(object sender, EventArgs e)
    {
       btnOk.Enabled = !string.IsNullOrEmpty(txtPassword.Text);
    }
}
