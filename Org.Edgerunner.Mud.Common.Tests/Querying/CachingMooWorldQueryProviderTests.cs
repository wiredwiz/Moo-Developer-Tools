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
