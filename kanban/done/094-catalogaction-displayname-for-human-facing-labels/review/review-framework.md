# Review framework

## Budget (by-diff)

- Lines changed: 293
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 094

**Date:** 2026-10-01
**Host task:** kanban/to-do/094-catalogaction-displayname-for-human-facing-labels/
**Diff scope:** branch task/094-catalogaction-displayname-for-human-facing-labels vs master (commit 7e4608b1)
**Plan / brief:** task.md — optional `CatalogAction.DisplayName`, `ActionCatalogEntry.DisplayName`, generator copy, TWS0008, tests, 12.0.0-beta.7 bump
**Effort:** 2 (by-diff budget)
**Reviewer roster:** general
**Session IDs:** review oracle (ganda task work, Claude Opus 5.5), 2026-10-01

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
