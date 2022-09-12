#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="CommandBuffer.cs">
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

using Org.Edgerunner.Common.Buffers;

namespace Org.Edgerunner.Mud.Communication.Buffers;

/// <summary>
/// A buffer for storing a list of previous commands.
/// </summary>
/// <seealso cref="Org.Edgerunner.Common.Buffers.CircularBuffer&lt;System.String&gt;" />
public class CommandBuffer : CircularBuffer<string?>
{
   private int _CurrentPosition;

   /// <summary>
   /// Initializes a new instance of the <see cref="CommandBuffer"/> class.
   /// </summary>
   /// <param name="capacity">The capacity to use for the buffer.</param>
   public CommandBuffer(int capacity)
      : base(capacity)
   {
   }

   /// <summary>
   /// Initializes a new instance of the <see cref="CommandBuffer"/> class.
   /// </summary>
   /// <param name="capacity">The capacity to use for the buffer.</param>
   /// <param name="items">Items to fill buffer with.</param>
   public CommandBuffer(int capacity, string[] items)
      : base(capacity, items)
   {
      _CurrentPosition = 0;
   }

   /// <summary>
   /// Gets or sets the current buffer position.
   /// </summary>
   /// <value>
   /// The current position.
   /// </value>
   /// <exception cref="System.ArgumentOutOfRangeException">value</exception>
   public virtual int CurrentPosition
   {
      get => _CurrentPosition;
      set
      {
         if (value >= Size)
            throw new ArgumentOutOfRangeException(nameof(value));

         _CurrentPosition = value;
      }
   }

   /// <summary>
   /// Moves the back in buffer.
   /// </summary>
   /// <param name="spaces">The number of spaces to move.</param>
   /// <returns>The value in the buffer at the new position.</returns>
   /// <remarks>If you attempt to move past the end of the buffer
   /// it simply stops at the end without notice.</remarks>
   public virtual string? MoveBackInBuffer(int spaces = 1)
   {
      if (_CurrentPosition + spaces >= Size)
         _CurrentPosition = Size - 1;
      else
         _CurrentPosition += spaces;

      return Size == 0 ? null : this[_CurrentPosition];
   }

   /// <summary>
   /// Moves the forward in buffer.
   /// </summary>
   /// <param name="spaces">The number of spaces to move.</param>
   /// <returns>The value in the buffer at the new position.</returns>
   /// <remarks>If you attempt to move past the start of the buffer
   /// it simply stops at the start without notice.</remarks>
   public virtual string? MoveForwardInBuffer(int spaces = 1)
   {
      if (_CurrentPosition - spaces < 0)
         _CurrentPosition = 0;
      else
         _CurrentPosition -= spaces;

      return Size == 0 ? null : this[_CurrentPosition];
   }
}