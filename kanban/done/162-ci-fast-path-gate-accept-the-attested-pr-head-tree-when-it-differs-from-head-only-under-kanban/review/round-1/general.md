# Round 1 — general
**Date:** 2026-11-18
**Scope reviewed:** branch `task/162-ci-fast-path-gate-accept-the-attested-pr-head-tree` vs `master` (three-dot): the fast-path gate, the dot-sourced kanban strip helper, the workflow env, and the pwsh unit test. Also the operator doc the workflow banner cites.

## Summary

The gate walks HEAD, then `CI_FAST_PATH_PR_HEAD`, then each parent when HEAD is a merge. The first candidate whose tree has a note and matches HEAD once the top-level `kanban` entry is removed wins; a noted candidate that differs outside `kanban/` is rejected and the walk continues. Stage, SDK, RID, package set, and sha256 checks still read the attested tree. The new test runs that helper under pwsh. Recorded `./bin/dev build` (0 warnings), the three `CiFastPathTreeTests`, the six `CiNoteInputTests`, `ganda repo audit`, and the scratch-clone dry run on `8139649c` cover the change. This review did not re-run those gates. Checkout is `fetch-depth: 0`, so the PR head and merge parents are present. The hit/miss decision is the stripped tree id, and the recorded dry run shows that check accepting the real PR #692 trees.

## Issues

### Issue 1 — Severity: suggestion
- File: scripts/ci-fast-path-tree.ps1:32
- Description: The miss text is built from `git diff --name-only` without `--no-renames`, then paths under `kanban/` are dropped. A move from `source/` into `kanban/` is a real difference outside `kanban/` (the stripped trees differ, so the gate still misses), but default rename detection lists only the `kanban/` destination. The filter removes that line, and the reason becomes `differs outside kanban/:` with no path. The repo already treats this as load-bearing in `tools/dev-cli/endpoints/workflow/ci-artifact-service.cs` (`--no-renames` so a move from `source/` into `kanban/` still shows `source/`). Targeted check, not a build or test run: in `/tmp/kanban-rename-check-162`, tree `8cfac0244a190e9740efa3cee4759e512aab9f09` (`source/a.cs`) vs tree `bbfcc1f456391948aed25ddb8fa9a5e4bc416096` (`git mv` to `kanban/a.cs`). `git diff --name-only` printed only `kanban/a.cs`. `git diff --name-only --no-renames` printed `kanban/a.cs` and `source/a.cs`.
- Suggestion: Pass `--no-renames` on that diff so the printed reason names the source path. Keep the accept/reject decision on the stripped tree ids.
- Status: open

### Issue 2 — Severity: suggestion
- File: .github/workflows/workflow.yml:7
- Description: The workflow step comment and the gate script header describe the candidate walk and the kanban-only rule. The banner at the top of the same workflow still says the fast path runs when `dev ci` passed on "this exact tree" and points at `documentation/dev-ops/local-ci.md`. That doc still says the note must be for `HEAD^{tree}`, and that if `master` moved after `dev ci` the merge tree differs and CI runs in full (`documentation/dev-ops/local-ci.md:37` and `:49`). That sentence is the miss this task exists to fix. The SourceLink note there also says the packed commit has the same tree CI checked out.
- Suggestion: Update the workflow banner and `documentation/dev-ops/local-ci.md` so they describe the candidate walk and the top-level `kanban` rule. Keep the exact-tree rule for `dev ci` itself.
- Status: open
