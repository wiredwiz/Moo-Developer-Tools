using FastColoredTextBoxNS.Types;
using FluentAssertions;
using Org.Edgerunner.Moo.Editor.Autocomplete;
using Org.Edgerunner.Mud.Common.Querying;
using Xunit;

namespace Org.Edgerunner.Moo.Editor.Tests;

public class MemberCompletionControllerTests
{
   private sealed class FakeQueryProvider : IMooWorldQueryProvider
   {
      public List<MooVerbSummary> Verbs { get; } = new();
      public List<MooPropertySummary> Properties { get; } = new();
      public int VerbCalls;
      public int PropertyCalls;
      public TaskCompletionSource? Gate;
      public Exception? ThrowOnVerbs;

      public async Task<IReadOnlyList<MooVerbSummary>> GetVerbsAsync(MooObjectId objectId, CancellationToken cancellationToken)
      {
         Interlocked.Increment(ref VerbCalls);
         if (Gate is not null)
            await Gate.Task.WaitAsync(cancellationToken);
         if (ThrowOnVerbs is not null)
            throw ThrowOnVerbs;
         return Verbs;
      }

      public async Task<IReadOnlyList<MooPropertySummary>> GetPropertiesAsync(MooObjectId objectId, CancellationToken cancellationToken)
      {
         Interlocked.Increment(ref PropertyCalls);
         if (Gate is not null)
            await Gate.Task.WaitAsync(cancellationToken);
         return Properties;
      }

