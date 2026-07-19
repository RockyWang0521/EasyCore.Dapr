using EasyCore.Dapr;
using EasyCore.Dapr.Workflow;
using Web.Dapr.Workflow.Activities;
using Web.Dapr.Workflow.Workflows;

var builder = WebApplication.CreateBuilder(args);

builder.EasyCoreDapr(o =>
{
    o.AppId = "easycore-workflow";
    o.WaitForSidecar = false;
    o.FailFastIfSidecarUnavailable = false;
});

builder.Services.EasyCoreDaprWorkflow(wf =>
{
    wf.RegisterWorkflow<HelloWorkflow>();
    wf.RegisterActivity<GreetActivity>();
    wf.RegisterActivity<EmphasizeActivity>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapEasyCoreDaprHealth();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapPost("/api/workflows/hello", async (HelloStartRequest? request, IEasyCoreWorkflowClient workflows) =>
{
    var name = request?.Name ?? "EasyCore";
    var instanceId = request?.InstanceId ?? $"hello-{Guid.NewGuid():N}"[..16];

    await workflows.ScheduleAsync(
        nameof(HelloWorkflow),
        new HelloInput(name),
        instanceId);

    return Results.Accepted($"/api/workflows/{instanceId}", new { instanceId, workflow = nameof(HelloWorkflow) });
});

app.MapGet("/api/workflows/{instanceId}", async (string instanceId, IEasyCoreWorkflowClient workflows) =>
{
    var state = await workflows.GetStateAsync(instanceId);
    if (!state.Exists)
        return Results.NotFound(new { instanceId });

    HelloResult? output = null;
    try
    {
        output = state.ReadOutputAs<HelloResult>();
    }
    catch
    {
        // still running or no output yet
    }

    return Results.Ok(new
    {
        instanceId,
        status = state.RuntimeStatus.ToString(),
        createdAt = state.CreatedAt,
        lastUpdatedAt = state.LastUpdatedAt,
        output
    });
});

app.MapPost("/api/workflows/{instanceId}/terminate", async (string instanceId, IEasyCoreWorkflowClient workflows) =>
{
    await workflows.TerminateAsync(instanceId);
    return Results.Accepted();
});

app.Run();

sealed record HelloStartRequest(string? Name, string? InstanceId);
