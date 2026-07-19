using Dapr.Actors;
using Dapr.Actors.Client;
using EasyCore.Dapr;
using Web.Dapr.Actors.Actors;

var builder = WebApplication.CreateBuilder(args);

builder.AddEasyCoreDapr(o =>
{
    o.AppId = "easycore-actors";
    o.WaitForSidecar = false;
    o.FailFastIfSidecarUnavailable = false;
});

builder.Services.AddEasyCoreDaprActors(actors =>
{
    actors.RegisterActor<CounterActor>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapEasyCoreDaprHealth();
app.MapEasyCoreDaprActors();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapGet("/api/counters/{id}", async (string id) =>
{
    var proxy = CreateProxy(id);
    var value = await proxy.GetAsync();
    return Results.Ok(new { id, value });
});

app.MapPost("/api/counters/{id}/increment", async (string id, IncrementInput? input) =>
{
    var proxy = CreateProxy(id);
    var value = await proxy.IncrementAsync(input?.By ?? 1);
    return Results.Ok(new { id, value });
});

app.MapPost("/api/counters/{id}/batch", async (string id, BatchInput input) =>
{
    var proxy = CreateProxy(id);
    var snapshot = await proxy.ApplyBatchAsync(input.A, input.B);
    return Results.Ok(new { id, snapshot });
});

app.Run();

static ICounterActor CreateProxy(string id)
    => ActorProxy.Create<ICounterActor>(new ActorId(id), nameof(CounterActor));

sealed record IncrementInput(int By);
sealed record BatchInput(int A, int B);
