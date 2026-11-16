# Analysis: isolated NuGet restore path vs project references (task 155)

## Executive Summary

Only one in-repo build restores a TimeWarp.State package from the local feed: sample 00 auto, which `dev verify-samples` restores with its own generated config. Every other in-repo consumer already uses ProjectReference. Issue 647 happened when the root `nuget.config` listed `artifacts/packages` and all samples used PackageReference. The project-reference move (`9c7dda13`) plus dropping the feed from root `nuget.config` (`00f3102f`) remove that ambient exposure. The one packed-artifact lane still needs isolation.

**Recommendation: narrow.** Keep restore isolation only on the packaged-consumer lanes and keep the pack guard. Remove the repo-wide `RestorePackagesPath`, the root `globalPackagesFolder`, and the blanket `NUGET_PACKAGES` override. Those no longer protect anything the narrow lane does not. They cost a full package graph per worktree (4.5 GB in the master worktree's `artifacts/nuget-packages`). They also leave `~/.dotnet/toolResolverCache` pointing into gc'd worktrees, which broke this task's own test gate. `LocalNuGetCache` and `./LocalNugetFeed` are dead.

## Scope

- Root `Directory.Build.props` line 10 `RestorePackagesPath`, root `nuget.config` `globalPackagesFolder`, and the `NuGetPackageIsolation.Apply` callers that set `NUGET_PACKAGES`.
- Every place in this repo that consumes a package this repo builds: samples, tests, templates, analyzers and generators, scripts, and CI.
- Issue 647 and commits `d0385ca2` (isolation), `9c7dda13` (nine samples to ProjectReference), and `00f3102f` (feed removed from root `nuget.config`).
- `LocalNuGetCache` in `msbuild/repository.props` line 17 and `./LocalNugetFeed` in `scripts/clean.cs` line 53.

No product files were changed.

## Methodology

- `grep` across the tree, excluding `artifacts/`, `bin/`, `obj/`, `.git/`, and `kanban/`, for `PackageReference Include="TimeWarp.State`, `#:package TimeWarp.State`, `artifacts/packages`, `nuget-packages`, `NUGET_PACKAGES`, `LocalNuGetFeed`, `LocalNuGetCache`, `local-nuget-feed`, `LocalNugetFeed`, `.template.config`, and `actions/cache`.
- Read `timewarp-state.slnx`, `Directory.Packages.props`, every sample `.csproj`, `tools/dev-cli/endpoints/{verify-samples,build,pack}-command.cs`, `tools/dev-cli/endpoints/nuget-package-isolation.cs`, `tools/dev-cli/dev.cs`, `scripts/{build,clean,package,test,e2e}.cs`, `.github/workflows/workflow.yml`, `tests/timewarp-state-devtools-tests/packaged-devtools-app-tests.cs`, and `tests/timewarp-state-packaging-tests/nupkg-content-tests.cs`.
- `git show` for `d0385ca2`, `9c7dda13`, and `00f3102f`. `git show d0385ca2^:nuget.config` for the config at the time of issue 647.
- `gh issue view 647`.
- `du -sh` on the master worktree's `artifacts/nuget-packages`.

## Findings

### F1. In-repo consumer inventory

| # | Consumer | How it consumes today | Would ProjectReference work? | What ProjectReference loses |
|---|----------|-----------------------|------------------------------|-----------------------------|
| 1 | `samples/00-state-action-handler/auto/sample-00-auto-client` (and its host `sample-00-auto`, which ProjectReferences the client) | `PackageReference` TimeWarp.State and TimeWarp.State.Blazor at `$(TimeWarpStateVersion)` (`Directory.Packages.props` lines 18–19). Not in `timewarp-state.slnx`. `dev build` leaves it out of the generated `.slnf` (`build-command.cs` Design). `dev verify-samples` packs, deletes same-version extracts under `artifacts/nuget-packages`, writes `artifacts/sample-00-auto.nuget.config` (local feed with `packageSourceMapping` for `TimeWarp.State` and `TimeWarp.State.*`, `globalPackagesFolder` = `artifacts/nuget-packages`), restores with `--force`, then builds `--no-restore`. | Yes, it would compile. | The only proof that a real Blazor Auto app builds against the **packed** library: analyzer and generator DLLs under `analyzers/dotnet/cs`, Blazor static web assets and the JS module path, and the dependency graph as published. This is the intended packed-artifact gate (`9c7dda13`: "Sample 00 auto stays the packaged consumer"). Keep it. |
| 2 | Nine other samples (00 server, 00 wasm, 02, 03, 04 apphost/server/wasm, 05, 06, 07) | `ProjectReference` to `source/*` plus analyzer and generator as `OutputItemType="Analyzer"` since `9c7dda13`. | Already ProjectReference. | Nothing more to lose. |
| 3 | `tests/test-app/test-app-client` | ProjectReference. Lines 21–22 hold commented-out `PackageReference` TimeWarp.State and TimeWarp.State.Plus. | Already ProjectReference. | None. The comments are dead and can go. |
| 4 | `tests/timewarp-state-devtools-tests/packaged-devtools-app-tests.cs` | Packs DevTools.App and DevTools.Hosting into a **temp** feed, writes a temp `nuget.config` with its own temp `globalPackagesFolder`, and builds an out-of-tree AppHost with `NUGET_PACKAGES` set to that temp folder. | No. The test proves the packed tool layout and the stamped path into the package folder. | The whole point of the test. It already isolates itself and does not depend on the repo-wide setting. Its Design comment mentions the repo test runner's `artifacts/nuget-packages`; that sentence goes stale under the narrow option. |
| 5 | `tests/timewarp-state-packaging-tests/nupkg-content-tests.cs` | Opens `artifacts/packages/*.nupkg` as zip files. No restore. | Not applicable. | Not applicable. No cache involvement. |
| 6 | `tests/timewarp-state-tests/packaging/nuget-package-isolation-tests.cs` | Unit tests for the pack guard and `DeleteVersionFolders` against a fake folder. | Not applicable. | Not applicable. |
| 7 | Templates | None. No `.template.config` in the repo. | Not applicable. | Not applicable. |
| 8 | Analyzers and source generators | Packed inside TimeWarp.State (`timewarp-state.csproj` lines 74–77, `analyzers\dotnet\cs`). In-repo consumers load them as `OutputItemType="Analyzer"` ProjectReferences. Packed layout checked by `nupkg-content-tests` and exercised by sample 00 auto. | Already ProjectReference for in-repo builds. | Covered by rows 1 and 5. |
| 9 | Scripts | `scripts/package.cs` packs only ("a leftover helper; CI uses dev pack"). `scripts/{build,clean,test,e2e}.cs` call `NuGetPackageIsolation.Apply`, which sets `NUGET_PACKAGES`, but none of them restore from the local feed. | Not applicable (no consumer). | Not applicable. |
| 10 | CI (`.github/workflows/workflow.yml` → `dev workflow`) | PR and merge: `clean → build → test → e2e → verify-samples`. Only verify-samples restores a packed package (row 1). Release downloads the CI `Packages-*` artifact into `artifacts/packages` and pushes it (`workflow/ci-artifact-service.cs`). It does not restore it. No `actions/cache` step exists. | Not applicable. | Not applicable. |

Result: two lanes restore a packed TimeWarp.State package, rows 1 and 4. Each already writes its own config with an explicit `globalPackagesFolder`. Nothing else in the repo restores from `artifacts/packages`.

### F2. Issue 647 and whether project references would have prevented it

Issue 647: `~/.nuget/packages/timewarp.state/12.0.0-beta.12/.nupkg.metadata` recorded `source: …/timewarp-state/task-117-…/artifacts/packages`. A task-worktree pack under the release version sat in the machine cache. Consumers on that machine (timewarp-architecture task 342) then failed with `TypeLoadException … PipelineBehaviorRegistry`.

At that time (`git show d0385ca2^:nuget.config`) the root `nuget.config` listed `<add key="LocalNuGetFeed" value="artifacts/packages" />`, and every sample used `PackageReference` TimeWarp.State at the repo version. So any sample restore in a worktree after a pack pulled the in-progress version from the local feed into `~/.nuget/packages`.

- **Project references alone would have prevented most of it.** With nine samples on ProjectReference (`9c7dda13`, same day, after `d0385ca2`) and the feed gone from root `nuget.config` (`00f3102f`), a normal build, test, or IDE restore has no local source, so it cannot extract a local pack anywhere. An IDE restore of sample 00 auto with the root config resolves the version from nuget.org. It either fails before release or gets the published bits. It does not poison the cache.
- **They would not have prevented all of it.** Sample 00 auto still restores the release version from a local folder. Without its own package folder it would write that local extract into `~/.nuget/packages`, which is the 647 failure. That lane needs isolation, and it already has it in its generated config.
- **Outside this repo's reach.** If another repo had pointed a source at this repo's `artifacts/packages`, no setting here would have helped. The pack guard (`dev pack` fails when the shared cache holds that version from a local path) still catches it the next time this repo packs. The issue text does not say which restore wrote the entry.

Commit order matters for this question. `d0385ca2` (isolation, 16:28) landed before `9c7dda13` (project references, 20:03) and `00f3102f` (feed removed). The repo-wide isolation was the right fix for the repo as it stood at 16:28. The later two commits changed the threat model and nobody re-sized the fix.

### F3. What the repo-wide isolation costs now

- **Disk and network per worktree.** `RestorePackagesPath` in root `Directory.Build.props` and `NUGET_PACKAGES` from `dev.cs` mean every worktree downloads and extracts the full package graph into its own `artifacts/nuget-packages`. The master worktree's copy is 4.5 GB with 194 package ids. Each task worktree repeats that, and `ganda worktree gc` throws it away.
- **Three mechanisms for one setting.** `RestorePackagesPath` (MSBuild property, wins over everything), root `nuget.config` `globalPackagesFolder`, and `NuGetPackageIsolation.Apply` setting `NUGET_PACKAGES` in `dev.cs` and five runfiles. Docs repeat it in `claude.md`, `documentation/contributing/overview.md`, `scripts/overview.md`, and the Design regions of eight files.
- **Local tools break after `ganda worktree gc`.** `dotnet tool restore` extracts tools (fixie, docfx, and others) into the worktree's `artifacts/nuget-packages`. The SDK's machine-wide `~/.dotnet/toolResolverCache` stores that absolute path keyed only by tool and version. When the worktree is gc'd, every other worktree's `dev test` fails with `Run "dotnet tool restore" to make the "fixie" command available.` until someone deletes `~/.dotnet/toolResolverCache`. This task's own test gate hit it: the cache pointed at the gc'd `task-158-…/artifacts/nuget-packages/fixie.console/3.4.0`. With tools restored into the shared `~/.nuget/packages`, that path stays valid.
- **`clean` couples to it.** `scripts/clean.cs` and `scripts/build.cs clean` run `dotnet nuget locals all --clear`. That is safe only because `NUGET_PACKAGES` points at the isolated folder. If isolation is narrowed, the clear must be removed or limited. Otherwise `clean` would wipe the shared `~/.nuget/packages`. (`all` also clears the machine-wide http-cache and temp folders today.)

### F4. What narrowing keeps

- `artifacts/sample-00-auto.nuget.config` already sets `globalPackagesFolder` to `artifacts/nuget-packages`. But `RestorePackagesPath` from root `Directory.Build.props` currently wins over it. Under the narrow option, that config, or a `-p:RestorePackagesPath=` on the sample's restore, becomes the only isolation the sample needs. A dedicated folder per run (for example `artifacts/packaged-consumer-packages`, deleted before each run) would also replace the `DeleteVersionFolders` purge and the `--force` workaround. The sample's `dotnet build --no-restore` reads `project.assets.json`, which records that folder.
- The devtools packaged test (row 4) needs nothing. It isolates itself with temp folders.
- The pack guard (`FindPoisonedEntries` / `ReportPoisonedEntries` in `pack-command.cs`) should stay. It catches poisoning from any origin, including other repos.

### F5. Dead leftovers

- **`LocalNuGetCache`** (`msbuild/repository.props` line 17, `$(RepositoryRoot)local-nuget-feed/`): **dead.** The tree has no reader of `$(LocalNuGetCache)` and no `local-nuget-feed` folder. Introduced in `76827885` ("Add custom repository variables section"). `LocalNuGetFeed` on line 14 is **live**: `source/Directory.Build.props` line 19 sets `PackageOutputPath` from it.
- **`./LocalNugetFeed`** (`scripts/clean.cs` line 53): **dead.** That folder name dates from the PowerShell-era scripts (`analysis/powershell-scripts-analysis.md` line 98). Packs now go to `artifacts/packages`, which the same array already removes through `./artifacts`.
- **Related stale text found along the way:**
  - `scripts/clean.cs` line 43 and `scripts/build.cs` line 114 log "so the restored actions/cache is reused". `workflow.yml` has no `actions/cache` step.
  - Root `nuget.config` says "The packaged consumer passes that folder as an explicit restore source". `verify-samples` uses a generated `--configfile` with source mapping, not `--source` (its own Design region says `--source` is deliberately not used).
  - `tests/test-app/test-app-client/test-app-client.csproj` lines 21–22 hold commented-out `PackageReference` items.

## Recommendations

**Narrow** the isolation to the packed-artifact lanes. Do not keep it repo-wide, and do not remove it entirely.

- Keep sample 00 auto as the packaged consumer. A ProjectReference loses the only end-to-end proof of the packed analyzer, generator, and Blazor static-asset layout.
- Keep the pack guard.
- Drop the repo-wide restore redirection and its three mechanisms.

### Follow-up tasks to file

1. **Narrow NuGet restore isolation to the packaged-consumer lane** (priority high; depends on nothing).
   - Remove `RestorePackagesPath` from root `Directory.Build.props` and `IsolatedNuGetPackagesDirectory` from `msbuild/repository.props` if it has no remaining reader.
   - Remove `<config><add key="globalPackagesFolder" …/></config>` from root `nuget.config` and rewrite its comment.
   - Remove `NuGetPackageIsolation.Apply` from `dev.cs` and from `scripts/{build,clean,test,e2e,package}.cs`. Keep the guard's shared-cache scan, which will then read the inherited `NUGET_PACKAGES` or `~/.nuget/packages` directly.
   - In `verify-samples`, restore and build sample 00 auto against a dedicated per-run package folder (generated config `globalPackagesFolder`, plus `NUGET_PACKAGES` for that child only). Delete that folder before each run, and drop `DeleteVersionFolders` and `--force` if they become redundant.
   - Change `clean` (both `scripts/clean.cs` and `scripts/build.cs clean`) so it never clears `~/.nuget/packages`. Delete `artifacts/` only, or clear only the http-cache if that is wanted.
   - Update `claude.md`, `documentation/contributing/overview.md`, `scripts/overview.md`, the Design regions that name `artifacts/nuget-packages`, the `packaged-devtools-app-tests.cs` Design comment, and `nuget-package-isolation-tests.cs`.
   - Ensures `dotnet tool restore` extracts into the shared cache, so `~/.dotnet/toolResolverCache` no longer points into gc'd worktrees (F3).
  - Acceptance: a fresh task worktree's `dev build` and `dev test` restore into `~/.nuget/packages`, and `dev verify-samples` restores sample 00 auto from the just-packed nupkg. After `dev verify-samples`, `~/.nuget/packages/timewarp.state/<repo version>/.nupkg.metadata` is absent or has an `https://` source. `dev pack` still refuses a local-source entry in the shared cache.
2. **Remove dead NuGet leftovers** (priority low; independent of task 1, or fold into it).
   - Delete `LocalNuGetCache` (`msbuild/repository.props` line 17) and `"./LocalNugetFeed"` (`scripts/clean.cs` line 53).
   - Delete the commented-out `PackageReference` lines in `test-app-client.csproj`.
   - Fix the stale "actions/cache" messages in `scripts/clean.cs` and `scripts/build.cs`.
   - Decide whether `scripts/package.cs` (self-described "leftover helper") should go.

## References

- Issue 647: <https://github.com/TimeWarpEngineering/timewarp-state/issues/647>
- Commits: `d0385ca2` (isolation + pack guard), `9c7dda13` (nine samples to ProjectReference, sample 00 auto packaged), `00f3102f` (local feed removed from root `nuget.config`)
- `tools/dev-cli/endpoints/verify-samples-command.cs` (Design region; `WritePackagedConsumerConfig`)
- `tools/dev-cli/endpoints/nuget-package-isolation.cs`
- `tools/dev-cli/endpoints/build-command.cs` (Design region: sample 00 auto left out of the build filter)
- `tests/timewarp-state-devtools-tests/packaged-devtools-app-tests.cs`
- `scripts/overview.md`, `documentation/contributing/overview.md`, `claude.md` line 136
- NuGet global-packages folder precedence: `RestorePackagesPath` property, then `NUGET_PACKAGES`, then `globalPackagesFolder` in `nuget.config` — <https://learn.microsoft.com/nuget/consume-packages/managing-the-global-packages-and-cache-folders>
