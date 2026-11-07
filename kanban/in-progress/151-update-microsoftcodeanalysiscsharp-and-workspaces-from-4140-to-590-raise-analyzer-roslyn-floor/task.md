# Update Microsoft.CodeAnalysis.CSharp and Workspaces from 4.14.0 to 5.9.0 (raise analyzer Roslyn floor)

## Description

`ganda nuget outdated` (2026-11-07) reports two outdated packages, both major:

| Package | Current | Latest |
|---|---|---|
| Microsoft.CodeAnalysis.CSharp | 4.14.0 | 5.9.0 |
| Microsoft.CodeAnalysis.CSharp.Workspaces | 4.14.0 | 5.9.0 |

These are the only two left; task 146 updated everything else. They are pinned deliberately. The comment in
`Directory.Packages.props` says: "Analyzer and source generator compile against this Roslyn. 4.14.0 loads on every
.NET 10 SDK (5.9.0 would require SDK 10.0.400+)." The analyzer (`timewarp-state-analyzer`), code fixes, and the source
generator (`timewarp-state-source-generator`) ship inside the NuGet packages and run in the consumer's compiler, so
the Roslyn version they compile against sets the minimum SDK a consumer needs. The pin is guarded by
`PinRoslynAtTheConsumerCompilerFloor` in `tests/timewarp-state-tests/packaging/nuspec-dependency-allow-list-tests.cs`,
which asserts the literal `Version="4.14.0"` line.

Repo context: `global.json` is SDK 11.0.100-rc.3; CI uses it via setup-dotnet. The 10.x SDK on the dev box is 10.0.403,
already above the 10.0.400 floor.

## Decision: BUMP THE FLOOR (Steve, 2026-11-07)

Steve decided: bump to 5.9.0 and raise the consumer SDK floor. Do not re-deliberate the keep path. Still verify and
record the exact floor 5.9.0 implies (which SDK bundles Roslyn 5.9) under `## Results`, because the props comment may
be stale. The remaining bullets describe what was considered:

- Do we accept that TimeWarp.State 12 consumers need SDK 10.0.400+ (or any .NET 11 SDK)? Check the floor 5.9.0
  actually implies (Roslyn 5.9 ships with which SDK; verify against the package's release notes / the SDK's bundled
  Roslyn, not the comment).
- If yes: bump, update the pin comment and the guard test to the new floor and reason, and state the new minimum in
  `documentation/` wherever the supported SDK is documented and in the release notes for the next version.
- If no: keep the pin, and make the task's deliverable the mechanism that stops `ganda nuget outdated` flagging it
  (an explicit allow/ignore entry if ganda supports one, else a note in the props comment pointing here) so the
  report is clean. Say which under Results.

## Requirements (bump path)

- Update both versions in `Directory.Packages.props`; keep them equal (code fixes compile against the same floor as
  the analyzer).
- Check `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` / `CodeFix.Testing` 1.1.4 still resolve against Roslyn 5.9
  and the analyzer tests, generator tests (Jaribu), and the Roslyn-based `state-clone-planner` tests all pass.
- Build the shipped packages and confirm the analyzer and generator load in a consumer at the new floor:
  `verify-samples` (sample 00 auto restores the packed packages) is the existing proof; run it locally with the
  10.0.403 SDK as well as 11 rc.3 and record both.
- Keep `PrivateAssets="all"` on the analyzer/generator Roslyn references so Microsoft.CodeAnalysis.CSharp does not
  become a package dependency (`NuspecDependencyAllowList` test).
- Update `PinRoslynAtTheConsumerCompilerFloor` to the new version and rename/recomment if the floor rationale changes.
- `dev build`, `dev test`, `dev e2e` green with `TreatWarningsAsErrors` on (task 148); new Roslyn analyzers may surface
  new warnings in the analyzer/generator projects. Fix them, do not NoWarn.
- Run `ganda repo audit --fix` in this worktree and commit whatever it changes (audit fixes are policy).

## Goal (implementer restatement)

