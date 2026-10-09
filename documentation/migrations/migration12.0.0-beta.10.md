---
uid: TimeWarpState:Migration12.0.0-beta.10.md
title: Migrate to 12.0.0-beta.10
---

# Migration to 12.0.0-beta.10

## TimeWarp.State.Blazor

`TimeWarp.State` no longer references Blazor. Components, JavaScript interop, render subscriptions, Redux DevTools, and wwwroot moved to `TimeWarp.State.Blazor`. Type names and namespaces are unchanged.

### Packages

```console
dotnet add package TimeWarp.State
dotnet add package TimeWarp.State.Blazor
```

`TimeWarp.State.Plus` keeps its package id and now depends on `TimeWarp.State.Blazor`. A host that already references Plus still needs the registration call below.

Console and other non-UI hosts reference `TimeWarp.State` only.

### Registration

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

### Pipeline

`StateInitializationPreProcessor` (order 200) and `StateTransactionBehavior` (order 300) stay in `TimeWarp.State`.

`ReduxDevToolsBehavior` (order 100) and `RenderSubscriptionsPostProcessor` (order 400) are woven when the host references `TimeWarp.State.Blazor`. A console host that does not reference that package does not run them.

### Static web assets

Script URLs stay `/_content/TimeWarp.State/js/timewarp-state.js`. `StaticWebAssetBasePath` is `_content/TimeWarp.State` (the `_content/` prefix is part of the property). Blazor loads the initializer `TimeWarp.State.Blazor.lib.module.js` from that base path. Hosts do not add a script tag for it.

### What stays in TimeWarp.State

Store, `State<T>`, actions, handlers, cloning, the action catalog, persistence attributes, `[SuppressRender]`, and `ITimeWarpStateComponent`. `[SuppressRender]` remains on the core package so action types can opt out of re-render without a direct Blazor reference. The processor that reads it is in `TimeWarp.State.Blazor`.

## TimeWarpCacheableState

`TimeWarpCacheableState<TState>` now derives from `State<TState>` and requires
`where TState : TimeWarpCacheableState<TState>`.

A derived state written as `class X : TimeWarpCacheableState<X>` still compiles. It implements `IState<X>`, and
`Hydrate` returns `X`.

A type argument that is not a `TimeWarpCacheableState` no longer compiles. Change it so the argument is the derived
state itself. Passing a different cacheable state still compiles, and `StateInheritanceAnalyzer` checks only classes
that derive directly from `State<T>`, so check those declarations by hand and pass the derived state itself. Change an
override of `Hydrate` that returned `TimeWarpCacheableState<X>` so it returns `X`.

## FeatureFlagState

`FeatureFlagState` is no longer in `TimeWarp.State.Plus`. Delete references to it. There is no
`UseFeatureFlags` registration method. The 10-to-11 migration guide no longer tells you to call one.

## InvalidCloneException

The constructor is `InvalidCloneException(Type enclosingStateType, InvalidCloneException.Cause cause)`.

Pass `Cause.EmptyGuid` when the clone's `Guid` is empty, and `Cause.EqualGuid` when it matches the original.
`StateTransactionBehavior` chooses the cause. A custom `ICloneable.Clone` must construct the clone so `Guid` is
new. Mark `Guid` with `[IgnoreDataMember]` when the default cloner should leave it alone. That cloner also skips
`[NonSerialized]` and `[JsonIgnore]`.

## Also in this version

NuGet pins moved to the current latest, including TimeWarp.Amuru and TimeWarp.Amuru.Tools 2.0.0-beta.2. Microsoft.CodeAnalysis.CSharp stays 4.14.0 so the packed analyzer still loads on the consumer compiler floor.

Every tracked C# file carries a `#region Purpose` block and a `#region Design` block. TWA0004 (Purpose region required) is enabled through TimeWarp.Architecture.Analyzers. Other TWA rules stay off. `.githooks` is excluded because those files are owned by ganda.
