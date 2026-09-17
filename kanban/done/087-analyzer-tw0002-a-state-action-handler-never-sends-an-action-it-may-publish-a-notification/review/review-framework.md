# Review framework — task 087

**Date:** 2026-09-17
**Host task:** kanban/in-progress/087-analyzer-tw0002-a-state-action-handler-never-sends-an-action-it-may-publish-a-notification/
**Diff scope:** branch `task/087-analyzer-tw0002-a-state-action-handler-never-sends` vs `origin/master` (implement commit `d0e8f692` — feat(analyzer): add TW0002 handler-must-not-send-action and TW0003 escape hatch)
**Plan / brief:** `task.md` — Roslyn analyzer TW0002 (`HandlerMustNotSendAction`, Design, Warning) so state action handlers must not dispatch actions; they may publish notifications. Escape hatch `[AllowActionSend("reason")]` emits Info TW0003. Tests follow the TW0001 matrix; docs include analyzer README, `documentation/topics/analyzers.md`, and a “State is a boundary” overview paragraph.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok review oracle `01a0afba-68c9-7c22-8688-aa83a82e7e39` (2026-09-17)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
