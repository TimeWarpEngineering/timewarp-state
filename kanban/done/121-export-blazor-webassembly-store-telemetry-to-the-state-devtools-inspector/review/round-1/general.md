# Round 1 — general
**Date:** 2026-10-16
**Scope reviewed:** `git diff master...HEAD` (d302b2d0), 58 files. Covers the new `TimeWarp.State.DevTools.Wasm` package, the Export, Hosting, and inspector changes, sample 04 WASM, and the tests.

## Summary

The change adds a browser exporter. It uses `ActivityListener` with an async `SendAsync` flush, compiles in the shared ingest document and mapping sources, adds a Development-only discovery route, adds Development-only CORS on the inspector, and adds a `timewarp.state.runtime` badge. The design follows the restatement, and the restatement matches the task, so there is no misread.

`dotnet fixie timewarp-state-devtools-tests`: 89 passed, which matches Results.

The main risk is that the end-to-end path has not been proven. Under Aspire the packaged inspector runs as Production, so the new CORS policy is never registered and the browser cannot post. The tests miss this because `WebApplicationFactory` defaults to Development. The other findings are about scope and robustness: CORS applies to every route, discovery ignores the base path, the CORS origin list is last-writer-wins, and one test can never fail.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-state-devtools/dev-tools-ingest-cors.cs:45 (with source/timewarp-state-devtools-hosting/add-timewarp-state-dev-tools.cs:173)
- Description: `DevToolsIngestCors.Add` registers the policy only when `builder.Environment.IsDevelopment()`. The packaged inspector is started by `AddTimeWarpStateDevTools` through `AddExecutable`. Nothing sets `ASPNETCORE_ENVIRONMENT` or `DOTNET_ENVIRONMENT`, and the published `tools/` folder has no launch profile. The sample AppHost has no launchSettings either. So under Aspire the inspector runs as **Production** (the task 118 round-1 review says the same: "it runs as Production under Aspire"). The policy is never added, and the browser's `application/json` POST fails its preflight. As a result, the goal does not work on its main path. The sample's `/counter-wasm` does not show **wasm** rows, and the "How to validate" steps in Results would fail.
  Reproduced against `source/timewarp-state-devtools-app/tools/TimeWarp.State.DevTools.dll` (built from this branch: it contains `TIMEWARP_STATE_DEVTOOLS_CORS_ORIGINS`):
  - Packaged launch shape (`ASPNETCORE_HTTP_PORTS=5399`, `TIMEWARP_STATE_DEVTOOLS_CORS_ORIGINS=http://localhost:5086`, no environment variable): log says `Hosting environment: Production`. `OPTIONS /ingest/spans` with `Origin: http://localhost:5086` returns `405 Method Not Allowed` with no `Access-Control-Allow-Origin`.
  - Same launch with `ASPNETCORE_ENVIRONMENT=Development`: returns `204` with `Access-Control-Allow-Origin: http://localhost:5086`.
  The CORS tests use `WebApplicationFactory<Program>`, which defaults to Development, so they cannot catch this. The hosting tests do not assert the inspector's environment.
- Suggestion: Make the inspector's Development state deliberate in run mode. For example, have `AddTimeWarpStateDevTools` (run mode only; the resource is already `ExcludeFromManifest`) set `ASPNETCORE_ENVIRONMENT=Development` on the executable, or derive the CORS gate from something the AppHost controls. Then:
  - Add a hosting test that asserts the inspector resource's environment.
  - Add a CORS test that runs the real launch shape (no explicit environment), or extend the packaged-app launch test from task 118.
  - Run the sample smoke and record the result in Results.
- Status: open

### Issue 2 — Severity: suggestion
- File: source/timewarp-state-devtools/dev-tools-ingest-cors.cs:96
- Description: `app.UseCors(PolicyName)` applies the policy to every request on the inspector, not just `POST /ingest/spans`. Against a Development run with the origin configured, `GET /` from `Origin: http://localhost:5086` returned `Access-Control-Allow-Origin: http://localhost:5086`. A preflight for `POST /_blazor/negotiate` returned 204 with allow headers. The requirement and the restatement both say CORS on the ingest endpoint. Today the configured origin can read the inspector's pages cross-origin and can preflight the circuit hub. The risk is low because the origin is the inspected dev app, but the inspector is wider than the design says. The private-network `OnStarting` hook is global too.
- Suggestion: Call `app.UseCors()` with no policy name, and attach the policy to the ingest route with `.RequireCors(DevToolsIngestCors.PolicyName)` in `SpanIngestEndpoint.Map` when the policy is registered. Scope the private-network header to that path, or to preflights the policy accepted. Add a test that a non-ingest route gets no `Access-Control-Allow-Origin`.
- Status: open

