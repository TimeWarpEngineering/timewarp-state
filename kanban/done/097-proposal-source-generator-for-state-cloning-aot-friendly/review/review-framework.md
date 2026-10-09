# Review framework

## Budget (by-diff)

- Lines changed: 3280
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

# Review framework — task 097

**Date:** 2026-10-10
**Host task:** kanban/to-do/097-proposal-source-generator-for-state-cloning-aot-friendly/
**Diff scope:** branch `task/097-proposal-source-generator-for-state-cloning-aot-fr` vs `master` (commit 633d58b5 product change)
**Plan / brief:** Incremental source generator (`StateCloneSourceGenerator` / `state-clone-planner.cs`) emits AOT-safe clones for `State<T>`; reflection `DeepCloner` deleted; TWSG002 build error; TWS001 redefined; `StateCloneRegistry` dispatch in `StateTransactionBehavior`. Design in task.md `## Results`.
**Effort:** 3 (by-diff budget)
**Reviewer roster:** general
**Session IDs:** review oracle (claude-opus-5-5, ganda task work review node)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
