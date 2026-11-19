# Round 1 — merged findings
**Date:** 2026-11-19
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: tools/dev-cli/endpoints/nuspec-dependency-allow-list.cs:53
- Description: `TimeWarp.State.DevTools.Hosting` references `Aspire.Hosting.Dotnet` with no `PrivateAssets` (`source/timewarp-state-devtools-hosting/timewarp-state-devtools-hosting.csproj:14`). `AddTimeWarpStateDevToolsProject` returns `IResourceBuilder<DotnetProjectResource>`, so the dependency belongs in the nuspec. The allow-list for that package is only `Aspire.Hosting` and `TimeWarp.State.DevTools.App`. `dev pack` exits 1 on any other id (`tools/dev-cli/endpoints/pack-command.cs:126`). The allow-list tests do not open a nupkg, and `## Results` does not record `./bin/dev pack`. Targeted check: `unzip -p artifacts/packages/TimeWarp.State.DevTools.Hosting.12.0.0-beta.19.nupkg TimeWarp.State.DevTools.Hosting.nuspec` shows `<dependency id="Aspire.Hosting.Dotnet" version="17.0.0" exclude="Build,Analyzers" />`. The current project file still has that reference.
- Suggestion: Add `Aspire.Hosting.Dotnet` to the Hosting allow-list. Keep the reference public. Run `./bin/dev pack` and expect exit 0 with that dependency listed.
- Source: general
- Disposition notes: fixed in fix pass 2026-11-19. `Aspire.Hosting.Dotnet` added to the Hosting allow-list; the reference stays public. New test `AllowEachPublicPackageReferenceOfEveryPackableProject` checks every packable project's public PackageReference ids against its allow-list, so this drift fails in the test run. `./bin/dev pack` exits 0 and the Hosting nuspec lists `Aspire.Hosting.Dotnet` 17.0.0.

## Duplicates / conflicts

- None. One reviewer.
