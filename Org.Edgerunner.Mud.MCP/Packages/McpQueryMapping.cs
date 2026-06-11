#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="McpQueryMapping.cs">
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
using Org.Edgerunner.Mud.Communication.Sdwc;

namespace Org.Edgerunner.Mud.MCP.Packages;

/// <summary>
/// Pure, network-independent helpers that parse <c>edgerunner-org-moo-query</c> JSON payloads
/// (System.Text.Json) into the <see cref="Org.Edgerunner.Mud.Common.Querying"/> models. See
/// <c>docs/edgerunner-org-moo-query-protocol.md</c> for the payload schemas.
/// </summary>
/// <remarks>
/// Summary listings (<see cref="MapVerbSummaries"/>, <see cref="MapPropertySummaries"/>,
/// <see cref="MapPropertyInfo"/>) describe the queried object only; the caller supplies the queried
/// id and it is used as <c>DefiningObject</c>. Resolved-object semantics exist only on the
/// verb-info/doc/code payloads, which carry explicit <c>q</c>/<c>r</c> fields.
/// </remarks>
public static class McpQueryMapping
{
   private static readonly Dictionary<string, Preposition> PrepositionAliases = new(StringComparer.OrdinalIgnoreCase)
   {
      ["with"] = Preposition.With,
      ["using"] = Preposition.With,
      ["at"] = Preposition.At,
      ["to"] = Preposition.At,
      ["in front of"] = Preposition.InFrontOf,
      ["in"] = Preposition.In,
      ["inside"] = Preposition.In,
      ["into"] = Preposition.In,
      ["on top of"] = Preposition.OnTopOf,
      ["on"] = Preposition.OnTopOf,
      ["onto"] = Preposition.OnTopOf,
      ["upon"] = Preposition.OnTopOf,
      ["out of"] = Preposition.OutOf,
      ["from inside"] = Preposition.OutOf,
      ["from"] = Preposition.OutOf,
      ["over"] = Preposition.Over,
      ["through"] = Preposition.Through,
      ["under"] = Preposition.Under,
      ["underneath"] = Preposition.Under,
      ["beneath"] = Preposition.Under,
      ["behind"] = Preposition.Behind,
      ["beside"] = Preposition.Beside,
      ["for"] = Preposition.For,
      ["about"] = Preposition.For,
      ["is"] = Preposition.Is,
      ["as"] = Preposition.As,
      ["off"] = Preposition.Off,
      ["off of"] = Preposition.Off
   };

   /// <summary>
   /// Maps a <c>{"d":[[num,name,[aliases]],…]}</c> payload to object summaries.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <returns>A read-only list of <see cref="MooObjectSummary"/>.</returns>
   public static IReadOnlyList<MooObjectSummary> MapObjectSummaries(string json)
   {
      using var document = JsonDocument.Parse(json);
      var result = new List<MooObjectSummary>();
      if (document.RootElement.TryGetProperty("d", out var rows) && rows.ValueKind == JsonValueKind.Array)
         foreach (var row in rows.EnumerateArray())
         {
            if (row.ValueKind != JsonValueKind.Array || row.GetArrayLength() < 3)
               continue;

            var id = SdwcMapping.ParseObjectId(row[0]);
            var name = row[1].ValueKind == JsonValueKind.String ? row[1].GetString() ?? string.Empty : string.Empty;
            var aliases = new List<string>();
            if (row[2].ValueKind == JsonValueKind.Array)
               foreach (var alias in row[2].EnumerateArray())
                  if (alias.ValueKind == JsonValueKind.String)
                     aliases.Add(alias.GetString()!);

            result.Add(new MooObjectSummary(id, name, aliases));
         }

      return result;
   }

   /// <summary>
   /// Maps a <c>{"p":num}</c> payload to a parent id; a negative number means no parent.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <returns>The parent <see cref="MooObjectId"/>, or <c>null</c> when the object has no parent.</returns>
   public static MooObjectId? MapParent(string json)
   {
      using var document = JsonDocument.Parse(json);
      var number = document.RootElement.GetProperty("p").GetInt32();
      return number < 0 ? null : new MooObjectId(number);
   }

