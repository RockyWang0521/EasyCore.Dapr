namespace EasyCore.Dapr.Secrets;

/// <summary>Dapr secret store access.</summary>
public interface IDaprSecrets
{
    Task<IReadOnlyDictionary<string, string>> GetSecretAsync(
        string secretName,
        string? storeName = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> GetBulkSecretAsync(
        string? storeName = null,
        CancellationToken cancellationToken = default);
}
