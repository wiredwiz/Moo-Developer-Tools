using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
using Xunit;

namespace Org.Edgerunner.Mud.Common.Tests.Querying;

public class MooWorldQueryServiceTests
{
    private static MooObjectSummary Summary(int id, string name)
    {
        return new MooObjectSummary(new MooObjectId(id), name, Array.Empty<string>());
    }

    [Fact]
    public async Task Query_RoutesThroughRegistry()
    {
        using var service = new MooWorldQueryService(TimeSpan.FromMinutes(5));
        var provider = new FakeQueryProvider
        {
            OnGetObjects = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(1, "x") }),
        };
        service.Register(provider, 1);

        var result = await service.Query.GetObjectsAsync(CancellationToken.None);

        result.Should().ContainSingle().Which.Name.Should().Be("x");
    }

    [Fact]
    public async Task ProvidersChanged_ClearsCache()
    {
        using var service = new MooWorldQueryService(TimeSpan.FromMinutes(5));
        var provider = new FakeQueryProvider
        {
            OnGetObjects = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(1, "x") }),
        };
        service.Register(provider, 1);

        // Prime the cache.
        await service.Query.GetObjectsAsync(CancellationToken.None);
        provider.GetObjectsCallCount.Should().Be(1);

        // Registering another provider raises ProvidersChanged, which clears the cache.
        service.Register(new FakeQueryProvider(), 0);

        await service.Query.GetObjectsAsync(CancellationToken.None);
        provider.GetObjectsCallCount.Should().Be(2);
    }

    [Fact]
    public async Task Service_ReExposes_ProvidersChanged()
    {
        using var service = new MooWorldQueryService(TimeSpan.FromMinutes(5));
        var fired = 0;
        service.ProvidersChanged += (_, _) => fired++;

        var provider = new FakeQueryProvider();
        service.Register(provider, 1);
        service.Unregister(provider);

        fired.Should().Be(2);
        await Task.CompletedTask;
    }

    [Fact]
    public async Task InvalidateObject_Passthrough_ForcesReFetch()
    {
        using var service = new MooWorldQueryService(TimeSpan.FromMinutes(5));
        var calls = 0;
        var provider = new FakeQueryProvider
        {
            OnGetParent = _ =>
            {
                calls++;
                return Task.FromResult<MooObjectId?>(new MooObjectId(42));
            },
        };
        service.Register(provider, 1);
        var id = new MooObjectId(7);

        await service.Query.GetParentAsync(id, CancellationToken.None);
        await service.Query.GetParentAsync(id, CancellationToken.None);
        calls.Should().Be(1);

        service.InvalidateObject(id);
        await service.Query.GetParentAsync(id, CancellationToken.None);
        calls.Should().Be(2);
    }

    [Fact]
    public async Task Clear_Passthrough_ForcesReFetch()
    {
        using var service = new MooWorldQueryService(TimeSpan.FromMinutes(5));
        var provider = new FakeQueryProvider
        {
            OnGetObjects = () => Task.FromResult<IReadOnlyList<MooObjectSummary>>(new[] { Summary(1, "x") }),
        };
        service.Register(provider, 1);

        await service.Query.GetObjectsAsync(CancellationToken.None);
        service.Clear();
        await service.Query.GetObjectsAsync(CancellationToken.None);

        provider.GetObjectsCallCount.Should().Be(2);
    }
}
