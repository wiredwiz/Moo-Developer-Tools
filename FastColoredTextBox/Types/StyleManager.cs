#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="StyleManager.cs">
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

using System.Collections.Generic;

namespace FastColoredTextBoxNS.Types;

/// <summary>
/// A class in charge of managing a list of <see cref="Style"/> instances.
/// </summary>
// ReSharper disable once HollowTypeName
public class StyleManager
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StyleManager"/> class.
    /// </summary>
    public StyleManager()
    {
        Styles = new HashSet<Style>();
    }

    /// <summary>
    /// Gets or sets the managed styles.
    /// </summary>
    /// <value>The styles.</value>
    protected HashSet<Style> Styles { get; set; }

    /// <summary>Adds the style to the managed Styles.</summary>
    /// <param name="style">The style to add.</param>
    /// <returns>Style.</returns>
    public Style AddStyle(Style style)
    {
        Styles.Add(style);
        return style;
    }

    public HashSet<Style> GetStyles()
    {
        return Styles;
    }

    /// <summary>
    /// Removes the style from the managed list.
    /// </summary>
    /// <param name="style">The style to remove.</param>
    /// <returns>Style.</returns>
    public Style RemoveStyle(Style style)
    {
        return Styles.Remove(style) ? style : null;
    }

    /// <summary>
    /// Determines whether the specified style is managed by this instance.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <returns><c>true</c> if the specified style is managed; otherwise, <c>false</c>.</returns>
    public bool IsManaged(Style style)
    {
        return Styles.Contains(style);
    }

    /// <summary>
    /// Clears all managed styles.
    /// </summary>
    public void Clear()
    {
        Styles.Clear();
    }
}