﻿#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="RootMessageProcessor.cs">
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

using Org.Edgerunner.Moo.Communication.Exceptions;
using Org.Edgerunner.Moo.Communication.OutOfBand;

namespace Org.Edgerunner.Moo.Communication;

/// <summary>
/// Class that represents a message processor.
/// </summary>
// ReSharper disable once HollowTypeName
public class RootMessageProcessor
{
    /// <summary>
    /// The current state.
    /// </summary>
    private MessageProcessingState _State;

    /// <summary>
    /// Initializes a new instance of the <see cref="RootMessageProcessor" /> class.
    /// </summary>
    /// <param name="outOfBandPrefix">The out of band prefix.</param>
    /// <param name="outOfBandMessageProcessor">The out of band message processor.</param>
    protected RootMessageProcessor(string outOfBandPrefix, IMessageProcessor outOfBandMessageProcessor)
    {
        OutOfBandPrefix = outOfBandPrefix;
        OutOfBandMessageProcessor = outOfBandMessageProcessor;
        _State = new MessageProcessingState();
    }

    /// <summary>
    /// Gets or sets the out of band prefix.
    /// </summary>
    /// <value>The out of band prefix.</value>
    public string OutOfBandPrefix { get; protected set; }

    /// <summary>
    /// Gets or sets the out of band messaging timeout.
    /// </summary>
    /// <value>The out of band messaging timeout interval in milliseconds.</value>
    public int OutOfBandMessagingTimeout { get; set; }
    
    /// <summary>
    /// Gets the out of band message processor.
    /// </summary>
    /// <value>The out of band message processor.</value>
    protected IMessageProcessor OutOfBandMessageProcessor { get; }

    /// <summary>
    /// Processes the message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>An <see>
    ///         <cref>IEnumerable</cref>
    ///     </see>
    ///     containing response messages.</returns>
    /// <exception cref="MessagingException">An error occurred during the processing of the message.</exception>
    public virtual IEnumerable<string>? ProcessMessage(string message)
    {
        // If we have been in an Out of Band message processing state too long
        // we reset the state along with all out of band processors.
        if (_State.CurrentState == MessagingState.OUtOfBand && (DateTime.UtcNow - _State.LastMessageReceived).Milliseconds >= OutOfBandMessagingTimeout)
        {
            OutOfBandMessageProcessor.Reset();
            _State.Reset();
        }
        // Otherwise if the state is finished from our previous message, we reset the state.
        else if (_State.Finished)
            _State.Reset();

        // Set our last message receipt time
        _State.LastMessageReceived = DateTime.UtcNow;

        // If the message is an out of band message, modify the message and our state
        if (message.StartsWith(OutOfBandPrefix))
        {
            message = message.Length > OutOfBandPrefix.Length ? message.Remove(OutOfBandPrefix.Length) : String.Empty;
            _State.CurrentState = MessagingState.OUtOfBand;
        }

        // If we are in an out of band state, assign processing to our out of band processor if no other processor is already defined.
        if (_State.CurrentState == MessagingState.OUtOfBand && _State.CurrentProcessor == null)
            _State.CurrentProcessor = OutOfBandMessageProcessor;

        // If we have a predefined processor, then hand off work to it.
        if (_State.CurrentProcessor != null)
            if (_State.CurrentProcessor.ProcessMessage(message, ref _State))
                return _State.Finished ? _State.Response : null;

        // If we have reached this point, then we have plain vanilla display line, so we return it as a message to display
        _State.Response.Clear();
        _State.Finished = true;
        _State.Response.Add(message);
        return _State.Response;
    }
}