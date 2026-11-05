# Review framework — task 138

**Date:** 2026-11-05
**Host task:** kanban/in-progress/138-split-devtools-inspectorrazor-into-components-and-move-logic-out-of-handlers/
**Diff scope:** branch `task/138-split-devtools-inspectorrazor-into-components-and` vs `master` (product commit `eae15f97`; 32 files, +1587 / −846)
**Plan / brief:** Split DevTools `Inspector.razor` into components, move handler logic into actions, and keep the page showing the same thing. See `task.md` `## Goal (implementer restatement)` and `## Requirements`.
**Effort:** 3
**Reviewer roster:** general
**Session IDs:** review oracle (grok, model grok-4.7), 2026-11-05. No vendor session id on this launch.

## Budget (by-diff)

- Lines changed: 2433
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
