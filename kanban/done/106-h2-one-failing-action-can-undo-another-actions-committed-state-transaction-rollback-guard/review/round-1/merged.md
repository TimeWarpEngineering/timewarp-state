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
- File: documentation/topics/cloning.md:12
- Description: The docs do not say that a failing action's pre-clone in-place writes carry into a concurrent action's clone.
- Suggestion: Name the limit in the Transaction rollback section.
- Source: general
- Disposition notes: Fixed. The Transaction rollback section in `cloning.md` now has a sentence that states the limit.

### M2 — Severity: nit — Status: wontfix
- File: source/timewarp-state/features/pipeline/state-transaction-behavior.cs:142
- Description: The skip warning says "concurrent action advanced" even when the cause was `RemoveState` or `Reset`.
- Suggestion: Reword the message.
- Source: general
- Disposition notes: Wontfix (orchestrator). The skip behavior is correct in both cases. The message and the 405 event name match the task's recorded decision and the tests, and a removal during an in-flight action is rare. A rename gives little benefit for the churn.

## Duplicates / conflicts

- None.
