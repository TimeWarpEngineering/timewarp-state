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

## Decision to make first

Bumping is a consumer-facing floor change, not a plain dependency refresh. Decide and record under `## Results`:

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

## Checklist

- [ ] Floor decision recorded under `## Results` with the verified SDK → Roslyn mapping
- [ ] Both packages at 5.9.0 in `Directory.Packages.props` (or pin kept and outdated report silenced, per decision)
- [ ] Pin comment and `PinRoslynAtTheConsumerCompilerFloor` updated
- [ ] Analyzer, code-fix, generator and clone-planner test suites pass
- [ ] `verify-samples` passes on SDK 10.0.403 and 11 rc.3; results recorded
- [ ] `NuspecDependencyAllowList` still passes (no new package dependency)
- [ ] Supported-SDK statement updated in docs / release notes
- [ ] `ganda nuget outdated` reports 0 outdated
- [ ] `ganda repo audit --fix` run and its changes committed
- [ ] `## Results` filled in

## Session

- Created: ffba6066-c22c-42a7-95c8-de75506a6687 (2026-11-07)
- Context: Steve asked for a task to update the two outdated CodeAnalysis libs after `ganda nuget outdated` (2026-11-07)

## Notes

- Related: task 146 updated all other outdated packages and left these two pinned.
- Consumer impact is the whole point of this task. Treat the bump as a release-notes item, not a chore.
