namespace EasyCore.Dapr.Workflow;

/// <summary>
/// Manages Dapr workflow instances (schedule, query, suspend/resume, raise events, terminate, purge).
/// Wraps <see cref="global::Dapr.Workflow.DaprWorkflowClient"/>.
/// </summary>
public interface IEasyCoreWorkflowClient
{
    /// <summary>Schedules a new workflow instance. Returns the instance id.</summary>
    Task<string> ScheduleAsync(
        string workflowName,
        object? input = null,
        string? instanceId = null,
        DateTime? startTime = null);

    Task<global::Dapr.Workflow.WorkflowState> GetStateAsync(
        string instanceId,
        bool getInputsAndOutputs = true);

    Task<global::Dapr.Workflow.WorkflowState> WaitForStartAsync(
        string instanceId,
        bool getInputsAndOutputs = true,
        CancellationToken cancellationToken = default);

    Task<global::Dapr.Workflow.WorkflowState> WaitForCompletionAsync(
        string instanceId,
        bool getInputsAndOutputs = true,
        CancellationToken cancellationToken = default);

    Task RaiseEventAsync(
        string instanceId,
        string eventName,
        object? eventPayload = null,
        CancellationToken cancellationToken = default);

    Task SuspendAsync(
        string instanceId,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task ResumeAsync(
        string instanceId,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task TerminateAsync(
        string instanceId,
        string? output = null,
        CancellationToken cancellationToken = default);

    Task PurgeAsync(
        string instanceId,
        CancellationToken cancellationToken = default);
}
