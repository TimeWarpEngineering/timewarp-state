# Review framework — task 137

**Date:** 2026-11-05
**Host task:** kanban/in-progress/137-fix-cancellation-exit-code-handling-and-structure-in-dev-cli-workflow-commandcs/
**Diff scope:** branch `task/137-fix-cancellation-exit-code-handling-and-structure` vs merge-base `4e09ae4c` (master). Product hunk is `tools/dev-cli/endpoints/workflow-command.cs`, the new `tools/dev-cli/endpoints/workflow/*.cs` files, deletion of `workflow-command.release.cs`, and `.github/workflows/workflow.yml`.
**Plan / brief:** Make `dev workflow` honest about failure and cancellation, and split the workflow handler into `ReleasePipeline`, `ReleaseGitGate`, `CiArtifactService`, `NuGetPushService`, and `RepoVersionService`.
**Effort:** 3 (by-diff, 1619 lines)
**Reviewer roster:** general
**Session IDs:** not exposed (cursor, model grok-4.7, turn cap 200)

## Budget (by-diff)

- Lines changed: 1619
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
