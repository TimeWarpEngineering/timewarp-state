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

- [ ] Fresh worktree restore works with no manual folder creation (claim a scratch worktree or delete `artifacts/` and restore)
- [ ] nupkg content test added and green against `dev pack` output
- [ ] sample 00 auto stays on the local feed and still builds and runs
- [ ] nine other samples on `ProjectReference`; `./bin/dev build` and `./bin/dev verify-samples` green
- [ ] CI pack ordering reduced to the one packaged consumer
- [ ] `./bin/dev test` green
- [ ] docs updated

## Session

- Created: 259f62af-98bf-4133-8bba-2f8a32eb7caa (2026-10-26)

## Notes

Commit the regression fix (requirement 1) separately from the sample migration so it can be cherry-picked if the rest stalls. Task 125 chose an isolated cache over a local version suffix; do not revisit that choice here.
