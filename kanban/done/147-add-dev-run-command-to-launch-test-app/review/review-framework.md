# Review framework

## Budget (by-diff)

- Lines changed: 266
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope — task 147

**Date:** 2026-11-07
**Host task:** kanban/in-progress/147-add-dev-run-command-to-launch-test-app/
**Diff scope:** branch `task/147-add-dev-run-command-to-launch-test-app` (HEAD f6c1efbe) vs merge-base with `master` b2317f33. Product commit a18952fd. Product paths: `tools/dev-cli/endpoints/run-command.cs`, `.gitignore`, `.vscode/settings.json` (untracked). Kanban move excluded from the product review.
**Plan / brief:** task.md `## Goal (implementer restatement)`: `./bin/dev run` starts `tests/test-app/test-app-server` (hosts `test-app-client`) with default launch profile `https`, `--profile` / `--http`, catch-all args when Nuru can pass them through, help plus build and audit clean, and `ganda repo audit --fix` committed.
**Effort:** 2 (by-diff budget), roster axis general only
**Reviewer roster:** general
**Session IDs:** review oracle, ganda task work, grok-4.7 (this session). Session id not exposed. Turn cap 120; cursor harness has no `--max-turns` flag (uncapped, bound 120).

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
