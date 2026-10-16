# Export Blazor WebAssembly store telemetry to the State DevTools inspector

## Description

`TimeWarp.State.DevTools.App`, `.Hosting` and `.Export` (12.0.0-beta.14, task 118, Aspire-dashboard
pattern) let an AppHost run the inspector and let a server-side host export store activity to it.
The timewarp-architecture template (its task 335-001, PR #517) wires `AddTimeWarpStateDevTools()` in the
AppHost and `WithTimeWarpStateDevTools()` on web-server. Web-server exports server-side stores
(prerender and InteractiveServer) in Development when `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` is set.

**Gap:** stores running in Blazor WebAssembly are not exported. After first load, most state lives in
WASM, so the inspector shows only part of the picture. Causes:

- The browser has no tracing / OpenTelemetry pipeline set up for `TimeWarp.State` activities.
- The inspector runs on a different origin from the app, so a browser POST needs CORS on the ingest
  endpoint, which has none.
- The current exporter is not WASM-capable (see Current code).

**Goal:** a Blazor WASM app in Development, opted in, sends its `TimeWarp.State` store spans to the
running inspector, and they appear alongside server-side stores. No effect on production or publish.

## Current code (read-only findings)

- Exporter package: `source/timewarp-state-devtools-export/`
  - `timewarp-state-dev-tools-export.cs`: `AddTimeWarpStateDevToolsExporter(this TracerProviderBuilder)`.
    No-op when env var `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` is unset; throws on a non-absolute URI;
    adds `BatchActivityExportProcessor` (200 ms delay) wrapping `DevToolsSpanExporter`.
    `IngestUri` builds `{origin}/ingest/spans`.
  - `dev-tools-span-exporter.cs`: `BaseExporter<Activity>`; synchronous `HttpClient.Send` on the batch
    processor's background thread, 2 s timeout, failures return `ExportResult.Failure` without throwing.
    Copies only `InspectorCapture.ActivitySourceName` activities; builds `SpanIngestItem`s.
  - `inspector-capture.cs` (capture filter and tag names), `span-ingest-document.cs` (the JSON wire
    document, source-generated `SpanIngestJsonContext`). The protocol is a dev-only JSON document, **not OTLP**.
  - Package refs `OpenTelemetry` (csproj). Target framework net11.0 from root `Directory.Build.props`.
- Inspector ingest: `source/timewarp-state-devtools/span-ingest-endpoint.cs` (`SpanIngestEndpoint.Map`,
  `POST /ingest/spans`, JSON content type only, 1 MiB cap, 512 spans max, `DisableAntiforgery`).
  Mapped in `source/timewarp-state-devtools/program.cs` (line ~50). No CORS services or middleware
  exist there today.
- Endpoint delivery today: `source/timewarp-state-devtools-hosting/add-timewarp-state-dev-tools.cs`
  (`WithTimeWarpStateDevTools`) sets the env var `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` on the
  consuming resource. Env vars do not reach the browser, so the WASM client has no way to learn it yet.
- Existing tests/samples to extend or mirror: `tests/timewarp-state-devtools-tests`,
  `tests/timewarp-state-devtools-inspected-app`, `tests/timewarp-state-devtools-isolation-tests`,
  `samples/04-telemetry`.

## Requirements

- **WASM-capable export path.** The server exporter's blocking `HttpClient.Send` on a background
  thread cannot work in the browser (single-threaded runtime, no synchronous HTTP). Provide an
  export path that works in WASM, reusing the existing ingest document so the inspector needs no
  new parser unless the implementer decides otherwise.
- **Flush strategy for single-threaded WASM.** Replace the timer-thread batch assumption with a
  strategy that runs on the one thread (for example async fetch-based flush on a timer/yield, or flush
  per action). Failure (inspector stopped, network error) must never throw into the app.
- **CORS on the ingest endpoint**, restricted to Development and to configured origins. The inspector
  must not become an open cross-origin sink.
- **Endpoint discovery for the WASM client.** The client needs the inspector origin from somewhere
  the host controls (host-delivered config, for example the hosting side surfaces the env value into
  client configuration). Document how an app wires it.
- **Development-only and opt-in**, like the server path: unset or absent endpoint means no export,
  no sockets, no extra work. Production and publish output are unaffected (no dev packages or code
  paths shipping in a published WASM bundle without opt-in).
- Server-side export behavior and wire compatibility with already-released packages are preserved.

## Open questions (implementer decides, documents the choice and why)

- Wire format: keep the dev-only JSON document, or add OTLP/HTTP+JSON? (Browser-capable either way;
  OTLP/gRPC and the OpenTelemetry batch background thread are not.)
- Where the WASM exporter lives: new package, a multi-target of `DevTools.Export`, or a separate
  entry point in the same package. Does `OpenTelemetry` itself run in WASM, or is a direct
  `ActivityListener` simpler?
- Transport: `HttpClient` on the WASM handler vs JS `fetch` interop (and `keepalive` / `sendBeacon`
  for page unload).
- How the client learns the endpoint: Hosting-provided config endpoint, a generated JSON in
  `wwwroot`, or a value the host injects at render time.
- CORS shape: named policy, allowed-origin source (config key, Aspire-supplied env var), and whether
  preflight needs `Access-Control-Allow-Private-Network` for localhost cases.
- How WASM spans are distinguished from server spans in the inspector (`StoreId`/tag) so a store
  that renders in both places stays understandable.

## Proof gates

- A sample or test showing a WASM store's actions appearing in the inspector (extend
  `tests/timewarp-state-devtools-inspected-app` or `samples/04-telemetry`; state which).
- Tests cover the CORS allow and deny cases and the Development-only / opt-in gating.
- Existing tests pass (`dev test`); isolation tests still pass.
- `dev build` is 0 warnings and 0 errors.
- Package readmes for the touched packages describe the WASM wiring.

## Downstream

Once released, timewarp-architecture follows up to wire the WASM client (its template currently
exports server-side stores only).

## Checklist

- [ ] Decide and record answers to the open questions
- [ ] WASM-capable exporter and flush strategy
- [ ] CORS on the inspector ingest endpoint (Development / configured origins only)
- [ ] Endpoint delivery to the WASM client
- [ ] Sample or test showing WASM store actions in the inspector
- [ ] Tests for CORS and Development-only gating
- [ ] Readme updates; `dev build` 0 warnings; `dev test` green

## Session

- Created: 3911642 (2026-10-16)

## Notes

Context: task 118 (packaged inspector), timewarp-architecture 335-001 (PR #517).
