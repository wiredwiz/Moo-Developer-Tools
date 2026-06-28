#region BSD 3-Clause License
// <copyright file="ListedCodeHighlighter.cs" company="Edgerunner.org">
// Copyright 2024
// </copyright>
//
// BSD 3-Clause License
//
// Copyright (c) 2024,
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

using System.Drawing;
using System.Text.RegularExpressions;
using Org.Edgerunner.Moo.Editor.Configuration;
using Org.Edgerunner.Moo.Editor.SyntaxHighlighting;

namespace Org.Edgerunner.Moo.Editor.Controls
{
   /// <summary>
   /// Stateful interceptor that detects verb-code listings printed to the terminal (e.g. the output of
   /// <c>@list obj:verb</c>), and re-renders the listed code using the editor's Moo syntax-highlighting
   /// color scheme. The decision logic (header detection, numbered/unnumbered mode and termination) is
   /// fully self-contained and unit-testable without a live console: the caller injects the arrival
   /// timestamp and receives output through callbacks.
   /// </summary>
   public class ListedCodeHighlighter
   {
      /// <summary>
      /// The internal capture state.
      /// </summary>
      private enum CaptureState
      {
         /// <summary>Not currently inside a listing.</summary>
         Idle,

         /// <summary>A header was seen; the next line decides numbered vs. unnumbered mode.</summary>
         AwaitingFirst,

         /// <summary>Capturing a numbered listing (each code line is prefixed with <c>N:</c>).</summary>
         Numbered,

         /// <summary>Capturing an unnumbered listing (terminated by a timing gap or a blank line).</summary>
         Unnumbered
      }

      /// <summary>
      /// The maximum inter-line gap, in milliseconds, before an unnumbered listing is considered ended.
      /// </summary>
      public const int UnnumberedGapMilliseconds = 500;

      private static readonly TimeSpan GapThreshold = TimeSpan.FromMilliseconds(UnnumberedGapMilliseconds);

      /// <summary>
      /// Matches the standard verb-list header, e.g. <c>#106:"tell"  this none this Wizard (#2), rxd</c>.
      /// Requires an object id, a verb name (optionally quoted, possibly an alias list), and the verb
      /// argument specification (<c>&lt;dobj&gt; &lt;prep&gt; &lt;iobj&gt;</c> where dobj/iobj are
      /// <c>this</c>/<c>none</c>/<c>any</c>); any owner/flags suffix is optional.
      /// </summary>
      private static readonly Regex HeaderPattern = new(
         @"^#\d+:(?:""[^""]*""|\S+)\s+(?:this|none|any)\s+.+?\s+(?:this|none|any)(?:\s.*)?$",
         RegexOptions.Compiled | RegexOptions.IgnoreCase);

      /// <summary>
      /// Matches a numbered code line and captures the leading <c>N:</c> prefix (with any leading
      /// whitespace) so it can be rendered plainly.
      /// </summary>
      private static readonly Regex NumberedPattern = new(@"^(?<prefix>\s*\d+:)", RegexOptions.Compiled);

      private readonly Func<GrammarDialect> _dialectProvider;
      private readonly Settings _settings;

      private CaptureState _state = CaptureState.Idle;
      private DateTime _lastCaptureUtc;

      /// <summary>
      /// Initializes a new instance of the <see cref="ListedCodeHighlighter"/> class.
      /// </summary>
      /// <param name="dialectProvider">
      /// A delegate returning the grammar dialect to lex captured code with. When <see langword="null"/>,
      /// the configured default dialect (<see cref="Settings.DefaultGrammarDialect"/>) is used and is
      /// re-read on every captured line so a runtime dialect change is honored.
      /// </param>
      /// <param name="settings">
      /// The settings source for colors. When <see langword="null"/>, the singleton
      /// <see cref="Settings.Instance"/> is used.
      /// </param>
      public ListedCodeHighlighter(Func<GrammarDialect> dialectProvider = null, Settings settings = null)
      {
         _settings = settings ?? Settings.Instance;
         _dialectProvider = dialectProvider ?? (() => _settings.DefaultGrammarDialect);
      }

      /// <summary>
      /// Resets the interceptor to its initial idle state, abandoning any in-progress capture.
      /// </summary>
      public void Reset()
      {
         _state = CaptureState.Idle;
         _lastCaptureUtc = default;
      }

