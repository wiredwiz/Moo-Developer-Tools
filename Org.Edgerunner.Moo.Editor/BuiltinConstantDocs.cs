#region BSD 3-Clause License
// <copyright file="BuiltinConstantDocs.cs" company="Edgerunner.org">
// Copyright 2020
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

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Org.Edgerunner.Moo.Editor;

/// <summary>
/// Documentation for a MOO literal constant: its <see cref="Kind"/> (<c>type</c>, <c>error</c> or
/// <c>bool</c>) and the <see cref="Display"/> value shown after the constant name in a hover tooltip
/// (a <c>typeof</c> code for types, a <c>tostr()</c> message for errors, the literal for bools).
/// </summary>
public sealed record BuiltinConstantDoc(string Kind, string Display);

/// <summary>
/// Loads (once, lazily) the embedded built-in constant documentation (type codes, error messages and the
/// boolean literals) and looks it up by constant name. Mirrors <see cref="BuiltinFunctionDocs"/>.
/// </summary>
public static class BuiltinConstantDocs
{
   private const string ResourceName = "Org.Edgerunner.Moo.Editor.Resources.builtin-constant-docs.json";

   private static readonly Lazy<IReadOnlyDictionary<string, BuiltinConstantDoc>> Docs = new(Load);

   /// <summary>Gets the documentation for the named constant, or <c>null</c> if unknown.</summary>
   public static BuiltinConstantDoc Get(string name) =>
      name != null && Docs.Value.TryGetValue(name, out var doc) ? doc : null;

   /// <summary>
   /// Builds the tooltip body for a constant: <c>"NAME =&gt; display"</c> (the type code, error message or
   /// boolean literal). Returns <c>null</c> when the constant is unknown.
   /// </summary>
   public static string GetTooltipText(string name)
   {
      var doc = Get(name);
      return doc == null ? null : $"{name} => {doc.Display}";
   }

   private static IReadOnlyDictionary<string, BuiltinConstantDoc> Load()
   {
      try
      {
         using var stream = typeof(BuiltinConstantDocs).Assembly.GetManifestResourceStream(ResourceName);
         if (stream == null)
            return new Dictionary<string, BuiltinConstantDoc>();
         var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
         return JsonSerializer.Deserialize<Dictionary<string, BuiltinConstantDoc>>(stream, options)
                ?? new Dictionary<string, BuiltinConstantDoc>();
      }
      catch
      {
         return new Dictionary<string, BuiltinConstantDoc>();
      }
   }
}
