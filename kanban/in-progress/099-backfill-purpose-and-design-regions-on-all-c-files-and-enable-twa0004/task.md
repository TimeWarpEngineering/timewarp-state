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

- [x] `source/timewarp-state` (48)
- [x] `source/timewarp-state-plus` (29)
- [x] `source/timewarp-state-policies` (9)
- [x] `source/timewarp-state-analyzer` (6)
- [x] `source/timewarp-state-source-generator` (4)
- [x] `source/timewarp-state-telemetry` (4)

Tests:

- [x] `tests/test-app` (72)
- [x] `tests/client-integration-tests` (19)
- [x] `tests/test-app-end-to-end-tests` (18)
- [x] `tests/timewarp-state-tests` (14)
- [x] `tests/timewarp-state-plus-tests` (12)
- [x] `tests/timewarp-state-analyzer-tests` (11)
- [x] `tests/timewarp-state-telemetry-tests` (9)
- [x] `tests/timewarp-state-source-generator-tests` (6)
- [x] `tests/test-app-architecture-tests` (4)

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

## Results

### PR 1: library source (`source/`)

- Added the missing regions to 100 files under `source/`, 6 projects (48 + 29 + 9 + 6 + 4 + 4). 96 files got both
  Purpose and Design. 4 files already had Purpose and got Design only, with the existing Purpose unchanged:
  `equatable-array.cs`, `is-external-init.cs`, telemetry `service-collection-extensions.cs` and
  `timewarp-state-telemetry.cs`.
- Audit of the tracked `source/**/*.cs` files (138): 100 were missing at least one region before, and 0 are
  missing either region after.
- Comments only. `git diff` shows 893 insertions and 13 deletions. The 13 deletions are files with a UTF-8 BOM,
  where the BOM moves from the old first line to the new `#region Purpose` line. A byte-level check compared
  each file against HEAD with the BOM and the inserted region blocks stripped. It found 0 mismatches over the
  100 files. Every added line is `#region Purpose`, `#region Design`, `#endregion`, a `// ` comment or blank.
- `dotnet run --file tools/dev-cli/dev.cs -- workflow` passed: Pipeline SUCCEEDED (build, test, e2e, pack,
  verify-samples).
  - Unit and integration tests: 228 passed, 4 skipped, 0 failed (analyzer 33, source-generator 15, core 73 + 1
    skipped, Plus 31 + 1 skipped, Telemetry 13, client integration 56 + 1 skipped, architecture 7 + 1 skipped).
  - E2E: 11 passed, 3 skipped, 0 failed.
  - The first local run failed only in E2E. The Playwright chromium-headless-shell v1234 browser was missing,
    and `--with-deps` needed sudo. Installing the browser into the WSL user cache fixed it, with no code change.
- Open items flagged while describing the code (described as-is, not changed):
  - `TimeWarpCacheableState<TState>` derives from `State<TimeWarpCacheableState<TState>>`, not from
    `State<TState>`.
  - `FeatureFlagState` is a placeholder whose Initialize throws NotImplementedException.
  - The `InvalidCloneException` message still points at a parameterless-constructor requirement, but it is
    thrown when the clone's Guid is empty or unchanged.

### PR 2a (part 2a/n): test app (`tests/test-app/`)

- Added the missing regions to all 72 files under `tests/test-app/` that lacked them (client 66, contracts 4,
  server 2). 71 got both Purpose and Design. `counter-state.add-to-count.cs` already had Purpose and got Design
  only, with its Purpose unchanged.
- Header audit of tracked `tests/test-app/**/*.cs` (76 files), counting files missing Purpose or Design:
  - Before: 72 (71 with neither, 1 with Purpose only)
  - After: 0
  - All of `tests/` (171 files) goes from 165 missing to 93; those 93 are PR 2b.
- Comments only. `git diff` shows 626 insertions and 4 deletions. The 4 deletions are the 4 files with a UTF-8
  BOM, where the BOM moves to the new `#region Purpose` line. The byte-level check (strip the BOM and the
  inserted region blocks, compare with HEAD) found 0 mismatches over the 72 files. Line endings are kept (71 LF,
  1 CRLF). No files outside `tests/test-app/` changed, apart from this task.md.
