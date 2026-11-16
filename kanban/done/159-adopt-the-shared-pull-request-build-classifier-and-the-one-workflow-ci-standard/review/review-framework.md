# Review framework

## Budget (by-diff)

- Lines changed: 219
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope — task 159

**Date:** 2026-11-17
**Host task:** kanban/in-progress/159-adopt-the-shared-pull-request-build-classifier-and-the-one-workflow-ci-standard/
**Diff scope:** branch `task/159-adopt-the-shared-pull-request-build-classifier-and` (HEAD `fed54a1d`) vs merge-base with `master` (`0aaa1f92`). Product commit `e2715169`. Product paths: `.github/workflows/workflow.yml`, `.timewarp/dev.jsonc`, `Directory.Packages.props`, `tools/dev-cli/endpoints/workflow-command.cs`. Kanban commits are out of the product review. `master` is one commit ahead (`c1d41bbe`, land kanban 159, `task.md` only).
**Plan / brief:** task.md Description, Requirements, and Notes, checked against `## Goal (implementer restatement)`. Adopt TimeWarp.Nuru.DevCli 4.0.0-beta.15 `PullRequestBuildClassifier`, drop the `pull_request` paths filter, keep the `push` filter with a why comment, and declare extra build roots through `PullRequestBuildConfig`.
**Effort:** 2 (by-diff budget), roster axis general only
**Reviewer roster:** general
**Session IDs:** review oracle, ganda task work, grok-4.7, session `01a1e574-a9eb-7561-9f39-5e44ab2cc7a8`. Turn cap 120.

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
