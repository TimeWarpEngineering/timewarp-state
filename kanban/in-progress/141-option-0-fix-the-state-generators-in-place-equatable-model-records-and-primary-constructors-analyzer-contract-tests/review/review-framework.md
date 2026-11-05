# Review framework

## Budget (by-diff)

- Lines changed: 2433
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

# Review framework — task 141

**Date:** 2026-11-06
**Host task:** kanban/in-progress/141-option-0-fix-the-state-generators-in-place-equatable-model-records-and-primary-constructors-analyzer-contract-tests/
**Diff scope:** branch `task/141-option-0-fix-the-state-generators-in-place-equatab` vs merge-base with `master` (commits 8722115e..6ab9405d; 26 files, +2100/-333)
**Plan / brief:** task.md `## Requirements` and `## Goal (implementer restatement)`: fix the four State generators in place (equatable models, records and primary constructors, TWSG003, incrementality tests), analyzer-contract table, Ramani inventory, non-breaking, version bump.
**Effort:** 3 (by-diff budget), roster axes: general
**Reviewer roster:** general (review oracle, claude-opus-5-5)
**Session IDs:** review oracle session for the ganda task work review node (2026-11-06)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
