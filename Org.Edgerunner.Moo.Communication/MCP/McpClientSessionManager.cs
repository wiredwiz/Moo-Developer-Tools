#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="McpClientSessionManager.cs">
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

using System.Globalization;

namespace Org.Edgerunner.Moo.Communication.MCP;

/// <summary>
/// A class responsible for managing MCP protocol interactions.
/// </summary>
/// <seealso cref="Org.Edgerunner.Moo.Communication.MCP.IMcpProtocolHandler" />
public class McpClientSessionManager
{
   public McpClientSessionManager(double minimumVersion, double maximumVersion, IEnumerable<IMcpPackage> supportedPackages)
   {
      MinimumVersion = minimumVersion;
      MaximumVersion = maximumVersion;
      SupportedPackages = supportedPackages.ToList();
   }

   /// <summary>
   /// Gets or sets the minimum protocol version.
   /// </summary>
   /// <value>
   /// The minimum protocol version.
   /// </value>
   public double MinimumVersion { get; set; }

   /// <summary>
   /// Gets or sets the maximum supported protocol version.
   /// </summary>
   /// <value>
   /// The maximum supported protocol version.
   /// </value>
   public double MaximumVersion { get; set; }

   /// <summary>
   /// Gets or sets the supported MCP packages.
   /// </summary>
   /// <value>
   /// The supported MCP packages.
   /// </value>
   public List<IMcpPackage> SupportedPackages { get; set; }

   /// <summary>
   /// Determines whether this instance can handle the message.
   /// </summary>
   /// <param name="message">The message to analyze.</param>
   /// <returns></returns>
   /// <exception cref="System.NotImplementedException"></exception>
   public bool CanHandleMessage(MCP.Message message)
   {
      if (message.Name.ToLowerInvariant() != "mcp")
         return false;

      if (message.Data.Count == 0 || !message.Data.ContainsKey("version:"))
         return false;

      return true;
   }

   /// <summary>
   /// Processes the message.
   /// </summary>
   /// <param name="message">The message to process.</param>
   /// <returns>
   /// The resulting list of session messages to send or display.
   /// </returns>
   /// <exception cref="System.NotImplementedException"></exception>
   public IMcpSession? ProcessMessage(MCP.Message message)
   {
      if (message.Name.ToLowerInvariant() != "mcp")
         throw new InvalidMcpMessageFormatException($"Message \"{message}\" does not resemble a properly formatted MCP message.");

      if (message.Data.Count == 0)
         throw new InvalidMcpMessageFormatException($"Message is missing data key/value pairs.");

      if (!message.Data.ContainsKey("version:"))
         throw new InvalidMcpMessageFormatException($"Message is missing a \"version:\" key.");

      if (!message.Data.ContainsKey("to:"))
         throw new InvalidMcpMessageFormatException($"Message is missing a \"to:\" key.");

      var minVersion = 0.0;
      var maxVersion = 0.0;

      if (!double.TryParse(message.Data["version:"],
                           System.Globalization.NumberStyles.AllowDecimalPoint,
                           CultureInfo.GetCultureInfo("en-US"),
                           out minVersion))
         throw new InvalidMcpMessageFormatException($"Value \"{message.Data["version:"]}\" does not appear to be a valid version number.");

      if (!double.TryParse(message.Data["to:"],
                           System.Globalization.NumberStyles.AllowDecimalPoint,
                           CultureInfo.GetCultureInfo("en-US"),
                           out maxVersion))
         throw new InvalidMcpMessageFormatException($"Value \"{message.Data["to:"]}\" does not appear to be a valid version number.");

      // If there is no compatible version between our ranges
      // return a null result.
      if (MaximumVersion < minVersion || maxVersion < MinimumVersion)
         return null;

      var sessionVersion = Math.Min(maxVersion, MaximumVersion);

      return new McpClientSession(this, McpUtils.GenerateSessionKey(12), sessionVersion);
   }
}