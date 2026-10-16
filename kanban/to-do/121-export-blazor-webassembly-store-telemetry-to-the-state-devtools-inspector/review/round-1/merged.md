# Round 1 — merged findings
**Date:** 2026-10-16
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 5 | 0 | 0 |
| nit | 3 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-state-devtools/dev-tools-ingest-cors.cs:45 (source/timewarp-state-devtools-hosting/add-timewarp-state-dev-tools.cs:173)
- Description: Under Aspire the packaged inspector runs as Production, because `AddExecutable` sets no environment and `tools/` has no launch profile. CORS is gated on `IsDevelopment()`, so it is never registered. The browser preflight gets 405 and WASM spans never reach the inspector. Reproduced against the branch-built `tools/TimeWarp.State.DevTools.dll`: with no environment set, the preflight returned 405 and the log said `Hosting environment: Production`. With `ASPNETCORE_ENVIRONMENT=Development`, it returned 204 with `Access-Control-Allow-Origin`. The tests use `WebApplicationFactory`, which defaults to Development, so they miss this.
- Suggestion: In run mode, make the inspector run as Development (for example, `AddTimeWarpStateDevTools` sets `ASPNETCORE_ENVIRONMENT=Development` on the executable). Add a hosting test for the inspector environment and a CORS test that uses the real launch shape. Run the sample smoke and record it.
- Source: general
- Disposition notes:

### M2 — Severity: suggestion — Status: open
- File: source/timewarp-state-devtools/dev-tools-ingest-cors.cs:96
- Description: `UseCors(PolicyName)` is global. `GET /` and the `/_blazor/negotiate` preflight both return allow headers for the configured origin. Policy and design say ingest only.
- Suggestion: Use `UseCors()` plus `.RequireCors(PolicyName)` on the ingest route, and scope the private-network header. Add a test that a non-ingest route gets no allow header.
- Source: general
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: tests/timewarp-state-devtools-tests/wasm-exporter-tests.cs:15
- Description: The proof gate has only a CoreCLR/TestServer test. No browser run is recorded (no fetch handler, CORS, or discovery), and M1 means the documented smoke fails.
- Suggestion: After M1, run the sample through the AppHost in a browser and record a **wasm** row in Results. Consider a Playwright e2e for `/counter-wasm`.
- Source: general
- Disposition notes:

### M4 — Severity: suggestion — Status: open
- File: tests/timewarp-state-devtools-tests/wasm-exporter-tests.cs:48
- Description: `SwallowADownInspector` asserts `exporter.ShouldNotBeNull()`, and a fault in the fire-and-forget flush loop is unobserved. The test cannot fail.
- Suggestion: Observe the flush task (internal plus `InternalsVisibleTo`) and assert it is not faulted. Use a throwing, counting handler.
- Source: general
- Disposition notes:

### M5 — Severity: suggestion — Status: open
- File: source/timewarp-state-devtools-wasm/timewarp-state-dev-tools-wasm.cs:150
- Description: Discovery uses the rooted `/_timewarp/state-devtools` and drops the app base path. Under `UsePathBase` the route is at `{base}/_timewarp/...`, so discovery gets a 404 and nothing reports it. The readme says `{base}/...`.
- Suggestion: Combine with a relative path against a `/`-terminated base. Add a non-root base test, or fix the readme.
- Source: general
- Disposition notes:

### M6 — Severity: suggestion — Status: open
- File: source/timewarp-state-devtools-hosting/add-timewarp-state-dev-tools.cs:249
- Description: Each `WithTimeWarpStateDevTools` replaces `TIMEWARP_STATE_DEVTOOLS_CORS_ORIGINS`, so with two inspected apps the first app's browser posts are blocked.
- Suggestion: Collect origins from every inspected resource into one callback. Add a two-resource hosting test.
- Source: general
- Disposition notes:

### M7 — Severity: nit — Status: open
- File: source/timewarp-state-devtools-export/timewarp-state-devtools-export.csproj:13
- Description: The Export package now needs the ASP.NET Core shared framework for one route. The release note does not say so.
- Suggestion: Name the requirement in `release12.0.0-beta.15.md`, or move the route out of the exporter package.
- Source: general
- Disposition notes:

### M8 — Severity: nit — Status: open
- File: source/timewarp-state-devtools-wasm/dev-tools-wasm-exporter.cs:105
- Description: Dead statements `_ = FlushTask.IsCompleted;` and `_ = response.StatusCode;` (line 180).
- Suggestion: Remove them. Drop or use the `FlushTask` field (see M4).
- Source: general
- Disposition notes:

### M9 — Severity: nit — Status: open
- File: timewarp-state.slnx:1
- Description: The diff removes the `<Configurations>` Debug/Release block, which is unrelated to this task.
- Suggestion: Restore it unless the removal was intended.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- Single reviewer, so no duplicates. M3 depends on M1: the e2e run is only meaningful after the environment fix.

## Verification notes

- `dotnet fixie timewarp-state-devtools-tests` from the repo root: 89 passed, which matches Results.
- `dotnet run --project tests/timewarp-state-devtools-tests` from the repo root fails 21 `WebApplicationFactory` tests because the content root does not exist. This is a runner working-directory artifact that already existed (the SpanIngestEndpoint tests fail the same way). It is not a finding.
- Restatement against the task: consistent. No misread recorded.
