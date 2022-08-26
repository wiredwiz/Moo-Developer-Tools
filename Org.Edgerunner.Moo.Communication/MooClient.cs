#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="MooClient.cs">
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

using System.Net.Sockets;
using Org.Edgerunner.Moo.Communication.Interfaces;

namespace Org.Edgerunner.Moo.Communication;

public static class MooClient
{
   /// <summary>
   /// Creates a session for the specified Moo world.
   /// </summary>
   /// <typeparam name="T">The IMooClientSession type to use.</typeparam>
   /// <param name="world">The Moo world name.</param>
   /// <param name="host">The Moo host address.</param>
   /// <param name="port">The Moo port address.</param>
   /// <param name="outOfBandPrefix">The out of band prefix.</param>
   /// <returns>
   /// A new client session.
   /// </returns>
   /// <exception cref="System.ArgumentNullException">world or host are null; or port in a non-positive integer</exception>
   /// <exception cref="System.ArgumentOutOfRangeException">port</exception>
   public static IMooClientSession Create<T>(string world, string host, int port, string outOfBandPrefix = "#$#") where T : IMooClientSession
   {
      if (world == null) throw new ArgumentNullException(nameof(world));
      if (host == null) throw new ArgumentNullException(nameof(host));
      if (port <= 0) throw new ArgumentOutOfRangeException(nameof(port));

      return (Activator.CreateInstance(typeof(T), world, host, port, outOfBandPrefix) as IMooClientSession)!;
   }
}