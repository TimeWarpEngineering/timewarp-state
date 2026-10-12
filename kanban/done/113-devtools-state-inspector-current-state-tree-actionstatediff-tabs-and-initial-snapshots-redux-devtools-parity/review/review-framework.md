# Review framework

## Budget (by-diff)

- Lines changed: 3413
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

# Review framework — task 113

**Date:** 2026-10-12
**Host task:** kanban/to-do/113-devtools-state-inspector-current-state-tree-actionstatediff-tabs-and-initial-snapshots-redux-devtools-parity/
**Diff scope:** branch `task/113-devtools-state-inspector-current-state-tree-action` vs `origin/master` (implementation commit 0fa2c164)
**Plan / brief:** task.md Requirements — DevTools state tree, Action/State/Diff tabs, init snapshots in telemetry, TimeWarp theme
**Effort:** 3
**Reviewer roster:** general (two independent general passes: `general` on DevTools UI + inspector logic, `general-telemetry` on telemetry + ingest + tests)
**Session IDs:** review oracle Claude Opus 5.5 (ganda task work, headless)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
