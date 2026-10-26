# Shrink the local-feed lane to one packaged sample plus a nupkg content test

## Description

The local feed (`artifacts/packages`) exists to prove the packed `.nupkg` files are sound. Today ten sample projects consume it through `PackageReference`, while the e2e test-app already uses `ProjectReference`. Over the repo's history the packed-consumption lane caught four packaging bugs (static web assets dropped under SDK 11, Roslyn and JetBrains.Annotations leaking into packed dependencies, the `_content` script path after the Blazor split, and the packaged DevTools inspector), and it caused about six incidents (CI feed ordering, same-version repack shadowing, global-cache poisoning in issue 647 fixed by task 125).

One packaged consumer is enough to prove the package. The dependency allow-list is already a unit test (`tests/timewarp-state-tests/packaging/nuspec-dependency-allow-list-tests.cs`). Nothing asserts the static web assets or analyzer layout inside the `.nupkg`. Moving the other samples to `ProjectReference` gives faster inner-loop builds, no cold per-worktree caches for them, and no feed ordering.

### Regression from task 125 (fix first)

Since PR #657 merged, a fresh worktree cannot restore anything until `artifacts/packages` exists. `nuget.config` lists `LocalNuGetFeed` = `artifacts/packages`, and the isolated `globalPackagesFolder` is empty in a fresh worktree, so NuGet consults every source and fails with NU1301 on the missing folder. Before 125 the shared cache already held the packages and the missing source was never queried. Observed on 2026-10-26 claiming this task: the worktree on-create `bin/dev` build failed, and the pre-commit hook runfile failed to restore, so the first kitchen commit was refused. Workaround used: `mkdir -p artifacts/packages` in the worktree.

## Requirements

1. A fresh worktree restores and commits without manual steps. Any of: `nuget.config` stops listing the feed and the pack lane passes `--source` to the one packaged consumer; an MSBuild target or `Directory.Build.props` ensures the folder exists before restore; or the worktree on-create handler creates it. Prefer the option that also removes the feed from restores that do not need it.
2. Add a package content test that opens each packed `.nupkg` from `dev pack` and asserts the expected layout. TimeWarp.State.Blazor must carry its `_content` static web assets including the `.lib.module.js`. The analyzer and source-generator packages must place assemblies under `analyzers/dotnet/cs`. A missing entry fails the test.
3. Keep sample 00 auto (`samples/00-state-action-handler/auto`) on `PackageReference` via the local feed. It covers server, WebAssembly, analyzer and generator in one consumer.
4. Move the other nine sample projects to `ProjectReference`, matching `tests/test-app/test-app-client/test-app-client.csproj`.
5. Keep `tests/timewarp-state-devtools-tests/packaged-devtools-app-tests.cs` on the pack lane. The inspector runs from a NuGet package by design.
6. CI and `verify-samples` pack before the sample 00 auto restore only. Remove feed ordering from the other sample steps.
7. Update `samples/overview.md` and the contributing scripts section to state which consumer proves packaging and why the rest use project references.

## Checklist

- [x] Fresh worktree restore works with no manual folder creation (claim a scratch worktree or delete `artifacts/` and restore)
- [x] nupkg content test added and green against `dev pack` output
- [x] sample 00 auto stays on the local feed and still builds and runs
- [x] nine other samples on `ProjectReference`; `./bin/dev build` and `./bin/dev verify-samples` green
- [x] CI pack ordering reduced to the one packaged consumer
- [x] `./bin/dev test` green
- [x] docs updated
- [x] M1: purge same-version extracts and `--force` the sample 00 auto restore
- [x] M2: Plus content test requires both scripts
- [x] M3: content test parses static web asset manifests only

## Session

