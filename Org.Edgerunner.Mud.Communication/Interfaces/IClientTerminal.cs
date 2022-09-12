#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="IClientTerminal.cs">
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

using System.Drawing;

namespace Org.Edgerunner.Mud.Communication.Interfaces;

public interface IClientTerminal
{
   /// <summary>
   /// Gets or sets the color of the console foreground.
   /// </summary>
   /// <value>
   /// The color of the console foreground.
   /// </value>
   public Color ConsoleForegroundColor { get; set; }

   /// <summary>
   /// Gets or sets the color of the console background.
   /// </summary>
   /// <value>
   /// The color of the console background.
   /// </value>
   public Color ConsoleBackgroundColor { get; set; }

   /// <summary>
   /// Gets the client connection host address.
   /// </summary>
   /// <value>
   /// The host address.
   /// </value>
   public string Host { get; }

   /// <summary>
   /// Gets the client connection port number.
   /// </summary>
   /// <value>
   /// The port number.
   /// </value>
   public int Port { get; }

   /// <summary>
   /// Gets the client connection world name.
   /// </summary>
   /// <value>
   /// The world name.
   /// </value>
   public string World  { get; }

   /// <summary>
   /// Gets a value indicating whether this instance is connected.
   /// </summary>
   /// <value>
   ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
   /// </value>
   public bool IsConnected { get; }

   /// <summary>
   /// Gets or sets a value indicating whether [echo enabled].
   /// </summary>
   /// <value>
   ///   <c>true</c> if [echo enabled]; otherwise, <c>false</c>.
   /// </value>
   public bool EchoEnabled { get; set; }

   /// <summary>
   /// Sends the text lines to the client connection.
   /// </summary>
   /// <param name="lines">The text lines to send.</param>
   public void SendTextLines(IEnumerable<string> lines);

   /// <summary>
   /// Sends the text line to the client connection.
   /// </summary>
   /// <param name="text">The text to send.</param>
   public void SendTextLine(string text);

   /// <summary>
   /// Sends the text to the client connection.
   /// </summary>
   /// <param name="text">The text to send.</param>
   public void SendText(string text);

   /// <summary>
   /// Sends lines of text to the client connection as out of band command.
   /// </summary>
   /// <param name="lines">The list of lines.</param>
   public void SendOutOfBandLines(IEnumerable<string> lines);

   /// <summary>
   /// Sends the text to the client connection as out of band.
   /// </summary>
   /// <param name="text">The text to send.</param>
   public void SendOutOfBandLine(string text);

   /// <summary>
   /// Displays text to the terminal console emulator.
   /// </summary>
   /// <param name="text">The message to display.</param>
   public void DisplayToConsole(string text);

   /// <summary>
   /// Displays text line to the terminal console emulator.
   /// </summary>
   /// <param name="text">The message to display.</param>
   public void DisplayLineToConsole(string text);

   /// <summary>
   /// Displays text lines to the terminal console emulator.
   /// </summary>
   /// <param name="lines">The message lines to display.</param>
   public void DisplayLinesToConsole(IEnumerable<string> lines);

   /// <summary>
   /// Displays text line to the terminal console emulator.
   /// </summary>
   /// <param name="text">The message to display.</param>
   /// <param name="foregroundColor">The foreground color of the text.</param>
   /// <remarks>The current terminal console emulator background color is used.</remarks>
   public void DisplayToConsole(string text, Color foregroundColor);

   /// <summary>
   /// Displays text line to the terminal console emulator.
   /// </summary>
   /// <param name="text">The message to display.</param>
   /// <param name="foregroundColor">The foreground color of the text.</param>
   /// <remarks>The current terminal console emulator background color is used.</remarks>
   public void DisplayLineToConsole(string text, Color foregroundColor);

   /// <summary>
   /// Displays text lines to the terminal console emulator.
   /// </summary>
   /// <param name="lines">The message lines to display.</param>
   /// <param name="foregroundColor">The foreground color of the text.</param>
   public void DisplayLinesToConsole(IEnumerable<string> lines, Color foregroundColor);

   /// <summary>
   /// Displays text line to the terminal console emulator.
   /// </summary>
   /// <param name="text">The message to display.</param>
   /// <param name="foregroundColor">The foreground color of the text.</param>
   /// <param name="backgroundColor">The background color of the text.</param>
   public void DisplayToConsole(string text, Color foregroundColor, Color backgroundColor);

   /// <summary>
   /// Displays text to the terminal console emulator.
   /// </summary>
   /// <param name="text">The message to display.</param>
   /// <param name="foregroundColor">The foreground color of the text.</param>
   /// <param name="backgroundColor">The background color of the text.</param>
   public void DisplayLineToConsole(string text, Color foregroundColor, Color backgroundColor);

   /// <summary>
   /// Displays text lines to the terminal console emulator.
   /// </summary>
   /// <param name="lines">The message lines to display.</param>
   /// <param name="foregroundColor">The foreground color of the text.</param>
   /// <param name="backgroundColor">The background color of the text.</param>
   public void DisplayLinesToConsole(IEnumerable<string> lines, Color foregroundColor, Color backgroundColor);
}