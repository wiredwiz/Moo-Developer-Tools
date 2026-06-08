using Org.Edgerunner.Mud.Common.Querying;

namespace Org.Edgerunner.Mud.Common.Tests.Querying;

/// <summary>
/// A configurable in-memory <see cref="IMooWorldQueryProvider"/> for tests. Each operation either
/// returns a configured result or throws a configured exception (for example
/// <see cref="NotImplementedException"/> to trigger fall-through). By default every operation throws
/// <see cref="NotImplementedException"/> (i.e. "unsupported").
/// </summary>
public class FakeQueryProvider : IMooWorldQueryProvider
{
    public Func<Task<IReadOnlyList<MooObjectSummary>>>? OnGetObjects { get; set; }

    public Func<Task<IReadOnlyList<MooObjectSummary>>>? OnGetOwnedObjectsForPlayer { get; set; }

    public Func<MooObjectId, Task<IReadOnlyList<MooObjectSummary>>>? OnGetOwnedObjectsForOwner { get; set; }

    public Func<MooObjectId, Task<MooObjectId?>>? OnGetParent { get; set; }

    public Func<MooObjectId, string, Task<IReadOnlyList<string>>>? OnGetVerbCode { get; set; }

    /// <summary>
    /// Gets the number of times <see cref="GetObjectsAsync"/> has been invoked.
    /// </summary>
    public int GetObjectsCallCount { get; private set; }

    /// <summary>
    /// Gets the number of times the current-player <see cref="GetOwnedObjectsAsync(CancellationToken)"/> overload has been invoked.
    /// </summary>
    public int GetOwnedObjectsForPlayerCallCount { get; private set; }

    /// <summary>
    /// Gets the number of times the owner <see cref="GetOwnedObjectsAsync(MooObjectId, CancellationToken)"/> overload has been invoked.
    /// </summary>
    public int GetOwnedObjectsForOwnerCallCount { get; private set; }

    public Task<IReadOnlyList<MooObjectSummary>> GetObjectsAsync(CancellationToken cancellationToken)
    {
        GetObjectsCallCount++;
        if (OnGetObjects == null)
            throw new NotImplementedException();
        return OnGetObjects();
    }

    public Task<IReadOnlyList<MooObjectSummary>> GetChildrenAsync(MooObjectId objectId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(CancellationToken cancellationToken)
    {
        GetOwnedObjectsForPlayerCallCount++;
        if (OnGetOwnedObjectsForPlayer == null)
            throw new NotImplementedException();
        return OnGetOwnedObjectsForPlayer();
    }

    public Task<IReadOnlyList<MooObjectSummary>> GetOwnedObjectsAsync(MooObjectId owner, CancellationToken cancellationToken)
    {
        GetOwnedObjectsForOwnerCallCount++;
        if (OnGetOwnedObjectsForOwner == null)
            throw new NotImplementedException();
        return OnGetOwnedObjectsForOwner(owner);
    }

    public Task<MooObjectId?> GetParentAsync(MooObjectId objectId, CancellationToken cancellationToken)
    {
        if (OnGetParent == null)
            throw new NotImplementedException();
        return OnGetParent(objectId);
    }

    public Task<IReadOnlyList<MooVerbSummary>> GetVerbsAsync(MooObjectId objectId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<string>> GetVerbDocumentationAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MooPropertySummary>> GetPropertiesAsync(MooObjectId objectId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<MooVerbInfo?> GetVerbInfoAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<MooPropertyInfo?> GetPropertyInfoAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<string>> GetVerbCodeAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken)
    {
        if (OnGetVerbCode == null)
            throw new NotImplementedException();
        return OnGetVerbCode(objectId, verbName);
    }

    public Task<MooPropertyValue?> GetPropertyValueAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
