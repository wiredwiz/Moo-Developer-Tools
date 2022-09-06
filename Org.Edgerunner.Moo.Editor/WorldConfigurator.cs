using Org.Edgerunner.Moo.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Org.Edgerunner.Moo.Editor
{
   public partial class WorldConfigurator : Form
   {
      private WorldConfiguration _World;

      public WorldConfigurator()
      {
         InitializeComponent();
      }

      public WorldConfiguration World
      {
         get => _World;
         set
         {
            _World = value;
            txtName.Text = value.Name;
            txtHost.Text = value.HostAddress;
            txtPort.Text = value.PortNumber.ToString();
            txtUser.Text = value.UserInfo.Name;
            txtPassword.Text = value.UserInfo.Password;
            txtConnection.Text = value.UserInfo.ConnectionString;
            chkAutoLogin.Checked = value.UserInfo.AutomaticallyLogin;
            chkPrompt.Checked = value.UserInfo.PromptForCredentials;
            chkEnableEcho.Checked = value.EchoEnabled;
            chkLocalEdit.Checked = value.LocalEditEnabled;
            chkMcp.Checked = value.McpEnabled;
            chkColor.Checked = value.ColorEnabled;
            chkShowOnMenu.Checked = value.ShowAsMenuShortcut;
         }
      }

      private void txtPort_Validating(object sender, CancelEventArgs e)
      {
         if (!int.TryParse(txtPort.Text, out int port))
            e.Cancel = true;
      }

      void UpdateButtons()
      {
         btnOk.Enabled = !int.TryParse(txtPort.Text, out int port) &&
                         port > 0 &&
                         !string.IsNullOrEmpty(txtName.Text) &&
                         !string.IsNullOrEmpty(txtHost.Text);
      }

      private void WorldConfigurator_Load(object sender, EventArgs e)
      {
         UpdateButtons();
      }

      private void btnOk_Click(object sender, EventArgs e)
      {
         DialogResult = DialogResult.OK;
         Close();
      }

      private void txtName_Validated(object sender, EventArgs e)
      {
         World.Name = txtName.Text;
         UpdateButtons();
      }

      private void txtHost_Validated(object sender, EventArgs e)
      {
         World.HostAddress = txtHost.Text;
         UpdateButtons();
      }

      private void txtPort_Validated(object sender, EventArgs e)
      {
         World.PortNumber = int.Parse(txtPort.Text);
         UpdateButtons();
      }
   }
}
