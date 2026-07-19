using EasyCore.Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace Web.Dapr.Workflow.Activities;

public sealed class GreetActivity : EasyCoreWorkflowActivity<string, string>
{
    private readonly ILogger<GreetActivity> _logger;

    public GreetActivity(ILogger<GreetActivity> logger)
    {
        _logger = logger;
    }

    public override Task<string> RunAsync(
        global::Dapr.Workflow.WorkflowActivityContext context,
        string name)
    {
        var value = string.IsNullOrWhiteSpace(name) ? "world" : name.Trim();
        var message = $"Hello, {value}";
        _logger.LogInformation("GreetActivity: {Message}", message);
        return Task.FromResult(message);
    }
}
