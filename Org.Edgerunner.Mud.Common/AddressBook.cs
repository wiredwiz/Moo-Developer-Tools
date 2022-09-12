﻿#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="AddressBook.cs">
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

using System.Xml;
using System.Xml.Serialization;

namespace Org.Edgerunner.Mud.Common;

/// <summary>
/// Class that represents an address book of worlds.
/// </summary>
[Serializable]
public class AddressBook
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddressBook"/> class.
    /// </summary>
    public AddressBook()
    {
        Worlds = new List<WorldConfiguration>();
    }

    /// <summary>
    /// Gets or sets the worlds in the address book.
    /// </summary>
    /// <value>The list of worlds.</value>
    public List<WorldConfiguration> Worlds { get; set; }

    /// <summary>
    /// Saves this <see cref="AddressBook" /> to a file.
    /// </summary>
    /// <param name="filePath">The file path to save to.</param>
    public void SaveToFile(string filePath)
    {
       SaveToFile(this, filePath);
    }

   /// <summary>
   /// Saves this <see cref="AddressBook" /> to a file.
   /// </summary>
   /// <param name="book">The address book.</param>
   /// <param name="filePath">The file path to save to.</param>
   public static void SaveToFile(AddressBook book, string filePath)
    {
        XmlWriterSettings xmlWriterSettings = new()
                                              {
                                                  Indent = true,
                                                  CloseOutput = true
                                              };
        XmlSerializer serializer = new(typeof(AddressBook));
        using (XmlWriter xmlWriter = XmlWriter.Create(filePath, xmlWriterSettings))
            serializer.Serialize(xmlWriter, book);
    }

    /// <summary>
    /// Saves this <see cref="AddressBook"/> to a file.
    /// </summary>
    /// <param name="filePath">The file path to save to.</param>
    public static AddressBook? LoadFromFile(string filePath)
    {
        var serializer = new XmlSerializer(typeof(AddressBook));
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
            return serializer.Deserialize(fs) as AddressBook;
    }
}