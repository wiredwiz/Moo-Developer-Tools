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
      public MooObjectId? LastVerbQueryObjectId;
      public TaskCompletionSource? Gate;
      public Exception? ThrowOnVerbs;
      public Dictionary<string, MooPropertyValue?> PropertyValues { get; } = new(StringComparer.OrdinalIgnoreCase);

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
   public void CoreName_queued_marshal_cancellation_releases_inflight_marker_so_retry_can_succeed()
   {
      // I1 regression test for FetchCoreNameAsync success-lambda path:
      // With a queuing marshal the success lambda is deferred.  The cancellation token is
      // checked INSIDE the lambda (after the marker clear).  This test proves that the
      // marker is cleared even when cancellation arrives before the lambda runs, so that a
      // subsequent $network: call is not permanently suppressed.
      //
      // Sequence:
      //   (a) GetMemberItems("$network:") — provider has no gate so GetPropertyValueAsync
      //       and GetVerbsAsync both return completed tasks; FetchCoreNameAsync runs to
      //       completion synchronously, queuing the success lambda before GetMemberItems
      //       returns.
      //   (b) GetMemberItems("#7.") — swaps/cancels _fetchCancellation, invalidating the
      //       token captured by the queued lambda; also queues the #7. success lambda.
      //   (c) Drain the queue — both lambdas execute; the $network: lambda sees
      //       IsCancellationRequested, clears the marker, and does NOT populate the cache.
      //   (d) Assert $network: cache is empty, then issue a retry call; assert PropValueCalls
      //       reaches 2 (new resolve started), drain the newly queued lambda, and assert
      //       items are now returned.
      var provider = new FakeQueryProvider();
      provider.PropertyValues["network"] = new MooPropertyValue(1, "#62");
      provider.Verbs.Add(new MooVerbSummary(new[] { "tell" }, new MooObjectId(62)));
      provider.Properties.Add(new MooPropertySummary("name", new MooObjectId(7)));

      var marshalQueue = new List<Action>();
      using var controller = CreateControllerWithQueuedMarshal(
         provider, marshalQueue, contextObject: new MooObjectId(7));

      // (a) Start core-name resolve for $network: — success lambda queued immediately.
      controller.GetMemberItems("$network:");
      SpinWait.SpinUntil(() => marshalQueue.Count > 0, TimeSpan.FromSeconds(5))
              .Should().BeTrue("success lambda should be queued synchronously by the ungated provider");

      // (b) Switch context — cancels the queued lambda's token, queues #7. success lambda.
      controller.GetMemberItems("#7.");

      // (c) Drain all queued actions.
      var snapshot = marshalQueue.ToList();
      marshalQueue.Clear();
      foreach (var action in snapshot)
         action();

      // (d) Retry: single call — starts a new resolve (marker was cleared) and returns empty
      //     while the new lambda is queued.  PropValueCalls must reach 2, proving the marker
      //     was released so the retry was not suppressed.
      var retryResult = controller.GetMemberItems("$network:");
      retryResult.Should().BeEmpty("the cache must be empty and the retry must start a new resolve");
      provider.PropValueCalls.Should().Be(2,
         "cancellation inside the success lambda must release the inflight marker so a retry starts a new resolve");

      // Drain the retry's queued lambda and verify items appear.
      var retrySnapshot = marshalQueue.ToList();
      marshalQueue.Clear();
      foreach (var action in retrySnapshot)
         action();

      controller.GetMemberItems("$network:").Select(i => i.Text).Should().Contain("tell",
         "after draining the retry lambda the cache should be populated");
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
   public void Diagnostic_invoked_with_null_exception_when_core_name_validation_fails()
   {
      var provider = new FakeQueryProvider();
      // Type 2 = STR, not OBJ — validation should fail
      provider.PropertyValues["network"] = new MooPropertyValue(2, "\"hello\"");
      string? capturedMessage = null;
      Exception? capturedException = null;
      var diagnosticInvoked = false;
      using var controller = CreateController(
         provider,
         diagnostic: (msg, ex) =>
         {
            capturedMessage = msg;
            capturedException = ex;
            diagnosticInvoked = true;
         });

      controller.GetMemberItems("$network:");
      Thread.Sleep(250); // allow the faulted resolve to finish

      diagnosticInvoked.Should().BeTrue("diagnostic must be invoked on core-name validation failure");
      capturedMessage.Should().NotBeNullOrEmpty("diagnostic must receive a message describing the failure");
      capturedMessage.Should().Contain("network", "message should name the core reference");
      capturedException.Should().BeNull("validation failures carry no exception");
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
