# Review framework — task 109

**Date:** 2026-10-11
**Host task:** kanban/in-progress/109-fable-review-medium-findings-m1-m15-batched/
**Diff scope:** branch `task/109-fable-review-medium-findings-m1-m15-batched` vs `origin/master` (commit `0d0c7f87`, 78 files, ~1914 lines changed)
**Plan / brief:** Fable Medium findings M1-M15 (see task.md checklist); M1 closed with reason (task 104).
**Effort:** 3 (Budget.ByDiff: 1914 lines changed → effort 3; turn cap 200)
**Reviewer roster:** general
**Session IDs:** review oracle (Claude Opus 5.5, ganda task work); general reviewer subagent

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
