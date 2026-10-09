# TimeWarp.State package structure

`TimeWarp.State` is the core store. It does not reference `Microsoft.AspNetCore.Components` or `Microsoft.JSInterop`.

| Package | Project | Contains |
| --- | --- | --- |
| `TimeWarp.State` | `source/timewarp-state` | Store, `State<T>`, actions, handlers, cloning, action catalog, persistence attributes, `[SuppressRender]`, `ITimeWarpStateComponent`, `Subscriptions`, state-initialization (order 200) and state-transaction (order 300) behaviors. Packs the analyzer and source generator. |
| `TimeWarp.State.Blazor` | `source/timewarp-state-blazor` | `TimeWarpStateComponent`, `TimeWarpStateInputComponent`, `TimeWarpStateDevComponent`, `RenderModeDisplay`, `ReduxDevTools`, `TimeWarpJavaScriptInterop`, JavaScript dispatch, Redux DevTools interop and behavior (order 100), render subscriptions (order 400), `wwwroot` scripts. Static web assets stay at `_content/TimeWarp.State/`. |
| `TimeWarp.State.Plus` | `source/timewarp-state-plus` | Routing, persistence, action tracking. Package id is not renamed. Depends on `TimeWarp.State` and `TimeWarp.State.Blazor`. |
| `TimeWarp.State.Telemetry` | `source/timewarp-state-telemetry` | OpenTelemetry action spans. Depends on `TimeWarp.State` only. Behavior order 350. |

Blazor hosts:

```csharp
builder.Services.AddTimeWarpState();
builder.Services.AddTimeWarpStateBlazor();
```

Console hosts call `AddTimeWarpState` and do not reference `TimeWarp.State.Blazor`. See `samples/07-console`.

Migration: `documentation/migrations/migration12.0.0-beta.10.md`.
