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

- [ ] `ganda nuget outdated` re-run; baseline table refreshed in Results
- [ ] `Directory.Packages.props` bumped (all except the Roslyn hold)
- [ ] `dev self-install` after the Nuru bump; dev CLI commands verified
- [ ] e2e discovers 12 tests with the Playwright/MSTest bump
- [ ] Version / release notes per `dev check-version`
- [ ] build, test, verify-samples, e2e, audit all green
- [ ] Results written

## Session

- Created: cockpit (2026-10-31)

## Notes

- Related: 119 (previous sweep, Mediator beta.5 dogfood, 2026-10-13), 105 (nuspec dependency
  allow-list), 109 M2 (`IsAotCompatible` on Blazor and Plus).
- Roslyn hold rationale lives in 119's Results ("Roslyn floor"): the analyzer and generator ship inside
  the packages, so the pin is the minimum compiler a consumer needs; raising to 5.x raises that floor.
