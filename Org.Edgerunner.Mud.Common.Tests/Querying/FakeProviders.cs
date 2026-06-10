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

    public Func<MooObjectId, string, Task<MooVerbCode?>>? OnGetVerbCode { get; set; }

    public Func<MooObjectId, string, Task<MooVerbDocumentation?>>? OnGetVerbDocumentation { get; set; }

    public Func<MooObjectId, string, Task<MooVerbInfo?>>? OnGetVerbInfo { get; set; }

    public Func<MooObjectId, string, Task<IReadOnlyList<string>>>? OnGetPropertyDocumentation { get; set; }

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

    public Task<MooVerbDocumentation?> GetVerbDocumentationAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken)
    {
        if (OnGetVerbDocumentation == null)
            throw new NotImplementedException();
        return OnGetVerbDocumentation(objectId, verbName);
    }

    public Task<IReadOnlyList<MooPropertySummary>> GetPropertiesAsync(MooObjectId objectId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<string>> GetPropertyDocumentationAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken)
    {
        if (OnGetPropertyDocumentation == null)
            throw new NotImplementedException();
        return OnGetPropertyDocumentation(objectId, propName);
    }

    public Task<MooVerbInfo?> GetVerbInfoAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken)
    {
        if (OnGetVerbInfo == null)
            throw new NotImplementedException();
        return OnGetVerbInfo(objectId, verbName);
    }

    public Task<MooPropertyInfo?> GetPropertyInfoAsync(MooObjectId objectId, string propName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<MooVerbCode?> GetVerbCodeAsync(MooObjectId objectId, string verbName, CancellationToken cancellationToken)
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
