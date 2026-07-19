using Dapr.Actors;
using Dapr.Actors.Runtime;
using EasyCore.Dapr.Actors;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.Dapr.Tests;

public class ActorDependencyInjectionTests
{
    [Fact]
    public void EasyCoreDaprActors_Registers_Actor_Runtime()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.EasyCoreDaprActors(o => o.RegisterActor<SmokeActor>());

        using var sp = services.BuildServiceProvider();
        sp.GetRequiredService<ActorRuntime>().Should().NotBeNull();
    }

    [Fact]
    public void EasyCoreDaprActors_Without_RegisterActor_Throws()
    {
        var services = new ServiceCollection();
        var act = () => services.EasyCoreDaprActors();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*RegisterActor*");
    }

    private interface ISmokeActor : IActor
    {
        Task PingAsync();
    }

    private sealed class SmokeActor : EasyCoreActor, ISmokeActor
    {
        public SmokeActor(ActorHost host) : base(host)
        {
        }

        public Task PingAsync() => Task.CompletedTask;
    }
}
