using System.Text.Json;
using EasyCore.Dapr.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EasyCore.Dapr;

/// <summary>ASP.NET Core endpoint helpers for Dapr.</summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Maps app health endpoint and optional Dapr pub/sub subscription discovery endpoint.
    /// </summary>
    public static WebApplication UseEasyCoreDapr(
        this WebApplication app,
        Action<DaprEndpointOptions>? configureEndpoints = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        var endpointOptions = new DaprEndpointOptions();
        configureEndpoints?.Invoke(endpointOptions);

        var daprOptions = app.Services.GetService<IOptions<DaprOptions>>()?.Value;
        var healthPath = daprOptions?.AppHealthPath ?? "/healthz";
        app.MapGet(healthPath, () => Results.Ok(new { status = "ok" }));

        if (endpointOptions.Subscriptions.Count > 0)
        {
            app.MapGet("/dapr/subscribe", () =>
            {
                var payload = endpointOptions.Subscriptions.Select(s => new
                {
                    pubsubname = s.PubSubName,
                    topic = s.Topic,
                    route = s.Route,
                    metadata = s.Metadata
                });
                return Results.Json(payload, DaprSidecarClient.JsonOptions);
            });

            foreach (var sub in endpointOptions.Subscriptions)
            {
                var route = sub.Route.StartsWith('/') ? sub.Route : "/" + sub.Route;
                app.MapPost(route, async (HttpRequest request, CancellationToken ct) =>
                {
                    if (sub.Handler is null)
                        return Results.Accepted();

                    using var doc = await JsonDocument.ParseAsync(request.Body, cancellationToken: ct)
                        .ConfigureAwait(false);
                    await sub.Handler(doc.RootElement, ct).ConfigureAwait(false);
                    return Results.Ok();
                });
            }
        }

        return app;
    }

    /// <summary>Maps only the app health endpoint.</summary>
    public static IEndpointConventionBuilder MapEasyCoreDaprHealth(
        this IEndpointRouteBuilder endpoints,
        string? pattern = null)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        var path = pattern
                   ?? endpoints.ServiceProvider.GetService<IOptions<DaprOptions>>()?.Value.AppHealthPath
                   ?? "/healthz";
        return endpoints.MapGet(path, () => Results.Ok(new { status = "ok" }));
    }

    /// <summary>
    /// Maps Dapr actor HTTP handlers. Requires <c>EasyCoreDaprActors</c>.
    /// </summary>
    public static IEndpointRouteBuilder MapEasyCoreDaprActors(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapActorsHandlers();
        return endpoints;
    }
}

/// <summary>Endpoint wiring for Dapr subscribe routes.</summary>
public sealed class DaprEndpointOptions
{
    internal List<DaprSubscriptionRegistration> Subscriptions { get; } = [];

    /// <summary>
    /// Registers a topic subscription discovered via <c>GET /dapr/subscribe</c>
    /// and handled by <paramref name="route"/> POST endpoint.
    /// </summary>
    public DaprEndpointOptions Subscribe(
        string topic,
        string route,
        Func<JsonElement, CancellationToken, Task>? handler = null,
        string? pubSubName = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentException.ThrowIfNullOrWhiteSpace(route);

        Subscriptions.Add(new DaprSubscriptionRegistration
        {
            Topic = topic,
            Route = route.StartsWith('/') ? route : "/" + route,
            PubSubName = pubSubName ?? "pubsub",
            Handler = handler,
            Metadata = metadata
        });
        return this;
    }
}

internal sealed class DaprSubscriptionRegistration
{
    public required string Topic { get; init; }
    public required string Route { get; init; }
    public required string PubSubName { get; init; }
    public Func<JsonElement, CancellationToken, Task>? Handler { get; init; }
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}
