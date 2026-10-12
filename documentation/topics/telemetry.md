---
uid: TimeWarpState:Telemetry.md
title: TimeWarp.State Telemetry
---

# Observe actions with OpenTelemetry

`TimeWarp.State.Telemetry` instruments the mediator pipeline. Each dispatched action is a span (type names, duration, success/failure) on ActivitySource `TimeWarp.State`. View the timeline in the Aspire dashboard or any OTel backend.

This is the **observation** half of Redux DevTools (action log, timing, optional snapshots). Time-travel, commit, and import/export need a control channel and stay out of this package.

## Register (Blazor Server)

```csharp
builder.Services.AddGeneratedMediator<ClientPipeline>();
builder.Services.AddTimeWarpState();
builder.Services.AddTimeWarpStateBlazor();
builder.Services.AddTimeWarpStateTelemetry();

builder.Services.AddOpenTelemetry()
  .WithTracing(tracing =>
  {
    tracing.AddSource(TimeWarpStateTelemetry.ActivitySourceName);
    tracing.AddOtlpExporter();
  });
```

Default spans have **no payload**. Snapshots are opt-in, sampled, and serialized only through caller `JsonSerializerOptions` / `JsonTypeInfo` (the same options as `TimeWarpStateOptions` after task 065). With snapshots on, state initialization emits `{StateTypeName}.Initialized` (`timewarp.state.kind` = `init` and a `state.snapshot`) before the first action. See the [package README](../../source/timewarp-state-telemetry/readme.md) for AOT, security, and performance notes.

## Sample

[04-Telemetry](../../samples/04-telemetry/overview.md) is a Blazor Server counter wired to the Aspire dashboard.

## Blazor WebAssembly

WASM uses the JS OpenTelemetry SDK (Aspire dashboard browser telemetry: OTLP/HTTP + CORS). When the AppHost launches both the app and the dashboard, CORS is automatic. A standalone dashboard needs `DASHBOARD__OTLP__CORS__ALLOWEDORIGINS`. See [Enable browser telemetry](https://aspire.dev/dashboard/enable-browser-telemetry/).

## DevTools app

[TimeWarp.State DevTools](xref:TimeWarpState:DevTools.md) is a Blazor inspector added to an Aspire AppHost with `AddTimeWarpStateDevTools()`. It reads these spans. The dashboard link is the DevTools resource. The browser-extension time-travel API was removed in 12.0.0-beta.11. Observation is this package plus that app.
