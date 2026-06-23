using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
using Org.Edgerunner.Mud.Communication.Sdwc;
using Xunit;

namespace Org.Edgerunner.Mud.Communication.Tests.Sdwc;

public class SdwcCorrelatorTests
{
   private static SdwcCorrelationKey Key(string marker, int id, string? name = null)
      => new(marker, new MooObjectId(id), name);

   [Fact]
   public async Task Complete_ResolvesPendingTask_AndRemovesEntry()
   {
      var correlator = new SdwcCorrelator();
      var key = Key("VERBS", 1);
      var pending = correlator.CreatePending(key);

      correlator.Complete(key, "{}").Should().BeTrue();

      (await pending).Should().Be("{}");
      correlator.PendingCount.Should().Be(0);
   }

   [Fact]
   public async Task Fail_FaultsPendingTask_AndRemovesEntry()
   {
      var correlator = new SdwcCorrelator();
      var key = Key("VERBS", 1);
      var pending = correlator.CreatePending(key);

      correlator.Fail(key, new InvalidOperationException("boom")).Should().BeTrue();

      var act = async () => await pending;
      await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
      correlator.PendingCount.Should().Be(0);
   }

   [Fact]
   public void Complete_UnknownKey_ReturnsFalse()
   {
      var correlator = new SdwcCorrelator();
      correlator.Complete(Key("VERBS", 99), "{}").Should().BeFalse();
   }

   [Fact]
   public void Remove_DropsPendingEntry()
   {
      var correlator = new SdwcCorrelator();
      var key = Key("VERBS", 1);
      correlator.CreatePending(key);

      correlator.Remove(key).Should().BeTrue();
      correlator.PendingCount.Should().Be(0);
   }

   [Fact]
   public async Task ConcurrentKeys_AreResolvedIndependently()
   {
      var correlator = new SdwcCorrelator();
      var a = correlator.CreatePending(Key("VERBS", 1));
      var b = correlator.CreatePending(Key("VERBS", 2));

      // Resolve out of order.
      correlator.Complete(Key("VERBS", 2), "two");
      correlator.Complete(Key("VERBS", 1), "one");

      (await a).Should().Be("one");
      (await b).Should().Be("two");
   }

   [Fact]
   public void DuplicateKey_Throws()
   {
      var correlator = new SdwcCorrelator();
      var key = Key("VERBS", 1);
      correlator.CreatePending(key);

      Action act = () => correlator.CreatePending(key);
      act.Should().Throw<InvalidOperationException>();
   }

   [Fact]
   public async Task FaultAll_FaultsEveryPendingTaskWithGivenException()
   {
      var correlator = new SdwcCorrelator();
      var first = correlator.CreatePending(Key("VERBS", 1));
      var second = correlator.CreatePending(Key("PROPS", 2));

      var boom = new InvalidOperationException("boom");
      correlator.FaultAll(boom);

      var firstAct = async () => await first;
      var secondAct = async () => await second;
      (await firstAct.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(boom);
      (await secondAct.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(boom);
      correlator.PendingCount.Should().Be(0);
   }

   [Fact]
   public async Task FaultAll_LeavesCorrelatorReusable()
   {
      var correlator = new SdwcCorrelator();
      correlator.CreatePending(Key("VERBS", 1));

      correlator.FaultAll(new QueryConnectionClosedException());

      // Still usable: a fresh pending registers and completes normally.
      var key = Key("VERBS", 2);
      var next = correlator.CreatePending(key);
      correlator.Complete(key, "{}").Should().BeTrue();
      (await next).Should().Be("{}");
   }

   [Fact]
   public async Task Dispose_FaultsPendingTasksWithQueryConnectionClosedException()
   {
      var correlator = new SdwcCorrelator();
      var pending = correlator.CreatePending(Key("VERBS", 1));

      correlator.Dispose();

      var act = async () => await pending;
      await act.Should().ThrowAsync<QueryConnectionClosedException>();
   }

   [Fact]
   public async Task CreatePending_AfterDispose_ReturnsAlreadyFaultedTask()
   {
      var correlator = new SdwcCorrelator();
      correlator.Dispose();

      var pending = correlator.CreatePending(Key("VERBS", 1));

      pending.IsFaulted.Should().BeTrue();
      var act = async () => await pending;
      await act.Should().ThrowAsync<QueryConnectionClosedException>();
   }

   [Fact]
   public void CompleteFailAndRemove_AfterDispose_AreSafeNoOps()
   {
      var correlator = new SdwcCorrelator();
      correlator.Dispose();

      var key = Key("VERBS", 1);
      correlator.Complete(key, "{}").Should().BeFalse();
      correlator.Fail(key, new InvalidOperationException("x")).Should().BeFalse();
      correlator.Remove(key).Should().BeFalse();
   }

   [Fact]
   public void Dispose_Twice_DoesNotThrow()
   {
      var correlator = new SdwcCorrelator();
      correlator.Dispose();

      Action act = () => correlator.Dispose();
      act.Should().NotThrow();
   }
}
