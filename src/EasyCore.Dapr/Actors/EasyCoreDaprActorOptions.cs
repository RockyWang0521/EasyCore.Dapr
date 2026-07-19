using Dapr.Actors.Runtime;

namespace EasyCore.Dapr.Actors;

/// <summary>
/// Options for EasyCore Dapr Actors registration.
/// </summary>
public sealed class EasyCoreDaprActorOptions
{
    /// <summary>
    /// Actor idle timeout passed to the Dapr actor runtime (optional).
    /// </summary>
    public TimeSpan? ActorIdleTimeout { get; set; }

    /// <summary>
    /// Actor scan interval for idle deactivation (optional).
    /// </summary>
    public TimeSpan? ActorScanInterval { get; set; }

    /// <summary>
    /// Drain timeout when deactivating actors (optional).
    /// </summary>
    public TimeSpan? DrainOngoingCallTimeout { get; set; }

    /// <summary>
    /// When true, ongoing calls are drained before deactivation.
    /// </summary>
    public bool? DrainRebalancedActors { get; set; }

    internal List<Action<ActorRuntimeOptions>> Registrations { get; } = [];

    /// <summary>
    /// Registers an actor type with the Dapr actor runtime.
    /// </summary>
    public EasyCoreDaprActorOptions RegisterActor<TActor>()
        where TActor : Actor
    {
        Registrations.Add(options => options.Actors.RegisterActor<TActor>());
        return this;
    }

    /// <summary>
    /// Registers an actor type with a custom type name.
    /// </summary>
    public EasyCoreDaprActorOptions RegisterActor<TActor>(string actorTypeName)
        where TActor : Actor
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorTypeName);
        Registrations.Add(options => options.Actors.RegisterActor<TActor>(actorTypeName));
        return this;
    }
}
