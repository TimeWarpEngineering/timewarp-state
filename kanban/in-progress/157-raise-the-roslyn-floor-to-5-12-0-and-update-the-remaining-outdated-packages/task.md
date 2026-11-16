# Raise the Roslyn floor to 5.12.0 and update the remaining outdated packages

## Description

`ganda nuget outdated` on master (2026-11-16) reports 10 outdated packages. Bring them all current.

| Package | Current | Latest |
|---|---|---|
| Microsoft.CodeAnalysis.Analyzers | 5.9.0 | 5.12.0 |
| Microsoft.CodeAnalysis.CSharp | 5.9.0 | 5.12.0 |
| Microsoft.CodeAnalysis.CSharp.Workspaces | 5.9.0 | 5.12.0 |
| OpenTelemetry | 1.20.1 | 1.21.0 |
| OpenTelemetry.Extensions.Hosting | 1.20.1 | 1.21.0 |
| OpenTelemetry.Exporter.OpenTelemetryProtocol | 1.20.1 | 1.21.0 |
| Microsoft.NET.Test.Sdk | 18.11.0 | 18.12.0 |
| MSTest.TestAdapter | 4.5.2 | 4.5.3 |
| MSTest.TestFramework | 4.5.2 | 4.5.3 |
| coverlet.collector | 10.1.0 | 10.1.1 |

## Decision: raise the Roslyn floor (Steve, 2026-11-16)

Roslyn was pinned at 5.9.0 as the consumer compiler floor (task 151): the analyzer, code fixes, and
source generator ship in the packages and run in the consumer's compiler, so the Roslyn they
compile against sets the minimum SDK a consumer needs.

That floor no longer protects anyone. Since task 156 the repo is on .NET 11 GA (`global.json`
SDK 11.0.100) and the library projects target `net11.0`, so a consumer must already be on a
.NET 11 SDK. SDK 11.0.100 bundles Roslyn 5.12.0 (verified from the installed SDK's
`Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll`: `5.12.0-1.26529.118`; SDK 10.0.404 bundles
5.9.0). Accepted cost: .NET 11 preview / RC SDKs that bundle an older Roslyn are no longer
supported. Do not re-deliberate the keep path.

Raising the floor also undoes a workaround from task 156: `TimeWarp.SourceGenerators` beta.16
requires Roslyn 5.12, so the two analyzer test projects held at 5.9 dropped their lint-only
SourceGenerators reference and lost TW0007 / TWA0004.

## Requirements

1. Run `ganda repo audit --fix` first and commit what it changes (audit fixes are policy).
2. Bump the three Roslyn packages to 5.12.0 in `Directory.Packages.props`; keep them equal.
3. Update the pin comment in `Directory.Packages.props` to the new floor and reason
   (5.12.0 loads on SDK 11.0.100+; state which SDK that is, verified, not copied from here if you
   can check it).
4. Update the guard test `PinRoslynAtTheConsumerCompilerFloor` in
   `tests/timewarp-state-tests/packaging/nuspec-dependency-allow-list-tests.cs` to the new literal
   version and reason. Keep the guard; do not delete it.
5. Restore the `TimeWarp.SourceGenerators` reference on the two analyzer test projects that task
   156 removed it from, so TW0007 and TWA0004 run on them again. Find them from the task 156 diff
   (merge commit `36e0194d`) and remove any comment that explains the removal. Fix any lint
   findings that surface.
6. Confirm the analyzer, code-fix, and source-generator projects (netstandard2.0) still build and
   that Roslyn stays `PrivateAssets` and does not become a package dependency (task 105).
7. Bump the remaining seven packages in the table. `OpenTelemetry` is a library reference, so it
   is a packed dependency change; the other two OpenTelemetry packages are sample-only.
8. Document the new minimum wherever the supported SDK is stated (`documentation/`, readme,
   `claude.md`, overview, release notes for the unreleased version). Search for `5.9` and
   `10.0.400` so no stale floor statement survives, including in the packed readme.
9. Version: 12.0.0-beta.18 is on master but not released. Check `dev check-version`; bump only if
   it requires it, and add these changes to the unreleased release notes.
10. Gates: `./bin/dev build` 0 warnings / 0 errors; unit tests green; `ganda repo audit` passing
    including `package-pin-currency`; `ganda nuget outdated` reports 0 outdated (the two
    `Blazored.*` "not found on nuget.org" lines are pre-existing; leave them and mention them).
11. `--no-incremental` builds delete tracked `wwwroot` JS in `timewarp-state-blazor`; rebuild to
    regenerate and check `git status` before finishing. If fixie asks for a tool restore, that is
    the stale `~/.dotnet/toolResolverCache`; report it rather than working around it in the repo.

## Goal (implementer restatement)

Move the analyzer / code-fix / source-generator Roslyn floor from 5.9.0 to 5.12.0 (the Roslyn that
.NET SDK 11.0.100 GA bundles), and bring the other seven outdated pins current.

Acceptance:

- `ganda repo audit --fix` run first; whatever it changes is committed.
- `Microsoft.CodeAnalysis.Analyzers`, `.CSharp`, `.CSharp.Workspaces` all 5.12.0; pin comment states the
  verified SDK that bundles 5.12; guard test `PinRoslynAtTheConsumerCompilerFloor` asserts 5.12.0.
- The two analyzer test projects that dropped `TimeWarp.SourceGenerators` in task 156 get it back (comment
  removed) and build clean with TW0007 / TWA0004 running.
- netstandard2.0 analyzer, code-fix and generator projects build; Roslyn stays `PrivateAssets` and is not a
  nuspec dependency.
