# Round 1 — merged findings
**Date:** 2026-10-10
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 2 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: kanban/to-do/102-full-codebase-review-by-claude-fable-review-only/review-findings.md:60
- Description: Two citations point past end of file. `store.redux-dev-tools.cs:274-349` (file has 119 lines; the seven suppressions are at 43–71). `i-store.cs:379,387` (file has 38 lines; `GetState(Type)` is 27, `GetSemaphore` is 29, `StateInitializationTasks` is 37). The underlying claims match the code.
- Suggestion: Cite the real lines.
- Source: general
- Disposition notes: Findings now cite `store.redux-dev-tools.cs:43-71` and `i-store.cs:27,29,37`.

### M2 — Severity: nit — Status: fixed
- File: kanban/to-do/102-full-codebase-review-by-claude-fable-review-only/review-findings.md:206
- Description: The nit says four `TODO` comments remain and that all of them are the M9 persistence no-ops. There are five. The extra one is `policies.action-policy.cs:15`.
- Suggestion: Count five and separate the policies note from the four persistence no-ops.
- Source: general
- Disposition notes: Nit now counts five TODOs and names `policies.action-policy.cs:15`.

### M3 — Severity: nit — Status: fixed
- File: kanban/to-do/102-full-codebase-review-by-claude-fable-review-only/review-findings.md:152
- Description: L1 cites `timewarp-state-options.cs:25` for `UseStateTransactionBehavior`. That line is `Assemblies`. The property is line 29.
- Suggestion: Cite line 29.
- Source: general
- Disposition notes: L1 now cites line 29.

## Duplicates / conflicts

- Single reviewer. Nothing to collapse.
