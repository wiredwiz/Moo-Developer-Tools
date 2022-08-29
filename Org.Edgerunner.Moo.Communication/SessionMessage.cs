#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="SessionMessage.cs">
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

namespace Org.Edgerunner.Moo.Communication;

/// <summary>
/// Struct representing a client session message.
/// </summary>
public struct SessionMessage
{
   /// <summary>
   /// Initializes a new instance of the <see cref="SessionMessage"/> struct.
   /// </summary>
   /// <param name="text">The text.</param>
   /// <param name="outOfBand">if set to <c>true</c> [out of band].</param>
   public SessionMessage(string text, bool outOfBand)
   {
      Text = text;
      OutOfBand = outOfBand;
   }

   /// <summary>
   /// Gets or sets the message text.
   /// </summary>
   /// <value>
   /// The message text.
   /// </value>
   public string Text { get; }

   /// <summary>
   /// Gets or sets a value indicating whether the message is out of band.
   /// </summary>
   /// <value>
   ///   <c>true</c> if [out of band]; otherwise, <c>false</c>.
   /// </value>
   public bool OutOfBand { get; }
}