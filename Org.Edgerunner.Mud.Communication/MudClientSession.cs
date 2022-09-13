#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="MudClientSession.cs">
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

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using Org.Edgerunner.Common.Extensions;
using Org.Edgerunner.Mud.Communication.Buffers;
using Org.Edgerunner.Mud.Communication.Interfaces;
using IOException = System.IO.IOException;

namespace Org.Edgerunner.Mud.Communication;

public class MudClientSession : IMudClientSession, IDisposable
{
   /// <summary>
   /// Initializes a new instance of the <see cref="MudClientSession" /> class.
   /// </summary>
   /// <param name="world">The world.</param>
   /// <param name="host">The host.</param>
   /// <param name="port">The port.</param>
   /// <param name="outOfBandPrefix">The out of band prefix.</param>
   public MudClientSession(string world, string host, int port, string outOfBandPrefix = "#$#")
   {
      Client = new TcpClient();
      CommandBuffer = new CommunicationBuffer(2048);
      CommandChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = true });
      World = world;
      Host = host;
      Port = port;
      TokenSource = new CancellationTokenSource();
   }

   /// <summary>
   /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
   /// </summary>
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

   protected Stream? _Stream;

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
         try
         {
            if (Client.Client != null)
               return !(Client.Client.Poll(5, SelectMode.SelectRead) && Client.Client.Available == 0);

            return false;
         }
         catch (SocketException)
         {
            return false;
         }
         catch (NullReferenceException)
         {
            return false;
         }
      }
   }

   /// <summary>
   /// Gets the connection <see cref="System.IO.Stream" />.
   /// </summary>
   /// <value>
   /// The stream.
   /// </value>
   /// <seealso cref="System.IO.Stream" />
   public Stream? Stream => _Stream;

   /// <summary>
   /// Gets or sets a value indicating whether this instance is authenticated.
   /// </summary>
   /// <value>
   ///   <c>true</c> if this instance is authenticated; otherwise, <c>false</c>.
   /// </value>
   public bool IsAuthenticated { get; protected set; }

   /// <summary>
   /// Gets the command queue.
   /// </summary>
   /// <value>
   /// The command queue.
   /// </value>
   public Channel<string> CommandChannel { get; }

   /// <summary>
   /// Gets the command buffer.
   /// </summary>
   /// <value>
   /// The command buffer.
   /// </value>
   protected CommunicationBuffer CommandBuffer { get; }

   /// <summary>
   /// Sends the contents of the data buffer over the session connection.
   /// </summary>
   /// <param name="buffer">The data buffer.</param>
   public void Send(byte[] buffer)
   {
      SendData(buffer);
   }

   /// <summary>
   /// Sends the text over the session connection.
   /// </summary>
   /// <param name="text">The text to send.</param>
   public void Send(string text)
   {
      var data = Encoding.UTF8.GetBytes(text);
      SendData(data);
   }

   /// <summary>
   /// Sends the line of text over the session connection.
   /// </summary>
   /// <param name="text">The line of text to send.</param>
   /// <remarks>This method appends a line feed for you.</remarks>
   public void SendLine(string text)
   {
      var data = Encoding.UTF8.GetBytes($"{text}\n");
      SendData(data);
   }

   private void SendData(byte[] data)
   {
      try
      {
         _Stream?.Write(data, 0, data.Length);
      }
      catch (IOException)
      {
         Debug.WriteLine("failed to write to socket");
      }
   }

   /// <summary>
   /// Occurs when a message is received on the connection.
   /// </summary>
   public event EventHandler<string>? MessageReceived;

   /// <summary>
   /// Occurs when data is dropped because the buffer overflowed.
   /// </summary>
   public event EventHandler<int>? DataDropped;

   /// <summary>
   /// Occurs when session connection is closed.
   /// </summary>
   public event EventHandler? Closed;

   protected virtual Stream GetStream(TcpClient client)
   {
      return client.GetStream();
   }

   public async Task OpenAsync(string host, int port)
   {
      try
      {
         await Task.Run(() =>
                         {
                            Client.Connect(Host, Port);
                            _Stream = GetStream(Client);
                         }, TokenSource.Token);
      }
      catch (ObjectDisposedException)
      {
         Debug.WriteLine("Socket related objects were peremptorily disposed of.");
      }
      catch (SocketException ex)
      {
         if (ex.SocketErrorCode != SocketError.NotSocket)
            throw;
      }
      catch (TaskCanceledException)
      {
         Debug.WriteLine("Socket open attempt cancelled.");
      }
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

   /// <summary>
   /// Called when session [closed].
   /// </summary>
   protected void OnClosed()
   {
      Closed?.InvokeOnUI(new object[] { this });
   }

   /// <summary>
   /// Begins the reading data till session close.
   /// </summary>
   public void BeginReadingDataTillClose()
   {
      Task.Run(ReadFromConnection);
   }

   protected void OnMessageReceived(string message)
   {
      // We are not firing this event on the UI so it can do background work
      MessageReceived?.Invoke(this, message);
   }

   protected void OnDataDropped(int droppedBytes)
   {
      // ReSharper disable once EventExceptionNotDocumented
      DataDropped?.InvokeOnUI(new object[] { this, droppedBytes });
   }

   protected async void ReadFromConnection()
   {
      var buffer = new byte[10240];
      while (IsOpen)
      {
         Thread.Sleep(5);
         try
         {
            while (_Stream != null && Client.Available > 0)
            {
               // ReSharper disable ExceptionNotDocumented
               var bytes = await _Stream.ReadAsync(buffer, 0, buffer.Length, TokenSource.Token);
               // ReSharper restore ExceptionNotDocumented
               await ProcessReadBuffer(buffer, bytes);
            }

            await FlushCommandBuffer();
         }
         catch (ObjectDisposedException)
         {
            _Stream = null;
            OnClosed();
            Debug.WriteLine("Stream disposed");
         }
         if (TokenSource.IsCancellationRequested)
            break;
      }
      OnClosed();
   }

   private async Task ProcessReadBuffer(byte[] buffer, int bytes)
   {
      var droppedBytes = 0;
      for (int i = 0; i < bytes; i++)
      {
         // We convert any carriage return line feeds into line feeds
         if (buffer[i] == '\r' && bytes - i > 1 && buffer[i + 1] == '\n')
         {
            // Do nothing, we are skipping the carriage return
         }
         else if (buffer[i] == '\n')
         {
            var data = "\n";
            if (!CommandBuffer.IsEmpty)
            {
               BufferData(buffer[i]);
               var dataBuffer = CommandBuffer.ToArray();
               CommandBuffer.Clear();
               var decoder = Encoding.UTF8.GetDecoder();

               var chars = new char[decoder.GetCharCount(dataBuffer, 0, dataBuffer.Length)];
               decoder.GetChars(dataBuffer, 0, dataBuffer.Length, chars, 0);
               data = new string(chars);
            }

            await CommandChannel.Writer.WriteAsync(data, TokenSource.Token);
            OnMessageReceived(data);
         }
         else
            droppedBytes += BufferData(buffer[i]);
      }
      if (droppedBytes != 0)
         OnDataDropped(droppedBytes);
   }

   private async Task FlushCommandBuffer()
   {
      if (!CommandBuffer.IsEmpty)
      {
         var dataBuffer = CommandBuffer.ToArray();
         CommandBuffer.Clear();
         var decoder = Encoding.UTF8.GetDecoder();

         var chars = new char[decoder.GetCharCount(dataBuffer, 0, dataBuffer.Length)];
         decoder.GetChars(dataBuffer, 0, dataBuffer.Length, chars, 0);
         var data = new string(chars);
         await CommandChannel.Writer.WriteAsync(data, TokenSource.Token);
         OnMessageReceived(data);
      }
   }

   private int BufferData(byte b)
   {
      var full = CommandBuffer.IsFull;
      CommandBuffer.PushBack(b);
      return full ? 1 : 0;
   }
}