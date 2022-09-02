#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="OutOfBandMessageProcessor.cs">
// Copyright (c) Thaddeus Ryker 2022
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

using System.Diagnostics;
using Org.Edgerunner.Moo.Communication.Exceptions;
using Org.Edgerunner.Moo.Communication.Interfaces;

namespace Org.Edgerunner.Moo.Communication.OutOfBand;

/// <summary>
/// Class that processes out of band messages.
/// Implements the <see cref="Org.Edgerunner.Moo.Communication.OutOfBand.IOutOfBandMessageProcessor" />
/// </summary>
/// <seealso cref="Org.Edgerunner.Moo.Communication.OutOfBand.IOutOfBandMessageProcessor" />
// ReSharper disable once HollowTypeName
public class OutOfBandMessageProcessor : IOutOfBandMessageProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OutOfBandMessageProcessor"/> class.
    /// </summary>
    public OutOfBandMessageProcessor()
    {
        Handlers = new HashSet<IOutOfBandMessageHandler>();
    }

   /// <summary>
   /// Processes the message.
   /// </summary>
   /// <param name="client">The client terminal emulator.</param>
   /// <param name="message">The message.</param>
   /// <param name="state">The current message processing state.</param>
   /// <returns>
   ///   <c>true</c> if was processed, <c>false</c> otherwise.
   /// </returns>
   /// <exception cref="MessagingException">An error occurred during the processing of the message.</exception>
   public bool ProcessMessage(IClientTerminal client, string message, ref MessageProcessingState state)
    {
       Debug.WriteLine($"OOB: {message}");
      foreach (var handler in Handlers)
      {
         var result = handler.ProcessMessage(client, message, ref state);
         if (result)
         {
            state.CurrentProcessor = handler;
            return true;
         }
      }

      state.Reset();
        return true;
    }

    public void Reset()
    {
        foreach (var handler in Handlers)
            handler.Reset();
    }

    /// <summary>
    /// Gets the out of band handlers.
    /// </summary>
    /// <value>The handlers.</value>
    protected HashSet<IOutOfBandMessageHandler> Handlers { get; }

    /// <summary>
    /// Registers the handler.
    /// </summary>
    /// <param name="handler">The handler.</param>
    public void RegisterHandler(IOutOfBandMessageHandler handler)
    {
        Handlers.Add(handler);
    }

    /// <summary>
    /// Unregisters the handler.
    /// </summary>
    /// <param name="handler">The handler.</param>
    public void UnregisterHandler(IOutOfBandMessageHandler handler)
    {
        Handlers.Remove(handler);
    }
}