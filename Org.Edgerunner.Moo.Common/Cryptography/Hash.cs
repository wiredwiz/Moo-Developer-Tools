﻿#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="Hashing.cs">
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

using System.Security.Cryptography;
using System.Text;

namespace Org.Edgerunner.Moo.Common.Cryptography;

/// <summary>
/// Class that handles various hashes
/// </summary>
public static class Hash
{
   /// <summary>
   /// Hashes the value using the Sha256 hashing algorithm.
   /// </summary>
   /// <param name="value">The value to hash.</param>
   /// <returns></returns>
   /// <exception cref="System.ArgumentNullException">value</exception>
   public static string Sha256(string value)
   {
      if (string.IsNullOrEmpty(value))
         throw new ArgumentNullException(nameof(value));

      var sb = new StringBuilder();

      using (var hash = SHA256.Create())
      {
         var enc = Encoding.UTF8;
         var result = hash.ComputeHash(enc.GetBytes(value));

         foreach (byte b in result)
            sb.Append(b.ToString("x2"));
      }

      return sb.ToString();
   }
}