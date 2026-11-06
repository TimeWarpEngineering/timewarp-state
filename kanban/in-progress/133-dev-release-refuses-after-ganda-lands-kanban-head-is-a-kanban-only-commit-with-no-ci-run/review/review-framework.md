# Review framework — task 133

**Date:** 2026-11-06
**Host task:** kanban/in-progress/133-dev-release-refuses-after-ganda-lands-kanban-head-is-a-kanban-only-commit-with-no-ci-run/
**Diff scope:** branch `task/133-dev-release-refuses-after-ganda-lands-kanban-head` vs merge-base with master (`e592c3a820234b4fc4d1e37d6ad1282284bed3ca`). Product paths: `tools/dev-cli/endpoints/workflow/artifact-source-walk.cs`, `ci-artifact-service.cs`, `release-pipeline.cs`, `tests/timewarp-state-tests/release-pipeline/artifact-source-walk-tests.cs`, `tests/timewarp-state-tests/timewarp-state-tests.csproj`, `tests/timewarp-state-tests/global-usings.cs`, `documentation/dev-ops/releasing.md`, `documentation/dev-ops/dev-ops.md`, `documentation/dev-ops/toc.yml`.
**Plan / brief:** Option 1 from the task. Accept an ancestor `workflow.yml` run when every commit after it changes only `kanban/`. `dev release` already does that walk in TimeWarp.Nuru.DevCli 4.0.0-beta.11. This change adds the same walk to pipeline locate-run, with a fake run list, and a releasing guide.
**Effort:** 2 (general only)
**Reviewer roster:** general
**Session IDs:** implementer session `a37b4184-9519-4560-85a4-015fc2ec9150` (task.md). This review walk has no separate vendor session id.

## Budget (by-diff)

- Lines changed: 686
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
