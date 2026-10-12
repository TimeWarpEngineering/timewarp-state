# Review framework

## Budget (by-diff)

- Lines changed: 35
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

# Review framework — task 114

**Date:** 2026-10-12
**Host task:** kanban/to-do/114-remove-enabletrimanalyzer-from-the-blazor-server-telemetry-sample-il2026-and-il2111-noise/
**Diff scope:** branch `task/114-remove-enabletrimanalyzer-from-the-blazor-server-t` vs `master`
**Plan / brief:** Drop `EnableTrimAnalyzer` from `sample-04-server.csproj`, add an explanatory comment, no `NoWarn` suppression.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** Claude review oracle (ganda task work, headless)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
