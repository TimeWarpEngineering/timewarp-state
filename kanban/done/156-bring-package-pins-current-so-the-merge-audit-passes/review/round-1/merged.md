# Round 1 — merged findings
**Date:** 2026-11-16
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: readme.md:26
- Description: The readme packed into `TimeWarp.State` (`source/timewarp-state/timewarp-state.csproj` includes `$(RepositoryRoot)readme.md`) says hosts consume TimeWarp.Mediator 15.0.0-beta.5. The packed `TimeWarp.State.12.0.0-beta.18.nupkg` depends on Mediator 15.0.0-beta.9. The same current-version sentence remains in `claude.md:80`, `claude.md:170`, `documentation/overview.md:44`, and `documentation/partials/summary.md:10`. "Or later" feature sentences and historical release notes stay accurate.
- Suggestion: Name 15.0.0-beta.9 in those five current-version sentences so the packed readme matches the nuspec.
- Source: general
- Disposition notes: fixed 2026-11-16 (implement-fix-2). readme.md:26, claude.md:80, claude.md:170, documentation/overview.md:44 and documentation/partials/summary.md:10 now say 15.0.0-beta.9.

## Duplicates / conflicts

- Single reviewer. No overlap.