- `dotnet run --file tools/dev-cli/dev.cs -- workflow`: Pipeline SUCCEEDED (assert-version-ssot, clean, build,
  test, e2e, pack, verify-samples).
  - Unit and integration tests: 228 passed, 4 skipped, 0 failed (analyzer 33, source-generator 15, core 73+1,
    plus 31+1, telemetry 13, client-integration 56+1, architecture 7+1).
  - E2E (Playwright): 11 passed, 3 skipped, 0 failed.
- Things the region text describes as-is rather than fixes:
  - `ColorState.Hydrate` reads `MyColorName` from the `FavoriteColor` key.
  - `PreIncrementCountNotificationHandler` logs `nameof(IncrementCountNotificationHandler)`.
  - `ThrowServerSideExceptionActionSet` calls a route the test server never maps, so the "server-side exception"
    is the failed HTTP call, and `action.Message` is never sent.
  - `ColorState`, `UpdateColorState` (empty), `WindowDimensionsState`, `TestEnum` and `MyBehavior` are not
    used anywhere.
  - `CloneTestPage` doesn't run the 2D/3D array cases or `ModifiedClone_InterfaceObject` from
    `CloneProviderTests`.

### PR 2b (part 2b/n): test projects (`tests/` outside `tests/test-app/`)

- Added the missing regions to the remaining 93 files under `tests/`: client-integration 19, end-to-end 18, core
  14, plus 12, analyzer 11, telemetry 9, source-generator 6, architecture 4. 71 got both Purpose and Design. 22
  already had Purpose and got Design only, with their Purpose unchanged.
- Header audit of tracked `tests/**/*.cs` (171 files), counting files missing Purpose or Design:
  - Before: 93 (71 with neither, 22 with Purpose only)
  - After: 0. `tests/` is done.
- Comments only. `git diff` shows 762 insertions and 10 deletions. The 10 deletions are the 10 files with a UTF-8
  BOM, where the BOM moves to the new `#region Purpose` line. The byte-level check (strip the BOM and the
  inserted region blocks, compare with HEAD) found 0 mismatches over the 93 files. Line endings are kept (88 LF,
  5 CRLF). No files outside these 8 projects changed, apart from this task.md.
- `dotnet run --file tools/dev-cli/dev.cs -- workflow`: Pipeline SUCCEEDED (assert-version-ssot, clean, build,
  test, e2e, pack, verify-samples).
  - Unit and integration tests: 228 passed, 4 skipped, 0 failed (analyzer 33, source-generator 15, core 73+1,
    plus 31+1, telemetry 13, client-integration 56+1, architecture 7+1). Same as PR 2a.
  - E2E (Playwright): 11 passed, 3 skipped, 0 failed.
- Things the region text describes as-is rather than fixes:
- `test-app-end-to-end-tests/persistence-test-page-tests.cs`: the `[Ignore]` reason says Playwright chromium
  can't be installed, but the other E2E tests run in chromium now (11 pass), so the reason looks stale. This is
  the third skipped E2E test.
- `timewarp-state-analyzer-tests/state-read-only-public-properties-analyzer-tests.cs`: the whole file is
  commented out. Its tests expected a Warning, and the rule is now an Error.
- `timewarp-state-analyzer-tests/timewarp-state-action-analyser-tests.cs`: comments say net10 but the tests use
  the Net110 references. The file name also spells "analyser".
- `timewarp-state-plus-tests/testing-convention.cs`: uses namespace `TimeWarp.State.Tests`, the same as the core
  test project.
- `timewarp-state-plus-tests/features/routing/go-back-repro-tests.cs`: marked "Temporary repro", and its comments say the seed
  order is uncertain.
- `client-integration-tests/pipeline/state-transaction-tests.cs` `RollbackState_OnException`: `initialGuid` is assigned but never used.
- `timewarp-state-tests/type-extensions-tests.cs`: the "Deeply_Nested" cases use a derived class at the same
  nesting level, not a more deeply nested type.
- `convention-tests.cs` (the Fixie convention samples) is copied into 4 projects; its `[Skip]` `SkipExample` is all
  4 of the skipped unit/integration tests.
- `test-app-end-to-end-tests/sample-test.cs`: the ignored playwright.dev samples are 2 of the 3 skipped E2E tests.

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
- Steven asked for this task to be created only at first. He approved starting it on 2026-10-09, beginning
  with a PR for the library source (`source/`).

## Session

- Created: 974842 (2026-10-09)
