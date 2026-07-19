using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyCore.Dapr.Client;

/// <summary>Low-level HTTP client for the Dapr sidecar.</summary>
internal sealed class DaprSidecarClient
{
    public const string HttpClientName = "EasyCore.Dapr.Sidecar";

    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<DaprOptions> _options;
    private readonly ILogger<DaprSidecarClient> _logger;

    public DaprSidecarClient(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<DaprOptions> options,
        ILogger<DaprSidecarClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public Uri BaseAddress => _options.CurrentValue.ResolveHttpEndpoint();

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string relativeUrl,
        HttpContent? content = null,
        IEnumerable<KeyValuePair<string, string>>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);
        client.BaseAddress ??= BaseAddress;
        client.Timeout = _options.CurrentValue.Timeout;

        using var request = new HttpRequestMessage(method, relativeUrl.TrimStart('/'));
        request.Content = content;

        var token = _options.CurrentValue.ResolveApiToken();
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.TryAddWithoutValidation("dapr-api-token", token);

        if (headers is not null)
        {
            foreach (var (key, value) in headers)
            {
                if (!string.IsNullOrWhiteSpace(key) && value is not null)
                    request.Headers.TryAddWithoutValidation(key, value);
            }
        }

        var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogWarning(
                "Dapr sidecar {Method} {Url} failed: {Status} {Body}",
                method,
                relativeUrl,
                (int)response.StatusCode,
                body);
        }

        return response;
    }

    public async Task<T?> GetJsonAsync<T>(
        string relativeUrl,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, relativeUrl, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return default;

        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<string?> GetStringAsync(
        string relativeUrl,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, relativeUrl, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    public StringContent JsonContent(object value)
        => new(JsonSerializer.Serialize(value, JsonOptions), Encoding.UTF8, "application/json");

    public async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new DaprException(
            $"Dapr request failed ({(int)response.StatusCode}): {body}",
            (int)response.StatusCode,
            body);
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await SendAsync(HttpMethod.Get, "v1.0/healthz", cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Dapr sidecar health check failed");
            return false;
        }
    }
}

/// <summary>Exception thrown when a Dapr sidecar call fails.</summary>
public sealed class DaprException : Exception
{
    public int? StatusCode { get; }
    public string? ResponseBody { get; }

    public DaprException(string message, int? statusCode = null, string? responseBody = null)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
