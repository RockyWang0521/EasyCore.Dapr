using EasyCore.Dapr.Actors;
using EasyCore.Dapr.Bindings;
using EasyCore.Dapr.Client;
using EasyCore.Dapr.Health;
using EasyCore.Dapr.Invocation;
using EasyCore.Dapr.PubSub;
using EasyCore.Dapr.Secrets;
using EasyCore.Dapr.State;
using EasyCore.Dapr.Workflow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace EasyCore.Dapr;

/// <summary>DI registration extensions for EasyCore.Dapr.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Dapr options, sidecar HttpClient, and optional sidecar health wait.
    /// </summary>
    public static IServiceCollection AddEasyCoreDapr(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DaprOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var builder = services.AddOptions<DaprOptions>()
            .Bind(configuration.GetSection(DaprOptions.SectionName))
            .ValidateOnStart();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<DaprOptions>, DaprOptionsValidator>());

        if (configure is not null)
            builder.Configure(configure);

        services.AddHttpClient(DaprSidecarClient.HttpClientName, (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<DaprOptions>>().Value;
            client.BaseAddress = options.ResolveHttpEndpoint();
            client.Timeout = options.Timeout;
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "EasyCore.Dapr/8.0");
        });

        services.TryAddSingleton<DaprSidecarClient>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, DaprSidecarHealthHostedService>());

        return services;
    }

    public static IServiceCollection AddEasyCoreDapr(
        this IHostApplicationBuilder builder,
        Action<DaprOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Services.AddEasyCoreDapr(builder.Configuration, configure);
    }

    public static IServiceCollection AddEasyCoreDaprInvocation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<DaprSidecarClient>();
        services.TryAddSingleton<IDaprInvoker, DaprInvoker>();
        return services;
    }

    public static IServiceCollection AddEasyCoreDaprState(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<DaprSidecarClient>();
        services.TryAddSingleton<IDaprStateStore, DaprStateStore>();
        return services;
    }

    public static IServiceCollection AddEasyCoreDaprPubSub(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<DaprSidecarClient>();
        services.TryAddSingleton<IDaprPubSub, DaprPubSub>();
        return services;
    }

    public static IServiceCollection AddEasyCoreDaprSecrets(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<DaprSidecarClient>();
        services.TryAddSingleton<IDaprSecrets, DaprSecrets>();
        return services;
    }

    public static IServiceCollection AddEasyCoreDaprBindings(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<DaprSidecarClient>();
        services.TryAddSingleton<IDaprBinding, DaprBinding>();
        return services;
    }

    /// <summary>Registers invocation + state + pub/sub + secrets + bindings in one call.</summary>
    public static IServiceCollection AddEasyCoreDaprAll(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services
            .AddEasyCoreDaprInvocation()
            .AddEasyCoreDaprState()
            .AddEasyCoreDaprPubSub()
            .AddEasyCoreDaprSecrets()
            .AddEasyCoreDaprBindings();
    }

    /// <summary>
    /// Registers official Dapr Actors runtime. Pair with <c>MapEasyCoreDaprActors()</c>.
    /// Each actor instance is single-threaded: concurrent calls are queued as serial turns
    /// (method-level atomic execution). Prefer <see cref="EasyCoreActor"/> for state helpers.
    /// </summary>
    public static IServiceCollection AddEasyCoreDaprActors(
        this IServiceCollection services,
        Action<EasyCoreDaprActorOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var actorOptions = new EasyCoreDaprActorOptions();
        configure?.Invoke(actorOptions);

        if (actorOptions.Registrations.Count == 0)
        {
            throw new InvalidOperationException(
                "AddEasyCoreDaprActors requires at least one RegisterActor<T>().");
        }

        services.AddActors(options =>
        {
            if (actorOptions.ActorIdleTimeout is { } idle)
                options.ActorIdleTimeout = idle;
            if (actorOptions.ActorScanInterval is { } scan)
                options.ActorScanInterval = scan;
            if (actorOptions.DrainOngoingCallTimeout is { } drain)
                options.DrainOngoingCallTimeout = drain;
            if (actorOptions.DrainRebalancedActors is { } drainRebalanced)
                options.DrainRebalancedActors = drainRebalanced;

            foreach (var register in actorOptions.Registrations)
                register(options);
        });

        return services;
    }

    /// <summary>
    /// Registers Dapr Workflow host + <see cref="IEasyCoreWorkflowClient"/>.
    /// Requires at least one <c>RegisterWorkflow</c> / <c>RegisterActivity</c>.
    /// Uses the official <c>Dapr.Workflow</c> runtime (gRPC to sidecar).
    /// </summary>
    public static IServiceCollection AddEasyCoreDaprWorkflow(
        this IServiceCollection services,
        Action<EasyCoreDaprWorkflowOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var workflowOptions = new EasyCoreDaprWorkflowOptions();
        configure?.Invoke(workflowOptions);

        if (workflowOptions.Registrations.Count == 0)
        {
            throw new InvalidOperationException(
                "AddEasyCoreDaprWorkflow requires at least one RegisterWorkflow / RegisterActivity.");
        }

        global::Dapr.Workflow.WorkflowServiceCollectionExtensions.AddDaprWorkflow(
            services,
            options =>
            {
                foreach (var register in workflowOptions.Registrations)
                    register(options);
            },
            workflowOptions.Lifetime);

        services.TryAdd(new ServiceDescriptor(
            typeof(IEasyCoreWorkflowClient),
            sp => new EasyCoreWorkflowClient(
                sp.GetRequiredService<global::Dapr.Workflow.DaprWorkflowClient>()),
            workflowOptions.Lifetime));

        return services;
    }

    #region Aliases

    public static IServiceCollection EasyCoreDapr(
        this IHostApplicationBuilder builder,
        Action<DaprOptions>? configure = null)
        => builder.AddEasyCoreDapr(configure);

    public static IServiceCollection EasyCoreDapr(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DaprOptions>? configure = null)
        => services.AddEasyCoreDapr(configuration, configure);

    public static IServiceCollection EasyCoreDaprInvocation(this IServiceCollection services)
        => services.AddEasyCoreDaprInvocation();

    public static IServiceCollection EasyCoreDaprState(this IServiceCollection services)
        => services.AddEasyCoreDaprState();

    public static IServiceCollection EasyCoreDaprPubSub(this IServiceCollection services)
        => services.AddEasyCoreDaprPubSub();

    public static IServiceCollection EasyCoreDaprSecrets(this IServiceCollection services)
        => services.AddEasyCoreDaprSecrets();

    public static IServiceCollection EasyCoreDaprBindings(this IServiceCollection services)
        => services.AddEasyCoreDaprBindings();

    public static IServiceCollection EasyCoreDaprAll(this IServiceCollection services)
        => services.AddEasyCoreDaprAll();

    public static IServiceCollection EasyCoreDaprActors(
        this IServiceCollection services,
        Action<EasyCoreDaprActorOptions>? configure = null)
        => services.AddEasyCoreDaprActors(configure);

    public static IServiceCollection EasyCoreDaprWorkflow(
        this IServiceCollection services,
        Action<EasyCoreDaprWorkflowOptions>? configure = null)
        => services.AddEasyCoreDaprWorkflow(configure);

    #endregion
}
