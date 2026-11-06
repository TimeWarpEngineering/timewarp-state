# Round 1 — general
**Date:** 2026-11-06
**Scope reviewed:** branch `task/133-dev-release-refuses-after-ganda-lands-kanban-head` vs merge-base `e592c3a820234b4fc4d1e37d6ad1282284bed3ca`. Product diff under `tools/dev-cli/endpoints/workflow/`, `tests/timewarp-state-tests/release-pipeline/`, and `documentation/dev-ops/`. Checked against `## Goal (implementer restatement)`, the original task, and TimeWarp.Nuru.DevCli 4.0.0-beta.11 (`release-command.cs`, `release-guard.cs`, `ci-run-promotion.cs`).

## Summary

Pipeline locate-run walks first-parent history the same way pinned `dev release` does: the first successful `workflow.yml` run is the artifact source, and the first path outside `kanban/` ends the walk. Download, verify, and the no-run refusal name that commit. Seven Fixie cases drive the walk with a fake run list and fake paths. Re-running `ArtifactSourceWalkTests*` reported 7 passed. The new guide at `documentation/dev-ops/releasing.md` states the rule, including that the tag goes on the release target. No findings.

## Issues

None.
