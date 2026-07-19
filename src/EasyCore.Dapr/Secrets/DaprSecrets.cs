using EasyCore.Dapr.Client;
using Microsoft.Extensions.Options;

namespace EasyCore.Dapr.Secrets;

internal sealed class DaprSecrets : IDaprSecrets
{
    private readonly DaprSidecarClient _client;
    private readonly IOptionsMonitor<DaprOptions> _options;

    public DaprSecrets(DaprSidecarClient client, IOptionsMonitor<DaprOptions> options)
    {
        _client = client;
        _options = options;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetSecretAsync(
        string secretName,
        string? storeName = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);
        var store = ResolveStore(storeName);
        var url = $"v1.0/secrets/{Uri.EscapeDataString(store)}/{Uri.EscapeDataString(secretName)}";
        var result = await _client.GetJsonAsync<Dictionary<string, string>>(url, cancellationToken)
            .ConfigureAwait(false);
        return result ?? new Dictionary<string, string>();
    }

    public async Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> GetBulkSecretAsync(
        string? storeName = null,
        CancellationToken cancellationToken = default)
    {
        var store = ResolveStore(storeName);
        var url = $"v1.0/secrets/{Uri.EscapeDataString(store)}/bulk";
        var result = await _client
            .GetJsonAsync<Dictionary<string, Dictionary<string, string>>>(url, cancellationToken)
            .ConfigureAwait(false);

        if (result is null)
            return new Dictionary<string, IReadOnlyDictionary<string, string>>();

        return result.ToDictionary(
            kv => kv.Key,
            kv => (IReadOnlyDictionary<string, string>)kv.Value);
    }

    private string ResolveStore(string? storeName)
        => string.IsNullOrWhiteSpace(storeName)
            ? _options.CurrentValue.DefaultSecretStore
            : storeName!;
}
