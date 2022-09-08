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

namespace Org.Edgerunner.Moo.Editor
{
   public partial class CredentialsPrompt : KryptonForm
   {
      public CredentialsPrompt()
      {
         InitializeComponent();
      }

      /// <summary>
      /// Gets the name of the user.
      /// </summary>
      /// <value>
      /// The name of the user.
      /// </value>
      public string UserName
      {
         get => txtName.Text;
         set => txtName.Text = value;
      }

      /// <summary>
      /// Gets the user password.
      /// </summary>
      /// <value>
      /// The user password.
      /// </value>
      public string Password
      {
         get => txtPassword.Text;
         set => txtPassword.Text = value;
      }

      private void btnOk_Click(object sender, EventArgs e)
      {
         DialogResult = DialogResult.OK;
         Close();
      }

      private void Credentials_Validated(object sender, EventArgs e)
      {
         UpdateButtonStatus();
      }

      private void UpdateButtonStatus()
      {
         btnOk.Enabled = !string.IsNullOrEmpty(txtName.Text) && !string.IsNullOrEmpty(txtPassword.Text);
      }

      private void CredentialsPrompt_Load(object sender, EventArgs e)
      {
         UpdateButtonStatus();
      }
   }
}
