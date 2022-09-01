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
using Org.Edgerunner.Mud.MCP;
using Org.Edgerunner.Mud.MCP.Exceptions;
using Org.Edgerunner.Mud.MCP.Interfaces;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Forms.Application;

namespace Org.Edgerunner.Moo.Editor.Controls
{
    public partial class MooClientTerminal : UserControl
   {
      private IMooClientSession _Session;

      private int _ReadingCommands;

      private bool _LoggedInConnection;

      private readonly CommandBuffer _InputCommandBuffer;

      private bool _UserInteraction;

      private bool _LastCommandAppearedToBeALogin;

      protected McpClientSessionManager McpSessionManager { get; set; }

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
         _InputCommandBuffer = new CommandBuffer(15);
         _UserInteraction = false;
         _LastCommandAppearedToBeALogin = false;
         McpSessionManager = new McpClientSessionManager(new Version(2,1), new Version(2,1), new List<IMcpPackage>());
         ActiveControl = txtInput;
         splitContainer1.ActiveControl = txtInput;
      }

      /// <summary>
      /// Occurs when [new message(s) received].
      /// </summary>
        public event EventHandler NewMessageReceived;

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
            if (_LoggedInConnection || !IsConnecting(txtInput.Text))
            {
               SendTextLines(txtInput.Text.Split('\n'));

               _LastCommandAppearedToBeALogin = false;
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
               _LastCommandAppearedToBeALogin = true;
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
         if (!_LoggedInConnection)
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
         consoleSim.EchoTextLine(text);
         _Session.SendLine(text);
      }

      public void SendText(string text)
      {
         consoleSim.EchoText(text);
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
         _Session.MessageReceived += SessionMessageReceived;
         _Session.DataDropped += Session_DataDropped;
         _Session.Open(host, port);
         _LoggedInConnection = false;
      }

      private void Session_DataDropped(object sender, int e)
      {
         Debug.WriteLine($"{e} bytes dropped from buffer overflow");
      }

      private void SessionMessageReceived(object sender, ClientMessageEventArgs e)
      {
         // While other client events are executed on the UI thread
         // This one is not, because it may perform long running operations.
         var reading = Interlocked.Exchange(ref _ReadingCommands, 1);
         if (reading == 1)
            return;

         // We define a local function to fetch the console line count to be used via thread invocation
         int GetLineCount() => consoleSim.Lines.Count;

         var existingLineCount = consoleSim.Invoke((Func<int>)GetLineCount);
         while (!_Session.CommandQueue.IsEmpty)
         {
            if (_Session.CommandQueue.TryDequeue(out var message))
            {
               if (message.OutOfBand)
               {
                  Debug.WriteLine($"OOB: {message.Text}");
                  try
                  {
                     var parsed = McpUtils.ParseMessage(message.Text);
                     if (parsed != null)
                        if (McpSessionManager.IsNegotiationMessage(parsed))
                        {
                           var result = McpSessionManager.NegotiationMcpSession(parsed);
                           Debug.WriteLine($"Handshake: {result?.Handshake()}");
                        }
                  }
                  catch (InvalidMcpMessageFormatException ex)
                  {
                     Debug.WriteLine(ex.Message);
                  }
               }
               else
               {
                  void SafeWrite()
                  {
                     var atBottom = consoleSim.VerticalScrollbarPositionedAtBottom;
                     consoleSim.WriteAnsi(message.Text);
                     if (atBottom)
                        consoleSim.GoEnd();
                  }

                  consoleSim.Invoke(SafeWrite);
                  NewMessageReceived?.InvokeOnUI(new object[] { this, EventArgs.Empty });
               }
            }
         }

         // This logic is a bit ugly, but it kind of works.
         // We are assuming that if we see more than 2 new lines printed to screen
         // after the user types a command, the appears to be a login command
         // We can mark the connection as being logged in.
         if (!_LoggedInConnection && _UserInteraction && _LastCommandAppearedToBeALogin)
            if (consoleSim.Invoke(GetLineCount) - existingLineCount > 2)
               _LoggedInConnection = true;

         _ReadingCommands = 0;
      }

      private void Session_Closed(object sender, EventArgs e)
      {
         _LoggedInConnection = false;
         Debug.WriteLine("** Session was closed **");
         consoleSim.WriteLine("** Session closed by server **");
      }

      public void Close()
      {
         _LoggedInConnection = false;
         try
         {
            _Session.Close();
         }
         catch (SocketException ex)
         {
            Debug.WriteLine(ex);
         }
      }

      private void MooClientTerminal_Enter(object sender, EventArgs e)
      {
         txtInput.Focus();
         txtInput.SelectAll();
      }
   }
}
