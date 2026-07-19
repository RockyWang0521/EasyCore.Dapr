using EasyCore.Dapr.Workflow;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.Dapr.Tests;

public class WorkflowDependencyInjectionTests
{
    [Fact]
    public async Task AddEasyCoreDaprWorkflow_Registers_Client()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEasyCoreDaprWorkflow(o =>
        {
            o.RegisterWorkflow<SmokeWorkflow>();
            o.RegisterActivity<SmokeActivity>();
        });

        await using var sp = services.BuildServiceProvider();
        sp.GetRequiredService<IEasyCoreWorkflowClient>().Should().NotBeNull();
        sp.GetRequiredService<global::Dapr.Workflow.DaprWorkflowClient>().Should().NotBeNull();
    }

    [Fact]
    public void AddEasyCoreDaprWorkflow_Without_Registrations_Throws()
    {
        var services = new ServiceCollection();
        var act = () => services.AddEasyCoreDaprWorkflow();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*RegisterWorkflow*");
    }

    private sealed class SmokeWorkflow : EasyCoreWorkflow<string, string>
    {
        public override Task<string> RunAsync(
            global::Dapr.Workflow.WorkflowContext context,
            string input)
            => Task.FromResult(input);
    }

    private sealed class SmokeActivity : EasyCoreWorkflowActivity<string, string>
    {
        public override Task<string> RunAsync(
            global::Dapr.Workflow.WorkflowActivityContext context,
            string input)
            => Task.FromResult(input);
    }
}