### Issue 3 — Severity: suggestion
- File: tests/timewarp-state-devtools-tests/wasm-exporter-tests.cs:15
- Description: The proof gate is "a sample or test showing a WASM store's actions appearing in the inspector". `PostAWasmStoreActionIntoTheInspector` runs the exporter on CoreCLR against the TestServer handler. It does not exercise the browser fetch handler, the CORS preflight, or the discovery route. Results give smoke steps under "How to validate" but do not say they were run, and Issue 1 shows they would fail. The browser path is the one at risk.
- Suggestion: After the Issue 1 fix, run the sample through the AppHost with a browser (`playwright-cli` or `dev e2e`). Confirm a **wasm** `WasmCounterState` row, and record the outcome in Results. A Playwright e2e for `/counter-wasm` would make it durable.
- Status: open

### Issue 4 — Severity: suggestion
- File: tests/timewarp-state-devtools-tests/wasm-exporter-tests.cs:48
- Description: `SwallowADownInspector` ends with `exporter.ShouldNotBeNull()`. Post failures happen inside the fire-and-forget `FlushLoopAsync` task. An exception that escaped there would become an unobserved task fault and would not fail the test. The test cannot fail, so it does not prove the "never throws into the app" requirement.
- Suggestion: Make completion observable to tests, for example an internal `FlushTask` with `InternalsVisibleTo`. After `Dispose`, assert the task is not `Faulted`. Use a handler that throws `HttpRequestException` and counts calls, and assert at least one post was attempted.
- Status: open

### Issue 5 — Severity: suggestion
- File: source/timewarp-state-devtools-wasm/timewarp-state-dev-tools-wasm.cs:150
- Description: `new Uri(baseUri, DiscoveryPath)` uses the rooted path `/_timewarp/state-devtools`, which drops any base path. An app served under a base href such as `/app/` with `UsePathBase("/app")` maps the route at `/app/_timewarp/state-devtools`. The client then requests `/_timewarp/state-devtools`, gets a 404, and exports nothing, with no signal. The WASM readme says `GET {base}/_timewarp/state-devtools`, which does not match the code.
- Suggestion: Combine against the base address with a relative path (`_timewarp/state-devtools`, base guaranteed to end in `/`), keep the server `Path` constant as it is, and add a test with a non-root base address. Or, if root-only is intended, fix the readme.
- Status: open

### Issue 6 — Severity: suggestion
- File: source/timewarp-state-devtools-hosting/add-timewarp-state-dev-tools.cs:249
- Description: `PublishClientOrigins` calls `devtools.WithEnvironment(CORS_ORIGINS, …)` for each inspected resource. When an AppHost calls `WithTimeWarpStateDevTools` on two front-ends, the last one wins (the hosting readme says so). Browser posts from the first app are then blocked, and nothing reports it. The timewarp-architecture downstream will likely wire more than one web resource over time.
- Suggestion: Collect origins from every inspected resource. For example, keep an annotation or list on the devtools resource and register one `WithEnvironment` callback that joins all of them. Add a hosting test with two inspected resources.
- Status: open

### Issue 7 — Severity: nit
- File: source/timewarp-state-devtools-export/timewarp-state-devtools-export.csproj:13
- Description: The server exporter package now carries `FrameworkReference Microsoft.AspNetCore.App` for the one discovery route. A non-web consumer (worker or console host exporting server spans) now needs the ASP.NET Core shared framework at runtime. The readme says this, but the release note does not call out the new requirement.
- Suggestion: Either name the framework requirement in `release12.0.0-beta.15.md`, or move `MapTimeWarpStateDevToolsClientConfig` somewhere only web hosts reference.
- Status: open

### Issue 8 — Severity: nit
- File: source/timewarp-state-devtools-wasm/dev-tools-wasm-exporter.cs:105
- Description: `_ = FlushTask.IsCompleted;` (Dispose) and `_ = response.StatusCode;` (PostAsync, line 180) are dead statements. They do nothing, and it is unclear why they are there.
- Suggestion: Remove both. If `FlushTask` is only kept to be read, drop the field and discard the task at construction, or use it for Issue 4.
- Status: open

### Issue 9 — Severity: nit
- File: timewarp-state.slnx:1
- Description: The diff removes the `<Configurations>` block (Debug/Release `BuildType`), which is unrelated to this task. It was probably a side effect of the solution tooling.
- Suggestion: Restore the block unless removing it was intended. If it was, say why in the commit.
- Status: open
