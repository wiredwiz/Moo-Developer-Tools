#region BSD 3-Clause License

// <copyright company="Edgerunner.org" file="IMooClientSession.cs">
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

namespace Org.Edgerunner.Moo.Communication.Interfaces;

public interface IMooClientSession
{
   /// <summary>
   ///    Gets the command queue.
   /// </summary>
   /// <value>
   ///    The command queue.
   /// </value>
   ConcurrentQueue<SessionMessage> CommandQueue { get; }

   /// <summary>
   ///    Gets the host address.
   /// </summary>
   /// <value>
   ///    The host.
   /// </value>
   string Host { get; }

   /// <summary>
   ///    Gets a value indicating whether this client session is open.
   /// </summary>
   /// <value>
   ///    <c>true</c> if this session is open; otherwise, <c>false</c>.
   /// </value>
   bool IsOpen { get; }

   /// <summary>
   ///    Gets the port address.
   /// </summary>
   /// <value>
   ///    The port.
   /// </value>
   int Port { get; }

   /// <summary>
   ///    Gets the world.
   /// </summary>
   /// <value>
   ///    The world.
   /// </value>
   string World { get; }

   /// <summary>
   ///    Closes the session connection.
   /// </summary>
   /// <returns>Returns <c>true</c> if closed normally; <c>false</c> if was already closed.</returns>
   bool Close();

   /// <summary>
   ///    Occurs when session connection is closed.
   /// </summary>
   event EventHandler? Closed;

   /// <summary>
   ///    Occurs when data is dropped because the buffer overflowed.
   /// </summary>
   public event EventHandler<int>? DataDropped;

   /// <summary>
   ///    Occurs when a message is received on the connection.
   /// </summary>
   event EventHandler<ClientMessageEventArgs>? MessageReceived;

   /// <summary>
   ///    Opens a connection to the specified host address.
   /// </summary>
   /// <param name="host">The host address.</param>
   /// <param name="port">The port address.</param>
   void Open(string host, int port);

   /// <summary>
   ///    Sends the contents of the data buffer over the session connection.
   /// </summary>
   /// <param name="buffer">The data buffer.</param>
   void Send(byte[] buffer);

   /// <summary>
   ///    Sends the text over the session connection.
   /// </summary>
   /// <param name="text">The text to send.</param>
   void Send(string text);

   /// <summary>
   ///    Sends the line of text over the session connection.
   /// </summary>
   /// <param name="text">The line of text to send.</param>
   /// <remarks>This method appends a line feed for you.</remarks>
   void SendLine(string text);

   /// <summary>
   ///    Sends the contents of the data buffer over the session connection as an out of band command.
   /// </summary>
   /// <param name="buffer">The data buffer.</param>
   void SendOutOfBand(byte[] buffer);

   /// <summary>
   ///    Sends the text over the session connection as an out of band command.
   /// </summary>
   /// <param name="text">The text to send.</param>
   void SendOutOfBand(string text);

   /// <summary>
   ///    Sends the line of text over the session connection as an out of band command.
   /// </summary>
   /// <param name="text">The line of text to send.</param>
   /// <remarks>This method appends a line feed for you.</remarks>
   void SendOutOfBandLine(string text);
}