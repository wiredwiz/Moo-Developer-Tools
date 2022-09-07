#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="World.cs">
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

using System.Xml.Serialization;
using static System.String;

namespace Org.Edgerunner.Moo.Common;

/// <summary>
/// Class that represents a Mud/Muck/Mush/Moo World.
/// </summary>
[Serializable]
[XmlType(TypeName = "World")]
public class WorldConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorldConfiguration"/> class.
    /// </summary>
    public WorldConfiguration()
        : this(Empty, Empty, 0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldConfiguration"/> class.
    /// </summary>
    /// <param name="name">The world name.</param>
    /// <param name="hostAddress">The host world address.</param>
    /// <param name="portNumber">The port world number.</param>
    public WorldConfiguration(string name, string hostAddress, int portNumber)
    {
        Name = name;
        HostAddress = hostAddress;
        PortNumber = portNumber;
        EchoEnabled = true;
        LocalEditEnabled = true;
        McpEnabled = true;
        ColorEnabled = true;
        UserInfo = new UserLogin(Empty, Empty);
        ShowAsMenuShortcut = false;
        UseTls = false;
    }

    /// <summary>
    /// Gets or sets the world name.
    /// </summary>
    /// <value>The name.</value>
    [XmlAttribute(AttributeName ="Name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the world host address.
    /// </summary>
    /// <value>The host address.</value>
    [XmlAttribute(AttributeName ="host")]
    public string HostAddress { get; set; }

    /// <summary>
    /// Gets or sets the world port number.
    /// </summary>
    /// <value>The port number.</value>
    [XmlAttribute(AttributeName ="port")]
    public int PortNumber { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use TLS.
    /// </summary>
    /// <value>
    ///   <c>true</c> if [use TLS]; otherwise, <c>false</c>.
    /// </value>
    [XmlElement("UseTLS")]
    public bool UseTls { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether echo is enabled.
    /// </summary>
    /// <value><c>true</c> if [echo enabled]; otherwise, <c>false</c>.</value>
    [XmlElement(ElementName = "EchoEnabled")]
    public bool EchoEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether local edit is enabled.
    /// </summary>
    /// <value><c>true</c> if [local edit enabled]; otherwise, <c>false</c>.</value>
    public bool LocalEditEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether MCP is enabled.
    /// </summary>
    /// <value><c>true</c> if [MCP enabled]; otherwise, <c>false</c>.</value>
    [XmlElement(ElementName = "McpEnabled")]
    public bool McpEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether color is enabled.
    /// </summary>
    /// <value><c>true</c> if [color enabled]; otherwise, <c>false</c>.</value>
    [XmlElement(ElementName = "ColorEnabled")]
    public bool ColorEnabled { get; set; }

    /// <summary>
    /// Gets or sets the user login information.
    /// </summary>
    /// <value>The user login information.</value>
    [XmlElement(ElementName = "User")]
    public UserLogin UserInfo { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether world is shown as terminal menu shortcut.
    /// </summary>
    /// <value><c>true</c> if [show as menu shortcut]; otherwise, <c>false</c>.</value>
    [XmlElement(ElementName = "ShowAsMenuShortcut")]
    public bool ShowAsMenuShortcut { get; set; }
}