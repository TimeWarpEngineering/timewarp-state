# Round 1 — merged findings
**Date:** 2026-10-10
**Sources:** general

Orchestrator verification: `dotnet docfx documentation/docfx.json --disableGitFeatures --logLevel warning` → Build succeeded, 0 warning(s), 0 error(s).

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 1 |

## Issues

### M1 — Severity: nit — Status: wontfix
- File: samples/overview.md:7
- Description: Link texts use PascalCase sample names (`00-StateActionHandler`, …) while targets are kebab-case folders.
- Suggestion: Rename link text to match folder names.
- Source: general
- Disposition notes: Link text is a human-readable heading, not a path; it matches the display style of samples 04–07 (`06-Render control`) and the sample titles. No warning, no broken link. Decided by: review orchestrator.

## Duplicates / conflicts

- None.
