---
uid: TimeWarpState:DevTools.md
title: TimeWarp.State DevTools
---

# Companion DevTools app

`TimeWarp.State.DevTools` is a Blazor Server inspector for a running app's actions and state. An Aspire AppHost adds it with `AddTimeWarpStateDevTools()`. The dashboard endpoint is labeled **TimeWarp.State DevTools**.

The inspector uses TimeWarp.State for its own filter, selection, and pause. That store is a different process from the app. The action list is the app's `TimeWarp.State` activities, not the inspector's.

## What it observes

`TimeWarp.State.Telemetry` emits spans on ActivitySource `TimeWarp.State`. `AddTimeWarpStateDevToolsExporter()` copies those spans to `POST /ingest/spans` as a small JSON document (name, tags, snapshot or diff events, status, duration). It is not an OTLP collector. Aspire's OTLP endpoint is unchanged, so the dashboard still receives traces.

`WithTimeWarpStateDevTools` sets `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` on the inspected resource. When that variable is unset or blank, the exporter does nothing. A value that is not an absolute URI throws `InvalidOperationException`.

There is no time-travel, commit, or import/export channel. The browser-extension surface was removed in 12.0.0-beta.11.

## Prerender and interactive stores

A Blazor Server page with prerendering builds two stores for one page load. The static render creates a scoped store, initializes each state, and discards that store with the request. The interactive circuit then creates a second store and initializes the same states again. Both initializations run through the pipeline, so the log shows both `init` rows. Neither row is skipped or deduped.

Every span carries `timewarp.state.store`, the `Guid` created with that `Store`. When the host registered `IStateRenderKind` (Blazor does, through `AddTimeWarpStateBlazor`), the span also carries `timewarp.state.render` as `prerender` or `interactive`. Components record that from `RendererInfo.IsInteractive` before `GetState` initializes the state. A host that cannot tell leaves the render tag off; the store id is enough.

Each row shows a short badge. **pre** and **live** come from the render tag. Without that tag the badge is the first four characters of the store id. The badge colour is a stable hash of the full id into eight colours, so two stores that read the same word are usually distinct; about one pair in eight shares a colour. Hover the badge, or open the **Action** tab, for the full id. No rows are hidden by default.

## State panel

The right-hand pane has **Action**, **State**, and **Diff** tabs.

**Action** lists the selected span: activity name, state type, status, duration, start time, span id, store id, render kind when the span carried one, and the other tags.

**State** lists every state type in one store. Each node is a collapsible JSON tree. **Tree** and **Raw** switch the format. **Current state** is the default store's fold: the latest snapshot of each state type in the newest store that is not a prerender, or the newest store when every store is a prerender. Enhanced navigation can prerender a page into a new store while the existing circuit keeps running, so a prerender store never replaces the live store on screen. A later span from an older store updates that store only. A store selector above the tree starts with **Follow live**, which tracks the default store, then lists stores newest first. Spans with no store tag appear as **(untagged)**. Choosing a store pins it, and a newer store does not move the pin; the prerender fold stays available. Selecting a row pins that row's store. Choose **Follow live**, or **Clear** the log, to unpin. With a row selected in the store on screen, the heading is **State after action**: that store's fold up to and including the row. Expanded and collapsed paths stay put as new rows arrive.

**Diff** compares the selected snapshot with the previous snapshot of the same state type in the same store. Added, removed, and changed values are colour coded. Equal JSON says **No change.** A span with no snapshot says so. A truncated payload is shown as raw text with a hint to raise `MaxSnapshotChars`; it is not parsed as a tree.

The filter box matches action names, state type names, and the start of a store id, store badge (`pre`, `live`, or the id prefix), or runtime badge (`wasm`, `srv`) in the log. In the State panel, a filter that matches the store on screen shows that store's whole fold; any other filter matches state type names, and says **No state matches the filter** when none do.

A row whose `timewarp.state.kind` tag is `init` is a state-initialization snapshot. It is tinted like Redux DevTools' `init` row and folds like any other snapshot. With `IncludeSnapshots` on, telemetry emits that row when the state is created, before its first action.

The header theme control uses the TimeWarp palette. Dark is the default. Light follows `prefers-color-scheme` or an explicit choice on the control.

