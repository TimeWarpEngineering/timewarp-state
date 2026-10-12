# Review framework

## Budget (by-diff)

- Lines changed: 81
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

# Review framework — task 115

**Date:** 2026-10-12
**Host task:** kanban/to-do/115-devtools-action-tab-long-attribute-keys-overflow-the-fixed-7rem-label-column-and-overwrite-values/
**Diff scope:** branch task/115-… vs origin/master (commit 98cda6ab)
**Plan / brief:** content-sized `.facts` label column (7rem floor, 16rem cap via `dt` max-width), mono attribute keys, wrapping keys in State/Diff views, ingest test with a long attribute key
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle (claude, ganda task work)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
