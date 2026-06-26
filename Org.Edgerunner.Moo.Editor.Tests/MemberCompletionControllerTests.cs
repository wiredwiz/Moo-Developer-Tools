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
      public int PropValueCalls;
      public int CurrentPlayerCalls;
      public MooObjectId? LastVerbQueryObjectId;
      public TaskCompletionSource? Gate;
      public Exception? ThrowOnVerbs;
      public MooObjectId? CurrentPlayer;
      public Dictionary<string, MooPropertyValue?> PropertyValues { get; } = new(StringComparer.OrdinalIgnoreCase);

      public async Task<MooObjectId?> GetCurrentPlayerAsync(CancellationToken cancellationToken)
      {
         Interlocked.Increment(ref CurrentPlayerCalls);
         if (Gate is not null)
            await Gate.Task.WaitAsync(cancellationToken);
         return CurrentPlayer;
      }

      public async Task<IReadOnlyList<MooVerbSummary>> GetVerbsAsync(MooObjectId objectId, CancellationToken cancellationToken)
      {
         LastVerbQueryObjectId = objectId;
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

      public async Task<MooPropertyValue?> GetPropertyValueAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken)
      {
         Interlocked.Increment(ref PropValueCalls);
         if (Gate is not null)
            await Gate.Task.WaitAsync(cancellationToken);
         PropertyValues.TryGetValue(propName, out var value);
         return value;
      }

      public Task<IReadOnlyList<MooObjectSummary>> GetCoreObjectsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetChildrenAsync(MooObjectId objectId, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(MooObjectId owner, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooObjectId?> GetParentAsync(MooObjectId objectId, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbInfo?> GetVerbInfoAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooPropertyInfo?> GetPropertyInfoAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbCode?> GetVerbCodeAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<MooVerbDocumentation?> GetVerbDocumentationAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken) => throw new NotImplementedException();
      public Task<IReadOnlyList<string>> GetPropertyDocumentationAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken) => throw new NotImplementedException();
   }

   private static MemberCompletionController CreateController(
      FakeQueryProvider provider,
      MooObjectId? contextObject = null,
      Action? refresh = null,
      Action<string, Exception?>? diagnostic = null)
   {
      return new MemberCompletionController(
         () => provider,
         () => contextObject,
         action => action(),          // immediate marshal: deterministic single-threaded tests
         refresh ?? (() => { }),
         diagnostic: diagnostic);
   }

   private static MemberCompletionController CreateControllerWithQueuedMarshal(
      FakeQueryProvider provider,
      List<Action> marshalQueue,
      MooObjectId? contextObject = null,
      Action? refresh = null)
   {
      return new MemberCompletionController(
         () => provider,
         () => contextObject,
         action => marshalQueue.Add(action),  // queuing marshal: deferred execution for I1 regression tests
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

   // Core-name ($name) operand tests

   [Fact]
   public void CoreName_verb_context_resolves_via_property_value_and_fetches_verbs()
   {
      var provider = new FakeQueryProvider();
      provider.PropertyValues["network"] = new MooPropertyValue(1, "#62");
      provider.Verbs.Add(new MooVerbSummary(new[] { "tell" }, new MooObjectId(62)));
      using var controller = CreateController(provider);

      controller.GetMemberItems("$network:").Should().BeEmpty("first call only starts the resolve");
      WaitForCache(controller, "$network:");

      var items = controller.GetMemberItems("$network:");
      items.Select(i => i.Text).Should().Contain("tell");
      items.Should().AllSatisfy(i => i.ImageIndex.Should().Be((int)CompletionIconCategory.Verb));
      provider.PropValueCalls.Should().Be(1);
      provider.VerbCalls.Should().Be(1);
      provider.LastVerbQueryObjectId.Should().Be(new MooObjectId(62));
   }

   [Fact]
   public void CoreName_property_context_resolves_and_fetches_properties_with_property_icon()
   {
      var provider = new FakeQueryProvider();
      provider.PropertyValues["network"] = new MooPropertyValue(1, "#62");
      provider.Properties.Add(new MooPropertySummary("ip", new MooObjectId(62)));
      using var controller = CreateController(provider);

      controller.GetMemberItems("$network.");
      WaitForCache(controller, "$network.");

      var items = controller.GetMemberItems("$network.");
      items.Select(i => i.Text).Should().Contain("ip");
      items.Should().AllSatisfy(i => i.ImageIndex.Should().Be((int)CompletionIconCategory.Property),
         "Property context must use Property icon, not CoreReference icon");
   }

   [Fact]
   public void CoreName_resolution_is_cached_so_property_value_not_queried_again()
   {
      var provider = new FakeQueryProvider();
      provider.PropertyValues["network"] = new MooPropertyValue(1, "#62");
      provider.Verbs.Add(new MooVerbSummary(new[] { "tell" }, new MooObjectId(62)));
      provider.Properties.Add(new MooPropertySummary("ip", new MooObjectId(62)));
      using var controller = CreateController(provider);

      // Resolve the verb context first
      controller.GetMemberItems("$network:");
      WaitForCache(controller, "$network:");

      // Now request the property context; name is already resolved so PropValueCalls must not grow
      controller.GetMemberItems("$network.");
      SpinWait.SpinUntil(() => controller.GetMemberItems("$network.").Count > 0, TimeSpan.FromSeconds(5));

      provider.PropValueCalls.Should().Be(1, "the core-name cache should prevent a second #0 property-value query");
   }

   [Fact]
   public void CoreName_non_object_property_value_yields_empty_and_no_member_fetch()
   {
      var provider = new FakeQueryProvider();
      provider.PropertyValues["mylist"] = new MooPropertyValue(4, "{1, 2, 3}"); // LIST type, not OBJ
      using var controller = CreateController(provider);

      controller.GetMemberItems("$mylist:");
      Thread.Sleep(250); // allow the faulted resolve to finish

      controller.GetMemberItems("$mylist:").Should().BeEmpty();
      provider.VerbCalls.Should().Be(0);
      provider.PropertyCalls.Should().Be(0);
   }

   [Fact]
   public void CoreName_null_property_value_yields_empty_and_no_member_fetch()
   {
      var provider = new FakeQueryProvider();
      // "unknown" is not in PropertyValues, so GetPropertyValueAsync returns null
      using var controller = CreateController(provider);

      controller.GetMemberItems("$unknown:");
      Thread.Sleep(250);

      controller.GetMemberItems("$unknown:").Should().BeEmpty();
      provider.VerbCalls.Should().Be(0);
   }

   [Fact]
   public void CoreName_negative_object_number_yields_empty_and_no_member_fetch()
   {
      var provider = new FakeQueryProvider();
      provider.PropertyValues["network"] = new MooPropertyValue(1, "#-1"); // unset corified value
      using var controller = CreateController(provider);

      controller.GetMemberItems("$network:");
      Thread.Sleep(250);

      controller.GetMemberItems("$network:").Should().BeEmpty("negative object numbers must be rejected");
      provider.VerbCalls.Should().Be(0);
      provider.PropertyCalls.Should().Be(0);
   }

   [Fact]
   public void CoreName_repeated_trigger_does_not_start_a_second_prop_value_fetch()
   {
      // Dedup test (I2): with a gated provider the first GetMemberItems parks the core-name
      // resolve on the gate; a second call for the same $name must not start another fetch.
      var provider = new FakeQueryProvider
      {
         Gate = new TaskCompletionSource()
      };
      provider.PropertyValues["network"] = new MooPropertyValue(1, "#62");
      provider.Verbs.Add(new MooVerbSummary(new[] { "tell" }, new MooObjectId(62)));
      using var controller = CreateController(provider);

      controller.GetMemberItems("$network:");   // starts resolve, parked on gate
      controller.GetMemberItems("$network:t");  // same core name — must NOT start a second fetch

      provider.PropValueCalls.Should().Be(1, "the inflight marker must suppress the duplicate fetch");

      provider.Gate.SetResult();  // release the parked call

      WaitForCache(controller, "$network:");
      controller.GetMemberItems("$network:").Select(i => i.Text).Should().Contain("tell");
   }

   [Fact]
   public void CoreName_cancellation_releases_marker_so_retry_can_succeed()
   {
      // Catch-path marker-release test (covers the CATCH handler in FetchCoreNameAsync):
      // 1. Start $network: resolve — parks on gate.
      // 2. Switch to #7. — cancels the in-flight core resolve CTS.
      // 3. Release gate — the parked prop-value awaitable throws OperationCanceledException.
      //    The catch handler must clear _inflightCoreName so a subsequent $network: call can
      //    start a brand-new resolve.
      // 4. Assert $network: cache was NOT populated by the cancelled fetch.
      // 5. Assert a retry does start a new resolve (PropValueCalls reaches 2) and eventually
      //    populates the cache.
      var provider = new FakeQueryProvider
      {
         Gate = new TaskCompletionSource()
      };
      provider.PropertyValues["network"] = new MooPropertyValue(1, "#62");
      provider.Verbs.Add(new MooVerbSummary(new[] { "tell" }, new MooObjectId(62)));
      using var controller = CreateController(provider, contextObject: new MooObjectId(7));

      controller.GetMemberItems("$network:");  // parks prop-value call on gate
      controller.GetMemberItems("#7.");        // cancels the parked core resolve, starts member fetch

      provider.Gate.SetResult();  // release both the cancelled prop-value call and any subsequent calls

      // Wait for the #7. member fetch to land in its cache (proves the gate is fully drained).
      SpinWait.SpinUntil(() => controller.GetMemberItems("#7.").Count > 0, TimeSpan.FromSeconds(5));

      // The catch handler clears _inflightCoreName; once clear a new $network: resolve starts
      // immediately (gate is open so the immediate-marshal path completes synchronously).
      // WaitForCache polls GetMemberItems; the first successful poll triggers the retry resolve.
      WaitForCache(controller, "$network:");
      provider.PropValueCalls.Should().Be(2,
         "cancellation must release the inflight marker so a retry can start a new resolve");
      controller.GetMemberItems("$network:").Select(i => i.Text).Should().Contain("tell");
   }

   [Fact]
   public void CoreName_queued_marshal_cancellation_does_not_permanently_suppress_a_retry()
   {
      // udd-efk: under the consolidated extract->resolve->fetch pipeline, switching context cancels
      // the in-flight chain resolve. This test proves the in-flight chain marker is released even when
      // the cancellation interleaves with a deferred (queued) marshal, so a later identical $network:
      // request is NOT permanently suppressed and eventually yields its members.
      var provider = new FakeQueryProvider();
      provider.PropertyValues["network"] = new MooPropertyValue(1, "#62");
      provider.Verbs.Add(new MooVerbSummary(new[] { "tell" }, new MooObjectId(62)));
      provider.Properties.Add(new MooPropertySummary("name", new MooObjectId(7)));

      var marshalQueue = new List<Action>();
      using var controller = CreateControllerWithQueuedMarshal(
         provider, marshalQueue, contextObject: new MooObjectId(7));

      // (a) Start core-name resolve for $network: — completion lambda queued by the ungated provider.
      controller.GetMemberItems("$network:");
      SpinWait.SpinUntil(() => marshalQueue.Count > 0, TimeSpan.FromSeconds(5))
              .Should().BeTrue("a completion lambda should be queued by the ungated provider");

      // (b) Switch context — cancels the queued lambda's token, queues the #7. completion lambda.
      controller.GetMemberItems("#7.");

      // (c) Drain all queued actions (the cancelled $network: lambda must release its chain marker).
      foreach (var action in marshalQueue.ToList())
         action();
      marshalQueue.Clear();

      // (d) Retry $network: — must not be permanently suppressed. Drain any queued completion and
      //     confirm the members eventually appear.
      controller.GetMemberItems("$network:");
      foreach (var action in marshalQueue.ToList())
         action();
      marshalQueue.Clear();

      SpinWait.SpinUntil(
         () =>
         {
            controller.GetMemberItems("$network:");
            foreach (var action in marshalQueue.ToList())
               action();
            marshalQueue.Clear();
            return controller.GetMemberItems("$network:").Any(i => i.Text == "tell");
         },
         TimeSpan.FromSeconds(5)).Should().BeTrue("a retry after cancellation must not be permanently suppressed");
   }

   [Fact]
   public void MemberFetch_queued_marshal_cancellation_releases_inflight_marker_so_retry_can_succeed()
   {
      // I1 regression test for FetchAsync success-lambda path:
      // Same queueing-marshal technique, but targeting _inflightKey rather than _inflightCoreName.
      //
      // The cancellation trigger must cancel the CTS WITHOUT overwriting _inflightKey, otherwise
      // the unfixed code would not wedge (the key would be overwritten by the new context).
      // A core-name ($network:) call does exactly that: it sets _inflightCoreName and cancels the
      // CTS, leaving _inflightKey = (Verb, 5) intact.  So when the queued this: lambda runs with
      // the cancelled token, the unfixed early-return sees _inflightKey == key and returns without
      // clearing it — the wedge.
      //
      // Sequence:
      //   (a) GetMemberItems("this:") — GetVerbsAsync returns a completed task; FetchAsync
      //       runs to completion synchronously, queuing the success lambda before returning.
      //       _inflightKey = (Verb, 5).
      //   (b) GetMemberItems("$network:") — cancels the CTS (invalidating the this: lambda's
      //       token) and sets _inflightCoreName, leaving _inflightKey = (Verb, 5) intact.
      //       FetchCoreNameAsync also completes synchronously, queuing a second lambda.
      //   (c) Drain the queue — the this: lambda sees IsCancellationRequested while
      //       _inflightKey is still (Verb, 5); unfixed code early-returns WITHOUT clearing it.
      //       Fixed code clears _inflightKey first, then early-returns.
      //   (d) Retry GetMemberItems("this:"); assert VerbCalls reaches 2 (new fetch started —
      //       only possible if _inflightKey was cleared), drain, assert items appear.
      var provider = new FakeQueryProvider();
      provider.Verbs.Add(new MooVerbSummary(new[] { "get" }, new MooObjectId(5)));
      provider.PropertyValues["network"] = new MooPropertyValue(1, "#62");
      // Add verbs for #62 so FetchCoreNameAsync succeeds without error (does not affect VerbCalls
      // for key (Verb, 5) since it queries object #62, not object #5).
      provider.Verbs.Add(new MooVerbSummary(new[] { "tell" }, new MooObjectId(62)));

      var marshalQueue = new List<Action>();
      using var controller = CreateControllerWithQueuedMarshal(
         provider, marshalQueue, contextObject: new MooObjectId(5));

      // (a) Start member fetch for this: (object #5, Verb context) — success lambda queued immediately.
      controller.GetMemberItems("this:");
      SpinWait.SpinUntil(() => marshalQueue.Count > 0, TimeSpan.FromSeconds(5))
              .Should().BeTrue("success lambda should be queued synchronously by the ungated provider");

      // (b) $network: cancels the this: lambda's token, sets _inflightCoreName, leaves _inflightKey
      //     = (Verb, 5) intact.  FetchCoreNameAsync also completes synchronously, queuing its lambda.
      controller.GetMemberItems("$network:");

      // (c) Drain all queued actions.
      var snapshot = marshalQueue.ToList();
      marshalQueue.Clear();
      foreach (var action in snapshot)
         action();

      // (d) Retry: single call — _inflightKey must be null (fixed) so a new fetch starts.
      //     VerbCalls was 2 after step (a)+(b) (this: fetch + $network: fetch for #62).
      //     After drain and retry it must be 3.
      var verbCallsAfterSetup = provider.VerbCalls;
      var retryResult = controller.GetMemberItems("this:");
      retryResult.Should().BeEmpty("the cache must be empty and the retry must start a new fetch");
      provider.VerbCalls.Should().Be(verbCallsAfterSetup + 1,
         "cancellation inside the success lambda must release _inflightKey so a retry starts a new fetch");

      // Drain the retry's queued lambda and verify items now appear.
      var retrySnapshot = marshalQueue.ToList();
      marshalQueue.Clear();
      foreach (var action in retrySnapshot)
         action();

      controller.GetMemberItems("this:").Select(i => i.Text).Should().Contain("get",
         "after draining the retry lambda the cache should be populated");
   }

   // player / caller (current-player) operand tests

   [Theory]
   [InlineData("player:")]
   [InlineData("caller:")]
   public void CurrentPlayer_verb_context_resolves_via_query_and_fetches_verbs(string linePrefix)
   {
      var provider = new FakeQueryProvider { CurrentPlayer = new MooObjectId(62) };
      provider.Verbs.Add(new MooVerbSummary(new[] { "tell" }, new MooObjectId(62)));
      using var controller = CreateController(provider);

      controller.GetMemberItems(linePrefix).Should().BeEmpty("first call only starts the resolve");
      WaitForCache(controller, linePrefix);

      var items = controller.GetMemberItems(linePrefix);
      items.Select(i => i.Text).Should().Contain("tell");
      items.Should().AllSatisfy(i => i.ImageIndex.Should().Be((int)CompletionIconCategory.Verb));
      provider.CurrentPlayerCalls.Should().Be(1);
      provider.VerbCalls.Should().Be(1);
      provider.LastVerbQueryObjectId.Should().Be(new MooObjectId(62));
   }

   [Fact]
   public void CurrentPlayer_property_context_resolves_and_fetches_properties_with_property_icon()
   {
      var provider = new FakeQueryProvider { CurrentPlayer = new MooObjectId(62) };
      provider.Properties.Add(new MooPropertySummary("name", new MooObjectId(62)));
      using var controller = CreateController(provider);

      controller.GetMemberItems("player.");
      WaitForCache(controller, "player.");

      var items = controller.GetMemberItems("player.");
      items.Select(i => i.Text).Should().Contain("name");
      items.Should().AllSatisfy(i => i.ImageIndex.Should().Be((int)CompletionIconCategory.Property));
   }

   [Fact]
   public void CurrentPlayer_resolution_is_cached_so_player_query_not_repeated()
   {
      var provider = new FakeQueryProvider { CurrentPlayer = new MooObjectId(62) };
      provider.Verbs.Add(new MooVerbSummary(new[] { "tell" }, new MooObjectId(62)));
      provider.Properties.Add(new MooPropertySummary("name", new MooObjectId(62)));
      using var controller = CreateController(provider);

      // Resolve the verb context first (caches the player id).
      controller.GetMemberItems("player:");
      WaitForCache(controller, "player:");

      // caller maps to the same current player; the cached id must avoid a second query.
      controller.GetMemberItems("caller.");
      SpinWait.SpinUntil(() => controller.GetMemberItems("caller.").Count > 0, TimeSpan.FromSeconds(5));

      provider.CurrentPlayerCalls.Should().Be(1, "the current-player cache should prevent a second player query");
   }

   [Fact]
   public void CurrentPlayer_null_result_yields_empty_and_no_member_fetch()
   {
      var provider = new FakeQueryProvider { CurrentPlayer = null };
      using var controller = CreateController(provider);

      controller.GetMemberItems("player:");
      Thread.Sleep(250); // allow the resolve to finish

      controller.GetMemberItems("player:").Should().BeEmpty();
      provider.VerbCalls.Should().Be(0);
      provider.PropertyCalls.Should().Be(0);
   }

   // Inherited-member origin tests

   [Fact]
   public void Verb_items_use_VerbInherited_icon_only_for_inherited_origin()
   {
      var provider = new FakeQueryProvider();
      provider.Verbs.Add(new MooVerbSummary(new[] { "get" }, new MooObjectId(5), MemberOrigin.Local));
      provider.Verbs.Add(new MooVerbSummary(new[] { "look" }, new MooObjectId(1), MemberOrigin.Inherited));
      provider.Verbs.Add(new MooVerbSummary(new[] { "drop" }, new MooObjectId(5), MemberOrigin.Unknown));
      using var controller = CreateController(provider, contextObject: new MooObjectId(5));

      controller.GetMemberItems("this:");
      WaitForCache(controller, "this:");
      var items = controller.GetMemberItems("this:");

      IconFor(items, "get").Should().Be((int)CompletionIconCategory.Verb, "local verbs keep the plain icon");
      IconFor(items, "drop").Should().Be((int)CompletionIconCategory.Verb, "unknown-origin verbs keep the plain icon");
      IconFor(items, "look").Should().Be((int)CompletionIconCategory.VerbInherited, "inherited verbs get the badged icon");
   }

   [Fact]
   public void Property_items_use_PropertyInherited_icon_only_for_inherited_origin()
   {
      var provider = new FakeQueryProvider();
      provider.Properties.Add(new MooPropertySummary("name", new MooObjectId(5), MemberOrigin.Local));
      provider.Properties.Add(new MooPropertySummary("description", new MooObjectId(1), MemberOrigin.Inherited));
      provider.Properties.Add(new MooPropertySummary("weight", new MooObjectId(5), MemberOrigin.Unknown));
      using var controller = CreateController(provider, contextObject: new MooObjectId(5));

      controller.GetMemberItems("this.");
      WaitForCache(controller, "this.");
      var items = controller.GetMemberItems("this.");

      IconFor(items, "name").Should().Be((int)CompletionIconCategory.Property, "local properties keep the plain icon");
      IconFor(items, "weight").Should().Be((int)CompletionIconCategory.Property, "unknown-origin properties keep the plain icon");
      IconFor(items, "description").Should().Be((int)CompletionIconCategory.PropertyInherited, "inherited properties get the badged icon");
   }

   [Fact]
   public void CoreReference_items_keep_core_icon_even_for_inherited_origin()
   {
      var provider = new FakeQueryProvider();
      provider.Properties.Add(new MooPropertySummary("room", new MooObjectId(0), MemberOrigin.Inherited));
      using var controller = CreateController(provider);

      controller.GetMemberItems("$ro");
      WaitForCache(controller, "$ro");
      var items = controller.GetMemberItems("$ro");

      IconFor(items, "room").Should().Be((int)CompletionIconCategory.CoreReference,
         "core-reference context keeps the CoreReference icon regardless of origin");
   }

   private static int IconFor(IReadOnlyList<AutocompleteItem> items, string text) =>
      items.Single(i => i.Text == text).ImageIndex;

   // Verb paren-insertion tests (udd-16x)

   [Fact]
   public void Verb_items_insert_call_parens_with_caret_between()
   {
      var provider = new FakeQueryProvider();
      provider.Verbs.Add(new MooVerbSummary(new[] { "tell" }, new MooObjectId(5)));
      using var controller = CreateController(provider, contextObject: new MooObjectId(5));

      controller.GetMemberItems("this:");
      WaitForCache(controller, "this:");

      var item = controller.GetMemberItems("this:").Single(i => i.Text == "tell");
      item.Compare("this:te");                       // records the fragment prefix
      item.GetTextForReplace().Should().Be("this:tell(^)",
         "selecting a verb must insert name() with the caret marker between the parens");
   }

   [Fact]
   public void Property_items_still_insert_the_bare_name()
   {
      var provider = new FakeQueryProvider();
      provider.Properties.Add(new MooPropertySummary("name", new MooObjectId(5)));
      using var controller = CreateController(provider, contextObject: new MooObjectId(5));

      controller.GetMemberItems("this.");
      WaitForCache(controller, "this.");

      var item = controller.GetMemberItems("this.").Single(i => i.Text == "name");
      item.Compare("this.na");
      item.GetTextForReplace().Should().Be("this.name", "properties are not callable and must insert a bare name");
   }

   [Fact]
   public void CoreReference_property_items_still_insert_the_bare_name()
   {
      var provider = new FakeQueryProvider();
      provider.Properties.Add(new MooPropertySummary("room", new MooObjectId(0)));
      using var controller = CreateController(provider);

      controller.GetMemberItems("$ro");
      WaitForCache(controller, "$ro");

      var item = controller.GetMemberItems("$ro").Single(i => i.Text == "room");
      item.Compare("$ro");
      item.GetTextForReplace().Should().Be("$room", "core-reference properties must insert a bare name");
   }

   // Core-reference ($) lists #0 verbs too (udd-wwk)

   [Fact]
   public void CoreReference_context_lists_both_properties_and_verbs()
   {
      var provider = new FakeQueryProvider();
      provider.Properties.Add(new MooPropertySummary("room", new MooObjectId(0)));
      provider.Verbs.Add(new MooVerbSummary(new[] { "do_login_command" }, new MooObjectId(0)));
      using var controller = CreateController(provider);

      controller.GetMemberItems("$").Should().BeEmpty("first call only starts the fetch");
      WaitForCache(controller, "$");

      var items = controller.GetMemberItems("$");

      // The #0 property carries the core-reference icon and inserts a bare name.
      var property = items.Single(i => i.Text == "room");
      property.ImageIndex.Should().Be((int)CompletionIconCategory.CoreReference);
      property.Compare("$ro");
      property.GetTextForReplace().Should().Be("$room", "core-reference properties insert a bare name");

      // The #0 verb carries the verb icon and inserts call parens with the caret between (udd-16x).
      var verb = items.Single(i => i.Text == "do_login_command");
      verb.ImageIndex.Should().Be((int)CompletionIconCategory.Verb);
      verb.Should().BeOfType<VerbCompletionItem>();
      verb.Compare("$do");
      verb.GetTextForReplace().Should().Be("$do_login_command(^)",
         "core-reference verbs are callable and insert name() with the caret between the parens");

      provider.PropertyCalls.Should().Be(1, "core reference fetches #0 properties");
      provider.VerbCalls.Should().Be(1, "core reference also fetches #0 verbs");
   }

   [Fact]
   public void CoreReference_same_name_property_and_verb_yields_two_entries()
   {
      // #0 may carry both a property foo ($foo reads it) and a verb foo ($foo() calls it).
      // Both must appear — FCTB does not dedup by text — each with its own icon and insertion form.
      var provider = new FakeQueryProvider();
      provider.Properties.Add(new MooPropertySummary("foo", new MooObjectId(0)));
      provider.Verbs.Add(new MooVerbSummary(new[] { "foo" }, new MooObjectId(0)));
      using var controller = CreateController(provider);

      controller.GetMemberItems("$foo");
      WaitForCache(controller, "$foo");

      var foos = controller.GetMemberItems("$foo").Where(i => i.Text == "foo").ToList();
      foos.Should().HaveCount(2, "a same-name property and verb must both be listed");
      foos.Select(i => i.ImageIndex).Should().BeEquivalentTo(new[]
      {
         (int)CompletionIconCategory.CoreReference,
         (int)CompletionIconCategory.Verb,
      });
      foos.Should().ContainSingle(i => i is VerbCompletionItem, "exactly one entry is the callable verb");
   }

   [Fact]
   public void CoreReference_merges_verbs_and_properties_into_one_alphabetical_run()
   {
      // $ lists #0 properties AND verbs as ONE alphabetical list with verbs and properties
      // INTERLEAVED — never all properties then all verbs (udd-7wl).
      var provider = new FakeQueryProvider();
      provider.Properties.Add(new MooPropertySummary("apple", new MooObjectId(0)));
      provider.Properties.Add(new MooPropertySummary("cherry", new MooObjectId(0)));
      provider.Verbs.Add(new MooVerbSummary(new[] { "banana" }, new MooObjectId(0)));
      using var controller = CreateController(provider);

      controller.GetMemberItems("$");
      WaitForCache(controller, "$");

      var names = controller.GetMemberItems("$").Select(i => i.Text).ToList();
      names.Should().Equal(new[] { "apple", "banana", "cherry" },
         "verbs and properties must be interleaved in one alphabetical run, not segmented props-then-verbs");
   }

   [Fact]
   public void CoreReference_same_name_property_sorts_before_its_verb()
   {
      // On a same-name tie the property (added first) precedes the verb — stable sort (udd-7wl).
      var provider = new FakeQueryProvider();
      provider.Properties.Add(new MooPropertySummary("foo", new MooObjectId(0)));
      provider.Verbs.Add(new MooVerbSummary(new[] { "foo" }, new MooObjectId(0)));
      using var controller = CreateController(provider);

      controller.GetMemberItems("$foo");
      WaitForCache(controller, "$foo");

      var foos = controller.GetMemberItems("$foo").Where(i => i.Text == "foo").ToList();
      foos.Should().HaveCount(2);
      foos[0].ImageIndex.Should().Be((int)CompletionIconCategory.CoreReference,
         "the property comes first on a same-name tie");
      foos[1].Should().BeOfType<VerbCompletionItem>("the verb follows the same-name property");
   }

   [Fact]
   public void Property_context_does_not_fetch_verbs()
   {
      // The regular '.' property path is unchanged: it queries only properties.
      var provider = new FakeQueryProvider();
      provider.Properties.Add(new MooPropertySummary("name", new MooObjectId(5)));
      using var controller = CreateController(provider, contextObject: new MooObjectId(5));

      controller.GetMemberItems("this.");
      WaitForCache(controller, "this.");
      controller.GetMemberItems("this.");

      provider.PropertyCalls.Should().Be(1);
      provider.VerbCalls.Should().Be(0, "the '.' property path must not query verbs");
   }

   [Fact]
   public void Verb_context_does_not_fetch_properties()
   {
      // The ':' verb path is unchanged: it queries only verbs.
      var provider = new FakeQueryProvider();
      provider.Verbs.Add(new MooVerbSummary(new[] { "tell" }, new MooObjectId(5)));
      using var controller = CreateController(provider, contextObject: new MooObjectId(5));

      controller.GetMemberItems("this:");
      WaitForCache(controller, "this:");
      controller.GetMemberItems("this:");

      provider.VerbCalls.Should().Be(1);
      provider.PropertyCalls.Should().Be(0, "the ':' verb path must not query properties");
   }

   // Diagnostic callback tests

   [Fact]
   public void Diagnostic_invoked_with_exception_when_provider_throws()
   {
      var provider = new FakeQueryProvider { ThrowOnVerbs = new TimeoutException("SDWC timed out") };
      string? capturedMessage = null;
      Exception? capturedException = null;
      using var controller = CreateController(
         provider,
         contextObject: new MooObjectId(5),
         diagnostic: (msg, ex) => { capturedMessage = msg; capturedException = ex; });

      controller.GetMemberItems("this:");
      Thread.Sleep(250); // allow the faulted fetch to finish

      capturedMessage.Should().NotBeNullOrEmpty("diagnostic must receive a message on provider throw");
      capturedMessage.Should().Contain("5", "message should include the object number");
      capturedException.Should().BeOfType<TimeoutException>("diagnostic must receive the originating exception");
   }

   [Fact]
   public void CoreName_non_object_value_is_expected_non_resolution_and_not_logged()
   {
      // udd-efk: a core reference resolving to a non-object value is an *expected* non-resolution
      // (a normal outcome of typing), so it yields no source WITHOUT invoking the warning diagnostic
      // — otherwise the log would flood on every partial keystroke. Only thrown exceptions are logged.
      var provider = new FakeQueryProvider();
      // Type 2 = STR, not OBJ — resolves to no object.
      provider.PropertyValues["network"] = new MooPropertyValue(2, "\"hello\"");
      var diagnosticInvoked = false;
      using var controller = CreateController(
         provider,
         diagnostic: (_, _) => diagnosticInvoked = true);

      controller.GetMemberItems("$network:");
      Thread.Sleep(250); // allow the resolve to finish

      diagnosticInvoked.Should().BeFalse(
         "an expected non-resolution (non-object core value) must not be logged as a warning");
      controller.GetMemberItems("$network:").Should().BeEmpty();
   }

   [Fact]
   public void Diagnostic_callback_that_throws_does_not_break_completion()
   {
      var provider = new FakeQueryProvider { ThrowOnVerbs = new InvalidOperationException("boom") };
      using var controller = CreateController(
         provider,
         contextObject: new MooObjectId(5),
         diagnostic: (_, _) => throw new Exception("bad diagnostic"));

      // Completion must not throw even though both the provider and the diagnostic callback throw.
      var act = () =>
      {
         controller.GetMemberItems("this:");
         Thread.Sleep(250);
         controller.GetMemberItems("this:x");
      };

      act.Should().NotThrow("a faulty diagnostic callback must not propagate out of the controller");
   }

   [Fact]
   public void Constructor_without_diagnostic_parameter_still_works()
   {
      // Prove the existing 4-parameter factory path (no diagnostic) compiles and functions.
      var provider = new FakeQueryProvider();
      provider.Verbs.Add(new MooVerbSummary(new[] { "tell" }, new MooObjectId(5)));
      using var controller = new MemberCompletionController(
         () => provider,
         () => new MooObjectId(5),
         action => action(),
         () => { });

      controller.GetMemberItems("this:");
      WaitForCache(controller, "this:");
      controller.GetMemberItems("this:").Should().NotBeEmpty("basic operation must work without a diagnostic callback");
   }
}
