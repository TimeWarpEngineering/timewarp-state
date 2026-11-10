# Round 1 — merged findings
**Date:** 2026-11-11
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 1 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: .github/workflows/workflow.yml:120
- Description: The new `coverage` upload step uploads `artifacts/coverage`, but nothing in the repo writes coverage there. Neither `scripts/e2e-tests.cs` nor `scripts/test.cs` passes `--collect` or `--results-directory`, and `coverlet.collector` is referenced but never turned on. So the step uploads nothing on every run (`if-no-files-found: warn`), and its only effect is that the **`ci-code-coverage`** audit check passes.
- Suggestion: Either (a) collect coverage for real: add `--collect:"XPlat Code Coverage"` and `--results-directory <repo>/artifacts/coverage` to the e2e `dotnet test` (and the unit runner if feasible), then tighten `if-no-files-found`; or (b) if coverage is not wanted, remove the unused `coverlet.collector` reference and the empty upload step. Record the choice in Results.
- Source: general
- Disposition notes:

### M2 — Severity: nit — Status: open
- File: .github/workflows/workflow.yml:122
- Description: The step comment says the directory may be empty because release and probe skip the test step. The step's own `if:` already excludes those modes. The directory is empty because nothing writes it.
- Suggestion: Rewrite the comment together with M1 so it states the real reason for the `if-no-files-found` setting.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None. M2 comes from the same root cause as M1, but it is a separate edit (a comment), so it is kept as a separate finding.

## Verified without findings

- `.editorconfig` `[ganda.audit]`: the check stays on at `warning`, nothing was renamed, and the reason and task id are recorded. Pruning the directories `js`, `types`, `typescript`, `build`, or `wwwroot` would hide far more than the six files, so not using `prune` is correct.
- Backlog 110: left as it is. `ganda kanban --help` has no rename command (the command list in Results matches). Nothing was exempted for it.
- `.vscode/settings.json`: ignored and not force-added. `vscode-window-icon` passes.
- A fresh `ganda repo audit` agrees with Results on `ci-code-coverage`, `vscode-window-icon`, and the seven `kebab-path-names` paths. `kanban-landed` fails only on 154's own unlanded kitchen, a host step. The 098 dead claim is gone.
- `git status`: no tracked wwwroot JS deleted.
