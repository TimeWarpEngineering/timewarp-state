# Round 2 — merged findings
**Date:** 2026-10-12
**Sources:** general (orchestrator re-review of 656b438a)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 4 | 1 |
| nit | 0 | 3 | 1 |

## Issues (carried from round 1)

- **M1** bug — fixed (656b438a): latest-per-state index survives trim; test `KeepAnIdleStatesSnapshotAfterItsEntryIsTrimmed`.
- **M2** suggestion — fixed: resume flush is atomic inside `SetPaused(false)`; `FlushPending` removed.
- **M3** suggestion — wontfix: re-parse per render is bounded (500-entry cap, 64 Ki snapshot cap, local dev tool); caching parsed documents adds invalidation complexity not justified now. Decided by: review orchestrator.
- **M4** suggestion — fixed: hint names previous / selected / both truncated.
- **M5** suggestion — fixed: diff nodes use `diff:` keys and default open; tree and diff no longer share expansion.
- **M6** nit — fixed: CSS duplicates merged, empty state type guarded, theme re-applied on first render.
- **M7** suggestion — fixed: init-span truncation and serialization-failure tests added.
- **M8** nit — fixed: cache record inside the recorder guard.
- **M9** nit — wontfix: handler pre-check intentionally avoids starting an empty span; the duplication is two short lines. Decided by: review orchestrator.
- **M10** nit — fixed: Design region documents sampled-out init behaviour.

## New issues

None.
