# Round 2 — general
**Date:** 2026-10-11
**Scope reviewed:** fix delta 1a3b8865..HEAD plus re-verification of M1–M9

## Summary
All nine round 1 findings are fixed in the code. The fix for M4 introduced one regression: the pause toggle still decides from each circuit's own `InspectorState.Paused`, while the button label now reads the shared `InspectorLog.IsPaused`. Tests: `dotnet fixie timewarp-state-devtools-tests` 28 passed, 0 failed; `dotnet fixie timewarp-state-devtools-isolation-tests` 4 passed, 0 failed.

## Prior findings
| ID | Verified status | Notes |
|----|-----------------|-------|
| M1 | fixed | `span-ingest-endpoint.cs:79-83` returns 400 for null `Spans`; null items skipped (`:90`); `TryMap` and `Attribute` turn null strings, null attributes and null attribute values into `""`; `LatestSnapshot` skips null events, null event attributes and maps a null `snapshot.json` to `""`. `NormalizeNullFieldsAndSkipNullItems` covers null item, null Name/Status/SpanId/Attributes/Events, null event, null event attributes, null snapshot value. |
| M2 | fixed | XML doc, Design region, export readme now say `InvalidOperationException` on a non-absolute URI. |
| M3 | fixed | 415 via `HasJsonContentType`; `IHttpMaxRequestBodySizeFeature` set to 1 MiB with `BadHttpRequestException` 413 mapped; snapshot cut at 65536 chars and marked truncated. The chunked 413 test passes (`ChunkedContent` has no length); a Kestrel run was not done, but Kestrel throws the same exception when the limit is exceeded. |
| M4 | fixed, with regression (Issue 1) | `SetPaused` (only on change), `FlushPending` and `Clear` read `Changed` inside the lock and invoke it after the lock is released. Page label reads `InspectorLog.IsPaused`. `RaiseChangedWhenPausedOrClearedSoOtherCircuitsRefresh` asserts the counts. |
| M5 | fixed | `visible` computed once in a `@{ }` block (`Inspector.razor:114-116`) and used for the count and the list. `SyncLogSafeAsync` catches `ObjectDisposedException` and `InvalidOperationException`. |
| M6 | fixed | Hosting readme and `documentation/topics/devtools.md` state the limitation. |
| M7 | fixed | `inspector-log-tests.cs` and `span-ingest-endpoint-tests.cs` added; the concurrency test is deterministic (1000 adds, count 500, 500 distinct sequences, min > 0). The reflection on `StateCloneRegistry.Cloners` in the isolation test remains and the reason is accepted. |
| M8 | fixed | Catch filter is now `exception is not OutOfMemoryException`. |
| M9 | fixed | Hosting has no Export reference, no `using`, no InternalsVisibleTo; its own constant is asserted equal to the Export constant; the allow-list entry for Export is removed and matches the csproj (only `Aspire.Hosting`). Only an XML doc mentions Export, as text. |

## Issues
### Issue 1 — Severity: bug
- File: source/timewarp-state-devtools/features/inspector/inspector-state.toggle-pause.cs:29
- Description: The handler flips the per-circuit `InspectorState.Paused`, but since the M4 fix the label reads the process-wide `InspectorLog.IsPaused`. With two tabs, tab A pauses and tab B's state still has `Paused == false`. Tab B shows "Resume". Clicking it computes `paused = !false = true`, calls `SetPaused(true)` (no change), and the log stays paused with the label still "Resume". The user must click twice.
- Suggestion: Compute `bool paused = !inspectorLog.IsPaused;` in the handler (keeping `InspectorState.Paused` as a mirror, or drop that field). Add a test with two stores and one log.
- Status: open

### Issue 2 — Severity: nit
- File: source/timewarp-state-devtools-export/timewarp-state-dev-tools-export.cs:7-8
- Description: The Design comment now has a very long line from the M2 edit, because the sentence was joined onto the existing line.
- Suggestion: Re-wrap it.
- Status: open
