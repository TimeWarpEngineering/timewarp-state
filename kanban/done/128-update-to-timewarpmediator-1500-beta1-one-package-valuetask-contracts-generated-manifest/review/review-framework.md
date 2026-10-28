# Review framework

## Budget (by-diff)

- Lines changed: 141
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Scope — task 128

**Date:** 2026-10-28
**Host task:** kanban/in-progress/128-update-to-timewarpmediator-1500-beta1-one-package-valuetask-contracts-generated-manifest/
**Diff scope:** branch `task/128-update-to-timewarpmediator-1500-beta1-one-package` vs merge-base `184bc681` with `master` (HEAD `ae31fb0b`). Product commits `743c83ed` (beta.1 move) and `5a59c209` (beta.2 pin). Master is ahead only by kanban landings.
**Plan / brief:** move State from TimeWarp.Mediator 14.0.0-beta.5 (Contracts, Generators, Analyzers) to 15.0.0-beta.2 (`TimeWarp.Mediator` + `TimeWarp.Mediator.Generators`). Handlers and behaviors use `ValueTask` and `MessageHandlerDelegate`. The pipeline log reads `MediatorManifest` v2. See task.md `## Goal (implementer restatement)`.
**Effort:** 3 (by-diff budget), roster axis general only
**Reviewer roster:** general
**Session IDs:** review oracle, claude (claude-opus-5-5), ganda task work headless

## Round 2 scope

Fix-pass delta `ae31fb0b..96912260` (round-1 base to HEAD). Effort 1, roster general. The budget above is the round-2 by-diff ladder: 141 lines.

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
