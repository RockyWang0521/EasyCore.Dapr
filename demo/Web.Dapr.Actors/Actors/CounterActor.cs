using Dapr.Actors.Runtime;
using EasyCore.Dapr.Actors;

namespace Web.Dapr.Actors.Actors;

/// <summary>
/// Demo actor: same instance is single-threaded — concurrent invokes are serialized (atomic turns).
/// </summary>
public sealed class CounterActor : EasyCoreActor, ICounterActor
{
    private const string KeyN = "n";
    private const string KeyA = "a";
    private const string KeyB = "b";
    private const string KeySum = "sum";

    public CounterActor(ActorHost host) : base(host)
    {
    }

    public Task<int> GetAsync()
        => State.GetOrAddAsync(KeyN, () => 0);

    public async Task<int> IncrementAsync(int by)
    {
        var n = await State.GetOrAddAsync(KeyN, () => 0);
        n += by;
        await State.SetAsync(KeyN, n);
        // Runtime persists pending state when this turn completes successfully.
        return n;
    }

    public async Task<CounterSnapshot> ApplyBatchAsync(int a, int b)
    {
        // Multiple writes in one method still run under one exclusive turn —
        // another caller cannot interleave with this actor instance.
        await State.SetAsync(KeyA, a);
        await State.SetAsync(KeyB, b);

        var sum = a + b;
        await State.SetAsync(KeySum, sum);

        var n = await State.GetOrAddAsync(KeyN, () => 0);
        n += sum;
        await State.SetAsync(KeyN, n);

        // Optional mid-turn flush (usually unnecessary):
        // await State.SaveAsync();

        return new CounterSnapshot(n, a, b, sum);
    }
}
