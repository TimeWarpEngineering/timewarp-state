# TimeWarp.State.DevTools.Export

Dev-only exporter for the TimeWarp.State DevTools inspector. It copies completed activities from ActivitySource `TimeWarp.State` to `POST /ingest/spans` on the DevTools app.

This package has no `IState` types. Referencing it does not register inspector states in `StateCloneRegistry`. Activity tags, including `timewarp.state.store` and `timewarp.state.render`, are copied onto the ingest item.

It is not an OpenTelemetry collector. The body is a small JSON document (name, tags, snapshot events, status, duration), not OTLP. Time-travel and other control operations are not in this package.

## Register

`WithTimeWarpStateDevTools` on the Aspire AppHost sets `TIMEWARP_STATE_DEVTOOLS_ENDPOINT`. In the inspected app:

```csharp
builder.Services.AddOpenTelemetry()
  .WithTracing(tracing =>
  {
    tracing.AddSource(TimeWarpStateTelemetry.ActivitySourceName);
    tracing.AddOtlpExporter();
    tracing.AddTimeWarpStateDevToolsExporter();
  });
```

When the variable is unset or blank, `AddTimeWarpStateDevToolsExporter` does nothing. When it is set to a value that is not an absolute URI, it throws `InvalidOperationException`.

Spans whose `timewarp.state.state_type` is `InspectorState`, or whose `timewarp.state.inspector` tag is `true`, are dropped so the inspector cannot log itself.
