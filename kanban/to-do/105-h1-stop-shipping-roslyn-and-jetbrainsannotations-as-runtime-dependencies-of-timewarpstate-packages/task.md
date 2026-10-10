# Task 105: H1: Stop shipping Roslyn and JetBrains.Annotations as runtime dependencies of TimeWarp.State packages

## Description

The published `TimeWarp.State` and `TimeWarp.State.Blazor` packages declare a runtime dependency on
`Microsoft.CodeAnalysis.CSharp` (and `TimeWarp.State` on `JetBrains.Annotations`), so every consumer,
Blazor WebAssembly included, restores Roslyn. This is first in Fable's recommended order.

Filed 2026-10-10 at Steven's request (relayed by Amina) from Claude Fable's full codebase review,
task 102 (`kanban/done/102-full-codebase-review-by-claude-fable-review-only/`, PR #629, `00ade373`).
Not launched.

## Finding H1 (Fable review)

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 26-32 (PR #629, merged as `00ade373`):

> ### H1. `TimeWarp.State` and `TimeWarp.State.Blazor` ship a runtime dependency on `Microsoft.CodeAnalysis.CSharp`
>
> - `Directory.Build.props:39` adds `<PackageReference Include="Microsoft.CodeAnalysis.CSharp" />` to every project without `PrivateAssets="all"`. The comment above it explains the version floor for the analyzer and generator, but the reference is applied to the runtime libraries too.
> - Verified in the local NuGet cache: `timewarp.state/12.0.0-beta.11/timewarp.state.nuspec` lists `Microsoft.CodeAnalysis.CSharp 4.14.0` and `JetBrains.Annotations 2026.2.0` as dependencies; `timewarp.state.blazor/12.0.0-beta.10` lists `Microsoft.CodeAnalysis.CSharp 4.14.0` as well. Task 037's Results record the same list.
> - Why it matters: every consumer restores and deploys the Roslyn compiler (plus `Microsoft.CodeAnalysis` and `System.Collections.Immutable` pins) for a state-management library. For Blazor WebAssembly that is download size and trimming work for nothing. It also pins consumers to a Roslyn version band and can cause `NU1605`/`NU1608` conflicts in apps that reference newer Roslyn packages.
> - `JetBrains.Annotations` (`source/timewarp-state/timewarp-state.csproj:39`) is a compile-time-only package and should also be `PrivateAssets="all"`.
> - Suggestion: move the `Microsoft.CodeAnalysis.CSharp` reference into the analyzer and generator csproj files (they are the only ones that need it), or keep it in `Directory.Build.props` with `PrivateAssets="all"` and a `Condition` on the two netstandard2.0 projects. Mark `JetBrains.Annotations` `PrivateAssets="all"`. Add a packaging test (or a `dev pack` step) that fails when a nuspec dependency list contains anything outside an allow-list, so this cannot regress silently.

## Order

Fable's recommended order (verbatim, `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md`, Overall assessment):

> Recommended order of attack: the package dependency (one line), the transaction rollback guard (one `ReferenceEquals` check), a `[CloneShared]`-style attribute or equivalent for injected members, then the DevTools and persistence dead paths, then `IsAotCompatible` on the two remaining runtime packages.

Filed tasks in that order: 105 (H1, package dependency) -> 106 (H2, rollback guard) -> 107 (H3, share-by-reference attribute) -> 104 and 109 M1/M9 (dead DevTools and persistence paths) -> 109 M2 (AOT checks on Blazor and Plus). 108 (H4) and the remaining Medium items are not in Fable's ordered list; schedule them after 107.

## Requirements

- Reference `Microsoft.CodeAnalysis.CSharp` only from the analyzer and source-generator projects (or keep it
  in `Directory.Build.props` with `PrivateAssets="all"` conditioned to those netstandard2.0 projects).
- `JetBrains.Annotations` is `PrivateAssets="all"`.
- A packaging test or `dev pack` step fails when any nuspec dependency list contains a package outside an allow-list.

## Checklist

- [x] Move/condition the Roslyn reference (`Directory.Build.props:39`)
- [x] `JetBrains.Annotations` private (`source/timewarp-state/timewarp-state.csproj:39`)
- [x] Nuspec dependency allow-list check (test or `dev pack` step) for all packed packages
- [x] Verify analyzer/generator still load in consumers and the 4.14.0 Roslyn floor (task 101) still holds
- [x] Code review

## Acceptance criteria

- `dotnet pack` output: the `TimeWarp.State` and `TimeWarp.State.Blazor` nuspecs list neither
  `Microsoft.CodeAnalysis.CSharp` nor `JetBrains.Annotations`.
- The allow-list check fails when an unexpected dependency is added (demonstrated in the PR) and passes on master.
- Build, tests and `ganda repo audit` green.

## Results

`Microsoft.CodeAnalysis.CSharp` is a compile reference of the analyzer, the source generator, and their test projects, each with `PrivateAssets="all"`. `Directory.Build.props` no longer adds that package to every project. The central pin stays `4.14.0` (task 101 consumer compiler floor: 5.9.0 needs SDK 10.0.400+).

`JetBrains.Annotations` is `PrivateAssets="all"` on `TimeWarp.State` and on `TimeWarp.State.Plus` (`[UsedImplicitly]`). Unused `global using JetBrains.Annotations` lines were removed from Blazor, client integration tests, and the test-app client.

`dev pack` reads each packed nuspec and exits 1 when a dependency id is outside `NuspecDependencyAllowList` (`tools/dev-cli/endpoints/nuspec-dependency-allow-list.cs`). The same type is linked into `timewarp-state-tests`. The check compares ids, so an allowed dependency can change version without a failure. A package id with no entry fails closed. Version stays `12.0.0-beta.11`.

Release nupkgs in `artifacts/packages` declare these dependency ids:

- TimeWarp.State: TimeWarp.Mediator.Contracts. Analyzer payload: `analyzers/dotnet/cs/timewarp-state-analyzer.dll`, `analyzers/dotnet/cs/timewarp-state-source-generator.dll`.
- TimeWarp.State.Blazor: TimeWarp.State, Microsoft.AspNetCore.Components.Web
- TimeWarp.State.Plus: TimeWarp.State.Blazor, TimeWarp.State, Blazored.LocalStorage, Blazored.SessionStorage, Microsoft.AspNetCore.Components.Web
- TimeWarp.State.Policies: TimeWarp.State, NetArchTest.eNhancedEdition, Shouldly
- TimeWarp.State.Telemetry: TimeWarp.State, TimeWarp.Mediator.Contracts

None of those nuspecs list `Microsoft.CodeAnalysis.CSharp` or `JetBrains.Annotations`.

`NuspecDependencyAllowList_Should_` rejects those two ids on every packed package, rejects a package id with no allow-list entry, and rejects a nuspec that adds `Microsoft.CodeAnalysis.CSharp` beside `TimeWarp.Mediator.Contracts`. `PinRoslynAtTheConsumerCompilerFloor` locks the `4.14.0` package version and requires `Directory.Build.props` to omit a repo-wide Roslyn `PackageReference`. `dotnet run --file tools/dev-cli/dev.cs -- pack` printed the five allow-list lines and `Packed set verified`.

A throwaway `net11.0` consumer of `TimeWarp.State` 12.0.0-beta.11, restored from a fresh `--packages` folder and the local `artifacts/packages` feed, compiled a public `State<T>` and reported `TWS001` for a state type with a private constructor. Its assets file lists both analyzer assemblies and does not list `Microsoft.CodeAnalysis.CSharp` or `JetBrains.Annotations`.

`./bin/dev test` exited 0 (`Tests completed successfully!`). `ganda repo audit` exited 0: 29 passed, 1 advisory failure, 1 skipped. The advisory is the pre-existing `kebab-path-names` warning on five `*.lib.module.*` paths (Blazor JS module names). Banner: `Repository passes — 1 advisory (non-blocking) warning(s).`

### How to validate

Smoke:

- `dotnet run --file tools/dev-cli/dev.cs -- pack`
- `dotnet run --file scripts/test.cs`
- `ganda repo audit`
- Optional consumer: a `net11.0` project that references `TimeWarp.State` 12.0.0-beta.11 from `artifacts/packages` only (`dotnet build --packages <fresh-folder>`), with one valid `State<T>` and one state whose only constructor is private.

Expect:

- Pack prints `TimeWarp.State dependencies: TimeWarp.Mediator.Contracts`, the Blazor, Plus, Policies, and Telemetry lines from Results, then `Packed set verified`. Unzipped nuspecs contain neither `Microsoft.CodeAnalysis.CSharp` nor `JetBrains.Annotations`. `TimeWarp.State` still contains both analyzer assemblies under `analyzers/dotnet/cs/`.
- `scripts/test.cs` exits 0, including `NuspecDependencyAllowList_Should_` (rejection of Roslyn and JetBrains.Annotations, and the 4.14.0 pin).
- `ganda repo audit` exits 0.
- The valid state builds. The private-constructor state fails with `TWS001`. The consumer assets file does not list `Microsoft.CodeAnalysis.CSharp` or `JetBrains.Annotations`.

Run pack through `dotnet run --file tools/dev-cli/dev.cs -- pack`. The locally built `./bin/dev` binary (not tracked by git) is built separately and can lag the allow-list check in source.

### Review disposition

- Rounds: 1. Effort 2, roster: general (claude review oracle).
- Final counts: bug 0, suggestion 0, nit 2 fixed (M1: XML-based Roslyn guard in `PinRoslynAtTheConsumerCompilerFloor`; M2: `bin/dev` wording). Open: 0, wontfix: 0.
- Disposition: **clean**.
- Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Session

- Created: 2026-10-10 (Grok Bot, at Steven's request via Amina; not launched)
- Implementation: grok task-work implementer (2026-10-10)
- Review: claude review oracle, ganda task work (2026-10-10)
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 120 — 2026-10-10T16:53:20Z

## Notes

- Source: 102 review-findings.md finding H1. Related batch: 109 (Medium findings).
