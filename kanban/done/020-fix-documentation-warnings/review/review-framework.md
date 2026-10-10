# Review framework

## Budget (by-diff)

- Lines changed: 373
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 020

**Date:** 2026-10-10
**Host task:** kanban/to-do/020-fix-documentation-warnings/
**Diff scope:** branch task/020-fix-documentation-warnings vs master (commit bf1f3c25)
**Plan / brief:** clear DocFX warnings (renamed paths, sample links, TOC hrefs, migration xrefs); see task.md Results
**Effort:** 2 (by-diff budget)
**Reviewer roster:** general
**Session IDs:** review oracle claude-opus-5-5 (2026-10-10); general reviewer subagent a9d878d886d845b42

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
