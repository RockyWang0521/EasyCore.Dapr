using EasyCore.Dapr.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyCore.Dapr.Health;

/// <summary>Optionally blocks host start until the Dapr sidecar reports healthy.</summary>
internal sealed class DaprSidecarHealthHostedService : IHostedService
{
    private readonly DaprSidecarClient _client;
    private readonly IOptions<DaprOptions> _options;
    private readonly ILogger<DaprSidecarHealthHostedService> _logger;

    public DaprSidecarHealthHostedService(
        DaprSidecarClient client,
        IOptions<DaprOptions> options,
        ILogger<DaprSidecarHealthHostedService> logger)
    {
        _client = client;
        _options = options;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var options = _options.Value;
        if (!options.WaitForSidecar)
        {
            _logger.LogInformation("Dapr sidecar wait is disabled.");
            return;
        }

        var deadline = DateTimeOffset.UtcNow + options.SidecarWaitTimeout;
        _logger.LogInformation(
            "Waiting for Dapr sidecar at {Endpoint} (timeout {Timeout})",
            options.ResolveHttpEndpoint(),
            options.SidecarWaitTimeout);

        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await _client.IsHealthyAsync(cancellationToken).ConfigureAwait(false))
            {
                _logger.LogInformation("Dapr sidecar is healthy.");
                return;
            }

            await Task.Delay(options.SidecarPollInterval, cancellationToken).ConfigureAwait(false);
        }

        var message =
            $"Dapr sidecar was not healthy within {options.SidecarWaitTimeout} at {options.ResolveHttpEndpoint()}.";

        if (options.FailFastIfSidecarUnavailable)
        {
            _logger.LogError(message);
            throw new DaprException(message);
        }

        _logger.LogWarning(message);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
