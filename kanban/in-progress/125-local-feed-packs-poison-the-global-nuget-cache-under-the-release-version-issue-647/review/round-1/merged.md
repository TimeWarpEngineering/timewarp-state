# Round 1 — merged findings
**Date:** 2026-10-26
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 1 |

## Issues

### M1 — Severity: nit — Status: wontfix
- File: scripts/overview.md:9
- Description: The docs do not say that the isolated `artifacts/nuget-packages` belongs to one worktree, starts cold, and is deleted by `clean`. So every new task worktree, and every clean, means a full re-download from nuget.org.
- Suggestion: Add one sentence on that cost to the "Local NuGet cache" section.
- Source: general
- Disposition notes: wontfix, decided by the review oracle (orchestrator). This is a documentation-only nit, and the cost is inherent to the isolation approach the task explicitly allows. Behaviour is correct, and the mechanism and the guard are already documented. A fix pass plus another review round for a single sentence costs more than the gap. It can go into a later docs touch.

## Duplicates / conflicts

- None (single reviewer).
