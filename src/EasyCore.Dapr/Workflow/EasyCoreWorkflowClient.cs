namespace EasyCore.Dapr.Workflow;

/// <summary>
/// Default <see cref="IEasyCoreWorkflowClient"/> over <see cref="global::Dapr.Workflow.DaprWorkflowClient"/>.
/// </summary>
public sealed class EasyCoreWorkflowClient : IEasyCoreWorkflowClient
{
    private readonly global::Dapr.Workflow.DaprWorkflowClient _inner;

    public EasyCoreWorkflowClient(global::Dapr.Workflow.DaprWorkflowClient inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public Task<string> ScheduleAsync(
        string workflowName,
        object? input = null,
        string? instanceId = null,
        DateTime? startTime = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowName);
        return _inner.ScheduleNewWorkflowAsync(workflowName, instanceId, input, startTime);
    }

    public Task<global::Dapr.Workflow.WorkflowState> GetStateAsync(
        string instanceId,
        bool getInputsAndOutputs = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        return _inner.GetWorkflowStateAsync(instanceId, getInputsAndOutputs);
    }

    public Task<global::Dapr.Workflow.WorkflowState> WaitForStartAsync(
        string instanceId,
        bool getInputsAndOutputs = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        return _inner.WaitForWorkflowStartAsync(instanceId, getInputsAndOutputs, cancellationToken);
    }

    public Task<global::Dapr.Workflow.WorkflowState> WaitForCompletionAsync(
        string instanceId,
        bool getInputsAndOutputs = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        return _inner.WaitForWorkflowCompletionAsync(instanceId, getInputsAndOutputs, cancellationToken);
    }

    public Task RaiseEventAsync(
        string instanceId,
        string eventName,
        object? eventPayload = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        return _inner.RaiseEventAsync(instanceId, eventName, eventPayload, cancellationToken);
    }

    public Task SuspendAsync(
        string instanceId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        return _inner.SuspendWorkflowAsync(instanceId, reason, cancellationToken);
    }

    public Task ResumeAsync(
        string instanceId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        return _inner.ResumeWorkflowAsync(instanceId, reason, cancellationToken);
    }

    public Task TerminateAsync(
        string instanceId,
        string? output = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        return _inner.TerminateWorkflowAsync(instanceId, output, cancellationToken);
    }

    public Task PurgeAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        return _inner.PurgeInstanceAsync(instanceId, cancellationToken);
    }
}
