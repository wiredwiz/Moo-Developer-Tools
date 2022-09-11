using System;
using Krypton.Toolkit;

namespace Org.Edgerunner.Moo.Udditor;
public partial class ConnectionInfoPrompt : KryptonForm
{
   public ConnectionInfoPrompt()
   {
      InitializeComponent();
   }

   public string HostAddress
   {
      get => txtHost.Text;
      set => txtHost.Text = value;
   }

   public int HostPort
   {
      get => int.Parse(txtPort.Text);
      set => txtPort.Text = value.ToString();
   }

   public bool UseTls
   {
      get => chkTls.Checked;
      set => chkTls.Checked = value;
   }

   private void btnConnect_Click(object sender, EventArgs e)
   {
      DialogResult = DialogResult.OK;
      Close();
   }

   private void ConnectInfo_TextChanged(object sender, EventArgs e)
   {
      btnConnect.Enabled = !string.IsNullOrEmpty(txtHost.Text) && int.TryParse(txtPort.Text, out _);
   }
}
