# Round 2 — merged findings
**Date:** 2026-11-16
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Resolved prior

### M1 — Severity: suggestion — Status: fixed
- File: readme.md:44
- Description: Round 1: the packed readme and `documentation/partials/installation.md:8` said every .NET 11 preview and RC SDK was unsupported. The decision limits that to SDKs that bundle an older Roslyn.
- Suggestion: Use the decision's qualifier. Leave the minimum at .NET SDK 11.0.100 (GA).
- Source: general
- Disposition notes: Fixed in `70df11fc`. Round 2 re-read both files and `unzip -p artifacts/packages/TimeWarp.State.12.0.0-beta.18.nupkg readme.md`. All three carry ".NET 11 preview and RC SDKs that bundle an older Roslyn are not supported."

## Issues

No new issues.

## Duplicates / conflicts

- One reviewer. Nothing to collapse.
