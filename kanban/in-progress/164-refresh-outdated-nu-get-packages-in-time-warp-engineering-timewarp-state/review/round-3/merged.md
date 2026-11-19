# Round 3 — merged findings
**Date:** 2026-11-19
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Resolved prior

### M1 — Severity: bug — Status: fixed
- File: tools/dev-cli/endpoints/nuspec-dependency-allow-list.cs:57
- Description: Re-checked on `005d9b2e` after the merge of `origin/master`. The Hosting allow-list still includes `Aspire.Hosting.Dotnet`. `source/timewarp-state-devtools-hosting/timewarp-state-devtools-hosting.csproj` still has a public `PackageReference` to that id (no `PrivateAssets`). The merge did not edit the allow-list or that project. `## Results` from the fix pass still records `./bin/dev pack` exit 0 and the Hosting nuspec dependency `Aspire.Hosting.Dotnet` 17.0.0. The resume pass did not change those files.
- Suggestion: none remaining
- Source: general
- Disposition notes: fixed in fix pass 2026-11-19. Re-verified in round 2 and again in round 3 after the task 165 merge. Stays fixed.

## Issues

None.

## Duplicates / conflicts

- None. One reviewer. M1 carried forward from rounds 1 and 2.
