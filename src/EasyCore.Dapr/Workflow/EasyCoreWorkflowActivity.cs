namespace EasyCore.Dapr.Workflow;

/// <summary>
/// EasyCore base for Dapr workflow activities — a single orchestrated step
/// (call services, state, pub/sub, or external APIs). Supports constructor DI.
/// </summary>
/// <typeparam name="TInput">JSON-serializable input.</typeparam>
/// <typeparam name="TOutput">JSON-serializable output.</typeparam>
public abstract class EasyCoreWorkflowActivity<TInput, TOutput>
    : global::Dapr.Workflow.WorkflowActivity<TInput, TOutput>
{
}
