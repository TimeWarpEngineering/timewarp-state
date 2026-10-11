# Round 2 — merged findings
**Date:** 2026-10-11
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 3 | 0 |
| suggestion | 0 | 5 | 0 |
| nit | 0 | 3 | 0 |

## Prior findings, re-verified

Fix commit `454ee6cf`. The round 2 reviewer checked each one against the code.

| ID | Severity | Status | Notes |
|----|----------|--------|-------|
| M1 | bug | fixed | Ingest returns 400 for null `Spans`, skips null items, and treats null strings, collections and event attributes as empty. A test covers it. |
| M2 | bug | fixed | The code still throws. The XML doc, Design region, export readme and `devtools.md` now say it throws `InvalidOperationException`. |
| M3 | suggestion | fixed | 415 for non-JSON content types. The 1 MiB limit now covers chunked bodies and returns 413. Snapshots are capped at 65,536 characters and marked truncated. |
| M4 | suggestion | fixed | `SetPaused`, `FlushPending` and `Clear` raise `Changed` outside the lock, and the label reads `InspectorLog.IsPaused`. The fix introduced the regression M10. |
| M5 | suggestion | fixed | The page computes visible entries once per render, and `SyncLogSafeAsync` observes errors after dispose. |
| M6 | suggestion | fixed | The Hosting readme and `devtools.md` say the app is not packed and name the `TIMEWARP_STATE_DEVTOOLS_PROJECT` requirement. |
| M7 | suggestion | fixed | New tests in `inspector-log-tests.cs` and `span-ingest-endpoint-tests.cs`. The isolation test still reads the private `Cloners` field by reflection, because `StateCloneRegistry` has no accessor and this task does not add API to `TimeWarp.State`. |
| M8 | nit | fixed | The export catch filter is now `exception is not OutOfMemoryException`. |
| M9 | nit | fixed | Removed the `InternalsVisibleTo` and the Export reference. The constant is now in Hosting, and a test asserts it matches. Allow-list updated. |

## Issues

### M10 — Severity: bug — Status: fixed
- File: source/timewarp-state-devtools/features/inspector/inspector-state.toggle-pause.cs:29
- Description: The M4 change made the button label read the shared `InspectorLog.IsPaused`, but the handler still toggled from the circuit's own `InspectorState.Paused`. If tab A paused, tab B showed "Resume", but clicking it set paused to true again, so the user had to click twice.
- Suggestion: Toggle from `inspectorLog.IsPaused`.
- Source: general
- Disposition notes: Fixed by the orchestrator. The handler now computes `paused = !inspectorLog.IsPaused`, and the Design region was updated. The new test `InspectorLoop_Should.ToggleFromTheSharedPauseFlag` pauses from one scope and resumes from another. That test fails against the old code.

### M11 — Severity: nit — Status: fixed
- File: source/timewarp-state-devtools-export/timewarp-state-dev-tools-export.cs:7
- Description: The M2 edit left an overlong line in the Design comment.
- Suggestion: Rewrap it.
- Source: general
- Disposition notes: Rewrapped.

## Duplicates / conflicts

- None.
