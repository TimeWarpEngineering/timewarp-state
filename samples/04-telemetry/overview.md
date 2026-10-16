---
uid: TimeWarp.State:04-Telemetry.md
title: TimeWarp.State Telemetry sample
description: Observe TimeWarp.State actions in the Aspire dashboard via OpenTelemetry
---

# TimeWarp.State Telemetry sample

Blazor Server counter that exports one OpenTelemetry span per dispatched action. View the action timeline in the Aspire dashboard.

Time-travel / commit / import-export are not in this sample.

## What it shows

- `AddTimeWarpStateTelemetry()` plus `tracing.AddSource(TimeWarpStateTelemetry.ActivitySourceName)`
- Default metadata-only spans (`CounterState.IncrementCountActionSet.Action`, duration, Ok/Error)
- OTLP export when `OTEL_EXPORTER_OTLP_ENDPOINT` is set (Aspire AppHost sets this)

Snapshots stay off unless `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` is set. The AppHost sets that variable and turns snapshots on through `SampleJsonContext`. See the package README for the `JsonTypeInfo` rule.

## Run with the Aspire dashboard (AppHost)

Pack the in-tree packages first so the sample can restore `TimeWarp.State.Telemetry` from `artifacts/packages`:

```bash
./bin/dev pack
dotnet run --project samples/04-telemetry/apphost/sample-04-apphost.csproj
```

The Aspire CLI writes `aspire.config.json` beside the AppHost; that file is gitignored.

Open the dashboard URL printed by the AppHost. Start the `sample-04-server` resource if it is not already running. The `timewarp-state-devtools` resource is the inspector, started from the `TimeWarp.State.DevTools.App` package with the same `AddTimeWarpStateDevTools()` call any AppHost uses. Its HTTP endpoint is labeled **TimeWarp.State DevTools**. Open that link, then open the sample's `/counter` or `/counter-wasm`.

On connect, DevTools shows an `init` row and a **CounterState** node (snapshots are on because the AppHost set the DevTools endpoint). Click **Click me**. The **State** tab updates `Count` live (each click adds 5). Select an older row: **State** shows the count as of that row, and **Diff** shows the +5 change. **Action**, **State**, and **Diff** are the right-hand tabs; **Tree** and **Raw** switch the state format. Filter, pause, and clear in DevTools do not add rows. The same click is also a trace named `CounterState.IncrementCountActionSet.Action` on source `TimeWarp.State`.

## Run against a standalone dashboard

```bash
aspire dashboard
```

Note the OTLP HTTP endpoint (commonly `http://localhost:18890` or `http://localhost:4318`). Then:

```bash
export OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:18890
export OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
dotnet run --project samples/04-telemetry/server/sample-04-server/sample-04-server.csproj
```

## WebAssembly counter

`/counter-wasm` is `WasmCounterState` in the browser (`InteractiveWebAssembly`, prerender off). It is a different type from the server `CounterState`, so the two stores stay separate in the inspector.

In Development the client calls `GET /_timewarp/state-devtools` on this host. The route returns the `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` origin. The client then posts spans with `TimeWarp.State.DevTools.Wasm`. Rows are stamped `timewarp.state.runtime=wasm` and show a **wasm** badge. Server rows show **srv**. An absent inspector leaves the counter running and exports nothing.

`WithTimeWarpStateDevTools` puts this app's http and https origins into `TIMEWARP_STATE_DEVTOOLS_CORS_ORIGINS` on the inspector. `AddTimeWarpStateDevTools` runs the packaged inspector as Development, so that CORS policy is registered. The browser post is allowed only from this app and only on `POST /ingest/spans`.

Aspire dashboard browser telemetry (the JS OpenTelemetry SDK and `DASHBOARD__OTLP__CORS__ALLOWEDORIGINS`) is a different channel. This sample does not use it for TimeWarp.State.
