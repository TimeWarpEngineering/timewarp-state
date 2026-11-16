# Round 1 — general
**Date:** 2026-11-11
**Scope reviewed:** branch vs master: `.editorconfig` `[ganda.audit]` comment, `.github/workflows/workflow.yml` new `coverage` step; task.md Results claims re-verified against the repo and a fresh `ganda repo audit`.

## Summary

The change rewrites the `kebab-path-names` comment in `.editorconfig` and adds an `actions/upload-artifact` step for `artifacts/coverage` to CI. The kebab, backlog-110, window-icon, and kanban-landed handling follows the task: no rename, no check disabled, nothing hand-renamed, `.vscode/settings.json` left ignored. All of these are disclosed in Results. A fresh `ganda repo audit` agrees: `ci-code-coverage` and `vscode-window-icon` pass, and `kebab-path-names` warns on the same seven paths. (`kanban-landed` currently fails only on this task's own unlanded kitchen, which the host lands.) The coverage step is the problem. Nothing in the repo writes to `artifacts/coverage`, so the step uploads nothing on every run. Its only effect is to make `ci-code-coverage` pass.

## Issues

### Issue 1 — Severity: bug
- File: .github/workflows/workflow.yml:120
- Description: The `coverage` step uploads `artifacts/coverage`, but no command in the repo writes there. A repo-wide search outside `artifacts/` and `kanban/` finds no `--collect`, `XPlat Code Coverage`, `coverlet` settings, `--coverage`, or `--results-directory`. `scripts/e2e-tests.cs:14-20` runs `dotnet test --no-build --settings:... ./test-app-end-to-end-tests.csproj`, and `scripts/test.cs` (Fixie) passes no coverage flags either. `coverlet.collector` is referenced (`tests/test-app-end-to-end-tests/test-app-end-to-end-tests.csproj:10`) but never turned on. With `if-no-files-found: warn`, every CI run uploads nothing and logs a warning. The audit check silenced is **`ci-code-coverage`** ("CI collects code coverage but does not upload it"). After this change it passes, but no coverage is collected or uploaded. This is a change whose only effect is to make an audit pass without doing the work it measures, so the bar classes it as a bug. Results discloses that the directory may be empty, which is honest. It is still a satisfy-the-check step.
- Suggestion: Pick one, on this task:
  (a) Collect coverage for real. Add `--collect:"XPlat Code Coverage"` and `--results-directory` (pointing at `artifacts/coverage`, resolved from the repo root) to the `dotnet test` arguments in `scripts/e2e-tests.cs`, and optionally do the same for the unit-test runner. Then the upload step carries a Cobertura file, and `if-no-files-found` can become `error` on merge/PR runs so a lost artifact fails loudly, which is what the commit subject says it wants.
  (b) If coverage is not wanted, remove the unused `coverlet.collector` reference (csproj + `Directory.Packages.props`) so the check stops seeing collection, and drop the empty upload step.
  Either way, record the choice in Results. Do not keep an upload of a directory that nothing writes.
- Status: open

### Issue 2 — Severity: nit
- File: .github/workflows/workflow.yml:122
- Description: The comment says "warn: release and probe do not run the test step, so the directory may be empty." The step's own `if:` already skips release and probe, so that is not why the directory is empty. It is empty on every PR and merge run because nothing writes it (Issue 1). The comment gives the wrong reason for `warn`.
- Suggestion: Fix this together with Issue 1. Once coverage is produced, state the real reason for the `if-no-files-found` setting, or switch to `error`.
- Status: open
