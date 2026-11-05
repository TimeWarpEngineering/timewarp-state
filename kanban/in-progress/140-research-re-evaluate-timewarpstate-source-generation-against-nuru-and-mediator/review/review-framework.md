# Review framework

## Budget (by-diff)

- Lines changed: 184
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Scope

- **Date:** 2026-11-06
- **Host task:** kanban/in-progress/140-research-re-evaluate-timewarpstate-source-generation-against-nuru-and-mediator/
- **Diff scope:** branch `task/140-research-re-evaluate-timewarpstate-source-generati` vs `master` (`git diff master...HEAD`). The diff covers kitchen files only: `report/analysis.md`, `report/spike/*`, and `task.md`. No product code is in it.
- **Plan / brief:** research report comparing State's four generators with Nuru and Mediator, with options 0-5 for Steve to choose from. See `## Goal (implementer restatement)` in task.md.
- **Effort:** 2 (by-diff budget). **Roster:** general.
- **Session IDs:** review oracle (claude, claude-opus-5-5) under ganda task work, 2026-11-06.

## Ground rules

- Reviewers are read-only on product code. They write only under `review/round-N/`.
- Severity is bug, suggestion or nit. Status starts as open.
- Do not invent issues to fill space. Zero issues is a valid outcome.
- Re-check falsifiable claims (citations, spike output) against the repositories.
- Prior rounds are immutable. New work goes in `round-(N+1)/`.
