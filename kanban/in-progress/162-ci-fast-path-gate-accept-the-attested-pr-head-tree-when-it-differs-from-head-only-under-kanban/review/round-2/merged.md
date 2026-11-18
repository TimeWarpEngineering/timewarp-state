# Round 2 — merged findings
**Date:** 2026-11-18
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: scripts/ci-fast-path-tree.ps1:35
- Description: Round 1: `git diff --name-only` without `--no-renames` dropped the only listed path when a file moved from `source/` into `kanban/`, so the miss reason named no path. The gate still missed, because the stripped tree ids differ.
- Suggestion: Pass `--no-renames` so the reason names the source path. Keep the accept or reject decision on the stripped tree ids.
- Source: general
- Disposition notes: Verified on `57c98e15`. The diff uses `git diff --name-only --no-renames`, and `Name_The_Source_Path_When_A_File_Moves_Into_Kanban` expects `differs outside kanban/: source/lib/a.cs`. The fix-pass record says that class passed (4 tests).

### M2 — Severity: suggestion — Status: fixed
- File: .github/workflows/workflow.yml:7
- Description: Round 1: the workflow banner and `documentation/dev-ops/local-ci.md` still said the fast path needs this exact tree, and that a master move after `dev ci` always makes CI run in full. That sentence is the miss this task exists to fix.
- Suggestion: Update the banner and the doc for the candidate walk and the top-level `kanban` rule. Keep the exact-tree rule for `dev ci` itself.
- Source: general
- Disposition notes: Verified on `57c98e15`. Banner (workflow.yml:7), gate Purpose (`scripts/ci-fast-path-gate.ps1`), and `documentation/dev-ops/local-ci.md` describe the walk and the kanban-only rule. `dev ci` stays exact-tree. The SourceLink note matches the stripped-tree rule.

## Duplicates / conflicts

- None. One reviewer. No new findings on the fix delta.
