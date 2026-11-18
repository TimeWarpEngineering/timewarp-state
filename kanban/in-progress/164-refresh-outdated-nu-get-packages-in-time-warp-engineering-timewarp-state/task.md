# Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-state

## Description

Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-state.
Filter: (all)
Outdated:
  Microsoft.Playwright.MSTest.v4 1.64.1 -> 1.65.0 (dev-only: tests/test-app-end-to-end-tests/test-app-end-to-end-tests.csproj)
  Aspire.Hosting 13.6.5 -> 17.0.0 (library: source/timewarp-state-devtools-hosting/timewarp-state-devtools-hosting.csproj has no PrivateAssets=all)
  Aspire.Hosting.AppHost 13.6.5 -> 17.0.0 (dev-only: samples/04-telemetry/apphost/sample-04-apphost.csproj)
Bump the dev-only pins in this same task, so one release carries them.
Edit Directory.Packages.props in this repo only.

Version: set <Version> in source/Directory.Build.props from 12.0.0-beta.18 to 12.0.0-beta.19 in this pull request. The prerelease suffix is -beta.N, never +beta.N. The wave releases it after merge.

## Goal (implementer restatement)

Bring three outdated NuGet pins in `Directory.Packages.props` up to their latest stable versions and bump the package version so the next wave release carries them.

Acceptance criteria:
- `Microsoft.Playwright.MSTest.v4` 1.64.1 -> 1.65.0 (E2E test project only).
- `Aspire.Hosting` 13.6.5 -> 17.0.0 (shipped dependency of `TimeWarp.State.DevTools.Hosting`).
- `Aspire.Hosting.AppHost` 13.6.5 -> 17.0.0 (telemetry sample AppHost). The sample's `Aspire.AppHost.Sdk` pin moves to 17.0.0 with it so SDK and package match.
- `<Version>` in `source/Directory.Build.props` goes from 12.0.0-beta.18 to 12.0.0-beta.19.
- Any API breaks from the Aspire 13 -> 17 major are fixed in source; build, the DevTools hosting tests, and `ganda repo audit` pass.

## Session

- Minted by ganda task wave run 20261118T170809Z-b6e473
- 2026-11-19 implementer (claude): bumped Playwright.MSTest.v4 1.65.0, Aspire.Hosting / Aspire.Hosting.AppHost 17.0.0, Version 12.0.0-beta.19. Aspire 17 obsoletes `AddProject(name, path)` (CS0618 broke the build) and `Aspire.AppHost.Sdk` (ASPIRE012), so the hosting package, sample AppHost, and DevTools tests moved to `AddDotnetProject` from new pin `Aspire.Hosting.Dotnet` 17.0.0.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5), session not reported, max-turns 500, Done — 2026-11-18T20:10:48Z

## Results

### Changes

- `Directory.Packages.props`: Microsoft.Playwright.MSTest.v4 `1.64.1` → `1.65.0`; Aspire.Hosting, Aspire.Hosting.AppHost `13.6.5` → `17.0.0`; new pin Aspire.Hosting.Dotnet `17.0.0`.
- `source/Directory.Build.props`: `<Version>` `12.0.0-beta.18` → `12.0.0-beta.19`.
- `TimeWarp.State.DevTools.Hosting`: references Aspire.Hosting.Dotnet; `AddTimeWarpStateDevToolsProject` calls `AddDotnetProject` and returns `IResourceBuilder<DotnetProjectResource>` (was `ProjectResource`). Breaking on a beta package; recorded in release notes.
- `samples/04-telemetry/apphost`: off the obsolete `Aspire.AppHost.Sdk`. It is now plain `Microsoft.NET.Sdk` with Aspire.Hosting.AppHost + Aspire.Hosting.Dotnet, and it adds the server with `AddDotnetProject` (no server ProjectReference, no `Projects.sample_04_server`).
- `tests/timewarp-state-devtools-tests`: hosting tests use `AddDotnetProject` / `DotnetProjectResource`. The packaged consumer AppHost drops `Aspire.AppHost.Sdk` and references Aspire.Hosting.AppHost `17.0.0`.
- Docs: the hosting readme and `documentation/topics/devtools.md` show `AddDotnetProject`. New `documentation/release-notes/release12.0.0-beta.19.md` plus a toc entry.
- Audit boy-scout: `ganda repo audit --fix` cleared advisory `vscode-window-icon`. It wrote the gitignored local `.vscode/settings.json` only, so nothing was committed.

### Gates

- `./bin/dev build` → Build succeeded, 0 warnings, 0 errors. The first run failed on CS0618 `AddProject` obsolete. The migration above fixed it and the rerun passed.
- Changed tests are Fixie tests in `tests/timewarp-state-devtools-tests`. They are not standalone Jaribu runfiles (no `#if !JARIBU_MULTI`), and `./bin/dev test` takes no file arguments. The suite was run the way `scripts/test.cs` runs it: `NUGET_PACKAGES=artifacts/nuget-packages dotnet fixie timewarp-state-devtools-tests` → 124 passed.
  - `tests/timewarp-state-devtools-tests/hosting-registration-tests.cs` → all `AddTimeWarpStateDevTools_Should` cases passed (15).
  - `tests/timewarp-state-devtools-tests/packaged-devtools-app-tests.cs` → `PackagedDevToolsApp_Should.StartFromAnOutOfTreeAppHost` passed (packs, restores an Aspire 17 AppHost, starts the packaged inspector).
- E2E (`tests/test-app-end-to-end-tests`, the Playwright bump): it compiles in the build and was not run here. CI runs it.
- Smoke: `dotnet run --project samples/04-telemetry/apphost/sample-04-apphost.csproj` (timeout 150 s). The dashboard started, and the `timewarp-devtools` and `sample-04-server` resources both reached Running.
- `ganda repo audit` → passes (one advisory, vscode-window-icon). `ganda repo audit --fix` cleared it: "Repository passes all audit checks."

### How to validate

Smoke:
```bash
./bin/dev build
NUGET_PACKAGES=$PWD/artifacts/nuget-packages dotnet fixie timewarp-state-devtools-tests
dotnet run --project samples/04-telemetry/apphost/sample-04-apphost.csproj
```

Expect: the build succeeds with 0 warnings (no CS0618, no ASPIRE012), and Fixie reports `124 passed`. The AppHost prints a dashboard URL, and its log shows `sample-04-server` and `timewarp-devtools` in state Running. `grep -n Version source/Directory.Build.props` shows `12.0.0-beta.19`.
