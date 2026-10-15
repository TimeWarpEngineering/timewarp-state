# Round 2 — merged findings
**Date:** 2026-10-16
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 3 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-state-devtools-hosting/add-timewarp-state-dev-tools.cs:234
- Description: Publish mode wired the inspected app to the excluded inspector.
- Source: general (round 1)
- Disposition notes: `WithTimeWarpStateDevTools` returns early on `IsPublishMode`. The file-exists
  check is skipped in publish mode. `LeaveTheInspectedAppUnwiredWhenPublishing` asserts no
  `WaitAnnotation` and no `TIMEWARP_STATE_DEVTOOLS_ENDPOINT`. The test passed in round 2.

### M2 — Severity: suggestion — Status: fixed
- File: tests/timewarp-state-devtools-tests/packaged-devtools-app-tests.cs:146
- Description: The out-of-tree test did not use the resource's launch shape.
- Source: general (round 1)
- Disposition notes: The test now uses `devtools.Resource.Command`, the dll directory, and only
  `ASPNETCORE_HTTP_PORTS`, and it asserts that `blazor.web.js` returns 200. It still runs the dll
  itself, not the consumer AppHost. That is the lighter option round 1 offered.

### M3 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-devtools-hosting/add-timewarp-state-dev-tools.cs:66
- Description: The launch command was `dotnet` from PATH.
- Source: general (round 1)
- Disposition notes: `ResolveAppLaunchCommand` tries `DOTNET_HOST_PATH`, the running muxer,
  `DOTNET_ROOT/dotnet`, then `dotnet`. `ResolveTheDotNetHostInOrder` passed. Docs and the Design
  region were updated.

### M4 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-devtools-app/timewarp-state-devtools-app.csproj:42
- Description: The build-time publish compiled the inspector a second time under different global
  properties.
- Source: general (round 1)
- Disposition notes: The target is now `AfterTargets="Build"`, and both publishes use `NoBuild=true`
  and `BuildProjectReferences=false`. The build log shows the inspector build running after the
  ProjectReference build, so no concurrent compile.

### M5 — Severity: nit — Status: fixed
- File: source/timewarp-state-devtools-app/timewarp-state-devtools-app.csproj:69
- Description: The package `tools/` folder shipped development and documentation files.
- Source: general (round 1)
- Disposition notes: A Release pack in round 2 has no `appsettings.Development.json`,
  `tsconfig.json`, `web.config`, or `*.xml` under `tools/`. The pdbs and `blazor.web.js` are still
  there.

## New findings

None.

## Duplicates / conflicts

- Single reviewer; none.
