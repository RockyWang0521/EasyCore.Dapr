using Dapr.Actors.Runtime;
using Microsoft.Extensions.Logging;

namespace EasyCore.Dapr.Actors;

/// <summary>
/// Base actor using the official Dapr Actors runtime.
/// <para>
/// Each actor instance is <b>single-threaded</b>: concurrent method calls are queued and run as
/// serial <b>turns</b>. One method invocation completes atomically before the next begins —
/// that is the concurrency model (not a business Unit of Work).
/// </para>
/// </summary>
public abstract class EasyCoreActor : Actor
{
    private IEasyCoreActorState? _state;

    protected EasyCoreActor(ActorHost host) : base(host)
    {
    }

    /// <summary>
    /// Actor state helpers for the current turn (wraps <c>StateManager</c>).
    /// </summary>
    protected IEasyCoreActorState State
        => _state ??= new EasyCoreActorState(StateManager);

    /// <summary>
    /// Logger bound to this actor instance.
    /// </summary>
    protected ILogger ActorLogger => Logger;
}
