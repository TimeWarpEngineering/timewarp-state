# Review framework

## Budget (by-diff)

- Lines changed: 83
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Scope

**Date:** 2026-10-26
**Host task:** kanban/in-progress/130-bump-net-11-rc2-to-rc3/
**Diff scope:** branch `task/130-bump-net-11-rc2-to-rc3` vs `master` (merge base)
**Plan / brief:** Pin the .NET 11 RC3 SDK and packages (`11.0.100-rc.3.26522.148` / `11.0.0-rc.3.26522.148`) in place of RC2. See the task.md Goal (implementer restatement).
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle (claude, claude-opus-5-5), session id not exposed in print mode

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
