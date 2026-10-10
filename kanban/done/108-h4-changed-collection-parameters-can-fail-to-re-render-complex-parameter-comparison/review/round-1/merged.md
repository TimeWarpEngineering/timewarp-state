# Round 1 — merged findings
**Date:** 2026-10-11
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 1 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-blazor/components/timewarp-state-component.check-complex-parameter-changed.cs:217
- Description: Every parameter set copies both collections. The old check was O(1) for `ICollection`.
- Suggestion: Return early when the `ICollection` counts differ, and add a test.
- Source: general
- Disposition notes: Fixed by the review oracle. Added the `ICollection` count short-circuit and the test `DifferentCount_Rerenders`. Tests: 111 passed, 1 skipped.

### M2 — Severity: nit — Status: wontfix
- File: tests/timewarp-state-tests/timewarp-state-component/parameter-change-tests.cs:107
- Description: The leftover "Null value change" detail assertion reads oddly.
- Suggestion: None.
- Source: general
- Disposition notes: wontfix. The assertion is how the test proves the detail was not rewritten. Decided by the orchestrator.

## Duplicates / conflicts

- None (single reviewer).
