﻿#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="MooClientSession.cs">
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

using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using Org.Edgerunner.Common.Extensions;
using Org.Edgerunner.Moo.Communication.Interfaces;
using System;
using Org.Edgerunner.Common.Buffers;
using Org.Edgerunner.Moo.Communication.Buffers;

namespace Org.Edgerunner.Moo.Communication;

public class MooClientSession : IMooClientSession, IDisposable
{
   /// <summary>
   /// Initializes a new instance of the <see cref="MooClientSession" /> class.
   /// </summary>
   /// <param name="world">The world.</param>
   /// <param name="host">The host.</param>
   /// <param name="port">The port.</param>
   /// <param name="outOfBandPrefix">The out of band prefix.</param>
   public MooClientSession(string world, string host, int port, string outOfBandPrefix = "#$#")
   {
      Client = new TcpClient();
      CommandBuffer = new CommunicationBuffer(10000);
      OutOfBandCommandBuffer = new CommunicationBuffer(10000);
      World = world;
      Host = host;
      Port = port;
      OutOfBandPrefix = outOfBandPrefix;
      _OutOfBandBytes = Encoding.ASCII.GetBytes(outOfBandPrefix);
      TokenSource = new CancellationTokenSource();
   }

   public void Dispose()
   {
      try
      {
         if (IsOpen)
            Client.Close();

         _Stream?.Dispose();
      }
      finally
      {
         Client.Dispose();
      }
   }

   protected readonly TcpClient Client;
   protected readonly CancellationTokenSource TokenSource;
   protected readonly string OutOfBandPrefix;

   protected NetworkStream? _Stream;
   protected byte[] _OutOfBandBytes;

   /// <summary>
   /// Gets the session world name.
   /// </summary>
   /// <value>
   /// The world.
   /// </value>
   public string World { get; }

   /// <summary>
   /// Gets the host address.
   /// </summary>
   /// <value>
   /// The host.
   /// </value>
   public string Host { get; }

   /// <summary>
   /// Gets the port address.
   /// </summary>
   /// <value>
   /// The port.
   /// </value>
   public int Port { get; }

   /// <summary>
   /// Gets a value indicating whether this client session is open.
   /// </summary>
   /// <value>
   ///   <c>true</c> if this session is open; otherwise, <c>false</c>.
   /// </value>
   public bool IsOpen
   {
      get
      {
         Client.Client.Poll(0, SelectMode.SelectRead);
         return Client is { Connected: true };
      }
   }

   /// <summary>
   /// Gets the command buffer.
   /// </summary>
   /// <value>
   /// The command buffer.
   /// </value>
   public CommunicationBuffer CommandBuffer { get; }

   /// <summary>
   /// Gets the out of band command buffer.
   /// </summary>
   /// <value>
   /// The out of band command buffer.
   /// </value>
   public CommunicationBuffer OutOfBandCommandBuffer { get; }

   /// <summary>
   /// Sends the contents of the data buffer over the session connection.
   /// </summary>
   /// <param name="buffer">The data buffer.</param>
   public void Send(byte[] buffer)
   {
      _Stream?.Write(buffer, 0, buffer.Length);
   }

   /// <summary>
   /// Sends the text over the session connection.
   /// </summary>
   /// <param name="text">The text to send.</param>
   public void Send(string text)
   {
      var data = Encoding.ASCII.GetBytes(text);
      _Stream?.Write(data, 0, data.Length);
   }

   /// <summary>
   /// Sends the line of text over the session connection.
   /// </summary>
   /// <param name="text">The line of text to send.</param>
   /// <remarks>This method appends a line feed for you.</remarks>
   public void SendLine(string text)
   {
      var data = Encoding.ASCII.GetBytes($"{text}\n");
      _Stream?.Write(data, 0, data.Length);
   }

   /// <summary>
   /// Sends the contents of the data buffer over the session connection as an out of band command.
   /// </summary>
   /// <param name="buffer">The data buffer.</param>
   public void SendOutOfBand(byte[] buffer)
   {
      var data = new byte[_OutOfBandBytes.Length + buffer.Length];
      Array.Copy(_OutOfBandBytes, data, _OutOfBandBytes.Length);
      buffer.CopyTo(data, _OutOfBandBytes.Length);
      _Stream?.Write(data, 0, data.Length);
   }

