#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="FCTBAccessibleObject.cs">
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

using System.Drawing;
using System.Windows.Forms;

namespace FastColoredTextBoxNS.Types;

/// <summary>
/// Class that implements accessibility for a Fast Colored Text Box instance.
/// Implements the <see cref="AccessibleObject" />
/// </summary>
/// <seealso cref="AccessibleObject" />
// ReSharper disable once InconsistentNaming
public class FCTBAccessibleObject : AccessibleObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FCTBAccessibleObject"/> class.
    /// </summary>
    /// <param name="textBox">The text box.</param>
    public FCTBAccessibleObject(FastColoredTextBox textBox)
    {
        TextBox = textBox;
        Name = string.Empty;
    }

    /// <summary>
    /// Gets or sets the text box.
    /// </summary>
    /// <value>The text box.</value>
    protected FastColoredTextBox TextBox { get; set; }

    /// <summary>
    /// Gets the location and size of the accessible object.
    /// </summary>
    /// <value>The bounds.</value>
    public override Rectangle Bounds => new Rectangle(TextBox.PointToScreen(TextBox.Location), TextBox.Size);

    /// <summary>
    /// Gets the role of this accessible object.
    /// </summary>
    /// <value>The role.</value>
    public override AccessibleRole Role => AccessibleRole.Text;

    /// <summary>
    /// Gets a string that describes the visual appearance of the specified object. Not all objects have a description.
    /// </summary>
    /// <value>The description.</value>
    public override string Description => "A text box";

    /// <summary>
    /// Gets or sets the object name.
    /// </summary>
    /// <value>The name.</value>
    public override string Name { get; set; } = "sample name text";

   /// <summary>
   /// Gets a string that describes the default action of the object. Not all objects have a default action.
   /// </summary>
   public override string DefaultAction => "Edit";

   protected string _Value = "foo text";

   /// <summary>
   /// Gets or sets the value of an accessible object.
   /// </summary>
   public override string Value
    {
       get => _Value;
       set => _Value = value;
    }

    /// <summary>
    /// Gets the state of this accessible object.
    /// </summary>
    /// <value>The state.</value>
    public override AccessibleStates State
    {
        get
        {
            AccessibleStates state = AccessibleStates.Selectable | AccessibleStates.Focusable;
            //if (TextBox.Focused)
            //    state |= AccessibleStates.Selected | AccessibleStates.Focused;
            return state;
        }
    }

    /// <summary>
    /// Gets a description of what the object does or how the object is used.
    /// </summary>
    /// <value>The help.</value>
    public override string Help => "Edit";

    /// <summary>
    /// Gets the parent of an accessible object.
    /// </summary>
    /// <value>The parent.</value>
    public override AccessibleObject Parent => Application.OpenForms[0].AccessibilityObject;// TextBox.Parent.AccessibilityObject;
}