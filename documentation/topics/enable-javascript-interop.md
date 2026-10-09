---
uid: TimeWarp.State:EnableJavascriptInterop.md
title: Enable JavaScript Interop
---

# Enable JavaScript interop

JavaScript can dispatch actions into the client pipeline with `timeWarpState.DispatchRequest(name, request)`.
Dispatch is **opt-in**: since 12.0.0-beta.8 only actions you allow are dispatchable. Everything else is
rejected.

## Allow actions

```csharp
builder.Services.AddTimeWarpState();
builder.Services.AddTimeWarpStateBlazor();
builder.Services.AddJavaScriptDispatch
(
  b => b
    .Allow<CounterState.IncrementCountActionSet.Action>()
    .Allow<CounterState.ResetActionSet.Action>("Counter.Reset") // optional short alias
);
```

`Allow<TAction>()` requires `TAction : class, IAction`, so only actions can be allowed. `Allow(Type)` checks the
same rule at startup and throws for non-action types. Call `AddJavaScriptDispatch` as often as you like and in
any order relative to `AddTimeWarpState` and `AddTimeWarpStateBlazor`. `AddJavaScriptDispatch` and `AddTimeWarpStateBlazor` are in the `TimeWarp.State.Blazor` package.

Render `<TimeWarpJavaScriptInterop />` once (for example in your layout) to register the handler with JavaScript.

## Dispatch from JavaScript

```js
import { timeWarpState } from '/_content/TimeWarp.State/js/timewarp-state.js'

// Any of these names resolve to an allowed type:
await timeWarpState.DispatchRequest("MyApp.Features.Counter.CounterState+IncrementCountActionSet+Action", { amount: 7 });
await timeWarpState.DispatchRequest("MyApp.Features.Counter.CounterState+IncrementCountActionSet+Action, MyApp", { amount: 7 });
await timeWarpState.DispatchRequest("Counter.Reset", {});
```

The wire name may be the type's full name, `"FullName, AssemblyName"`, its assembly-qualified name (version,
culture and token are ignored), or the alias. Names are looked up in the allow-list only; they are never passed
to `Type.GetType`.

## Rejections

`JsonRequestHandler.Handle` fails closed. Each case logs a warning and throws `InvalidRequestTypeException`:

- the name is not allowed (including any non-action type);
- the JSON cannot be deserialized into the type, or deserializes to `null`;
- the JSON is empty and the type has no public parameterless constructor.

## Redux DevTools

Redux DevTools messages (Start, Commit) reach .NET through the same handler. They are not actions, so
`UseReduxDevTools()` allow-lists them itself. Without `UseReduxDevTools()` they are rejected. Call
`UseReduxDevTools()` only in Development if you do not want DevTools in production.

## Why explicit registration

- The generic constraint proves "is an action" at compile time, with no reflection scan and no analyzer.
- `[CatalogAction]` is a user-facing palette and agent surface; reusing it would expose every cataloged action to
  page script and require a `Description` on purely technical interop actions.
- An attribute such as `[JsDispatchable]` would need a second generated registry for the same result.
