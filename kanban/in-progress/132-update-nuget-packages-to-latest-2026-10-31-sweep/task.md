# Update NuGet packages to latest (2026-10-31 sweep)

## Description

Routine dependency sweep per the repo audit policy: take every central pin in
`Directory.Packages.props` to its latest version. `ganda nuget outdated` on master `e55f64d7`
(2026-10-31) listed 13 outdated pins: 1 major, 5 minor, 7 patch.

| Package | Current | Latest | Kind |
| --- | --- | --- | --- |
| Microsoft.CodeAnalysis.CSharp | 4.14.0 | 5.x | major — **hold** (consumer compiler floor; see Notes) |
| Aspire.Hosting, Aspire.Hosting.AppHost | 13.6.1 | 13.6.4 | patch |
| OpenTelemetry, .Extensions.Hosting, .Exporter.OpenTelemetryProtocol | 1.19.1 | 1.20.1 | minor |
| MSTest.TestAdapter, MSTest.TestFramework | 4.5.1 | 4.5.2 | patch |
| Microsoft.NET.Test.Sdk | 18.10.1 | 18.11.0 | minor |
| Microsoft.Playwright.MSTest.v4 | 1.63.0 | 1.64.1 | minor |
| TimeWarp.Nuru, TimeWarp.Nuru.DevCli | 4.0.0-beta.x | newer 4.0.0-beta | prerelease |
| TimeWarp.Architecture (contracts) | 2.0.0-beta.x | newer 2.0.0-beta | prerelease |

Blazored.LocalStorage / Blazored.SessionStorage report "not found on nuget.org"; previous sweeps
(task 119) confirmed 4.5.0 / 2.4.0 are the latest published releases — re-check and leave them if so.

Filed by Claude (cockpit) at Steven's request, 2026-10-31.

## Requirements

- Re-run `ganda nuget outdated` at the start; the table above is the baseline, not the contract.
  TimeWarp packages resolve from the TimeWarp GitHub source
  (`https://nuget.pkg.github.com/TimeWarpEngineering/index.json`) as well as nuget.org.
- Bump every outdated pin to latest except where a bump breaks the build or tests; record each
  exception here with the reason. `Microsoft.CodeAnalysis.CSharp` stays at `4.14.0`
  (consumer compiler floor for the shipped analyzer and generator; `PinRoslynAtTheConsumerCompilerFloor`
  locks it) unless Steven says otherwise — record that as the one intended hold.
- Audit fixes are policy: whatever `ganda repo audit --fix` changes is kept and committed.
- Playwright bump: keep `Microsoft.Playwright.MSTest.v4` paired with MSTest 4.x (memory:
  MSTest 4 needs the `.v4` adapter or e2e discovers zero tests and still exits 0) and confirm the
  e2e run still discovers 12 tests.
- TimeWarp.Nuru / Nuru.DevCli bump: `./bin/dev self-install` afterwards and confirm `dev --help`,
  `dev check-version`, `dev release --dry-run` still work (the dev CLI is built on Nuru).
- Version: run `./bin/dev check-version`. If the current source version is already released, bump
  `<Version>` in `source/Directory.Build.props` and `TimeWarpStateVersion` in `msbuild/repository.props`
  to the next prerelease and add `documentation/release-notes/release<version>.md` plus the
  `toc.yml` entry, listing the dependency changes; otherwise add the dependency changes to the
  current unreleased release-notes page.
- `./bin/dev build` warning-free for touched projects, `./bin/dev test` green,
  `./bin/dev verify-samples` green, e2e green, `ganda repo audit` clean.
- Results: before/after table, exceptions, and the check-version outcome.

## Checklist

- [x] `ganda nuget outdated` re-run; baseline table refreshed in Results
- [x] `Directory.Packages.props` bumped (all except the Roslyn hold)
- [x] `dev self-install` after the Nuru bump; dev CLI commands verified
- [x] e2e discovers 12 tests with the Playwright/MSTest bump
- [x] Version / release notes per `dev check-version`
- [x] build, test, verify-samples, e2e, audit all green
- [x] Results written

## Results

`ganda nuget outdated` on 2026-10-31 listed the same 13 pins as the filing table. Versions below are the tool's line output, confirmed against the nuget.org flat container. The TimeWarp GitHub org feed (`https://nuget.pkg.github.com/TimeWarpEngineering/index.json`) authenticates and lists five packages only: TimeWarp.Flexbox, TimeWarp.Ganda, TimeWarp.Taratibu, TimeWarp.Taratibu.Routines, TimeWarp.Zana. None of the central TimeWarp pins are published there, so nuget.org is the source for those versions.

