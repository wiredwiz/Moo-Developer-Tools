using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FastColoredTextBoxNS.Types;
using Org.Edgerunner.Common.Extensions;
using Org.Edgerunner.Mud.Communication;
using Org.Edgerunner.Mud.Communication.Buffers;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP;
using Org.Edgerunner.Mud.MCP.Exceptions;
using Org.Edgerunner.Mud.MCP.Interfaces;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Forms.Application;

namespace Org.Edgerunner.Moo.Editor.Controls
{
    public partial class MooClientTerminal : UserControl, IClientTerminal
   {
      private IMudClientSession _Session;

      private int _ReadingCommands;

      private bool _LoggedInConnection;

      private readonly CommandBuffer _InputCommandBuffer;

      private bool _UserInteraction;

      private int _LastAttemptedLoginScreenLines;

      private string _OutOfBandPrefix = "#$#";

      private bool _LastCommandIsLogin;

      protected McpClientSessionManager McpSessionManager { get; set; }

      public string Host => _Session.Host;

      public int Port => _Session.Port;

      public string World => _Session.World;

      protected bool LastCommandIsLogin
      {
         get => _LastCommandIsLogin;
         set
         {
            if (value)
               _LastAttemptedLoginScreenLines = consoleSim.Lines.Count;
            _LastCommandIsLogin = value;
         }
      }

      protected readonly CancellationTokenSource TokenSource;

      public ConsoleWindowEmulator Output => consoleSim;

      public TextBox Input => txtInput;

      /// <summary>
      /// Gets or sets a value indicating whether word wrap is enabled for the console.
      /// </summary>
      /// <value>
      ///   <c>true</c> if word wrap enabled; otherwise, <c>false</c>.
      /// </value>
      public bool WordWrap
      {
         get => consoleSim.WordWrap;
         set => consoleSim.WordWrap = value;
      }

      /// <summary>
      /// Gets or sets the message processor.
      /// </summary>
      /// <value>
      /// The message processor.
      /// </value>
      public IMessageProcessor MessageProcessor { get; set; }

      /// <summary>
      /// Gets or sets a value indicating whether this <see cref="MooClientTerminal"/> is using TLS.
      /// </summary>
      /// <value>
      ///   <c>true</c> if TLS; otherwise, <c>false</c>.
      /// </value>
      public bool Tls { get; protected set; }

      public string OutOfBandPrefix
      {
         get => _OutOfBandPrefix;
         set
         {
            _OutOfBandPrefix = value;
            MessageProcessor.OutOfBandPrefix = value;
         }
      }

      /// <summary>
      /// Gets or sets a value indicating whether [echo enabled].
      /// </summary>
      /// <value>
      ///   <c>true</c> if [echo enabled]; otherwise, <c>false</c>.
      /// </value>
      public bool EchoEnabled
      {
         get => consoleSim.AnsiManager.EchoEnabled;
         set => consoleSim.AnsiManager.EchoEnabled = value;
      }

      /// <summary>
      /// Gets or sets a value indicating whether to enable ANSI color.
      /// </summary>
      /// <value><c>true</c> if [ANSI color enabled]; otherwise, <c>false</c>.</value>
      public bool AnsiColorEnabled
      {
         get => consoleSim.AnsiColorEnabled;
         set => consoleSim.AnsiColorEnabled = value;
      }

      /// <summary>
      /// Gets or sets a value indicating whether to enable blinking text.
      /// </summary>
      /// <value><c>true</c> if [blinking text enabled]; otherwise, <c>false</c>.</value>
      public bool BlinkingTextEnabled
      {
         get => consoleSim.BlinkingTextEnabled;
         set => consoleSim.BlinkingTextEnabled = value;
      }

      /// <summary>
      /// Gets or sets a value indicating whether to enable ascii bell.
      /// </summary>
      /// <value><c>true</c> if [bell enabled]; otherwise, <c>false</c>.</value>
      public bool AsciiBellEnabled
      {
         get => consoleSim.AsciiBellEnabled;
         set => consoleSim.AsciiBellEnabled = value;
      }

