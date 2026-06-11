using Org.Edgerunner.Mud.Communication.Interfaces;

namespace Org.Edgerunner.Mud.MCP.Tests;

/// <summary>A minimal fake terminal with a real query service, capturing single OOB lines.</summary>
public sealed class FakeQueryTerminal : IClientTerminal
{
   public List<string> SentOutOfBandLines { get; } = new();

   public Org.Edgerunner.Mud.Common.Querying.MooWorldQueryService QueryProviders { get; } = new();

   public bool IsConnected => true;

   public void SendOutOfBandLine(string text) => SentOutOfBandLines.Add(text);

   // Unused members.
   public System.Drawing.Color ConsoleForegroundColor { get; set; }
   public System.Drawing.Color ConsoleBackgroundColor { get; set; }
   public string Host => string.Empty;
   public int Port => 0;
   public string World => "TestWorld";
   public bool EchoEnabled { get; set; }
   public void SendTextLines(IEnumerable<string> lines) { }
   public void SendTextLine(string text) { }
   public void SendText(string text) { }
   public void SendOutOfBandLines(IEnumerable<string> lines) { }
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
