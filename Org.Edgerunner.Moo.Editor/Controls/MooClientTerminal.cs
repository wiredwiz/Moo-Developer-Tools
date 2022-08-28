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
using FastColoredTextBoxNS.Types;
using Org.Edgerunner.Common.Extensions;
using Org.Edgerunner.Moo.Communication;
using Org.Edgerunner.Moo.Communication.Buffers;
using Org.Edgerunner.Moo.Communication.Interfaces;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Forms.Application;

namespace Org.Edgerunner.Moo.Editor.Controls
{
   public partial class MooClientTerminal : UserControl
   {
      private IMooClientSession _Session;

      private int _ReadingCommands;

      private int _ReadingOOB;

      private bool _FreshConnection;

      private readonly CommandBuffer _InputCommandBuffer;

      private bool _UserInteraction;

      public string Host => _Session.Host;

      public int Port => _Session.Port;

      public string World => _Session.World;

      public bool EchoEnabled { get; set; }

      public MooClientTerminal()
      {
         InitializeComponent();
         ConsoleForeColor = Color.WhiteSmoke;
         ConsoleBackgroundColor = Color.Black;
         consoleSim.Text = string.Empty;
         _ReadingCommands = 0;
         _ReadingOOB = 0;
         _InputCommandBuffer = new CommandBuffer(15);
         _UserInteraction = false;
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
            if (!_FreshConnection || !IsConnecting(txtInput.Text))
            {
               SendTextLines(txtInput.Text.Split('\n'));

               if (!_InputCommandBuffer.IsEmpty && _InputCommandBuffer[0] != txtInput.Text)
                  _InputCommandBuffer.PushFront(txtInput.Text);
               else if (_InputCommandBuffer.IsEmpty)
                  _InputCommandBuffer.PushFront(txtInput.Text);
               _InputCommandBuffer.CurrentPosition = 0;
               txtInput.SelectAll();
            }
            else
            {
               var existing = consoleSim.AnsiManager.EchoEnabled;
               consoleSim.AnsiManager.EchoEnabled = false;
               SendTextLines(txtInput.Text.Split('\n'));
               consoleSim.AnsiManager.EchoEnabled = existing;
               _UserInteraction = true;
               txtInput.Text = string.Empty;
               txtInput.UseSystemPasswordChar = true;
            }

            e.SuppressKeyPress = true;
         }
         else if (e.KeyCode == Keys.Up)
         {
            if (txtInput.SelectionLength == txtInput.Text.Length)
            {
               if (!_InputCommandBuffer.IsEmpty)
               {
                  txtInput.Text = _InputCommandBuffer.MoveBackInBuffer();
                  txtInput.SelectAll();
               }

               e.SuppressKeyPress = true;
            }
         }
         else if (e.KeyCode == Keys.Down)
         {
            if (txtInput.SelectionLength == txtInput.Text.Length)
            {
               if (!_InputCommandBuffer.IsEmpty)
               {
                  txtInput.Text = _InputCommandBuffer.MoveForwardInBuffer();
                  txtInput.SelectAll();
               }

               e.SuppressKeyPress = true;
            }
         }
      }

      private void txtInput_TextChanged(object sender, EventArgs e)
      {
         if (_FreshConnection)
         {
            if (consoleSim.Lines[^1].ToLowerInvariant().Contains("password:"))
               txtInput.UseSystemPasswordChar = false;
            else
               txtInput.UseSystemPasswordChar = string.IsNullOrEmpty(txtInput.Text) || !IsConnecting(txtInput.Text);

            if (txtInput.Text != null)
               txtInput.SelectionStart = txtInput.Text.Length;
         }
         else
         {
            txtInput.UseSystemPasswordChar = true;
            txtInput.TextChanged -= txtInput_TextChanged;
         }
      }

      private static bool IsConnecting(string text)
      {
         if (text.Length >= 3)
         {
            var words = text.ToLowerInvariant().Split(' ').ToList();
            if (words[^1] == "")
               words.RemoveAt(words.Count - 1);
            var connecting = false;
            switch (words.Count)
            {
               case 3 when words[0][..] == "connect"[..words[0].Length]:
               case 2 when text[^1] == ' ' &&
                           words[0][..] == "connect"[..words[0].Length]:
                  connecting = true;
                  break;
            }

            return connecting;
         }

         return false;
      }

      public void SendTextLines(IEnumerable<string> text)
      {
         foreach (var line in text)
            SendTextLine(line);
      }

      public void SendTextLine(string text)
      {
         _Session.SendLine(text);
         consoleSim.EchoTextLine(text);
      }

      public void SendText(string text)
      {
         _Session.Send(text);
         consoleSim.EchoText(text);
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
         _Session.OutOfBandCommandReceived += Session_OutOfBandCommandReceived;
         _Session.DataDropped += Session_DataDropped;
         _Session.Open(host, port);
         _FreshConnection = true;
      }

      private void Session_DataDropped(object sender, int e)
      {
         Debug.WriteLine($"{e} bytes dropped from buffer overflow");
      }

      private void Session_DataReceived(object sender, string e)
      {
         var reading = Interlocked.Exchange(ref _ReadingCommands, 1);
         if (reading == 1)
            return;

         var existingLines = consoleSim.Lines.Count;
         while (!_Session.CommandQueue.IsEmpty)
         {
            if (_Session.CommandQueue.TryDequeue(out var text))
            {
               consoleSim.WriteAnsi(text);
               consoleSim.GoEnd();
            }
         }

         if (_FreshConnection && _UserInteraction)
            if (consoleSim.Lines.Count - existingLines > 2)
               _FreshConnection = false;

         _ReadingCommands = 0;
      }

      private void Session_OutOfBandCommandReceived(object sender, string e)
      {
         var reading = Interlocked.Exchange(ref _ReadingOOB, 1);
         if (reading == 1)
            return;

         while (!_Session.OutOfBandCommandQueue.IsEmpty)
            if (_Session.OutOfBandCommandQueue.TryDequeue(out var command))
               Debug.WriteLine($"OOB: {command}");

         _ReadingOOB = 0;
      }

      private void Session_Closed(object sender, EventArgs e)
      {
         Debug.WriteLine("** Session was closed **");
         consoleSim.WriteLine("** Session closed by server **");
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
