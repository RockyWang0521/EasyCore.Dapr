namespace EasyCore.Dapr.Bindings;

/// <summary>Dapr output binding invoke.</summary>
public interface IDaprBinding
{
    Task InvokeBindingAsync(
        string bindingName,
        string operation,
        object? data = null,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    Task<TResponse?> InvokeBindingAsync<TResponse>(
        string bindingName,
        string operation,
        object? data = null,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);
}
