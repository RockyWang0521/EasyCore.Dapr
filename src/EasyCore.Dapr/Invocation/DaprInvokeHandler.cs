namespace EasyCore.Dapr.Invocation;

/// <summary>
/// HTTP message handler that rewrites outbound requests to Dapr service invocation URLs.
/// Suitable for named HttpClients and AspNetCore.Mvc remote proxies.
/// </summary>
public sealed class DaprInvokeHandler : DelegatingHandler
{
    private readonly string _appId;

    public DaprInvokeHandler(string appId)
    {
        _appId = string.IsNullOrWhiteSpace(appId)
            ? throw new ArgumentException("Dapr app id is required.", nameof(appId))
            : appId;
    }

    /// <summary>Target Dapr app id.</summary>
    public string AppId => _appId;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.RequestUri = DaprInvokePaths.RewriteRequestUri(request.RequestUri, _appId);
        return base.SendAsync(request, cancellationToken);
    }
}
