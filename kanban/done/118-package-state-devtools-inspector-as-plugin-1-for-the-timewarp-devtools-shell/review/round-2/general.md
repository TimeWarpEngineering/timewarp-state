# Round 2 — general
**Date:** 2026-10-16
**Scope reviewed:** fix delta `9f4be5b8..04c63679` ("fix: keep the packaged DevTools inspector out of
publish output"; 10 files). Re-verified round-1 M1–M5 and scanned the delta for new defects.

## Summary

The fix pass addresses every round-1 finding. `WithTimeWarpStateDevTools` returns the inspected
resource unchanged in publish mode, and `AddTimeWarpStateDevTools` skips the file check there (M1).
The out-of-tree test launches with the resource's command, the dll directory, and only
`ASPNETCORE_HTTP_PORTS`, and it requests `/_framework/blazor.web.js` (M2). `ResolveAppLaunchCommand`
tries `DOTNET_HOST_PATH`, then the running muxer, then `DOTNET_ROOT/dotnet`, then `dotnet`, and a test
covers that order (M3). The inspector is published after `Build` with `NoBuild=true` (M4). Pack drops
the development files and XML docs from `tools/` (M5). Docs, readmes, and Design regions match the
code. No new defects found.

## Verification

- `dotnet build tests/timewarp-state-devtools-tests -v:n -m`: Build succeeded, 0 errors. The log
  shows the inspector's `Build` from the App's `PublishDevToolsAppOnBuild` (`AfterTargets="Build"`)
  starting only after the ProjectReference build of the inspector finished. The `Publish` invocation
  runs `NoBuild`.
  A `dotnet build` with no `-c` gives the explicit `Build` call a second configuration
  (`Configuration=Debug` is global there and not on the ProjectReference). That call runs after the
  first build finished and every target is up to date. In Release pack runs (`-c Release`) the
  property sets match. No concurrent second compile was observed, so M4 holds.
- `dotnet test … --no-build` with console logger: all `AddTimeWarpStateDevTools_Should` cases pass,
  including `ResolveTheDotNetHostInOrder` and `LeaveTheInspectedAppUnwiredWhenPublishing`. 29 passes
  were logged before `PackagedDevToolsApp_Should.StartFromAnOutOfTreeAppHost` ran past the 240 s
  timeout. Like round 1, that test restores Aspire into a fresh `globalPackagesFolder` from nuget.org.
  Fixie ignored `--filter`. The implementer's 66/66 for the full project was not reproduced here.
- I reproduced the test's pack step:
  `dotnet pack source/timewarp-state-devtools-app/… -c Release -p:DevToolsToolsDir=<tmp>/publish/`
  succeeded. The nupkg ships `tools/TimeWarp.State.DevTools.dll`, the pdbs, and
  `tools/wwwroot/_framework/blazor.web.js`. It has no `appsettings.Development.json`,
  `tsconfig.json`, `web.config`, or `*.xml` under `tools/`; the only `.xml` is NuGet's
  `[Content_Types].xml` (M5).
- The inspector's `appsettings.Development.json` holds only logging levels, so leaving it out does
  not change behavior when the AppHost environment is Development.
- The timed-out test runs may have left `/tmp/tw-devtools-consumer-*` folders behind, and the pack
  check left `/tmp/r118-pack-*`. The reviewer may not `rm`.

## Issues

<!-- None. -->
