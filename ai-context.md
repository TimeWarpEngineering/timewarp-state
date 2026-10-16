# TimeWarp.State package structure

`TimeWarp.State` is the core store. It does not reference `Microsoft.AspNetCore.Components` or `Microsoft.JSInterop`.

| Package | Project | Contains |
| --- | --- | --- |
| `TimeWarp.State` | `source/timewarp-state` | Store, `State<T>`, actions, handlers, cloning, action catalog, persistence attributes, `[SuppressRender]`, `ITimeWarpStateComponent`, `Subscriptions`, state-initialization (order 200) and state-transaction (order 300) behaviors. Packs the analyzer and source generator. `IsAotCompatible`. |
| `TimeWarp.State.Blazor` | `source/timewarp-state-blazor` | `TimeWarpStateComponent`, `TimeWarpStateInputComponent`, `RenderModeDisplay`, `TimeWarpJavaScriptInterop`, JavaScript dispatch, render subscriptions (order 400), `wwwroot` scripts. Static web assets stay at `_content/TimeWarp.State/`. `IsAotCompatible`. Does not register `HttpClient`. |
| `TimeWarp.State.Plus` | `source/timewarp-state-plus` | Routing, persistence, action tracking. Package id is not renamed. Depends on `TimeWarp.State` and `TimeWarp.State.Blazor`. `IsAotCompatible`. `AddTimeWarpStatePersistence()` registers `IPersistenceService`. |
| `TimeWarp.State.Telemetry` | `source/timewarp-state-telemetry` | OpenTelemetry action spans. Depends on `TimeWarp.State` only. Behavior order 350. `IsAotCompatible`. |
| `TimeWarp.State.DevTools.Export` | `source/timewarp-state-devtools-export` | Dev-only exporter. Copies `TimeWarp.State` activities to the inspector. No `IState` types. Development route `GET /_timewarp/state-devtools`. |
| `TimeWarp.State.DevTools.Wasm` | `source/timewarp-state-devtools-wasm` | Browser exporter. `ActivityListener` plus `SendAsync`. No OpenTelemetry reference. Stamps `timewarp.state.runtime=wasm`. |
| `TimeWarp.State.DevTools.Hosting` | `source/timewarp-state-devtools-hosting` | Aspire `AddTimeWarpStateDevTools()` / `WithTimeWarpStateDevTools()`. Runs the App package. |
| `TimeWarp.State.DevTools.App` | `source/timewarp-state-devtools-app` | Published inspector (`tools/TimeWarp.State.DevTools.dll`). Framework-dependent, no RID. Same version as Hosting. |
| `TimeWarp.State.DevTools` | `source/timewarp-state-devtools` | Blazor Server inspector project. Unpackable. Own store. Publish output is the App package. Not referenced by production packages. |

Blazor hosts:

```csharp
builder.Services.AddTimeWarpState();
builder.Services.AddTimeWarpStateBlazor();
builder.Services.AddTimeWarpStatePersistence();
```

`AddTimeWarpStateBlazor` does not register `HttpClient`. A server host that injects one registers it. Persistence storage (`AddBlazoredSessionStorage` / `AddBlazoredLocalStorage`) stays on the host. Each storage service is optional on `PersistenceService`.

Console hosts call `AddTimeWarpState` and do not reference `TimeWarp.State.Blazor`. See `samples/07-console`.

Migration: `documentation/migrations/migration12.0.0-beta.11.md`.