      /// <summary>
      /// Gets a value indicating whether this instance is connected.
      /// </summary>
      /// <value><c>true</c> if this instance is connected; otherwise, <c>false</c>.</value>
      public bool IsConnected => _Session?.IsOpen ?? false;

      /// <summary>
      /// Initializes a new instance of the <see cref="MooClientTerminal"/> class.
      /// </summary>
      /// <param name="useTls">if set to <c>true</c> [use TLS].</param>
      public MooClientTerminal(bool useTls = false)
      {
         InitializeComponent();
         ConsoleForegroundColor = Color.WhiteSmoke;
         ConsoleBackgroundColor = Color.Black;
         consoleSim.Text = string.Empty;
         _ReadingCommands = 0;
         _InputCommandBuffer = new CommandBuffer(15);
         _UserInteraction = false;
         LastCommandIsLogin = false;
         McpSessionManager = new McpClientSessionManager(new Version(2,1), new Version(2,1), new List<IMcpPackage>());
         ActiveControl = txtInput;
         splitContainer1.SplitterDistance = splitContainer1.ClientSize.Height - txtInput.Height;
         splitContainer1.ActiveControl = txtInput;
         TokenSource = new CancellationTokenSource();
         Tls = useTls;
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

      public Color ConsoleForegroundColor
      {
         get => consoleSim.ConsoleForeColor;
         set => consoleSim.ConsoleForeColor = value;
      }

      public Color ConsoleBackgroundColor
      {
         get => consoleSim.ConsoleBackgroundColor;
         set => consoleSim.ConsoleBackgroundColor = value;
      }

      private void txtInput_KeyDown(object sender, KeyEventArgs e)
      {
         if (e.KeyCode == Keys.Enter && !e.Control)
         {
            if (_LoggedInConnection || !IsConnecting(txtInput.Text))
            {
               LastCommandIsLogin = false;
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
               _UserInteraction = true;
               LastCommandIsLogin = true;

               var existing = consoleSim.AnsiManager.EchoEnabled;
               consoleSim.AnsiManager.EchoEnabled = false;
               SendTextLines(txtInput.Text.Split('\n'));
               consoleSim.AnsiManager.EchoEnabled = existing;

               txtInput.Text = string.Empty;
               txtInput.UseSystemPasswordChar = true;
            }

            e.SuppressKeyPress = true;
         }
         else if (e.KeyCode == Keys.Up && !e.Alt && !e.Shift)
         {
            if (e.Control || txtInput.SelectionLength == txtInput.Text.Length)
            {
               if (!_InputCommandBuffer.IsEmpty)
               {
                  txtInput.Text = _InputCommandBuffer.MoveBackInBuffer();
                  txtInput.SelectAll();
               }

               e.SuppressKeyPress = true;
            }
         }
         else if (e.KeyCode == Keys.Down && !e.Alt && !e.Shift)
         {
            if (e.Control || txtInput.SelectionLength == txtInput.Text.Length)
            {
               if (_InputCommandBuffer.IsEmpty && !string.IsNullOrEmpty(txtInput.Text))
               {
                  _InputCommandBuffer.PushFront(txtInput.Text);
               }
               else if (_InputCommandBuffer.CurrentPosition == 0 &&
                        !string.IsNullOrEmpty(txtInput.Text) &&
                        !_InputCommandBuffer.IsEmpty &&
                        _InputCommandBuffer[0] != txtInput.Text)
               {
                  _InputCommandBuffer.PushFront(txtInput.Text);
               }

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

            //if (txtInput.Text != null)
            //   txtInput.SelectionStart = txtInput.Text.Length;
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

      /// <summary>
      /// Sends the text lines to the client connection.
      /// </summary>
      /// <param name="text"></param>
      public void SendTextLines(IEnumerable<string> text)
      {
         foreach (var line in text)
            SendTextLine(line);
      }

      /// <summary>
      /// Sends the text line to the client connection.
      /// </summary>
      /// <param name="text">The text to send.</param>
      public void SendLoginTextLine(string text)
      {
         void SafeWrite()
         {
            var atBottom = consoleSim.VerticalScrollbarPositionedAtBottom;

            _UserInteraction = true;
            LastCommandIsLogin = true;
            _Session.SendLine(text);
            if (atBottom)
               consoleSim.GoEnd();
         }

         if (InvokeRequired)
            Invoke(SafeWrite);
         else
            SafeWrite();
      }

      /// <summary>
      /// Sends the text line to the client connection.
      /// </summary>
      /// <param name="text">The text to send.</param>
      public void SendTextLine(string text)
      {
         void SafeWrite()
         {
            var atBottom = consoleSim.VerticalScrollbarPositionedAtBottom;

            consoleSim.EchoTextLine(text);
            _Session.SendLine(text);
            if (atBottom)
               consoleSim.GoEnd();
         }

         if (InvokeRequired)
            Invoke(SafeWrite);
         else
            SafeWrite();
      }

      /// <summary>
      /// Sends the text to the client connection.
      /// </summary>
      /// <param name="text">The text to send.</param>
      public void SendText(string text)
      {
         void SafeWrite()
         {
            var atBottom = consoleSim.VerticalScrollbarPositionedAtBottom;

            consoleSim.EchoText(text);
            _Session.Send(text);
            if (atBottom)
               consoleSim.GoEnd();
         }

         if (InvokeRequired)
            Invoke(SafeWrite);
         else
            SafeWrite();
      }

      /// <summary>
      /// Sends lines of text to the client connection as out of band command.
      /// </summary>
      /// <param name="lines">The list of lines.</param>
      public void SendOutOfBandLines(IEnumerable<string> lines)
      {
         foreach (var line in lines)
            _Session.SendLine($"{OutOfBandPrefix}{line}");
      }

      /// <summary>
      /// Sends the text to the client connection as out of band.
      /// </summary>
      /// <param name="line"></param>
      public void SendOutOfBandLine(string line)
      {
         _Session.SendLine($"{OutOfBandPrefix}{line}");
      }

      /// <summary>
      /// Displays text to the terminal console emulator.
      /// </summary>
      /// <param name="text">The message to display.</param>
      public void DisplayToConsole(string text)
      {
         void SafeWrite()
         {
            var atBottom = consoleSim.VerticalScrollbarPositionedAtBottom;

            consoleSim.Write(text);

            if (atBottom)
               consoleSim.GoEnd();
         }

         if (InvokeRequired)
            Invoke(SafeWrite);
         else
            SafeWrite();
      }

      /// <summary>
      /// Displays text line to the terminal console emulator.
      /// </summary>
      /// <param name="text">The message to display.</param>
      public void DisplayLineToConsole(string text)
      {
         void SafeWrite()
         {
            var atBottom = consoleSim.VerticalScrollbarPositionedAtBottom;

            consoleSim.WriteLine(text);

            if (atBottom)
               consoleSim.GoEnd();
         }

         if (InvokeRequired)
            Invoke(SafeWrite);
         else
            SafeWrite();
      }

      /// <summary>
      /// Displays text lines to the terminal console emulator.
      /// </summary>
      /// <param name="lines">The message lines to display.</param>
      public void DisplayLinesToConsole(IEnumerable<string> lines)
      {
         void SafeWrite()
         {
            var atBottom = consoleSim.VerticalScrollbarPositionedAtBottom;

            foreach (var line in lines)
               consoleSim.WriteLine(line);

            if (atBottom)
               consoleSim.GoEnd();
         }

         if (InvokeRequired)
            Invoke(SafeWrite);
         else
            SafeWrite();
      }

      /// <summary>
      /// Displays text line to the terminal console emulator.
      /// </summary>
      /// <param name="text">The message to display.</param>
      /// <param name="foregroundColor">The foreground color of the text.</param>
      /// <remarks>
      /// The current terminal console emulator background color is used.
      /// </remarks>
      public void DisplayToConsole(string text, Color foregroundColor)
      {
         void SafeWrite()
         {
            var atBottom = consoleSim.VerticalScrollbarPositionedAtBottom;

            var style = AnsiManager.GetStyle(foregroundColor, ConsoleBackgroundColor, FontStyle.Regular);
            consoleSim.WriteWithStyle(text, style);

            if (atBottom)
               consoleSim.GoEnd();
         }

         if (InvokeRequired)
            Invoke(SafeWrite);
         else
            SafeWrite();
      }

      /// <summary>
      /// Displays text line to the terminal console emulator.
      /// </summary>
      /// <param name="text">The message to display.</param>
      /// <param name="foregroundColor">The foreground color of the text.</param>
      /// <remarks>
      /// The current terminal console emulator background color is used.
      /// </remarks>
      public void DisplayLineToConsole(string text, Color foregroundColor)
      {
         void SafeWrite()
         {
            var atBottom = consoleSim.VerticalScrollbarPositionedAtBottom;

            var style = AnsiManager.GetStyle(foregroundColor, ConsoleBackgroundColor, FontStyle.Regular);
            consoleSim.WriteWithStyle($"{text}\n", style);

            if (atBottom)
               consoleSim.GoEnd();
         }

         if (InvokeRequired)
            Invoke(SafeWrite);
         else
            SafeWrite();
      }

      /// <summary>
      /// Displays text lines to the terminal console emulator.
      /// </summary>
      /// <param name="lines">The message lines to display.</param>
      /// <param name="foregroundColor">The foreground color of the text.</param>
      public void DisplayLinesToConsole(IEnumerable<string> lines, Color foregroundColor)
      {
         void SafeWrite()
         {
            var atBottom = consoleSim.VerticalScrollbarPositionedAtBottom;

            var style = AnsiManager.GetStyle(foregroundColor, ConsoleBackgroundColor, FontStyle.Regular);
            foreach (var line in lines)
               consoleSim.WriteWithStyle($"{line}\n", style);

            if (atBottom)
               consoleSim.GoEnd();
         }

         if (InvokeRequired)
            Invoke(SafeWrite);
         else
            SafeWrite();
      }

      /// <summary>
      /// Displays text line to the terminal console emulator.
      /// </summary>
      /// <param name="text">The message to display.</param>
      /// <param name="foregroundColor">The foreground color of the text.</param>
      /// <param name="backgroundColor">The background color of the text.</param>
      public void DisplayToConsole(string text, Color foregroundColor, Color backgroundColor)
      {
         void SafeWrite()
         {
            var atBottom = consoleSim.VerticalScrollbarPositionedAtBottom;

            var style = AnsiManager.GetStyle(foregroundColor, backgroundColor, FontStyle.Regular);
            consoleSim.WriteWithStyle(text, style);

            if (atBottom)
               consoleSim.GoEnd();
         }

         if (InvokeRequired)
            Invoke(SafeWrite);
         else
            SafeWrite();
      }

      /// <summary>
      /// Displays text to the terminal console emulator.
      /// </summary>
      /// <param name="text">The message to display.</param>
      /// <param name="foregroundColor">The foreground color of the text.</param>
      /// <param name="backgroundColor">The background color of the text.</param>
      public void DisplayLineToConsole(string text, Color foregroundColor, Color backgroundColor)
      {
         void SafeWrite()
         {
            var atBottom = consoleSim.VerticalScrollbarPositionedAtBottom;

            var style = AnsiManager.GetStyle(foregroundColor, backgroundColor, FontStyle.Regular);
            consoleSim.WriteWithStyle($"{text}\n", style);

            if (atBottom)
               consoleSim.GoEnd();
         }

         if (InvokeRequired)
            Invoke(SafeWrite);
         else
            SafeWrite();
      }

      /// <summary>
      /// Displays text lines to the terminal console emulator.
      /// </summary>
      /// <param name="lines">The message lines to display.</param>
      /// <param name="foregroundColor">The foreground color of the text.</param>
      /// <param name="backgroundColor">The background color of the text.</param>
      public void DisplayLinesToConsole(IEnumerable<string> lines, Color foregroundColor, Color backgroundColor)
      {
         void SafeWrite()
         {
            var atBottom = consoleSim.VerticalScrollbarPositionedAtBottom;

            var style = AnsiManager.GetStyle(foregroundColor, backgroundColor, FontStyle.Regular);
            foreach (var line in lines)
               consoleSim.WriteWithStyle($"{line}\n", style);

            if (atBottom)
               consoleSim.GoEnd();
         }

         if (InvokeRequired)
            Invoke(SafeWrite);
         else
            SafeWrite();
      }

      public async Task ConnectAsync(string world, string host, int port, bool useTls = false)
      {
         Tls = useTls;
         _Session = Tls ? MudClient.Create<TlsMudClientSession>(world, host, port) : MudClient.Create<MudClientSession>(world, host, port);
         _Session.Closed += Session_Closed;
         _Session.MessageReceived += SessionMessageReceived;
         _Session.DataDropped += Session_DataDropped;
         await _Session.OpenAsync(host, port);
         if (Tls)
            if (_Session.Stream is SslStream stream)
            {
               var okStyle = AnsiManager.GetStyle(Color.LawnGreen, ConsoleBackgroundColor, FontStyle.Regular);
               var badStyle = AnsiManager.GetStyle(Color.Red, ConsoleBackgroundColor, FontStyle.Regular);
               if (!stream.IsEncrypted)
                  consoleSim.WriteLine("[ Data Encryption: DISABLED ]", badStyle);
               else
                  consoleSim.WriteLine($"[ Data Encryption: ENABLED ]", okStyle);
               consoleSim.WriteLine($"[ Hash algorithm: {stream.HashAlgorithm} ]", okStyle);
               consoleSim.WriteLine($"[ Cipher algorithm: {stream.CipherAlgorithm} ]", okStyle);
               if (!stream.IsAuthenticated)
                  consoleSim.WriteLine("[ Authenticated: NO ]", badStyle);
               else
                  consoleSim.WriteLine($"[ Authenticated: {host} ]", okStyle);
            }

         _Session.BeginReadingDataTillClose();
         ReadFromChannel();
         _LoggedInConnection = false;
      }

      /// <summary>
      /// Puts focus the on the input box.
      /// </summary>
      public void FocusOnInput()
      {
         txtInput.Focus();
      }

      private void Session_DataDropped(object sender, int e)
      {
         Debug.WriteLine($"{e} bytes dropped from buffer overflow");
      }

      private void SessionMessageReceived(object sender, string e)
      {
      }

      private void ReadFromChannel()
      {
         Task.Run(PerformReadFromChannel);
      }

      private async void PerformReadFromChannel()
      {
         try
         {
            while (await _Session.CommandChannel.Reader.WaitToReadAsync(TokenSource.Token))
            {
               while (_Session.CommandChannel.Reader.TryRead(out var text))
               {
                  if (MessageProcessor == null || !MessageProcessor.ProcessMessage(this, text))
                  {
                     void SafeWrite()
                     {
                        var atBottom = consoleSim.VerticalScrollbarPositionedAtBottom;
                        consoleSim.WriteAnsi(text);
                        if (atBottom)
                           consoleSim.GoEnd();
                     }

                     try
                     {
                        Invoke(SafeWrite);
                     }
                     catch (InvalidOperationException)
                     {
                        Debug.WriteLine("Our terminal was disposed of");
                        return;
                     }
                  }

                  NewMessageReceived?.InvokeOnUI(new object[] { this, EventArgs.Empty });
               }

               // We define a local function to fetch the console line count to be used via thread invocation
               int GetLineCount() => consoleSim.Lines.Count;

               // This logic is a bit ugly, but it kind of works.
               // We are assuming that if we see more than 2 new lines printed to screen
               // after the user types a command, the appears to be a login command
               // We can mark the connection as being logged in.
               if (!_LoggedInConnection && _UserInteraction && LastCommandIsLogin)
                  if (consoleSim.Invoke(GetLineCount) - _LastAttemptedLoginScreenLines > 2)
                     _LoggedInConnection = true;

               if (TokenSource.IsCancellationRequested)
                  return;
            }
         }
         catch (OperationCanceledException)
         {
            // We are probably shutting down
            return;
         }
      }

      private void Session_Closed(object sender, EventArgs e)
      {
         _LoggedInConnection = false;
         Debug.WriteLine("** Session was closed **");
         consoleSim.WriteLine("** Connection closed **");
      }

      public void Close()
      {
         _LoggedInConnection = false;
         try
         {
            TokenSource.Cancel();
            _Session?.Close();
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
