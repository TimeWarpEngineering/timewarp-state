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

A copied span with no `timewarp.state.runtime` tag is stamped `server`. A tag that is already present is left as it is. The browser exporter stamps `wasm`.

Spans whose `timewarp.state.state_type` is `InspectorState`, or whose `timewarp.state.inspector` tag is `true`, are dropped so the inspector cannot log itself.

## Browser discovery

The browser does not see `TIMEWARP_STATE_DEVTOOLS_ENDPOINT`. On the server that hosts the WebAssembly app:

```csharp
app.MapTimeWarpStateDevToolsClientConfig();
```

In Development that maps `GET /_timewarp/state-devtools`. A set variable returns `{ "endpoint": "<origin>" }`. An unset variable returns 204. Other environments do not map the route. `TimeWarp.State.DevTools.Wasm` calls it. This package references the shared ASP.NET Core framework for that one route. It does not add a package dependency.
