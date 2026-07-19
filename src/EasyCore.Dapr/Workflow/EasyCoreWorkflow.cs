namespace EasyCore.Dapr.Workflow;

/// <summary>
/// EasyCore base for Dapr workflows. Orchestrate activities; keep code deterministic
/// (no I/O, random, or wall-clock time on the workflow thread — use
/// <see cref="global::Dapr.Workflow.WorkflowContext"/> helpers).
/// </summary>
/// <typeparam name="TInput">JSON-serializable input.</typeparam>
/// <typeparam name="TOutput">JSON-serializable output.</typeparam>
public abstract class EasyCoreWorkflow<TInput, TOutput> : global::Dapr.Workflow.Workflow<TInput, TOutput>
{
}
