using EasyCore.Dapr;

var builder = WebApplication.CreateBuilder(args);

// Provider app: typically started with `dapr run --app-id easycore-service --app-port 5288 ...`
builder.Services.EasyCoreDapr(builder.Configuration, o =>
{
    o.AppId = "easycore-service";
    o.WaitForSidecar = false; // allow plain `dotnet run` without sidecar
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseEasyCoreDapr();

app.MapGet("/api/hello", () => Results.Ok(new { message = "hello from Web.Dapr.Service", at = DateTimeOffset.UtcNow }));
app.MapPost("/api/echo", (EchoInput input) => Results.Ok(new { echoed = input.Text }));

app.Run();

sealed record EchoInput(string Text);
