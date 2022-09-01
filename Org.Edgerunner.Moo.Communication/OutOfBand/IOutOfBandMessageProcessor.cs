#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="IOutOfBandMessageProcessor.cs">
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

namespace Org.Edgerunner.Moo.Communication.OutOfBand;

/// <summary>
/// Interface that represents a message processor for out of band messages.
/// Extends the <see cref="Org.Edgerunner.Moo.Communication.IMessageProcessor" />
/// </summary>
/// <seealso cref="Org.Edgerunner.Moo.Communication.IMessageProcessor" />
public interface IOutOfBandMessageProcessor : IMessageProcessor
{
    /// <summary>
    /// Registers the handler.
    /// </summary>
    /// <param name="handler">The handler.</param>
    void RegisterHandler(IOutOfBandMessageHandler handler);

    /// <summary>
    /// Unregisters the handler.
    /// </summary>
    /// <param name="handler">The handler.</param>
    void UnregisterHandler(IOutOfBandMessageHandler handler);
}