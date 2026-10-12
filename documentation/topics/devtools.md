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

There is no time-travel, commit, or import/export channel. Those stay on `UseReduxDevTools` until generated hydration exists.

## State panel

The right-hand pane has **Action**, **State**, and **Diff** tabs.

**Action** lists the selected span: activity name, state type, status, duration, start time, span id, and the other tags.

**State** lists every state type seen. Each node is a collapsible JSON tree. **Tree** and **Raw** switch the format. With no row selected the heading is **Current state**: the latest snapshot of each state type. With a row selected the heading is **State after action**: the fold of snapshots up to and including that row. Expanded and collapsed paths stay put as new rows arrive.

**Diff** compares the selected snapshot with the previous snapshot of the same state type. Added, removed, and changed values are colour coded. Equal JSON says **No change.** A span with no snapshot says so. A truncated payload is shown as raw text with a hint to raise `MaxSnapshotChars`; it is not parsed as a tree.

The filter box matches action names and state type names in the log and in the State panel.

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

## Redux DevTools

Use this app plus [telemetry](xref:TimeWarpState:Telemetry.md) for the action log. Keep [Redux DevTools](xref:TimeWarpState:AddReduxDevTools.md) only when you still need time-travel, commit, or import/export. `UseReduxDevTools` is unchanged.
