// ReSharper disable CommentTypo
// ReSharper disable UnusedMember.Global
#region Copyright
// <copyright company="Edgerunner.org" file="CircularBuffer.cs">
// This is a modified version of the CircularBuffer created by João Paulo dos Santos Portela
// and contributed to Tino Hager and Levi Botelho.
//
// Use and modify freely.
// </copyright>
#endregion

namespace Org.Edgerunner.Common.Buffers;

using System;
using System.Collections;
using System.Collections.Generic;

// Note from: João Paulo
// this implementation is inspired by
// http://www.boost.org/doc/libs/1_53_0/libs/circular_buffer/doc/circular_buffer.html
// because I liked their interface.

/// <inheritdoc/>
/// <summary>
/// A circular buffer.
///
/// When writing to a full buffer:
/// PushBack -> removes this[0] / Front()
/// PushFront -> removes this[Size-1] / Back()
///
/// </summary>
public class CircularBuffer<T> : IEnumerable<T?>
{
   protected readonly T?[] Buffer;

   /// <summary>
   /// The _start. Index of the first element in buffer.
   /// </summary>
   protected int _Start;

   /// <summary>
   /// The _end. Index after the last element in the buffer.
   /// </summary>
   protected int _End;

   /// <summary>
   /// The _size. Buffer size.
   /// </summary>
   protected int _Size;

   /// <summary>
   /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class.
   ///
   /// </summary>
   /// <param name='capacity'>
   /// Buffer capacity. Must be positive.
   /// </param>
   public CircularBuffer(int capacity)
       : this(capacity: capacity, items: Array.Empty<T>())
   {
   }

   /// <summary>
   /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class.
   ///
   /// </summary>
   /// <param name='capacity'>
   /// Buffer capacity. Must be positive.
   /// </param>
   /// <param name='items'>
   /// Items to fill buffer with. Items length must be less than capacity.
   /// Suggestion: use Skip(x).Take(y).ToArray() to build this argument from
   /// any enumerable.
   /// </param>
   public CircularBuffer(int capacity, T[] items)
   {
      if (capacity < 1)
      {
         throw new ArgumentException(
             "Circular buffer cannot have negative or zero capacity.", nameof(capacity));
      }
      if (items == null)
      {
         throw new ArgumentNullException(nameof(items));
      }
      if (items.Length > capacity)
      {
         throw new ArgumentException(
             "Too many items to fit circular buffer", nameof(items));
      }

      Buffer = new T[capacity];

      Array.Copy(items, Buffer, items.Length);
      _Size = items.Length;

      _Start = 0;
      _End = _Size == capacity ? 0 : _Size;
   }

   /// <summary>
   /// Maximum capacity of the buffer. Elements pushed into the buffer after
   /// maximum capacity is reached (IsFull = true), will remove an element.
   /// </summary>
   public virtual int Capacity => Buffer.Length;

   /// <summary>
   /// Boolean indicating if Circular is at full capacity.
   /// Adding more elements when the buffer is full will
   /// cause elements to be removed from the other end
   /// of the buffer.
   /// </summary>
   public virtual bool IsFull => Size == Capacity;

   /// <summary>
   /// True if has no elements.
   /// </summary>
   public virtual bool IsEmpty => Size == 0;

   /// <summary>
   /// Current buffer size (the number of elements that the buffer has).
   /// </summary>
   public virtual int Size => _Size;

   /// <summary>
   /// Element at the front of the buffer - this[0].
   /// </summary>
   /// <returns>The value of the element of type T at the front of the buffer.</returns>
   public virtual T? Front()
   {
      ThrowIfEmpty();
      return Buffer[_Start];
   }

   /// <summary>
   /// Element at the back of the buffer - this[Size - 1].
   /// </summary>
   /// <returns>The value of the element of type T at the back of the buffer.</returns>
   public virtual T? Back()
   {
      ThrowIfEmpty();
      return Buffer[(_End != 0 ? _End : Capacity) - 1];
   }

   /// <summary>
   /// Index access to elements in buffer.
   /// Index does not loop around like when adding elements,
   /// valid interval is [0;Size[
   /// </summary>
   /// <param name="index">Index of element to access.</param>
   /// <exception cref="IndexOutOfRangeException">Thrown when index is outside of [; Size[ interval.</exception>
   public virtual T? this[int index]
   {
      get
      {
         if (IsEmpty)
         {
            throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty");
         }
         if (index >= _Size)
         {
            throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer size is {_Size}");
         }
         var actualIndex = InternalIndex(index);
         return Buffer[actualIndex];
      }
      set
      {
         if (IsEmpty)
         {
            throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty");
         }
         if (index >= _Size)
         {
            throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer size is {_Size}");
         }
         var actualIndex = InternalIndex(index);
         Buffer[actualIndex] = value!;
      }
   }

