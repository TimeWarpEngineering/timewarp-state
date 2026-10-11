# Round 1 — general
**Date:** 2026-10-11
**Scope reviewed:** branch vs master (f7e837f7)

## Summary
The design meets the stated acceptance criteria: both new test projects pass (17 and 4), TimeWarp.State is untouched, nothing production depends on the new packages, and the exporter never throws into the app's pipeline. The main defects are in the ingest endpoint, which trusts the JSON shape (a null field produces a 500 or a poison log entry that crashes the page), and in the exporter, whose registration code contradicts its documentation. The rest is process-wide mailbox semantics across multiple circuits, packaging usefulness of the Hosting package outside the repo, and test gaps. `dotnet build timewarp-state.slnx` has 3 errors, all in samples/05-persistence, a project this diff does not touch. The new projects build with no warnings of their own.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-state-devtools/span-ingest-endpoint.cs:221
- Description: The ingest endpoint does not validate the deserialized shape. System.Text.Json source-gen does not enforce non-nullable annotations, so `"spans": null`, a `null` span element, `"attributes": null`, `"events": null` or `"status": null` all deserialize to null members. `document.Spans.Count`, `TryMap` (ThrowIfNull on item), `Attribute()` and `LatestSnapshot()` then throw and return a 500. Worse, `"status": null` (or missing name) passes `TryMap` and is stored in `InspectorLog`. `Inspector.razor` line 257 calls `entry.Status.ToLowerInvariant()` on every render, so the page throws on every render and on every new circuit until the process restarts. Clear cannot be clicked because the page no longer renders. The endpoint is unauthenticated, and it also accepts any content type (see Issue 3).
- Suggestion: Normalize in `TryMap` (`item.Name ?? ""`, `item.Status ?? ""`, null-safe `Attributes` and `Events`), skip null items, and return 400 when `Spans` is null. Add a test with null fields.
- Status: open

### Issue 2 — Severity: bug
- File: source/timewarp-state-devtools-export/timewarp-state-dev-tools-export.cs:213
- Description: The XML doc on `AddTimeWarpStateDevToolsExporter` says it "Does nothing when ... unset or not an absolute URI", and the Design region says it is a no-op when unset. The code throws `InvalidOperationException` for a non-absolute value, and the test `RejectANonAbsoluteEndpoint` asserts the throw. Both the doc and the sample's contract ("does not open a socket") disagree with the code. Throwing at tracer-provider construction also lets a bad dev-only env var take down the inspected app's startup.
- Suggestion: Pick one. Either fix the doc to say it throws, or make it a silent no-op and update the test.
- Status: open

### Issue 3 — Severity: suggestion
- File: source/timewarp-state-devtools/span-ingest-endpoint.cs:191
- Description: The body size guard (`ContentLength > 1 MiB`) is bypassed by chunked requests, which fall back to the Kestrel default of 30 MB. Individual `snapshot.json` values are not bounded. 500 visible entries plus up to 500 pending entries, each carrying an arbitrarily large string, make memory per process effectively unbounded. The endpoint also does not require `Content-Type: application/json`, so a cross-origin `text/plain` "simple" POST from any web page open in the developer's browser can inject entries without a CORS preflight.
- Suggestion: Use `IHttpMaxRequestBodySizeFeature` or `.WithRequestSizeLimit`. Cap the snapshot length at ingest. Reject non-JSON content types with 415.
- Status: open

### Issue 4 — Severity: suggestion
- File: source/timewarp-state-devtools/inspector-log.cs:17
- Description: `InspectorLog` is a process-wide singleton, but `InspectorState.Paused`, filter and selection are per circuit. With two browser tabs, pausing or clearing in tab A changes the global pause flag and clears the global list, yet tab B still shows "Pause" and keeps a stale selection. `Clear(raiseChanged: false)` and `SetPaused` do not notify other circuits, so tab B only refreshes on the next Add. A circuit that disposes while paused leaves the whole inspector paused forever with no UI indicating it.
- Suggestion: Raise `Changed` from `Clear` and `SetPaused`, derive the pause display from `InspectorLog.IsPaused`, or document the single-viewer assumption. Consider un-pausing on the last circuit's disposal.
- Status: open

