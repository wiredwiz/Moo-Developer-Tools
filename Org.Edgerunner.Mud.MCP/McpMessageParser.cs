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
   private string _dataTag = string.Empty;
   private Dictionary<string, string> _simpleFields = new();

   // Multiline buffers keyed by field NAME (the keyword without its trailing "*:").
   // All multiline fields in a single message share one _data-tag and are distinguished
   // by keyword on the continuation lines.
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
         _multilineBuffers = new Dictionary<string, List<string>>();
         _dataTag = string.Empty;

         bool hasMultiline = false;
         bool hasDataTag = false;
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
                  // A multiline field. Identified by its field NAME (keyword minus the "*:").
                  // The inline value (this word) is ignored, but still consumed.
                  var fieldName = currentKey[..^2].ToLowerInvariant();
                  _multilineBuffers[fieldName] = new List<string>();
                  hasMultiline = true;
               }
               else if (currentKey.Equals("_data-tag:", StringComparison.OrdinalIgnoreCase))
               {
                  // The single framing data-tag for the whole message. Not part of the
                  // assembled Message.Data.
                  _dataTag = word;
                  hasDataTag = true;
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
            // A multiline message must carry exactly one _data-tag to frame its continuation
            // lines. Missing it is malformed.
            if (!hasDataTag)
               return McpParseState.Error;

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
      // Continuation line: "* <data-tag> <keyword>: <value>"
      // The value is LITERAL: everything after the single space following "<keyword>:" is
      // verbatim (quotes, colons, leading spaces preserved). It must NOT be re-parsed.
      if (line.StartsWith("* "))
      {
         var rest = line.Substring(2);

         var tagEnd = rest.IndexOf(' ');
         if (tagEnd < 0)
            return McpParseState.Error;

         var tag = rest.Substring(0, tagEnd);
         var afterTag = rest.Substring(tagEnd + 1);

         var colonIndex = afterTag.IndexOf(':');
         if (colonIndex < 0)
            return McpParseState.Error;

         var keyword = afterTag.Substring(0, colonIndex).ToLowerInvariant();

         // Skip exactly one space following the "<keyword>:" if present; the remainder
         // (possibly empty, possibly with leading spaces) is the literal value.
         var afterColon = afterTag.Substring(colonIndex + 1);
         var value = afterColon.StartsWith(" ") ? afterColon.Substring(1) : afterColon;

         // Only append when both the framing tag and the keyword match the message.
         if (tag == _dataTag && _multilineBuffers.TryGetValue(keyword, out var buffer))
            buffer.Add(value);

         return McpParseState.InProgress;
      }

      // Close line: ": <data-tag>"
      if (line == ":" || line.StartsWith(": "))
      {
         var tag = line.Length > 2 ? line.Substring(2).Trim() : string.Empty;

         if (tag != _dataTag)
            return McpParseState.Error;

         var data = new Dictionary<string, string>(_simpleFields);
         foreach (var (fieldName, lines) in _multilineBuffers)
            data[fieldName + ":"] = string.Join("\n", lines);

         Result = new Message(_name, _key, data);
         _state = InternalState.Normal;
         return McpParseState.Complete;
      }

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
      _dataTag = string.Empty;
      _simpleFields = new Dictionary<string, string>();
      _multilineBuffers = new Dictionary<string, List<string>>();
      Result = null;
   }
}
