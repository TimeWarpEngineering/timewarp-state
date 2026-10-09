# Backfill Purpose and Design regions on all C# files and enable TWA0004

## Description

This task backfills a `#region Purpose` block and a `#region Design` block at the top of every tracked `.cs`
file that is missing them (Part 1).

Part 2, turning on TWA0004 ("Source file lacks a #region Purpose block") via TimeWarp.Architecture.Analyzers,
was moved on 2026-10-09 to child task **099-001** ("Enable TWA0004 (Purpose region analyzer) via
TimeWarp.Architecture.Analyzers"), because Steven set one kanban task per PR. The task title still says "and
enable TWA0004" because the folder name carries it.

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

Moved to task 099-001, with its requirements, checklist and TWA rule review.

## Checklist

PRs: library source (#618), test app (#619), other tests (#620), then samples, scripts, .githooks and
tools (this task's final PR). Part 2 is task 099-001. Counts are files missing at least one region (see region-audit.md).

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

- [x] `samples` (42)
- [x] `scripts` (7)
- [x] `.githooks` (5)
- [x] `tools` (1)
- [x] Re-run the audit: 0 files missing Purpose and 0 missing Design. Workflow green.

### Part 1 status

**Part 1 (the backfill) is complete.** The repo-wide header audit reads 0 of 387 tracked `.cs` files missing
Purpose or Design.

### Part 2: enable TWA0004

Moved to task 099-001.

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

### PR 3: samples, scripts, .githooks and tools (final backfill PR)

- Added the missing regions to the last 55 files: samples 42, scripts 7, .githooks 5, tools 1. 44 got both
  Purpose and Design. 11 already had Purpose and got Design only, with their Purpose unchanged (10 under
  samples/04-telemetry/apphost, 05-persistence and 06-render-control, plus tools/dev-cli/global-usings.cs).
- Runfiles: the 5 hooks and 6 scripts start with a `#!` shebang and `#:package`/`#:property` lines. Those
  lines stay first; the regions go after them and the blank line that follows. That matches
  `tools/dev-cli/dev.cs`, the one runfile that already had regions (shebang first, regions further down).
- The header audit now treats leading `#!` and `#:` lines as header, not code. Before, it stopped at the
  shebang, so it counted dev.cs as missing even though dev.cs already has both regions. That is why the old
  script read 56 missing, not the 55 in region-audit.md.
- Repo-wide header audit of all 387 tracked `.cs` files, counting files missing Purpose or Design:
  - Before: 55 (44 with neither, 11 with Purpose only)
  - After: **0**. source 138/138, tests 171/171, samples 58/58, scripts 7/7, .githooks 5/5, tools 8/8.
- Comments only. `git diff` shows 55 files changed, 442 insertions, 0 deletions (none of these files has a BOM).
  The byte-level check (strip the shebang/directive header, remove the inserted region block, compare with
  HEAD) found 0 mismatches over the 55 files, 11 of them with a header. Line endings are kept (29 CRLF, 26 LF).
  No files outside samples/, scripts/, .githooks/ and tools/ changed, apart from this task.md.
- `dotnet run --file tools/dev-cli/dev.cs -- workflow`: Pipeline SUCCEEDED (assert-version-ssot, clean, build,
  test, e2e, pack, verify-samples). verify-samples ran 11 sample project builds, 0 errors.
  - Unit and integration tests: 228 passed, 4 skipped, 0 failed (analyzer 33, source-generator 15, core 73+1,
    plus 31+1, telemetry 13, client-integration 56+1, architecture 7+1).
  - E2E (Playwright): 11 passed, 3 skipped, 0 failed.
  - The workflow doesn't build every runfile, so each of the 12 was built with `dotnet build <file>`
    (scripts 6, .githooks 5, tools/dev-cli/dev.cs): all exit 0, 0 warnings, 0 errors.
- Things the region text describes as-is rather than fixes:
- `scripts/build.cs` `clean` route: runs `pkill -f dotnet`, which kills every process whose command line contains
  "dotnet" (other builds, IDE language servers), not just this repo's. `scripts/clean.cs` doesn't do this.
- `scripts/build.cs`: ends with "Packages available in: ./artifacts/packages", but it only builds; nothing is
  packed there.
- `scripts/e2e.cs`: `runMode` is hard-coded to "Auto", so the Manual, Development and Release branches can't run.
  If they could, the Development and Release branches `await` a `dotnet run` of the SUT, which blocks until the
  server exits, so the tests would never start.
- `scripts/e2e.cs` and `scripts/run-test-app.cs`: most `if (exitCode != 0) Environment.Exit(1)` checks can't
  fire. The comments in these same files say this Amuru line throws on a non-zero exit instead of returning it.
- `scripts/build.cs` `clean` and `scripts/clean.cs` are two different "clean" implementations: only build.cs
  removes the generated JS, and only clean.cs removes bin/obj, LocalNugetFeed and tests/test-app/output.
- `.githooks/*.cs` are ganda-managed hooks. `ganda repo audit` (memsearch-scaffold, an advisory warning) already
  listed post-commit, post-merge and post-checkout as "outdated" on master. With the regions, pre-commit and
  pre-push are listed too, because they no longer match ganda's template. The audit still passes. But
  `ganda repo audit --fix` or `ganda hooks install attest` would likely rewrite the hooks and drop the regions,
  and the region audit would then flag them again. Either ganda's hook template should carry the regions, or
  the region audit should exempt `.githooks/`.

## Notes

- **Trivial files:** generated or designer files and `global-usings.cs` may justify a one-line Purpose (and
  possibly no Design). Decide how to treat them before Part 1. TWA0004's own guidance is that "trivial files
  use a one-line Purpose rather than being exempt". It already skips `.g.cs`, `.generated.cs`,
  `.designer.cs` and Razor/cshtml generated files.
- The Part 2 notes (enforcement today, TWA rule review, file-based apps) moved to task 099-001.
- Steven asked for this task to be created only at first. He approved starting it on 2026-10-09, beginning
  with a PR for the library source (`source/`).

## Session

- Created: 974842 (2026-10-09)