| Package | Before | After |
| --- | --- | --- |
| Aspire.Hosting | 13.6.1 | 13.6.4 |
| Aspire.Hosting.AppHost | 13.6.1 | 13.6.4 |
| Aspire.AppHost.Sdk (sample 04 and the packaged DevTools consumer test) | 13.6.1 | 13.6.4 |
| OpenTelemetry | 1.19.1 | 1.20.1 |
| OpenTelemetry.Extensions.Hosting | 1.19.1 | 1.20.1 |
| OpenTelemetry.Exporter.OpenTelemetryProtocol | 1.19.1 | 1.20.1 |
| MSTest.TestAdapter | 4.5.1 | 4.5.2 |
| MSTest.TestFramework | 4.5.1 | 4.5.2 |
| Microsoft.NET.Test.Sdk | 18.10.1 | 18.11.0 |
| Microsoft.Playwright.MSTest.v4 | 1.63.0 | 1.64.1 |
| TimeWarp.Nuru | 4.0.0-beta.7 | 4.0.0-beta.10 |
| TimeWarp.Nuru.DevCli | 4.0.0-beta.7 | 4.0.0-beta.10 |
| TimeWarp.Architecture.Analyzers | 2.0.0-beta.19 | 2.0.0-beta.29 |
| Microsoft.CodeAnalysis.CSharp | 4.14.0 | 4.14.0 (held) |
| Blazored.LocalStorage | 4.5.0 | 4.5.0 |
| Blazored.SessionStorage | 2.4.0 | 2.4.0 |

Every other central pin was already current, including TimeWarp.Mediator and TimeWarp.Mediator.Generators at `15.0.0-beta.2`, TimeWarp.Amuru and TimeWarp.Amuru.Tools at `2.0.0`, and TimeWarp.SourceGenerators at `1.0.0-beta.11`.

### Exceptions

`Microsoft.CodeAnalysis.CSharp` stays `4.14.0`. The outdated tool offers `5.9.0` (major). The analyzer and source generator compile against this pin and ship inside the packages, so the pin is the consumer compiler floor. `PinRoslynAtTheConsumerCompilerFloor` locks it. `5.9.0` would raise that floor.

Blazored.LocalStorage `4.5.0` and Blazored.SessionStorage `2.4.0` stay. The outdated tool says both are not found on nuget.org. The flat container still lists those versions as the newest published releases (the packages are unlisted, which is why the tool cannot see them).

TimeWarp.Architecture.Analyzers `2.0.0-beta.29` adds TWA0026, TWA0027, TWA0028, TWA0032, TWA0033, TWA0034, TWA0036, and TWA0037. They stay off. Only TWA0004 applies to this library (task 099-001). TWA0036 (ISO 8601 dates) warns on sample and test-app weather dates and one DevTools timestamp. `[*.cs]` covers the C# hits. Razor files are additional files, so the same severities are also in `msbuild/external-sources.globalconfig`. A probe file without a Purpose region still fails TWA0004. The date displays are unchanged.

TimeWarp.Nuru.DevCli `4.0.0-beta.10` content calls `Task.FromResult`. `global using static TimeWarp.Mediator.Unit` makes `Unit.Task` collide with `System.Threading.Tasks.Task` (CS0229), so `./bin/dev self-install` failed on that pin. The dev CLI drops the static import and returns `Unit.Value`. The pin stays at beta.10.

### check-version

`./bin/dev check-version`: source `12.0.0-beta.16`, latest NuGet `12.0.0-beta.15`, one prerelease increment ahead, "Version in source is new — safe to release." `<Version>` and `TimeWarpStateVersion` stay `12.0.0-beta.16`. The dependency list is on `documentation/release-notes/release12.0.0-beta.16.md`. `toc.yml` already lists that page.

### Dev CLI

`./bin/dev self-install` succeeded after the Unit import change. `./bin/dev --help` lists build, test, e2e, pack, verify-samples, workflow, check-version, clean, release, and self-install. On a dirty tree, `./bin/dev release --dry-run` authenticates `gh` and refuses until the tree is clean. After the sweep commit, the same command on this branch prints `gh authenticated` and `working tree clean`, then exits 1 with `release must be cut from master — current branch is task/132-update-nuget-packages-to-latest-2026-10-31-sweep`. It does not tag.

