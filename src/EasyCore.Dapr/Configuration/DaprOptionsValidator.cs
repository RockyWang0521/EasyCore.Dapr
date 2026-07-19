using Microsoft.Extensions.Options;

namespace EasyCore.Dapr;

internal sealed class DaprOptionsValidator : IValidateOptions<DaprOptions>
{
    public ValidateOptionsResult Validate(string? name, DaprOptions options)
    {
        if (options is null)
            return ValidateOptionsResult.Fail("Dapr options are required.");

        if (options.Timeout <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Dapr:Timeout must be greater than zero.");

        if (options.SidecarWaitTimeout <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Dapr:SidecarWaitTimeout must be greater than zero.");

        if (options.SidecarPollInterval <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Dapr:SidecarPollInterval must be greater than zero.");

        try
        {
            _ = options.ResolveHttpEndpoint();
        }
        catch (Exception ex)
        {
            return ValidateOptionsResult.Fail($"Dapr:HttpEndpoint is invalid: {ex.Message}");
        }

        return ValidateOptionsResult.Success;
    }
}
