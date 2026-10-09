---
uid: TimeWarpState:Migration13.0.0-beta.1.md
title: Migrate to 13.0.0-beta.1
---

# Migration to 13.0.0-beta.1

`TimeWarp.State` no longer references Blazor. Components, JavaScript interop, render subscriptions, and Redux DevTools moved to `TimeWarp.State.Blazor`. Type names and namespaces are unchanged.

## Packages

```console
dotnet add package TimeWarp.State
dotnet add package TimeWarp.State.Blazor
```

`TimeWarp.State.Plus` keeps its package id and now depends on `TimeWarp.State.Blazor`. A host that already references Plus still needs the registration call below.

Console and other non-UI hosts reference `TimeWarp.State` only.

## Registration

Blazor hosts add one call:

```csharp
builder.Services.AddTimeWarpState(options =>
{
  options.UseReduxDevTools();
});
builder.Services.AddTimeWarpStateBlazor();
```

`AddTimeWarpStateBlazor` registers `RenderSubscriptionContext`, `JsonRequestHandler`, the JavaScript dispatch registry, and the server-side `HttpClient` that used to be registered by `AddTimeWarpState`. `UseReduxDevTools` and `AddJavaScriptDispatch` are extension methods on the Blazor package, in the `TimeWarp.State` namespace.

`AddTimeWarpState` still registers the store, subscriptions, options, and `State<T>` types.

## Pipeline

`StateInitializationPreProcessor` (order 200) and `StateTransactionBehavior` (order 300) stay in `TimeWarp.State`.

`ReduxDevToolsBehavior` (order 100) and `RenderSubscriptionsPostProcessor` (order 400) are woven when the host references `TimeWarp.State.Blazor`. A console host that does not reference that package does not run them.

## Static web assets

Script URLs stay `/_content/TimeWarp.State/js/timewarp-state.js`. The Blazor package sets `StaticWebAssetBasePath` to `TimeWarp.State`.

## What stays in TimeWarp.State

Store, `State<T>`, actions, handlers, cloning, the action catalog, persistence attributes, `[SuppressRender]`, and `ITimeWarpStateComponent`. `[SuppressRender]` remains on the core package so action types can opt out of re-render without a direct Blazor reference. The processor that reads it is in `TimeWarp.State.Blazor`.
