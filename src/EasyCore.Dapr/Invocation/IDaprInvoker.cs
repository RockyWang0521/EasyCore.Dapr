namespace EasyCore.Dapr.Invocation;

/// <summary>Invokes methods on other Dapr applications through the local sidecar.</summary>
public interface IDaprInvoker
{
    Task<TResponse?> InvokeMethodAsync<TResponse>(
        HttpMethod method,
        string appId,
        string methodName,
        CancellationToken cancellationToken = default);

    Task<TResponse?> InvokeMethodAsync<TRequest, TResponse>(
        HttpMethod method,
        string appId,
        string methodName,
        TRequest data,
        CancellationToken cancellationToken = default);

    Task InvokeMethodAsync(
        HttpMethod method,
        string appId,
        string methodName,
        CancellationToken cancellationToken = default);

    Task InvokeMethodAsync<TRequest>(
        HttpMethod method,
        string appId,
        string methodName,
        TRequest data,
        CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> InvokeMethodRawAsync(
        HttpMethod method,
        string appId,
        string methodName,
        HttpContent? content = null,
        IEnumerable<KeyValuePair<string, string>>? headers = null,
        CancellationToken cancellationToken = default);
}
