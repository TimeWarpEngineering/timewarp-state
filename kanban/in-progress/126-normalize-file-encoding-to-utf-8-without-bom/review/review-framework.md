# Review framework

## Budget (by-diff)

- Lines changed: 252
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope

**Date:** 2026-10-24
**Host task:** kanban/in-progress/126-normalize-file-encoding-to-utf-8-without-bom/
**Diff scope:** branch `task/126-normalize-file-encoding-to-utf-8-without-bom` vs merge-base `3c4d0f5b` (commits `500780f2` BOM strip, `508cd512` audit fixes), product paths only
**Plan / brief:** task.md `## Goal (implementer restatement)` and `## Description`
**Effort:** 2 (budget by diff); roster axes: general
**Reviewer roster:** general (review oracle, inline)
**Session IDs:** review oracle, ganda task work headless (Claude Opus 5.5), session id not reported

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit. Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
