using EasyCore.Dapr;
using EasyCore.Dapr.Client;
using EasyCore.Dapr.Invocation;
using EasyCore.Dapr.PubSub;
using EasyCore.Dapr.State;

var builder = WebApplication.CreateBuilder(args);

builder.AddEasyCoreDapr(o =>
{
    o.AppId = "easycore-web";
    o.WaitForSidecar = false;
    o.FailFastIfSidecarUnavailable = false;
}).AddEasyCoreDaprAll();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseEasyCoreDapr(endpoints =>
{
    endpoints.Subscribe(
        topic: "orders",
        route: "/events/orders",
        handler: async (cloudEvent, ct) =>
        {
            await Task.CompletedTask;
            Console.WriteLine($"[orders] {cloudEvent}");
        });
});

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapGet("/api/invoke-hello", async (IDaprInvoker invoker) =>
{
    try
    {
        var result = await invoker.InvokeMethodAsync<HelloDto>(
            HttpMethod.Get,
            "easycore-service",
            "api/hello");
        return Results.Ok(result);
    }
    catch (DaprException ex)
    {
        return Results.Problem(
            detail: ex.Message + " — start sidecar + Web.Dapr.Service (see README).",
            statusCode: ex.StatusCode ?? 502);
    }
});

app.MapPost("/api/state/{key}", async (string key, StateInput input, IDaprStateStore state) =>
{
    await state.SaveStateAsync(key, input);
    return Results.Accepted();
});

app.MapGet("/api/state/{key}", async (string key, IDaprStateStore state) =>
{
    var value = await state.GetStateAsync<StateInput>(key);
    return value is null ? Results.NotFound() : Results.Ok(value);
});

app.MapPost("/api/publish", async (PublishInput input, IDaprPubSub pubsub) =>
{
    await pubsub.PublishEventAsync(input.Topic, input.Payload);
    return Results.Accepted();
});

app.Run();

sealed record HelloDto(string Message, DateTimeOffset At);
sealed record StateInput(string Name, int Count);
sealed record PublishInput(string Topic, object Payload);
