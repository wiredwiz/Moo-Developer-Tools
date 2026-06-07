using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
using Xunit;

namespace Org.Edgerunner.Mud.Common.Tests.Querying;

public class MooWorldQueryProviderRegistryTests
{
    private static MooObjectSummary Summary(int id, string name)
    {
        return new MooObjectSummary(new MooObjectId(id), name, Array.Empty<string>());
    }

    [Fact]
    public async Task HigherPriorityProvider_AnswersFirst()
    {
        var low = new FakeQueryProvider
        {
            OnGetObjects = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(1, "low") }),
        };
        var high = new FakeQueryProvider
        {
            OnGetObjects = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(2, "high") }),
        };

        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(low, 1);
        registry.Register(high, 10);

        var result = await registry.GetObjectsAsync(CancellationToken.None);

        result.Should().ContainSingle().Which.Name.Should().Be("high");
        low.GetObjectsCallCount.Should().Be(0);
        high.GetObjectsCallCount.Should().Be(1);
    }

    [Fact]
    public async Task NotImplementedException_FallsThroughToNextProvider()
    {
        // First provider (higher priority) does not support GetObjects (throws NotImplementedException).
        var unsupported = new FakeQueryProvider();
        var supported = new FakeQueryProvider
        {
            OnGetObjects = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(5, "fallback") }),
        };

        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(unsupported, 10);
        registry.Register(supported, 1);

        var result = await registry.GetObjectsAsync(CancellationToken.None);

        result.Should().ContainSingle().Which.Name.Should().Be("fallback");
        unsupported.GetObjectsCallCount.Should().Be(1);
        supported.GetObjectsCallCount.Should().Be(1);
    }

    [Fact]
    public async Task NonNotImplementedException_Surfaces_AndDoesNotFallThrough()
    {
        var throwing = new FakeQueryProvider
        {
            OnGetObjects = () => throw new InvalidOperationException("boom"),
        };
        var supported = new FakeQueryProvider
        {
            OnGetObjects = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(9, "should-not-reach") }),
        };

        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(throwing, 10);
        registry.Register(supported, 1);

        var act = async () => await registry.GetObjectsAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        supported.GetObjectsCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Exhaustion_ListMethod_ReturnsEmpty()
    {
        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(new FakeQueryProvider(), 1); // unsupported

        var result = await registry.GetObjectsAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task NoProviders_ListMethod_ReturnsEmpty()
    {
        var registry = new MooWorldQueryProviderRegistry();

        var result = await registry.GetObjectsAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Exhaustion_NullableMethod_ReturnsNull()
    {
        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(new FakeQueryProvider(), 1); // unsupported

        var result = await registry.GetParentAsync(new MooObjectId(3), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task EqualPriority_PreservesRegistrationOrder()
    {
        var first = new FakeQueryProvider
        {
            OnGetObjects = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(1, "first") }),
        };
        var second = new FakeQueryProvider
        {
            OnGetObjects = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(2, "second") }),
        };

        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(first, 5);
        registry.Register(second, 5);

        var result = await registry.GetObjectsAsync(CancellationToken.None);

        result.Should().ContainSingle().Which.Name.Should().Be("first");
    }

    [Fact]
    public void Register_And_Unregister_Fire_ProvidersChanged()
    {
        var registry = new MooWorldQueryProviderRegistry();
        var provider = new FakeQueryProvider();
        var count = 0;
        registry.ProvidersChanged += (_, _) => count++;

        registry.Register(provider, 1);
        registry.Unregister(provider).Should().BeTrue();
        registry.Unregister(provider).Should().BeFalse();

        count.Should().Be(2);
    }

    [Fact]
    public async Task Unregister_RemovesProviderFromChain()
    {
        var provider = new FakeQueryProvider
        {
            OnGetObjects = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(1, "x") }),
        };
        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(provider, 1);
        registry.Unregister(provider);

        var result = await registry.GetObjectsAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }
}
