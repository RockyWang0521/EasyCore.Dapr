using System.Runtime.Serialization;
using Dapr.Actors;

namespace Web.Dapr.Actors.Actors;

public interface ICounterActor : IActor
{
    Task<int> GetAsync();

    Task<int> IncrementAsync(int by);

    /// <summary>
    /// Multiple state writes in one exclusive actor turn (no concurrent interleaving on this id).
    /// </summary>
    Task<CounterSnapshot> ApplyBatchAsync(int a, int b);
}

[DataContract]
public sealed class CounterSnapshot
{
    public CounterSnapshot()
    {
    }

    public CounterSnapshot(int n, int a, int b, int sum)
    {
        N = n;
        A = a;
        B = b;
        Sum = sum;
    }

    [DataMember(Order = 1)]
    public int N { get; set; }

    [DataMember(Order = 2)]
    public int A { get; set; }

    [DataMember(Order = 3)]
    public int B { get; set; }

    [DataMember(Order = 4)]
    public int Sum { get; set; }
}