- OpenTelemetry x3 1.21.0, Microsoft.NET.Test.Sdk 18.12.0, MSTest x2 4.5.3, coverlet.collector 10.1.1.
- No stale 5.9 / 10.0.400 floor statement in docs, readme, claude.md, packed readme; beta.18 release notes
  describe the change.
- `./bin/dev build` 0/0, `./bin/dev test` green, `ganda repo audit` passes, `ganda nuget outdated` reports 0
  outdated (Blazored.* "not found" lines pre-existing), tracked wwwroot JS intact.

## Checklist

- [x] `ganda repo audit --fix` run and committed
- [x] Roslyn ×3 at 5.12.0; pin comment and guard test updated
- [x] SourceGenerators lint restored on the two analyzer test projects
- [x] Roslyn still not a package dependency
- [x] OpenTelemetry ×3, Test.Sdk, MSTest ×2, coverlet bumped
- [x] Docs and release notes state the new floor; no stale 5.9 / 10.0.400 statements
- [x] Build clean, tests green, tracked wwwroot JS intact
- [x] `ganda repo audit` passes; `ganda nuget outdated` reports 0 outdated

## Results

### What changed

- `ganda repo audit --fix` ran first and changed nothing, so there was no audit-fix commit.
- `Directory.Packages.props`: Microsoft.CodeAnalysis.Analyzers / .CSharp / .CSharp.Workspaces 5.9.0 → 5.12.0;
  OpenTelemetry ×3 1.20.1 → 1.21.0; Microsoft.NET.Test.Sdk 18.11.0 → 18.12.0; MSTest.TestAdapter /
  TestFramework 4.5.2 → 4.5.3; coverlet.collector 10.1.0 → 10.1.1. The pin comment now gives the 5.12.0 floor
  and the SDK that bundles it.
- Floor verified from the installed SDKs' `Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll` ProductVersion:
  11.0.100 → `5.12.0-1.26529.118`, 10.0.404 → `5.9.0-1.26529.119`.
- Guard test `PinRoslynAtTheConsumerCompilerFloor` now asserts 5.12.0, with the reason in its Design region.
  The nuspec fixture in `RejectAnUnexpectedDependencyInNuspecXml` uses 5.12.0 too.
- `timewarp-state-analyzer-tests` and `timewarp-state-source-generator-tests`: removed the task 156
  `<PackageReference Remove="TimeWarp.SourceGenerators" />` and its comment. Both restore
  TimeWarp.SourceGenerators 1.0.0-beta.16 with Microsoft.CodeAnalysis.CSharp 5.12.0 (checked in
  `obj/project.assets.json`). No new lint findings came up: the build has 0 warnings.
- Docs: `readme.md` (this is the packed readme of TimeWarp.State; the 5.12 line is confirmed inside the nupkg),
  `documentation/partials/installation.md`, `documentation/topics/analyzers.md` and the release notes
  for unreleased 12.0.0-beta.18 (the floor section is rewritten to 5.12 / SDK 11.0.100, and a task 157 dependency
  section is added) now give the new floor. `claude.md` and `overview.md` had no floor statement. No stale
  5.9 / 10.0.400 floor statement remains outside older release notes and history comments.
- Version: `./bin/dev check-version` reports source 12.0.0-beta.18 against NuGet beta.17, safe to release.
  No bump.
- Roslyn is still not a package dependency. None of the packed nuspecs lists any `Microsoft.CodeAnalysis.*`.
  `TimeWarp.State.DevTools.Export` now depends on `OpenTelemetry` 1.21.0. The analyzer, code-fix and
  generator csprojs keep `PrivateAssets="all"`.
- Tracked `wwwroot` JS is intact: `git status` shows no deletions. No `--no-incremental` build ran.
- Environment note: the first `./bin/dev test` failed with "Run dotnet tool restore" for fixie.
  `~/.dotnet/toolResolverCache/1/fixie.console` pointed at the gc'd task-156 worktree's `artifacts/nuget-packages`.
  `rm -rf ~/.dotnet/toolResolverCache` followed by `dotnet tool restore` fixed it. Nothing in the repo changed.

### Gates

| Gate | Command | Result |
|---|---|---|
| Build | `./bin/dev build` | exit 0, 0 Warning(s), 0 Error(s) |
| Test | `./bin/dev test` | exit 0. All suites pass (90, 174, 153+1 skipped, 40+1 skipped, 21, 124, 4, 67+1 skipped, 7+1 skipped). The skips are the template `SkipExample`. |
| Audit | `ganda repo audit` | exit 0, "Repository passes", `package-pin-currency` PASS. One advisory: `kebab-path-names` (warning, pre-existing and documented in `.editorconfig` by task 154) |
| Outdated | `ganda nuget outdated` | "All packages are up to date!". `Blazored.LocalStorage` / `Blazored.SessionStorage` "not found on nuget.org" is pre-existing |

### How to validate

- Smoke: `./bin/dev build && ./bin/dev test`, then `grep -n 'CodeAnalysis' Directory.Packages.props` and
  `unzip -p artifacts/packages/TimeWarp.State.12.0.0-beta.18.nupkg '*.nuspec' | grep -c CodeAnalysis`.
- Expect: the build has 0 warnings and 0 errors, and the tests are green, including
  `NuspecDependencyAllowList_Should_.PinRoslynAtTheConsumerCompilerFloor`. All three Roslyn pins read 5.12.0.
  The nuspec count is `0`. `ganda nuget outdated` reports all packages up to date.

## Session

- Created: 31bc0748-8dda-4ee8-912e-8d546f2d6990 (2026-11-16)

## Notes

Requested by Steven 2026-11-16 after `ganda nuget outdated`: asked why Roslyn was held, pointed
out the repo is on .NET 11, then "create task and launch it".
