using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
using Xunit;

namespace Org.Edgerunner.Mud.Common.Tests.Querying;

public class CachingMooWorldQueryProviderTests
{
    private static MooObjectSummary Summary(int id, string name)
    {
        return new MooObjectSummary(new MooObjectId(id), name, Array.Empty<string>());
    }

    [Fact]
    public async Task CacheHit_DoesNotCallInnerAgain()
    {
        var inner = new FakeQueryProvider
        {
            OnGetObjects = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(1, "a") }),
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMinutes(5));

        var first = await cache.GetObjectsAsync(CancellationToken.None);
        var second = await cache.GetObjectsAsync(CancellationToken.None);

        first.Should().BeSameAs(second);
        inner.GetObjectsCallCount.Should().Be(1);
    }

    [Fact]
    public async Task EmptyResult_IsCached()
    {
        var inner = new FakeQueryProvider
        {
            OnGetObjects = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(Array.Empty<MooObjectSummary>()),
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMinutes(5));

        await cache.GetObjectsAsync(CancellationToken.None);
        await cache.GetObjectsAsync(CancellationToken.None);

        inner.GetObjectsCallCount.Should().Be(1);
    }

    [Fact]
    public async Task NullResult_IsCached()
    {
        var callCount = 0;
        var inner = new FakeQueryProvider
        {
            OnGetParent = _ =>
            {
                callCount++;
                return Task.FromResult<MooObjectId?>(null);
            },
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMinutes(5));

        (await cache.GetParentAsync(new MooObjectId(1), CancellationToken.None)).Should().BeNull();
        (await cache.GetParentAsync(new MooObjectId(1), CancellationToken.None)).Should().BeNull();

        callCount.Should().Be(1);
    }

    [Fact]
    public async Task Exception_IsNotCached()
    {
        var attempts = 0;
        var inner = new FakeQueryProvider
        {
            OnGetObjects = () =>
            {
                attempts++;
                throw new InvalidOperationException("boom");
            },
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMinutes(5));

        await ((Func<Task>)(() => cache.GetObjectsAsync(CancellationToken.None))).Should().ThrowAsync<InvalidOperationException>();
        await ((Func<Task>)(() => cache.GetObjectsAsync(CancellationToken.None))).Should().ThrowAsync<InvalidOperationException>();

        attempts.Should().Be(2);
    }

    [Fact]
    public async Task TtlExpiry_CausesReFetch()
    {
        var inner = new FakeQueryProvider
        {
            OnGetObjects = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(1, "a") }),
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMilliseconds(30));

        await cache.GetObjectsAsync(CancellationToken.None);
        await Task.Delay(80);
        await cache.GetObjectsAsync(CancellationToken.None);

        inner.GetObjectsCallCount.Should().Be(2);
    }

    [Fact]
    public async Task InvalidateObject_ForcesReFetch()
    {
        var calls = 0;
        var inner = new FakeQueryProvider
        {
            OnGetParent = _ =>
            {
                calls++;
                return Task.FromResult<MooObjectId?>(new MooObjectId(99));
            },
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMinutes(5));
        var id = new MooObjectId(7);

        await cache.GetParentAsync(id, CancellationToken.None);
        await cache.GetParentAsync(id, CancellationToken.None);
        calls.Should().Be(1);

        cache.InvalidateObject(id);
        await cache.GetParentAsync(id, CancellationToken.None);
        calls.Should().Be(2);
    }

    [Fact]
    public async Task Clear_DropsAllEntries()
    {
        var inner = new FakeQueryProvider
        {
            OnGetObjects = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(1, "a") }),
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMinutes(5));

        await cache.GetObjectsAsync(CancellationToken.None);
        cache.Clear();
        await cache.GetObjectsAsync(CancellationToken.None);

        inner.GetObjectsCallCount.Should().Be(2);
    }

    [Fact]
    public async Task GetOwnedObjectsForPlayer_CacheHit_DoesNotCallInnerAgain()
    {
        var inner = new FakeQueryProvider
        {
            OnGetOwnedObjectsForPlayer = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(1, "a") }),
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMinutes(5));

        var first = await cache.GetOwnedObjectsAsync(CancellationToken.None);
        var second = await cache.GetOwnedObjectsAsync(CancellationToken.None);

        first.Should().BeSameAs(second);
        inner.GetOwnedObjectsForPlayerCallCount.Should().Be(1);
    }

    [Fact]
    public async Task GetOwnedObjectsForOwner_CacheHit_DoesNotCallInnerAgain()
    {
        var owner = new MooObjectId(42);
        var inner = new FakeQueryProvider
        {
            OnGetOwnedObjectsForOwner = _ => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(1, "a") }),
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMinutes(5));

        var first = await cache.GetOwnedObjectsAsync(owner, CancellationToken.None);
        var second = await cache.GetOwnedObjectsAsync(owner, CancellationToken.None);

        first.Should().BeSameAs(second);
        inner.GetOwnedObjectsForOwnerCallCount.Should().Be(1);
    }

    [Fact]
    public async Task GetOwnedObjectsForOwner_DifferentOwners_AreCachedSeparately()
    {
        var inner = new FakeQueryProvider
        {
            OnGetOwnedObjectsForOwner = owner =>
                Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(owner.Number, "owned") }),
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMinutes(5));

        await cache.GetOwnedObjectsAsync(new MooObjectId(1), CancellationToken.None);
        await cache.GetOwnedObjectsAsync(new MooObjectId(2), CancellationToken.None);

        inner.GetOwnedObjectsForOwnerCallCount.Should().Be(2);
    }

    [Fact]
    public async Task OwnedObjectsOverloads_UseDistinctCacheKeys()
    {
        // The owner overload is invoked with MooObjectId.Nothing, which is the same placeholder id the
        // current-player overload uses. The two overloads must still be cached under distinct keys.
        var inner = new FakeQueryProvider
        {
            OnGetOwnedObjectsForPlayer = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(1, "player") }),
            OnGetOwnedObjectsForOwner = _ => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(2, "owner") }),
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMinutes(5));

        var playerResult = await cache.GetOwnedObjectsAsync(CancellationToken.None);
        var ownerResult = await cache.GetOwnedObjectsAsync(MooObjectId.Nothing, CancellationToken.None);

        // A cached current-player result must NOT be returned for the owner overload and vice versa.
        playerResult.Should().ContainSingle().Which.Name.Should().Be("player");
        ownerResult.Should().ContainSingle().Which.Name.Should().Be("owner");
        inner.GetOwnedObjectsForPlayerCallCount.Should().Be(1);
        inner.GetOwnedObjectsForOwnerCallCount.Should().Be(1);
    }

    [Fact]
    public async Task InvalidateObject_DropsOwnerKeyedOwnedObjectsEntry()
    {
        var owner = new MooObjectId(7);
        var calls = 0;
        var inner = new FakeQueryProvider
        {
            OnGetOwnedObjectsForOwner = _ =>
            {
                calls++;
                return Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(99, "owned") });
            },
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMinutes(5));

        await cache.GetOwnedObjectsAsync(owner, CancellationToken.None);
        await cache.GetOwnedObjectsAsync(owner, CancellationToken.None);
        calls.Should().Be(1);

        cache.InvalidateObject(owner);
        await cache.GetOwnedObjectsAsync(owner, CancellationToken.None);
        calls.Should().Be(2);
    }

    [Fact]
    public async Task GetVerbCode_CacheHit_DoesNotCallInnerAgain_AndSurfacesResolvedId()
    {
        var queried = new MooObjectId(5);
        var resolved = new MooObjectId(2);
        var calls = 0;
        var inner = new FakeQueryProvider
        {
            OnGetVerbCode = (objId, _) =>
            {
                calls++;
                return Task.FromResult<MooVerbCode?>(new MooVerbCode(objId, resolved, new[] { "return 1;" }));
            },
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMinutes(5));

        var first = await cache.GetVerbCodeAsync(queried, "foo", CancellationToken.None);
        var second = await cache.GetVerbCodeAsync(queried, "foo", CancellationToken.None);

        first.Should().BeSameAs(second);
        calls.Should().Be(1);
        // Inherited verb: queried object differs from the resolved (defining) object.
        first!.QueriedObjectId.Should().Be(queried);
        first.ResolvedObjectId.Should().Be(resolved);
    }

    [Fact]
    public async Task GetVerbDocumentation_NullResult_IsCached()
    {
        var calls = 0;
        var inner = new FakeQueryProvider
        {
            OnGetVerbDocumentation = (_, _) =>
            {
                calls++;
                return Task.FromResult<MooVerbDocumentation?>(null);
            },
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMinutes(5));

        (await cache.GetVerbDocumentationAsync(new MooObjectId(1), "foo", CancellationToken.None)).Should().BeNull();
        (await cache.GetVerbDocumentationAsync(new MooObjectId(1), "foo", CancellationToken.None)).Should().BeNull();

        calls.Should().Be(1);
    }

    [Fact]
    public async Task GetPropertyDocumentation_CacheHit_DoesNotCallInnerAgain()
    {
        var calls = 0;
        var inner = new FakeQueryProvider
        {
            OnGetPropertyDocumentation = (_, _) =>
            {
                calls++;
                return Task.FromResult<IReadOnlyList<string>>(new[] { "line" });
            },
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMinutes(5));

        var first = await cache.GetPropertyDocumentationAsync(new MooObjectId(5), "desc", CancellationToken.None);
        var second = await cache.GetPropertyDocumentationAsync(new MooObjectId(5), "desc", CancellationToken.None);

        first.Should().BeSameAs(second);
        calls.Should().Be(1);
    }

    [Fact]
    public async Task GetPropertyDocumentation_EmptyResult_IsCached()
    {
        var calls = 0;
        var inner = new FakeQueryProvider
        {
            OnGetPropertyDocumentation = (_, _) =>
            {
                calls++;
                return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
            },
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMinutes(5));

        await cache.GetPropertyDocumentationAsync(new MooObjectId(5), "desc", CancellationToken.None);
        await cache.GetPropertyDocumentationAsync(new MooObjectId(5), "desc", CancellationToken.None);

        calls.Should().Be(1);
    }

    [Fact]
    public async Task GetPropertyDocumentation_DifferentNames_AreCachedSeparately()
    {
        var calls = 0;
        var inner = new FakeQueryProvider
        {
            OnGetPropertyDocumentation = (_, name) =>
            {
                calls++;
                return Task.FromResult<IReadOnlyList<string>>(new[] { name });
            },
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMinutes(5));

        await cache.GetPropertyDocumentationAsync(new MooObjectId(5), "desc", CancellationToken.None);
        await cache.GetPropertyDocumentationAsync(new MooObjectId(5), "name", CancellationToken.None);

        calls.Should().Be(2);
    }

    [Fact]
    public async Task Invalidate_SpecificKey_ForcesReFetch()
    {
        var inner = new FakeQueryProvider
        {
            OnGetObjects = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(1, "a") }),
        };
        using var cache = new CachingMooWorldQueryProvider(inner, TimeSpan.FromMinutes(5));

        await cache.GetObjectsAsync(CancellationToken.None);
        cache.Invalidate(CachingMooWorldQueryProvider.Operation.GetObjects, MooObjectId.Nothing);
        await cache.GetObjectsAsync(CancellationToken.None);

        inner.GetObjectsCallCount.Should().Be(2);
    }
}
