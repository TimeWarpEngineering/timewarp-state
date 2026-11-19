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
- Reviewer: Grok session 01a1f048-ab18-7b91-a7c5-42dbba6fef1e (2026-11-19), effort 1, roster general, round 1. Turn cap 80.
- Reviewer round 2: Grok session 01a1f053-b783-7360-b462-b58d6babf168 (2026-11-19), effort 1, roster general, turn cap 80. Re-reviewed the M1 fix delta `43e09174..04cf07c0`. Disposition clean, 0 open.
- Reviewer round 3: Grok session 01a1f193-cfea-74c2-b386-5d29855ca2f7 (2026-11-19), effort 1, roster general, turn cap 80. Re-reviewed the resume merge `04cf07c0..005d9b2e` (task 165 into this branch). Disposition clean, 0 open.
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1f048-ab18-7b91-a7c5-42dbba6fef1e, max-turns 80, artifacts general.md, merged.md — 2026-11-18T20:18:54Z
- 2026-11-19 implementer (claude) resume pass: merged `origin/master` (task 165, one version SSOT) into the branch. The only conflict was this task file (master's copy lacked the resume note; kept ours). `msbuild/repository.props` and `Directory.Packages.props` merged clean.
- 2026-11-19 implementer (claude) fix pass for round 1: M1 fixed. `Aspire.Hosting.Dotnet` added to the `TimeWarp.State.DevTools.Hosting` entry in `tools/dev-cli/endpoints/nuspec-dependency-allow-list.cs`; new allow-list test checks packable projects' public PackageReferences against the list.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5), session not reported, max-turns 500, Done — 2026-11-19T02:12:37Z
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1f193-cfea-74c2-b386-5d29855ca2f7, max-turns 80, artifacts general.md, merged.md, disposition.md — 2026-11-19T02:17:29Z

## Results

### Changes

- `Directory.Packages.props`: Microsoft.Playwright.MSTest.v4 `1.64.1` → `1.65.0`; Aspire.Hosting, Aspire.Hosting.AppHost `13.6.5` → `17.0.0`; new pin Aspire.Hosting.Dotnet `17.0.0`.
- `source/Directory.Build.props`: `<Version>` `12.0.0-beta.18` → `12.0.0-beta.19`.
- `TimeWarp.State.DevTools.Hosting`: references Aspire.Hosting.Dotnet; `AddTimeWarpStateDevToolsProject` calls `AddDotnetProject` and returns `IResourceBuilder<DotnetProjectResource>` (was `ProjectResource`). Breaking on a beta package; recorded in release notes.
- `samples/04-telemetry/apphost`: off the obsolete `Aspire.AppHost.Sdk`. It is now plain `Microsoft.NET.Sdk` with Aspire.Hosting.AppHost + Aspire.Hosting.Dotnet, and it adds the server with `AddDotnetProject` (no server ProjectReference, no `Projects.sample_04_server`).
- `tests/timewarp-state-devtools-tests`: hosting tests use `AddDotnetProject` / `DotnetProjectResource`. The packaged consumer AppHost drops `Aspire.AppHost.Sdk` and references Aspire.Hosting.AppHost `17.0.0`.
- `tools/dev-cli/endpoints/nuspec-dependency-allow-list.cs` (review M1): the Hosting allow-list adds `Aspire.Hosting.Dotnet`, a public dependency because `AddTimeWarpStateDevToolsProject` returns `IResourceBuilder<DotnetProjectResource>`.
- `tests/timewarp-state-tests/packaging/nuspec-dependency-allow-list-tests.cs`: new `AllowEachPublicPackageReferenceOfEveryPackableProject` reads each packable `source/**/*.csproj` and fails when a PackageReference without `PrivateAssets="all"` is missing from that package's allow-list. It catches M1-style drift without packing.
- Docs: the hosting readme and `documentation/topics/devtools.md` show `AddDotnetProject`. New `documentation/release-notes/release12.0.0-beta.19.md` plus a toc entry.
- Audit boy-scout: `ganda repo audit --fix` cleared advisory `vscode-window-icon`. It wrote the gitignored local `.vscode/settings.json` only, so nothing was committed.

### Gates

- `./bin/dev build` → Build succeeded, 0 warnings, 0 errors. The first run failed on CS0618 `AddProject` obsolete. The migration above fixed it and the rerun passed.
- Changed tests are Fixie tests in `tests/timewarp-state-devtools-tests`. They are not standalone Jaribu runfiles (no `#if !JARIBU_MULTI`), and `./bin/dev test` takes no file arguments. The suite was run the way `scripts/test.cs` runs it: `NUGET_PACKAGES=artifacts/nuget-packages dotnet fixie timewarp-state-devtools-tests` → 124 passed.
  - `tests/timewarp-state-devtools-tests/hosting-registration-tests.cs` → all `AddTimeWarpStateDevTools_Should` cases passed (15).
  - `tests/timewarp-state-devtools-tests/packaged-devtools-app-tests.cs` → `PackagedDevToolsApp_Should.StartFromAnOutOfTreeAppHost` passed (packs, restores an Aspire 17 AppHost, starts the packaged inspector).
- E2E (`tests/test-app-end-to-end-tests`, the Playwright bump): it compiles in the build and was not run here. CI runs it.
- Smoke: `dotnet run --project samples/04-telemetry/apphost/sample-04-apphost.csproj` (timeout 150 s). The dashboard started, and the `timewarp-devtools` and `sample-04-server` resources both reached Running.
- Fix pass (review M1), 2026-11-19:
  - `dotnet run --file tools/dev-cli/dev.cs -- self-install` (dev-cli source changed), then `./bin/dev build --quiet` → Build completed successfully.
  - `tests/timewarp-state-tests/packaging/nuspec-dependency-allow-list-tests.cs` (Fixie, not a Jaribu runfile): `NUGET_PACKAGES=$PWD/artifacts/nuget-packages dotnet tool run fixie timewarp-state-tests --no-build --tests "*NuspecDependencyAllowList*"` → 6 passed.
  - `./bin/dev pack` → exit 0, "Packed set verified." `unzip -p artifacts/packages/TimeWarp.State.DevTools.Hosting.12.0.0-beta.19.nupkg TimeWarp.State.DevTools.Hosting.nuspec` lists `Aspire.Hosting.Dotnet` 17.0.0.
  - `ganda repo audit` → "Repository passes all audit checks."
- Resume pass (merge of `origin/master` for task 165), 2026-11-19:
  - `dotnet run --file tools/dev-cli/dev.cs -- self-install` (master changed `tools/dev-cli`), then `./bin/dev build` → Build completed successfully, 0 warnings, 0 errors.
  - `dotnet msbuild source/timewarp-state/timewarp-state.csproj -getProperty:TimeWarpStateVersion -getProperty:Version` → both `12.0.0-beta.19`. The "Version SSOT mismatch" check that parked open-pr no longer exists.
  - `./bin/dev check-version` → "Version in source is new — safe to release." (12.0.0-beta.19, NuGet latest 12.0.0-beta.17).
  - No test file changed in this pass; the full `./bin/dev ci` is left to the host open-pr node and PR CI.
  - `ganda repo audit` → "Repository passes all audit checks."
- `ganda repo audit` → passes (one advisory, vscode-window-icon). `ganda repo audit --fix` cleared it: "Repository passes all audit checks."

### How to validate

Smoke:
```bash
./bin/dev build
NUGET_PACKAGES=$PWD/artifacts/nuget-packages dotnet fixie timewarp-state-devtools-tests
NUGET_PACKAGES=$PWD/artifacts/nuget-packages dotnet tool run fixie timewarp-state-tests --tests "*NuspecDependencyAllowList*"
./bin/dev pack
unzip -p artifacts/packages/TimeWarp.State.DevTools.Hosting.12.0.0-beta.19.nupkg TimeWarp.State.DevTools.Hosting.nuspec | grep dependency
dotnet run --project samples/04-telemetry/apphost/sample-04-apphost.csproj
```

Expect: the build succeeds with 0 warnings (no CS0618, no ASPIRE012), and Fixie reports `124 passed`. The allow-list run reports `6 passed`. `dev pack` exits 0 with "Packed set verified.", and the Hosting nuspec lists `Aspire.Hosting.Dotnet` 17.0.0. The AppHost prints a dashboard URL, and its log shows `sample-04-server` and `timewarp-devtools` in state Running. `grep -n Version source/Directory.Build.props` shows `12.0.0-beta.19`.

### Review

Round 1, effort 1, roster general. Counts at close of that round were 1 open bug (M1). The fix pass set M1 to fixed.

Round 2, effort 1, roster general. Re-reviewed the fix delta `43e09174..04cf07c0`. M1 stays fixed. No new findings. Counts at close of that round: 0 open, 1 fixed (M1 bug), 0 wontfix. Disposition then: clean. Paths: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`.

Round 3, effort 1, roster general. Re-reviewed the resume merge of `origin/master` (task 165) `04cf07c0..005d9b2e`. The merge kept the task 164 pins and the M1 allow-list fix, and took the single version read from `source/Directory.Build.props`. The recorded resume build (0 warnings, both version properties `12.0.0-beta.19`, `./bin/dev check-version`, clean audit) covers that delta. M1 stays fixed. No new findings. Final counts: 0 open, 1 fixed (M1 bug), 0 wontfix. Disposition: clean. Paths: `review/review-framework.md`, `review/round-3/merged.md`, `review/disposition.md`. Gates were not re-run in this round.

## Learnings

- Misread (review round 1): the implementer restatement adds "The sample's `Aspire.AppHost.Sdk` pin moves to 17.0.0 with it so SDK and package match." The task does not ask for that pin. It asks to bump `Aspire.Hosting.AppHost` to 17.0.0. The code removes the obsolete SDK (ASPIRE012) instead of moving it to 17.0.0, which matches the task and the recorded 0-warning build.

## Review

- Round 1: Blocked — open M1
- Round 2: Done
- Round 3: Done
## Resume note (2026-11-19)

The walk parked at open-pr because local CI failed its first step:

```
Version SSOT mismatch: source/Directory.Build.props Version='12.0.0-beta.19',
msbuild/repository.props TimeWarpStateVersion='12.0.0-beta.18'. Align both.
```

The branch itself is not wrong. Task 165 (PR #696, merged 2026-11-19) made
`TimeWarpStateVersion` derive from `source/Directory.Build.props` `<Version>`, so there is now
one version and the single bump on this branch is enough.

To resume: merge `origin/master` into this branch to take task 165 (this branch does not touch
`msbuild/repository.props`, so no conflict is expected), run `./bin/dev ci`, then let the walk
continue from open-pr. Do not edit `msbuild/repository.props` by hand and do not change the
version. Review is already clean (round 2); the merge of master is the only new delta.

After merge the version 12.0.0-beta.19 is on master but not released. The release is run
separately (`ganda task wave --brief release-pending`), on Steve's go.

- Why: Steve 2026-11-19, wave run 20261118T170809Z-b6e473 parked this task; 165 fixed the cause.
