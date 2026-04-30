#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="McpCordPackageTests.cs">
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

using FluentAssertions;
using NSubstitute;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP;
using Org.Edgerunner.Mud.MCP.Interfaces;
using Org.Edgerunner.Mud.MCP.Packages;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class McpCordPackageTests
{
    private static Message CordOpenMsg(string id = "I12345", string type = "whiteboard") =>
        new("mcp-cord-open", "key123", new Dictionary<string, string>
        {
            ["_id:"] = id,
            ["_type:"] = type
        });

    [Fact]
    public void ProcessMessage_CordOpen_ReturnsTrueAndCreatesCord()
    {
        var client = Substitute.For<IClientTerminal>();
        var package = new McpCordPackage(new Dictionary<string, IMcpPackage>());

        var result = package.ProcessMessage(client, CordOpenMsg());

        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessMessage_CordClosed_ReturnsTrueAndRemovesCord()
    {
        var client = Substitute.For<IClientTerminal>();
        var package = new McpCordPackage(new Dictionary<string, IMcpPackage>());
        package.ProcessMessage(client, CordOpenMsg());

        var closeMsg = new Message("mcp-cord-closed", "key123", new Dictionary<string, string>
        {
            ["_id:"] = "I12345"
        });
        var result = package.ProcessMessage(client, closeMsg);

        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessMessage_CordMessage_RoutesToRegisteredHandler()
    {
        var client = Substitute.For<IClientTerminal>();
        var handler = Substitute.For<IMcpPackage>();
        handler.ProcessMessage(Arg.Any<IClientTerminal>(), Arg.Any<Message>()).Returns(true);

        var package = new McpCordPackage(new Dictionary<string, IMcpPackage>
        {
            ["whiteboard"] = handler
        });

        package.ProcessMessage(client, CordOpenMsg());

        package.ProcessMessage(client, new Message("mcp-cord", "key123", new Dictionary<string, string>
        {
            ["_id:"] = "I12345",
            ["_message:"] = "draw",
            ["color:"] = "red"
        }));

        handler.Received(1).ProcessMessage(client, Arg.Is<Message>(m => m.Name == "draw"));
    }

    [Fact]
    public void ProcessMessage_CordMessage_NoHandlerRegistered_DoesNotThrow()
    {
        var client = Substitute.For<IClientTerminal>();
        var package = new McpCordPackage(new Dictionary<string, IMcpPackage>());
        package.ProcessMessage(client, CordOpenMsg());

        var act = () => package.ProcessMessage(client, new Message("mcp-cord", "key123",
            new Dictionary<string, string> { ["_id:"] = "I12345", ["_message:"] = "draw" }));

        act.Should().NotThrow();
    }

    [Fact]
    public void ProcessMessage_CordMessage_ClosedCord_ReturnsFalse()
    {
        var client = Substitute.For<IClientTerminal>();
        var package = new McpCordPackage(new Dictionary<string, IMcpPackage>());
        package.ProcessMessage(client, CordOpenMsg());
        package.ProcessMessage(client, new Message("mcp-cord-closed", "key123",
            new Dictionary<string, string> { ["_id:"] = "I12345" }));

        var result = package.ProcessMessage(client, new Message("mcp-cord", "key123",
            new Dictionary<string, string> { ["_id:"] = "I12345", ["_message:"] = "draw" }));

        result.Should().BeFalse();
    }

    [Fact]
    public void CanHandleMessage_CordMessages_ReturnsTrue()
    {
        var package = new McpCordPackage(new Dictionary<string, IMcpPackage>());

        package.CanHandleMessage(new Message("mcp-cord-open", "key", new Dictionary<string, string>())).Should().BeTrue();
        package.CanHandleMessage(new Message("mcp-cord", "key", new Dictionary<string, string>())).Should().BeTrue();
        package.CanHandleMessage(new Message("mcp-cord-closed", "key", new Dictionary<string, string>())).Should().BeTrue();
    }
}
