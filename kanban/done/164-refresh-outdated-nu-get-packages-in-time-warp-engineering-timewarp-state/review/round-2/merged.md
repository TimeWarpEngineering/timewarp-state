# Round 2 — merged findings
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
- Description: Re-checked on `04cf07c0`. The Hosting allow-list includes `Aspire.Hosting.Dotnet`. `source/timewarp-state-devtools-hosting/timewarp-state-devtools-hosting.csproj` still has a public `PackageReference` to that id (no `PrivateAssets`). `AllowEachPublicPackageReferenceOfEveryPackableProject` reads that project (`PackageId` and `IsPackable` are in the csproj) and would fail if the id were missing. `## Results` records `./bin/dev pack` exit 0 and the Hosting nuspec dependency `Aspire.Hosting.Dotnet` 17.0.0.
- Suggestion: none remaining
- Source: general
- Disposition notes: fixed in fix pass 2026-11-19. Re-verified in round 2. Stays fixed.

## Issues

None.

## Duplicates / conflicts

- None. One reviewer. M1 carried forward from round 1.
