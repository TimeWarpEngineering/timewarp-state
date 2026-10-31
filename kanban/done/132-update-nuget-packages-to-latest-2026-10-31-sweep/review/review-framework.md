# Review framework

## Budget (by-diff)

- Lines changed: 161
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Scope — task 132

**Date:** 2026-10-31
**Host task:** kanban/in-progress/132-update-nuget-packages-to-latest-2026-10-31-sweep/
**Diff scope:** branch `task/132-update-nuget-packages-to-latest-2026-10-31-sweep` vs merge-base with `master` (commits 16c65a8a, e20927e2; kanban-only commits excluded). `master` is 2 commits ahead, kanban-only.
**Plan / brief:** central NuGet pin sweep (Aspire, OpenTelemetry, MSTest, Test.Sdk, Playwright.MSTest.v4, Nuru/Nuru.DevCli, TimeWarp.Architecture.Analyzers); Roslyn 4.14.0 hold; dev CLI drops `using static Unit`; new TWA rules turned off; beta.16 release notes.
**Effort:** 2 (budget by diff)
**Reviewer roster:** general (review oracle, Claude Opus 5.5)
**Session IDs:** review oracle under `ganda task work` (headless print mode; no vendor session id exposed)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
