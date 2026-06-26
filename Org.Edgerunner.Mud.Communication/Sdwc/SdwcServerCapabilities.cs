#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="SdwcServerCapabilities.cs">
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

namespace Org.Edgerunner.Mud.Communication.Sdwc;

/// <summary>
/// An immutable value object describing the set of SDWC abilities a server advertises via its
/// <c>SDWC%%SUPPORT%%</c> broadcast (a <c>|</c>-separated token list). Typed accessors expose the
/// known abilities (case-insensitive against the documented tokens), while <see cref="RawTokens"/>
/// preserves the full set — including unknown or future tokens — so nothing is silently dropped.
/// </summary>
public sealed class SdwcServerCapabilities
{
   /// <summary>The token advertising verb-listing support.</summary>
   public const string VerbsToken = "verbs";

   /// <summary>The token advertising property-listing support.</summary>
   public const string PropsToken = "props";

   /// <summary>The token advertising verb-overlay (hover documentation) support.</summary>
   public const string VerbOverlayToken = "VERB-OVERLAY";

   /// <summary>The token advertising property-overlay (hover documentation) support.</summary>
   public const string PropOverlayToken = "PROP-OVERLAY";

   private readonly HashSet<string> _tokens;

   /// <summary>
   /// Initializes a new instance of the <see cref="SdwcServerCapabilities"/> class from the parsed
   /// token set. Tokens are compared case-insensitively; the set is copied so the instance is immutable.
   /// </summary>
   /// <param name="tokens">The parsed (trimmed, non-empty) ability tokens. <c>null</c> is treated as empty.</param>
   public SdwcServerCapabilities(IEnumerable<string>? tokens)
   {
      _tokens = tokens is null
         ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
         : new HashSet<string>(tokens, StringComparer.OrdinalIgnoreCase);
   }

   /// <summary>Gets the full set of advertised tokens, with a case-insensitive comparer.</summary>
   public IReadOnlySet<string> RawTokens => _tokens;

   /// <summary>Gets a value indicating whether the server advertises verb-listing support.</summary>
   public bool SupportsVerbs => _tokens.Contains(VerbsToken);

   /// <summary>Gets a value indicating whether the server advertises property-listing support.</summary>
   public bool SupportsProps => _tokens.Contains(PropsToken);

   /// <summary>Gets a value indicating whether the server advertises verb-overlay support.</summary>
   public bool SupportsVerbOverlay => _tokens.Contains(VerbOverlayToken);

   /// <summary>Gets a value indicating whether the server advertises property-overlay support.</summary>
   public bool SupportsPropOverlay => _tokens.Contains(PropOverlayToken);

   /// <summary>
   /// Gets a value indicating whether the server advertises at least one queryable ability
   /// (verbs, props, verb-overlay, or prop-overlay) — the gate for registering the query provider.
   /// </summary>
   public bool HasAnyQueryableAbility => SupportsVerbs || SupportsProps || SupportsVerbOverlay || SupportsPropOverlay;
}