      /// <summary>
      /// Processes a single received display line.
      /// </summary>
      /// <param name="line">
      /// The raw display line (its ANSI codes and any trailing newline are handled internally).
      /// </param>
      /// <param name="arrivalUtc">The UTC arrival time of the line (injected for testability).</param>
      /// <param name="passThrough">
      /// Invoked with the original <paramref name="line"/> when the interceptor consumes a line that
      /// should still be displayed verbatim (e.g. a listing header).
      /// </param>
      /// <param name="writeStyledLine">
      /// Invoked with the ordered (text, color) segments of a captured code line. The segments cover
      /// the whole visible line (including any plainly-rendered <c>N:</c> prefix). The caller is
      /// responsible for appending the line terminator.
      /// </param>
      /// <returns>
      /// <see langword="true"/> when the line was consumed by the interceptor (output produced via the
      /// callbacks); <see langword="false"/> when the line is not part of a listing and the caller
      /// should render it normally.
      /// </returns>
      public bool TryHandle(
         string line,
         DateTime arrivalUtc,
         Action<string> passThrough,
         Action<IReadOnlyList<(string Text, Color Color)>> writeStyledLine)
      {
         var content = MooCodeColorizer.StripAnsi(line ?? string.Empty).TrimEnd('\r', '\n');

         switch (_state)
         {
            case CaptureState.Idle:
               if (IsHeader(content))
               {
                  passThrough?.Invoke(line);
                  _state = CaptureState.AwaitingFirst;
                  return true;
               }

               return false;

            case CaptureState.AwaitingFirst:
               // A fresh header restarts a new block (multi-verb listings).
               if (IsHeader(content))
               {
                  passThrough?.Invoke(line);
                  _state = CaptureState.AwaitingFirst;
                  return true;
               }

               // A blank first line is not code: end the listing and render the blank normally.
               if (IsBlank(content))
               {
                  _state = CaptureState.Idle;
                  return false;
               }

               if (TryGetNumberedPrefix(content, out _))
               {
                  _state = CaptureState.Numbered;
                  EmitNumbered(content, writeStyledLine);
                  _lastCaptureUtc = arrivalUtc;
                  return true;
               }

               _state = CaptureState.Unnumbered;
               EmitUnnumbered(content, writeStyledLine);
               _lastCaptureUtc = arrivalUtc;
               return true;

            case CaptureState.Numbered:
               if (IsHeader(content))
               {
                  passThrough?.Invoke(line);
                  _state = CaptureState.AwaitingFirst;
                  return true;
               }

               if (TryGetNumberedPrefix(content, out _))
               {
                  EmitNumbered(content, writeStyledLine);
                  _lastCaptureUtc = arrivalUtc;
                  return true;
               }

               // First non-numbered line terminates a numbered listing (deterministic; no timing).
               _state = CaptureState.Idle;
               return false;

            case CaptureState.Unnumbered:
               if (IsHeader(content))
               {
                  passThrough?.Invoke(line);
                  _state = CaptureState.AwaitingFirst;
                  return true;
               }

               // A blank line, or a gap of >= 500 ms since the previous captured line, ends the listing.
               if (IsBlank(content) || arrivalUtc - _lastCaptureUtc >= GapThreshold)
               {
                  _state = CaptureState.Idle;
                  return false;
               }

               EmitUnnumbered(content, writeStyledLine);
               _lastCaptureUtc = arrivalUtc;
               return true;

            default:
               return false;
         }
      }

      /// <summary>
      /// Determines whether the supplied (ANSI-stripped, newline-trimmed) line is a verb-list header.
      /// </summary>
      /// <param name="content">The line content.</param>
      /// <returns><see langword="true"/> if it matches the header pattern; otherwise <see langword="false"/>.</returns>
      public static bool IsHeader(string content)
      {
         return !string.IsNullOrEmpty(content) && HeaderPattern.IsMatch(content);
      }

      private static bool IsBlank(string content) => string.IsNullOrWhiteSpace(content);

      private static bool TryGetNumberedPrefix(string content, out string prefix)
      {
         prefix = null;
         if (string.IsNullOrEmpty(content))
            return false;

         var match = NumberedPattern.Match(content);
         if (!match.Success)
            return false;

         prefix = match.Groups["prefix"].Value;
         return true;
      }

      private void EmitNumbered(string content, Action<IReadOnlyList<(string Text, Color Color)>> writeStyledLine)
      {
         if (writeStyledLine == null)
            return;

         TryGetNumberedPrefix(content, out var prefix);
         prefix ??= string.Empty;
         var remainder = content.Substring(prefix.Length);

         var segments = new List<(string Text, Color Color)>
         {
            // The line-number prefix is rendered plainly (default word color), preserving line numbers.
            (prefix, _settings.DefaultWordColor)
         };
         segments.AddRange(MooCodeColorizer.GetColoredSegments(remainder, _dialectProvider(), _settings));
         writeStyledLine(segments);
      }

      private void EmitUnnumbered(string content, Action<IReadOnlyList<(string Text, Color Color)>> writeStyledLine)
      {
         writeStyledLine?.Invoke(MooCodeColorizer.GetColoredSegments(content, _dialectProvider(), _settings));
      }
   }
}
