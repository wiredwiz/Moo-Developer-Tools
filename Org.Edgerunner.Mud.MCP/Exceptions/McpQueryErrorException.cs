#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="McpQueryErrorException.cs">
// Copyright (c) Thaddeus Ryker 2026
// </copyright>
//
// BSD 3-Clause License
//
// Copyright (c) 2026,
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

namespace Org.Edgerunner.Mud.MCP.Exceptions;

/// <summary>
/// Represents an <c>edgerunner-org-moo-query-error</c> reply from the server: a MOO error code
/// (e.g. <c>E_PERM</c>, <c>E_VERBNF</c>) plus the server's human-readable message.
/// </summary>
public class McpQueryErrorException : Exception
{
   /// <summary>
   /// Initializes a new instance of the <see cref="McpQueryErrorException"/> class.
   /// </summary>
   /// <param name="code">The MOO error constant name reported by the server.</param>
   /// <param name="message">The server's human-readable error message.</param>
   public McpQueryErrorException(string code, string message)
      : base(message)
   {
      Code = code;
   }

   /// <summary>
   /// Gets the MOO error constant name reported by the server (e.g. <c>E_PERM</c>).
   /// </summary>
   public string Code { get; }
}
