# Review framework

## Budget (by-diff)

- Lines changed: 335
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 108

**Date:** 2026-10-11
**Host task:** kanban/to-do/108-h4-changed-collection-parameters-can-fail-to-re-render-complex-parameter-comparison/
**Diff scope:** branch task/108-h4-changed-collection-parameters-can-fail-to-re-re vs origin/master (commit dcf5b933)
**Plan / brief:** compare collection parameters element-wise on snapshots (never enumerate IQueryable); route value types through Equals; unit tests
**Effort:** 2 (budget by diff); roster axes: general
**Reviewer roster:** general
**Session IDs:** review oracle, Claude Opus 5.5 (ganda task work review node)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
