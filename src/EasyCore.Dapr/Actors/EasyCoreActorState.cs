using Dapr.Actors.Runtime;

namespace EasyCore.Dapr.Actors;

/// <summary>
/// Default <see cref="IEasyCoreActorState"/> wrapping <see cref="IActorStateManager"/>.
/// </summary>
public sealed class EasyCoreActorState : IEasyCoreActorState
{
    private readonly IActorStateManager _stateManager;

    public EasyCoreActorState(IActorStateManager stateManager)
    {
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
    }

    public Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _stateManager.GetStateAsync<T>(key, cancellationToken);
    }

    public async Task<T> GetOrAddAsync<T>(string key, Func<T> factory, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        var exists = await _stateManager.ContainsStateAsync(key, cancellationToken).ConfigureAwait(false);
        if (exists)
            return await _stateManager.GetStateAsync<T>(key, cancellationToken).ConfigureAwait(false);

        var value = factory();
        await _stateManager.SetStateAsync(key, value, cancellationToken).ConfigureAwait(false);
        return value;
    }

    public Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _stateManager.SetStateAsync(key, value, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _stateManager.RemoveStateAsync(key, cancellationToken);
    }

    public Task<bool> ContainsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _stateManager.ContainsStateAsync(key, cancellationToken);
    }

    public Task SaveAsync(CancellationToken cancellationToken = default)
        => _stateManager.SaveStateAsync(cancellationToken);
}
