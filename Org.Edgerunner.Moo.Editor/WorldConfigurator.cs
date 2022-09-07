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
using Org.Edgerunner.Moo.Common.Encryption;

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
            txtPassword.Text = value.UserInfo.DecryptedPassword;
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
         btnOk.Enabled = int.TryParse(txtPort.Text, out int port) &&
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
         UpdateWorld();
         Close();
      }

      private void Text_Validated(object sender, EventArgs e)
      {
         UpdateButtons();
      }

      private void UpdateWorld()
      {
         World.Name = txtName.Text;
         World.HostAddress = txtHost.Text;
         World.PortNumber = int.Parse(txtPort.Text);
         World.UserInfo.Name = txtUser.Text;
         World.UserInfo.DecryptedPassword = txtPassword.Text;
         World.UserInfo.ConnectionString = txtConnection.Text;
         World.UserInfo.AutomaticallyLogin = chkAutoLogin.Checked;
         World.UserInfo.PromptForCredentials = chkPrompt.Checked;
         World.EchoEnabled = chkEnableEcho.Checked;
         World.ColorEnabled = chkColor.Checked;
         World.LocalEditEnabled = chkLocalEdit.Checked;
         World.McpEnabled = chkMcp.Checked;
         World.ShowAsMenuShortcut = chkShowOnMenu.Checked;
         World.UseTls = chkUseTLS.Checked;
      }

      private void btnShowPassword_CheckedChanged(object sender, EventArgs e)
      {
         txtPassword.UseSystemPasswordChar = !btnShowPassword.Checked;
         txtPassword.Invalidate();
      }
   }
}
