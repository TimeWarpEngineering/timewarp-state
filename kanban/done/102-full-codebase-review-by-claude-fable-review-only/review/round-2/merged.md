# Round 2 — merged findings
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
- Description: Two citations pointed past end of file (`store.redux-dev-tools.cs:274-349`, `i-store.cs:379,387`).
- Suggestion: Cite the real lines.
- Source: general
- Disposition notes: Re-checked. Findings cite `store.redux-dev-tools.cs:43-71` and `i-store.cs:27`, `:29`, and `:37`.

### M2 — Severity: nit — Status: fixed
- File: kanban/to-do/102-full-codebase-review-by-claude-fable-review-only/review-findings.md:206
- Description: TODO count said four, all persistence no-ops. A fifth TODO is the policies file-layout note.
- Suggestion: Count five and name the policies note.
- Source: general
- Disposition notes: Re-checked. The nit counts five and cites `policies.action-policy.cs:15`.

### M3 — Severity: nit — Status: fixed
- File: kanban/to-do/102-full-codebase-review-by-claude-fable-review-only/review-findings.md:152
- Description: L1 cited `timewarp-state-options.cs:25` for `UseStateTransactionBehavior`. The property is line 29.
- Suggestion: Cite line 29.
- Source: general
- Disposition notes: Re-checked. L1 cites line 29, which declares `UseStateTransactionBehavior`.

## Duplicates / conflicts

- No new findings. M1–M3 carried forward from round 1 as fixed.
