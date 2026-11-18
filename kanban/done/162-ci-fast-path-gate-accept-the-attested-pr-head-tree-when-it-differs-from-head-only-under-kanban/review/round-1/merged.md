# Round 1 — merged findings
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
- File: scripts/ci-fast-path-tree.ps1:32
- Description: The miss text is built from `git diff --name-only` without `--no-renames`, then paths under `kanban/` are dropped. A move from `source/` into `kanban/` is a real difference outside `kanban/` (the stripped trees differ, so the gate still misses), but default rename detection lists only the `kanban/` destination. The filter removes that line, and the reason becomes `differs outside kanban/:` with no path. The repo already treats this as load-bearing in `tools/dev-cli/endpoints/workflow/ci-artifact-service.cs` (`--no-renames` so a move from `source/` into `kanban/` still shows `source/`). Targeted check, not a build or test run: in `/tmp/kanban-rename-check-162`, tree `8cfac0244a190e9740efa3cee4759e512aab9f09` (`source/a.cs`) vs tree `bbfcc1f456391948aed25ddb8fa9a5e4bc416096` (`git mv` to `kanban/a.cs`). `git diff --name-only` printed only `kanban/a.cs`. `git diff --name-only --no-renames` printed `kanban/a.cs` and `source/a.cs`.
- Suggestion: Pass `--no-renames` on that diff so the printed reason names the source path. Keep the accept/reject decision on the stripped tree ids.
- Source: general
- Disposition notes: fixed in the fix pass. `git diff --name-only --no-renames` in `scripts/ci-fast-path-tree.ps1`; test `Name_The_Source_Path_When_A_File_Moves_Into_Kanban` pins `differs outside kanban/: source/lib/a.cs` for a move into kanban/.

### M2 — Severity: suggestion — Status: fixed
- File: .github/workflows/workflow.yml:7
- Description: The workflow step comment and the gate script header describe the candidate walk and the kanban-only rule. The banner at the top of the same workflow still says the fast path runs when `dev ci` passed on "this exact tree" and points at `documentation/dev-ops/local-ci.md`. That doc still says the note must be for `HEAD^{tree}`, and that if `master` moved after `dev ci` the merge tree differs and CI runs in full (`documentation/dev-ops/local-ci.md:37` and `:49`). That sentence is the miss this task exists to fix. The SourceLink note there also says the packed commit has the same tree CI checked out.
- Suggestion: Update the workflow banner and `documentation/dev-ops/local-ci.md` so they describe the candidate walk and the top-level `kanban` rule. Keep the exact-tree rule for `dev ci` itself.
- Source: general
- Disposition notes: fixed in the fix pass. Workflow banner, gate Purpose line, and `documentation/dev-ops/local-ci.md` (What CI does, Notes, SourceLink) describe the candidate walk and the top-level `kanban` rule; `dev ci` keeps the exact-tree rule.

## Duplicates / conflicts

- None. One reviewer.
