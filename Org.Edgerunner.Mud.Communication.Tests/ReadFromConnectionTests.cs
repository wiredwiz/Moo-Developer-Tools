using System.Text;
using FluentAssertions;
using Org.Edgerunner.Mud.Communication;
using Xunit;

namespace Org.Edgerunner.Mud.Communication.Tests;

/// <summary>
/// Tests covering the read loop's line reassembly behaviour in <see cref="MudClientSession"/>.
/// These exercise the fix for splitting a single logical server line across read-buffer
/// boundaries (which corrupted multi-line OOB/MCP messages and broke @edit).
/// </summary>
public class ReadFromConnectionTests
{
    // Must match the read buffer size used inside MudClientSession.ReadFromConnection.
    private const int ReadBufferSize = 10240;

    /// <summary>
    /// A stream that hands out a pre-scripted sequence of byte chunks, one per ReadAsync call,
    /// honouring the caller's requested count (so a chunk larger than the buffer is delivered
    /// across multiple reads, exactly like a real socket). Returns 0 (EOF) once exhausted.
    /// </summary>
    private sealed class ScriptedStream : Stream
    {
        private readonly Queue<byte[]> _chunks;

        /// <param name="chunks">Each entry becomes one ReadAsync result (truncated to the caller's count).</param>
        public ScriptedStream(IEnumerable<byte[]> chunks)
        {
            _chunks = new Queue<byte[]>(chunks);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_chunks.Count == 0)
                return 0; // EOF

            var chunk = _chunks.Dequeue();
            var toCopy = Math.Min(chunk.Length, count);
            Array.Copy(chunk, 0, buffer, offset, toCopy);

            // If the caller's buffer was smaller than the chunk, requeue the remainder.
            if (toCopy < chunk.Length)
            {
                var remainder = new byte[chunk.Length - toCopy];
                Array.Copy(chunk, toCopy, remainder, 0, remainder.Length);
                var requeued = new Queue<byte[]>();
                requeued.Enqueue(remainder);
                while (_chunks.Count > 0)
                    requeued.Enqueue(_chunks.Dequeue());
                while (requeued.Count > 0)
                    _chunks.Enqueue(requeued.Dequeue());
            }

            return toCopy;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => Task.FromResult(Read(buffer, offset, count));

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    /// <summary>
    /// Test harness exposing a seam to inject a scripted stream into the session without a real socket.
    /// </summary>
    private sealed class TestSession : MudClientSession
    {
        public TestSession(Stream stream)
            : base("test", "localhost", 0)
        {
            _Stream = stream;
        }
    }

    private static async Task<List<string>> DrainAsync(MudClientSession session, int timeoutMs = 2000)
    {
        var messages = new List<string>();
        var reader = session.CommandChannel.Reader;
        using var cts = new CancellationTokenSource(timeoutMs);
        try
        {
            await foreach (var msg in reader.ReadAllAsync(cts.Token).ConfigureAwait(false))
                messages.Add(msg);
        }
        catch (OperationCanceledException)
        {
            // Channel is never completed by the session; we stop when no more data arrives.
        }
        return messages;
    }

    [Fact]
    public async Task LongLineSpanningTwoReads_IsReassembledAsSingleMessage()
    {
        // Build a single logical OOB line longer than the read buffer.
        // First read returns a full ReadBufferSize chunk ending mid-line;
        // the second read returns the remainder plus the terminating newline.
        var prefix = "#$#* 1234 content: ";
        var fillerLength = ReadBufferSize - prefix.Length + 500; // pushes well past one buffer
        var body = new string('X', fillerLength);
        var logicalLine = prefix + body; // no newline yet
        var lineBytes = Encoding.UTF8.GetBytes(logicalLine);

        var firstChunk = new byte[ReadBufferSize];
        Array.Copy(lineBytes, 0, firstChunk, 0, ReadBufferSize);

        var remainderLength = lineBytes.Length - ReadBufferSize;
        var secondChunk = new byte[remainderLength + 1];
        Array.Copy(lineBytes, ReadBufferSize, secondChunk, 0, remainderLength);
        secondChunk[remainderLength] = (byte)'\n';

        var stream = new ScriptedStream(new[] { firstChunk, secondChunk });
        var session = new TestSession(stream);

        session.BeginReadingDataTillClose();
        var messages = await DrainAsync(session);

        // The line must arrive as ONE reassembled message, not split across the read boundary.
        messages.Should().ContainSingle();
        messages[0].Should().Be(logicalLine + "\n");
    }

    [Fact]
    public async Task ShortReadWithoutNewline_IsFlushedAsItsOwnMessage()
    {
        // A prompt: a short read (less than the buffer) with no trailing newline must still be flushed.
        var prompt = "Enter password: ";
        var stream = new ScriptedStream(new[] { Encoding.UTF8.GetBytes(prompt) });
        var session = new TestSession(stream);

        session.BeginReadingDataTillClose();
        var messages = await DrainAsync(session);

        messages.Should().ContainSingle();
        messages[0].Should().Be(prompt);
    }
}
