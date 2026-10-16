# TimeWarp.State.DevTools.Wasm

Dev-only browser exporter for the TimeWarp.State DevTools inspector. A Blazor WebAssembly host posts completed `TimeWarp.State` activities to `POST /ingest/spans`.

This package does not reference OpenTelemetry. The browser has one thread, so the server exporter's blocking `HttpClient.Send` cannot run here. `DevToolsWasmExporter` listens with `ActivityListener` and posts with `HttpClient.SendAsync`. A stopped inspector or a network error is swallowed. The last queued batch can be lost when the page goes away: there is no `sendBeacon` flush.

Each posted span is stamped `timewarp.state.runtime=wasm`. The wire document is the same JSON document the server exporter posts. It is not OTLP.

## Opt in

Call this only from a Development host. A missing origin registers nothing: no listener and no request.

```csharp
await builder.AddTimeWarpStateDevToolsWasmExporterAsync();
```

That reads, in order:

1. Configuration `TimeWarp:State:DevTools:Endpoint` when it is non-blank.
2. `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` when the configuration value is blank. The browser process does not receive the variable Aspire sets on the server.
3. `GET {base}/_timewarp/state-devtools` on the host. `{base}` is the host base address, including a path base, with a trailing slash. The server maps that route with `MapTimeWarpStateDevToolsClientConfig()` from `TimeWarp.State.DevTools.Export`, and only in Development. The body is `{ "endpoint": "<origin>" }`. An unset variable is 204.

A value that is not an absolute http(s) URI is ignored. Outside Development the call returns without reading configuration or calling the route.

To turn on snapshots only when an origin resolves, call `ResolveEndpointAsync` yourself and pass the origin to `AddTimeWarpStateDevToolsWasmExporter`. The [telemetry sample](../../samples/04-telemetry/overview.md) does that for `WasmCounterState`.

## Keep it out of a published bundle

Reference the package only for Development builds, and compile the call out of Release:

```xml
<PackageReference Include="TimeWarp.State.DevTools.Wasm" Condition="'$(Configuration)' == 'Debug'" />
```

```csharp
#if DEBUG
await builder.AddTimeWarpStateDevToolsWasmExporterAsync();
#endif
```

Production and `dotnet publish` then contain neither the assembly nor the call. The sample keeps the reference unconditional so the wiring stays visible.

The inspector allows the browser post only in Development, and only for origins in `TIMEWARP_STATE_DEVTOOLS_CORS_ORIGINS`. `WithTimeWarpStateDevTools` sets that from the inspected app's http and https endpoints.
