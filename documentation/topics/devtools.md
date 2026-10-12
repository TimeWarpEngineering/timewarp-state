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

Each row shows a short badge. **pre** and **live** come from the render tag. Without that tag the badge is the first four characters of the store id. The badge colour is a stable hash of the full id, so the prerender store and the circuit store stay visually distinct even when both badges read the same word. Hover the badge, or open the **Action** tab, for the full id. No rows are hidden by default.

## State panel

The right-hand pane has **Action**, **State**, and **Diff** tabs.

**Action** lists the selected span: activity name, state type, status, duration, start time, span id, store id, render kind when the span carried one, and the other tags.

**State** lists every state type in one store. Each node is a collapsible JSON tree. **Tree** and **Raw** switch the format. **Current state** is the newest store's fold: the latest snapshot of each state type in the store that first appeared latest in the log. A later span from an older store updates that store and leaves the newer store on screen. A store selector above the tree lists stores newest first. Choosing one pins that store, so a prerender snapshot does not replace the live store, and the prerender fold stays available. Selecting a row pins that row's store. With a row selected in the store on screen, the heading is **State after action**: that store's fold up to and including the row. Expanded and collapsed paths stay put as new rows arrive.

**Diff** compares the selected snapshot with the previous snapshot of the same state type in the same store. Added, removed, and changed values are colour coded. Equal JSON says **No change.** A span with no snapshot says so. A truncated payload is shown as raw text with a hint to raise `MaxSnapshotChars`; it is not parsed as a tree.

The filter box matches action names, state type names, store ids, and store badges (`pre`, `live`, or the id prefix) in the log. In the State panel, a filter that matches the store on screen shows that store's whole fold; any other filter matches state type names.

A row whose `timewarp.state.kind` tag is `init` is a state-initialization snapshot. It is tinted like Redux DevTools' `init` row and folds like any other snapshot. With `IncludeSnapshots` on, telemetry emits that row when the state is created, before its first action.

The header theme control uses the TimeWarp palette. Dark is the default. Light follows `prefers-color-scheme` or an explicit choice on the control.

## AppHost

```csharp
IResourceBuilder<ProjectResource> devtools = builder.AddTimeWarpStateDevTools();
builder.AddProject<Projects.MyApp>("my-app")
  .WithTimeWarpStateDevTools(devtools);
```

`AddTimeWarpStateDevTools` finds `source/timewarp-state-devtools/timewarp-state-devtools.csproj` by walking up from the AppHost directory. Set `TIMEWARP_STATE_DEVTOOLS_PROJECT` when the AppHost is outside this repository. It declares an HTTP endpoint named `http`, so the resource has an endpoint even when Aspire does not find `Properties/launchSettings.json`. An external checkout still needs that `Properties/` folder (capital P) for the Development launch profile.

The DevTools app is not packed into this package. Outside this repository, set `TIMEWARP_STATE_DEVTOOLS_PROJECT` to the full path of a checkout's `source/timewarp-state-devtools/timewarp-state-devtools.csproj`; without it `AddTimeWarpStateDevTools` throws.

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

## Separate store

The inspector's states are compiled into the DevTools assembly. The inspected app references `TimeWarp.State.DevTools.Export` only. That package has no `IState` types, so the app's `StateCloneRegistry` and `StateTransactionBehavior` never see `InspectorState`.

Spans whose `timewarp.state.state_type` is `InspectorState`, or whose `timewarp.state.inspector` tag is `true`, are dropped on export and again on ingest.

## Packages

| Package | Role |
| --- | --- |
| `TimeWarp.State.DevTools.Export` | Dev-only exporter. Safe for the inspected app to reference. |
| `TimeWarp.State.DevTools.Hosting` | `AddTimeWarpStateDevTools()` and `WithTimeWarpStateDevTools()`. |
| `TimeWarp.State.DevTools` | The Blazor app. Not a NuGet package. |

Production packages do not reference these.

## Replaces the browser extension

Use this app plus [telemetry](xref:TimeWarpState:Telemetry.md) for the action log, the state tree, and the diff. `UseReduxDevTools()` and `<ReduxDevTools />` are removed in 12.0.0-beta.11.
