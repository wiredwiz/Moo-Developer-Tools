// ReSharper disable CommentTypo
// ReSharper disable UnusedMember.Global
#region BSD 3-Clause License

// <copyright company="Edgerunner.org" file="ConcurrentCircularBuffer.cs">
// Copyright (c)  2022
// </copyright>
//
// This my modified .Net 6 version of the CircularBuffer that was created by
// João Paulo dos Santos Portela and contributed to by Tino Hager and Levi Botelho.
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

using System.Collections;

namespace Org.Edgerunner.Common.Buffers;

using System;

// Note from: João Paulo
// this implementation is inspired by
// http://www.boost.org/doc/libs/1_53_0/libs/circular_buffer/doc/circular_buffer.html
// because I liked their interface.

/// <inheritdoc />
/// <summary>
///    A circular buffer.
/// </summary>
/// <remarks>
///    When writing to a full buffer:
///    PushBack -> removes this[0] / Front()
///    PushFront -> removes this[Size-1] / Back()
/// </remarks>
public class CircularBuffer<T> : IEnumerable<T?>
{
   protected readonly T?[] Buffer;

   /// <summary>
   ///    The _end. Index after the last element in the buffer.
   /// </summary>
   protected int _End;

   /// <summary>
   ///    The _size. Buffer size.
   /// </summary>
   protected int _Size;

   /// <summary>
   ///    The _start. Index of the first element in buffer.
   /// </summary>
   protected int _Start;

   #region Constructors And Finalizers

   /// <summary>
   ///    Initializes a new instance of the <see cref="CircularBuffer{T}" /> class.
   /// </summary>
   /// <param name="capacity">The capacity to use for the buffer.</param>
   public CircularBuffer(int capacity)
      : this(capacity, Array.Empty<T>())
   {
   }

   /// <summary>
   /// Initializes a new instance of the <see cref="CircularBuffer{T}" /> class.
   /// </summary>
   /// <param name="capacity">The capacity to use for the buffer.</param>
   /// <param name="items">Items to fill buffer with.</param>
   /// <exception cref="System.ArgumentNullException">items is null.</exception>
   /// <exception cref="System.ArgumentException">items size is larger than the buffer capacity.</exception>
   /// <exception cref="System.ArgumentOutOfRangeException">capacity is a non-positive number.</exception>
   public CircularBuffer(int capacity, T[] items)
   {
      if (capacity < 1)
         throw new ArgumentException(
                                     "Circular buffer cannot have negative or zero capacity.",
                                     nameof(capacity));
      if (items == null) throw new ArgumentNullException(nameof(items));
      if (items.Length > capacity)
         throw new ArgumentException(
                                     "Too many items to fit circular buffer",
                                     nameof(items));

      Buffer = new T[capacity];

      Array.Copy(items, Buffer, items.Length);
      _Size = items.Length;

      _Start = 0;
      _End = _Size == capacity ? 0 : _Size;
   }

   #endregion

   /// <summary>
   ///    Maximum capacity of the buffer. Elements pushed into the buffer after
   ///    maximum capacity is reached (IsFull = true), will remove an element.
   /// </summary>
   public virtual int Capacity => Buffer.Length;

   /// <summary>
   ///    True if has no elements.
   /// </summary>
   public virtual bool IsEmpty => Size == 0;

   /// <summary>
   ///    Boolean indicating if Circular is at full capacity.
   ///    Adding more elements when the buffer is full will
   ///    cause elements to be removed from the other end
   ///    of the buffer.
   /// </summary>
   public virtual bool IsFull => Size == Capacity;


   /// <summary>
   /// Gets or sets the <see cref="System.Nullable{T}"/> at the specified index.
   /// </summary>
   /// <value>
   /// The <see cref="System.Nullable{T}"/>.
   /// </value>
   /// <param name="index">The index to access.</param>
   /// <returns></returns>
   /// <exception cref="System.IndexOutOfRangeException">
   /// Buffer is empty
   /// or
   /// Index is greater than buffer size.
   /// </exception>
   public virtual T? this[int index]
   {
      get
      {
         if (IsEmpty) throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty");
         if (index >= _Size) throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer size is {_Size}");
         var actualIndex = InternalIndex(index);
         return Buffer[actualIndex];
      }
      set
      {
         if (IsEmpty) throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty");
         if (index >= _Size) throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer size is {_Size}");
         var actualIndex = InternalIndex(index);
         Buffer[actualIndex] = value!;
      }
   }

