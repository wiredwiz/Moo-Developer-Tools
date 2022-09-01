#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="Message.cs">
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

namespace Org.Edgerunner.Mud.MCP;

/// <summary>
/// A class representing an MCP message.
/// </summary>
public class Message
{
   /// <summary>
   /// Initializes a new instance of the <see cref="Message"/> class.
   /// </summary>
   /// <param name="name">The message name.</param>
   /// <param name="key">The message key.</param>
   /// <param name="data">The message data dictionary.</param>
   public Message(string name, string key, Dictionary<string, string> data)
   {
      Name = name;
      Key = key;
      Data = data;
   }

   /// <summary>
   /// Gets the message name.
   /// </summary>
   /// <value>
   /// The message name.
   /// </value>
   public string Name { get; }

   /// <summary>
   /// Gets the message key.
   /// </summary>
   /// <value>
   /// The message key.
   /// </value>
   public string Key { get; }

   /// <summary>
   /// Gets the message key/value pair dictionary.
   /// </summary>
   /// <value>
   /// The data dictionary.
   /// </value>
   public Dictionary<string, string> Data { get; }
}