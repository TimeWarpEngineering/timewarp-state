# Round 1 — merged findings
**Date:** 2026-09-04
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: msbuild/repository.props:17
- Description: Product version is duplicated as `<TimeWarpStateVersion>12.0.0-beta.3</TimeWarpStateVersion>` (CPM pins for `TimeWarp.State` / `TimeWarp.State.Plus`) and a literal `<Version>12.0.0-beta.3</Version>` in `source/Directory.Build.props` (pack + `workflow.yml` `//Version` extract + DevCli `check-version`). They match today, and the audit requires a literal `source/` `<Version>`, but nothing asserts they stay aligned. Drift would pack/tag one version while samples restore another from `$(TimeWarpStateVersion)`.
- Suggestion: Add a cheap release/workflow gate (or DevCli step) that fails when `TimeWarpStateVersion` ≠ `source/Directory.Build.props` `<Version>`, mirroring mediator’s SSOT assert.
- Source: general
- Disposition notes: Added AssertVersionSsot in `tools/dev-cli/endpoints/workflow-command.cs` (PR and release modes) and matching pwsh asserts in `workflow.yml` ci job + release `extract_version`. Dual literals remain (audit requires a readable source `<Version>`); the gates fail on drift.

## Duplicates / conflicts

- None. Single reviewer.
