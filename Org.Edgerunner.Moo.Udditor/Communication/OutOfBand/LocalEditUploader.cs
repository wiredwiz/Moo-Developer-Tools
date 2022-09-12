#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="LocalEditUploader.cs">
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

using Org.Edgerunner.Mud.Communication.Interfaces;

namespace Org.Edgerunner.Moo.Udditor.Communication.OutOfBand;

/// <summary>
/// Class responsible for handling local edit protocol uploads.
/// </summary>
/// <seealso cref="Org.Edgerunner.Mud.Communication.Interfaces.IClientUploader" />
public sealed class LocalEditUploader : IClientUploader
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalEditUploader"/> class.
    /// </summary>
    /// <param name="uploadCommand">The upload command.</param>
    /// <param name="clientTerminal">The client terminal.</param>
    public LocalEditUploader(string uploadCommand, IClientTerminal clientTerminal)
    {
        UploadCommand = uploadCommand;
        ClientTerminal = clientTerminal;
    }

    private string UploadCommand { get; }

    /// <summary>
    /// Gets the client terminal.
    /// </summary>
    /// <value>
    /// The client terminal.
    /// </value>
    public IClientTerminal ClientTerminal { get; }

    /// <summary>
    /// Uploads the content of this instance.
    /// </summary>
    /// <param name="sourceCode">The source code.</param>
    /// <returns>
    /// <c>true</c> if upload is successful; <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public bool Upload(string sourceCode)
    {
        if (!ClientTerminal.IsConnected)
            return false;
        var echoEnabled = ClientTerminal.EchoEnabled;
        ClientTerminal.EchoEnabled = false;
        ClientTerminal.SendTextLine(UploadCommand);
        ClientTerminal.SendTextLine(sourceCode);
        ClientTerminal.SendTextLine(".");
        ClientTerminal.EchoEnabled = echoEnabled;
        return true;
    }
}
