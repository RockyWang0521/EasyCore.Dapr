using System.ComponentModel.DataAnnotations;

namespace EasyCore.Dapr;

/// <summary>
/// Dapr sidecar options bound from configuration section <see cref="SectionName"/>.
/// </summary>
public sealed class DaprOptions
{
    public const string SectionName = "Dapr";

    /// <summary>
    /// Dapr HTTP sidecar base address (e.g. <c>http://127.0.0.1:3500/</c>).
    /// When empty: <c>DAPR_HTTP_ENDPOINT</c> → <c>DAPR_HTTP_PORT</c> → <c>http://127.0.0.1:3500/</c>.
    /// </summary>
    public string HttpEndpoint { get; set; } = string.Empty;

    /// <summary>Optional Dapr app id of this process (used by pub/sub subscribe metadata).</summary>
    public string? AppId { get; set; }

    /// <summary>
    /// Optional API token sent as <c>dapr-api-token</c>.
    /// When empty, falls back to <c>DAPR_API_TOKEN</c> environment variable.
    /// </summary>
    public string? ApiToken { get; set; }

    /// <summary>Default state store component name.</summary>
    public string DefaultStateStore { get; set; } = "statestore";

    /// <summary>Default pub/sub component name.</summary>
    public string DefaultPubSub { get; set; } = "pubsub";

    /// <summary>Default secret store component name.</summary>
    public string DefaultSecretStore { get; set; } = "secretstore";

    /// <summary>HTTP timeout for sidecar calls.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>When true, host waits for sidecar <c>/v1.0/healthz</c> before continuing start.</summary>
    public bool WaitForSidecar { get; set; } = true;

    /// <summary>Max time to wait for sidecar health when <see cref="WaitForSidecar"/> is true.</summary>
    public TimeSpan SidecarWaitTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Polling interval while waiting for sidecar health.</summary>
    public TimeSpan SidecarPollInterval { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>When true, sidecar wait failure throws and aborts host start.</summary>
    public bool FailFastIfSidecarUnavailable { get; set; } = true;

    /// <summary>Health endpoint path mapped by <c>UseEasyCoreDapr</c> for app probes.</summary>
    public string AppHealthPath { get; set; } = "/healthz";

    /// <summary>Resolves sidecar base URI with trailing slash.</summary>
    public Uri ResolveHttpEndpoint()
    {
        var endpoint = HttpEndpoint;
        if (string.IsNullOrWhiteSpace(endpoint))
            endpoint = Environment.GetEnvironmentVariable("DAPR_HTTP_ENDPOINT");

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            var port = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT");
            endpoint = string.IsNullOrWhiteSpace(port)
                ? "http://127.0.0.1:3500/"
                : $"http://127.0.0.1:{port}/";
        }

        if (!endpoint.EndsWith('/'))
            endpoint += "/";

        return new Uri(endpoint);
    }

    /// <summary>Resolves API token from options or environment.</summary>
    public string? ResolveApiToken()
    {
        if (!string.IsNullOrWhiteSpace(ApiToken))
            return ApiToken;

        var env = Environment.GetEnvironmentVariable("DAPR_API_TOKEN");
        return string.IsNullOrWhiteSpace(env) ? null : env;
    }
}
