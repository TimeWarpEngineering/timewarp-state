# Review framework — task 100

**Date:** 2026-10-09
**Host task:** kanban/to-do/100-fix-bugs-found-during-099-region-backfill/
**Diff scope:** branch task/100-fix-bugs-found-during-099-region-backfill vs origin/master (47eeaeed)
**Plan / brief:** task.md, items 1–5 (cacheable state base type and analyzer, InvalidCloneException causes, StartHandler EventIds, FeatureFlagState removal, test-app defects)
**Effort:** 3 (from Budget.ByDiff)
**Reviewer roster:** general
**Session IDs:** review oracle (claude opus 5.5), general reviewer subagent aefe703e4239512ea

## Budget (by-diff)

- Lines changed: 983
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit. Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
