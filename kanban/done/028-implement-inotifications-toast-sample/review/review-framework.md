# Review framework

## Budget (by-diff)

- Lines changed: 349
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 028

**Date:** 2026-10-10
**Host task:** kanban/to-do/028-implement-inotifications-toast-sample/
**Diff scope:** branch `task/028-implement-inotifications-toast-sample` vs `origin/master` (`samples/08-notifications/readme.md` + kanban task files)
**Plan / brief:** README-only pointer to TimeWarp Architecture's `NotificationState` (scope reduced 2026-10-10)
**Effort:** 2 (by-diff budget); roster axes: general
**Reviewer roster:** general
**Session IDs:** review oracle — Claude Opus 5.5 (ganda task work review node, 2026-10-10)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
