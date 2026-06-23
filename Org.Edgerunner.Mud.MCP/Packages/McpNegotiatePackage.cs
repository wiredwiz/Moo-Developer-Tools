#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="McpNegotiatePackage.cs">
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

using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP.Interfaces;

namespace Org.Edgerunner.Mud.MCP.Packages;

/// <summary>
/// Implements the required mcp-negotiate MCP package, handling capability exchange.
/// </summary>
public class McpNegotiatePackage : IMcpPackage
{
   private McpClientSession? _session;
   private readonly Dictionary<string, IMcpPackage> _registeredPackages;

   /// <summary>
   /// Initializes a new instance of the <see cref="McpNegotiatePackage"/> class.
   /// </summary>
   /// <param name="registeredPackages">The client's registered package registry, shared with the dispatcher.</param>
   public McpNegotiatePackage(Dictionary<string, IMcpPackage> registeredPackages)
   {
      _registeredPackages = registeredPackages;
   }

   /// <inheritdoc/>
   public string Name { get; set; } = "mcp-negotiate";

   /// <inheritdoc/>
   public double MinimumVersion { get; set; } = 1.0;

   /// <inheritdoc/>
   public double MaximumVersion { get; set; } = 2.0;

   /// <inheritdoc/>
   public void SetSession(McpClientSession session) => _session = session;

   /// <inheritdoc/>
   public bool CanHandleMessage(Message message)
   {
      var name = message.Name.ToLowerInvariant();
      return name is "mcp-negotiate-can" or "mcp-negotiate-end";
   }

   /// <inheritdoc/>
   public bool ProcessMessage(IClientTerminal client, Message message)
   {
      return message.Name.ToLowerInvariant() switch
      {
         "mcp-negotiate-can" => ProcessNegotiateCan(client, message),
         "mcp-negotiate-end" => ProcessNegotiateEnd(),
         _ => false
      };
   }

   private bool ProcessNegotiateCan(IClientTerminal client, Message message)
   {
      if (_session == null) return false;

      if (!message.Data.TryGetValue("package:", out var packageName)) return true;
      if (!message.Data.TryGetValue("min-version:", out var minStr)) return true;
      if (!message.Data.TryGetValue("max-version:", out var maxStr)) return true;

      if (!double.TryParse(minStr, out var serverMin) ||
          !double.TryParse(maxStr, out var serverMax))
         return true;

      if (!_registeredPackages.TryGetValue(packageName.ToLowerInvariant(), out var pkg))
         return true;

      if (pkg.MaximumVersion < serverMin || serverMax < pkg.MinimumVersion)
         return true;

      _session.SupportedPackages[packageName.ToLowerInvariant()] = pkg;

      if (pkg is IPackageNegotiationListener listener)
         listener.OnPackageSupported(client);

      return true;
   }

   private bool ProcessNegotiateEnd()
   {
      if (_session != null)
         _session.IsNegotiationComplete = true;
      return true;
   }

   /// <inheritdoc/>
   public void Reset() { }

   /// <inheritdoc/>
   public void OnDisconnected() { }

   /// <inheritdoc/>
   public void Dispose() { }
}
