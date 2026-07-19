using System.Text.Json;
using EasyCore.Dapr.Client;

namespace EasyCore.Dapr.Invocation;

internal sealed class DaprInvoker : IDaprInvoker
{
    private readonly DaprSidecarClient _client;

    public DaprInvoker(DaprSidecarClient client) => _client = client;

    public async Task<TResponse?> InvokeMethodAsync<TResponse>(
        HttpMethod method,
        string appId,
        string methodName,
        CancellationToken cancellationToken = default)
    {
        using var response = await InvokeMethodRawAsync(method, appId, methodName, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        await _client.EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return default;

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<TResponse>(stream, DaprSidecarClient.JsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<TResponse?> InvokeMethodAsync<TRequest, TResponse>(
        HttpMethod method,
        string appId,
        string methodName,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        using var content = _client.JsonContent(data!);
        using var response = await InvokeMethodRawAsync(method, appId, methodName, content, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        await _client.EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return default;

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<TResponse>(stream, DaprSidecarClient.JsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task InvokeMethodAsync(
        HttpMethod method,
        string appId,
        string methodName,
        CancellationToken cancellationToken = default)
    {
        using var response = await InvokeMethodRawAsync(method, appId, methodName, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        await _client.EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task InvokeMethodAsync<TRequest>(
        HttpMethod method,
        string appId,
        string methodName,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        using var content = _client.JsonContent(data!);
        using var response = await InvokeMethodRawAsync(method, appId, methodName, content, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        await _client.EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public Task<HttpResponseMessage> InvokeMethodRawAsync(
        HttpMethod method,
        string appId,
        string methodName,
        HttpContent? content = null,
        IEnumerable<KeyValuePair<string, string>>? headers = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appId);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);

        var relative = DaprInvokePaths.BuildInvokeRelativeUri(appId, methodName).ToString();
        return _client.SendAsync(method, relative, content, headers, cancellationToken);
    }
}