### Gates

`./bin/dev build` (Release filter, samples except 00-auto): 0 errors. After the TWA0036 severity change the printed warnings are the pre-existing set (NU1510, NU1902, NU1903, TW0007, CS8600, CS8602, RS0030, BL0016). No TWA0036. `verify-samples` also printed no TWA0036.

`./bin/dev test` exited 0: analyzer 44 passed; source generator 86 passed; state 131 passed and 1 skipped; plus 40 passed and 1 skipped; telemetry 21 passed; devtools 103 passed; devtools isolation 4 passed; client integration 64 passed and 1 skipped; architecture 7 passed and 1 skipped.

`./bin/dev verify-samples` printed `Packed set verified` and `Samples verified successfully!` (10 project-referenced samples, then 2 packaged).

`./bin/dev e2e`: Playwright's browser install tried `sudo` and exited 1. Chromium was already present. Failed 0, Passed 12, Total 12.

`ganda repo audit` exits 0. `ganda repo audit --fix` changed nothing. Two advisories, both pre-existing: `kanban-landed` (task 098 has no usable worktree) and `kebab-path-names` (7 paths: Blazor `*.lib.module.js` names, a `.targets` file, and backlog task 110's folder).

### How to validate

**Smoke:**

```bash
./bin/dev build
./bin/dev test
./bin/dev verify-samples
./bin/dev e2e
./bin/dev check-version
ganda repo audit
```

**Expect:**

- `build` exits 0 with 0 errors and no `TWA0036`.
- `test` exits 0 with the counts in Gates.
- `verify-samples` prints `Packed set verified` and `Samples verified successfully!`.
- `e2e` exits 0: Failed 0, Passed 12, Total 12. The Playwright `sudo` install may exit 1 when Chromium is already installed; the test run still discovers 12 tests.
- `check-version` prints source `12.0.0-beta.16`, latest NuGet `12.0.0-beta.15`, and "safe to release" until that prerelease is published.
- `ganda repo audit` exits 0. The advisories are `kanban-landed` for task 098 and `kebab-path-names` with the same 7 paths.

## Session

- Created: cockpit (2026-10-31)
- Implementation: implementer oracle (2026-10-31)
- Clean-tree `dev release --dry-run` confirmed the master guard; the task branch is pushed (2026-10-31)

## Goal (implementer restatement)

Bring every central package pin in `Directory.Packages.props` up to the latest version that still builds and tests, then record what moved and what stayed. The live `ganda nuget outdated` report is the list to act on; the table in Description is only the starting snapshot. TimeWarp packages are checked on nuget.org and on the TimeWarp GitHub package source. `Microsoft.CodeAnalysis.CSharp` stays at `4.14.0` because that pin is the compiler floor for the shipped analyzer and generator (`PinRoslynAtTheConsumerCompilerFloor`). Any other pin that cannot move is written down here with the reason. Blazored.LocalStorage and Blazored.SessionStorage stay put if 4.5.0 and 2.4.0 are still the newest published releases.

After the Nuru and Nuru.DevCli bump, rebuild the dev CLI with `./bin/dev self-install` and confirm `dev --help`, `dev check-version`, and `dev release --dry-run` still run. Keep `Microsoft.Playwright.MSTest.v4` on the MSTest 4 adapter and confirm the end-to-end run still discovers 12 tests. `./bin/dev check-version` decides the package version: if `12.0.0-beta.16` is already released, move `<Version>` and `TimeWarpStateVersion` to the next prerelease and add a release-notes page plus a `toc.yml` entry; if it is still unreleased, add the dependency changes to the existing beta.16 notes. Whatever `ganda repo audit --fix` changes is kept. Done means a warning-free build of the touched projects, green `./bin/dev test`, green `./bin/dev verify-samples`, green e2e, and a clean `ganda repo audit`, with a before/after table in Results.

## Notes

- Related: 119 (previous sweep, Mediator beta.5 dogfood, 2026-10-13), 105 (nuspec dependency
  allow-list), 109 M2 (`IsAotCompatible` on Blazor and Plus).
- Roslyn hold rationale lives in 119's Results ("Roslyn floor"): the analyzer and generator ship inside
  the packages, so the pin is the minimum compiler a consumer needs; raising to 5.x raises that floor.
