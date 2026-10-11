# Round 1 — merged findings
**Date:** 2026-10-11
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 2 | 0 | 0 |
| suggestion | 5 | 0 | 0 |
| nit | 2 | 0 | 0 |

## Issues

The orchestrator checked M1 and M2 against the code (`span-ingest-endpoint.cs:62,80-132`, `timewarp-state-dev-tools-export.cs:26,41-47`). The reviewer's line numbers were wrong; the ones below are corrected.

### M1 — Severity: bug — Status: open
- File: source/timewarp-state-devtools/span-ingest-endpoint.cs:62
- Description: Ingest does not validate the JSON shape. A null `"spans"`, a null span item, or null `attributes`, `events` or event attributes returns a 500. A null `"status"` or `"name"` is stored, and then `Inspector.razor` throws on `entry.Status.ToLowerInvariant()` on every render until the process restarts.
- Suggestion: Return 400 when `Spans` is null and skip null items. In `TryMap`, `Attribute` and `LatestSnapshot`, turn null strings into `""` and null dictionaries or lists into empty ones. Add a test with null fields.
- Source: general
- Disposition notes:

### M2 — Severity: bug — Status: open
- File: source/timewarp-state-devtools-export/timewarp-state-dev-tools-export.cs:26
- Description: The XML doc says the exporter does nothing when the endpoint is not an absolute URI. The code throws, and `RejectANonAbsoluteEndpoint` asserts that it throws.
- Suggestion: Keep failing fast, because only the AppHost sets this endpoint, so a bad value is a wiring error. Update the XML doc and the Design region to say it throws `InvalidOperationException`.
- Source: general
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: source/timewarp-state-devtools/span-ingest-endpoint.cs:36
- Description: The 1 MiB check reads only `ContentLength`, so chunked bodies get past it. Snapshot strings have no size limit. Any content type is accepted, so a cross-origin `text/plain` simple POST can inject entries.
- Suggestion: Enforce the body limit with `IHttpMaxRequestBodySizeFeature` or request-size-limit metadata. Cap the stored snapshot length and mark capped snapshots truncated. Return 415 unless the content type is JSON.
- Source: general
- Disposition notes:

### M4 — Severity: suggestion — Status: open
- File: source/timewarp-state-devtools/inspector-log.cs:86
- Description: `InspectorLog` is shared by the whole process, but each circuit has its own pause display. `SetPaused` and `Clear(false)` do not notify other circuits, so a second tab shows a stale pause state and a stale list.
- Suggestion: Raise `Changed` from `SetPaused` and `Clear`. Read the pause display from `InspectorLog.IsPaused`.
- Source: general
- Disposition notes:

### M5 — Severity: suggestion — Status: open
- File: source/timewarp-state-devtools/components/pages/Inspector.razor
- Description: `VisibleEntries()` runs three times per render, so the count and the rows can disagree. `_ = InvokeAsync(SyncLogAsync)` drops failures, for example from a disposed renderer.
- Suggestion: Compute the entries once per render. Catch `ObjectDisposedException` in the change handler.
- Source: general
- Disposition notes:

### M6 — Severity: suggestion — Status: open
- File: source/timewarp-state-devtools-hosting/dev-tools-project-locator.cs
- Description: The Hosting package is packable but the DevTools app is not packed. Outside this repo, `AddTimeWarpStateDevTools()` only works if `TIMEWARP_STATE_DEVTOOLS_PROJECT` points at a checkout.
- Suggestion: State this limitation in the Hosting readme and in `documentation/topics/devtools.md`.
- Source: general
- Disposition notes:

### M7 — Severity: suggestion — Status: open
- File: tests/timewarp-state-devtools-tests/inspector-loop-tests.cs
- Description: Several tests are missing: concurrent `InspectorLog.Add`, the 500-entry trim, the `MaxSpansPerRequest` cap, the 413 and 415 paths, and payloads with null fields. The isolation test also reads the private `Cloners` field by reflection.
- Suggestion: Add those tests. If `StateCloneRegistry` has a public accessor, use it instead of reflection.
- Source: general
- Disposition notes:

### M8 — Severity: nit — Status: open
- File: source/timewarp-state-devtools-export/dev-tools-span-exporter.cs
- Description: The export catch filter does not catch `OperationCanceledException` (it catches only the `TaskCanceledException` subtype), `IOException` or `ObjectDisposedException`.
- Suggestion: Widen the filter so no export failure escapes the batch thread.
- Source: general
- Disposition notes:

### M9 — Severity: nit — Status: open
- File: source/timewarp-state-devtools-hosting/timewarp-state-devtools-hosting.csproj:20
- Description: `InternalsVisibleTo TimeWarp.State.DevTools.Hosting.Tests` names an assembly that does not exist. Hosting references Export, and through it OpenTelemetry, only for one constant.
- Suggestion: Remove the `InternalsVisibleTo`. Copy the environment-variable constant into Hosting and drop the project reference. Add a test that the two constants match.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None. There was a single reviewer.
