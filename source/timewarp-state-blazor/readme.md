# TimeWarp.State.Blazor

Blazor components, JavaScript interop, render subscriptions, and Redux DevTools for [TimeWarp.State](https://www.nuget.org/packages/TimeWarp.State/).

`TimeWarp.State` has no Blazor dependency. Reference this package from Blazor WebAssembly and Blazor Server hosts, then register both entry points:

```csharp
builder.Services.AddTimeWarpState(options => options.UseReduxDevTools());
builder.Services.AddTimeWarpStateBlazor();
```

`TimeWarp.State.Plus` (routing, persistence, action tracking) depends on this package. The Plus package id is unchanged.

Static web assets stay at `/_content/TimeWarp.State/`, including `js/timewarp-state.js`. Blazor loads the initializer `js/TimeWarp.State.Blazor.lib.module.js` from that same base path.

See [Migrate to 12.0.0-beta.10](../../documentation/migrations/migration12.0.0-beta.10.md).
