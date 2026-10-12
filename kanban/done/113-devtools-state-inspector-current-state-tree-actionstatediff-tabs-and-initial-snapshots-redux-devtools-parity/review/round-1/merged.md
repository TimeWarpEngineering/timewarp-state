# Round 1 — merged findings
**Date:** 2026-10-12
**Sources:** general, general-telemetry

## Counts (at merge; live ledger below)

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 5 | 0 | 0 |
| nit | 4 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-state-devtools/inspector-log.cs (AppendVisible / FlushPending → RebuildLatest)
- Description: Trimming to 500 entries rebuilds `LatestByStateType` from surviving entries, so an idle state whose only snapshot was evicted vanishes from Current state.
- Suggestion: Keep the latest index independent of the display cap; clear only in `Clear`. Add a test.
- Source: general (Issue 1). Verified by orchestrator.
- Disposition notes:

### M2 — Severity: suggestion — Status: open
- File: features/inspector/inspector-state.toggle-pause.cs:32; inspector-log.cs SetPaused/FlushPending
- Description: Resume is two lock acquisitions; an ingest in between leaves `Entries` out of sequence order (fold/diff rely on order) and lets an older pending snapshot overwrite the latest index.
- Suggestion: Flush pending atomically inside `SetPaused(false)`.
- Source: general (Issue 2). Verified by orchestrator.
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: components/inspector/FoldedStateNode.razor:11; Inspector.razor DiffFor
- Description: Snapshot JSON re-parsed on every render / every ingested span.
- Suggestion: Cache parsed documents keyed by (state type, sequence).
- Source: general (Issue 3)
- Disposition notes:

### M4 — Severity: suggestion — Status: open
- File: features/inspector/state-diff.cs:42
- Description: When only the previous snapshot is truncated, Diff shows a truncation hint implying the selected snapshot is truncated and hides a parseable selected snapshot.
- Suggestion: Say which side is truncated.
- Source: general (Issue 4)
- Disposition notes:

### M5 — Severity: suggestion — Status: open
- File: components/inspector/StateDiffView.razor:7; inspector-state.cs
- Description: Nested diff branches start collapsed (so deep changes are hidden), diff and tree share expansion keys, `ExpandedNodes` never pruned.
- Suggestion: Default diff branches to open.
- Source: general (Issue 5)
- Disposition notes:

### M6 — Severity: nit — Status: open
- File: components/pages/Inspector.razor.css; inspector-log.cs TryGetPreviousSnapshot; theme toggle
- Description: Duplicate CSS rule blocks; `TryGetPreviousSnapshot` doesn't skip empty state type; theme label can disagree with `<html data-theme>` after circuit reconnect.
- Suggestion: Merge CSS; guard empty state type; re-sync theme.
- Source: general (Issue 6)
- Disposition notes:

### M7 — Severity: suggestion — Status: open
- File: tests/timewarp-state-telemetry-tests/initialized-state-snapshot-handler-tests.cs
- Description: No tests for truncation on `.Initialized` span or serialization-failure path; DI wiring only verified by inspecting generated code.
- Suggestion: Add truncation + throwing-serializer tests.
- Source: general-telemetry (Issue 1)
- Disposition notes:

### M8 — Severity: nit — Status: open
- File: source/timewarp-state-telemetry/state-snapshot-recorder.cs:62
- Description: `cache.Record` outside try; "Does not throw" doc slightly overstated.
- Source: general-telemetry (Issue 2)
- Disposition notes:

### M9 — Severity: nit — Status: open
- File: source/timewarp-state-telemetry/initialized-state-snapshot-handler.cs:85
- Description: Duplicated options/JsonTypeInfo skip logic vs recorder.
- Source: general-telemetry (Issue 3)
- Disposition notes:

### M10 — Severity: nit — Status: open
- File: source/timewarp-state-telemetry/initialized-state-snapshot-handler.cs:125
- Description: Sampled-out init activity does not prime the cache; undocumented.
- Source: general-telemetry (Issue 4)
- Disposition notes:

## Duplicates / conflicts

- None; the two reviewers had disjoint scopes.
