# 🚀 EasyCore.Dapr

> **EasyCore.Dapr** is a production-ready [Dapr](https://dapr.io/) integration for .NET 8: **HTTP sidecar** building blocks (invoke, state, pub/sub, secrets, bindings — **no** `Dapr.Client` SDK) + **Dapr Actors** (single-threaded atomic turns) + **[Dapr Workflow](https://docs.dapr.io/developing-applications/building-blocks/workflow/workflow-overview/)** (durable orchestration).

<p align="center">
  <img src="https://raw.githubusercontent.com/RockyWang0521/EasyCore.Dapr/master/png/EasyCoreLogo.png" alt="EasyCore Logo" width="120" />
</p>

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![Dapr](https://img.shields.io/badge/Dapr-HTTP%20API-0d61ff)
![Actors](https://img.shields.io/badge/Actors-Workflow-6f42c1)
![License](https://img.shields.io/badge/License-MIT%20OR%20Apache--2.0-yellow)
![Version](https://img.shields.io/badge/Version-8.0.0-blue)

---

## 🌍 Language

- Chinese: [README.md](https://github.com/RockyWang0521/EasyCore.Dapr/blob/master/README.md)
- **English (this document)**

Source: [github.com/RockyWang0521/EasyCore.Dapr](https://github.com/RockyWang0521/EasyCore.Dapr)

---

## 📚 Table of Contents

### Part I — Overview & Architecture
- [1. Positioning](#1--positioning)
- [2. Architecture & Data Flow](#2--architecture--data-flow)
- [3. Repository Layout](#3--repository-layout)

### Part II — Getting Started
- [4. Installation](#4--installation)
- [5. Quick Start](#5--quick-start)
- [6. Options](#6--options)
- [7. Extensions](#7--extensions)

### Part III — Capabilities, Demo & Production
- [8. Capability Cheatsheet](#8--capability-cheatsheet)
- [9. Demo](#9--demo)
- [10. Production Checklist](#10--production-checklist)
- [11. FAQ](#11--faq)
- [12. License](#12--license)

---

## 1. 🎯 Positioning

EasyCore.Dapr solves “wire Dapr with Options + DI instead of hand-written sidecar URLs / scattered SDKs”:

| Pain point | EasyCore.Dapr approach |
|---|---|
| Hand-written sidecar URL / token | Options + automatic `dapr-api-token` |
| Sidecar not ready at startup | `WaitForSidecar` + FailFast |
| Hard-to-trim coupling | Per-capability `Add*` for Invoke / State / PubSub / Secrets / Bindings |
| Actor races / interleaving | Official single-threaded turns + `EasyCoreActor` / `IEasyCoreActorState` |
| Long-running orchestration | `EasyCoreDaprWorkflow` + `IEasyCoreWorkflowClient` |
| Reuse Mvc remote channels | Public `DaprInvokeHandler` / `DaprInvokePaths` |

Complements `EasyCore.AspNetCore.Mvc` `[DaprApp]`: Mvc proxies remote APIs; this package hosts sidecar building blocks, Actors, and Workflow.

### 1.1 Design Principles

| Principle | Meaning |
|---|---|
| **Low friction** | Options + selective `Add*` / `Use*` |
| **HTTP blocks without Dapr.Client** | Invoke / State / PubSub / Secrets / Bindings via sidecar HTTP |
| **Trimable defaults** | Register only what you need |
| **Official Actor / Workflow** | Still `Dapr.Actors` / `Dapr.Workflow`; EasyCore is a thin layer |
| **Demo-verifiable** | `dapr run` + curl steps in the README |

---

## 2. 🏗️ Architecture & Data Flow

### 2.1 Components (text diagram)

```text
┌─────────────────────────────────────────────────────────────┐
│  ASP.NET Core Host                                          │
│                                                             │
│   EasyCoreDapr → DaprSidecarClient (named HttpClient)    │
│            ├─ IDaprInvoker      → /v1.0/invoke/...          │
│            ├─ IDaprStateStore   → /v1.0/state/...           │
│            ├─ IDaprPubSub       → /v1.0/publish/...         │
│            ├─ IDaprSecrets      → /v1.0/secrets/...         │
│            └─ IDaprBinding      → /v1.0/bindings/...        │
│   UseEasyCoreDapr → /healthz + optional GET /dapr/subscribe │
│                                                             │
│   EasyCoreDaprActors → official Actors (single-threaded) │
│            └─ EasyCoreActor.State (IEasyCoreActorState)     │
│   MapEasyCoreDaprActors → MapActorsHandlers                 │
│                                                             │
│   EasyCoreDaprWorkflow → Workflow host (gRPC)            │
│            ├─ EasyCoreWorkflow / EasyCoreWorkflowActivity   │
│            └─ IEasyCoreWorkflowClient                       │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Local debug sequence

```text
[Docker Desktop] ──► dapr init (Redis / Placement / Scheduler)
         ▲
         │ sidecar
[dapr run + Demo] ──► Invoke / State / PubSub / Actors / Workflow
         │
         ▼
   Verify with curl / Swagger
```

---

## 3. 📁 Repository Layout

```text
EasyCore.Dapr/
├── src/EasyCore.Dapr/
│   ├── Actors/                 # EasyCoreActor / ActorOptions
│   ├── Bindings/ Secrets/      # Bindings & secrets
│   ├── Client/                 # DaprSidecarClient
│   ├── Configuration/          # DaprOptions + Validator
│   ├── DependencyInjection/    # Add* / Use* / Map*
│   ├── Health/ Invocation/     # Health wait / service invoke
│   ├── PubSub/ State/          # Pub/sub / state
│   └── Workflow/               # Workflow wrappers + Client
├── demo/
│   ├── Web.Dapr.Service/       # Invoke target
│   ├── Web.Dapr/               # Invoke / State / PubSub
│   ├── Web.Dapr.Actors/        # CounterActor
│   └── Web.Dapr.Workflow/      # HelloWorkflow
├── components/                 # Redis state / pubsub (actorStateStore)
├── tests/EasyCore.Dapr.Tests/
├── README.md
└── README.en.md
```

---

## 4. 📦 Installation

```bash
dotnet add package EasyCore.Dapr
```

Requires **.NET 8** and a Dapr sidecar (default `http://127.0.0.1:3500`).

Locally: install the [Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/) and run `dapr init` (needs Docker Desktop).

---

## 5. ⚡ Quick Start

### 5.1 Code registration

```csharp
using EasyCore.Dapr;

var builder = WebApplication.CreateBuilder(args);

builder.EasyCoreDapr()
    .EasyCoreDaprAll(); // or EasyCoreDaprInvocation / State / PubSub ...

var app = builder.Build();
app.UseEasyCoreDapr(endpoints =>
{
    endpoints.Subscribe("orders", "/events/orders", async (evt, ct) =>
    {
        await Task.CompletedTask;
    });
});
app.Run();
```

### 5.2 Configuration binding

```json
{
  "Dapr": {
    "HttpEndpoint": "http://127.0.0.1:3500/",
    "AppId": "easycore-web",
    "DefaultStateStore": "statestore",
    "DefaultPubSub": "pubsub",
    "WaitForSidecar": true,
    "FailFastIfSidecarUnavailable": true,
    "SidecarWaitTimeout": "00:00:30"
  }
}
```

Endpoint resolution: `Dapr:HttpEndpoint` → `DAPR_HTTP_ENDPOINT` → `DAPR_HTTP_PORT` → `http://127.0.0.1:3500/`.  
API token: `Dapr:ApiToken` → `DAPR_API_TOKEN`.

---

## 6. ⚙️ Options

| Key | Description | Default | Icon |
|---|---|---|---|
| `HttpEndpoint` | Sidecar HTTP base URL | empty (see resolution) | 🔌 |
| `AppId` | This process Dapr app id | `null` | 🏷️ |
| `ApiToken` | `dapr-api-token` | empty (env fallback) | 🔑 |
| `DefaultStateStore` | Default state store name | `statestore` | 🗄️ |
| `DefaultPubSub` | Default pub/sub name | `pubsub` | 📣 |
| `DefaultSecretStore` | Default secret store name | `secretstore` | 🤫 |
| `Timeout` | Sidecar HTTP timeout | `00:00:30` | ⏱️ |
| `WaitForSidecar` | Wait for `/v1.0/healthz` | `true` | ⏳ |
| `SidecarWaitTimeout` | Wait timeout | `00:00:30` | ⌛ |
| `SidecarPollInterval` | Poll interval | `00:00:00.500` | 🔁 |
| `FailFastIfSidecarUnavailable` | Abort host if wait fails | `true` | 🛑 |
| `AppHealthPath` | App health path | `/healthz` | ❤️ |

> Demos set `WaitForSidecar=false` so plain `dotnet run` still starts; Actor / Workflow calls still need a sidecar.

---

## 7. 🧩 Extensions

| Extension | Registers | Icon |
|---|---|---|
| `EasyCoreDapr` | Options, sidecar client, health wait | 🧱 |
| `EasyCoreDaprInvocation` | `IDaprInvoker` | 📞 |
| `EasyCoreDaprState` | `IDaprStateStore` (ETag) | 🗄️ |
| `EasyCoreDaprPubSub` | `IDaprPubSub` | 📣 |
| `EasyCoreDaprSecrets` | `IDaprSecrets` | 🤫 |
| `EasyCoreDaprBindings` | `IDaprBinding` | 🔗 |
| `EasyCoreDaprAll` | All HTTP capabilities above | 📦 |
| `EasyCoreDaprActors` | Actors runtime + `RegisterActor<T>()` | 🎭 |
| `EasyCoreDaprWorkflow` | Workflow host + `IEasyCoreWorkflowClient` | 🔄 |
| `UseEasyCoreDapr` | `/healthz` + optional `/dapr/subscribe` | 🛠️ |
| `MapEasyCoreDaprActors` | Actor HTTP handlers | 🗺️ |

---

## 8. 📖 Capability Cheatsheet

### 8.1 📞 Service invocation

```csharp
var dto = await invoker.InvokeMethodAsync<HelloDto>(HttpMethod.Get, "easycore-service", "api/hello");
```

Reuse from Mvc remote channels:

```csharp
services.AddHttpClient("x")
    .AddHttpMessageHandler(() => new DaprInvokeHandler("provider"));
```

### 8.2 🗄️ State

```csharp
await state.SaveStateAsync("user:1", new { Name = "a" });
var (value, etag) = await state.GetStateAndETagAsync<MyType>("user:1");
```

### 8.3 📣 Pub/Sub

```csharp
await pubsub.PublishEventAsync("orders", new { Id = 1 });
// UseEasyCoreDapr(e => e.Subscribe("orders", "/events/orders", handler));
```

### 8.4 🎭 Actor atomic turns

**Same actor instance = single-threaded execution.** Concurrent calls are queued; one method turn finishes before the next. That is method-level atomic serialization — **not** `EasyCore.UnitOfWork`, and **not** the State Transaction API.

Still powered by official `Dapr.Actors`. `EasyCoreActor.State` (`IEasyCoreActorState`) is a thin `StateManager` helper. Prefer end-of-turn auto-save; use `State.SaveAsync()` only for mid-turn flushes.

```csharp
builder.Services.EasyCoreDaprActors(a => a.RegisterActor<CounterActor>());
app.MapEasyCoreDaprActors();

public class CounterActor : EasyCoreActor, ICounterActor
{
    public async Task<int> IncrementAsync(int by)
    {
        var n = await State.GetOrAddAsync("n", () => 0);
        n += by;
        await State.SetAsync("n", n);
        return n;
    }
}
```

> Actor interface methods must not use `out` / `ref` / optional parameters. Custom return types must be serializable (DataContract by default, or configure JSON).

### 8.5 🔄 Dapr Workflow

[Dapr Workflow](https://docs.dapr.io/developing-applications/building-blocks/workflow/workflow-overview/) provides durable, resilient orchestration.

**Actor ≠ Workflow**: Actor = per-instance serial atomic turns; Workflow = replay-safe durable orchestration across activities.

```csharp
builder.Services.EasyCoreDaprWorkflow(wf =>
{
    wf.RegisterWorkflow<HelloWorkflow>();
    wf.RegisterActivity<GreetActivity>();
    wf.RegisterActivity<EmphasizeActivity>();
});

await workflows.ScheduleAsync(nameof(HelloWorkflow), input, instanceId);
var state = await workflows.GetStateAsync(instanceId);
```

Requires a [workflow-capable state store](https://docs.dapr.io/developing-applications/building-blocks/workflow/workflow-overview/#limitations) and gRPC to the sidecar (`DAPR_GRPC_PORT`). This repo’s `components/statestore.yaml` sets `actorStateStore: true`.

---

## 9. 🧪 Demo

| Project | Port | Role |
|---|---|---|
| `demo/Web.Dapr.Service` | 5288 | Invoke target `/api/hello` |
| `demo/Web.Dapr` | 5287 | Invoke / State / Publish / Subscribe |
| `demo/Web.Dapr.Actors` | 5289 | CounterActor + ActorProxy |
| `demo/Web.Dapr.Workflow` | 5290 | HelloWorkflow + activities |

### 9.1 🐳 Prerequisites: Docker + Dapr CLI

1. Start **Docker Desktop**  
2. Install and init Dapr: `dapr init` (Redis / Placement / Scheduler)  
3. Ensure Redis on `6379` (`dapr_redis` or your own)

### 9.2 🚀 Recommended startup

```bash
dapr run --app-id easycore-service --app-port 5288 --dapr-http-port 3501 -- dotnet run --project demo/Web.Dapr.Service
dapr run --app-id easycore-web --app-port 5287 --dapr-http-port 3500 --resources-path ./components -- dotnet run --project demo/Web.Dapr
dapr run --app-id easycore-actors --app-port 5289 --dapr-http-port 3502 --resources-path ./components -- dotnet run --project demo/Web.Dapr.Actors
dapr run --app-id easycore-workflow --app-port 5290 --dapr-http-port 3503 --dapr-grpc-port 50001 --resources-path ./components -- dotnet run --project demo/Web.Dapr.Workflow
```

> When running multiple demos in parallel, use **distinct** `--dapr-http-port` / `--dapr-grpc-port` values.

### 9.3 👀 Sample verification

**HTTP building blocks**

```bash
curl http://localhost:5287/api/invoke-hello
curl -X POST http://localhost:5287/api/state/demo-key -H "Content-Type: application/json" -d "{\"name\":\"EasyCore\",\"count\":42}"
curl http://localhost:5287/api/state/demo-key
curl -X POST http://localhost:5287/api/publish -H "Content-Type: application/json" -d "{\"topic\":\"orders\",\"payload\":{\"orderId\":\"O-1\"}}"
```

**Actors**

```bash
curl -X POST http://localhost:5289/api/counters/demo/batch -H "Content-Type: application/json" -d "{\"a\":1,\"b\":2}"
curl http://localhost:5289/api/counters/demo
```

**Workflow**

```bash
curl -X POST http://localhost:5290/api/workflows/hello -H "Content-Type: application/json" -d "{\"name\":\"EasyCore\"}"
curl http://localhost:5290/api/workflows/<instanceId>
```

Without a sidecar, demos still start with `WaitForSidecar=false` for compile / health checks.

---

## 10. ✅ Production Checklist

- [ ] Enable `WaitForSidecar=true` with a sensible `SidecarWaitTimeout`
- [ ] Configure `DAPR_API_TOKEN` / `ApiToken`
- [ ] Distinct `DefaultStateStore` / `DefaultPubSub` per environment
- [ ] Use ETag for state write conflicts (`GetStateAndETagAsync` + `If-Match`)
- [ ] Idempotent pub/sub handlers; return non-2xx to trigger redelivery
- [ ] Actors: actor state store; rely on official single-threaded turns
- [ ] Actors: prefer end-of-turn save; use mid-turn `SaveAsync` sparingly
- [ ] Workflow: workflow-capable store; deterministic code; careful versioning
- [ ] Workflow: set `DAPR_GRPC_PORT`; manage long instances with Suspend/Resume/RaiseEvent
- [ ] Do not expose the sidecar port publicly

---

## 11. ❓ FAQ

**Q: Invoke fails with sidecar unavailable?**  
A: Confirm `dapr run` is active and `Dapr:HttpEndpoint` / `DAPR_HTTP_PORT` match the sidecar port.

**Q: Actor startup fails on optional parameters?**  
A: Actor interface methods cannot have default optional parameters — make them required and default at the call site.

**Q: Actor return type serialization failed?**  
A: Default is DataContract — add `[DataContract]` / `[DataMember]` and a parameterless constructor, or enable Actors JSON serialization.

**Q: Workflow never reaches Completed?**  
A: Ensure the state store has `actorStateStore: true` and `DAPR_GRPC_PORT` matches `dapr run`.

**Q: Relation to EasyCore.AspNetCore.Mvc `[DaprApp]`?**  
A: Mvc proxies remote APIs; this package hosts sidecar building blocks, Actors, and Workflow — they complement each other.

---

## 12. 📄 License

MIT OR Apache-2.0

### 🤝 Contributing

1. Fork and create a feature branch  
2. Add tests under `tests/EasyCore.Dapr.Tests`  
3. Run `dotnet test` and `dotnet build`  
4. Open a Pull Request  

Issues / PRs welcome 🚀