   /// <summary>
   /// Maps a <c>{"d":["g*et put",…]}</c> payload to verb summaries of the queried object.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <param name="queried">The queried object id, used as every row's <c>DefiningObject</c>.</param>
   /// <returns>A read-only list of <see cref="MooVerbSummary"/>.</returns>
   public static IReadOnlyList<MooVerbSummary> MapVerbSummaries(string json, MooObjectId queried)
   {
      using var document = JsonDocument.Parse(json);
      var result = new List<MooVerbSummary>();
      if (document.RootElement.TryGetProperty("d", out var rows) && rows.ValueKind == JsonValueKind.Array)
         foreach (var row in rows.EnumerateArray())
            if (row.ValueKind == JsonValueKind.String)
               result.Add(new MooVerbSummary(SdwcMapping.SplitAliases(row.GetString()), queried));

      return result;
   }

   /// <summary>
   /// Maps a <c>{"d":["name",…]}</c> payload to property summaries of the queried object.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <param name="queried">The queried object id, used as every row's <c>DefiningObject</c>.</param>
   /// <returns>A read-only list of <see cref="MooPropertySummary"/>.</returns>
   public static IReadOnlyList<MooPropertySummary> MapPropertySummaries(string json, MooObjectId queried)
   {
      using var document = JsonDocument.Parse(json);
      var result = new List<MooPropertySummary>();
      if (document.RootElement.TryGetProperty("d", out var rows) && rows.ValueKind == JsonValueKind.Array)
         foreach (var row in rows.EnumerateArray())
            if (row.ValueKind == JsonValueKind.String)
               result.Add(new MooPropertySummary(row.GetString()!, queried));

      return result;
   }

   /// <summary>
   /// Maps a <c>{"q","r","a","o","p","g"}</c> verb-info payload to a <see cref="MooVerbInfo"/>.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <returns>The parsed <see cref="MooVerbInfo"/>.</returns>
   public static MooVerbInfo MapVerbInfo(string json)
   {
      using var document = JsonDocument.Parse(json);
      var root = document.RootElement;

      var queried = new MooObjectId(root.GetProperty("q").GetInt32());
      var resolved = new MooObjectId(root.GetProperty("r").GetInt32());
      var aliases = SdwcMapping.SplitAliases(root.GetProperty("a").GetString());
      var owner = new MooObjectId(root.GetProperty("o").GetInt32());
      var permissions = ParseVerbPermissions(root.GetProperty("p").GetString() ?? string.Empty);

      var specs = root.GetProperty("g");
      var args = new VerbArgs(
         ParseDirectObject(specs[0].GetString() ?? string.Empty),
         ParsePreposition(specs[1].GetString() ?? string.Empty),
         ParseIndirectObject(specs[2].GetString() ?? string.Empty));

      return new MooVerbInfo(queried, resolved, aliases, owner, permissions, args);
   }

   /// <summary>
   /// Maps a <c>{"q","r","l"}</c> verb-doc payload to a <see cref="MooVerbDocumentation"/>.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <returns>The parsed <see cref="MooVerbDocumentation"/>.</returns>
   public static MooVerbDocumentation MapVerbDocumentation(string json)
   {
      using var document = JsonDocument.Parse(json);
      var root = document.RootElement;
      return new MooVerbDocumentation(
         new MooObjectId(root.GetProperty("q").GetInt32()),
         new MooObjectId(root.GetProperty("r").GetInt32()),
         ReadLines(root));
   }

   /// <summary>
   /// Maps a <c>{"q","r","l"}</c> verb-code payload to a <see cref="MooVerbCode"/>.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <returns>The parsed <see cref="MooVerbCode"/>.</returns>
   public static MooVerbCode MapVerbCode(string json)
   {
      using var document = JsonDocument.Parse(json);
      var root = document.RootElement;
      return new MooVerbCode(
         new MooObjectId(root.GetProperty("q").GetInt32()),
         new MooObjectId(root.GetProperty("r").GetInt32()),
         ReadLines(root));
   }

   /// <summary>
   /// Maps a <c>{"n","o","p","t","v"}</c> prop-info payload to a <see cref="MooPropertyInfo"/>.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <param name="queried">The queried object id, used as the <c>DefiningObject</c>.</param>
   /// <returns>The parsed <see cref="MooPropertyInfo"/>.</returns>
   public static MooPropertyInfo MapPropertyInfo(string json, MooObjectId queried)
   {
      using var document = JsonDocument.Parse(json);
      var root = document.RootElement;
      return new MooPropertyInfo(
         root.GetProperty("n").GetString() ?? string.Empty,
         new MooObjectId(root.GetProperty("o").GetInt32()),
         ParsePropertyPermissions(root.GetProperty("p").GetString() ?? string.Empty),
         queried,
         root.GetProperty("t").GetInt32(),
         root.GetProperty("v").GetString() ?? string.Empty);
   }

