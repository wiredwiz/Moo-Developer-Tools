using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using Org.Edgerunner.Mud.Communication;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Xunit;

namespace Org.Edgerunner.Mud.Communication.Tests;

/// <summary>
/// Tests covering <see cref="RootMessageProcessor"/>'s reassembly of out-of-band lines that are
/// split across network read boundaries. The transport flushes complete lines with a trailing
/// '\n' and incomplete partials without one; a split OOB line must be stitched back together
/// before being handed to the OOB processor.
/// </summary>
public class RootMessageProcessorTests
{
   /// <summary>
   /// A hand-coded OOB processor that records every message handed to it. NSubstitute cannot be
   /// used here because <see cref="IMessageProtocolProcessor.ProcessMessage"/> takes a ref param.
   /// </summary>
   private sealed class RecordingOutOfBandProcessor : IMessageProtocolProcessor
   {
      public List<string> Received { get; } = new();

      public bool ProcessMessage(IClientTerminal client, string message, ref MessageProcessingState state)
      {
         Received.Add(message);
         return true;
      }

      public void Reset()
      {
      }

      public void OnDisconnected()
      {
      }
   }

   private const string Prefix = "#$#";

   private static (RootMessageProcessor processor, RecordingOutOfBandProcessor oob, IClientTerminal client) CreateSut()
   {
      var oob = new RecordingOutOfBandProcessor();
      var client = Substitute.For<IClientTerminal>();
      var processor = new RootMessageProcessor(Prefix, oob);
      return (processor, oob, client);
   }

   [Fact]
   public void Split_oob_line_is_reassembled_before_processing()
   {
      var (processor, oob, client) = CreateSut();

      // First piece: starts with the OOB prefix but has no terminating newline -> held.
      var firstResult = processor.ProcessMessage(client, "#$#* 1234 content: hello wor");

      firstResult.Should().BeTrue();
      oob.Received.Should().BeEmpty();

      // Second piece: completes the line. It is stitched onto the held partial, then routed.
      var secondResult = processor.ProcessMessage(client, "ld\n");

      secondResult.Should().BeTrue();
      oob.Received.Should().ContainSingle()
         .Which.Should().Be("* 1234 content: hello world\n");
   }

   [Fact]
   public void Complete_oob_line_in_one_call_is_processed_immediately()
   {
      var (processor, oob, client) = CreateSut();

      var result = processor.ProcessMessage(client, "#$#mcp-foo 1234 x: y\n");

      result.Should().BeTrue();
      oob.Received.Should().ContainSingle()
         .Which.Should().Be("mcp-foo 1234 x: y\n");
   }

   [Fact]
   public void Inband_prompt_partial_is_displayed_and_does_not_affect_later_lines()
   {
      var (processor, oob, client) = CreateSut();

      // A no-newline prompt that is not OOB must be displayed immediately, not held.
      var promptResult = processor.ProcessMessage(client, "Login: ");

      promptResult.Should().BeFalse();
      oob.Received.Should().BeEmpty();

      // A subsequent complete in-band line is unaffected by the prompt.
      var lineResult = processor.ProcessMessage(client, "You see nothing.\n");

      lineResult.Should().BeFalse();
      oob.Received.Should().BeEmpty();
   }

   [Fact]
   public void Inband_complete_line_is_displayed()
   {
      var (processor, oob, client) = CreateSut();

      var result = processor.ProcessMessage(client, "You see nothing.\n");

      result.Should().BeFalse();
      oob.Received.Should().BeEmpty();
   }
}
