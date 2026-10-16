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

## Goal (implementer restatement)

A Development Blazor WebAssembly store posts its `TimeWarp.State` activities to the running inspector only when the app opts in and an inspector origin is available. Those rows show up beside server-side stores. An unset endpoint does no work. Production and a published bundle that did not opt in do not export. The inspector accepts cross-origin posts only from configured origins, and only on the ingest route.

This fix pass makes that path work under Aspire. The packaged inspector has no launch profile, so run mode has to start it as Development or ingest CORS never registers and the browser preflight fails. CORS headers stay on `POST /ingest/spans`. Discovery keeps the app's base path. Every inspected app contributes origins. A down inspector does not fault the flush task. The release note names the ASP.NET Core shared framework the exporter needs. The solution keeps its Debug and Release configurations.

Acceptance for this pass:

- Run mode sets `ASPNETCORE_ENVIRONMENT=Development` on the packaged inspector. A launch that uses that environment answers the ingest preflight. Publish mode does not set the variable.
- A non-ingest route returns no access-control allow header.
- Discovery against a non-root base requests `{base}/_timewarp/state-devtools`.
- Two `WithTimeWarpStateDevTools` calls keep both apps' origins.
- The down-inspector test observes a throwing post and a flush task that completes successfully.
- `release12.0.0-beta.15.md` names the shared framework requirement.
- `timewarp-state.slnx` still lists Debug and Release.
- The sample smoke records a **wasm** row, or Results say what blocked the browser run.

Acceptance:

- The browser export path does not use a blocking HTTP call or a background batch thread. Inspector downtime or a network error does not throw into the app.
- The body stays the existing dev-only JSON document, so the ingest parser is unchanged.
- CORS on `POST /ingest/spans` is Development-only and limited to configured origins.
- The WASM client learns the origin from configuration or from a Development-only route on the host that reads `TIMEWARP_STATE_DEVTOOLS_ENDPOINT`.
- A row carries `timewarp.state.runtime` (`wasm` or `server`) so a feature that runs in both places stays distinguishable. `timewarp.state.store` still separates store instances.
- Sample 04 hosts a WASM counter on this path. Tests cover CORS allow and deny, the Development and opt-in gates, and a WASM-exported action landing in the inspector log.
- `dev build` is clean, `dev test` is green, and the touched package readmes describe the wiring.

Decisions:

- Wire format stays the dev-only JSON document. OTLP would be a second parser, and the released server exporter already posts this document.
- The WASM exporter is a new package, `TimeWarp.State.DevTools.Wasm`. It uses an `ActivityListener` and does not reference OpenTelemetry. The batch processor's timer thread and synchronous `HttpClient.Send` cannot run on the browser's one thread. Shared document and mapping sources are compiled into that package with internal types so the browser bundle does not load the server exporter.
- Transport is `HttpClient.SendAsync` (the browser fetch handler). There is no `sendBeacon`. A batch still in the queue can be lost when the page goes away. That is acceptable for a dev inspector, and it keeps the failure path inside the catch that already ignores a down inspector.
- Flush is an async loop on the caller: wait for a span, coalesce for 200 ms, post. The queue drops the oldest item when it is full. Nothing in that loop is synchronous HTTP.
- The client reads `TimeWarp:State:DevTools:Endpoint`, then the same environment variable the server uses (empty in the browser), then `GET /_timewarp/state-devtools` on its own origin. The server maps that route only in Development, from `TIMEWARP_STATE_DEVTOOLS_ENDPOINT`. `WithTimeWarpStateDevTools` publishes the inspected app's http and https origins to the inspector as `TIMEWARP_STATE_DEVTOOLS_CORS_ORIGINS`.
- CORS is a named policy. No configured origin means no CORS middleware, so the endpoint is not an open sink. Same-process server posts do not need CORS. When the policy is on and the preflight asks for private-network access, the response includes `Access-Control-Allow-Private-Network`.
- The server exporter stamps `timewarp.state.runtime=server` when the activity has no runtime tag. The WASM exporter stamps `wasm`. The log badge shows `srv` or `wasm` next to the existing pre/live store badge.

## Checklist

- [x] Decide and record answers to the open questions
- [x] WASM-capable exporter and flush strategy
- [x] CORS on the inspector ingest endpoint (Development / configured origins only)
- [x] Endpoint delivery to the WASM client
- [x] Sample or test showing WASM store actions in the inspector
- [x] Tests for CORS and Development-only gating
- [x] Readme updates; `dev build` 0 warnings; `dev test` green
- [x] Review round 1 fixes (M1–M9), including the sample serving `_framework/blazor.web.js`

## Session

- Created: 3911642 (2026-10-16)
- Implemented: Grok (2026-10-16)
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session not reported, max-turns 500, Done — 2026-10-16T09:57:53Z
- Review oracle round 1: Claude Opus 5.5 (claude-opus-5-5), effort 3, roster general, turn cap 200, session not exposed. 9 open findings (M1 bug). Fix pass requested. 2026-10-16
- Review oracle: review by reviewer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-16T10:07:42Z
- Fix pass (implement-fix-2): Grok (2026-10-16). Review round 1 findings M1–M9 are implemented on this branch.
- Review oracle round 2: Claude Opus 5.5 (claude-opus-5-5), effort 2, roster general, turn cap 120, session not exposed. M1–M9 are fixed, 0 open, and the disposition is clean. 2026-10-16

