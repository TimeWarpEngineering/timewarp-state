# TimeWarp.State.Telemetry

OpenTelemetry instrumentation for the TimeWarp.State mediator pipeline. Each dispatched action becomes a span with type names, duration, and success/failure — the observation half of Redux DevTools, on any OTel backend (Aspire dashboard, Jaeger, Seq, Application Insights).

Time-travel, commit, and import/export stay out of this package. Those need a dev-only back-channel; OTLP is one-directional.

## Install

```bash
dotnet add package TimeWarp.State.Telemetry
```

The package weaves `TelemetryBehavior<TAction, TResponse>` into `ClientPipeline` the same way `StateTransactionBehavior` is woven (generated mediator, closed types visible to the trimmer).

## Register

```csharp
builder.Services.AddGeneratedMediator<ClientPipeline>();
builder.Services.AddTimeWarpState();
builder.Services.AddTimeWarpStateTelemetry();

builder.Services.AddOpenTelemetry()
  .WithTracing(tracing =>
  {
    tracing.AddSource(TimeWarpStateTelemetry.ActivitySourceName);
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddOtlpExporter(); // reads OTEL_EXPORTER_OTLP_* (Aspire sets these)
  });
```

`AddTimeWarpStateTelemetry` uses `TryAdd*` so a host can replace options or the snapshot cache.

## What a span contains

Default (no payload):

| Item | Source |
|------|--------|
| Name | `{StateType}.{ActionType}` |
| `timewarp.state.action` | Action type name |
| `timewarp.state.state_type` | Enclosing state type name |
| Duration | Activity start/stop |
| Status | `Ok` or `Error` (exception recorded) |

An action that triggers HTTP is the parent of that HTTP span when the handler runs under `Activity.Current`.

## Opt-in snapshots

Snapshots are **off** unless all of these are true:

1. An Activity listener is attached (`ActivitySource.HasListeners()`).
2. The span is sampled (`IsAllDataRequested`).
3. `TimeWarpStateTelemetryOptions.IncludeSnapshots` is true.
4. Caller-supplied `JsonSerializerOptions.TypeInfoResolver` returns `JsonTypeInfo` for the state type.

```csharp
builder.Services.AddTimeWarpStateTelemetry(options =>
{
  options.IncludeSnapshots = true;
  options.JsonSerializerOptions = timeWarpStateJsonOptions; // TypeInfoResolver required
  options.MaxSnapshotChars = 16_384;
});
```

Or set `TypeInfoResolver` on `TimeWarpStateOptions.JsonSerializerOptions` (the same options Store and persistence use). This package never calls `new JsonSerializerOptions()` and never `JsonSerializer.Serialize(object)` on an open `TState`.

Events (not span attributes):

- First JSON for a state type in the scope: `state.snapshot` with tag `snapshot.json`
- Later unequal JSON: `state.diff` with the new JSON (string compare, no reflection property walk)
- Equal JSON: no event

## Performance

The dispatch hot path with no listener is `ActivitySource.HasListeners()` (false) and `next()`. No `GetState`, no JSON, no snapshot cache.

With a listener, `StartActivity` returns null when the sampler drops the span; that path also skips snapshots.

## Security

State JSON may contain user data. Defaults emit **no payload**. Enable snapshots only in local/dev, pair them with sampling, and redact via `JsonConverter` / source-generated context (omit secrets, tokens, PII). Truncation (`MaxSnapshotChars`) is a size cap, not redaction.

Do not enable `IncludeSnapshots` in production unless the OTel backend is access-controlled and payloads are redacted.

## AOT / trimming

- `IsAotCompatible=true` on this package.
- Default spans use `typeof` names only.
- Snapshots require caller `JsonTypeInfo` (typically a `JsonSerializerContext`).
- No `Type.GetType`, no `AssemblyQualifiedName`, no `GetProperties` walk.
- Time-travel / `LoadStatesFromJson` is not in this package.

## Blazor Server

Works out of the box: the circuit runs .NET, so `ActivitySource` spans export over OTLP to the Aspire dashboard (or any backend).

## Blazor WebAssembly / browser telemetry

WASM process telemetry is the **JS OpenTelemetry SDK**, not this `ActivitySource`. That is maintained SDK code, not the custom `redux-dev-tools.ts` / `timewarp-state.ts` bridge.

Aspire dashboard browser telemetry (OTLP/HTTP + CORS):

- When the AppHost launches both the app and the dashboard, CORS for the OTLP HTTP endpoint is configured automatically. See [Enable browser telemetry](https://aspire.dev/dashboard/enable-browser-telemetry/).
- A **standalone** dashboard needs `DASHBOARD__OTLP__CORS__ALLOWEDORIGINS` set to the WASM origin (for example `https://localhost:7001`).

This JS is not the C# trimmer’s problem.

## Redux DevTools

Use this package for the **observation** half (action log, timing, snapshots). Keep `UseReduxDevTools` only if you still need time-travel / commit / import-export in the browser extension. A dedicated control channel is a follow-on; that path is AOT-hostile.

## Sample

`samples/04-telemetry/` is a Blazor Server counter that exports action spans. Run it against the Aspire dashboard (see that sample’s overview).
