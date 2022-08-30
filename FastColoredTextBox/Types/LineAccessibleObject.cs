#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="LineAccessibleObject.cs">
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

using System;
using System.Drawing;
using System.Windows.Forms;

namespace FastColoredTextBoxNS.Types;

/// <summary>
/// A class representing an accessibility object for a text line.
/// </summary>
/// <seealso cref="System.Windows.Forms.AccessibleObject" />
public class LineAccessibleObject : AccessibleObject
{
   public LineAccessibleObject(FastColoredTextBox textBox, Line line, int offset)
   {
      Line = line;
      Offset = offset;
      TextBox = textBox;
   }

   protected Line Line { get; set; }

   protected int Offset { get; set; }

   protected FastColoredTextBox TextBox { get; set; }

   /// <summary>
   /// Gets the location and size of the accessible object.
   /// </summary>
   public override Rectangle Bounds
   {
      get
      {
         var floor = TextBox.AccessibilityObject.Bounds.Top;
         var boundsY = Offset * TextBox.CharHeight + floor - 2;
         var boundsX = TextBox.AccessibilityObject.Bounds.X - 2;
         var width = TextBox.AccessibilityObject.Bounds.Width + 4;
         return new Rectangle(boundsX, boundsY, width, TextBox.CharHeight + 4);
      }
   }

   /// <summary>
   /// Gets a string that describes the default action of the object. Not all objects have a default action.
   /// </summary>
   public override string DefaultAction => "Edit";

   /// <summary>
   /// Gets a string that describes the visual appearance of the specified object. Not all objects have a description.
   /// </summary>
   public override string Description => "Text line";

   /// <summary>
   /// Gets a description of what the object does or how the object is used.
   /// </summary>
   public override string Help => "Edit";

   /// <summary>
   /// Gets or sets the object name.
   /// </summary>
   public override string Name
   {
      get => Line.Text;
      set => throw new InvalidOperationException();
   }

   /// <summary>
   /// Gets the parent of an accessible object.
   /// </summary>
   public override AccessibleObject Parent => TextBox.AccessibilityObject;

   /// <summary>
   /// Gets the role of this accessible object.
   /// </summary>
   public override AccessibleRole Role => AccessibleRole.Text;

   /// <summary>
   /// Gets the state of this accessible object.
   /// </summary>
   public override AccessibleStates State => AccessibleStates.Focusable | AccessibleStates.Selectable;
}