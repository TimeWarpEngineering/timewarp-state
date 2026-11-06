# Update outdated NuGet packages 2026-11-06 (Roslyn 5.9, TimeWarp.Architecture patch)

## Description

`ganda nuget outdated` on master reported three outdated packages in Directory.Packages.props. Update them. This PR's merge commit also gives master a CI run, which the `dev release` guard needs (see task 133).

## Checklist

- [x] Run `ganda nuget outdated --update --force`
- [x] `./bin/dev build` with Roslyn 5.9.0 (green, 0 errors)
- [x] `./bin/dev test` with Roslyn 5.9.0 (1 failure: PinRoslynAtTheConsumerCompilerFloor, intentional guard)
- [x] Hold Roslyn packages at 4.14.0, rebuild and retest (green)
- [x] `ganda repo audit --fix` (no changes)
- [x] Commit, push, open PR

## Results

Version changes kept:

- TimeWarp.Architecture.Analyzers 2.0.0-beta.29 -> 2.0.0-beta.31

Held back:

- Microsoft.CodeAnalysis.CSharp 4.14.0 (5.9.0 available)
- Microsoft.CodeAnalysis.CSharp.Workspaces 4.14.0 (5.9.0 available)

Why held back: with 5.9.0 the build is green but
`NuspecDependencyAllowList_Should_.PinRoslynAtTheConsumerCompilerFloor`
(tests/timewarp-state-tests/packaging/nuspec-dependency-allow-list-tests.cs) fails by design: it asserts
`Microsoft.CodeAnalysis.CSharp` is pinned at 4.14.0, the floor that loads in every consumer .NET 10 SDK
(5.9.0 needs SDK 10.0.400+). Moving the floor is a product decision (consumer compatibility), not a trivial fix.
A follow-up task should decide whether to raise the floor and update that test and the comment in Directory.Packages.props.

Test results (with Roslyn 4.14.0 and Analyzers beta.31): build 0 errors, 151 warnings; `dev test` exit 0, all suites passed
(90, 174, 146+1 skipped, 40+1 skipped, 21, 124, 4, 67+1 skipped, 7+1 skipped).
`ganda repo audit --fix` changed nothing; two pre-existing advisories (kanban-landed 098, kebab-path-names).

## Session

- Created: ffba6066-c22c-42a7-95c8-de75506a6687 (2026-11-06)
- Implementation: claude code session (2026-11-06)
