# Update outdated NuGet packages 2026-11-06 (Roslyn 5.9, TimeWarp.Architecture patch)

## Description

`ganda nuget outdated` on master reported three outdated packages in Directory.Packages.props. Update them. This PR's merge commit also gives master a CI run, which the `dev release` guard needs (see task 133).

## Goal (implementer restatement)

Bring the central package versions in `Directory.Packages.props` up to date, as `ganda nuget outdated` reports them,
and keep anything that would break a deliberate guard. The merged PR also gives master a fresh CI run for the
`dev release` guard (task 133).

Acceptance criteria:

- Every safe outdated package in `Directory.Packages.props` is bumped (TimeWarp.Architecture.Analyzers).
- Any package held back is listed with the reason (Roslyn stays at the consumer compiler floor).
- `./bin/dev build` gives 0 errors and `./bin/dev test` exits 0 on the branch.
- `ganda repo audit` passes, and the change is committed and pushed on the task branch.

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

Re-verified on the implement node (2026-11-06): `ganda repo audit` passes (same two advisories),
`./bin/dev build` 0 errors / 151 warnings, `./bin/dev test` exit 0 with the same suite counts.

Environment note: the first `dev test` rerun failed with `Run "dotnet tool restore" to make the "fixie" command available`,
on master as well. Cause: the SDK's global `~/.dotnet/toolResolverCache` pointed every local tool at
the task-133 worktree's `artifacts/nuget-packages`, and that worktree had been gc'd. The cache is keyed by tool and version,
not by `NUGET_PACKAGES`, so per-worktree isolated package folders go stale once their worktree is deleted.
Fix: `rm -rf ~/.dotnet/toolResolverCache` and `dotnet tool restore`. That is outside the repo, so this PR makes no product change for it.

### How to validate

Smoke:

```bash
grep -n 'TimeWarp.Architecture.Analyzers\|Microsoft.CodeAnalysis.CSharp' Directory.Packages.props
./bin/dev build
./bin/dev test
ganda repo audit
```

Expect:

- `TimeWarp.Architecture.Analyzers` is at `2.0.0-beta.31`; both `Microsoft.CodeAnalysis.CSharp*` entries are still at `4.14.0`.
- Build reports `0 Error(s)`.
- Test exits 0, every suite passes (analyzer 90, generator 174, state 146+1 skipped, plus 40+1 skipped, telemetry 21,
  devtools 124, isolation 4, client integration 67+1 skipped, architecture 7+1 skipped), including
  `PinRoslynAtTheConsumerCompilerFloor`.
- Audit prints `Repository passes` (advisory warnings only).
- If test fails with `Run "dotnet tool restore"`, clear `~/.dotnet/toolResolverCache` (see the environment note above).

## Review

skip
## Session

- Created: ffba6066-c22c-42a7-95c8-de75506a6687 (2026-11-06)
- Implementation: claude code session (2026-11-06)
- Implement node (ganda task work, 2026-11-06): no new product changes; re-ran audit, build, and test (green);
  added Goal restatement and How to validate; fixed a stale global dotnet tool resolver cache (environment only).
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 40d566d8-b396-4a7e-b700-9a6c3eeed7aa, max-turns 500, Done — 2026-11-06T13:46:24Z
