# TimeWarp.State.Blazor

Blazor components, JavaScript interop, and render subscriptions for [TimeWarp.State](https://www.nuget.org/packages/TimeWarp.State/).

`TimeWarp.State` has no Blazor dependency. Reference this package from Blazor WebAssembly and Blazor Server hosts, then register both entry points:

```csharp
builder.Services.AddTimeWarpState();
builder.Services.AddTimeWarpStateBlazor();
```

The action log, state tree, and diff are the companion DevTools app, fed by [TimeWarp.State.Telemetry](../timewarp-state-telemetry/readme.md). `UseReduxDevTools()` and `<ReduxDevTools />` were removed in 12.0.0-beta.11.

`TimeWarp.State.Plus` (routing, persistence, action tracking) depends on this package. The Plus package id is unchanged.

Static web assets stay at `/_content/TimeWarp.State/`, including `js/timewarp-state.js`. Blazor loads the initializer `js/TimeWarp.State.Blazor.lib.module.js` from that same base path.

See [Migrate to 12.0.0-beta.11](../../documentation/migrations/migration12.0.0-beta.11.md).
