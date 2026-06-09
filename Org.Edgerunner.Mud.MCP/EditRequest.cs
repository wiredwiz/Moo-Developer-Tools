#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="EditRequest.cs">
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
/// An immutable model representing a single <c>dns-org-mud-moo-simpleedit</c> edit request
/// handed from the server to the client.
/// </summary>
public class EditRequest
{
   /// <summary>
   /// Initializes a new instance of the <see cref="EditRequest"/> class.
   /// </summary>
   /// <param name="reference">The opaque, machine-readable reference echoed back verbatim on save.</param>
   /// <param name="name">The human-readable label for the edit buffer.</param>
   /// <param name="editType">The content type (<c>string</c>, <c>string-list</c>, or <c>moo-code</c>).</param>
   /// <param name="content">The text to edit.</param>
   public EditRequest(string reference, string name, string editType, string content)
   {
      Reference = reference;
      Name = name;
      EditType = editType;
      Content = content;
   }

   /// <summary>
   /// Gets the opaque, machine-readable reference. The client treats it as a black box and
   /// echoes it back verbatim in the matching <c>set</c> message.
   /// </summary>
   public string Reference { get; }

   /// <summary>
   /// Gets the human-readable label for the edit buffer (used as the editor window/tab title).
   /// </summary>
   public string Name { get; }

   /// <summary>
   /// Gets the content type. One of <c>string</c>, <c>string-list</c>, or <c>moo-code</c>.
   /// </summary>
   public string EditType { get; }

   /// <summary>
   /// Gets the text to edit.
   /// </summary>
   public string Content { get; }
}
