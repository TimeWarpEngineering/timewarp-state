# Review framework

## Budget (by-diff)

- Lines changed: 1126
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

# Review framework — task 117

**Date:** 2026-10-12
**Host task:** kanban/to-do/117-devtools-shows-duplicate-init-rows-per-page-load-prerender-store-and-interactive-circuit-store/
**Diff scope:** branch `task/117-devtools-shows-duplicate-init-rows-per-page-load-p` vs `origin/master` (commits 225ce776, 646c6e1e)
**Plan / brief:** task.md Requirements — per-store id + render tag on spans, `StoreId` on `ActionLogEntry`, store badge, per-store State fold with selector, filter by store, docs.
**Effort:** 3
**Reviewer roster:** general
**Session IDs:** review oracle Claude Opus 5.5 (ganda task work, headless)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
