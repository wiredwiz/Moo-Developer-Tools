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
      {
         sb.Append(' ');
         sb.Append(k);
         sb.Append(' ');
         if (NeedsQuoting(v))
         {
            sb.Append('"');
            sb.Append(v);
            sb.Append('"');
         }
         else
            sb.Append(v);
      }

      return sb.ToString();
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