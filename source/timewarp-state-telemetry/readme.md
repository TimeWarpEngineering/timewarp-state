# TimeWarp.State.Telemetry

OpenTelemetry instrumentation for the TimeWarp.State mediator pipeline. Each dispatched action becomes a span with type names, duration, and success/failure on any OTel backend (Aspire dashboard, Jaeger, Seq, Application Insights).

The companion DevTools app reads these spans for the action log, state tree, and diff. OTLP is one-directional.

## Install

```bash
dotnet add package TimeWarp.State.Telemetry
```

The package weaves `TelemetryBehavior<TAction, TResponse>` into `ClientPipeline` the same way `StateTransactionBehavior` is woven (generated mediator, closed types visible to the trimmer). Order 350 sits inside `StateTransactionBehavior` (300) and outside `RenderSubscriptionsPostProcessor` (400): handler failures are recorded as `Error` before the transaction restores state and swallows the exception. Span duration is handler + render, not the clone.

## Register

```csharp
builder.Services.AddGeneratedMediator<ClientPipeline>();
builder.Services.AddTimeWarpState();
builder.Services.AddTimeWarpStateTelemetry();

builder.Services.AddOpenTelemetry()
  .WithTracing(tracing =>
  {
    tracing.AddSource(TimeWarpStateTelemetry.ActivitySourceName);
    // .NET 11 shared framework. OpenTelemetry.Instrumentation.AspNetCore listens to these same sources.
    tracing.AddSource("Microsoft.AspNetCore");
    tracing.AddSource("Microsoft.AspNetCore.Components");
    tracing.AddSource("Microsoft.AspNetCore.Components.Server.Circuits");
    tracing.AddSource("Microsoft.AspNetCore.SignalR.Server");
    tracing.AddOtlpExporter(); // reads OTEL_EXPORTER_OTLP_* (Aspire sets these)
  });
```

`AddTimeWarpStateTelemetry` uses `TryAdd*` so a host can replace options or the snapshot cache.

## What a span contains

Default (no payload):

| Item | Source |
|------|--------|
| Name | Nested declaring-type chain, e.g. `CounterState.IncrementCountActionSet.Action` |
| `timewarp.state.action` | Nested name relative to the enclosing state, e.g. `IncrementCountActionSet.Action` |
| `timewarp.state.state_type` | Enclosing state type name |
| `timewarp.state.store` | `Store.Guid` for the scope that handled the action, formatted `D` |
| `timewarp.state.render` | Optional. `prerender` or `interactive` when the host registered `IStateRenderKind` and a component called `Observe`. Absent otherwise |
| Duration | Activity start/stop (handler + render) |
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

- When a state finishes `Initialize`, and snapshots are on: activity `{StateTypeName}.Initialized` tagged `timewarp.state.action` = `init`, `timewarp.state.state_type`, `timewarp.state.store`, `timewarp.state.kind` = `init`, and `timewarp.state.render` when the host supplied a render kind, plus a `state.snapshot` event. The snapshot cache records that JSON, so the first later change is `state.diff`. No `JsonTypeInfo` skips it with the same warning as an action snapshot. The handler does not await, so the event is recorded before `Store.GetState` returns. A Blazor prerender and the interactive circuit are different stores, so one page load emits two init spans with different store ids.
- First JSON for a state type that was not initialized under snapshots: `state.snapshot` with tag `snapshot.json`
- Later unequal JSON: `state.diff` with the new JSON (string compare of the full JSON, no reflection property walk)
- Equal JSON: no event
- `MaxSnapshotChars` truncates only the event payload and sets boolean tag `snapshot.truncated`

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
- No reflection hydration. State JSON comes from the caller `JsonTypeInfo`.

## Blazor Server

Works out of the box: the circuit runs .NET, so `ActivitySource` spans export over OTLP to the Aspire dashboard (or any backend).

## Blazor WebAssembly / browser telemetry

WASM process telemetry is the **JS OpenTelemetry SDK**, not this `ActivitySource`. That is maintained SDK code.

Aspire dashboard browser telemetry (OTLP/HTTP + CORS):

- When the AppHost launches both the app and the dashboard, CORS for the OTLP HTTP endpoint is configured automatically. See [Enable browser telemetry](https://aspire.dev/dashboard/enable-browser-telemetry/).
- A **standalone** dashboard needs `DASHBOARD__OTLP__CORS__ALLOWEDORIGINS` set to the WASM origin (for example `https://localhost:7001`).

This JS is not the C# trimmer’s problem.

## DevTools

Use this package with [TimeWarp DevTools](../../documentation/topics/devtools.md) for the action log, timing, and snapshots. `UseReduxDevTools()` was removed in 12.0.0-beta.11.

## Sample

`samples/04-telemetry/` is a Blazor Server counter that exports action spans. Run it against the Aspire dashboard (see that sample’s overview).
