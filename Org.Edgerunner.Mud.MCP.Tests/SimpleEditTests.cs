using FluentAssertions;
using NSubstitute;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP;
using Org.Edgerunner.Mud.MCP.Interfaces;
using Org.Edgerunner.Mud.MCP.Packages;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class SimpleEditTests
{
   #region Test fakes

   private sealed class FakeSimpleEditConsumer : ISimpleEditConsumer
   {
      public int CallCount { get; private set; }
      public EditRequest? LastRequest { get; private set; }
      public IClientUploader? LastUploader { get; private set; }

      public void PresentEdit(EditRequest request, IClientUploader uploader)
      {
         CallCount++;
         LastRequest = request;
         LastUploader = uploader;
      }
   }

   private static McpClientSession CreateSession(string key)
   {
      var manager = new McpClientSessionManager(new Version(2, 1), new Version(2, 1), new List<IMcpPackage>());
      return new McpClientSession(manager, key, new Version(2, 1));
   }

   private static SimpleEditPackage CreatePackage(ISimpleEditConsumer consumer, string sessionKey = "KEY123")
   {
      var package = new SimpleEditPackage(consumer);
      package.SetSession(CreateSession(sessionKey));
      return package;
   }

   #endregion

   #region FormatMultilineMessage

   [Fact]
   public void FormatMultilineMessage_ProducesOrderedInitialContinuationAndCloseLines()
   {
      var lines = McpUtils.FormatMultilineMessage(
         "dns-org-mud-moo-simpleedit-set",
         "3487",
         new Dictionary<string, string>
         {
            ["reference:"] = "#73.name",
            ["type:"] = "string"
         },
         "content",
         new[] { "Erik" },
         "54321").ToList();

      lines.Should().HaveCount(3);
      lines[0].Should().Be("dns-org-mud-moo-simpleedit-set 3487 reference: #73.name type: string content*: \"\" _data-tag: 54321");
      lines[1].Should().Be("* 54321 content: Erik");
      lines[2].Should().Be(": 54321");
   }

   [Fact]
   public void FormatMultilineMessage_DoesNotQuoteOrEscapeContentLines()
   {
      // Content lines are literal — quotes, colons, and leading spaces must survive verbatim.
      var content = new[]
      {
         "player:tell(\"hello: world\");",
         "    return \"x\";"
      };

      var lines = McpUtils.FormatMultilineMessage(
         "dns-org-mud-moo-simpleedit-set",
         "3487",
         new Dictionary<string, string> { ["reference:"] = "#73:verb", ["type:"] = "moo-code" },
         "content",
         content,
         "abcd").ToList();

      lines[0].Should().Be("dns-org-mud-moo-simpleedit-set 3487 reference: \"#73:verb\" type: moo-code content*: \"\" _data-tag: abcd");
      lines[1].Should().Be("* abcd content: player:tell(\"hello: world\");");
      lines[2].Should().Be("* abcd content:     return \"x\";");
      lines[3].Should().Be(": abcd");
   }

   #endregion

   #region SimpleEditPackage

   private static Message ContentMessage(
      string reference = "#73:verb",
      string name = "Joe's verb",
      string type = "moo-code",
      string content = "player:tell(\"hi\");")
   {
      return new Message(
         SimpleEditPackage.ContentMessageName,
         "KEY123",
         new Dictionary<string, string>
         {
            ["reference:"] = reference,
            ["name:"] = name,
            ["type:"] = type,
            ["content:"] = content
         });
   }

   [Fact]
   public void ProcessMessage_ContentMessage_DrivesExactlyOnePresentEditWithCorrectFields()
   {
      var consumer = new FakeSimpleEditConsumer();
      var package = CreatePackage(consumer);
      var client = Substitute.For<IClientTerminal>();

      var handled = package.ProcessMessage(client, ContentMessage());

      handled.Should().BeTrue();
      consumer.CallCount.Should().Be(1);
      consumer.LastRequest!.Reference.Should().Be("#73:verb");
      consumer.LastRequest.Name.Should().Be("Joe's verb");
      consumer.LastRequest.EditType.Should().Be("moo-code");
      consumer.LastRequest.Content.Should().Be("player:tell(\"hi\");");
      consumer.LastUploader.Should().BeOfType<SimpleEditUploader>();
   }

   [Fact]
   public void CanHandleMessage_OnlyContentMessageName_CaseInsensitive()
   {
      var package = CreatePackage(new FakeSimpleEditConsumer());

      package.CanHandleMessage(new Message("DNS-ORG-MUD-MOO-SIMPLEEDIT-CONTENT", "KEY123",
         new Dictionary<string, string>())).Should().BeTrue();
      package.CanHandleMessage(new Message("dns-org-mud-moo-simpleedit-set", "KEY123",
         new Dictionary<string, string>())).Should().BeFalse();
   }

   [Fact]
   public void ProcessMessage_NonContentMessage_IsNotHandled()
   {
      var consumer = new FakeSimpleEditConsumer();
      var package = CreatePackage(consumer);
      var client = Substitute.For<IClientTerminal>();

      var handled = package.ProcessMessage(client, new Message("dns-org-mud-moo-simpleedit-set",
         "KEY123", new Dictionary<string, string>()));

      handled.Should().BeFalse();
      consumer.CallCount.Should().Be(0);
   }

   [Fact]
   public void ProcessMessage_MissingReference_IsNotHandled()
   {
      var consumer = new FakeSimpleEditConsumer();
      var package = CreatePackage(consumer);
      var client = Substitute.For<IClientTerminal>();

      var message = new Message(SimpleEditPackage.ContentMessageName, "KEY123",
         new Dictionary<string, string> { ["content:"] = "x" });

      package.ProcessMessage(client, message).Should().BeFalse();
      consumer.CallCount.Should().Be(0);
   }

   [Fact]
   public void ProcessMessage_MissingContent_IsNotHandled()
   {
      var consumer = new FakeSimpleEditConsumer();
      var package = CreatePackage(consumer);
      var client = Substitute.For<IClientTerminal>();

      var message = new Message(SimpleEditPackage.ContentMessageName, "KEY123",
         new Dictionary<string, string> { ["reference:"] = "#73:verb" });

      package.ProcessMessage(client, message).Should().BeFalse();
      consumer.CallCount.Should().Be(0);
   }

   #endregion

   #region SimpleEditUploader

   /// <summary>A minimal fake terminal capturing the OOB lines sent to it.</summary>
   private sealed class CapturingTerminal : IClientTerminal
   {
      public CapturingTerminal(bool connected) => IsConnected = connected;

      public List<string>? SentOutOfBandLines { get; private set; }

      public bool IsConnected { get; }

      public void SendOutOfBandLines(IEnumerable<string> lines) => SentOutOfBandLines = lines.ToList();

      // Unused members.
      public Org.Edgerunner.Mud.Common.Querying.MooWorldQueryService QueryProviders { get; } = new();
      public System.Drawing.Color ConsoleForegroundColor { get; set; }
      public System.Drawing.Color ConsoleBackgroundColor { get; set; }
      public string Host => string.Empty;
      public int Port => 0;
      public string World => "TestWorld";
      public bool EchoEnabled { get; set; }
      public void SendTextLines(IEnumerable<string> lines) { }
      public void SendTextLine(string text) { }
      public void SendText(string text) { }
      public void SendOutOfBandLine(string text) { }
      public void DisplayToConsole(string text) { }
      public void DisplayLineToConsole(string text) { }
      public void DisplayLinesToConsole(IEnumerable<string> lines) { }
      public void DisplayToConsole(string text, System.Drawing.Color foregroundColor) { }
      public void DisplayLineToConsole(string text, System.Drawing.Color foregroundColor) { }
      public void DisplayLinesToConsole(IEnumerable<string> lines, System.Drawing.Color foregroundColor) { }
      public void DisplayToConsole(string text, System.Drawing.Color foregroundColor, System.Drawing.Color backgroundColor) { }
      public void DisplayLineToConsole(string text, System.Drawing.Color foregroundColor, System.Drawing.Color backgroundColor) { }
      public void DisplayLinesToConsole(IEnumerable<string> lines, System.Drawing.Color foregroundColor, System.Drawing.Color backgroundColor) { }
   }

   [Fact]
   public void Upload_Connected_ProducesWellFormedSetBlockEchoingReferenceAndType()
   {
      var terminal = new CapturingTerminal(connected: true);
      var uploader = new SimpleEditUploader(terminal, "3487", "#73.name", "string");

      var result = uploader.Upload("Erik\nSecond line");

      result.Should().BeTrue();
      var lines = terminal.SentOutOfBandLines!;
      lines.Should().HaveCount(4);

      lines[0].Should().StartWith("dns-org-mud-moo-simpleedit-set 3487 reference: #73.name type: string content*: \"\" _data-tag: ");
      var dataTag = lines[0].Substring(lines[0].LastIndexOf(' ') + 1);
      dataTag.Should().NotBeNullOrEmpty();

      lines[1].Should().Be($"* {dataTag} content: Erik");
      lines[2].Should().Be($"* {dataTag} content: Second line");
      lines[3].Should().Be($": {dataTag}");
   }

   [Fact]
   public void Upload_HandlesCrLfLineEndings()
   {
      var terminal = new CapturingTerminal(connected: true);
      var uploader = new SimpleEditUploader(terminal, "3487", "#73.name", "string");

      uploader.Upload("a\r\nb");

      var lines = terminal.SentOutOfBandLines!;
      lines.Should().HaveCount(4);
      lines[1].Should().EndWith("content: a");
      lines[2].Should().EndWith("content: b");
   }

   [Fact]
   public void Upload_Disconnected_ReturnsFalseAndSendsNothing()
   {
      var terminal = new CapturingTerminal(connected: false);
      var uploader = new SimpleEditUploader(terminal, "3487", "#73.name", "string");

      var result = uploader.Upload("Erik");

      result.Should().BeFalse();
      terminal.SentOutOfBandLines.Should().BeNull();
   }

   #endregion

   #region Round-trip

   [Fact]
   public void RoundTrip_FormatSet_ThenParse_PreservesReferenceTypeAndContent()
   {
      var content = new[]
      {
         "player:tell(\"hello: world\");",
         "    return 1;"
      };

      var wireLines = McpUtils.FormatMultilineMessage(
         "dns-org-mud-moo-simpleedit-set",
         "3487",
         new Dictionary<string, string> { ["reference:"] = "#73:verb", ["type:"] = "moo-code" },
         "content",
         content,
         "tag99").ToList();

      var parser = new McpMessageParser();
      Message? result = null;
      foreach (var line in wireLines)
      {
         if (parser.FeedLine(line) == McpParseState.Complete)
            result = parser.Result;
      }

      result.Should().NotBeNull();
      result!.Name.Should().Be("dns-org-mud-moo-simpleedit-set");
      result.Key.Should().Be("3487");
      result.Data["reference:"].Should().Be("#73:verb");
      result.Data["type:"].Should().Be("moo-code");
      result.Data["content:"].Should().Be("player:tell(\"hello: world\");\n    return 1;");
   }

   #endregion
}
