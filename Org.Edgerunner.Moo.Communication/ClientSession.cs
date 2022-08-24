#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="ClientSession.cs">
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

using System.Net.Sockets;
using Org.Edgerunner.Moo.Communication.Interfaces;

namespace Org.Edgerunner.Moo.Communication;

public class ClientSession : IClientSession
{
   /// <summary>
   /// Initializes a new instance of the <see cref="ClientSession"/> class.
   /// </summary>
   /// <param name="client">The client.</param>
   /// <param name="world">The world.</param>
   /// <param name="host">The host.</param>
   /// <param name="port">The port.</param>
   internal ClientSession(TcpClient client, string world, string host, int port)
   {
      _Client = client;
      _Stream = _Client.GetStream();
      World = world;
      Host = host;
      Port = port;
   }

   private readonly TcpClient _Client;

   private readonly NetworkStream? _Stream;

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
   public bool IsOpen => _Client.Connected;

   /// <summary>
   /// Sends the contents of the data buffer over the session connection.
   /// </summary>
   /// <param name="buffer">The data buffer.</param>
   public void Send(byte[] buffer)
   {
      _Stream?.Write(buffer, 0, buffer.Length);
   }

   /// <summary>
   /// Occurs when data is received on the connection.
   /// </summary>
   public event EventHandler<byte[]>? DataReceived;

   /// <summary>
   /// Occurs when session connection is closed.
   /// </summary>
   public event EventHandler? Closed;

   /// <summary>
   /// Closes the session connection.
   /// </summary>
   public void Close()
   {
      _Client.Close();
   }

   protected void OnClosed()
   {
      Closed?.Invoke(this, EventArgs.Empty);
   }

   protected void OnDataReceived(byte[] data)
   {
      DataReceived?.Invoke(this, data);
   }
}