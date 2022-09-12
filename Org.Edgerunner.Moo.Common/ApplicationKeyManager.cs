#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="ApplicationKeyManager.cs">
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

using System.Security;
using Microsoft.Win32;

namespace Org.Edgerunner.Moo.Common;

/// <summary>
/// Class for managing application keys.
/// </summary>
// ReSharper disable once HollowTypeName
public static class ApplicationKeyManager
{
   /// <summary>
   /// Retrieves the master key from the registry.
   /// </summary>
   /// <param name="key">The master key.</param>
   /// <returns>
   /// <c>true</c> if the key was retrieved; <c>false</c> otherwise.
   /// </returns>
   /// <exception cref="SecurityException">The user does not have the permissions required to create or open the registry key.</exception>
   /// <exception cref="UnauthorizedAccessException">The <see cref="RegistryKey" /> cannot be written to; for example, it was not opened as a writable key , or the user does not have the necessary access rights.</exception>
   /// <exception cref="IOException">The nesting level exceeds 510.  
   ///  -or-  
   ///  A system error occurred, such as deletion of the key, or an attempt to create a key in the <see cref="Microsoft.Win32.Registry.LocalMachine" /> root.</exception>
   public static bool RetrieveMasterKey(out string? key)
   {
#pragma warning disable CA1416 // Validate platform compatibility
      // ReSharper disable once ExceptionNotDocumentedOptional
      RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Udditor");
      key = registryKey.GetValue("Key")?.ToString();
      registryKey.Close();
#pragma warning restore CA1416 // Validate platform compatibility
      if (string.IsNullOrEmpty(key))
         return false;

      return true;
   }

   /// <summary>
   /// Saves the master key to the registry.
   /// </summary>
   /// <param name="key">The key to store.</param>
   /// <exception cref="UnauthorizedAccessException">The <see cref="RegistryKey" /> cannot be written to; for example, it was not opened as a writable key , or the user does not have the necessary access rights.</exception>
   /// <exception cref="SecurityException">The user does not have the permissions required to create or open the registry key.</exception>
   /// <exception cref="IOException">The nesting level exceeds 510.  
   ///  -or-  
   ///  A system error occurred, such as deletion of the key, or an attempt to create a key in the <see cref="Microsoft.Win32.Registry.LocalMachine" /> root.</exception>
   public static void SaveMasterKey(string key)
   {
#pragma warning disable CA1416 // Validate platform compatibility
      // ReSharper disable once ExceptionNotDocumentedOptional
      RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Udditor");
      registryKey.SetValue("Key", key);
      registryKey.Close();
#pragma warning restore CA1416 // Validate platform compatibility
   }
}
