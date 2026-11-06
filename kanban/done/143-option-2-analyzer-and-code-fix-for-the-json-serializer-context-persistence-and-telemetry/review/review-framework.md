# Review framework

## Budget (by-diff)

- Lines changed: 209
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Framework — task 143

**Date:** 2026-11-06
**Host task:** kanban/in-progress/143-option-2-analyzer-and-code-fix-for-the-json-serializer-context-persistence-and-telemetry/
**Diff scope:** branch `task/143-option-2-analyzer-and-code-fix-for-the-json-serial` vs `master` (merge base), HEAD 862e06d5 (26 files, +1213/-41)
**Plan / brief:** task.md `## Goal (implementer restatement)`: TWS0013 analyzer, code fix in its own assembly packed at `analyzers/dotnet/cs`, tests, release tracking, `AddTimeWarpState` `TypeInfoResolver` guidance, no generator change
**Effort:** 3 (by-diff budget above)
**Reviewer roster:** general
**Session IDs:** review oracle claude-opus-5-5 (ganda task work, headless print mode)

### Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