   /// <summary>
   ///    Current buffer size (the number of elements that the buffer has).
   /// </summary>
   public virtual int Size => _Size;

   #region IEnumerable<T?> Members

   /// <summary>
   /// Returns an enumerator that iterates through a collection.
   /// </summary>
   /// <returns>
   /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
   /// </returns>
   IEnumerator IEnumerable.GetEnumerator()
   {
      return GetEnumerator();
   }


   /// <summary>
   /// Returns an enumerator that iterates through the collection.
   /// </summary>
   /// <returns>
   /// An enumerator that can be used to iterate through the collection.
   /// </returns>
   public virtual IEnumerator<T?> GetEnumerator()
   {
      var segments = ToArraySegments();
      foreach (var segment in segments)
         for (var i = 0; i < segment.Count; i++)
            yield return segment.Array![segment.Offset + i];
   }

   #endregion

   /// <summary>
   ///    Element at the back of the buffer - this[Size - 1].
   /// </summary>
   /// <returns>The value of the element of type T at the back of the buffer.</returns>
   public virtual T? Back()
   {
      ThrowIfEmpty();
      return Buffer[(_End != 0 ? _End : Capacity) - 1];
   }

   /// <summary>
   ///    Clears the contents of the array. Size = 0, Capacity is unchanged.
   /// </summary>
   /// <exception cref="NotImplementedException"></exception>
   public virtual void Clear()
   {
      // to clear we just reset everything.
      _Start = 0;
      _End = 0;
      _Size = 0;
      Array.Clear(Buffer, 0, Buffer.Length);
   }

   /// <summary>
   ///    Element at the front of the buffer - this[0].
   /// </summary>
   /// <returns>The value of the element of type T at the front of the buffer.</returns>
   public virtual T? Front()
   {
      ThrowIfEmpty();
      return Buffer[_Start];
   }

   /// <summary>
   /// Removes the element at the back of the buffer.
   /// </summary>
   /// <returns>The removed element value.</returns>
   /// <remarks>Decreases the Buffer size by 1.</remarks>
   public virtual T? PopBack()
   {
      ThrowIfEmpty("Cannot take elements from an empty buffer.");
      Decrement(ref _End);
      var current = Buffer[_End];
      Buffer[_End] = default;
      --_Size;
      return current;
   }

   /// <summary>
   /// Removes the element at the front of the buffer.
   /// </summary>
   /// <returns>The removed element value.</returns>
   /// <remarks>Decreases the Buffer size by 1.</remarks>
   public virtual T? PopFront()
   {
      ThrowIfEmpty("Cannot take elements from an empty buffer.");
      var current = Buffer[_Start];
      Buffer[_Start] = default;
      Increment(ref _Start);
      --_Size;
      return current;
   }

   /// <summary>
   /// Pushes a new element to the back of the buffer.
   /// </summary>
   /// <param name="item">Item to push to the back of the buffer</param>
   /// <returns>The popped instance of <see cref="T"/> if the buffer is full; <c>null</c> otherwise.</returns>
   /// <remarks>
   /// Back()/this[Size-1] will now return this element.
   /// When the buffer is full, the element at Front()/this[0] will be
   /// popped to allow for this new element to fit.
   /// </remarks>
   public virtual T? PushBack(T? item)
   {
      T? popped = default;
      if (IsFull)
      {
         popped = Buffer[_End];
         Buffer[_End] = item;
         Increment(ref _End);
         _Start = _End;
      }
      else
      {
         Buffer[_End] = item;
         Increment(ref _End);
         ++_Size;
      }

      return popped;
   }

   /// <summary>
   /// Pushes a new element to the front of the buffer.
   /// </summary>
   /// <param name="item">Item to push to the front of the buffer</param>
   /// <returns>The popped instance of <see cref="T"/> if the buffer is full; <c>null</c> otherwise.</returns>
   /// <remarks>
   /// Front()/this[0]
   /// will now return this element.
   /// When the buffer is full, the element at Back()/this[Size-1] will be
   /// popped to allow for this new element to fit.
   /// </remarks>
   public virtual T? PushFront(T? item)
   {
      T? popped = default;
      if (IsFull)
      {
         Decrement(ref _Start);
         _End = _Start;
         popped = Buffer[_Start];
         Buffer[_Start] = item;
      }
      else
      {
         Decrement(ref _Start);
         Buffer[_Start] = item;
         ++_Size;
      }

      return popped;
   }

