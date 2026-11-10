# Review framework

## Budget (by-diff)

- Lines changed: 248
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 154

**Date:** 2026-11-11
**Host task:** kanban/in-progress/154-clear-the-advisory-repo-audit-warnings/
**Diff scope:** branch `task/154-clear-the-advisory-repo-audit-warnings` vs `master` (merge-base). Product files: `.editorconfig`, `.github/workflows/workflow.yml`. Kitchen move of task.md is not reviewed as product.
**Plan / brief:** task.md `## Goal (implementer restatement)`: clear the four advisory `ganda repo audit` warnings (ci-code-coverage, kebab-path-names, vscode-window-icon, kanban-landed) under the per-item constraints in `## Requirements`.
**Effort:** 2 (by-diff budget), roster axis general
**Reviewer roster:** general (review oracle, claude-opus-5-5)
**Session IDs:** review oracle under ganda task work (claude, model claude-opus-5-5)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