Move the Roslyn compile references of the shipped analyzer, code fixes and source generator from
`Microsoft.CodeAnalysis.CSharp` / `.Workspaces` 4.14.0 to 5.9.0, which raises the compiler a consumer needs. Done means:

- Both pins are 5.9.0 and equal; the props comment, the analyzer/generator test csproj comments and
  `PinRoslynAtTheConsumerCompilerFloor` name the new floor and the reason.
- The floor is verified from the SDK's own bundled Roslyn (not the old comment) and written down: which 10.0.x and
  .NET 11 SDKs carry Roslyn 5.9 or later.
- Roslyn stays `PrivateAssets="all"`, so the nuspec allow-list test still passes (no new package dependency).
- `dev build`, `dev test` (analyzer, code fix, generator, clone planner, packaging), `dev e2e` and `verify-samples`
  (packed consumer, sample 00 auto) pass with warnings as errors, on SDK 11 rc.3 and on 10.0.403 where the SDK can
  build the sample.
- The consumer SDK requirement is stated in the docs (installation) and in the next release notes as a release item.
- `ganda nuget outdated` reports 0 outdated; `ganda repo audit --fix` run and its changes committed.

## Checklist

- [x] Floor decision: bump (Steve 2026-11-07); verified SDK → Roslyn mapping still to be recorded under `## Results`
- [x] Both packages at 5.9.0 in `Directory.Packages.props` (or pin kept and outdated report silenced, per decision)
- [x] Pin comment and `PinRoslynAtTheConsumerCompilerFloor` updated
- [x] Analyzer, code-fix, generator and clone-planner test suites pass
- [x] `verify-samples` passes on SDK 10.0.403 and 11 rc.3; results recorded
- [x] `NuspecDependencyAllowList` still passes (no new package dependency)
- [x] Supported-SDK statement updated in docs / release notes
- [x] `ganda nuget outdated` reports 0 outdated
- [x] `ganda repo audit --fix` run and its changes committed
- [x] `## Results` filled in

## Session

- Created: ffba6066-c22c-42a7-95c8-de75506a6687 (2026-11-07)
- Context: Steve asked for a task to update the two outdated CodeAnalysis libs after `ganda nuget outdated` (2026-11-07)
- Launch: Steve said "launch 151 bump the floor" — decision made, bump path only (2026-11-07)
- Implement (ganda task work, 2026-11-07): bumped both pins to 5.9.0, verified the floor from dotnet/sdk tags, docs + beta.18 notes; build/test/e2e/verify-samples green. `dev test` first failed on the stale `~/.dotnet/toolResolverCache` (fixie); cleared it and reran.

## Results

### Floor (verified, not from the old comment)

The SDK's bundled Roslyn, read from `eng/Version.Details.props` (`MicrosoftNetCompilersToolsetPackageVersion`) at
each dotnet/sdk release tag, and from the installed SDKs' `Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll` file
version:

| SDK | Bundled Roslyn | Loads 5.9.0 analyzers |
|---|---|---|
| 10.0.302 | 5.6.0-2.26217.103 | no |
| 10.0.400 | 5.9.0-1.26374.102 | yes (first on .NET 10) |
| 10.0.403 (dev box) | 5.900.26.52412 | yes |
| 11.0.100-preview.4 | 5.7.0 | no |
| 11.0.100-preview.5 | 5.8.0 | no |
| 11.0.100-preview.6 | 5.9.0-1.26311.113 | yes (first on .NET 11) |
| 11.0.100-rc.1 | 5.11.0 | yes |
| 11.0.100-rc.3 (global.json) | 5.1200.26.52348 (5.12) | yes |

The old comment ("5.9.0 would require SDK 10.0.400+") was right for the .NET 10 line. TimeWarp.State 12 packages
target `net11.0`, so the floor that matters to a consumer is **.NET 11 SDK preview.6 or later**. SDK 10.0.400 is the
matching floor for anything that runs the analyzers on the .NET 10 compiler.

### Changes

