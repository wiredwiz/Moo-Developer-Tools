#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="CommunicationBuffer.cs">
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

using System.Text;
using Org.Edgerunner.Common.Buffers;

namespace Org.Edgerunner.Moo.Communication.Buffers;

/// <summary>
/// A class that represents a moo communication byte buffer
/// </summary>
/// <seealso cref="byte" />
public class CommunicationBuffer : ConcurrentCircularBuffer<byte>
{
   /// <summary>
   /// Initializes a new instance of the <see cref="CommunicationBuffer"/> class.
   /// </summary>
   /// <param name="capacity">Buffer capacity. Must be positive.</param>
   public CommunicationBuffer(int capacity)
      : base(capacity)
   {
   }

   /// <summary>
   /// Initializes a new instance of the <see cref="CommunicationBuffer" /> class.
   /// </summary>
   /// <param name="capacity">The capacity to use for the buffer.</param>
   /// <param name="items">Items to fill buffer with.</param>
   /// <exception cref="System.ArgumentNullException">items is null.</exception>
   /// <exception cref="System.ArgumentException">items size is larger than the buffer capacity.</exception>
   /// <exception cref="System.ArgumentOutOfRangeException">capacity is a non-positive number.</exception>
   public CommunicationBuffer(int capacity, byte[] items)
      : base(capacity, items)
   {
   }

   public virtual string? PopTextLine(Decoder decoder)
   {
      byte[] buffer;
      int position;
      var endOfLine = -1;

      lock (SyncLock)
      {
         if (IsEmpty)
            return null;

         for (int i = 0; i < Size; i++)
         {
            if (this[i] == '\n')
            {
               endOfLine = i;
               break;
            }
         }

         if (endOfLine == -1)
            return null;

         buffer = new byte[endOfLine];
         position = 0;

         while (position < endOfLine)
         {
            buffer[position] = PopFront();
            position++;
         }

         PopFront();
      }

      var chars = new char[decoder.GetCharCount(buffer, 0, position)];
      decoder.GetChars(buffer, 0, endOfLine, chars, 0);
      return new string(chars);
   }
}