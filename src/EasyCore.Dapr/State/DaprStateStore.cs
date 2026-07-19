using System.Text.Json;
using EasyCore.Dapr.Client;
using Microsoft.Extensions.Options;

namespace EasyCore.Dapr.State;

internal sealed class DaprStateStore : IDaprStateStore
{
    private readonly DaprSidecarClient _client;
    private readonly IOptionsMonitor<DaprOptions> _options;

    public DaprStateStore(DaprSidecarClient client, IOptionsMonitor<DaprOptions> options)
    {
        _client = client;
        _options = options;
    }

    public async Task<T?> GetStateAsync<T>(
        string key,
        string? storeName = null,
        CancellationToken cancellationToken = default)
    {
        var (value, _) = await GetStateAndETagAsync<T>(key, storeName, cancellationToken).ConfigureAwait(false);
        return value;
    }

    public async Task<(T? Value, string? ETag)> GetStateAndETagAsync<T>(
        string key,
        string? storeName = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var store = ResolveStore(storeName);
        var url = $"v1.0/state/{Uri.EscapeDataString(store)}/{Uri.EscapeDataString(key)}";

        using var response = await _client.SendAsync(HttpMethod.Get, url, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent ||
            response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return (default, null);
        }

        await _client.EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        var etag = response.Headers.ETag?.Tag?.Trim('"');
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json))
            return (default, etag);

        var value = JsonSerializer.Deserialize<T>(json, DaprSidecarClient.JsonOptions);
        return (value, etag);
    }

    public async Task SaveStateAsync<T>(
        string key,
        T value,
        string? storeName = null,
        string? eTag = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var store = ResolveStore(storeName);
        var url = $"v1.0/state/{Uri.EscapeDataString(store)}";

        var payload = new[]
        {
            new Dictionary<string, object?>
            {
                ["key"] = key,
                ["value"] = value,
                ["etag"] = eTag
            }
        };

        using var content = _client.JsonContent(payload);
        if (!string.IsNullOrWhiteSpace(eTag))
            content.Headers.TryAddWithoutValidation("If-Match", eTag);

        using var response = await _client.SendAsync(HttpMethod.Post, url, content, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        await _client.EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteStateAsync(
        string key,
        string? storeName = null,
        string? eTag = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var store = ResolveStore(storeName);
        var url = $"v1.0/state/{Uri.EscapeDataString(store)}/{Uri.EscapeDataString(key)}";

        IEnumerable<KeyValuePair<string, string>>? headers = null;
        if (!string.IsNullOrWhiteSpace(eTag))
            headers = [new("If-Match", eTag!)];

        using var response = await _client.SendAsync(HttpMethod.Delete, url, headers: headers, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        await _client.EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task SaveBulkStateAsync(
        IReadOnlyList<DaprStateItem> items,
        string? storeName = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Count == 0)
            return;

        var store = ResolveStore(storeName);
        var url = $"v1.0/state/{Uri.EscapeDataString(store)}";
        var payload = items.Select(i => new Dictionary<string, object?>
        {
            ["key"] = i.Key,
            ["value"] = i.Value,
            ["etag"] = i.ETag
        }).ToArray();

        using var content = _client.JsonContent(payload);
        using var response = await _client.SendAsync(HttpMethod.Post, url, content, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        await _client.EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private string ResolveStore(string? storeName)
        => string.IsNullOrWhiteSpace(storeName)
            ? _options.CurrentValue.DefaultStateStore
            : storeName!;
}
