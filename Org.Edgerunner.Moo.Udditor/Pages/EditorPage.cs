#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="EditorPage.cs">
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

using Org.Edgerunner.ANTLR4.Tools.Common;
using Org.Edgerunner.Moo.Communication.Interfaces;
using Org.Edgerunner.Moo.Editor.Controls;

namespace Org.Edgerunner.Moo.Udditor.Pages;

public abstract class EditorPage : ManagedPage
{
    protected EditorPage(WindowManager manager)
        : base(manager)
    {
    }

    /// <summary>
    /// Gets or sets the source editor.
    /// </summary>
    /// <value>The source editor.</value>
    public abstract FastColoredTextBoxNS.FastColoredTextBox SourceEditor { get; }

    /// <summary>
    /// Gets the current line number.
    /// </summary>
    /// <value>The current line number.</value>
    public abstract int CurrentLineNumber { get; }

    /// <summary>
    /// Gets the current column position.
    /// </summary>
    /// <value>The current column position.</value>
    public abstract int CurrentColumnPosition { get; }

    /// <summary>
    /// Gets or sets the document.
    /// </summary>
    /// <value>The document.</value>
    public virtual DocumentInfo Document { get; set; }

    /// <summary>
    /// Gets or sets the uploader.
    /// </summary>
    /// <value>
    /// The uploader.
    /// </value>
    public virtual IClientUploader Uploader { get; set; }

    /// <summary>
    /// Gets a value indicating whether this instance can upload its contents.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance can upload; otherwise, <c>false</c>.
    /// </value>
    public virtual bool CanUpload => Uploader != null && Uploader.ClientTerminal.IsConnected;
}