   /// <summary>
   /// Sends the text over the session connection as an out of band command.
   /// </summary>
   /// <param name="text">The text to send.</param>
   public void SendOutOfBand(string text)
   {
      var data = Encoding.ASCII.GetBytes($"{OutOfBandPrefix}{text}");
      _Stream?.Write(data, 0, data.Length);
   }

   /// <summary>
   /// Sends the line of text over the session connection as an out of band command.
   /// </summary>
   /// <param name="text">The line of text to send.</param>
   /// <remarks>This method appends a line feed for you.</remarks>
   public void SendOutOfBandLine(string text)
   {
      var data = Encoding.ASCII.GetBytes($"{OutOfBandPrefix}{text}\n");
      _Stream?.Write(data, 0, data.Length);
   }

   /// <summary>
   /// Occurs when data is received on the connection.
   /// </summary>
   public event EventHandler? DataReceived;

   /// <summary>
   /// Occurs when data is dropped because the buffer overflowed.
   /// </summary>
   public event EventHandler<int>? DataDropped;

   /// <summary>
   /// Occurs when session connection is closed.
   /// </summary>
   public event EventHandler? Closed;

   protected virtual NetworkStream GetStream(TcpClient client)
   {
      return client.GetStream();
   }

   public void Open(string host, int port)
   {
      Client.Connect(Host, Port);
      _Stream = GetStream(Client);
      Task.Run(ReadFromConnection);
   }

   /// <summary>
   /// Closes the session connection.
   /// </summary>
   /// <returns>
   /// Returns <c>true</c> if closed normally; <c>false</c> if was already closed.
   /// </returns>
   public bool Close()
   {
      try
      {
         TokenSource.Cancel();
         Client.Close();
         return true;
      }
      catch (Exception)
      {
         return false;
      }
   }

   protected void OnClosed()
   {
      Closed?.InvokeOnUI(new object[] { this });
   }

   protected void OnDataReceived()
   {
      DataReceived?.InvokeOnUI(new object[] { this });
   }

   protected void OnDataDropped(int droppedBytes)
   {
      DataDropped?.InvokeOnUI(new object[] { this, droppedBytes });
   }

   protected void ReadFromConnection()
   {
      var buffer = new byte[2048];
      var outOfBand = false;
      while (Client is { Connected: true })
      {
         Thread.Sleep(5);
         try
         {
            while (_Stream is { DataAvailable: true })
            {
               var bytes = _Stream.Read(buffer, 0, buffer.Length);
               ProcessReadBuffer(buffer, bytes, ref outOfBand);
               OnDataReceived();
            }
         }
         catch (ObjectDisposedException)
         {
            _Stream = null;
            OnClosed();
            var text = Encoding.UTF8.GetBytes("\n** Connection Closed by client **");
            foreach (var character in text)
               CommandBuffer.PushBack(character);
            Debug.WriteLine("Stream disposed");
         }
         if (TokenSource.IsCancellationRequested)
            return;
      }
   }

   private void ProcessReadBuffer(byte[] buffer, int bytes, ref bool outOfBandMode)
   {
      var droppedBytes = 0;
      for (int i = 0; i < bytes; i++)
      {
         // We convert any carriage return line feeds into line feeds
         if (buffer[i] == '\r' && bytes - i > 1 && buffer[i + 1] == '\n')
         {
            // Do nothing, we are skipping the carriage return
         }
         else if (!outOfBandMode && buffer[i] == '#')
         {
            if ((bytes - i > 2) && (buffer[i + 1] == '$') && (buffer[i + 2] == '#'))
               outOfBandMode = true;

            droppedBytes += BufferData(outOfBandMode, buffer[i]);
         }
         else if (outOfBandMode)
         {
            if (buffer[i] == '\n')
            {
               outOfBandMode = false;
               droppedBytes += BufferData(true, buffer[i]);
            }
            else
               droppedBytes += BufferData(outOfBandMode, buffer[i]);
         }
         else
            droppedBytes += BufferData(outOfBandMode, buffer[i]);
      }
      if (droppedBytes != 0)
         OnDataDropped(droppedBytes);
   }

   private int BufferData(bool outOfBandMode, byte b)
   {
      if (outOfBandMode)
      {
         var full = OutOfBandCommandBuffer.IsFull;
         OutOfBandCommandBuffer.PushBack(b);
         return full ? 1 : 0;
      }
      else
      {
         var full = CommandBuffer.IsFull;
         CommandBuffer.PushBack(b);
         return full ? 1 : 0;
      }
   }
}