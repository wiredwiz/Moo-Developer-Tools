#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="QueryModels.cs">
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
/// Summary information describing a MOO object.
/// </summary>
/// <param name="Id">The object id.</param>
/// <param name="Name">The object name.</param>
/// <param name="Aliases">The object aliases.</param>
public record MooObjectSummary(MooObjectId Id, string Name, IReadOnlyList<string> Aliases);

/// <summary>
/// Summary information describing a MOO verb.
/// </summary>
/// <param name="Aliases">The verb names/aliases.</param>
/// <param name="DefiningObject">The object on which the verb is defined.</param>
public record MooVerbSummary(IReadOnlyList<string> Aliases, MooObjectId DefiningObject);

/// <summary>
/// Summary information describing a MOO property.
/// </summary>
/// <param name="Name">The property name.</param>
/// <param name="DefiningObject">The object on which the property is defined.</param>
public record MooPropertySummary(string Name, MooObjectId DefiningObject);

/// <summary>
/// Detailed information describing a MOO verb.
/// </summary>
/// <param name="QueriedObjectId">The object that was queried for the verb.</param>
/// <param name="ResolvedObjectId">The object on which the verb actually lives (the defining object, which may be an ancestor of the queried object when the verb is inherited).</param>
/// <param name="Aliases">The verb names/aliases.</param>
/// <param name="Owner">The verb owner.</param>
/// <param name="Permissions">The verb permission flags.</param>
/// <param name="Args">The verb argument specification.</param>
public record MooVerbInfo(
   MooObjectId QueriedObjectId,
   MooObjectId ResolvedObjectId,
   IReadOnlyList<string> Aliases,
   MooObjectId Owner,
   VerbPermission Permissions,
   VerbArgs Args);

/// <summary>
/// Documentation lines for a MOO verb, together with the queried and resolved objects.
/// </summary>
/// <param name="QueriedObjectId">The object that was queried for the verb.</param>
/// <param name="ResolvedObjectId">The object on which the verb actually lives (the defining object, which may be an ancestor of the queried object when the verb is inherited).</param>
/// <param name="Lines">The documentation lines.</param>
public record MooVerbDocumentation(MooObjectId QueriedObjectId, MooObjectId ResolvedObjectId, IReadOnlyList<string> Lines);

/// <summary>
/// Source code lines for a MOO verb, together with the queried and resolved objects.
/// </summary>
/// <param name="QueriedObjectId">The object that was queried for the verb.</param>
/// <param name="ResolvedObjectId">The object on which the verb actually lives (the defining object, which may be an ancestor of the queried object when the verb is inherited).</param>
/// <param name="Lines">The source code lines.</param>
public record MooVerbCode(MooObjectId QueriedObjectId, MooObjectId ResolvedObjectId, IReadOnlyList<string> Lines);

/// <summary>
/// Detailed information describing a MOO property.
/// </summary>
/// <param name="Name">The property name.</param>
/// <param name="Owner">The property owner.</param>
/// <param name="Permissions">The property permission flags.</param>
/// <param name="DefiningObject">The object on which the property is defined.</param>
/// <param name="ValueType">The MOO value type code of the property value.</param>
/// <param name="ValuePreview">A textual preview of the property value.</param>
public record MooPropertyInfo(
   string Name,
   MooObjectId Owner,
   PropertyPermission Permissions,
   MooObjectId DefiningObject,
   int ValueType,
   string ValuePreview);

/// <summary>
/// Represents the literal value of a MOO property.
/// </summary>
/// <param name="Type">The MOO value type code.</param>
/// <param name="Literal">The MOO literal representation of the value.</param>
public record MooPropertyValue(int Type, string Literal);
