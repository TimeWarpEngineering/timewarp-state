# Enable TWA0004 (Purpose region analyzer) via TimeWarp.Architecture.Analyzers

## Description

Child of task 099. 099 backfilled `#region Purpose` and `#region Design` on every tracked `.cs` file (its Part 1;
the last PR covers samples, scripts, .githooks and tools). This task is 099's former Part 2: turn on TWA0004
("Source file lacks a #region Purpose block") by referencing the TimeWarp.Architecture.Analyzers package, so
Purpose regions can't drift again.

Start after 099's final backfill PR is merged, so the repo-wide region audit reads 0 missing on master.

## Requirements

- Add `<PackageVersion Include="TimeWarp.Architecture.Analyzers" Version="…" />` to
  `Directory.Packages.props`. Use the latest available version: 2.0.0-beta.9 or newer is required, and
  2.0.0-beta.19 is the latest on nuget.org as of 2026-10-09. Re-check when doing the work.
- Reference it in the shared root `Directory.Build.props`, in the existing "Code Analyzers" ItemGroup, with
  `PrivateAssets="all"` so it does not flow into the published packages.
- Add TWA settings to `.editorconfig`:
  - `dotnet_diagnostic.TWA0004.severity = warning`, or `error` once 099's backfill is merged.
  - Set every other TWA rule to `none` unless it fits this library (see the rule review in Notes), so
    enabling the package doesn't flood the build with unrelated diagnostics.
- TWA0004 checks **only Purpose**. The Design region stays manual and is enforced by reviewers.
- Build stays green, with no new warnings except intended TWA0004 hits, and none once 099's backfill is merged.
- Runfiles (`scripts/*.cs`, `tools/dev-cli/dev.cs`) start with a `#!` shebang and `#:` directives, and their
  regions come right after those lines. Check that TWA0004 accepts that layout, or scope the analyzer away
  from runfiles.
- Exclude `.githooks/` from TWA0004. ganda owns `.githooks/*.cs` byte for byte from its repo-baseline
  template, which has no `#region` blocks, and `ganda hooks install attest` overwrites any hook that differs,
  so these files can never carry a Purpose region. Add an `.editorconfig` section:

  ```ini
  [.githooks/**.cs]
  dotnet_diagnostic.TWA0004.severity = none
  ```

  The hooks already set `#:property RunAnalyzers=false`, so no analyzer runs when git runs them. The
  `.editorconfig` section covers IDEs and any other build that loads the analyzer for those files.

## Checklist

- [ ] Add a `TimeWarp.Architecture.Analyzers` PackageVersion (latest, ≥ 2.0.0-beta.9) in `Directory.Packages.props`
- [ ] Add the PackageReference (`PrivateAssets="all"`) in the root `Directory.Build.props` "Code Analyzers" ItemGroup
- [ ] `.editorconfig`: `dotnet_diagnostic.TWA0004.severity = warning` (or `error`)
- [ ] `.editorconfig`: set the TWA rules that don't fit to `none` (review list in Notes); record the decision for each rule
- [ ] `.editorconfig`: `[.githooks/**.cs]` section with `dotnet_diagnostic.TWA0004.severity = none` (ganda-owned hook templates)
- [ ] Check which projects actually get the analyzer (library, tests, samples with their own props, file-based scripts/.githooks/tools) and that it's PrivateAssets in every packed nupkg
- [ ] Workflow green with no TWA warnings; packed nupkgs carry no dependency on TimeWarp.Architecture.Analyzers

## Notes

- Moved here from task 099 (Part 2) on 2026-10-09, when Steven set one kanban task per PR.
- **Enforcement today:** TWA0004 ships in the TimeWarp.Architecture.Analyzers package. timewarp-state does
  not reference that package in any props or csproj, so nothing enforces the regions today. This task fixes
  that for Purpose only. TWA0004 does not check Design, so Design stays a manual, reviewer-enforced
  convention. A Design check could be proposed upstream later.
- **Rule review.** Rule ids and titles come from TimeWarp.Architecture.Analyzers 2.0.0-beta.19
  (2026-10-09). Most rules target TimeWarp.Architecture apps (FluentValidation, FastEndpoints, Aspire,
  slices, the dotnet-new template), not this library. Proposed default is `none`, except TWA0004:
  - TWA0002 (nullable property has a presence validation rule) and TWA0003 (required property has a fabricated
    empty default) are FluentValidation rules: none.
  - TWA0004 (source file lacks a #region Purpose block): **warning/error**.
  - TWA0005: id present in the package, but no title was found in this quick review. Check it when doing
    the work; default none.
  - TWA0006 (routed contract has no server endpoint), TWA0013 and TWA0014 (generated endpoint auth posture),
    TWA0020 ([ApiEndpoint] with [ClientOnlyContract]) and TWA0024 ([EndpointAuthorize] policy not registered)
    are FastEndpoints/contract rules: none.
  - TWA0007 (Aspire resource name is not a ServiceNames constant): none. sample-04 uses Aspire but not
    ServiceNames.
  - TWA0008 and TWA0010 (dotnet-new template conditional tokens and flags) are template-repo only: none.
  - TWA0009 (slice references another product slice): none. This library is not sliced.
  - TWA0011 and TWA0012 (aggregate root Invariants validator) are domain-model rules: none.
  - TWA0015 and TWA0016 (feature filename function/layer grammar): none. The library's feature files don't
    follow `<name>[-<function>]-<layer>.cs`.
  - TWA0021 (mock auth registration) is a SPA auth rule: none.
  - TWA0022 (SPA client code must not call the mediator's Send directly): consider it for the samples and
    test-app, since it pushes dispatch through TimeWarp.State's generated ActionSet methods. It would hit
    the library's own internals, so it likely stays none (or is scoped to samples).
  - TWA0023 (identifier does not use the type stem) is a naming style rule. Evaluate the hit count; likely none.
- **File-based apps:** `scripts`, `.githooks` and `tools` `.cs` files are file-based apps. Confirm whether the
  analyzer reaches them through Directory.Build.props. 099 backfilled regions in `scripts` and `tools`.
  `.githooks` is out of scope: 099's region audit excludes it, and PR #621 synced the hooks to ganda's template
  with `ganda hooks install attest`.

## Session

- Created: 1253659 (2026-10-09)
