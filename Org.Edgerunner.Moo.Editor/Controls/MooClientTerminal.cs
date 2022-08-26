using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Org.Edgerunner.Common.Extensions;
using Org.Edgerunner.Moo.Communication;
using Org.Edgerunner.Moo.Communication.Interfaces;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Forms.Application;

namespace Org.Edgerunner.Moo.Editor.Controls
{
   public partial class MooClientTerminal : UserControl
   {
      private IMooClientSession _Session;

      private int _ReadingCommands;

      public string Host => _Session.Host;

      public int Port => _Session.Port;

      public string World => _Session.World;

      public MooClientTerminal()
      {
         InitializeComponent();
         ConsoleForeColor = Color.WhiteSmoke;
         ConsoleBackgroundColor = Color.Black;
         consoleSim.Text = string.Empty;
         _ReadingCommands = 0;
      }

      protected override void OnHandleDestroyed(EventArgs e)
      {
         Close();
         base.OnHandleDestroyed(e);
      }

      public Color ConsoleForeColor
      {
         get => consoleSim.ConsoleForeColor;
         set => consoleSim.ConsoleForeColor = value;
      }

      public Color ConsoleBackgroundColor
      {
         get => consoleSim.ConsoleBackgroundColor;
         set
         {
            consoleSim.ConsoleBackgroundColor = value;
            pnlSpacer.BackColor = value;
         }
      }

      private void txtInput_KeyDown(object sender, KeyEventArgs e)
      {
         if (e.KeyCode == Keys.Enter && !e.Control)
         {
            SendTextLines(txtInput.Text.Split('\n'));

            txtInput.Clear();
            e.SuppressKeyPress = true;
         }
      }

      public void SendTextLines(IEnumerable<string> text)
      {
         foreach (var line in text)
            SendTextLine(line);
      }

      public void SendTextLine(string text)
      {
         _Session.SendLine(text);
      }

      public void SendText(string text)
      {
         _Session.Send(text);
      }

      public void SendOutOfBandCommands(IEnumerable<string> commands)
      {
         foreach (var command in commands)
            _Session.SendOutOfBandLine(command);
      }

      public void SendOutOfBandCommand(string command)
      {
         _Session.SendOutOfBandLine(command);
      }

      public void Connect(string world, string host, int port)
      {
         _Session = MooClient.Create<MooClientSession>(world, host, port);
         _Session.Closed += Session_Closed;
         _Session.DataReceived += Session_DataReceived;
         _Session.DataDropped += Session_DataDropped;
         _Session.Open(host, port);
      }

      private void Session_DataDropped(object sender, int e)
      {
         Debug.WriteLine($"{e} bytes dropped from buffer overflow");
      }

      private void Session_DataReceived(object sender, EventArgs e)
      {
         var reading = Interlocked.Exchange(ref _ReadingCommands, 1);
         if (reading != 0)
            return;

         if (!_Session.CommandBuffer.IsEmpty)
         {
            var decoder = Encoding.UTF8.GetDecoder();
            while (_Session.CommandBuffer.PopTextLine(decoder) is { } line)
            {
               consoleSim.WriteAnsiLine(line);
               consoleSim.GoEnd();
               Application.DoEvents();
            }
         }

         if (!_Session.OutOfBandCommandBuffer.IsEmpty)
         {
            var decoder = Encoding.ASCII.GetDecoder();
            while (_Session.OutOfBandCommandBuffer.PopTextLine(decoder) is { } line)
               Debug.WriteLine($"OOB: {line}");
         }

         _ReadingCommands = 0;
      }

      private void Session_Closed(object sender, EventArgs e)
      {
         Debug.WriteLine("** Session was closed **");
      }

      public void Close()
      {
         try
         {
            _Session.Close();
         }
         catch (SocketException ex)
         {
            Debug.WriteLine(ex);
         }
      }
   }
}
