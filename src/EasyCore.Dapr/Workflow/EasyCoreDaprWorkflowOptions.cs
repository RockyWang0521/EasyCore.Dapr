using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.Dapr.Workflow;

/// <summary>
/// Options for EasyCore Dapr Workflow registration.
/// See <see href="https://docs.dapr.io/zh-hans/developing-applications/building-blocks/workflow/workflow-overview/">Dapr Workflow overview</see>.
/// </summary>
public sealed class EasyCoreDaprWorkflowOptions
{
    /// <summary>
    /// Lifetime of <see cref="global::Dapr.Workflow.DaprWorkflowClient"/> and related services. Default: Singleton.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Singleton;

    internal List<Action<global::Dapr.Workflow.WorkflowRuntimeOptions>> Registrations { get; } = [];

    /// <summary>
    /// Registers a workflow class deriving from <c>Workflow&lt;TInput,TOutput&gt;</c>.
    /// </summary>
    public EasyCoreDaprWorkflowOptions RegisterWorkflow<TWorkflow>()
        where TWorkflow : class, global::Dapr.Workflow.IWorkflow, new()
    {
        Registrations.Add(options => options.RegisterWorkflow<TWorkflow>());
        return this;
    }

    /// <summary>
    /// Registers a workflow activity class deriving from <c>WorkflowActivity&lt;TInput,TOutput&gt;</c>.
    /// </summary>
    public EasyCoreDaprWorkflowOptions RegisterActivity<TActivity>()
        where TActivity : class, global::Dapr.Workflow.IWorkflowActivity
    {
        Registrations.Add(options => options.RegisterActivity<TActivity>());
        return this;
    }

    /// <summary>
    /// Registers a functional workflow definition.
    /// </summary>
    public EasyCoreDaprWorkflowOptions RegisterWorkflow<TInput, TOutput>(
        string name,
        Func<global::Dapr.Workflow.WorkflowContext, TInput, Task<TOutput>> implementation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(implementation);
        Registrations.Add(options => options.RegisterWorkflow(name, implementation));
        return this;
    }

    /// <summary>
    /// Registers a functional activity definition.
    /// </summary>
    public EasyCoreDaprWorkflowOptions RegisterActivity<TInput, TOutput>(
        string name,
        Func<global::Dapr.Workflow.WorkflowActivityContext, TInput, Task<TOutput>> implementation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(implementation);
        Registrations.Add(options => options.RegisterActivity(name, implementation));
        return this;
    }
}
