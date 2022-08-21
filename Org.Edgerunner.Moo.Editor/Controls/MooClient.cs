﻿using System;
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

namespace Org.Edgerunner.Moo.Editor.Controls
{
   public partial class MooClient : UserControl
   {
      private TcpClient _Client;
      private NetworkStream _Stream;

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
            var lines = txtInput.Text.Split('\n');
            foreach (var line in lines)
               if (_Stream != null)
               {
                  Byte[] data = System.Text.Encoding.ASCII.GetBytes(line + '\n');
                  _Stream.Write(data, 0, data.Length);
               }

            txtInput.Clear();
            e.SuppressKeyPress = true;
         }
      }

      public void Connect(string host, int port)
      {
                 this.BeginInvoke(
                               new MethodInvoker(() =>
                               {
                                   _Client = new TcpClient();
                                   _Client.Connect(host, port);
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
                                           if (lines[^1] == string.Empty)
                                               lines.RemoveAt(lines.Count - 1);
                                           if (terminated)
                                               consoleSim.Write("\n");
                                           if (lines.Count > 1)
                                               for (int i = 0; i < lines.Count - 1; i++)
                                               {
                                                   consoleSim.WriteAnsiLine(lines[i]);
                                                   consoleSim.GoEnd();
                                               }
                                           consoleSim.WriteAnsi(lines[^1]);
                                           consoleSim.GoEnd();
                                           terminated = messageData[^1] == '\n';
                                       }
                                   }
                               }));
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