   /// <summary>
   /// Pushes a new element to the back of the buffer. Back()/this[Size-1]
   /// will now return this element.
   ///
   /// When the buffer is full, the element at Front()/this[0] will be
   /// popped to allow for this new element to fit.
   /// </summary>
   /// <param name="item">Item to push to the back of the buffer</param>
   public virtual void PushBack(T? item)
   {
      if (IsFull)
      {
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
   }

   /// <summary>
   /// Pushes a new element to the front of the buffer. Front()/this[0]
   /// will now return this element.
   ///
   /// When the buffer is full, the element at Back()/this[Size-1] will be
   /// popped to allow for this new element to fit.
   /// </summary>
   /// <param name="item">Item to push to the front of the buffer</param>
   public virtual void PushFront(T? item)
   {
      if (IsFull)
      {
         Decrement(ref _Start);
         _End = _Start;
         Buffer[_Start] = item;
      }
      else
      {
         Decrement(ref _Start);
         Buffer[_Start] = item;
         ++_Size;
      }
   }

   /// <summary>
   /// Removes the element at the back of the buffer. Decreasing the
   /// Buffer size by 1.
   /// </summary>
   public virtual void PopBack()
   {
      ThrowIfEmpty("Cannot take elements from an empty buffer.");
      Decrement(ref _End);
      Buffer[_End] = default;
      --_Size;
   }

   /// <summary>
   /// Removes the element at the front of the buffer. Decreasing the
   /// Buffer size by 1.
   /// </summary>
   public virtual void PopFront()
   {
      ThrowIfEmpty("Cannot take elements from an empty buffer.");
      Buffer[_Start] = default;
      Increment(ref _Start);
      --_Size;
   }

   /// <summary>
   /// Clears the contents of the array. Size = 0, Capacity is unchanged.
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
   /// Copies the buffer contents to an array, according to the logical
   /// contents of the buffer (i.e. independent of the internal
   /// order/contents)
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

   /// <summary>
   /// Get the contents of the buffer as 2 ArraySegments.
   /// Respects the logical contents of the buffer, where
   /// each segment and items in each segment are ordered
   /// according to insertion.
   ///
   /// Fast: does not copy the array elements.
   /// Useful for methods like <c>Send(IList&lt;ArraySegment&lt;Byte&gt;&gt;)</c>.
   ///
   /// <remarks>Segments may be empty.</remarks>
   /// </summary>
   /// <returns>An IList with 2 segments corresponding to the buffer content.</returns>
   protected virtual IList<ArraySegment<T?>> ToArraySegments() => new[] { ArrayOne(), ArrayTwo() };

   #region IEnumerable<T> implementation
   /// <summary>
   /// Returns an enumerator that iterates through this buffer.
   /// </summary>
   /// <returns>An enumerator that can be used to iterate this collection.</returns>
   public virtual IEnumerator<T?> GetEnumerator()
   {
      var segments = ToArraySegments();
      foreach (var segment in segments)
         for (var i = 0; i < segment.Count; i++)
            yield return segment.Array![segment.Offset + i];
   }
   #endregion
   #region IEnumerable implementation
   IEnumerator IEnumerable.GetEnumerator()
   {
      return GetEnumerator();
   }
   #endregion

   protected virtual void ThrowIfEmpty(string message = "Cannot access an empty buffer.")
   {
      if (IsEmpty)
      {
         throw new InvalidOperationException(message);
      }
   }

   /// <summary>
   /// Increments the provided index variable by one, wrapping
   /// around if necessary.
   /// </summary>
   /// <param name="index"></param>
   protected virtual void Increment(ref int index)
   {
      if (++index == Capacity)
      {
         index = 0;
      }
   }

   /// <summary>
   /// Decrements the provided index variable by one, wrapping
   /// around if necessary.
   /// </summary>
   /// <param name="index"></param>
   protected virtual void Decrement(ref int index)
   {
      if (index == 0)
      {
         index = Capacity;
      }
      index--;
   }

   /// <summary>
   /// Converts the index in the argument to an index in <code>_buffer</code>
   /// </summary>
   /// <returns>
   /// The transformed index.
   /// </returns>
   /// <param name='index'>
   /// External index.
   /// </param>
   protected virtual int InternalIndex(int index)
   {
      return _Start + (index < (Capacity - _Start) ? index : index - Capacity);
   }

   // doing ArrayOne and ArrayTwo methods returning ArraySegment<T> as seen here:
   // http://www.boost.org/doc/libs/1_37_0/libs/circular_buffer/doc/circular_buffer.html#classboost_1_1circular__buffer_1957cccdcb0c4ef7d80a34a990065818d
   // http://www.boost.org/doc/libs/1_37_0/libs/circular_buffer/doc/circular_buffer.html#classboost_1_1circular__buffer_1f5081a54afbc2dfc1a7fb20329df7d5b
   // should help a lot with the code.

   #region Array items easy access.
   // The array is composed by at most two non-contiguous segments,
   // the next two methods allow easy access to those.

   protected virtual ArraySegment<T?> ArrayOne()
   {
      if (IsEmpty)
         return new ArraySegment<T?>(Array.Empty<T>());

      return new ArraySegment<T?>(Buffer, _Start, (_Start < _End) ? _End - _Start : Buffer.Length - _Start);
   }

   protected virtual ArraySegment<T?> ArrayTwo()
   {
      if (IsEmpty)
         return new ArraySegment<T?>(Array.Empty<T>());

      return _Start < _End ? new ArraySegment<T?>(Buffer, _End, 0) : new ArraySegment<T?>(Buffer, 0, _End);
   }
   #endregion
}