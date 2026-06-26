using FluentAssertions;
using Org.Edgerunner.Mud.Common.Querying;
using Xunit;

namespace Org.Edgerunner.Mud.Common.Tests.Querying;

public class ConstantQueryTests
{
    [Fact]
    public async Task Service_RoutesConstantValueThroughRegistry()
    {
        using var service = new MooWorldQueryService(TimeSpan.FromMinutes(5));
        var provider = new FakeQueryProvider();
        provider.ConstantValues["INT"] = "0";
        service.Register(provider, 1);

        var result = await service.Query.GetConstantValueAsync("INT", CancellationToken.None);

        result.Should().Be("0");
    }

    [Fact]
    public async Task Service_RoutesConstantToStrThroughRegistry()
    {
        using var service = new MooWorldQueryService(TimeSpan.FromMinutes(5));
        var provider = new FakeQueryProvider();
        provider.ConstantToStrValues["E_PERM"] = "Permission denied";
        service.Register(provider, 1);

        var result = await service.Query.GetConstantToStrAsync("E_PERM", CancellationToken.None);

        result.Should().Be("Permission denied");
    }

    [Fact]
    public async Task Service_ReturnsNullWhenNoProviderRegistered()
    {
        using var service = new MooWorldQueryService(TimeSpan.FromMinutes(5));

        (await service.Query.GetConstantValueAsync("INT", CancellationToken.None)).Should().BeNull();
        (await service.Query.GetConstantToStrAsync("E_PERM", CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task Caching_DoesNotCacheNullConstantValue()
    {
        var provider = new FakeQueryProvider();
        using var caching = new CachingMooWorldQueryProvider(provider, TimeSpan.FromMinutes(5));

        // First call: "INT" not configured -> null.
        (await caching.GetConstantValueAsync("INT", CancellationToken.None)).Should().BeNull();

        // Now a live value becomes available; a cached null would mask it.
        provider.ConstantValues["INT"] = "0";
        (await caching.GetConstantValueAsync("INT", CancellationToken.None)).Should().Be("0");

        // The provider was hit on both calls (the null was never cached).
        provider.GetConstantValueCallCount.Should().Be(2);
    }
}
