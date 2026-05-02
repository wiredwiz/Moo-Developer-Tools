#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="ScreenReader.cs">
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

using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FastColoredTextBoxNS.Types;

internal class UnsafeNativeMethods
{
   // ReSharper disable once InconsistentNaming
   // ReSharper disable once IdentifierTypo
   public const uint SPI_GETSCREENREADER = 0x0046;

   [DllImport("user32.dll", SetLastError = true)]
   [return: MarshalAs(UnmanagedType.Bool)]
   public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref bool pvParam, uint fWinIni);
}

/// <summary>
/// Class representing ScreenReader functions.
/// </summary>
public static class ScreenReader
{
   /// <summary>
   /// Gets a value indicating whether a screen reader is currently running.
   /// </summary>
   /// <value><c>true</c> if this instance is running; otherwise, <c>false</c>.</value>
   /// <exception cref="System.ComponentModel.Win32Exception">error calling SystemParametersInfo</exception>
   public static bool IsRunning
   {
      get
      {
         bool returnValue = false;
         if (!UnsafeNativeMethods.SystemParametersInfo(UnsafeNativeMethods.SPI_GETSCREENREADER, 0, ref returnValue, 0))
         {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "error calling SystemParametersInfo");
         }
         return returnValue;
      }
   }
}