      public Task<IReadOnlyList<MooObjectSummary>> GetCoreObjectsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetChildrenAsync(MooObjectId objectId, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(MooObjectId owner, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooObjectId?> GetParentAsync(MooObjectId objectId, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbInfo?> GetVerbInfoAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooPropertyInfo?> GetPropertyInfoAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbCode?> GetVerbCodeAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooPropertyValue?> GetPropertyValueAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbDocumentation?> GetVerbDocumentationAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<string>> GetPropertyDocumentationAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
   }

   private static MemberCompletionController CreateController(
      FakeQueryProvider provider,
      MooObjectId? contextObject = null,
      Action? refresh = null)
   {
      return new MemberCompletionController(
         () => provider,
         () => contextObject,
         action => action(),          // immediate marshal: deterministic single-threaded tests
         refresh ?? (() => { }));
   }

   private static void WaitForCache(MemberCompletionController controller, string linePrefix)
   {
      // The fetch completes asynchronously; poll briefly until the marshalled cache write lands.
      SpinWait.SpinUntil(() => controller.GetMemberItems(linePrefix).Count > 0, TimeSpan.FromSeconds(5))
              .Should().BeTrue("the fetched members should land in the cache");
   }

   [Fact]
   public void GetMemberItems_returns_empty_for_non_member_context()
   {
      var provider = new FakeQueryProvider();
      using var controller = CreateController(provider);

      controller.GetMemberItems("x = 5").Should().BeEmpty();
      provider.VerbCalls.Should().Be(0);
      provider.PropertyCalls.Should().Be(0);
   }

   [Fact]
   public void GetMemberItems_returns_empty_when_operand_unresolved()
   {
      var provider = new FakeQueryProvider();
      using var controller = CreateController(provider, contextObject: null);

      controller.GetMemberItems("this:te").Should().BeEmpty();
      provider.VerbCalls.Should().Be(0);
   }

   [Fact]
   public void GetMemberItems_returns_empty_without_a_provider()
   {
      using var controller = new MemberCompletionController(
         () => null, () => new MooObjectId(5), action => action(), () => { });

      controller.GetMemberItems("this:te").Should().BeEmpty();
   }

   [Fact]
   public void Verb_context_fetches_verbs_and_caches_flattened_aliases()
   {
      var provider = new FakeQueryProvider();
      provider.Verbs.Add(new MooVerbSummary(new[] { "tell", "g*et" }, new MooObjectId(1)));
      provider.Verbs.Add(new MooVerbSummary(new[] { "drop" }, new MooObjectId(1)));
      using var controller = CreateController(provider, contextObject: new MooObjectId(5));

      controller.GetMemberItems("this:").Should().BeEmpty("first call only starts the fetch");
      WaitForCache(controller, "this:");

      var items = controller.GetMemberItems("this:");
      items.Select(i => i.Text).Should().BeEquivalentTo("drop", "get", "tell");
      items.Should().AllSatisfy(i => i.ImageIndex.Should().Be((int)CompletionIconCategory.Verb));
      provider.VerbCalls.Should().Be(1, "subsequent calls must be served from the cache");
   }

   [Fact]
   public void Property_context_fetches_properties_with_property_icon()
   {
      var provider = new FakeQueryProvider();
      provider.Properties.Add(new MooPropertySummary("name", new MooObjectId(1)));
      using var controller = CreateController(provider, contextObject: new MooObjectId(5));

      controller.GetMemberItems("this.");
      WaitForCache(controller, "this.");

      var items = controller.GetMemberItems("this.");
      items.Single().Text.Should().Be("name");
      items.Single().ImageIndex.Should().Be((int)CompletionIconCategory.Property);
   }

   [Fact]
   public void Core_reference_context_fetches_properties_of_object_zero_with_core_icon()
   {
      var provider = new FakeQueryProvider();
      provider.Properties.Add(new MooPropertySummary("room", new MooObjectId(0)));
      using var controller = CreateController(provider);

      controller.GetMemberItems("$ro");
      WaitForCache(controller, "$ro");

      var items = controller.GetMemberItems("$ro");
      items.Single().Text.Should().Be("room");
      items.Single().ImageIndex.Should().Be((int)CompletionIconCategory.CoreReference);
   }

   [Fact]
   public void Completed_fetch_invokes_the_menu_refresh_callback()
   {
      var provider = new FakeQueryProvider();
      provider.Properties.Add(new MooPropertySummary("name", new MooObjectId(1)));
      var refreshed = 0;
      using var controller = CreateController(provider, new MooObjectId(5), () => Interlocked.Increment(ref refreshed));

      controller.GetMemberItems("this.");
      WaitForCache(controller, "this.");

      refreshed.Should().BeGreaterThan(0);
   }

   [Fact]
   public void Provider_failure_is_swallowed_and_yields_no_items()
   {
      var provider = new FakeQueryProvider { ThrowOnVerbs = new TimeoutException("SDWC timed out") };
      using var controller = CreateController(provider, contextObject: new MooObjectId(5));

      controller.GetMemberItems("this:");
      Thread.Sleep(250); // allow the faulted fetch to finish

      controller.GetMemberItems("this:x").Should().BeEmpty();
   }

   [Fact]
   public void New_context_cancels_the_inflight_fetch()
   {
      var provider = new FakeQueryProvider { Gate = new TaskCompletionSource() };
      provider.Properties.Add(new MooPropertySummary("name", new MooObjectId(1)));
      using var controller = CreateController(provider, contextObject: new MooObjectId(5));

      controller.GetMemberItems("this:");    // starts verb fetch, parked on the gate
      controller.GetMemberItems("#7.");      // different key: must cancel the verb fetch and start this one
      provider.Gate.SetResult();             // release both

      SpinWait.SpinUntil(() => controller.GetMemberItems("#7.").Count > 0, TimeSpan.FromSeconds(5))
              .Should().BeTrue();
      controller.GetMemberItems("this:").Should().BeEmpty("the cancelled verb fetch must not populate the cache");
   }

   [Fact]
   public void Repeated_trigger_for_same_key_does_not_start_a_second_fetch()
   {
      var provider = new FakeQueryProvider { Gate = new TaskCompletionSource() };
      using var controller = CreateController(provider, contextObject: new MooObjectId(5));

      controller.GetMemberItems("this:");
      controller.GetMemberItems("this:t");
      controller.GetMemberItems("this:te");

      provider.VerbCalls.Should().Be(1);
      provider.Gate.SetResult();
   }
}
