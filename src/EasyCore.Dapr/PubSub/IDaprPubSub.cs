namespace EasyCore.Dapr.PubSub;

/// <summary>Dapr pub/sub publisher.</summary>
public interface IDaprPubSub
{
    Task PublishEventAsync<T>(
        string topic,
        T data,
        string? pubSubName = null,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    Task PublishEventAsync(
        string topic,
        object data,
        string? pubSubName = null,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);
}
