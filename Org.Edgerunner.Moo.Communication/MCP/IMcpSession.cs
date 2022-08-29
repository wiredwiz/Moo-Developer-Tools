#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="IMcpSession.cs">
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

namespace Org.Edgerunner.Moo.Communication.MCP;

/// <summary>
/// Interface representing a specific MCP session.
/// </summary>
public interface IMcpSession
{
   /// <summary>
   /// Gets or sets the manager for this session.
   /// </summary>
   /// <value>
   /// The manager.
   /// </value>
   McpClientSessionManager Manager { get; }

   /// <summary>
   /// Gets or sets the key negotiated for this session.
   /// </summary>
   /// <value>
   /// The session key.
   /// </value>
   string Key { get; }

   /// <summary>
   /// Gets or sets the protocol version being used for this session.
   /// </summary>
   /// <value>
   /// The protocol version.
   /// </value>
   double ProtocolVersion { get; }

   /// <summary>
   /// Gets or sets the supported MCP packages for this session.
   /// </summary>
   /// <value>
   /// The supported MCP packages.
   /// </value>
   List<IMcpPackage> SupportedPackages { get; }

   /// <summary>
   /// Generates a handshakes reply from this instance.
   /// </summary>
   /// <returns>The handshake reply.</returns>
   string Handshake();
}