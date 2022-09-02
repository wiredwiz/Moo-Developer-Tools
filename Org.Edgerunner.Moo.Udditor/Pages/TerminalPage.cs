#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="TerminalPage.cs">
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

using Krypton.Navigator;
using Org.Edgerunner.Moo.Editor.Controls;
using System;
using JetBrains.Annotations;
using Org.Edgerunner.Moo.Communication;
using Org.Edgerunner.Moo.Communication.Interfaces;
using Krypton.Toolkit;

namespace Org.Edgerunner.Moo.Udditor.Pages;

public class TerminalPage : ManagedPage
{
    private string _OutOfBandPrefix;

    /// <summary>
    /// Initializes a new instance of the <see cref="TerminalPage" /> class.
    /// </summary>
    /// <param name="messageProcessor">The message processor.</param>
    /// <param name="manager">The window manager.</param>
    /// <param name="worldName">Name of the world.</param>
    /// <param name="useTls">if set to <c>true</c> [use TLS].</param>
    public TerminalPage(WindowManager manager, [CanBeNull] IMessageProcessor messageProcessor, string worldName, bool useTls = false)
    : base(manager)
    {
        Terminal = new MooClientTerminal(useTls);
        Terminal.MessageProcessor = messageProcessor;
        // ReSharper disable VirtualMemberCallInConstructor
        Text = worldName;
        TextTitle = worldName;
        TextDescription = worldName;
        UniqueName = Guid.NewGuid().ToString();
        ToolTipBody = worldName;
        ToolTipStyle = LabelStyle.ToolTip;
        // ReSharper restore VirtualMemberCallInConstructor
        Terminal.Dock = DockStyle.Fill;
        Controls.Add(Terminal);
    }

    /// <summary>
    /// Occurs when [new message(s) received].
    /// </summary>
    public event EventHandler NewMessageReceived
    {
        add { Terminal.NewMessageReceived += value; }
        remove { Terminal.NewMessageReceived -= value; }
    }

    public MooClientTerminal Terminal { get; set; }

    /// <summary>
    /// Gets or sets the out of band prefix for messages with this terminal.
    /// </summary>
    /// <value>
    /// The out of band prefix for messages with this terminal.
    /// </value>
    public string OutOfBandPrefix
    {
        get => _OutOfBandPrefix;
        set
        {
            _OutOfBandPrefix = value;
            Terminal.OutOfBandPrefix = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether word wrap is enabled in the editor.
    /// </summary>
    /// <value>
    ///   <c>true</c> if word wrap enabled; otherwise, <c>false</c>.
    /// </value>
    public bool WordWrap
    {
        get => Terminal.WordWrap;
        set => Terminal.WordWrap = value;
    }
}
