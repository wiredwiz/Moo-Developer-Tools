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
   /// color scheme. The captured code body is buffered and emitted as a single block (rather than one
   /// line at a time) when the listing terminates, so the host can render it in one batched insert.
   /// The decision logic (header detection, numbered/unnumbered mode and termination) is fully
   /// self-contained and unit-testable without a live console: the host injects a flush scheduler
   /// (arm/cancel callbacks) and can drive the idle flush directly via <see cref="FlushPending"/>.
   /// </summary>
   public class ListedCodeHighlighter
   {
      /// <summary>
      /// The shape of the block flush callback: an ordered list of lines, each line being the ordered
      /// (text, color) segments covering that captured code line (including any plainly-rendered
      /// <c>N:</c> prefix). The host is responsible for line terminators.
      /// </summary>
      public delegate void WriteBlockCallback(IReadOnlyList<IReadOnlyList<(string Text, Color Color)>> lines);

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

         /// <summary>Capturing an unnumbered listing (terminated by a blank line, new header or idle flush).</summary>
         Unnumbered
      }

      /// <summary>
      /// The idle interval, in milliseconds, after the last captured line before the buffered block is
      /// flushed by the host's idle timer (so a listing that is the last output still renders). The host
      /// arms/resets a timer of this interval each time a line is captured.
      /// </summary>
      public const int IdleFlushMilliseconds = 500;

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
      private readonly Action _armFlush;
      private readonly Action _cancelFlush;

      /// <summary>The buffered code lines awaiting a block flush (Prefix is empty for unnumbered lines).</summary>
      private readonly List<(string Prefix, string Remainder)> _buffer = new();

      private CaptureState _state = CaptureState.Idle;

      /// <summary>The last block callback supplied to <see cref="TryHandle"/>, reused by <see cref="FlushPending"/>.</summary>
      private WriteBlockCallback _writeBlock;

      /// <summary>
      /// Initializes a new instance of the <see cref="ListedCodeHighlighter"/> class.
      /// </summary>
      /// <param name="dialectProvider">
      /// A delegate returning the grammar dialect to lex captured code with. When <see langword="null"/>,
      /// the configured default dialect (<see cref="Settings.DefaultGrammarDialect"/>) is used and is
      /// re-read on every flush so a runtime dialect change is honored.
      /// </param>
      /// <param name="settings">
      /// The settings source for colors. When <see langword="null"/>, the singleton
      /// <see cref="Settings.Instance"/> is used.
      /// </param>
      /// <param name="armFlush">
      /// Invoked (and reset) each time a code line is buffered. The host wires this to a timer of
      /// <see cref="IdleFlushMilliseconds"/> whose tick calls <see cref="FlushPending"/>, so a listing
      /// that is the last output still renders. May be <see langword="null"/> (e.g. in tests that drive
      /// <see cref="FlushPending"/> manually).
      /// </param>
      /// <param name="cancelFlush">
      /// Invoked when a pending flush is no longer needed (block flushed, or capture reset/abandoned).
      /// May be <see langword="null"/>.
      /// </param>
      public ListedCodeHighlighter(
         Func<GrammarDialect> dialectProvider = null,
         Settings settings = null,
         Action armFlush = null,
         Action cancelFlush = null)
      {
         _settings = settings ?? Settings.Instance;
         _dialectProvider = dialectProvider ?? (() => _settings.DefaultGrammarDialect);
         _armFlush = armFlush;
         _cancelFlush = cancelFlush;
      }

      /// <summary>
      /// Resets the interceptor to its initial idle state, abandoning any in-progress capture and any
      /// buffered (un-flushed) code, and cancelling any armed flush.
      /// </summary>
      public void Reset()
      {
         _state = CaptureState.Idle;
         _buffer.Clear();
         _cancelFlush?.Invoke();
      }

      /// <summary>
      /// Processes a single received display line, buffering captured code and flushing the buffered
      /// block (via <paramref name="writeBlock"/>) when the listing terminates.
      /// </summary>
      /// <param name="line">
      /// The raw display line (its ANSI codes and any trailing newline are handled internally).
      /// </param>
      /// <param name="passThrough">
      /// Invoked with the original <paramref name="line"/> when the interceptor consumes a line that
      /// should still be displayed verbatim (e.g. a listing header).
      /// </param>
      /// <param name="writeBlock">
      /// Invoked when a buffered code block is flushed, with the ordered (text, color) segments of every
      /// captured line. The caller is responsible for line terminators.
      /// </param>
      /// <returns>
      /// <see langword="true"/> when the line was consumed by the interceptor (buffered or it drove a
      /// flush); <see langword="false"/> when the line is not part of a listing and the caller should
      /// render it normally (any pending block is flushed first).
      /// </returns>
      public bool TryHandle(
         string line,
         Action<string> passThrough,
         WriteBlockCallback writeBlock)
      {
         _writeBlock = writeBlock;
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
               // A fresh header restarts a new block (multi-verb listings); nothing buffered yet.
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

               if (TryGetNumberedPrefix(content, out var firstPrefix))
               {
                  _state = CaptureState.Numbered;
                  BufferNumbered(content, firstPrefix);
                  return true;
               }

               _state = CaptureState.Unnumbered;
               BufferLine(string.Empty, content);
               return true;

            case CaptureState.Numbered:
               // A new header flushes the current block and starts a new listing.
               if (IsHeader(content))
               {
                  Flush();
                  passThrough?.Invoke(line);
                  _state = CaptureState.AwaitingFirst;
                  return true;
               }

               if (TryGetNumberedPrefix(content, out var nextPrefix))
               {
                  BufferNumbered(content, nextPrefix);
                  return true;
               }

               // First non-numbered line terminates a numbered listing (deterministic; no timing):
               // flush the buffered block, then let the caller render this line normally.
               Flush();
               _state = CaptureState.Idle;
               return false;

            case CaptureState.Unnumbered:
               // A new header flushes the current block and starts a new listing.
               if (IsHeader(content))
               {
                  Flush();
                  passThrough?.Invoke(line);
                  _state = CaptureState.AwaitingFirst;
                  return true;
               }

               // A blank line ends the listing: flush, then render the blank normally.
               if (IsBlank(content))
               {
                  Flush();
                  _state = CaptureState.Idle;
                  return false;
               }

               BufferLine(string.Empty, content);
               return true;

            default:
               return false;
         }
      }

      /// <summary>
      /// Flushes any buffered code block immediately and returns to idle. Wired by the host to the idle
      /// timer tick so a listing that is the last output is rendered after <see cref="IdleFlushMilliseconds"/>.
      /// Tests call this directly to stand in for the idle timer.
      /// </summary>
      public void FlushPending()
      {
         // A numbered listing is bounded deterministically by its line numbers, so an idle flush must
         // NOT end capture — a long listing can stream with gaps longer than IdleFlushMilliseconds.
         // Render what is buffered so far and KEEP capturing; only the first non-numbered line ends a
         // numbered listing. Unnumbered listings have no deterministic terminator, so the idle flush
         // ends them.
         if (_state == CaptureState.Numbered)
         {
            Flush();
            return;
         }

         Flush();
         _state = CaptureState.Idle;
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

      /// <summary>
      /// Buffers a numbered code line, peeling and retaining its <c>N:</c> prefix so it can be rendered
      /// plainly at flush time.
      /// </summary>
      private void BufferNumbered(string content, string prefix)
      {
         prefix ??= string.Empty;
         BufferLine(prefix, content.Substring(prefix.Length));
      }

      /// <summary>
      /// Buffers a single captured code line and (re)arms the idle flush.
      /// </summary>
      private void BufferLine(string prefix, string remainder)
      {
         _buffer.Add((prefix, remainder));
         _armFlush?.Invoke();
      }

      /// <summary>
      /// Colorizes and emits the buffered block (if any) via the last block callback, clears the buffer
      /// and cancels any armed flush. Colorizing happens here (per buffered line) so a runtime dialect
      /// change is honored.
      /// </summary>
      private void Flush()
      {
         _cancelFlush?.Invoke();

         if (_buffer.Count == 0)
            return;

         var dialect = _dialectProvider();
         var block = new List<IReadOnlyList<(string Text, Color Color)>>(_buffer.Count);
         foreach (var (prefix, remainder) in _buffer)
         {
            var segments = new List<(string Text, Color Color)>();

            // The line-number prefix is rendered plainly (default word color), preserving line numbers.
            if (!string.IsNullOrEmpty(prefix))
               segments.Add((prefix, _settings.DefaultWordColor));

            segments.AddRange(MooCodeColorizer.GetColoredSegments(remainder, dialect, _settings));
            block.Add(segments);
         }

         _buffer.Clear();
         _writeBlock?.Invoke(block);
      }
   }
}
