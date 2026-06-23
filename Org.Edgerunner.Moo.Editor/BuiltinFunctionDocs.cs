#region BSD 3-Clause License
// <copyright file="BuiltinFunctionDocs.cs" company="Edgerunner.org">
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

/// <summary>Documentation for a MOO built-in function (a signature line and a summary description).</summary>
public sealed record BuiltinFunctionDoc(string Signature, string Description);

/// <summary>
/// Loads (once, lazily) the embedded built-in function documentation extracted from the ToastStunt
/// programmer's manual, and looks it up by function name.
/// </summary>
public static class BuiltinFunctionDocs
{
   private const string ResourceName = "Org.Edgerunner.Moo.Editor.Resources.builtin-function-docs.json";

   private static readonly Lazy<IReadOnlyDictionary<string, BuiltinFunctionDoc>> Docs = new(Load);

   /// <summary>Gets the documentation for the named built-in, or <c>null</c> if unknown.</summary>
   public static BuiltinFunctionDoc Get(string name) =>
      name != null && Docs.Value.TryGetValue(name, out var doc) ? doc : null;

   /// <summary>
   /// Builds the tooltip body for a built-in: the signature on the first line, the description on the
   /// following lines. Returns <c>null</c> when the function is unknown.
   /// </summary>
   public static string GetTooltipText(string name)
   {
      var doc = Get(name);
      if (doc == null)
         return null;
      return string.IsNullOrEmpty(doc.Description) ? doc.Signature : doc.Signature + "\n" + doc.Description;
   }

   private static IReadOnlyDictionary<string, BuiltinFunctionDoc> Load()
   {
      try
      {
         using var stream = typeof(BuiltinFunctionDocs).Assembly.GetManifestResourceStream(ResourceName);
         if (stream == null)
            return new Dictionary<string, BuiltinFunctionDoc>();
         var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
         return JsonSerializer.Deserialize<Dictionary<string, BuiltinFunctionDoc>>(stream, options)
                ?? new Dictionary<string, BuiltinFunctionDoc>();
      }
      catch
      {
         return new Dictionary<string, BuiltinFunctionDoc>();
      }
   }
}
