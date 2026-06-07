#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="VerbArgumentEnums.cs">
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
/// Specifies the MOO verb direct object argument specifier.
/// </summary>
public enum DirectObject
{
   /// <summary>
   /// The verb's direct object must be the object that defines the verb (<c>this</c>).
   /// </summary>
   This,

   /// <summary>
   /// The verb takes no direct object (<c>none</c>).
   /// </summary>
   None,

   /// <summary>
   /// The verb's direct object may be anything (<c>any</c>).
   /// </summary>
   Any,
}

/// <summary>
/// Specifies the MOO verb indirect object argument specifier.
/// </summary>
public enum IndirectObject
{
   /// <summary>
   /// The verb's indirect object must be the object that defines the verb (<c>this</c>).
   /// </summary>
   This,

   /// <summary>
   /// The verb takes no indirect object (<c>none</c>).
   /// </summary>
   None,

   /// <summary>
   /// The verb's indirect object may be anything (<c>any</c>).
   /// </summary>
   Any,
}

/// <summary>
/// Specifies the MOO verb preposition argument specifier.
/// </summary>
public enum Preposition
{
   /// <summary>
   /// The verb takes no preposition (<c>none</c>).
   /// </summary>
   None,

   /// <summary>
   /// The verb matches any preposition (<c>any</c>).
   /// </summary>
   Any,

   /// <summary>
   /// Aliases: <c>with</c>, <c>using</c>.
   /// </summary>
   With,

   /// <summary>
   /// Aliases: <c>at</c>, <c>to</c>.
   /// </summary>
   At,

   /// <summary>
   /// Aliases: <c>in front of</c>.
   /// </summary>
   InFrontOf,

   /// <summary>
   /// Aliases: <c>in</c>, <c>inside</c>, <c>into</c>.
   /// </summary>
   In,

   /// <summary>
   /// Aliases: <c>on top of</c>, <c>on</c>, <c>onto</c>, <c>upon</c>.
   /// </summary>
   OnTopOf,

   /// <summary>
   /// Aliases: <c>out of</c>, <c>from inside</c>, <c>from</c>.
   /// </summary>
   OutOf,

   /// <summary>
   /// Aliases: <c>over</c>.
   /// </summary>
   Over,

   /// <summary>
   /// Aliases: <c>through</c>.
   /// </summary>
   Through,

   /// <summary>
   /// Aliases: <c>under</c>, <c>underneath</c>, <c>beneath</c>.
   /// </summary>
   Under,

   /// <summary>
   /// Aliases: <c>behind</c>.
   /// </summary>
   Behind,

   /// <summary>
   /// Aliases: <c>beside</c>.
   /// </summary>
   Beside,

   /// <summary>
   /// Aliases: <c>for</c>, <c>about</c>.
   /// </summary>
   For,

   /// <summary>
   /// Aliases: <c>is</c>.
   /// </summary>
   Is,

   /// <summary>
   /// Aliases: <c>as</c>.
   /// </summary>
   As,

   /// <summary>
   /// Aliases: <c>off</c>, <c>off of</c>.
   /// </summary>
   Off,
}
