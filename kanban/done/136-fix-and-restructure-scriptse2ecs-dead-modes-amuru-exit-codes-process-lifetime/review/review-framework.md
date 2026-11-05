# Review framework

## Budget (by-diff)

- Lines changed: 1034
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Scope (round 1)

- **Date:** 2026-11-05
- **Host task:** kanban/in-progress/136-fix-and-restructure-scriptse2ecs-dead-modes-amuru-exit-codes-process-lifetime/
- **Diff scope:** branch `task/136-fix-and-restructure-scriptse2ecs-dead-modes-amuru` (HEAD `de06d27a`) vs merge-base `770fa6d7`. Product diff is `scripts/e2e.cs`, `scripts/e2e-build.cs`, `scripts/e2e-dev-certs.cs`, `scripts/e2e-sut.cs` (deleted), `scripts/e2e-sut-host.cs`, `scripts/e2e-tests.cs`, and `scripts/global-usings.cs`. Kanban kitchen edits are out of the product review.
- **Plan / brief:** task.md Requirements, cleanups, and `## Goal (implementer restatement)` acceptance 1–9
- **Effort:** 3. Roster is general only, so this was one in-session pass over the e2e runfile. The diff is one script, not separate specialist surfaces.
- **Reviewer roster:** general
- **Session IDs:** reviewer (cursor, grok 4.7), headless review oracle, session not reported

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit. Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