- `Directory.Packages.props`: `Microsoft.CodeAnalysis.CSharp` and `.Workspaces` 4.14.0 → 5.9.0 (equal). Comment
  states the new floor and the reason. `PrivateAssets="all"` unchanged in every project.
- `PinRoslynAtTheConsumerCompilerFloor` asserts `Version="5.9.0"`; Design region names the new floor. Name kept: the
  rationale (pin = consumer compiler floor) did not change, only the version.
- Analyzer/generator test csproj comments and the incrementality test note updated to 5.9.0.
- Docs: requirement line in `readme.md` and `documentation/partials/installation.md`; floor + CS9057 note in
  `documentation/topics/analyzers.md`. New `documentation/release-notes/release12.0.0-beta.18.md` (added to the toc)
  with the floor as a breaking release item. Version stays 12.0.0-beta.17 (the bump belongs to the release PR).
- New Roslyn surfaced no new warnings: `dev build` 0 warnings / 0 errors with TreatWarningsAsErrors.
- `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` / `CodeFix.Testing` 1.1.4 resolve against Roslyn 5.9 without
  change (analyzer tests 90 passed).

### Gates

- `./bin/dev build`: succeeded, 0 warnings, 0 errors.
- `./bin/dev test`: exit 0. Analyzer 90 passed; source generator 174 passed (includes the `StateCloneSourceGenerator_`
  clone-planner tests); state 153 passed / 1 skipped (includes all five `NuspecDependencyAllowList_Should_`); plus
  40/1; telemetry 21; devtools 124; devtools isolation 4; client integration 67/1; architecture 7/1.
- `./bin/dev e2e`: 12/12 passed.
- `./bin/dev verify-samples` on SDK 11.0.100-rc.3: exit 0, "Packed set verified", "Samples verified successfully"
  (sample 00 auto restored the packed packages). The packed `TimeWarp.State.nuspec` lists only `TimeWarp.Mediator`.
- SDK 10.0.403: `verify-samples` cannot run there. Every sample targets `net11.0`, and SDK 10 refuses with NETSDK1045
  ("does not support targeting .NET 11.0"), checked with a scratch `net11.0` project pinned to 10.0.403. Substitute
  proof: a scratch `net10.0` project pinned to 10.0.403 (`rollForward: disable`) that loads the three DLLs from the
  packed `TimeWarp.State.12.0.0-beta.17.nupkg` `analyzers/dotnet/cs` as `<Analyzer>` items reported
  `error TWS0001: The Action 'LooseAction' is not a nested type of its State` with no CS9057, so the 5.9.0 analyzer
  loads and runs on the 10.0.4xx compiler.
- `ganda nuget outdated`: "All packages are up to date!" (0 outdated).
- `ganda repo audit --fix`: passes; `--fix` changed no files. Two advisory warnings were already there and are not
  fixed here: `kanban-landed` (task 098 claim has no worktree; another task's claim) and `kebab-path-names` (Blazor
  `*.lib.module.js` / `.targets` names, which the framework fixes by convention).

### How to validate

Smoke:

```bash
grep -n 'Microsoft.CodeAnalysis.CSharp' Directory.Packages.props
./bin/dev build && ./bin/dev test && ./bin/dev verify-samples
ganda nuget outdated
unzip -p artifacts/packages/TimeWarp.State.12.0.0-beta.17.nupkg TimeWarp.State.nuspec | grep dependency
```

Expect:

- Both `Microsoft.CodeAnalysis.CSharp` and `.Workspaces` show `Version="5.9.0"`.
- Build has 0 warnings. Every test suite passes, including `NuspecDependencyAllowList_Should_.PinRoslynAtTheConsumerCompilerFloor`.
- `verify-samples` ends with "Samples verified successfully!"
- `ganda nuget outdated` ends with "All packages are up to date!"
- The nuspec has one dependency, `TimeWarp.Mediator`, and no `Microsoft.CodeAnalysis.CSharp`.

## Notes

- Related: task 146 updated all other outdated packages and left these two pinned.
- Consumer impact is the whole point of this task. Treat the bump as a release-notes item, not a chore.