   /// <summary>
   /// Maps a <c>{"l":[lines]}</c> payload to its lines.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <returns>A read-only list of lines.</returns>
   public static IReadOnlyList<string> MapLines(string json)
   {
      using var document = JsonDocument.Parse(json);
      return ReadLines(document.RootElement);
   }

   /// <summary>
   /// Maps a <c>{"t","v"}</c> prop-value payload to a <see cref="MooPropertyValue"/>.
   /// </summary>
   /// <param name="json">The reply JSON.</param>
   /// <returns>The parsed <see cref="MooPropertyValue"/>.</returns>
   public static MooPropertyValue MapPropertyValue(string json)
   {
      using var document = JsonDocument.Parse(json);
      var root = document.RootElement;
      return new MooPropertyValue(
         root.GetProperty("t").GetInt32(),
         root.GetProperty("v").GetString() ?? string.Empty);
   }

   /// <summary>
   /// Parses a MOO verb permission flag string (e.g. <c>"rxd"</c>) into a <see cref="VerbPermission"/>.
   /// </summary>
   /// <param name="flags">The flag string.</param>
   /// <returns>The parsed <see cref="VerbPermission"/>.</returns>
   public static VerbPermission ParseVerbPermissions(string flags) =>
      new(flags.Contains('r'), flags.Contains('w'), flags.Contains('x'), flags.Contains('d'));

   /// <summary>
   /// Parses a MOO property permission flag string (e.g. <c>"rc"</c>) into a <see cref="PropertyPermission"/>.
   /// </summary>
   /// <param name="flags">The flag string.</param>
   /// <returns>The parsed <see cref="PropertyPermission"/>.</returns>
   public static PropertyPermission ParsePropertyPermissions(string flags) =>
      new(flags.Contains('r'), flags.Contains('w'), flags.Contains('c'));

   /// <summary>
   /// Parses a MOO direct object specifier (<c>this</c>/<c>none</c>/<c>any</c>).
   /// </summary>
   /// <param name="spec">The specifier text.</param>
   /// <returns>The parsed <see cref="DirectObject"/>; unrecognized specs map to <see cref="DirectObject.None"/>.</returns>
   public static DirectObject ParseDirectObject(string spec) =>
      spec.Trim().ToLowerInvariant() switch
      {
         "this" => DirectObject.This,
         "any" => DirectObject.Any,
         _ => DirectObject.None
      };

   /// <summary>
   /// Parses a MOO indirect object specifier (<c>this</c>/<c>none</c>/<c>any</c>).
   /// </summary>
   /// <param name="spec">The specifier text.</param>
   /// <returns>The parsed <see cref="IndirectObject"/>; unrecognized specs map to <see cref="IndirectObject.None"/>.</returns>
   public static IndirectObject ParseIndirectObject(string spec) =>
      spec.Trim().ToLowerInvariant() switch
      {
         "this" => IndirectObject.This,
         "any" => IndirectObject.Any,
         _ => IndirectObject.None
      };

   /// <summary>
   /// Parses a MOO preposition specifier as returned by <c>verb_args()</c> (e.g. <c>"with/using"</c>,
   /// <c>"in front of"</c>); any slash-separated segment matching a known alias resolves the preposition.
   /// </summary>
   /// <param name="spec">The specifier text.</param>
   /// <returns>The parsed <see cref="Preposition"/>; unrecognized specs map to <see cref="Preposition.None"/>.</returns>
   public static Preposition ParsePreposition(string spec)
   {
      var trimmed = spec.Trim().ToLowerInvariant();
      if (trimmed.Length == 0 || trimmed == "none")
         return Preposition.None;
      if (trimmed == "any")
         return Preposition.Any;

      foreach (var segment in trimmed.Split('/'))
         if (PrepositionAliases.TryGetValue(segment.Trim(), out var preposition))
            return preposition;

      return Preposition.None;
   }

   private static IReadOnlyList<string> ReadLines(JsonElement root)
   {
      var lines = new List<string>();
      if (root.TryGetProperty("l", out var array) && array.ValueKind == JsonValueKind.Array)
         foreach (var line in array.EnumerateArray())
            if (line.ValueKind == JsonValueKind.String)
               lines.Add(line.GetString()!);

      return lines;
   }
}