## AppHost

```csharp
IResourceBuilder<ExecutableResource> devtools = builder.AddTimeWarpStateDevTools();
builder.AddProject<Projects.MyApp>("my-app")
  .WithTimeWarpStateDevTools(devtools);
```

`TimeWarp.State.DevTools.Hosting` depends on `TimeWarp.State.DevTools.App` at the same version. That package contains the published inspector (`tools/TimeWarp.State.DevTools.dll`). The publish is framework-dependent and has no runtime identifier. The AppHost starts it with the .NET host (`DOTNET_HOST_PATH`, the running `dotnet` muxer, `DOTNET_ROOT/dotnet`, then `dotnet`). Package targets stamp the dll path onto the AppHost. `AddTimeWarpStateDevTools` adds that executable and declares an HTTP endpoint named `http`. The resource is left out of the publish manifest, and `WithTimeWarpStateDevTools` does nothing in publish mode, so publish output does not reference the inspector.

`AddTimeWarpStateDevToolsProject(projectPath)` is the explicit project switch for inner-loop work on the inspector. The sample AppHost calls `AddTimeWarpStateDevTools()`.

In the inspected app:

```csharp
builder.Services.AddOpenTelemetry()
  .WithTracing(tracing =>
  {
    tracing.AddSource(TimeWarpStateTelemetry.ActivitySourceName);
    tracing.AddOtlpExporter();
    tracing.AddTimeWarpStateDevToolsExporter();
  });
```

Turn on snapshots only with caller `JsonTypeInfo`. The [telemetry sample](../../samples/04-telemetry/overview.md) does that when the DevTools endpoint is set.

## WebAssembly

Stores that run in the browser are a different process from the server. `TimeWarp.State.DevTools.Wasm` posts the same JSON document with `HttpClient.SendAsync` and stamps `timewarp.state.runtime=wasm`. The server exporter stamps `server` when that tag is absent. The log shows **wasm** or **srv** beside the store badge. The store id still separates instances.

The browser learns the origin from configuration `TimeWarp:State:DevTools:Endpoint`, then from `GET /_timewarp/state-devtools` on the host (`MapTimeWarpStateDevToolsClientConfig`, Development only). The call is a no-op outside Development and when no origin is available.

`WithTimeWarpStateDevTools` publishes the inspected app's http and https origins as `TIMEWARP_STATE_DEVTOOLS_CORS_ORIGINS` on the inspector. CORS is registered only in Development and only when that list has an absolute http(s) origin. The inspector is not an open cross-origin sink.

Reference `TimeWarp.State.DevTools.Wasm` from Debug builds only, and wrap the registration in `#if DEBUG`, so a published bundle does not contain it. The telemetry sample keeps the reference so the wiring stays on the page `/counter-wasm`.

## Separate store

The inspector's states are compiled into the DevTools assembly. The inspected app references `TimeWarp.State.DevTools.Export` only. That package has no `IState` types, so the app's `StateCloneRegistry` and `StateTransactionBehavior` never see `InspectorState`.

Spans whose `timewarp.state.state_type` is `InspectorState`, or whose `timewarp.state.inspector` tag is `true`, are dropped on export and again on ingest.

## Packages

| Package | Role |
| --- | --- |
| `TimeWarp.State.DevTools.Export` | Dev-only exporter. Safe for the inspected app to reference. Maps the Development discovery route. |
| `TimeWarp.State.DevTools.Wasm` | Browser exporter. `ActivityListener` and `SendAsync`. No OpenTelemetry package. |
| `TimeWarp.State.DevTools.Hosting` | `AddTimeWarpStateDevTools()` and `WithTimeWarpStateDevTools()`. Depends on the App package. |
| `TimeWarp.State.DevTools.App` | Published inspector. Framework-dependent. Same version as Hosting. |
| `TimeWarp.State.DevTools` | The Blazor app project. Not itself a NuGet package. Its publish output is the App package. |

Production packages do not reference these.

## Replaces the browser extension

Use this app plus [telemetry](xref:TimeWarpState:Telemetry.md) for the action log, the state tree, and the diff. `UseReduxDevTools()` and `<ReduxDevTools />` are removed in 12.0.0-beta.11.
