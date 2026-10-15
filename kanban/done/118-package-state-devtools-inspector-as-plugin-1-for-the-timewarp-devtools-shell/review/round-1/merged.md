# Round 1 — merged findings
**Date:** 2026-10-16
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 3 | 0 | 0 |
| nit | 1 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-state-devtools-hosting/add-timewarp-state-dev-tools.cs:403
- Description: `WithTimeWarpStateDevTools` adds the endpoint env reference and `WaitFor` on the
  inspector in publish mode, so the inspected app's publish output references the excluded inspector
  (breaks "run mode only / never in publish output").
- Suggestion: No-op `WithTimeWarpStateDevTools` when `ExecutionContext.IsPublishMode`; optionally skip
  the dll file-exists check in publish mode; add a publish-mode test.
- Source: general
- Disposition notes:

### M2 — Severity: suggestion — Status: open
- File: tests/timewarp-state-devtools-tests/packaged-devtools-app-tests.cs:128
- Description: Out-of-tree test starts the dll itself with `ASPNETCORE_URLS`; the resource
  `AddTimeWarpStateDevTools` builds (`dotnet`, `tools/` cwd, `ASPNETCORE_HTTP_PORTS`) never runs, and
  `/_framework/blazor.web.js` is not checked.
- Suggestion: Launch with `ASPNETCORE_HTTP_PORTS` only and GET `blazor.web.js`; better, drive the
  launch from the built resource or run the consumer AppHost.
- Source: general
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: source/timewarp-state-devtools-hosting/add-timewarp-state-dev-tools.cs:274
- Description: Launch command is bare `dotnet` from PATH; not guaranteed for apphost-exe / IDE /
  `DOTNET_ROOT` installs, and may pick a runtime without net11 ASP.NET Core.
- Suggestion: Prefer `DOTNET_HOST_PATH`, then the running muxer, then `DOTNET_ROOT/dotnet`, then
  `dotnet`; fix Design region / readme wording; test the order.
- Source: general
- Disposition notes:

### M4 — Severity: suggestion — Status: open
- File: source/timewarp-state-devtools-app/timewarp-state-devtools-app.csproj:185
- Description: Build-time `Publish` of the inspector with different global properties is a second
  project instance sharing `bin/`/`obj/` with the normal build; parallel builds can race.
- Suggestion: Publish with `NoBuild=true` after the `ProjectReference` build (or isolate its
  intermediate/output paths); same for the pack-time publish.
- Source: general
- Disposition notes:

### M5 — Severity: nit — Status: open
- File: source/timewarp-state-devtools-app/timewarp-state-devtools-app.csproj:207
- Description: Package `tools/` ships `appsettings.Development.json`, `tsconfig.json`, `web.config`,
  and XML doc files that the packaged inspector does not use.
- Suggestion: Exclude them from `_DevToolsPublishedFile` or mark them not-for-publish.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- Single reviewer; none.
