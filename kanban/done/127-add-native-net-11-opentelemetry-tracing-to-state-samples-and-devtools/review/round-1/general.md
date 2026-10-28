# Round 1 — general
**Date:** 2026-10-29
**Scope reviewed:** the uncommitted working tree compared with HEAD (see review-framework.md)

## Summary

The product change is small and correct. The telemetry sample drops `OpenTelemetry.Instrumentation.AspNetCore`
(csproj and CPM) and subscribes with `AddSource` to `Microsoft.AspNetCore`, `Microsoft.AspNetCore.Components`,
`Microsoft.AspNetCore.Components.Server.Circuits`, and `Microsoft.AspNetCore.SignalR.Server`. Docs are updated, and a
server-exporter test rejects those four source names. Verified:

- `dotnet build` of sample-04-server: 0 errors.
- `dotnet test` of timewarp-state-devtools-tests: 102/102 passed, including the new `IgnoreNativeFrameworkActivitySources`.
- The ".NET 11 stamps the HTTP semantic tags natively" claim holds. In the RC3 `Microsoft.AspNetCore.Hosting.dll`,
  `HostingApplicationDiagnostics.GetSuppressActivityOpenTelemetryData()` returns false when the switch is unset, so
  the tags are on by default.
- No residual reference to `AddAspNetCoreInstrumentation` or the package outside `artifacts/`.

The goal restatement matches the task.

What blocks the task is completeness, not the code. The implementation was never committed, the checklist and
Results were never written, and the WASM half of the inspector check was never confirmed. The implement log ends:
"Verification is incomplete. Remaining tasks include … running the dev test suite and repo audit, updating the session
checklist and results, committing". The oracle then returned Done.

## Issues

### Issue 1 — Severity: bug
- File: kanban/in-progress/127-add-native-net-11-opentelemetry-tracing-to-state-samples-and-devtools/task.md:44
- Description: The implementation is uncommitted. All 11 modified product files and the untracked
  `native-trace-sources.cs` are only in the working tree, and `master..HEAD` holds only kanban commits. Every
  `## Checklist` item is still `[ ]`. There is no `## Results` section, and no repo audit was run. The untracked
  `.playwright-cli/` directory at the repo root is implementer scratch that must not be committed. The host's land
  step commits only kitchen paths, so the product change would be lost or would ship with no trail.
- Suggestion: Run `ganda repo audit` and the test suite. Commit the product diff, including
  `native-trace-sources.cs` and excluding `.playwright-cli/` (delete it or gitignore it). Tick the checklist items
  that were actually verified, and write `## Results`: files changed, tests run, and the span evidence from the OTLP
  capture (one `HandleEvent` with the `CounterState.IncrementCountActionSet.Action` child).
- Status: open

### Issue 2 — Severity: bug
- File: samples/04-telemetry/overview.md:34
- Description: Two requirements are unconfirmed for WebAssembly. One is the checklist item "Confirm the devtools
  inspector still shows actions and state for server and WASM counters". The other is the requirement that the
  inspector receives State spans from server and WASM. Under the AppHost, the implementer browsed the server's
  DCP target port (`localhost:20395`), and WASM posts failed CORS. The devtools CORS origins come from the
  `EndpointReference` values (`add-timewarp-state-dev-tools.cs:304-354`), which resolve to the proxied endpoint
  (`localhost:5086`, the launch-profile port). The failure is most likely a test-setup artifact, not a product
  regression. The implementer called it "pre-existing" but never checked that against master.
- Suggestion: Re-run the AppHost and open `/counter-wasm` through the dashboard's endpoint link (the proxied
  `http://localhost:5086`). Click once and confirm that a `WasmCounterState` action row appears in the inspector.
  Record the result in Results. If it still fails on that origin, check master. If master fails too, record it as
  pre-existing, with a follow-up task id, rather than leaving the item silently unchecked.
- Status: open

### Issue 3 — Severity: suggestion
- File: tests/timewarp-state-devtools-tests/wasm-exporter-tests.cs:15
- Description: The goal says DevTools ingests only `TimeWarp.State` "on the server and in WebAssembly", and the diff
  adds a comment to `dev-tools-wasm-exporter.cs` saying the same. Only the server `DevToolsSpanExporter` got a test
  for the four framework source names. The WASM listener's `ShouldListenTo` predicate
  (`dev-tools-wasm-exporter.cs:90`) has no test that rejects `Microsoft.AspNetCore.Components`, which is the source
  that actually fires in the browser.
- Suggestion: Add a WASM exporter test that starts an activity on `Microsoft.AspNetCore.Components`, with State tags
  set, and asserts nothing is queued or posted. One option is to reuse the existing
  `PostAWasmStoreActionIntoTheInspector` harness and assert that only the `TimeWarp.State` item arrives.
- Status: open

### Issue 4 — Severity: nit
- File: samples/04-telemetry/server/sample-04-server/native-trace-sources.cs:9
- Description: The Design region says keeping the instrumentation package "would add a second listener on one span,
  which is the duplication this sample avoids". The implementer's own investigation found that the package enriches
  the same `HttpRequestIn` activity and does not create a second span. A second listener is not span duplication, so
  the comment overstates why the package is removed. The real reason is that the package is redundant: the framework
  already sets the semantic tags, and the package skips its own when it detects them (`AspNetCoreHasNativeOpenTelemetryTags`).
- Suggestion: Reword it, for example: "That package enriches the same activity. On .NET 11 the framework already
  sets the semantic tags, so the package adds a dependency and no spans."
- Status: open
