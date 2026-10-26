# Review framework

## Budget (by-diff)

- Lines changed: 945
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Scope

**Date:** 2026-10-26
**Host task:** kanban/in-progress/129-shrink-the-local-feed-lane-to-one-packaged-sample-plus-a-nupkg-content-test/
**Diff scope:** branch `task/129-shrink-the-local-feed-lane-to-one-packaged-sample` vs merge-base with `master` (commits `00f3102f`, `9c7dda13`, plus kanban commits)
**Plan / brief:** task.md Requirements 1–7 and `## Goal (implementer restatement)`
**Effort:** 3 (by-diff budget); roster axes: general
**Reviewer roster:** general (review oracle, claude-opus-5-5, inline)
**Session IDs:** review oracle under `ganda task work` (session id not exposed to the oracle)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
