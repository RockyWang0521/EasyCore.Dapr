using EasyCore.Dapr.Bindings;
using EasyCore.Dapr.Invocation;
using EasyCore.Dapr.PubSub;
using EasyCore.Dapr.Secrets;
using EasyCore.Dapr.State;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.Dapr.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddEasyCoreDaprAll_Registers_Building_Blocks()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Dapr:HttpEndpoint"] = "http://127.0.0.1:3500/",
                ["Dapr:WaitForSidecar"] = "false"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEasyCoreDapr(config).AddEasyCoreDaprAll();

        using var sp = services.BuildServiceProvider();
        sp.GetRequiredService<IDaprInvoker>().Should().NotBeNull();
        sp.GetRequiredService<IDaprStateStore>().Should().NotBeNull();
        sp.GetRequiredService<IDaprPubSub>().Should().NotBeNull();
        sp.GetRequiredService<IDaprSecrets>().Should().NotBeNull();
        sp.GetRequiredService<IDaprBinding>().Should().NotBeNull();
    }
}
