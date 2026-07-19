using Dapr.Actors;
using Dapr.Actors.Runtime;
using EasyCore.Dapr.Actors;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.Dapr.Tests;

public class ActorDependencyInjectionTests
{
    [Fact]
    public void AddEasyCoreDaprActors_Registers_Actor_Runtime()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEasyCoreDaprActors(o => o.RegisterActor<SmokeActor>());

        using var sp = services.BuildServiceProvider();
        sp.GetRequiredService<ActorRuntime>().Should().NotBeNull();
    }

    [Fact]
    public void AddEasyCoreDaprActors_Without_RegisterActor_Throws()
    {
        var services = new ServiceCollection();
        var act = () => services.AddEasyCoreDaprActors();
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
