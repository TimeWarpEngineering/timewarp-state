---
uid: TimeWarpState:Release.12.0.0-beta.8.md
title: Release 12.0.0-beta.8
---

## Release 12.0.0-beta.8

### Breaking changes

- **JavaScript dispatch now requires opt-in.** `JsonRequestHandler.Handle` (reached from
  `timeWarpState.DispatchRequest(name, request)`) no longer resolves arbitrary type names with `Type.GetType`.
  Only actions allowed with `services.AddJavaScriptDispatch(b => b.Allow<TAction>())` are dispatched. Unknown
  names, non-action types, invalid JSON and empty payloads for types without a parameterless constructor log a
  warning and throw `InvalidRequestTypeException`.

  Migration: for each action your JavaScript dispatches, add `.Allow<TheAction>()`. The existing call shape
  `DispatchRequest("Ns.State+XActionSet+Action, Assembly, Version=…", …)` keeps working; the full name,
  `"FullName, AssemblyName"` or an optional alias also resolve.
- `JsonRequestHandler`'s constructor takes a `JavaScriptDispatchRegistry`. Only relevant if you construct it
  yourself; DI resolves it.

### Features

- `AddJavaScriptDispatch` / `JavaScriptDispatchBuilder.Allow<TAction>(alias)` / `JavaScriptDispatchRegistry`.
  See [Enable JavaScript interop](../topics/enable-javascript-interop.md).
- Redux DevTools Start and Commit messages are allow-listed by `UseReduxDevTools()` and rejected when DevTools
  is not enabled.
