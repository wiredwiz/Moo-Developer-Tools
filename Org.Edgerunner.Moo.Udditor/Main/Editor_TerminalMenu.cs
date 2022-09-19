#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="Editor_TerminalMenu.cs">
// Copyright (c)  2022
// </copyright>
// 
// BSD 3-Clause License
// 
// Copyright (c) 2022,
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
// 
// 3. Neither the name of the copyright holder nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

using System.Diagnostics;
using Org.Edgerunner.Moo.Editor;
using Org.Edgerunner.Moo.Udditor.Pages;
using Org.Edgerunner.Mud.Common;

namespace Org.Edgerunner.Moo.Udditor.Main;

public partial class Editor
{
      private async Task OpenTerminalConnectionAsync(string host,int port, string world, bool useTls = false)
   {
      Logger.Trace($"Opening address {host} via terminal menu {(useTls ? "with" : "without")} TLS");
      TerminalPage page = CurrentPage as TerminalPage;
      if (page == null || page.Terminal.IsConnected)
      {
         page = WindowManager.CreateTerminalPage(host);
      }
      else
      {
         page.Text = world;
         page.TextTitle = world;
         page.TextDescription = world;
      }

      WindowManager.ShowPage(page);
      try
      {
         page.Terminal.FocusOnInput();
         UpdateTerminalMenu();
         await page.Terminal.ConnectAsync(world, host, port, useTls).ConfigureAwait(true);
      }
      catch (Exception ex)
      {
         Logger.Error(ex, "Failed to open connection");
         if (useTls && ex is IOException)
            MessageBox.Show("This host address may not support TLS", "Unable to connect");
         else
            MessageBox.Show(ex.Message, "Unable to connect");
      }
   }

   private async Task OpenTerminalConnectionAsync(WorldConfiguration world)
   {
      Logger.Trace($"Opening world {world.Name} via terminal menu {(world.UseTls ? "with" : "without")} TLS");
      var userName = world.UserInfo.Name;
      var password = world.UserInfo.DecryptedPassword;
      if (world.UserInfo.PromptForCredentials)
         if (!PromptForCredentials(ref userName, ref password))
            return;

      TerminalPage page = CurrentPage as TerminalPage;
      if (page == null || page.Terminal.IsConnected)
      {
         page = WindowManager.CreateTerminalPage(world.Name);
      }
      else
      {
         page.Text = world.Name;
         page.TextTitle = world.Name;
         page.TextDescription = world.Name;
      }

      WindowManager.ShowPage(page);
      try
      {
         page.AnsiColorEnabled = world.ColorEnabled;
         page.AsciiBellEnabled = world.AudioEnabled;
         page.Terminal.EchoEnabled = world.EchoEnabled;
         page.BlinkingTextEnabled = world.BlinkEnabled;
         UpdateTerminalMenu();
         await page.Terminal.ConnectAsync(world.Name, world.HostAddress, world.PortNumber, world.UseTls).ConfigureAwait(true);
         if (world.UserInfo.AutomaticallyLogin && !string.IsNullOrEmpty(userName))
         {
            // ReSharper disable once TooManyChainedReferences
            var loginText = world.UserInfo.ConnectionString
                                 .Replace("%u", userName)
                                 .Replace("%p", password);
            page.Terminal.SendLoginTextLine(loginText);
         }
         page.Terminal.FocusOnInput();
      }
      catch (Exception ex)
      {
         Logger.Error(ex, "Failed to open connection");
         if (world.UseTls && ex is IOException)
            MessageBox.Show("This host address may not support TLS", "Unable to connect");
         else
            MessageBox.Show(ex.Message, "Unable to connect");
      }
   }

   private bool PromptForCredentials(ref string userName, ref string password)
   {
      var prompt = new CredentialsPrompt();
      prompt.StartPosition = FormStartPosition.CenterParent;
      prompt.UserName = userName;
      prompt.Password = password;
      if (prompt.ShowDialog(this) != DialogResult.OK)
         return false;

      userName = prompt.UserName;
      password = prompt.Password;
      return true;
   }
   private async void Manager_ConnectToWorld(object sender, WorldConfiguration e)
   {
      await OpenTerminalConnectionAsync(e);
   }
   
   private async void WorldShortcut_Click(object sender, EventArgs e)
   {
      if (sender is ToolStripMenuItem { Tag: WorldConfiguration world })
      {
         Debug.WriteLine($"World {world.Name} clicked");
         await OpenTerminalConnectionAsync(world);
      }
   }

   private async void tlMnuItemOpenConnection_Click(object sender, EventArgs e)
   {
      var prompt = new ConnectionInfoPrompt();
      prompt.StartPosition = FormStartPosition.CenterParent;
      var result = prompt.ShowDialog();
      if (result == DialogResult.OK)
      {
         var world = $"{prompt.HostAddress}:{prompt.HostPort}";
         await OpenTerminalConnectionAsync(prompt.HostAddress, prompt.HostPort, world, prompt.UseTls);
      }
   }

   private void tlMnuItemCloseConnection_Click(object sender, EventArgs e)
   {
      if (CurrentPage is TerminalPage page)
         page.Terminal.Close();
   }

   private void mnuItemEchoCommands_CheckStateChanged(object sender, EventArgs e)
   {
      if (CurrentPage is TerminalPage page)
         page.Terminal.EchoEnabled = mnuItemEchoCommands.CheckState == CheckState.Checked;
   }

   private void mnuItemWorldManager_Click(object sender, EventArgs e)
   {
      var manager = new WorldManager();
      var book = GetWorldsAddressBook();
      manager.LoadAddressBook(book);
      manager.SourceFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Worlds.xml");
      manager.ConnectToWorld += Manager_ConnectToWorld;
      manager.ShowDialog(this);
      BuildTerminalShortcutMenu();
   }

   private void mnuItemEnableColor_CheckStateChanged(object sender, EventArgs e)
   {
      if (CurrentPage is TerminalPage page)
         page.AnsiColorEnabled = mnuItemEnableColor.CheckState == CheckState.Checked;
   }

   private void mnuItemEnableBlinking_CheckStateChanged(object sender, EventArgs e)
   {
      if (CurrentPage is TerminalPage page)
         page.BlinkingTextEnabled = mnuItemEnableBlinking.CheckState == CheckState.Checked;
   }

   private void mnuItemEnableAudio_CheckStateChanged(object sender, EventArgs e)
   {
      if (CurrentPage is TerminalPage page)
         page.AsciiBellEnabled = mnuItemEnableAudio.CheckState == CheckState.Checked;
   }
}
