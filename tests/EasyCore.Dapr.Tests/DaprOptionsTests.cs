using FluentAssertions;

namespace EasyCore.Dapr.Tests;

public class DaprOptionsTests
{
    [Fact]
    public void ResolveHttpEndpoint_Defaults_To_Local_3500()
    {
        var option = new DaprOptions { HttpEndpoint = string.Empty };
        option.ResolveHttpEndpoint().ToString().Should().Be("http://127.0.0.1:3500/");
    }

    [Fact]
    public void ResolveHttpEndpoint_Appends_Trailing_Slash()
    {
        var option = new DaprOptions { HttpEndpoint = "http://127.0.0.1:3501" };
        option.ResolveHttpEndpoint().ToString().Should().Be("http://127.0.0.1:3501/");
    }

    [Fact]
    public void Validator_Fails_On_Zero_Timeout()
    {
        var validator = new DaprOptionsValidator();
        var result = validator.Validate(null, new DaprOptions { Timeout = TimeSpan.Zero });
        result.Failed.Should().BeTrue();
    }
}
