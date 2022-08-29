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

namespace Org.Edgerunner.Moo.Communication.MCP;

public static class McpUtils
{
   /// <summary>
   /// Attempts to parse the string as a MCP message.
   /// </summary>
   /// <param name="message">The message to parse.</param>
   /// <returns>A new <see cref="MCP.Message"/> instance.</returns>
   /// <exception cref="Org.Edgerunner.Moo.Communication.MCP.InvalidMcpMessageFormatException">
   /// The message does not resemble a properly formatted MCP message.
   /// </exception>
   public static MCP.Message? ParseMessage(string message)
   {
      var words = SplitMessageIntoWords(message);

      // Invalid message
      if (words.Count == 0)
         throw new InvalidMcpMessageFormatException($"Message \"{message}\" does not resemble a properly formatted MCP message.");

      var messageName = words[0];
      var messageKey = string.Empty;
      words.RemoveAt(0);

      // Fetch the key if it exists
      if (words.Count > 0 && !words[0].EndsWith(':'))
      {
         messageKey = words[1];
         words.RemoveAt(0);
      }

      // We have no data, end of message
      if (words.Count == 0)
         return new Message(messageName, messageKey, new Dictionary<string, string>());

      // Invalid message since we are not starting with a key
      if (!words[0].EndsWith(':'))
         throw new InvalidMcpMessageFormatException($"Message \"{message}\" does not resemble a properly formatted MCP message.\n" +
                                                    $"Word \"{words[0]}\" should be a data key.");

      var dataDictionary = new Dictionary<string, string>();
      var key = string.Empty;
      var value = string.Empty;
      foreach (var word in words)
      {
         // Process a key pair
         if (word.EndsWith(':'))
         {
            if (!string.IsNullOrEmpty(key))
            {
               dataDictionary[key.ToLowerInvariant()] = value;
               value = string.Empty;
               key = word;
            }
            else
               key = word;
         }
         else if (string.IsNullOrEmpty(key))
            throw new InvalidMcpMessageFormatException($"Message \"{message}\" does not resemble a properly formatted MCP message.\n" +
                                                       $"Word \"{word}\" appears to be a second data value rather than a key.");
         else
         {
            dataDictionary[key.ToLowerInvariant()] = word;
            key = value = string.Empty;
         }
      }
      if (!string.IsNullOrEmpty(key))
         dataDictionary[key.ToLowerInvariant()] = value;

      return new Message(messageName, messageKey, dataDictionary);
   }

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