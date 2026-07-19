namespace EasyCore.Dapr.Actors;

/// <summary>
/// Thin wrapper over actor <c>StateManager</c> for Get/Set within a single-threaded turn.
/// Concurrent calls to the same actor instance are serialized by the Dapr actor runtime;
/// call <see cref="SaveAsync"/> only when you need a mid-turn durable flush.
/// </summary>
public interface IEasyCoreActorState
{
    Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task<T> GetOrAddAsync<T>(string key, Func<T> factory, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> ContainsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Forces <c>StateManager.SaveStateAsync</c> before the turn ends.
    /// Prefer relying on end-of-turn auto-save for normal cases.
    /// </summary>
    Task SaveAsync(CancellationToken cancellationToken = default);
}
