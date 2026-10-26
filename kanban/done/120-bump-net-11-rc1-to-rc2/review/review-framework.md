# Review framework

## Budget (by-diff)

- Lines changed: 106
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Scope

**Date:** 2026-10-26
**Host task:** kanban/in-progress/120-bump-net-11-rc1-to-rc2/
**Diff scope:** branch `task/120-bump-net-11-rc1-to-rc2` vs merge-base with `master` (product commit `ff6efd04`: `global.json`, `Directory.Packages.props`, `claude.md`, `.gitignore`, `tests/timewarp-state-analyzer-tests/analyzer-test-factory.cs`)
**Plan / brief:** task.md `## Goal (implementer restatement)` + `## Requirements`
**Effort:** 1 (general only)
**Reviewer roster:** general (review oracle, claude-opus-5-5, ganda task work headless)
**Session IDs:** not reported by host

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
