#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="DelegateExtensions.cs">
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

using System.ComponentModel;
using System.Diagnostics;

namespace Org.Edgerunner.Common.Extensions;

/// <summary>
/// A class containing extension methods for delegates
/// </summary>
public static class DelegateExtensions
{
   /// <summary>
   /// Invokes the delegate on the UI thread.
   /// </summary>
   /// <param name="subject">The subject delegate.</param>
   /// <param name="args">The arguments to pass.</param>
   /// <remarks>If called on a GUI that is unloading, the event may fail to fire since
   /// the window handle may have been disposed of.</remarks>
   public static void InvokeOnUI(this Delegate subject, object[]? args)
   {
      var invocations = subject.GetInvocationList();
      foreach (var registered in invocations)
      {
         if (registered.Target is not ISynchronizeInvoke sync)
            registered.DynamicInvoke(args);
         else
            try
            {
               sync.BeginInvoke(registered, args);
            }
            catch (Exception ex)
            {
               if (ex is ObjectDisposedException or InvalidOperationException or Win32Exception)
                  Debug.WriteLine("Window is already disposed.");
            }
      }
   }
}