#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="User.cs">
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
using Org.Edgerunner.Moo.Common.Encryption;
using static System.String;

namespace Org.Edgerunner.Moo.Common;

[Serializable]
[XmlType(TypeName = "User")]
public class UserLogin
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserLogin"/> class.
    /// </summary>
    protected UserLogin()
        : this(Empty, Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserLogin"/> class.
    /// </summary>
    /// <param name="name">The user name.</param>
    /// <param name="password">The user password.</param>
    public UserLogin(string name, string password)
    {
        Name = name;
        Password = password;
        ConnectionString = @"co %u %p";
        PromptForCredentials = false;
        AutomaticallyLogin = true;
    }

    private const string _Key = "34avs#$%k7ikasS$564sdfaA%*12";

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    /// <value>The user name.</value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the user password.
    /// </summary>
    /// <value>The user password.</value>
    public string Password { get; set; }

    /// <summary>
    /// Gets or sets the connection string to use when logging in.
    /// </summary>
    /// <value>The connection string.</value>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to automatically login.
    /// </summary>
    /// <value><c>true</c> if [automatically login]; otherwise, <c>false</c>.</value>
    public bool AutomaticallyLogin { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to prompt the user for credentials.
    /// </summary>
    /// <value><c>true</c> if [prompt for credentials]; otherwise, <c>false</c>.</value>
    public bool PromptForCredentials { get; set; }

    /// <summary>
    /// Gets the decrypted password.
    /// </summary>
    /// <value>
    /// The decrypted password.
    /// </value>
    [XmlIgnore]
    public string DecryptedPassword
    {
        get => !IsNullOrEmpty(Password) ? AesCrypto.Decrypt(Password, _Key) ?? Empty : Empty;
        set => Password = IsNullOrEmpty(value) ? Empty : AesCrypto.Encrypt(value, _Key);
    }
}