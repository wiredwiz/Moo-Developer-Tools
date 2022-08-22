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
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Forms.Application;

namespace Org.Edgerunner.Moo.Editor.Controls
{
   public partial class MooClient : UserControl
   {
      private TcpClient _Client;
      private NetworkStream _Stream;

      public event EventHandler<string> OutOfBandCommandReceived;

      public string Host { get; set; }

      public int Port { get; set; }

      public string World { get; set; }

      public MooClient()
      {
         InitializeComponent();
         ConsoleForeColor = Color.WhiteSmoke;
         ConsoleBackgroundColor = Color.Black;
         consoleSim.Text = string.Empty;
      }

      protected override void OnHandleDestroyed(EventArgs e)
      {
         Close();
         base.OnHandleDestroyed(e);
      }

      protected virtual void OnOutOfBandCommandReceived(string command)
      {
         OutOfBandCommandReceived?.Invoke(this, command);
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
         if (_Stream != null)
         {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(text + '\n');
            _Stream.Write(data, 0, data.Length);
         }
      }

      public void SendText(string text)
      {
         if (_Stream != null)
         {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(text);
            _Stream.Write(data, 0, data.Length);
         }
      }

      public void SendOutOfBandCommands(IEnumerable<string> commands)
      {
         foreach (var command in commands)
            SendOutOfBandCommand(command);
      }

      public void SendOutOfBandCommand(string command)
      {
         SendTextLine($"#$#{command}");
      }

      public void Connect()
      {
         this.BeginInvoke(
                       new MethodInvoker(() =>
                       {
                          _Client = new TcpClient();
                          _Client.Connect(Host, Port);
                          _Stream = _Client.GetStream();
                          byte[] buffer = new byte[2048];
                          StringBuilder messageData = new StringBuilder();
                          bool terminated = false;
                          while (_Client is { Connected: true })
                          {
                             Application.DoEvents();
                             Thread.Sleep(5);
                             messageData.Clear();
                             try
                             {
                                while (_Stream is { DataAvailable: true })
                                {
                                   var bytes = _Stream.Read(buffer, 0, buffer.Length);

                                   Decoder decoder = Encoding.UTF8.GetDecoder();
                                   char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                                   decoder.GetChars(buffer, 0, bytes, chars, 0);
                                   messageData.Append(chars);
                                }
                             }
                             catch (ObjectDisposedException)
                             {
                                _Stream = null;
                                if (terminated)
                                   consoleSim.Write("\n");
                                consoleSim.WriteLine("** Connection Closed by client **");
                                consoleSim.GoEnd();
                                Debug.WriteLine("Stream disposed");
                             }

                             if (messageData.Length != 0)
                             {
                                messageData.Replace("\r\n", "\n");
                                var lines = messageData.ToString().Split('\n', StringSplitOptions.TrimEntries).ToList();
                                string line;
                                if (lines[^1] == string.Empty)
                                   lines.RemoveAt(lines.Count - 1);
                                if (terminated)
                                   consoleSim.Write("\n");
                                if (lines.Count > 1)
                                   for (int i = 0; i < lines.Count - 1; i++)
                                   {
                                      line = lines[i];
                                      if (line.StartsWith("#$#"))
                                         OnOutOfBandCommandReceived(line.Length > 3 ? line[3..] : string.Empty);
                                      else
                                      {
                                         consoleSim.WriteAnsiLine(lines[i]);
                                         consoleSim.GoEnd();
                                      }
                                   }

                                line = lines[^1];
                                if (line.StartsWith("#$#"))
                                   OnOutOfBandCommandReceived(line.Length > 3 ? line[3..] : string.Empty);
                                else
                                {
                                   consoleSim.WriteAnsi(lines[^1]);
                                   consoleSim.GoEnd();
                                }
                                terminated = messageData[^1] == '\n';
                             }
                          }
                       }));
      }

      public void Connect(string host, int port, string world = "")
      {
         Host = host;
         Port = port;
         World = world;
         Connect();
      }

      public void Close()
      {
         try
         {
            _Client?.Close();
         }
         catch (SocketException ex)
         {
            Debug.WriteLine(ex);
         }
      }
   }
}
