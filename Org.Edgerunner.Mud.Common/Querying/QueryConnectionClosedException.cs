#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="QueryConnectionClosedException.cs">
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

namespace Org.Edgerunner.Mud.Common.Querying;

/// <summary>
/// The exception that is thrown when a pending world query fails because the connection to the world
/// was closed (a disconnect or session teardown), as opposed to timing out. Callers can catch this to
/// distinguish a closed connection from a <see cref="TimeoutException"/>.
/// </summary>
public sealed class QueryConnectionClosedException : Exception
{
   /// <summary>
   /// Initializes a new instance of the <see cref="QueryConnectionClosedException"/> class.
   /// </summary>
   public QueryConnectionClosedException()
      : base("The query failed because the connection to the world was closed.")
   {
   }

   /// <summary>
   /// Initializes a new instance of the <see cref="QueryConnectionClosedException"/> class.
   /// </summary>
   /// <param name="message">The message that describes the error.</param>
   public QueryConnectionClosedException(string message)
      : base(message)
   {
   }

   /// <summary>
   /// Initializes a new instance of the <see cref="QueryConnectionClosedException"/> class.
   /// </summary>
   /// <param name="message">The message that describes the error.</param>
   /// <param name="inner">The exception that is the cause of the current exception.</param>
   public QueryConnectionClosedException(string message, Exception inner)
      : base(message, inner)
   {
   }
}
