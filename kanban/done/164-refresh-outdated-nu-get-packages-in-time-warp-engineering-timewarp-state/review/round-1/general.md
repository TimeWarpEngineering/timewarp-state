# Round 1 — general
**Date:** 2026-11-19
**Scope reviewed:** `task/164-refresh-outdated-nu-get-packages-in-time-warp` vs `master` (three-dot, 16 files). Kitchen `## Goal (implementer restatement)`, `## Results`, and `### How to validate`.

## Summary

The branch bumps the three pins the task names and sets the package version to `12.0.0-beta.19`. Aspire 17 makes `AddProject(name, path)` and `Aspire.AppHost.Sdk` obsolete, so the hosting package, the telemetry sample, the DevTools tests, and the docs move to `AddDotnetProject` from a new `Aspire.Hosting.Dotnet` 17.0.0 pin. That return type change is a beta break and the release notes say so. The recorded build is 0 warnings, the DevTools Fixie suite is 124 passed, the sample AppHost smoke reached Running, and audit passed. Those gates do not cover `dev pack`.

The new package reference on `TimeWarp.State.DevTools.Hosting` has no `PrivateAssets`, so it is a nuspec dependency. The pack allow-list for that package still names only `Aspire.Hosting` and `TimeWarp.State.DevTools.App`. `dev pack` fails closed on any other id, and the wave release packs through that command. One targeted read of the already packed nupkg confirms the id is in the nuspec. The implementer restatement also adds an `Aspire.AppHost.Sdk` 17.0.0 pin the task does not ask for; the code removes that SDK instead. That mismatch is a misread under `## Learnings`, not a product defect.

## Issues

### Issue 1 — Severity: bug
- File: tools/dev-cli/endpoints/nuspec-dependency-allow-list.cs:53
- Description: `TimeWarp.State.DevTools.Hosting` now has a direct `PackageReference` to `Aspire.Hosting.Dotnet` (`source/timewarp-state-devtools-hosting/timewarp-state-devtools-hosting.csproj:14`) with no `PrivateAssets`. The public method `AddTimeWarpStateDevToolsProject` returns `IResourceBuilder<DotnetProjectResource>`, so the dependency has to flow to consumers, and the packed nuspec lists it. `NuspecDependencyAllowList` for `TimeWarp.State.DevTools.Hosting` allows only `Aspire.Hosting` and `TimeWarp.State.DevTools.App`. `PackCommand.NuspecDependenciesAllowed` sets exit code 1 when it finds any other id (`tools/dev-cli/endpoints/pack-command.cs:126`). The allow-list unit tests compare the list to itself; they do not read the nuspec, so the recorded `./bin/dev build`, DevTools Fixie run, and `ganda repo audit` stay green. `## Results` does not record `./bin/dev pack`. Targeted check, because the new reference looked like an unlisted packed dependency: `unzip -p artifacts/packages/TimeWarp.State.DevTools.Hosting.12.0.0-beta.19.nupkg TimeWarp.State.DevTools.Hosting.nuspec`. That nuspec's net11.0 group contains `Aspire.Hosting.Dotnet` version `17.0.0` (SourceLink commit `49a817ba`, packed from this tree before the implementation commit). The current csproj still has the same reference, so the next pack emits the same dependency and `dev pack` fails.
- Suggestion: Add `Aspire.Hosting.Dotnet` to the `TimeWarp.State.DevTools.Hosting` set in `nuspec-dependency-allow-list.cs`. Leave the package reference public (do not set `PrivateAssets=all`); consumers of `AddTimeWarpStateDevToolsProject` need `DotnetProjectResource`. Then run `./bin/dev pack` and expect it to print the dependency and exit 0.
- Status: open