- Created: 259f62af-98bf-4133-8bba-2f8a32eb7caa (2026-10-26)
- Implementation: grok 01a1781a-5751-74c1-b158-536134dfae2d (2026-10-26). Regression fix is `00f3102f` (root `nuget.config` no longer lists the feed). The sample move, nupkg content test, and docs are the next commit. `Directory.Packages.props` dropped `TimeWarp.State.Plus` and `TimeWarp.State.Telemetry` because nothing package-references them after that move (`cpm-consistency`).
- Commands were `dotnet run --file tools/dev-cli/dev.cs`. `./bin/dev` is gitignored and was not the binary under test.
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session not reported, max-turns 500, Done — 2026-10-26T13:05:38Z
- Review oracle round 1: claude-opus-5-5, effort 3, roster general. 3 findings open (M1 bug, M2 suggestion, M3 nit). A fix pass is needed — 2026-10-26
- Review oracle: review by reviewer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-26T13:10:21Z
- Fix pass (implement-fix-2): grok, session 01a17855-5a5a-77e2-a698-de85fddcf079 — 2026-10-26. Purged the isolated same-version extracts before the sample 00 restore. Plus asserts both scripts. Manifest JSON skips contentFiles tsconfig.
- Audit: `ganda repo audit --fix --checks kanban-landed` landed kitchen 129. Two advisory warnings stay: task 098 has no worktree, and seven pre-existing non-kebab paths (Blazor `.lib.module` names, a `.targets` file, and backlog task 110). Those are not this change.
- Review oracle round 2: claude-opus-5-5, effort 2, roster general, session not reported. Re-verified M1–M3 as fixed and found nothing new. Disposition is clean — 2026-10-26

## Goal (implementer restatement)

A fresh worktree restores and commits with no hand-made `artifacts/packages` folder. Root `nuget.config` does not list that feed, so restores that never consume the local packages do not fail NU1301 when the folder is missing. Sample 00 auto (server host and WebAssembly client) is the one packaged consumer: `verify-samples` packs, then restores it through a config that maps `TimeWarp.State` and `TimeWarp.State.*` to `artifacts/packages`. The other nine samples compile against the source projects, the way the test app does, and CI does not order them behind the feed. The DevTools inspector test stays on its own pack lane. `samples/overview.md` and the contributing scripts section say which consumer proves packaging.

`dev pack` opens every `.nupkg`. `TimeWarp.State.Blazor` must contain its `_content` static web assets, including `.lib.module.js`. `TimeWarp.State.Plus` must contain both `document-title.js` and `download-file.js`, each paired with BasePath `_content/TimeWarp.State.Plus`. The analyzer and source generator assemblies must sit under `analyzers/dotnet/cs`. A missing entry fails the test. That test reads static web asset manifests only, so a comment in `contentFiles/.../tsconfig.json` is not a parse error.

Review round 1: before the sample 00 auto restore, delete `artifacts/nuget-packages/<id>/<version>` for each packable id and restore with `--force`. A same-version extract otherwise wins over the nupkg just packed, and the packaged-consumer gate can pass on stale bits. `RestorePackagesPath` names that isolated folder and wins over `NUGET_PACKAGES`, so a per-run env folder would not move the extract. Task 125's rule stays: the packed version is not written under `~/.nuget/packages`.

## Results

Root `nuget.config` no longer lists `artifacts/packages`. That is commit `00f3102f`, separate from the sample move. Nine samples that used to `PackageReference` TimeWarp.State packages now `ProjectReference` the source projects, plus the analyzer and source generator. Sample 04's AppHost was already a project reference. Sample 00 auto (host and WebAssembly client) stays on `PackageReference` and is out of `timewarp-state.slnx`, because `12.0.0-beta.16` is not on nuget.org. `verify-samples` builds the project-referenced samples, publishes sample 05 trimmed with `VerifySamplesTrim` (a global `PublishTrimmed` also marks the netstandard analyzer projects), packs, runs the nupkg content test, then restores sample 00 auto through `artifacts/sample-00-auto.nuget.config`.

