namespace EasyCore.Dapr.State;

/// <summary>Dapr state store operations.</summary>
public interface IDaprStateStore
{
    Task<T?> GetStateAsync<T>(
        string key,
        string? storeName = null,
        CancellationToken cancellationToken = default);

    Task<(T? Value, string? ETag)> GetStateAndETagAsync<T>(
        string key,
        string? storeName = null,
        CancellationToken cancellationToken = default);

    Task SaveStateAsync<T>(
        string key,
        T value,
        string? storeName = null,
        string? eTag = null,
        CancellationToken cancellationToken = default);

    Task DeleteStateAsync(
        string key,
        string? storeName = null,
        string? eTag = null,
        CancellationToken cancellationToken = default);

    Task SaveBulkStateAsync(
        IReadOnlyList<DaprStateItem> items,
        string? storeName = null,
        CancellationToken cancellationToken = default);
}

/// <summary>Bulk state item.</summary>
public sealed class DaprStateItem
{
    public required string Key { get; init; }
    public object? Value { get; init; }
    public string? ETag { get; init; }
}
