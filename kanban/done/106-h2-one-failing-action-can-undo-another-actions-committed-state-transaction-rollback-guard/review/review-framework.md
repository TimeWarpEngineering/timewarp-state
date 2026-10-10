# Review framework

## Budget (by-diff)

- Lines changed: 393
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 106

**Date:** 2026-10-11
**Host task:** kanban/to-do/106-h2-one-failing-action-can-undo-another-actions-committed-state-transaction-rollback-guard/
**Diff scope:** branch task/106-h2-one-failing-action-can-undo-another-actions-com vs `119a1dbd` (origin master merge of PR #630)
**Plan / brief:** H2 from task 102: a `ReferenceEquals` rollback guard in `StateTransactionBehavior`, event id 405, three interleaved-action tests, and docs
**Effort:** 2
**Reviewer roster:** general
**Session IDs:** review oracle, Claude Opus 5.5 (ganda task work, headless)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
