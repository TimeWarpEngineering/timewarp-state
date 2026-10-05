# Review framework

## Budget (by-diff)

- Lines changed: 634
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 096

**Date:** 2026-10-05
**Host task:** kanban/to-do/096-jsonrequesthandler-resolves-any-type-name-allow-list-dispatchable-actions/
**Diff scope:** branch task/096-jsonrequesthandler-resolves-any-type-name-allow-li vs master (commit 4002e6f5)
**Plan / brief:** task.md Requirements 1–6 — allow-list JS-dispatchable actions via `AddJavaScriptDispatch`, fail closed, gate Redux DevTools, tests, beta.8 release notes
**Effort:** 2 (by-diff budget)
**Reviewer roster:** general
**Session IDs:** review oracle (Claude Opus 5.5, headless ganda task work); general reviewer subagent

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
