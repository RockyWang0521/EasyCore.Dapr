using System.Text.Json;
using EasyCore.Dapr.Client;

namespace EasyCore.Dapr.Bindings;

internal sealed class DaprBinding : IDaprBinding
{
    private readonly DaprSidecarClient _client;

    public DaprBinding(DaprSidecarClient client) => _client = client;

    public async Task InvokeBindingAsync(
        string bindingName,
        string operation,
        object? data = null,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(bindingName, operation, data, metadata, cancellationToken)
            .ConfigureAwait(false);
        await _client.EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TResponse?> InvokeBindingAsync<TResponse>(
        string bindingName,
        string operation,
        object? data = null,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(bindingName, operation, data, metadata, cancellationToken)
            .ConfigureAwait(false);
        await _client.EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return default;

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<TResponse>(stream, DaprSidecarClient.JsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    private Task<HttpResponseMessage> SendAsync(
        string bindingName,
        string operation,
        object? data,
        IReadOnlyDictionary<string, string>? metadata,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bindingName);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);

        var payload = new Dictionary<string, object?>
        {
            ["operation"] = operation,
            ["data"] = data,
            ["metadata"] = metadata
        };

        var url = $"v1.0/bindings/{Uri.EscapeDataString(bindingName)}";
        var content = _client.JsonContent(payload);
        return _client.SendAsync(HttpMethod.Post, url, content, cancellationToken: cancellationToken);
    }
}
