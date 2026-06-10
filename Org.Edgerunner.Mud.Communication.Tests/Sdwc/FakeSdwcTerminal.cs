using System.Drawing;
using Org.Edgerunner.Mud.Common.Querying;
using Org.Edgerunner.Mud.Communication.Interfaces;

namespace Org.Edgerunner.Mud.Communication.Tests.Sdwc;

/// <summary>
/// A fake <see cref="IClientTerminal"/> that captures the OOB lines sent to it and exposes a real
/// <see cref="MooWorldQueryService"/> for provider registration. An optional callback fires whenever an
/// OOB line is sent so tests can script inbound SDWC responses synchronously.
/// </summary>
public sealed class FakeSdwcTerminal : IClientTerminal
{
   public List<string> SentOutOfBandLines { get; } = new();

   public Action<string>? OnOutOfBandLineSent { get; set; }

   public MooWorldQueryService QueryProviders { get; } = new();

   public void SendOutOfBandLine(string text)
   {
      SentOutOfBandLines.Add(text);
      OnOutOfBandLineSent?.Invoke(text);
   }

   public void SendOutOfBandLines(IEnumerable<string> lines)
   {
      foreach (var line in lines)
         SendOutOfBandLine(line);
   }

   // Unused members.
   public Color ConsoleForegroundColor { get; set; }
   public Color ConsoleBackgroundColor { get; set; }
   public string Host => string.Empty;
   public int Port => 0;
   public string World => "TestWorld";
   public bool IsConnected => true;
   public bool EchoEnabled { get; set; }
   public void SendTextLines(IEnumerable<string> lines) { }
   public void SendTextLine(string text) { }
   public void SendText(string text) { }
   public void DisplayToConsole(string text) { }
   public void DisplayLineToConsole(string text) { }
   public void DisplayLinesToConsole(IEnumerable<string> lines) { }
   public void DisplayToConsole(string text, Color foregroundColor) { }
   public void DisplayLineToConsole(string text, Color foregroundColor) { }
   public void DisplayLinesToConsole(IEnumerable<string> lines, Color foregroundColor) { }
   public void DisplayToConsole(string text, Color foregroundColor, Color backgroundColor) { }
   public void DisplayLineToConsole(string text, Color foregroundColor, Color backgroundColor) { }
   public void DisplayLinesToConsole(IEnumerable<string> lines, Color foregroundColor, Color backgroundColor) { }
}
