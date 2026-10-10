# Review framework

## Budget (by-diff)

- Lines changed: 373
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 107

**Date:** 2026-10-11
**Host task:** kanban/to-do/107-h3-injected-services-break-cloning-opt-in-share-by-reference-attribute/
**Diff scope:** branch `task/107-...` vs `master` (commit 19815c99)
**Plan / brief:** `[CloneShared]` share-by-reference opt-in for injected services (see task.md Rule)
**Effort:** 2 (by-diff budget); roster axes: general
**Reviewer roster:** general
**Session IDs:** review oracle (Claude Opus 5.5); general reviewer subagent aeae3ae5348e3b8b8

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Prior rounds are immutable; new work goes in `round-(N+1)/`