## Results

`TimeWarp.State.DevTools.Wasm` posts `TimeWarp.State` activities from the browser to the existing ingest document. The server stamps `timewarp.state.runtime=server` when that tag is absent. The browser exporter stamps `wasm`. The inspector log copies that field onto the row and shows **srv** or **wasm**.

The browser learns the origin from `TimeWarp:State:DevTools:Endpoint`, then `TIMEWARP_STATE_DEVTOOLS_ENDPOINT`, then `GET /_timewarp/state-devtools` (`MapTimeWarpStateDevToolsClientConfig`, Development only). `WithTimeWarpStateDevTools` sets `TIMEWARP_STATE_DEVTOOLS_CORS_ORIGINS` on the inspector from the inspected app's http and https endpoints. CORS is registered only in Development and only when that list has an absolute http(s) origin.

Packages are `12.0.0-beta.15`. A published app keeps the assembly out with a Debug-only `PackageReference` and `#if DEBUG` around the call. The telemetry sample keeps the reference so `/counter-wasm` stays visible.

`./bin/dev build` exited 0 with 0 compiler errors. The log still prints pre-existing NuGet warnings (NU1510, NU1902) that this change did not add. `./bin/dev test` exited 0, including DevTools (89) and DevTools isolation (4). The telemetry sample server, which hosts the WASM client, builds with 0 warnings.

Review round 1 is addressed on this branch. Run mode sets `ASPNETCORE_ENVIRONMENT=Development` on the packaged inspector, so ingest CORS is registered under Aspire. Allow headers are on `POST /ingest/spans` only, and the private-network header is added only after the policy has allowed the origin. Discovery joins a relative path onto the host base, so a path base is kept. Each `WithTimeWarpStateDevTools` adds origins to one callback. `FlushTask` is public because `InternalsVisibleTo` would also expose the internal copies of the ingest document and collide with the public exporter types. Dispose still does not wait on it. The release note names the `Microsoft.AspNetCore.App` shared framework. `timewarp-state.slnx` lists Debug and Release again.

The sample server was only calling `UseStaticFiles`, which serves `wwwroot` and not the framework manifest. `/counter-wasm` then 404s `_framework/blazor.web.js` and the WASM counter never starts. The host now calls `UseStaticWebAssets` and `MapStaticAssets`, the same pair the inspector uses.

`dotnet test tests/timewarp-state-devtools-tests/timewarp-state-devtools-tests.csproj` passed 94, failed 0. `./bin/dev test` exited 0. `./bin/dev build` exited 0 with 0 errors. Warnings in that log are pre-existing and outside the DevTools projects and sample 04 (nullable warnings in clone tests, TW0007, RS0030, plus NU1510 and NU1902).

### How to validate

Smoke: `dotnet run --project samples/04-telemetry/apphost/sample-04-apphost.csproj`. Open the sample at `http://localhost:5086/counter-wasm` (that origin is the one Aspire puts in `TIMEWARP_STATE_DEVTOOLS_CORS_ORIGINS`). Click **Click me**. Open the inspector URL from `GET http://localhost:5086/_timewarp/state-devtools`.

Expect: the count increases by 5. The inspector shows `WasmCounterState` with a **wasm** badge, including `IncrementCountActionSet.Action`. `OPTIONS /ingest/spans` from `http://localhost:5086` returns 204 with `Access-Control-Allow-Origin: http://localhost:5086`. `GET /` on the inspector does not send that header.

Observed on 2026-10-16: the count went from 3 to 8. The inspector listed `live wasm init WasmCounterState` and `live wasm IncrementCountActionSet.Action WasmCounterState`. The preflight returned 204. `GET /` had no allow-origin header.

### Review

- Round 1 (effort 3, general): 1 bug, 5 suggestions, and 3 nits, all open.
- M1 (bug): under Aspire the packaged inspector runs as Production, so ingest CORS never registers and browser posts fail their preflight. Reproduced against the branch-built `tools/` dll: 405 with no environment set, 204 with Development.
- Round 2 (effort 2, general, delta `90de0a77..ea02ee4a`): re-verified M1–M9, all fixed, no new findings. `dotnet fixie timewarp-state-devtools-tests` passed 94.
- Final counts: bug 0 open / 1 fixed, suggestion 0 open / 5 fixed, nit 0 open / 3 fixed, 0 wontfix.
- **Disposition: clean** (2 rounds, final open count 0).
- Environment note: `dotnet test` (VSTest adapter) on the devtools test project hung in the round-2 reviewer's environment, and it hung the same way at the round-1 commit. `dev test` uses `dotnet fixie`, which passes.
- Paths: `review/review-framework.md`, `review/round-1/merged.md`, `review/round-2/general.md`, `review/round-2/merged.md` (final ledger), `review/disposition.md`.

## Review

- Round 1: Blocked — open M1, M2, M3, M4, M5, M6, M7, M8, M9
- Round 2: Done — M1–M9 fixed, 0 open, disposition clean

## Notes

Context: task 118 (packaged inspector), timewarp-architecture 335-001 (PR #517).
