namespace EasyCore.Dapr.Invocation;

/// <summary>Helpers for Dapr service-invocation URL shapes.</summary>
public static class DaprInvokePaths
{
    /// <summary>
    /// Builds a relative invoke URI: <c>v1.0/invoke/{appId}/method/{pathAndQuery}</c>.
    /// </summary>
    public static Uri BuildInvokeRelativeUri(string appId, string? pathAndQuery)
    {
        if (string.IsNullOrWhiteSpace(appId))
            throw new ArgumentException("Dapr app id is required.", nameof(appId));

        var pq = string.IsNullOrWhiteSpace(pathAndQuery) ? "/" : pathAndQuery!;
        if (!pq.StartsWith('/'))
            pq = "/" + pq;

        var method = pq.TrimStart('/');
        return new Uri($"v1.0/invoke/{Uri.EscapeDataString(appId)}/method/{method}", UriKind.Relative);
    }

    /// <summary>
    /// Rewrites a request URI to the Dapr invoke form, preserving absolute authority when present.
    /// </summary>
    public static Uri RewriteRequestUri(Uri? requestUri, string appId)
    {
        var relative = BuildInvokeUri(requestUri, appId);
        if (requestUri is { IsAbsoluteUri: true })
        {
            var authority = requestUri.GetLeftPart(UriPartial.Authority);
            if (!authority.EndsWith('/'))
                authority += "/";
            return new Uri(new Uri(authority), relative);
        }

        return relative;
    }

    internal static Uri BuildInvokeUri(Uri? requestUri, string appId)
    {
        var pathAndQuery = "/";
        if (requestUri != null)
        {
            pathAndQuery = requestUri.IsAbsoluteUri
                ? requestUri.PathAndQuery
                : requestUri.OriginalString;

            if (string.IsNullOrWhiteSpace(pathAndQuery))
                pathAndQuery = "/";
        }

        return BuildInvokeRelativeUri(appId, pathAndQuery);
    }
}
