# Round 1 — merged findings
**Date:** 2026-10-24
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: nit — Status: fixed
- File: kanban/in-progress/126-normalize-file-encoding-to-utf-8-without-bom/task.md:53
- Description: Results described all 7 `kebab-path-names` warnings as Blazor `*.lib.module.js` / MSBuild `.targets`
  paths. One is the `kanban/backlog/110-…--predefined-state-for-any-site/` folder.
- Suggestion: Correct the Results and How-to-validate wording.
- Source: general (Issue 1)
- Disposition notes: Fixed in the kitchen by the review oracle, which may edit `## Results`. The Results and How-to-validate lines
  now name 6 Blazor/MSBuild paths plus the kanban 110 folder. No product change.

## Duplicates / conflicts

- None (single reviewer).
