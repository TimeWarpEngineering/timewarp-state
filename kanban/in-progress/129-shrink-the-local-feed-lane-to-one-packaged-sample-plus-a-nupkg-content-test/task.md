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

## Session

- Created: 259f62af-98bf-4133-8bba-2f8a32eb7caa (2026-10-26)
- Implementation: grok 01a1781a-5751-74c1-b158-536134dfae2d (2026-10-26). Regression fix is `00f3102f` (root `nuget.config` no longer lists the feed). The sample move, nupkg content test, and docs are the next commit. `Directory.Packages.props` dropped `TimeWarp.State.Plus` and `TimeWarp.State.Telemetry` because nothing package-references them after that move (`cpm-consistency`).
- Commands were `dotnet run --file tools/dev-cli/dev.cs`. `./bin/dev` is gitignored and was not the binary under test.
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session not reported, max-turns 500, Done — 2026-10-26T13:05:38Z

## Goal (implementer restatement)

A fresh worktree has to restore and commit without anyone creating `artifacts/packages` by hand. Root `nuget.config` lists that folder as a package source, so every restore, including ones that never consume the local packages, fails with NU1301 when the folder is missing. The feed should stay off those restores. The one consumer that still proves the packed bits is sample 00 auto: `verify-samples` packs, then restores that host and WebAssembly client with a config file that maps `TimeWarp.State` and `TimeWarp.State.*` to `artifacts/packages`. The other nine sample projects should compile against the source projects, the same way the test app does, and CI should not order them behind the feed.

The pack output needs a test that opens every `.nupkg` from `dev pack`. `TimeWarp.State.Blazor` has to contain its `_content` static web assets, including the `.lib.module.js`. The analyzer and source generator assemblies have to sit under `analyzers/dotnet/cs` in the package that carries them. A missing entry fails the test. The DevTools inspector test stays on its own pack lane. `samples/overview.md` and the contributing scripts section have to say which consumer proves packaging and why the rest use project references. The isolated package cache from task 125 stays as it is.

## Results

Root `nuget.config` no longer lists `artifacts/packages`. That is commit `00f3102f`, separate from the sample move. Nine samples that used to `PackageReference` TimeWarp.State packages now `ProjectReference` the source projects, plus the analyzer and source generator. Sample 04's AppHost was already a project reference. Sample 00 auto (host and WebAssembly client) stays on `PackageReference` and is out of `timewarp-state.slnx`, because `12.0.0-beta.16` is not on nuget.org. `verify-samples` builds the project-referenced samples, publishes sample 05 trimmed with `VerifySamplesTrim` (a global `PublishTrimmed` also marks the netstandard analyzer projects), packs, runs the nupkg content test, then restores sample 00 auto through `artifacts/sample-00-auto.nuget.config`.

`tests/timewarp-state-packaging-tests` opens every `.nupkg` in `artifacts/packages`. `dev pack` runs it after the nuspec allow-list. `dev test` does not, because the workflow tests before the pack. `TimeWarp.State` must contain `analyzers/dotnet/cs/timewarp-state-analyzer.dll` and `timewarp-state-source-generator.dll`. `TimeWarp.State.Blazor` must contain a fingerprinted `.lib.module.js` whose asset base path is `_content/TimeWarp.State`. The DevTools inspector test still packs its own temp feed.

`Directory.Packages.props` no longer versions `TimeWarp.State.Plus` or `TimeWarp.State.Telemetry`. Nothing in the repo package-references them after the sample move.

### How to validate

Smoke:

- Move `artifacts/packages` aside and run `dotnet restore timewarp-state.slnx`, then put the folder back.
- `dotnet run --file tools/dev-cli/dev.cs -- build`
- `dotnet run --file tools/dev-cli/dev.cs -- verify-samples`
- `dotnet run --project samples/00-state-action-handler/auto/sample-00-auto/sample-00-auto/sample-00-auto.csproj --configuration Release --no-build --launch-profile http`, then `curl -sS -o /dev/null -w '%{http_code}' http://localhost:5249/`
- `dotnet run --file tools/dev-cli/dev.cs -- test`

Expect:

- The solution restore exits 0 while `artifacts/packages` is missing. Sample 00 auto is not in the solution.
- Build prints `Build filter: 37 project(s); sample 00 auto omitted.` and exits 0.
- verify-samples builds 10 project-referenced samples, trimmed-publishes sample 05, packs 9 nupkgs, the content test passes (`1 passed`), then restores and builds both sample 00 auto projects. Last line: `Samples verified successfully!`
- The host prints `Now listening on: http://localhost:5249` and `GET /` returns 200.
- `dev test` exits 0. Suites: analyzer 44 passed; source generator 90 passed; state 126 passed, 1 skipped; plus 40 passed, 1 skipped; telemetry 21 passed; devtools 101 passed; devtools isolation 4 passed; client integration 64 passed, 1 skipped; architecture 7 passed, 1 skipped.

## Notes

Commit the regression fix (requirement 1) separately from the sample migration so it can be cherry-picked if the rest stalls. Task 125 chose an isolated cache over a local version suffix; do not revisit that choice here.
