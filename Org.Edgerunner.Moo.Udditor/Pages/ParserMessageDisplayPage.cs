#region BSD 3-Clause License

// <copyright company="Edgerunner.org" file="ParserMessageDisplayPage.cs">
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

using System;
using Krypton.Navigator;
using Org.Edgerunner.ANTLR4.Tools.Common.Grammar.Errors;
using Org.Edgerunner.Moo.Editor.Configuration;
using Org.Edgerunner.Moo.Editor.Controls;
using Org.Edgerunner.Moo.Udditor.Events;

namespace Org.Edgerunner.Moo.Udditor.Pages;

public class ParserMessageDisplayPage : ManagedPage
{
    #region Constructors And Finalizers

    /// <summary>
    ///     Initializes a new instance of the <see cref="ParserMessageDisplayPage" /> class.
    /// </summary>
    /// <param name="manager">The window manager.</param>
    public ParserMessageDisplayPage(WindowManager manager)
    : base(manager)
    {
        MessageDisplay = new ErrorDisplay();
        ConfigureMessageDisplay(MessageDisplay);
        // ReSharper disable VirtualMemberCallInConstructor
        UniqueName = "ParserMessages";
        Text = "Parser Messages";
        TextTitle = "Parser Messages";
        TextDescription = "A list of parser messages";
        ClearFlags(KryptonPageFlags.DockingAllowClose);
        // ReSharper restore VirtualMemberCallInConstructor
        MessageDisplay.Dock = DockStyle.Fill;
        Controls.Add(MessageDisplay);
        MessageDisplay.DoubleClick += MessageDisplay_DoubleClick;
    }

    #endregion

    public ErrorDisplay MessageDisplay { get; }

    /// <summary>
    ///     Occurs when [double click].
    /// </summary>
    public new event EventHandler<ParserMessageDoubleClickEventArgs> DoubleClick;

    private void ConfigureMessageDisplay(ErrorDisplay display)
    {
        var font = new Font(Settings.Instance.EditorFontFamily, Settings.Instance.EditorFontSize);
        display.Font = font;
        display.ForeColor = Settings.Instance.EditorTextColor;
        display.BackColor = Settings.Instance.EditorBackgroundColor;
    }

    private void MessageDisplay_DoubleClick(object sender, EventArgs e)
    {
        if (MessageDisplay.SelectedItems.Count == 0 || MessageDisplay.SelectedItems[0] == null)
        {
            DoubleClick?.Invoke(this,
                                new ParserMessageDoubleClickEventArgs(0, 0, string.Empty, string.Empty, null));
        }

        var selected = MessageDisplay.SelectedItems[0];
        var key = selected.SubItems[0].Text;
        var line = int.Parse(selected.SubItems[2].Text) - 1;
        var col = int.Parse(selected.SubItems[3].Text) - 1;
        var msg = selected.SubItems[4].Text;
        var guide = selected.Tag as ISyntaxErrorGuide;
        DoubleClick?.Invoke(this, new ParserMessageDoubleClickEventArgs(line, col, key, msg, guide));
    }
}
