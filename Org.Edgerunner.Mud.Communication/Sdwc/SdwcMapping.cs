#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="SdwcMapping.cs">
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

using System.Text.Json;
using Org.Edgerunner.Mud.Common.Querying;

namespace Org.Edgerunner.Mud.Communication.Sdwc;

/// <summary>
/// Pure, network-independent helpers that parse SDWC JSON payloads (System.Text.Json) into the
/// <see cref="Org.Edgerunner.Mud.Common.Querying"/> models. Kept free of any I/O so the mapping is
/// directly unit-testable.
/// </summary>
/// <remarks>
/// MOO object values may serialize either as a <c>#123</c> string or as a bare JSON number depending
/// on the server's JSON encoder; <see cref="ParseObjectId"/> accepts both forms. Verb <c>name</c> is a
/// whitespace-separated list of aliases.
/// </remarks>
public static class SdwcMapping
{
   /// <summary>
   /// Parses a MOO object id from a JSON element that may be a <c>#123</c> string or a bare number.
   /// </summary>
   /// <param name="element">The JSON element to interpret.</param>
   /// <returns>The parsed <see cref="MooObjectId"/>.</returns>
   /// <exception cref="FormatException">Thrown when the element cannot be interpreted as an object id.</exception>
   public static MooObjectId ParseObjectId(JsonElement element)
   {
      switch (element.ValueKind)
      {
         case JsonValueKind.Number when element.TryGetInt32(out var number):
            return new MooObjectId(number);
         case JsonValueKind.String:
            return ParseObjectId(element.GetString());
         default:
            throw new FormatException($"Cannot interpret JSON value '{element}' as a MOO object id.");
      }
   }

   /// <summary>
   /// Parses a MOO object id from a textual representation that may be <c>#123</c> or a bare number.
   /// </summary>
   /// <param name="text">The textual object id.</param>
   /// <returns>The parsed <see cref="MooObjectId"/>.</returns>
   /// <exception cref="FormatException">Thrown when the text cannot be interpreted as an object id.</exception>
   public static MooObjectId ParseObjectId(string? text)
   {
      if (string.IsNullOrWhiteSpace(text))
         throw new FormatException("Cannot interpret an empty value as a MOO object id.");

      var trimmed = text.Trim();
      if (trimmed.StartsWith("#", StringComparison.Ordinal))
         trimmed = trimmed.Substring(1);

      if (int.TryParse(trimmed, out var number))
         return new MooObjectId(number);

      throw new FormatException($"Cannot interpret '{text}' as a MOO object id.");
   }

   /// <summary>
   /// Splits a SDWC verb <c>name</c> (whitespace-separated aliases) into its individual aliases.
   /// </summary>
   /// <param name="name">The verb name field.</param>
   /// <returns>A read-only list of aliases (empty when <paramref name="name"/> is blank).</returns>
   public static IReadOnlyList<string> SplitAliases(string? name)
   {
      if (string.IsNullOrWhiteSpace(name))
         return Array.Empty<string>();

      return name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
   }

   /// <summary>
   /// Splits a SDWC overlay <c>value</c> into lines on either CR/LF or LF boundaries.
   /// </summary>
   /// <param name="value">The value text.</param>
   /// <returns>A read-only list of lines (empty when <paramref name="value"/> is null/empty).</returns>
   public static IReadOnlyList<string> SplitLines(string? value)
   {
      if (string.IsNullOrEmpty(value))
         return Array.Empty<string>();

      return value.Replace("\r\n", "\n").Split('\n');
   }

   /// <summary>
   /// Maps a SDWC <c>VERBS</c> payload to the queried object's verb summaries.
   /// </summary>
   /// <param name="json">The raw JSON object payload.</param>
   /// <returns>A read-only list of <see cref="MooVerbSummary"/>.</returns>
   public static IReadOnlyList<MooVerbSummary> MapVerbs(string json)
   {
      using var document = JsonDocument.Parse(json);
      var root = document.RootElement;
      var defining = ParseObjectId(root.GetProperty("object"));

      var result = new List<MooVerbSummary>();
      if (root.TryGetProperty("verbs", out var verbs) && verbs.ValueKind == JsonValueKind.Array)
         foreach (var verb in verbs.EnumerateArray())
         {
            var name = verb.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null;
            result.Add(new MooVerbSummary(SplitAliases(name), defining));
         }

      return result;
   }

   /// <summary>
   /// Maps a SDWC <c>PROPS</c> payload to the queried object's property summaries.
   /// </summary>
   /// <param name="json">The raw JSON object payload.</param>
   /// <returns>A read-only list of <see cref="MooPropertySummary"/>.</returns>
   public static IReadOnlyList<MooPropertySummary> MapProperties(string json)
   {
      using var document = JsonDocument.Parse(json);
      var root = document.RootElement;
      var defining = ParseObjectId(root.GetProperty("object"));

      var result = new List<MooPropertySummary>();
      if (root.TryGetProperty("props", out var props) && props.ValueKind == JsonValueKind.Object)
         foreach (var prop in props.EnumerateObject())
            result.Add(new MooPropertySummary(prop.Name, defining));

      return result;
   }

   /// <summary>
   /// Maps a SDWC <c>VERB-OVERLAY</c> payload to verb documentation, carrying the queried and resolved
   /// (defining) object ids.
   /// </summary>
   /// <param name="json">The raw JSON object payload.</param>
   /// <returns>A <see cref="MooVerbDocumentation"/>.</returns>
   public static MooVerbDocumentation MapVerbOverlay(string json)
   {
      using var document = JsonDocument.Parse(json);
      var root = document.RootElement;
      var queried = ParseObjectId(root.GetProperty("object"));
      var resolved = root.TryGetProperty("resolved_object", out var resolvedElement)
         ? ParseObjectId(resolvedElement)
         : queried;
      var value = root.TryGetProperty("value", out var valueElement) ? valueElement.GetString() : null;

      return new MooVerbDocumentation(queried, resolved, SplitLines(value));
   }

   /// <summary>
   /// Maps a SDWC <c>PROP-OVERLAY</c> payload to the property's value-preview lines.
   /// </summary>
   /// <param name="json">The raw JSON object payload.</param>
   /// <returns>A read-only list of value-preview lines.</returns>
   public static IReadOnlyList<string> MapPropOverlay(string json)
   {
      using var document = JsonDocument.Parse(json);
      var root = document.RootElement;
      var value = root.TryGetProperty("value", out var valueElement) ? valueElement.GetString() : null;

      return SplitLines(value);
   }
}
