# Backfill Purpose and Design regions on all C# files and enable TWA0004

## Description

This task has two parts:

- **Part 1:** backfill a `#region Purpose` block and a `#region Design` block at the top of every tracked
  `.cs` file that is missing them.
- **Part 2:** once the backfill is done, turn on TWA0004 ("Source file lacks a #region Purpose block") by
  referencing the TimeWarp.Architecture.Analyzers package, so Purpose regions can't drift again.

The audit was run on 2026-10-09 against master after the 12.0.0-beta.9 merge (`29990f46`). It found 387
`.cs` files:

| Status | Files |
|---|---|
| Both regions | 67 |
| Purpose, no Design | 38 |
| Design only | 0 |
| Neither | 282 |

That leaves **320 files missing at least one region**. The full per-file list, grouped by area in work
order, is in [region-audit.md](region-audit.md), next to this task.

## Requirements

### Part 1: region backfill

- Put the regions at the top of the file, before the namespace, in the existing style. The reference is
  `source/timewarp-state/features/cloning/deep-cloner.cs`:

  ```csharp
  #region Purpose
  // What this file is for, in a sentence or two.
  #endregion

  #region Design
  // How it does it and why: key decisions and constraints.
  #endregion
  ```

- Keep each region short, a sentence or two.
- The first pass through a file means working out its purpose and design from the code itself. Accurate is
  better than generic, and nothing may be invented. If the intent is unclear from the code, write what it
  verifiably does.
- Comments only: no behavior, formatting or API changes.
- Build and tests stay green (`dotnet run --file tools/dev-cli/dev.cs -- workflow`).

### Part 2: enable TWA0004

- Add `<PackageVersion Include="TimeWarp.Architecture.Analyzers" Version="…" />` to
  `Directory.Packages.props`. Use the latest available version: 2.0.0-beta.9 or newer is required, and
  2.0.0-beta.19 is the latest on nuget.org as of 2026-10-09. Re-check when doing the work.
- Reference it in the shared root `Directory.Build.props`, in the existing "Code Analyzers" ItemGroup, with
  `PrivateAssets="all"` so it does not flow into the published packages.
- Add TWA settings to `.editorconfig`:
  - `dotnet_diagnostic.TWA0004.severity = warning`, or `error` once the backfill is complete.
  - Set every other TWA rule to `none` unless it fits this library (see the rule review in Notes), so
    enabling the package doesn't flood the build with unrelated diagnostics.
- TWA0004 checks **only Purpose**. The Design region stays manual and is enforced by reviewers.
- Build stays green, with no new warnings except intended TWA0004 hits, and none once Part 1 is complete.

## Checklist

Suggested PR split: one PR per area group. Use (1) library source, (2) tests, (3) samples, scripts,
.githooks and tools, then (4) Part 2. Counts are files missing at least one region (see region-audit.md).

### Part 1: Purpose/Design backfill

Library source (first):

- [ ] `source/timewarp-state` (48)
- [ ] `source/timewarp-state-plus` (29)
- [ ] `source/timewarp-state-policies` (9)
- [ ] `source/timewarp-state-analyzer` (6)
- [ ] `source/timewarp-state-source-generator` (4)
- [ ] `source/timewarp-state-telemetry` (4)

Tests:

- [ ] `tests/test-app` (72)
- [ ] `tests/client-integration-tests` (19)
- [ ] `tests/test-app-end-to-end-tests` (18)
- [ ] `tests/timewarp-state-tests` (14)
- [ ] `tests/timewarp-state-plus-tests` (12)
- [ ] `tests/timewarp-state-analyzer-tests` (11)
- [ ] `tests/timewarp-state-telemetry-tests` (9)
- [ ] `tests/timewarp-state-source-generator-tests` (6)
- [ ] `tests/test-app-architecture-tests` (4)

Other:

- [ ] `samples` (42)
- [ ] `scripts` (7)
- [ ] `.githooks` (5)
- [ ] `tools` (1)
- [ ] Re-run the audit: 0 files missing Purpose and 0 missing Design. Workflow green.

### Part 2: enable TWA0004 (after Part 1)

- [ ] Add a `TimeWarp.Architecture.Analyzers` PackageVersion (latest, ≥ 2.0.0-beta.9) in `Directory.Packages.props`
- [ ] Add the PackageReference (`PrivateAssets="all"`) in the root `Directory.Build.props` "Code Analyzers" ItemGroup
- [ ] `.editorconfig`: `dotnet_diagnostic.TWA0004.severity = warning` (or `error`)
- [ ] `.editorconfig`: set the TWA rules that don't fit to `none` (review list in Notes); record the decision for each rule
- [ ] Check which projects actually get the analyzer (library, tests, samples with their own props, file-based scripts/.githooks/tools) and that it's PrivateAssets in every packed nupkg
- [ ] Workflow green with no TWA warnings; packed nupkgs carry no dependency on TimeWarp.Architecture.Analyzers

## Notes

- **Enforcement today:** TWA0004 ships in the TimeWarp.Architecture.Analyzers package. timewarp-state does
  not reference that package in any props or csproj, so nothing enforces the regions today. Part 2 fixes
  this for Purpose only. TWA0004 does not check Design, so Design stays a manual, reviewer-enforced
  convention. A Design check could be proposed upstream later.
- **Trivial files:** generated or designer files and `global-usings.cs` may justify a one-line Purpose (and
  possibly no Design). Decide how to treat them before Part 1. TWA0004's own guidance is that "trivial files
  use a one-line Purpose rather than being exempt". It already skips `.g.cs`, `.generated.cs`,
  `.designer.cs` and Razor/cshtml generated files.
- **Rule review for Part 2.** Rule ids and titles come from TimeWarp.Architecture.Analyzers 2.0.0-beta.19
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
  analyzer reaches them through Directory.Build.props. Their regions are backfilled in Part 1 either way.
- Steven asked for this task to be created only. Work has not started.

## Session

- Created: 974842 (2026-10-09)
