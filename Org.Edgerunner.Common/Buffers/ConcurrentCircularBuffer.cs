#region BSD 3-Clause License

// <copyright company="Edgerunner.org" file="ConcurrentCircularBuffer.cs">
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

namespace Org.Edgerunner.Common.Buffers;

/// <summary>
///    A concurrent circular buffer class
/// </summary>
/// <typeparam name="T">The type that the buffer will contain</typeparam>
/// <seealso cref="Org.Edgerunner.Common.Buffers.CircularBuffer&lt;T&gt;" />
public class ConcurrentCircularBuffer<T> : CircularBuffer<T?>
{
   protected readonly object SyncLock = new();

   #region Constructors And Finalizers

   /// <summary>
   /// Initializes a new instance of the <see cref="ConcurrentCircularBuffer{T}"/> class.
   /// </summary>
   /// <param name="capacity">Buffer capacity. Must be positive.</param>
   public ConcurrentCircularBuffer(int capacity)
      : base(capacity)
   {
   }

   /// <summary>
   /// Initializes a new instance of the <see cref="ConcurrentCircularBuffer{T}"/> class.
   /// </summary>
   /// <param name="capacity">Buffer capacity. Must be positive.</param>
   /// <param name="items">Items to fill buffer with. Items length must be less than capacity.
   /// Suggestion: use Skip(x).Take(y).ToArray() to build this argument from
   /// any enumerable.</param>
   public ConcurrentCircularBuffer(int capacity, T[] items)
      : base(capacity, items)
   {
   }

   #endregion

   /// <inheritdoc />
   public override bool IsEmpty
   {
      get
      {
         lock (SyncLock)
            return base.IsEmpty;
      }
   }

   /// <inheritdoc />
   public override bool IsFull
   {
      get
      {
         lock (SyncLock)
            return base.IsFull;
      }
   }

   /// <inheritdoc />
   public override T? this[int index]
   {
      get
      {
         lock (SyncLock)
            return base[index];
      }
      set
      {
         lock (SyncLock)
            base[index] = value;
      }
   }

   /// <inheritdoc />
   public override int Size
   {
      get
      {
         lock (SyncLock)
            return base.Size;
      }
   }

   /// <inheritdoc />
   public override T? Back()
   {
      lock (SyncLock)
         return base.Back();
   }

   /// <inheritdoc />
   public override void Clear()
   {
      lock (SyncLock)
         base.Clear();
   }

   /// <inheritdoc />
   public override T? Front()
   {
      lock (SyncLock)
         return base.Front();
   }

   /// <inheritdoc />
   public override IEnumerator<T?> GetEnumerator()
   {
      lock (SyncLock)
      {
         var segments = ToArraySegments();
         foreach (var segment in segments)
            for (var i = 0; i < segment.Count; i++)
               yield return segment.Array![segment.Offset + i];
      }
   }

   /// <inheritdoc />
   public override T? PopBack()
   {
      lock (SyncLock)
         return base.PopBack();
   }

   /// <inheritdoc />
   public override T? PopFront()
   {
      lock (SyncLock)
         return base.PopFront();
   }

   /// <inheritdoc />
   public override T? PushBack(T? item)
   {
      lock (SyncLock)
         return base.PushBack(item);
   }

   /// <inheritdoc />
   public override T? PushFront(T? item)
   {
      lock (SyncLock)
         return base.PushFront(item);
   }

   /// <inheritdoc />
   public override T?[] ToArray()
   {
      lock (SyncLock)
         return base.ToArray();
   }
}