using FluentAssertions;
using NSubstitute;
using Org.Edgerunner.Mud.Communication.Interfaces;
using Org.Edgerunner.Mud.MCP;
using Org.Edgerunner.Mud.MCP.Interfaces;
using Org.Edgerunner.Mud.MCP.Packages;
using Xunit;

namespace Org.Edgerunner.Mud.MCP.Tests;

public class NegotiationListenerTests
{
   private sealed class ListeningPackage : IMcpPackage, IPackageNegotiationListener
   {
      public int SupportedCallCount { get; private set; }
      public IClientTerminal? LastClient { get; private set; }

      public string Name { get; set; } = "edgerunner-org-moo-query";
      public double MinimumVersion { get; set; } = 1.0;
      public double MaximumVersion { get; set; } = 1.0;
      public void SetSession(McpClientSession session) { }
      public bool CanHandleMessage(Message message) => false;
      public bool ProcessMessage(IClientTerminal client, Message message) => false;
      public void Reset() { }
      public void OnDisconnected() { }
      public void Dispose() { }

      public void OnPackageSupported(IClientTerminal client)
      {
         SupportedCallCount++;
         LastClient = client;
      }
   }

   private sealed class PlainPackage : IMcpPackage
   {
      public string Name { get; set; } = "dns-org-mud-moo-simpleedit";
      public double MinimumVersion { get; set; } = 1.0;
      public double MaximumVersion { get; set; } = 1.0;
      public void SetSession(McpClientSession session) { }
      public bool CanHandleMessage(Message message) => false;
      public bool ProcessMessage(IClientTerminal client, Message message) => false;
      public void Reset() { }
      public void OnDisconnected() { }
      public void Dispose() { }
   }

   private static McpClientSession CreateSession()
   {
      var manager = new McpClientSessionManager(new Version(2, 1), new Version(2, 1), new List<IMcpPackage>());
      return new McpClientSession(manager, "KEY123", new Version(2, 1));
   }

   private static Message NegotiateCan(string package, string min = "1.0", string max = "1.0") =>
      new("mcp-negotiate-can", "KEY123", new Dictionary<string, string>
      {
         ["package:"] = package,
         ["min-version:"] = min,
         ["max-version:"] = max
      });

   private static (McpNegotiatePackage Negotiate, McpClientSession Session) CreateNegotiator(params IMcpPackage[] packages)
   {
      var registry = packages.ToDictionary(p => p.Name.ToLowerInvariant(), p => p);
      var negotiate = new McpNegotiatePackage(registry);
      var session = CreateSession();
      negotiate.SetSession(session);
      return (negotiate, session);
   }

   [Fact]
   public void NegotiateCan_CompatiblePackage_NotifiesListenerWithClient()
   {
      var package = new ListeningPackage();
      var (negotiate, session) = CreateNegotiator(package);
      var client = Substitute.For<IClientTerminal>();

      negotiate.ProcessMessage(client, NegotiateCan(package.Name));

      package.SupportedCallCount.Should().Be(1);
      package.LastClient.Should().BeSameAs(client);
      session.SupportedPackages.Should().ContainKey(package.Name);
   }

   [Fact]
   public void NegotiateCan_IncompatibleVersions_DoesNotNotify()
   {
      var package = new ListeningPackage();
      var (negotiate, session) = CreateNegotiator(package);
      var client = Substitute.For<IClientTerminal>();

      negotiate.ProcessMessage(client, NegotiateCan(package.Name, "2.0", "3.0"));

      package.SupportedCallCount.Should().Be(0);
      session.SupportedPackages.Should().BeEmpty();
   }

   [Fact]
   public void NegotiateCan_UnregisteredPackage_DoesNotNotify()
   {
      var package = new ListeningPackage();
      var (negotiate, _) = CreateNegotiator(package);
      var client = Substitute.For<IClientTerminal>();

      negotiate.ProcessMessage(client, NegotiateCan("dns-org-mud-moo-somethingelse"));

      package.SupportedCallCount.Should().Be(0);
   }

   [Fact]
   public void NegotiateCan_PackageWithoutListenerInterface_StillRecordsSupport()
   {
      var package = new PlainPackage();
      var (negotiate, session) = CreateNegotiator(package);
      var client = Substitute.For<IClientTerminal>();

      negotiate.ProcessMessage(client, NegotiateCan(package.Name));

      session.SupportedPackages.Should().ContainKey(package.Name);
   }
}
