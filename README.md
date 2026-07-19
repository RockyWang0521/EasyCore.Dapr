# 🚀 EasyCore.Dapr

> **EasyCore.Dapr** 是面向 .NET 8 的生产级 [Dapr](https://dapr.io/) 集成库：HTTP sidecar 构建块（服务调用、状态、发布订阅、密钥与绑定，**不依赖** `Dapr.Client`）+ **Dapr Actors**（单线程回合 / 方法级原子串行）+ **[Dapr Workflow](https://docs.dapr.io/zh-hans/developing-applications/building-blocks/workflow/workflow-overview/)**（持久化编排）。

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

- **中文（当前文档）**
- English: [README.en.md](https://github.com/RockyWang0521/EasyCore.Dapr/blob/master/README.en.md)

源码：[github.com/RockyWang0521/EasyCore.Dapr](https://github.com/RockyWang0521/EasyCore.Dapr)

---

## 📚 目录

### 第一部分：总览与架构
- [1. 项目定位](#1--项目定位)
- [2. 架构与数据流](#2--架构与数据流)
- [3. 仓库结构](#3--仓库结构)

### 第二部分：快速上手
- [4. 安装](#4--安装)
- [5. 三分钟快速开始](#5--三分钟快速开始)
- [6. 配置项](#6--配置项)
- [7. 扩展方法](#7--扩展方法)

### 第三部分：能力、Demo 与生产
- [8. 能力速查](#8--能力速查)
- [9. Demo](#9--demo)
- [10. 生产清单](#10--生产清单)
- [11. FAQ](#11--faq)
- [12. License](#12--license)

---

## 1. 🎯 项目定位

EasyCore.Dapr 解决「在 ASP.NET Core 里用 Options + DI 接好 Dapr，而不是手写 sidecar URL / 散落 SDK」的问题：

| 痛点 | EasyCore.Dapr 做法 |
|---|---|
| 手写 sidecar URL / Token | Options + `dapr-api-token` 自动注入 |
| 启动时 sidecar 未就绪 | `WaitForSidecar` + FailFast |
| 能力耦合难裁剪 | Invoke / State / PubSub / Secrets / Bindings 按需 `Add*` |
| Actor 并发交错 / 竞态 | 官方 Actor 单线程 turn + `EasyCoreActor` / `IEasyCoreActorState` |
| 长时业务编排难落地 | `AddEasyCoreDaprWorkflow` + `IEasyCoreWorkflowClient` |
| Mvc 远端通道要复用 | 公开 `DaprInvokeHandler` / `DaprInvokePaths` |

与 `EasyCore.AspNetCore.Mvc` 的 `[DaprApp]` 远端代理互补：Mvc 管接口代理；本包装 sidecar 平台能力、Actor 与 Workflow 宿主。

### 1.1 设计原则

| 原则 | 说明 |
|---|---|
| **低摩擦接入** | Options + 按需 `Add*` / `Use*` 即可跑通 |
| **HTTP 构建块无 Dapr.Client** | Invoke / State / PubSub / Secrets / Bindings 走 sidecar HTTP API |
| **默认可裁剪** | 各构建块独立注册，不必一次全上 |
| **官方 Actor / Workflow** | 运行时仍用 `Dapr.Actors` / `Dapr.Workflow`，EasyCore 做薄封装 |
| **Demo 可验证** | README 给出 `dapr run` + curl 联调步骤 |

---

## 2. 🏗️ 架构与数据流

### 2.1 组件关系（文本示意）

```text
┌─────────────────────────────────────────────────────────────┐
│  ASP.NET Core Host                                          │
│                                                             │
│   AddEasyCoreDapr → DaprSidecarClient (named HttpClient)    │
│            ├─ IDaprInvoker      → /v1.0/invoke/...          │
│            ├─ IDaprStateStore   → /v1.0/state/...           │
│            ├─ IDaprPubSub       → /v1.0/publish/...         │
│            ├─ IDaprSecrets      → /v1.0/secrets/...         │
│            └─ IDaprBinding      → /v1.0/bindings/...        │
│   UseEasyCoreDapr → /healthz + optional GET /dapr/subscribe │
│                                                             │
│   AddEasyCoreDaprActors → 官方 Actors（单线程 turn）         │
│            └─ EasyCoreActor.State (IEasyCoreActorState)     │
│   MapEasyCoreDaprActors → MapActorsHandlers                 │
│                                                             │
│   AddEasyCoreDaprWorkflow → Workflow host (gRPC)            │
│            ├─ EasyCoreWorkflow / EasyCoreWorkflowActivity   │
│            └─ IEasyCoreWorkflowClient                       │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 本地联调顺序

```text
[Docker Desktop] ──► dapr init（Redis / Placement / Scheduler）
         ▲
         │ sidecar
[dapr run + Demo] ──► Invoke / State / PubSub / Actors / Workflow
         │
         ▼
   curl / Swagger 验证 API
```

---

## 3. 📁 仓库结构

```text
EasyCore.Dapr/
├── src/EasyCore.Dapr/
│   ├── Actors/                 # EasyCoreActor / ActorOptions
│   ├── Bindings/ Secrets/      # 绑定与密钥
│   ├── Client/                 # DaprSidecarClient
│   ├── Configuration/          # DaprOptions + Validator
│   ├── DependencyInjection/    # Add* / Use* / Map*
│   ├── Health/ Invocation/     # 健康等待 / 服务调用
│   ├── PubSub/ State/          # 发布订阅 / 状态
│   └── Workflow/               # Workflow 封装 + Client
├── demo/
│   ├── Web.Dapr.Service/       # 被调用方
│   ├── Web.Dapr/               # Invoke / State / PubSub
│   ├── Web.Dapr.Actors/        # CounterActor
│   └── Web.Dapr.Workflow/      # HelloWorkflow
├── components/                 # Redis state / pubsub（含 actorStateStore）
├── tests/EasyCore.Dapr.Tests/
├── README.md
└── README.en.md
```

---

## 4. 📦 安装

```bash
dotnet add package EasyCore.Dapr
```

需要 **.NET 8**，以及本机或集群中的 Dapr sidecar（默认 `http://127.0.0.1:3500`）。

本地推荐安装 [Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/) 并执行 `dapr init`（依赖 Docker Desktop）。

---

## 5. ⚡ 三分钟快速开始

### 5.1 代码注册

```csharp
using EasyCore.Dapr;

var builder = WebApplication.CreateBuilder(args);

builder.AddEasyCoreDapr()
    .AddEasyCoreDaprAll(); // 或按需 AddEasyCoreDaprInvocation / State / PubSub ...

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

### 5.2 配置节绑定

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

端点解析顺序：`Dapr:HttpEndpoint` → `DAPR_HTTP_ENDPOINT` → `DAPR_HTTP_PORT` → `http://127.0.0.1:3500/`。  
API Token：`Dapr:ApiToken` → `DAPR_API_TOKEN`。

---

## 6. ⚙️ 配置项

| 键 | 说明 | 默认 | 图标语义 |
|---|---|---|---|
| `HttpEndpoint` | Sidecar HTTP 基址 | 空（见解析顺序） | 🔌 |
| `AppId` | 本进程 Dapr app id | `null` | 🏷️ |
| `ApiToken` | `dapr-api-token` | 空（可走环境变量） | 🔑 |
| `DefaultStateStore` | 默认状态存储组件名 | `statestore` | 🗄️ |
| `DefaultPubSub` | 默认 Pub/Sub 组件名 | `pubsub` | 📣 |
| `DefaultSecretStore` | 默认密钥存储组件名 | `secretstore` | 🤫 |
| `Timeout` | Sidecar HTTP 超时 | `00:00:30` | ⏱️ |
| `WaitForSidecar` | 启动前等待 `/v1.0/healthz` | `true` | ⏳ |
| `SidecarWaitTimeout` | 等待上限 | `00:00:30` | ⌛ |
| `SidecarPollInterval` | 轮询间隔 | `00:00:00.500` | 🔁 |
| `FailFastIfSidecarUnavailable` | 等待失败则中止启动 | `true` | 🛑 |
| `AppHealthPath` | 应用健康路径 | `/healthz` | ❤️ |

> Demo 本地无 sidecar 时可将 `WaitForSidecar=false`，先验证编译与健康端点；Actor / Workflow 调用仍需 sidecar。

---

## 7. 🧩 扩展方法

| 方法 | 作用 | 图标语义 |
|---|---|---|
| `AddEasyCoreDapr` / `EasyCoreDapr` | Options、Sidecar 客户端、健康等待 | 🧱 |
| `AddEasyCoreDaprInvocation` | `IDaprInvoker` | 📞 |
| `AddEasyCoreDaprState` | `IDaprStateStore`（含 ETag） | 🗄️ |
| `AddEasyCoreDaprPubSub` | `IDaprPubSub` | 📣 |
| `AddEasyCoreDaprSecrets` | `IDaprSecrets` | 🤫 |
| `AddEasyCoreDaprBindings` | `IDaprBinding` | 🔗 |
| `AddEasyCoreDaprAll` | 以上 HTTP 能力一次注册 | 📦 |
| `AddEasyCoreDaprActors` | Actors 运行时 + `RegisterActor<T>()` | 🎭 |
| `AddEasyCoreDaprWorkflow` | Workflow 宿主 + `IEasyCoreWorkflowClient` | 🔄 |
| `UseEasyCoreDapr` | `/healthz` + 可选 `/dapr/subscribe` | 🛠️ |
| `MapEasyCoreDaprActors` | Actor HTTP handlers | 🗺️ |

---

## 8. 📖 能力速查

### 8.1 📞 服务调用

```csharp
var dto = await invoker.InvokeMethodAsync<HelloDto>(HttpMethod.Get, "easycore-service", "api/hello");
```

Mvc 远端代理可复用：

```csharp
services.AddHttpClient("x")
    .AddHttpMessageHandler(() => new DaprInvokeHandler("provider"));
```

### 8.2 🗄️ 状态

```csharp
await state.SaveStateAsync("user:1", new { Name = "a" });
var (value, etag) = await state.GetStateAndETagAsync<MyType>("user:1");
```

### 8.3 📣 发布订阅

```csharp
await pubsub.PublishEventAsync("orders", new { Id = 1 });
// UseEasyCoreDapr(e => e.Subscribe("orders", "/events/orders", handler));
```

### 8.4 🎭 Actor 原子操作与单线程并发

**同一 Actor 实例单线程执行**：并发调用排队，一次方法调用（turn）完整结束后才处理下一次——这是方法级原子串行，用于限制并发，**不是** `EasyCore.UnitOfWork`，也不是 State Transaction API。

运行时仍用官方 `Dapr.Actors`；`EasyCoreActor.State`（`IEasyCoreActorState`）只是对 `StateManager` 的薄封装。回合结束由运行时落盘；中途强制落盘用 `State.SaveAsync()`（少用）。

```csharp
builder.Services.AddEasyCoreDaprActors(a => a.RegisterActor<CounterActor>());
app.MapEasyCoreDaprActors();

public class CounterActor : EasyCoreActor, ICounterActor
{
    public async Task<int> IncrementAsync(int by)
    {
        var n = await State.GetOrAddAsync("n", () => 0);
        n += by;
        await State.SetAsync("n", n);
        return n; // 本 turn 独占执行；返回后 runtime 落盘
    }
}
```

> Actor 接口方法**不能**有 `out` / `ref` / 可选参数；自定义返回类型需可序列化（默认 DataContract，或配置 JSON）。

### 8.5 🔄 Dapr Workflow（持久化编排）

[Dapr Workflow](https://docs.dapr.io/zh-hans/developing-applications/building-blocks/workflow/workflow-overview/) 用于可靠、可恢复的长时业务编排。

**与 Actor 的区别**：Actor = 单实例串行原子操作（限制并发）；Workflow = 跨活动、可重放的持久化编排。二者互补。

```csharp
builder.Services.AddEasyCoreDaprWorkflow(wf =>
{
    wf.RegisterWorkflow<HelloWorkflow>();
    wf.RegisterActivity<GreetActivity>();
    wf.RegisterActivity<EmphasizeActivity>();
});

await workflows.ScheduleAsync(nameof(HelloWorkflow), new HelloInput("EasyCore"), instanceId);
var state = await workflows.GetStateAsync(instanceId);
// Suspend / Resume / RaiseEvent / Terminate / Purge 同理
```

工作流宿主通过 gRPC 连接 sidecar（注意 `DAPR_GRPC_PORT`）；需使用[支持工作流的状态存储](https://docs.dapr.io/zh-hans/developing-applications/building-blocks/workflow/workflow-overview/#限制)（本仓库 `components/statestore.yaml` 已设 `actorStateStore: true`）。

---

## 9. 🧪 Demo

| 项目 | 端口 | 说明 |
|---|---|---|
| `demo/Web.Dapr.Service` | 5288 | 被调用方 `/api/hello` |
| `demo/Web.Dapr` | 5287 | Invoke / State / Publish / Subscribe |
| `demo/Web.Dapr.Actors` | 5289 | CounterActor + ActorProxy |
| `demo/Web.Dapr.Workflow` | 5290 | HelloWorkflow + Activity |

### 9.1 🐳 前置：Docker + Dapr CLI

1. 打开 **Docker Desktop**  
2. 安装并初始化 Dapr：`dapr init`（会起 Redis / Placement / Scheduler 等）  
3. 确认 Redis `6379` 可用（`dapr_redis` 或自建）

### 9.2 🚀 推荐启动

```bash
dapr run --app-id easycore-service --app-port 5288 --dapr-http-port 3501 -- dotnet run --project demo/Web.Dapr.Service
dapr run --app-id easycore-web --app-port 5287 --dapr-http-port 3500 --resources-path ./components -- dotnet run --project demo/Web.Dapr
dapr run --app-id easycore-actors --app-port 5289 --dapr-http-port 3502 --resources-path ./components -- dotnet run --project demo/Web.Dapr.Actors
dapr run --app-id easycore-workflow --app-port 5290 --dapr-http-port 3503 --dapr-grpc-port 50001 --resources-path ./components -- dotnet run --project demo/Web.Dapr.Workflow
```

> 多个 Demo 并行时请使用**不同的** `--dapr-http-port` / `--dapr-grpc-port`。

### 9.3 👀 验证示例

**HTTP 构建块**

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

本地无 sidecar 时 Demo 默认 `WaitForSidecar=false`，可先启动 ASP.NET 验证编译与健康端点。

---

## 10. ✅ 生产清单

- [ ] 生产开启 `WaitForSidecar=true` 与合理 `SidecarWaitTimeout`
- [ ] 配置 `DAPR_API_TOKEN` / `ApiToken`
- [ ] 按环境区分 `DefaultStateStore` / `DefaultPubSub` 组件名
- [ ] 状态写冲突使用 ETag（`GetStateAndETagAsync` + `If-Match`）
- [ ] Pub/Sub 路由幂等；失败返回非 2xx 以便 sidecar 重投
- [ ] Actor：配置 actor 状态存储；依赖官方单线程 turn；提醒器/空闲超时按业务配置
- [ ] Actor：优先依赖回合结束自动落盘；仅在需要时中途 `SaveAsync`
- [ ] Workflow：使用支持工作流的状态存储；编排保持确定性；版本变更谨慎
- [ ] Workflow：配置 `DAPR_GRPC_PORT`；长时实例用 Suspend/Resume/RaiseEvent 管理
- [ ] 勿把 sidecar 端口暴露到公网

---

## 11. ❓ FAQ

**Q: 为什么 Invoke 报 sidecar 不可用？**  
A: 确认已用 `dapr run` 启动，且 `Dapr:HttpEndpoint` / `DAPR_HTTP_PORT` 指向正确端口。

**Q: Actor 启动报 optional parameter？**  
A: Actor 接口方法不能带默认可选参数；改为必填参数，在调用方补默认值。

**Q: Actor 返回类型序列化失败？**  
A: 默认走 DataContract：给类型加 `[DataContract]` / `[DataMember]` 与无参构造，或配置 Actors JSON 序列化。

**Q: Workflow 一直不 Completed？**  
A: 检查状态存储是否带 `actorStateStore: true`，以及 `DAPR_GRPC_PORT` 是否与 `dapr run` 一致。

**Q: 和 EasyCore.AspNetCore.Mvc `[DaprApp]` 什么关系？**  
A: Mvc 管远端接口代理；本包装 sidecar 构建块、Actor 与 Workflow 宿主，二者互补。

---

## 12. 📄 License

MIT OR Apache-2.0

### 🤝 贡献

1. Fork 并创建特性分支  
2. 在 `tests/EasyCore.Dapr.Tests` 补充测试  
3. 执行 `dotnet test` 与 `dotnet build`  
4. 提交 Pull Request  

欢迎 Issue / PR 🚀
