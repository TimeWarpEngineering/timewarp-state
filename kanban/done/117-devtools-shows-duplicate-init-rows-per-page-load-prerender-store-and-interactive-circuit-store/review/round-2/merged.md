# Round 2 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 3 | 0 |
| nit | 0 | 5 | 0 |

## Resolved prior (round 1)

M1 (bug), M2–M3, M5 (suggestion), M4, M6–M8 (nit): re-verified against code as **fixed** in 9548c05e.

## Issues

### M9 — Severity: nit — Status: fixed
- File: tests/timewarp-state-devtools-tests/store-selection-tests.cs
- Description: No test covers the rule that a store with no render tag counts as live; every test lets the newest non-prerender store win on recency alone.
- Suggestion: Older store with no render tag plus a newer prerender store; default selection must be the untagged-render store.
- Source: general
- Disposition notes: fixed — added `TreatAStoreWithNoRenderTagAsLive`; devtools tests 60/60 pass.

## Duplicates / conflicts

- None.