### Issue 5 — Severity: suggestion
- File: source/timewarp-state-devtools/components/pages/Inspector.razor:233
- Description: `VisibleEntries()` is invoked three times per render (count, empty check and the foreach), and `SelectedEntry()` takes another snapshot. Each call copies the up-to-500-entry array and re-filters it. The three views of the log can differ if an ingest lands between calls, so the count and the rows can disagree. `OnLogChanged` also fires `_ = InvokeAsync(...)` without observing failures, so an `InvokeAsync` against a disposed renderer is an unobserved exception.
- Suggestion: Compute `entries` once per render in a local (`@{ ... }`). Catch `ObjectDisposedException` in `SyncLogAsync`.
- Status: open

### Issue 6 — Severity: suggestion
- File: source/timewarp-state-devtools-hosting/dev-tools-project-locator.cs:121
- Description: `TimeWarp.State.DevTools.Hosting` is packable, but the DevTools app is not packed and the integration locates it by walking up to `source/timewarp-state-devtools/...` or via an env var. A NuGet consumer outside this repo therefore cannot use `AddTimeWarpStateDevTools()` without setting `TIMEWARP_STATE_DEVTOOLS_PROJECT` to a checkout. The task's own open question (where the package lives and how it ships) is only resolved in-repo.
- Suggestion: State this limitation prominently in the Hosting readme and in `devtools.md`. Alternatively, defer `IsPackable` for Hosting until the app ships as a dotnet tool or a container, or add a follow-up task.
- Status: open

### Issue 7 — Severity: suggestion
- File: tests/timewarp-state-devtools-tests/inspector-loop-tests.cs:15
- Description: The test gaps against the stated hardening are:
  1. No concurrency test for `InspectorLog` (the main thread-safety claim).
  2. No test of the 500-entry trim, the `MaxSpansPerRequest` cap or the 413 path.
  3. No test of malformed-shape payloads (see Issue 1).
  4. The isolation test's proof that "no DevTools action passes through `StateTransactionBehavior`" is structural. It checks the absence of a "Inspector" method name and a `StateTransactionBehavior` type reference in the sample assembly, and it reads the private static `Cloners` field by reflection, so it breaks if the generator renames that field.
  5. Export-to-ingest is never exercised end to end (`WebApplicationFactory` plus the real `DevToolsSpanExporter`).
- Suggestion: Add a parallel `Add` test, a trim and cap test, a null-field ingest test and an exporter-to-factory round-trip. Prefer a public or internal accessor on `StateCloneRegistry` over reflection if one exists.
- Status: open

### Issue 8 — Severity: nit
- File: source/timewarp-state-devtools-export/dev-tools-span-exporter.cs:134
- Description: The catch filter covers `HttpRequestException`, `TaskCanceledException` and `InvalidOperationException`. A sync `HttpClient.Send` timeout can surface as `OperationCanceledException` (the base type), and `IOException` or `ObjectDisposedException` on shutdown are not covered. Export has a 2 s timeout per batch on the batch thread, and a down inspector means a 2 s stall per 200 ms batch. This never blocks the app, but it queues activities up to the processor's buffer (default 2048) and then drops them, which is acceptable.
- Suggestion: Widen to `catch (Exception) when (exception is not OutOfMemoryException)`, or at least add `OperationCanceledException`, `IOException` and `ObjectDisposedException`. Consider a circuit-breaker/backoff after repeated failures.
- Status: open

### Issue 9 — Severity: nit
- File: source/timewarp-state-devtools-hosting/timewarp-state-devtools-hosting.csproj:20
- Description: `InternalsVisibleTo TimeWarp.State.DevTools.Hosting.Tests` names an assembly that does not exist. The only test assembly is `TimeWarp.State.DevTools.Tests`, and the Hosting project has no internals to expose. Hosting also takes its `Export` project reference (and so the OpenTelemetry dependency) only for a const string, which pulls OpenTelemetry into every AppHost.
- Suggestion: Remove the `InternalsVisibleTo`. Duplicate the env-var name const in Hosting, or move the const into a tiny shared location, to avoid the OpenTelemetry dependency on AppHosts.
- Status: open
