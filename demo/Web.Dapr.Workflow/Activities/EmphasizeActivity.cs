using EasyCore.Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace Web.Dapr.Workflow.Activities;

public sealed class EmphasizeActivity : EasyCoreWorkflowActivity<string, string>
{
    private readonly ILogger<EmphasizeActivity> _logger;

    public EmphasizeActivity(ILogger<EmphasizeActivity> logger)
    {
        _logger = logger;
    }

    public override Task<string> RunAsync(
        global::Dapr.Workflow.WorkflowActivityContext context,
        string message)
    {
        var result = message.TrimEnd('!', '.') + "!";
        _logger.LogInformation("EmphasizeActivity: {Result}", result);
        return Task.FromResult(result);
    }
}
