# Review framework

## Budget (by-diff)

- Lines changed: 2005
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Scope

**Date:** 2026-10-16
**Host task:** kanban/to-do/121-export-blazor-webassembly-store-telemetry-to-the-state-devtools-inspector/
**Diff scope:** branch `task/121-export-blazor-webassembly-store-telemetry-to-the-s` vs `master` (`git diff master...HEAD`, HEAD d302b2d0)
**Plan / brief:** task.md `## Goal (implementer restatement)` plus the original Requirements and Proof gates
**Effort:** 3, general axis only
**Reviewer roster:** general (review oracle, Claude Opus 5.5)
**Session IDs:** review oracle session not exposed to the oracle

## Ground rules

- Reviewers are read-only on product code. They write only under `review/round-N/`.
- Severity: bug | suggestion | nit. Status starts as open.
- Do not invent issues to fill space. Zero issues is a valid outcome.
- Review the diff and the call sites around it. Re-check falsifiable claims against the repo.
- Prior rounds are immutable. New work goes in `round-(N+1)/`.
