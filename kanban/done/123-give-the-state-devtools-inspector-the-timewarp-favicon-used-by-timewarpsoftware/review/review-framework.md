# Review framework

## Budget (by-diff)

- Lines changed: 153
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Scope

**Date:** 2026-10-17
**Host task:** kanban/to-do/123-give-the-state-devtools-inspector-the-timewarp-favicon-used-by-timewarpsoftware/
**Diff scope:** branch `task/123-give-the-state-devtools-inspector-the-timewarp-fav`, commit 04ead89d (product) and 75705f36 (kanban), vs `master` (eba6720b)
**Plan / brief:** `## Goal (implementer restatement)` and `## Requirements` in task.md. Link `assets/logo.png` into the inspector's static web assets, add a PNG icon link to `App.razor`, ship it in `TimeWarp.State.DevTools.App` `tools/`, and add tests.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle, claude-opus-5-5 (headless `ganda task work`), session id not exposed

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
