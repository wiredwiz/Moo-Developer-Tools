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
    public async Task GetOwnedObjectsForPlayer_HigherPriorityProvider_AnswersFirst()
    {
        var low = new FakeQueryProvider
        {
            OnGetOwnedObjectsForPlayer = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(1, "low") }),
        };
        var high = new FakeQueryProvider
        {
            OnGetOwnedObjectsForPlayer = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(2, "high") }),
        };

        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(low, 1);
        registry.Register(high, 10);

        var result = await registry.GetOwnedObjectsAsync(CancellationToken.None);

        result.Should().ContainSingle().Which.Name.Should().Be("high");
        low.GetOwnedObjectsForPlayerCallCount.Should().Be(0);
        high.GetOwnedObjectsForPlayerCallCount.Should().Be(1);
    }

    [Fact]
    public async Task GetOwnedObjectsForPlayer_NotImplementedException_FallsThroughToNextProvider()
    {
        var unsupported = new FakeQueryProvider();
        var supported = new FakeQueryProvider
        {
            OnGetOwnedObjectsForPlayer = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(5, "fallback") }),
        };

        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(unsupported, 10);
        registry.Register(supported, 1);

        var result = await registry.GetOwnedObjectsAsync(CancellationToken.None);

        result.Should().ContainSingle().Which.Name.Should().Be("fallback");
        unsupported.GetOwnedObjectsForPlayerCallCount.Should().Be(1);
        supported.GetOwnedObjectsForPlayerCallCount.Should().Be(1);
    }

    [Fact]
    public async Task GetOwnedObjectsForPlayer_Exhaustion_ReturnsEmpty()
    {
        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(new FakeQueryProvider(), 1); // unsupported

        var result = await registry.GetOwnedObjectsAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOwnedObjectsForOwner_HigherPriorityProvider_AnswersFirst()
    {
        var owner = new MooObjectId(42);
        var low = new FakeQueryProvider
        {
            OnGetOwnedObjectsForOwner = _ => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(1, "low") }),
        };
        var high = new FakeQueryProvider
        {
            OnGetOwnedObjectsForOwner = _ => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(2, "high") }),
        };

        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(low, 1);
        registry.Register(high, 10);

        var result = await registry.GetOwnedObjectsAsync(owner, CancellationToken.None);

        result.Should().ContainSingle().Which.Name.Should().Be("high");
        low.GetOwnedObjectsForOwnerCallCount.Should().Be(0);
        high.GetOwnedObjectsForOwnerCallCount.Should().Be(1);
    }

    [Fact]
    public async Task GetOwnedObjectsForOwner_NotImplementedException_FallsThroughToNextProvider()
    {
        var owner = new MooObjectId(42);
        var unsupported = new FakeQueryProvider();
        var supported = new FakeQueryProvider
        {
            OnGetOwnedObjectsForOwner = _ => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(5, "fallback") }),
        };

        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(unsupported, 10);
        registry.Register(supported, 1);

        var result = await registry.GetOwnedObjectsAsync(owner, CancellationToken.None);

        result.Should().ContainSingle().Which.Name.Should().Be("fallback");
        unsupported.GetOwnedObjectsForOwnerCallCount.Should().Be(1);
        supported.GetOwnedObjectsForOwnerCallCount.Should().Be(1);
    }

    [Fact]
    public async Task GetOwnedObjectsForOwner_Exhaustion_ReturnsEmpty()
    {
        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(new FakeQueryProvider(), 1); // unsupported

        var result = await registry.GetOwnedObjectsAsync(new MooObjectId(42), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetVerbDocumentation_NotImplementedException_FallsThroughToNextProvider()
    {
        var queried = new MooObjectId(5);
        var resolved = new MooObjectId(2);
        var unsupported = new FakeQueryProvider();
        var supported = new FakeQueryProvider
        {
            OnGetVerbDocumentation = (objId, _) =>
                Task.FromResult<MooVerbDocumentation?>(new MooVerbDocumentation(objId, resolved, new[] { "doc" })),
        };

        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(unsupported, 10);
        registry.Register(supported, 1);

        var result = await registry.GetVerbDocumentationAsync(queried, "foo", CancellationToken.None);

        result.Should().NotBeNull();
        // The verb is inherited: the queried object differs from the resolved (defining) object.
        result!.QueriedObjectId.Should().Be(queried);
        result.ResolvedObjectId.Should().Be(resolved);
        result.Lines.Should().ContainSingle().Which.Should().Be("doc");
    }

    [Fact]
    public async Task GetVerbDocumentation_Exhaustion_ReturnsNull()
    {
        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(new FakeQueryProvider(), 1); // unsupported

        var result = await registry.GetVerbDocumentationAsync(new MooObjectId(3), "foo", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetVerbCode_NotImplementedException_FallsThroughToNextProvider()
    {
        var queried = new MooObjectId(5);
        var resolved = new MooObjectId(2);
        var unsupported = new FakeQueryProvider();
        var supported = new FakeQueryProvider
        {
            OnGetVerbCode = (objId, _) =>
                Task.FromResult<MooVerbCode?>(new MooVerbCode(objId, resolved, new[] { "return 1;" })),
        };

        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(unsupported, 10);
        registry.Register(supported, 1);

        var result = await registry.GetVerbCodeAsync(queried, "foo", CancellationToken.None);

        result.Should().NotBeNull();
        // The verb is inherited: the queried object differs from the resolved (defining) object.
        result!.QueriedObjectId.Should().Be(queried);
        result.ResolvedObjectId.Should().Be(resolved);
        result.Lines.Should().ContainSingle().Which.Should().Be("return 1;");
    }

    [Fact]
    public async Task GetVerbCode_Exhaustion_ReturnsNull()
    {
        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(new FakeQueryProvider(), 1); // unsupported

        var result = await registry.GetVerbCodeAsync(new MooObjectId(3), "foo", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetVerbInfo_SurfacesQueriedAndResolvedIds()
    {
        var queried = new MooObjectId(5);
        var resolved = new MooObjectId(2);
        var provider = new FakeQueryProvider
        {
            OnGetVerbInfo = (objId, _) => Task.FromResult<MooVerbInfo?>(new MooVerbInfo(
                objId,
                resolved,
                new[] { "foo" },
                new MooObjectId(1),
                new VerbPermission(true, true, true, false),
                new VerbArgs(DirectObject.This, Preposition.None, IndirectObject.This))),
        };

        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(provider, 1);

        var result = await registry.GetVerbInfoAsync(queried, "foo", CancellationToken.None);

        result.Should().NotBeNull();
        result!.QueriedObjectId.Should().Be(queried);
        result.ResolvedObjectId.Should().Be(resolved);
    }

    [Fact]
    public async Task GetVerbInfo_Exhaustion_ReturnsNull()
    {
        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(new FakeQueryProvider(), 1); // unsupported

        var result = await registry.GetVerbInfoAsync(new MooObjectId(3), "foo", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPropertyDocumentation_HigherPriorityProvider_AnswersFirst()
    {
        var low = new FakeQueryProvider
        {
            OnGetPropertyDocumentation = (_, _) => Task.FromResult<IReadOnlyList<string>>(new[] { "low" }),
        };
        var high = new FakeQueryProvider
        {
            OnGetPropertyDocumentation = (_, _) => Task.FromResult<IReadOnlyList<string>>(new[] { "high" }),
        };

        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(low, 1);
        registry.Register(high, 10);

        var result = await registry.GetPropertyDocumentationAsync(new MooObjectId(3), "desc", CancellationToken.None);

        result.Should().ContainSingle().Which.Should().Be("high");
    }

    [Fact]
    public async Task GetPropertyDocumentation_NotImplementedException_FallsThroughToNextProvider()
    {
        var unsupported = new FakeQueryProvider();
        var supported = new FakeQueryProvider
        {
            OnGetPropertyDocumentation = (_, _) => Task.FromResult<IReadOnlyList<string>>(new[] { "fallback" }),
        };

        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(unsupported, 10);
        registry.Register(supported, 1);

        var result = await registry.GetPropertyDocumentationAsync(new MooObjectId(3), "desc", CancellationToken.None);

        result.Should().ContainSingle().Which.Should().Be("fallback");
    }

    [Fact]
    public async Task GetPropertyDocumentation_Exhaustion_ReturnsEmpty()
    {
        var registry = new MooWorldQueryProviderRegistry();
        registry.Register(new FakeQueryProvider(), 1); // unsupported

        var result = await registry.GetPropertyDocumentationAsync(new MooObjectId(3), "desc", CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPropertyDocumentation_NoProviders_ReturnsEmpty()
    {
        var registry = new MooWorldQueryProviderRegistry();

        var result = await registry.GetPropertyDocumentationAsync(new MooObjectId(3), "desc", CancellationToken.None);

        result.Should().BeEmpty();
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
