#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="IMooWorldQueryProvider.cs">
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
/// Protocol-agnostic mechanism for querying a connected MOO world for developer/introspection
/// information (objects, hierarchy, verbs, properties, verb code, property values).
/// </summary>
/// <remarks>
/// Concrete providers (for example an SDWC out-of-band handler or an MCP dev-info package) implement
/// this interface and register themselves per connection. A provider that does not support a given
/// operation should throw <see cref="NotImplementedException"/> so that the routing aggregate can fall
/// through to the next provider.
/// </remarks>
public interface IMooWorldQueryProvider
{
   /// <summary>
   /// Gets the collection of known objects in the world.
   /// </summary>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>A read-only list of <see cref="MooObjectSummary"/>.</returns>
   Task<IReadOnlyList<MooObjectSummary>> GetObjectsAsync(CancellationToken cancellationToken);

   /// <summary>
   /// Gets the immediate children of the specified object.
   /// </summary>
   /// <param name="objectId">The object id.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>A read-only list of <see cref="MooObjectSummary"/>.</returns>
   Task<IReadOnlyList<MooObjectSummary>> GetChildrenAsync(MooObjectId objectId, CancellationToken cancellationToken);

   /// <summary>
   /// Gets the objects owned by the current player connection.
   /// </summary>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>A read-only list of <see cref="MooObjectSummary"/> owned by the current player.</returns>
   Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(CancellationToken cancellationToken);

   /// <summary>
   /// Gets the objects owned by the specified owner.
   /// </summary>
   /// <param name="owner">The owner object id whose owned objects are requested.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>A read-only list of <see cref="MooObjectSummary"/> owned by <paramref name="owner"/>.</returns>
   Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(MooObjectId owner, CancellationToken cancellationToken);

   /// <summary>
   /// Gets the parent of the specified object.
   /// </summary>
   /// <param name="objectId">The object id.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>The parent <see cref="MooObjectId"/>, or <c>null</c> if the object has no parent (<c>#-1</c>).</returns>
   Task<MooObjectId?> GetParentAsync(MooObjectId objectId, CancellationToken cancellationToken);

   /// <summary>
   /// Gets the verbs defined on the specified object.
   /// </summary>
   /// <param name="objectId">The object id.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>A read-only list of <see cref="MooVerbSummary"/>.</returns>
   Task<IReadOnlyList<MooVerbSummary>> GetVerbsAsync(MooObjectId objectId, CancellationToken cancellationToken);

   /// <summary>
   /// Gets the documentation lines for the specified verb.
   /// </summary>
   /// <param name="objectId">The object id queried for the verb.</param>
   /// <param name="verbName">The verb name.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>
   /// A <see cref="MooVerbDocumentation"/> carrying the queried object id, the resolved (defining) object
   /// id the verb actually lives on, and the documentation lines; or <c>null</c> if the verb was not found
   /// or the operation is unsupported.
   /// </returns>
   Task<MooVerbDocumentation?> GetVerbDocumentationAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken);

   /// <summary>
   /// Gets the properties defined on the specified object.
   /// </summary>
   /// <param name="objectId">The object id.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>A read-only list of <see cref="MooPropertySummary"/>.</returns>
   Task<IReadOnlyList<MooPropertySummary>> GetPropertiesAsync(MooObjectId objectId, CancellationToken cancellationToken);

   /// <summary>
   /// Gets detailed information about the specified verb.
   /// </summary>
   /// <param name="objectId">The object id queried for the verb.</param>
   /// <param name="verbName">The verb name.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>
   /// A <see cref="MooVerbInfo"/> carrying both the queried object id and the resolved (defining) object
   /// id the verb actually lives on; or <c>null</c> if the verb was not found or the operation is unsupported.
   /// </returns>
   Task<MooVerbInfo?> GetVerbInfoAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken);

   /// <summary>
   /// Gets detailed information about the specified property.
   /// </summary>
   /// <param name="objectId">The object id.</param>
   /// <param name="propName">The property name.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>A <see cref="MooPropertyInfo"/>, or <c>null</c> if the property was not found.</returns>
   Task<MooPropertyInfo?> GetPropertyInfoAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken);

   /// <summary>
   /// Gets the source code lines of the specified verb.
   /// </summary>
   /// <param name="objectId">The object id queried for the verb.</param>
   /// <param name="verbName">The verb name.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>
   /// A <see cref="MooVerbCode"/> carrying the queried object id, the resolved (defining) object id the
   /// verb actually lives on, and the source code lines; or <c>null</c> if the verb was not found or the
   /// operation is unsupported.
   /// </returns>
   Task<MooVerbCode?> GetVerbCodeAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken);

   /// <summary>
   /// Gets the value of the specified property.
   /// </summary>
   /// <param name="objectId">The object id.</param>
   /// <param name="propName">The property name.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>A <see cref="MooPropertyValue"/>, or <c>null</c> if the property was not found.</returns>
   Task<MooPropertyValue?> GetPropertyValueAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken);
}
