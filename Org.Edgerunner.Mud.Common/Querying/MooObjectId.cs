#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="MooObjectId.cs">
// Copyright (c) Thaddeus Ryker 2026
// </copyright>
//
// BSD 3-Clause License
//
// Copyright (c) 2026,
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

namespace Org.Edgerunner.Mud.Common.Querying;

/// <summary>
/// A readonly value type representing a MOO object number (for example <c>#123</c>).
/// </summary>
public readonly struct MooObjectId : IEquatable<MooObjectId>
{
   /// <summary>
   /// Initializes a new instance of the <see cref="MooObjectId"/> struct.
   /// </summary>
   /// <param name="number">The MOO object number.</param>
   public MooObjectId(int number)
   {
      Number = number;
   }

   /// <summary>
   /// Gets the MOO object number.
   /// </summary>
   /// <value>The object number.</value>
   public int Number { get; }

   /// <summary>
   /// Gets the MOO <c>$nothing</c> object id (<c>#-1</c>).
   /// </summary>
   public static MooObjectId Nothing => new(-1);

   /// <summary>
   /// Gets the MOO ambiguous match result id (<c>#-2</c>).
   /// </summary>
   public static MooObjectId AmbiguousMatch => new(-2);

   /// <summary>
   /// Gets the MOO failed match result id (<c>#-3</c>).
   /// </summary>
   public static MooObjectId FailedMatch => new(-3);

   /// <inheritdoc/>
   public bool Equals(MooObjectId other)
   {
      return Number == other.Number;
   }

   /// <inheritdoc/>
   public override bool Equals(object? obj)
   {
      return obj is MooObjectId other && Equals(other);
   }

   /// <inheritdoc/>
   public override int GetHashCode()
   {
      return Number;
   }

   /// <summary>
   /// Determines whether two <see cref="MooObjectId"/> values are equal.
   /// </summary>
   /// <param name="left">The left value.</param>
   /// <param name="right">The right value.</param>
   /// <returns><c>true</c> if the values are equal; otherwise, <c>false</c>.</returns>
   public static bool operator ==(MooObjectId left, MooObjectId right)
   {
      return left.Equals(right);
   }

   /// <summary>
   /// Determines whether two <see cref="MooObjectId"/> values are not equal.
   /// </summary>
   /// <param name="left">The left value.</param>
   /// <param name="right">The right value.</param>
   /// <returns><c>true</c> if the values are not equal; otherwise, <c>false</c>.</returns>
   public static bool operator !=(MooObjectId left, MooObjectId right)
   {
      return !left.Equals(right);
   }

   /// <summary>
   /// Returns the MOO textual representation of the object id (for example <c>#123</c>).
   /// </summary>
   /// <returns>A <see cref="string"/> in the form <c>#n</c>.</returns>
   public override string ToString()
   {
      return $"#{Number}";
   }
}
