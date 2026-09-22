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
- Default metadata-only spans (`CounterState.Action`, duration, Ok/Error)
- OTLP export when `OTEL_EXPORTER_OTLP_ENDPOINT` is set (Aspire AppHost sets this)

Snapshots stay off. Enable them only with caller `JsonTypeInfo` — see the package README.

## Run with the Aspire dashboard (AppHost)

Pack the in-tree packages first so the sample can restore `TimeWarp.State.Telemetry` from `artifacts/packages`:

```bash
./bin/dev pack
dotnet run --project samples/04-telemetry/apphost/sample-04-apphost.csproj
```

Open the dashboard URL printed by the AppHost. Start the `sample-04-server` resource if it is not already running, open `/counter`, click **Click me**. In **Traces**, each click is a span named `CounterState.Action` on source `TimeWarp.State`.

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

## WASM / browser telemetry

This sample is Blazor Server (the C# `ActivitySource` path). WASM uses the JS OpenTelemetry SDK and Aspire dashboard browser telemetry (OTLP/HTTP + CORS). When an AppHost launches both the app and the dashboard, CORS is configured automatically. A standalone dashboard needs `DASHBOARD__OTLP__CORS__ALLOWEDORIGINS` set to the WASM origin. See [Enable browser telemetry](https://aspire.dev/dashboard/enable-browser-telemetry/).
