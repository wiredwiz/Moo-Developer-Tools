#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="MessageProcessingState.cs">
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

namespace Org.Edgerunner.Mud.Communication;

/// <summary>
/// Class representing the current message state.
/// </summary>
public class MessageProcessingState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageProcessingState"/> class.
    /// </summary>
    public MessageProcessingState()
        : this(MessagingState.InBand, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageProcessingState"/> class.
    /// </summary>
    /// <param name="currentProcessor">The current processor.</param>
    /// <remarks>The messaging state is set to default in this case.</remarks>
    public MessageProcessingState(IMessageProtocolProcessor? currentProcessor)
        :this(MessagingState.InBand, currentProcessor)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageProcessingState"/> class.
    /// </summary>
    /// <param name="currentState">The state.</param>
    /// <param name="currentProcessor">The current processor.</param>
    public MessageProcessingState(MessagingState currentState, IMessageProtocolProcessor? currentProcessor)
    {
        CurrentState = currentState;
        CurrentProcessor = currentProcessor;
        LastMessageReceived = DateTime.UtcNow;
        Finished = false;
    }

    /// <summary>
    /// Gets or sets the state of the current.
    /// </summary>
    /// <value>The state of the current.</value>
    public MessagingState CurrentState { get; set; }

    /// <summary>
    /// Gets or sets the current processor.
    /// </summary>
    /// <value>The current processor.</value>
    public IMessageProtocolProcessor? CurrentProcessor { get; set; }

    /// <summary>
    /// Gets or sets the time for when the last message was received.
    /// </summary>
    /// <value>The last message received time.</value>
    public DateTime LastMessageReceived { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether processing of the current message state is finished.
    /// </summary>
    /// <value><c>true</c> if finished; otherwise, <c>false</c>.</value>
    public bool Finished { get; set; }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        CurrentProcessor = null;
        CurrentState = MessagingState.InBand;
        Finished = false;
        LastMessageReceived = DateTime.UtcNow;
    }
}