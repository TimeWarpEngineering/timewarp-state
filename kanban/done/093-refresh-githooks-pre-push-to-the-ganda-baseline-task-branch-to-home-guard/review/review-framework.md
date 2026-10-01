# Review framework

## Budget (by-diff)

- Lines changed: 72
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

# Review framework — task 093

**Date:** 2026-10-01
**Host task:** kanban/to-do/093-refresh-githooks-pre-push-to-the-ganda-baseline-task-branch-to-home-guard/
**Diff scope:** branch task/093 vs origin/master (commit 35090b52: `.githooks/pre-push.cs`, task.md)
**Plan / brief:** refresh pre-push hook to ganda baseline via `ganda repo audit --fix --checks memsearch-scaffold`
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle (claude, headless ganda task work)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Prior rounds are immutable; new work goes in `round-(N+1)/`
