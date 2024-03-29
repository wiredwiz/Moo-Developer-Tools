﻿#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="InvalidMcpMessageException.cs">
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

using System.Runtime.Serialization;

namespace Org.Edgerunner.Mud.MCP.Exceptions;

/// <summary>
/// An exception that represents a badly formed Mcp Message.
/// </summary>
/// <seealso cref="Exception" />
public class InvalidMcpMessageFormatException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidMcpMessageFormatException"/> class.
    /// </summary>
    public InvalidMcpMessageFormatException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidMcpMessageFormatException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
    protected InvalidMcpMessageFormatException(SerializationInfo info, StreamingContext context)
       : base(info, context)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidMcpMessageFormatException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidMcpMessageFormatException(string? message)
       : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidMcpMessageFormatException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
    public InvalidMcpMessageFormatException(string? message, Exception? innerException)
       : base(message, innerException)
    {
    }
}