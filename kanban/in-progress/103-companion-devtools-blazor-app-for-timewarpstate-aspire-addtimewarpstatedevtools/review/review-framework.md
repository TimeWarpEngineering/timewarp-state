# Review framework

## Budget (by-diff)

- Lines changed: 2893
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

# Review framework — task 103

**Date:** 2026-10-11
**Host task:** kanban/to-do/103-companion-devtools-blazor-app-for-timewarpstate-aspire-addtimewarpstatedevtools/
**Diff scope:** branch `task/103-companion-devtools-blazor-app-for-timewarpstate-as` vs `master` (commit f7e837f7)
**Plan / brief:** task.md Requirements, Acceptance criteria, and Results (Design + Implementation)
**Effort:** 3 (by-diff budget)
**Reviewer roster:** general
**Session IDs:** review oracle (Claude Opus 5.5, headless ganda task work); general reviewer subagent

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
