using EasyCore.Dapr.Workflow;
using Web.Dapr.Workflow.Activities;

namespace Web.Dapr.Workflow.Workflows;

/// <summary>
/// Demo orchestration: greet → emphasize. Activities do the work; workflow stays deterministic.
/// </summary>
public sealed class HelloWorkflow : EasyCoreWorkflow<HelloInput, HelloResult>
{
    public override async Task<HelloResult> RunAsync(
        global::Dapr.Workflow.WorkflowContext context,
        HelloInput input)
    {
        var greeted = await context.CallActivityAsync<string>(
            nameof(GreetActivity),
            input.Name);

        var message = await context.CallActivityAsync<string>(
            nameof(EmphasizeActivity),
            greeted);

        return new HelloResult(message, context.InstanceId);
    }
}

public sealed record HelloInput(string Name);

public sealed record HelloResult(string Message, string InstanceId);
