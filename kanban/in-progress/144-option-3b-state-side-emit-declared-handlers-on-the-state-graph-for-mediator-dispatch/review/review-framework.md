# Review framework

## Budget (by-diff)

- Lines changed: 1288
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

# Review framework — task 144

**Date:** 2026-11-06
**Host task:** kanban/in-progress/144-option-3b-state-side-emit-declared-handlers-on-the-state-graph-for-mediator-dispatch/
**Diff scope:** branch `task/144-option-3b-state-side-emit-declared-handlers-on-the` vs `master` (merge base), commit 896c2565 plus kanban commits
**Plan / brief:** task.md `## Goal (implementer restatement)`: `[Reducer]` (derives from Mediator `GeneratedHandlerAttribute`), reducer-handler emitter on the Option 1 State graph, TWS0014 analyzer from shared `facts/reducer-facts.cs`, end-to-end test in the test app, Mediator pin 15.0.0-beta.5
**Effort:** 3 (budget by diff, 1288 lines), roster axes: general
**Reviewer roster:** general (review oracle, read-only)
**Session IDs:** review oracle claude-opus-5-5 session f0bc6949-86e9-491b-b3c0-f2827586af95

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
