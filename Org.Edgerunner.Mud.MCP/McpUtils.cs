#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="McpUtils.cs">
// Copyright (c)  2022
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

using System.Text;
using Org.Edgerunner.Common.Extensions;

namespace Org.Edgerunner.Mud.MCP;

public static class McpUtils
{
   /// <summary>
   /// Splits the message into words as normally recognized by the MCP protocol.
   /// </summary>
   /// <param name="message">The message.</param>
   /// <returns></returns>
   internal static List<string> SplitMessageIntoWords(string message)
   {
      message = message.Trim();
      var datum = new StringBuilder(message.Length);
      var words = new List<string>();
      var readingData = false;
      var readingString = false;
      for (var i = 0; i < message.Length; i++)
      {
         var character = message[i];
         if (character >= 32 && character <= 126)
            if (readingString)
            {
               if (character == '"')
               {
                  words.Add(datum.ToString());
                  datum.Clear();
                  readingString = false;
                  readingData = false;
               }
               else
                  datum.Append(character);
            }
            else if (datum.Length == 0 && character == '"')
            {
               readingString = true;
               readingData = true;
            }
            else if (character == ':')
            {
               datum.Append(character);

               // Now let's see if we have been reading a previous value before we encountered this key
               var end = datum.LastIndexOf(' ');
               if (end != -1)
               {
                  // Find the value data and extract it as a word
                  words.Add(datum.ToString(0, end).Trim());

                  // Trim our preceding value from the datum so only our current key is left
                  //datum.Remove(0, end + 1);
               }
               words.Add(datum.ToString(end + 1, datum.Length - (end + 1)).Trim());
               datum.Clear();
               readingData = true;

               // Read ahead and skip the white space until our value starts
               while (i < message.Length - 1 && message[i+1] is ' ')
                  ++i;
            }
            else if (readingData)
               datum.Append(character);
            else if (character == ' ')
            {
               if (datum.Length != 0)
               {
                  words.Add(datum.ToString());
                  datum.Clear();
               }
            }
            else
               datum.Append(character);
      }

      if (datum.Length != 0)
         words.Add(datum.ToString());

      return words;
   }

   /// <summary>
   /// Formats an outbound MCP wire string (without the leading <c>#$#</c> prefix).
   /// </summary>
   /// <param name="name">The MCP message name.</param>
   /// <param name="key">The authentication key, or <see cref="string.Empty"/> if none.</param>
   /// <param name="data">The keyword/value pairs to append.</param>
   /// <returns>The formatted MCP message string.</returns>
   public static string FormatMessage(string name, string key, Dictionary<string, string> data)
   {
      var sb = new StringBuilder();
      sb.Append(name);

      if (!string.IsNullOrEmpty(key))
      {
         sb.Append(' ');
         sb.Append(key);
      }

      foreach (var (k, v) in data)
         AppendField(sb, k, v);

      return sb.ToString();
   }

   /// <summary>
   /// Formats an outbound MCP multiline message block (without the leading <c>#$#</c> prefix on any line).
   /// </summary>
   /// <param name="name">The MCP message name.</param>
   /// <param name="key">The authentication key, or <see cref="string.Empty"/> if none.</param>
   /// <param name="simpleFields">The simple (single-line) keyword/value pairs, formatted with the standard quoting rules.</param>
   /// <param name="multilineKeyword">The multiline field keyword (without the trailing <c>*:</c>), e.g. <c>content</c>.</param>
   /// <param name="contentLines">The literal multiline content lines (sent verbatim, never quoted).</param>
   /// <param name="dataTag">The data-tag correlating the continuation lines for this block.</param>
   /// <returns>The ordered wire lines: the initial message line, one continuation line per content line, then the closing line.</returns>
   public static IEnumerable<string> FormatMultilineMessage(
      string name,
      string key,
      Dictionary<string, string> simpleFields,
      string multilineKeyword,
      IEnumerable<string> contentLines,
      string dataTag)
   {
      var lines = new List<string>();

      // Initial line: name [key] <simple fields...> <keyword>*: "" _data-tag: <dataTag>
      var sb = new StringBuilder();
      sb.Append(name);
      if (!string.IsNullOrEmpty(key))
      {
         sb.Append(' ');
         sb.Append(key);
      }

      foreach (var (k, v) in simpleFields)
         AppendField(sb, k, v);

      // The value after "<keyword>*:" is syntactically required but ignored; "" is conventional.
      sb.Append(' ');
      sb.Append(multilineKeyword);
      sb.Append("*: \"\" _data-tag: ");
      sb.Append(dataTag);
      lines.Add(sb.ToString());

      // One continuation line per content line. The content is literal — never quoted.
      foreach (var line in contentLines)
         lines.Add($"* {dataTag} {multilineKeyword}: {line}");

      // Closing line.
      lines.Add($": {dataTag}");

      return lines;
   }

   /// <summary>
   /// Appends a single " <keyword> <value>" segment to the builder, quoting the value if required.
   /// </summary>
   private static void AppendField(StringBuilder sb, string keyword, string value)
   {
      sb.Append(' ');
      sb.Append(keyword);
      sb.Append(' ');
      if (NeedsQuoting(value))
      {
         sb.Append('"');
         sb.Append(value);
         sb.Append('"');
      }
      else
         sb.Append(value);
   }

   private static bool NeedsQuoting(string value) =>
      string.IsNullOrEmpty(value) || value.Any(c => c == ' ' || c == '\t' || c == '"' || c == '*' || c == ':');

   /// <summary>
   /// Generates a randomized session key.
   /// </summary>
   /// <param name="length">The length for the key.</param>
   /// <returns>A random session key.</returns>
   public static string GenerateSessionKey(int length)
   {
      const string keyCharacters =
         "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
         "abcdefghijklmnopqrstuvwxyz" +
         "0123456789" +
         "-~`!@#$%^&()=+{}[]|';?/><.,";

      var rnd = new Random();
      var result = new StringBuilder();
      while (0 < length--) result.Append(keyCharacters[rnd.Next(keyCharacters.Length)]);
      return result.ToString();
   }
}