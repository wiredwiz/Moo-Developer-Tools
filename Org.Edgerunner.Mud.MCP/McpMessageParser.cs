#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="McpMessageParser.cs">
// Copyright (c) Thaddeus Ryker 2022
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

namespace Org.Edgerunner.Mud.MCP;

public enum McpParseState { InProgress, Complete, Error }

/// <summary>
/// A stateful line-by-line parser that assembles a single MCP <see cref="Message"/> from
/// one or more text lines received from the server.
/// </summary>
public class McpMessageParser
{
   private enum InternalState { Normal, InMultiline }

   private InternalState _state = InternalState.Normal;
   private string _name = string.Empty;
   private string _key = string.Empty;
   private Dictionary<string, string> _simpleFields = new();
   private Dictionary<string, string> _dataTagToKeyword = new();
   private Dictionary<string, List<string>> _multilineBuffers = new();

   /// <summary>
   /// Gets the fully assembled <see cref="Message"/> after a <see cref="FeedLine"/> call
   /// returns <see cref="McpParseState.Complete"/>; <c>null</c> at all other times.
   /// </summary>
   public Message? Result { get; private set; }

   /// <summary>
   /// Feeds the next line of server output into the parser.
   /// </summary>
   /// <param name="line">A single line of text (without the terminating newline).</param>
   /// <returns>
   /// <see cref="McpParseState.Complete"/> when a full message has been assembled,
   /// <see cref="McpParseState.InProgress"/> when more lines are expected, or
   /// <see cref="McpParseState.Error"/> when the line could not be parsed.
   /// </returns>
   public McpParseState FeedLine(string line)
   {
      Result = null;
      return _state == InternalState.Normal
         ? ProcessNormalLine(line)
         : ProcessMultilineLine(line);
   }

   private McpParseState ProcessNormalLine(string line)
   {
      if (string.IsNullOrWhiteSpace(line))
         return McpParseState.Error;

      if (line.StartsWith("* ") || line == "*" || line.StartsWith(": ") || line == ":")
         return McpParseState.Error;

      try
      {
         var words = McpUtils.SplitMessageIntoWords(line);
         if (words.Count == 0)
            return McpParseState.Error;

         _name = words[0];
         words.RemoveAt(0);

         _key = string.Empty;
         if (words.Count > 0 && !words[0].EndsWith(':'))
         {
            _key = words[0];
            words.RemoveAt(0);
         }

         _simpleFields = new Dictionary<string, string>();
         _dataTagToKeyword = new Dictionary<string, string>();
         _multilineBuffers = new Dictionary<string, List<string>>();

         bool hasMultiline = false;
         string currentKey = string.Empty;

         foreach (var word in words)
         {
            if (word.EndsWith(':'))
            {
               currentKey = word;
            }
            else if (!string.IsNullOrEmpty(currentKey))
            {
               if (currentKey.EndsWith("*:"))
               {
                  var fieldName = currentKey[..^2];
                  _dataTagToKeyword[word] = fieldName;
                  _multilineBuffers[word] = new List<string>();
                  hasMultiline = true;
               }
               else
               {
                  _simpleFields[currentKey.ToLowerInvariant()] = word;
               }
               currentKey = string.Empty;
            }
         }

         if (hasMultiline)
         {
            _state = InternalState.InMultiline;
            return McpParseState.InProgress;
         }

         Result = new Message(_name, _key, _simpleFields);
         return McpParseState.Complete;
      }
      catch
      {
         // Malformed input from the server — per MCP spec, drop silently
         return McpParseState.Error;
      }
   }

   private McpParseState ProcessMultilineLine(string line)
   {
      // Placeholder — multiline support added in a later task
      return McpParseState.Error;
   }

   /// <summary>
   /// Resets the parser to its initial state, discarding any partially assembled message.
   /// </summary>
   public void Reset()
   {
      _state = InternalState.Normal;
      _name = string.Empty;
      _key = string.Empty;
      _simpleFields = new Dictionary<string, string>();
      _dataTagToKeyword = new Dictionary<string, string>();
      _multilineBuffers = new Dictionary<string, List<string>>();
      Result = null;
   }
}
