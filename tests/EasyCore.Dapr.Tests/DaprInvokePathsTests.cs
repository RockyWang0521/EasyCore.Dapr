using EasyCore.Dapr.Invocation;
using FluentAssertions;

namespace EasyCore.Dapr.Tests;

public class DaprInvokePathsTests
{
    [Fact]
    public void BuildInvokeRelativeUri_Rewrites_Path()
    {
        var uri = DaprInvokePaths.BuildInvokeRelativeUri("provider", "api/Foo/GetDto");
        uri.ToString().Should().Be("v1.0/invoke/provider/method/api/Foo/GetDto");
    }

    [Fact]
    public void BuildInvokeRelativeUri_Preserves_Query()
    {
        var uri = DaprInvokePaths.BuildInvokeRelativeUri("provider", "/api/Foo/DeleteDto?id=1");
        uri.ToString().Should().Be("v1.0/invoke/provider/method/api/Foo/DeleteDto?id=1");
    }

    [Fact]
    public void RewriteRequestUri_Keeps_Absolute_Authority()
    {
        var rewritten = DaprInvokePaths.RewriteRequestUri(
            new Uri("http://127.0.0.1:3500/api/Foo/GetDto"),
            "provider");

        rewritten.ToString().Should().Be(
            "http://127.0.0.1:3500/v1.0/invoke/provider/method/api/Foo/GetDto");
    }

    [Fact]
    public async Task DaprInvokeHandler_Rewrites_Outgoing_Uri()
    {
        var handler = new DaprInvokeHandler("provider")
        {
            InnerHandler = new AssertHandler(req =>
            {
                req.RequestUri!.ToString().Should().Be(
                    "http://127.0.0.1:3500/v1.0/invoke/provider/method/api/hello");
            })
        };

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:3500/") };
        var response = await client.GetAsync("api/hello");
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    private sealed class AssertHandler : HttpMessageHandler
    {
        private readonly Action<HttpRequestMessage> _assert;

        public AssertHandler(Action<HttpRequestMessage> assert) => _assert = assert;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _assert(request);
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }
}
