# Review framework — task 148

**Date:** 2026-11-07
**Host task:** kanban/in-progress/148-clear-build-warnings-surfaced-by-dev-run-and-turn-on-treatwarningsaserrors/
**Diff scope:** branch `task/148-clear-build-warnings-surfaced-by-dev-run-and-turn` vs merge-base `ffbd2efd` (master). Product commit `71624be0`. 61 files, +355/−248, including the kanban move.
**Plan / brief:** Clear every warning `dev run` surfaces in the test-app graph (NU1510, ASPDEPR011, TW0007, RS0030, BL0010/BL0016, CS8600/CS8602), then set `TreatWarningsAsErrors` repo-wide. Goal restatement in `task.md` matches the original requirements.
**Effort:** 2
**Reviewer roster:** general
**Session IDs:** review oracle, ganda task work, print mode (2026-11-07). Implementer session `84d5126b-fca2-4c3c-86a5-c4abf41cf9ab`.

## Budget (by-diff)

- Lines changed: 603
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
