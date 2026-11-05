# Review framework

## Budget (by-diff)

- Lines changed: 749
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Round 2 — task 141

**Date:** 2026-11-06
**Host task:** kanban/in-progress/141-option-0-fix-the-state-generators-in-place-equatable-model-records-and-primary-constructors-analyzer-contract-tests/
**Diff scope:** the fix delta `6ab9405d..8c4f08d9` (fix pass 1 for round-1 M1, M2, M3). Prior IDs are re-verified on
the post-fix tree.
**Plan / brief:** `review/round-1/merged.md`, plus task.md `## Requirements` and `## Goal (implementer restatement)`.
**Effort:** 2 (by-diff, 749 lines). **Reviewer roster:** general (review-2 oracle, claude-opus-5-5).
**Session IDs:** review-2 oracle for the ganda task work review node (2026-11-06).

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