`tests/timewarp-state-packaging-tests` opens every `.nupkg` in `artifacts/packages`. `dev pack` runs it after the nuspec allow-list. `dev test` does not, because the workflow tests before the pack. `TimeWarp.State` must contain `analyzers/dotnet/cs/timewarp-state-analyzer.dll` and `timewarp-state-source-generator.dll`. `TimeWarp.State.Blazor` must contain a fingerprinted `.lib.module.js` whose asset base path is `_content/TimeWarp.State`. The DevTools inspector test still packs its own temp feed.

`Directory.Packages.props` no longer versions `TimeWarp.State.Plus` or `TimeWarp.State.Telemetry`. Nothing in the repo package-references them after the sample move.

Review round 1 fix: `verify-samples` deletes `artifacts/nuget-packages/<lowercase id>/<version>` for each packable id after pack, then restores sample 00 auto with `--force`. Before this run the cached `TimeWarp.State` `contentHash` was `r4tHpL1d…` and the nupkg SHA-512 was `gFVoIVu2…`. After the run both were `sOKW/aPZWBV3TE2cpK03`. `TimeWarp.State.Blazor` matched the same way. The log printed both purged version folders, then `Samples verified successfully!`. `RestorePackagesPath` still names the isolated folder, so the extract does not move to `~/.nuget/packages`.

`TimeWarp.State.Plus` asserts `document-title.js` and `download-file.js` on their own. A package that has only the first script fails. `HasAssetPair` reads `build/`, `buildTransitive/`, and `*.PackageAssets.json`. A comment in `contentFiles/.../tsconfig.json` returns a layout miss instead of `JsonException`.

### How to validate

Smoke:

- `dotnet fixie timewarp-state-tests --tests '*NuGetPackageIsolation*'`
- `dotnet fixie timewarp-state-packaging-tests --tests '*NupkgContent*'`
- Record `artifacts/nuget-packages/timewarp.state/12.0.0-beta.16/.nupkg.metadata` `contentHash`, then `dotnet run --file tools/dev-cli/dev.cs -- verify-samples`, then compare that hash to the SHA-512 of `artifacts/packages/TimeWarp.State.12.0.0-beta.16.nupkg` (and the Blazor pair).

Expect:

- Isolation tests: 8 passed, including `DeleteTheNamedVersionFolderAndLeaveOtherVersions` and `LeaveTheCacheUntouchedWhenTheVersionIsAbsent`.
- Content tests: 4 passed. `FailPlusWhenDownloadFileIsMissing` fails the Plus layout when `download-file.js` is absent. `IgnoreACommentInContentFilesTsconfig` does not throw.
- verify-samples prints a `Purged artifacts/nuget-packages/...` line for each extract that was present, the content test passes, both sample 00 auto projects restore and build, and the last line is `Samples verified successfully!`. The isolated `.nupkg.metadata` `contentHash` equals the nupkg SHA-512. Exit code 0.

### Review

- Rounds: 2, roster general (claude-opus-5-5). Round 1 ran at effort 3, round 2 at effort 2.
- Final counts: bug 1 fixed, suggestion 1 fixed, nit 1 fixed. 0 open, 0 wontfix.
- Disposition: **clean**. Fix commit `6fa0f6b8` resolved all three findings, and round 2 found nothing new.
- M1: before the fix, `verify-samples` restored sample 00 auto from a stale same-version extract in `artifacts/nuget-packages`. Round 2 checked on disk that the isolated `contentHash` equals the packed nupkg SHA-512.
- Round 2 checks: packaging tests 4/4 passed. `timewarp-state-tests` passed 128, with 1 pre-existing skip.
- Paths: `review/review-framework.md`, `review/round-1/merged.md`, `review/round-2/general.md`, `review/round-2/merged.md`, `review/disposition.md`.

## Review

- Round 1: M1, M2, and M3 implemented in the fix pass.
- Round 2: all three verified fixed, no new findings. Disposition is clean (`review/disposition.md`).

## Notes

Commit the regression fix (requirement 1) separately from the sample migration so it can be cherry-picked if the rest stalls. Task 125 chose an isolated cache over a local version suffix; do not revisit that choice here.
