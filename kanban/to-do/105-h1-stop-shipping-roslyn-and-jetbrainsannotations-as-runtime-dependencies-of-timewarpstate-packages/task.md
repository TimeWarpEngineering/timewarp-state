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

- [ ] Move/condition the Roslyn reference (`Directory.Build.props:39`)
- [ ] `JetBrains.Annotations` private (`source/timewarp-state/timewarp-state.csproj:39`)
- [ ] Nuspec dependency allow-list check (test or `dev pack` step) for all packed packages
- [ ] Verify analyzer/generator still load in consumers and the 4.14.0 Roslyn floor (task 101) still holds
- [ ] Code review

## Acceptance criteria

- `dotnet pack` output: the `TimeWarp.State` and `TimeWarp.State.Blazor` nuspecs list neither
  `Microsoft.CodeAnalysis.CSharp` nor `JetBrains.Annotations`.
- The allow-list check fails when an unexpected dependency is added (demonstrated in the PR) and passes on master.
- Build, tests and `ganda repo audit` green.

## Session

- Created: 2026-10-10 (Grok Bot, at Steven's request via Amina; not launched)

## Notes

- Source: 102 review-findings.md finding H1. Related batch: 109 (Medium findings).