   /// <summary>
   ///    Copies the buffer contents to an array, according to the logical
   ///    contents of the buffer (i.e. independent of the internal
   ///    order/contents)
   /// </summary>
   /// <returns>A new array with a copy of the buffer contents.</returns>
   public virtual T?[] ToArray()
   {
      var newArray = new T?[Size];
      var newArrayOffset = 0;
      var segments = ToArraySegments();
      foreach (var segment in segments)
      {
         Array.Copy(segment.Array!, segment.Offset, newArray, newArrayOffset, segment.Count);
         newArrayOffset += segment.Count;
      }

      return newArray;
   }

   #region Buffer Access Helpers

   // doing ArrayOne and ArrayTwo methods returning ArraySegment<T> as seen here:
   // http://www.boost.org/doc/libs/1_37_0/libs/circular_buffer/doc/circular_buffer.html#classboost_1_1circular__buffer_1957cccdcb0c4ef7d80a34a990065818d
   // http://www.boost.org/doc/libs/1_37_0/libs/circular_buffer/doc/circular_buffer.html#classboost_1_1circular__buffer_1f5081a54afbc2dfc1a7fb20329df7d5b
   // should help a lot with the code.

   // The array is composed by at most two non-contiguous segments,
   // the next two methods allow easy access to those.

   /// <summary>
   ///   Gets the first array data chunk.
   /// </summary>
   /// <returns>An <see cref="ArraySegment{T}"/> with the first data chunk.</returns>
   protected virtual ArraySegment<T?> ArrayOne()
   {
      if (IsEmpty)
         return new ArraySegment<T?>(Array.Empty<T>());

      return new ArraySegment<T?>(Buffer, _Start, _Start < _End ? _End - _Start : Buffer.Length - _Start);
   }

   /// <summary>
   /// Gets the second array data chunk.
   /// </summary>
   /// <returns>An <see cref="ArraySegment{T}"/> with the second data chunk.</returns>
   /// <remarks>This chunk might be empty if there is no data or if all the data was in the first chunk.</remarks>
   protected virtual ArraySegment<T?> ArrayTwo()
   {
      if (IsEmpty)
         return new ArraySegment<T?>(Array.Empty<T>());

      return _Start < _End ? new ArraySegment<T?>(Buffer, _End, 0) : new ArraySegment<T?>(Buffer, 0, _End);
   }

   #endregion

   /// <summary>
   ///    Decrements the provided index variable by one, wrapping
   ///    around if necessary.
   /// </summary>
   /// <param name="index"></param>
   protected virtual void Decrement(ref int index)
   {
      if (index == 0) index = Capacity;
      index--;
   }

   /// <summary>
   ///    Increments the provided index variable by one, wrapping
   ///    around if necessary.
   /// </summary>
   /// <param name="index"></param>
   protected virtual void Increment(ref int index)
   {
      if (++index == Capacity) index = 0;
   }

   /// <summary>
   ///    Converts the index in the argument to an index in <code>_buffer</code>
   /// </summary>
   /// <returns>
   ///    The transformed index.
   /// </returns>
   /// <param name='index'>
   ///    External index.
   /// </param>
   protected virtual int InternalIndex(int index)
   {
      return _Start + (index < Capacity - _Start ? index : index - Capacity);
   }

   /// <summary>
   /// Throws an <see cref="InvalidOperationException"/> if the buffer is empty.
   /// </summary>
   /// <param name="message">The message to throw.</param>
   /// <exception cref="System.InvalidOperationException">Thrown if the buffer is empty.</exception>
   protected virtual void ThrowIfEmpty(string message = "Cannot access an empty buffer.")
   {
      if (IsEmpty) throw new InvalidOperationException(message);
   }

   /// <summary>
   ///    Get the contents of the buffer as 2 ArraySegments.
   ///    Respects the logical contents of the buffer, where
   ///    each segment and items in each segment are ordered
   ///    according to insertion.
   ///    Fast: does not copy the array elements.
   ///    Useful for methods like <c>Send(IList&lt;ArraySegment&lt;Byte&gt;&gt;)</c>.
   ///    <remarks>Segments may be empty.</remarks>
   /// </summary>
   /// <returns>An IList with 2 segments corresponding to the buffer content.</returns>
   protected virtual IList<ArraySegment<T?>> ToArraySegments()
   {
      return new[] { ArrayOne(), ArrayTwo() };
   }
}