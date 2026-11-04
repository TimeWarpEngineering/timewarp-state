# Review framework

## Budget (by-diff)

- Lines changed: 10408
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Scope (round 1)

- **Date:** 2026-11-04
- **Host task:** kanban/in-progress/135-decompose-state-clone-plannercs-and-cache-symbol-lookups/
- **Diff scope:** branch `task/135-decompose-state-clone-plannercs-and-cache-symbol-l` (HEAD `1ac6afb4`) vs merge-base `5d0564eb`, excluding `kanban/`
- **Plan / brief:** task.md Requirements + `## Goal (implementer restatement)`
- **Effort:** 3. Three `general` passes split by scope: (a) catalog/symbols/subtypes/roots/collections; (b) emit/diagnostics/metadata/accessors/construction/objects/shapes/slots/small types; (c) boy-scout test/script/dev-cli splits
- **Reviewer roster:** general
- **Session IDs:** reviewer-claude (claude-opus-5-5) headless review oracle; three in-process general-purpose subagents

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit. Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
