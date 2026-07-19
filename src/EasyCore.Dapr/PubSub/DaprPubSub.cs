using EasyCore.Dapr.Client;
using Microsoft.Extensions.Options;

namespace EasyCore.Dapr.PubSub;

internal sealed class DaprPubSub : IDaprPubSub
{
    private readonly DaprSidecarClient _client;
    private readonly IOptionsMonitor<DaprOptions> _options;

    public DaprPubSub(DaprSidecarClient client, IOptionsMonitor<DaprOptions> options)
    {
        _client = client;
        _options = options;
    }

    public Task PublishEventAsync<T>(
        string topic,
        T data,
        string? pubSubName = null,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
        => PublishEventAsync(topic, (object)data!, pubSubName, metadata, cancellationToken);

    public async Task PublishEventAsync(
        string topic,
        object data,
        string? pubSubName = null,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentNullException.ThrowIfNull(data);

        var pubsub = string.IsNullOrWhiteSpace(pubSubName)
            ? _options.CurrentValue.DefaultPubSub
            : pubSubName!;

        var url = $"v1.0/publish/{Uri.EscapeDataString(pubsub)}/{Uri.EscapeDataString(topic)}";
        using var content = _client.JsonContent(data);
        using var response = await _client.SendAsync(
            HttpMethod.Post,
            url,
            content,
            metadata,
            cancellationToken).ConfigureAwait(false);
        await _client.EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }
}
