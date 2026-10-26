# Local-feed packs poison the global NuGet cache under the release version (issue 647)

## Description

A local-feed pack from a task worktree (`artifacts/packages`) gets restored into the **global** NuGet cache (`~/.nuget/packages`) under the same version that is later published to nuget.org. Every consumer on the machine then silently restores the stale bits. In the reported case this caused `TypeLoadException: TimeWarp.State.PipelineBehaviorRegistry` and 207 of 215 timewarp-architecture SPA tests failing.

Source: GitHub issue https://github.com/TimeWarpEngineering/timewarp-state/issues/647 (open; filed 2026-10-13 by StevenTCramer). Do not create another issue; this task tracks it.

### Issue body (quoted verbatim)

> Found during timewarp-architecture task 342 (State `12.0.0-beta.12` dogfood).
>
> ## Problem
>
> `~/.nuget/packages/timewarp.state/12.0.0-beta.12/.nupkg.metadata` (and the `.blazor` / `.telemetry` entries) recorded `source: .../timewarp-state/task-117-.../artifacts/packages`. A local-feed pack from a task worktree had been restored into the **global** NuGet cache under the same version that was later published to nuget.org. The local build predates `PipelineBehaviorRegistry`, so every consumer on this machine restored the stale bits silently:
>
> ```
> TypeLoadException: Could not load type 'TimeWarp.State.PipelineBehaviorRegistry' from assembly 'TimeWarp.State, Version=12.0.0.0'
> ```
>
> 207 of 215 timewarp-architecture SPA tests failed until the cache entries were deleted. The same pattern appears for many older State versions (`12.0.0-beta.4` through `beta.11`, `13.0.0-beta.1`).
>
> ## Expected
>
> Local-feed packs and the restores that consume them should not write the release version into the shared global cache. Options include an isolated `NUGET_PACKAGES` for the local-feed lane, or a unique local prerelease suffix.

### Verified evidence (2026-10-23)

- **No fix on master.** `nuget.config` still lists `artifacts/packages` as `LocalNuGetFeed`. `scripts/package.cs`, `build.cs`, `test.cs`, `e2e.cs` and `clean.cs` still pack and restore against it. Nothing isolates `NUGET_PACKAGES`, and nothing suffixes local versions.
- **Related prior notes:**
  - Done task 076 (lines 51-52): "same-version local-feed repacks are fragile (the cache shadows them)".
  - Done task 080-001, review round 1: suggests bumping to a fresh prerelease.
  - CI-ordering fixes in commits `75f7f4de` and `03d4d931` (related, not a fix for this).
- **Still happening on TWE-001.** `~/.nuget/packages/timewarp.state/12.0.0-beta.14/.nupkg.metadata` has `source` = `.../timewarp-state/task-118-.../artifacts/packages`, and `12.0.0-beta.15` has `source` = `.../task-121-.../artifacts/packages`. Both versions are published on nuget.org. `12.0.0-beta.13` is correctly from nuget.org.
- **Workaround:** delete the poisoned cache folders (`~/.nuget/packages/timewarp.state*/<version>`).

## Requirements

- Local-feed packs, and the restores that consume them, must never write a release version into the shared global cache (`~/.nuget/packages`).
- Choose one approach and record the choice in `## Session`:
  - An isolated `NUGET_PACKAGES` for the local-feed lane (scripts set it for pack, restore, build, test, e2e and samples), or
  - A unique local prerelease suffix (for example `-local.<timestamp>` or `-local.<commit>`) so a local pack never equals a version published to nuget.org.
- Samples, e2e and dev scripts still work with the chosen approach.
- A guard fails or warns if the global cache entry for the version being packed came from a local source (`.nupkg.metadata` `source` points at a local path rather than nuget.org).
- Document the behaviour in the scripts overview or CONTRIBUTING.

## Suggested order

1. Decide the approach (isolated cache vs unique suffix) and write it in `## Session`.
2. Implement it in `scripts/package.cs`, `build.cs`, `test.cs`, `e2e.cs`, `clean.cs` and `nuget.config` as needed.
3. Add the guard.
4. Check samples, e2e and dev scripts.
5. Document it.

## Goal (implementer restatement)

A pack or restore that runs inside a timewarp-state worktree must not store that worktree's `TimeWarp.State*` nupkg in the machine-wide NuGet cache (`~/.nuget/packages`) under the version string that nuget.org also publishes. Other repos on the same machine must keep resolving the published bits. Samples, end-to-end tests, and the dev scripts (`build`, `test`, `package`, `clean`, `e2e`) must still build and restore the local feed. Packing must fail when the shared cache already holds that version from a local folder. The behaviour is written down, and this machine's already-poisoned `12.0.0-beta.14` and `12.0.0-beta.15` cache folders (and the `.blazor` / `.telemetry` siblings) are removed.

Done when:

- A local pack followed by a sample restore leaves `~/.nuget/packages/<id>/<packed-version>` unchanged, with a before/after log.
- Restores that consume `artifacts/packages` write the package into an isolated folder, so the packed version can still match a nuget.org version without landing in the shared cache.
- The pack guard fails when `.nupkg.metadata` `source` is a local path.
- Samples, e2e, and the dev scripts still succeed.
- `scripts/overview.md` or the contributing overview describes the isolated cache and the guard.

## Checklist

- [x] Choose the isolation approach and record it in `## Session`
- [x] Implement it in `scripts/package.cs`, `build.cs`, `test.cs`, `e2e.cs`, `clean.cs` and `nuget.config` so local packs/restores never write the release version into `~/.nuget/packages`
- [x] Samples still restore and build against local packs
- [x] e2e still works
- [x] Dev scripts (`build`, `test`, `package`, `clean`) still work
- [x] Add a guard (script or check) that fails or warns if the global cache entry for the version being packed came from a local source
- [x] Document it in the scripts overview or CONTRIBUTING
- [x] Proof in the PR: a log showing the global cache untouched after a local pack/restore
- [x] Clear the existing poisoned cache entries on TWE-001 (`timewarp.state` `12.0.0-beta.14` and `beta.15`, and any `.blazor` / `.telemetry` siblings) and note it in the PR

## Acceptance criteria

- After a local pack and restore from a task worktree, `~/.nuget/packages` has no new or changed entry for the packed version. The PR shows a log, such as a before/after listing or a `.nupkg.metadata` check.
- A local pack cannot produce a version equal to a published nuget.org version, or its restore lands only in an isolated cache.
- The guard flags a global cache entry whose source is a local path.
- Samples, e2e and the dev scripts still pass.
- The approach is documented.
- One PR, merged via `ganda pr merge`.

## Session

- Created: 2787017 (2026-10-23)
- Filed 2026-10-23 at Steven's request from GitHub issue #647. Filed only; not implemented or launched.
- Implementation approach (2026-10-26): **isolated packages folder**, not a local prerelease suffix. `nuget.config` `globalPackagesFolder` and `RestorePackagesPath` both point at `artifacts/nuget-packages`. The dev CLI and `scripts/package.cs`, `build.cs`, `test.cs`, `e2e.cs`, and `clean.cs` set `NUGET_PACKAGES` to that folder before child `dotnet` processes. A suffix would still write a new global-cache entry for the packed version, and it would split the version SSOT that samples and publish share. The release version string stays equal to the nuget.org version; only the restore location changes. `dev pack` fails when `~/.nuget/packages/<id>/<version>/.nupkg.metadata` `source` is not an http(s) URL.
- Validation (2026-10-26): `scripts/build.cs`, `scripts/package.cs`, `scripts/test.cs`, `scripts/e2e.cs`, `scripts/clean.cs`, `dev pack`, and `dev verify-samples` exited 0. `ganda repo audit` exits 0 after `audit --fix` restored `bin/dev` (clean removes every `bin/` directory, including that shim). Two advisory warnings remain and are pre-existing: task 098 has no worktree, and seven framework paths are not kebab-case (`*.lib.module.*`, a `.targets` file, and backlog task 110).

## Notes

- Related: done task 076, done task 080-001.
- Related commits: `75f7f4de`, `03d4d931` (CI ordering).

## Results

Local-feed restores extract into `artifacts/nuget-packages`. The packed version `12.0.0-beta.16` is not written to `~/.nuget/packages`. `dev pack` and `scripts/package.cs` refuse to pack when the shared-cache `.nupkg.metadata` `source` for that version is not an `http://` or `https://` URL.

The out-of-tree DevTools consumer test sets `NUGET_PACKAGES` to its temp packages folder. The repo runner's value would otherwise beat that project's `nuget.config` and stamp an in-repo path.

### Cache cleanup on TWE-001

Deleted only version folders whose `.nupkg.metadata` `source` was a local path (11 folders):

- `timewarp.state`, `timewarp.state.blazor`: `12.0.0-beta.14` (task-118), `12.0.0-beta.15` (task-121), `12.0.0-beta.16` (task-120)
- `timewarp.state.plus`: `12.0.0-beta.14` (task-118), `12.0.0-beta.16` (task-120)
- `timewarp.state.telemetry`: `12.0.0-beta.14` (task-118), `12.0.0-beta.15` (task-121), `12.0.0-beta.16` (task-120)

Kept every `https://api.nuget.org/v3/index.json` entry, including `timewarp.state.plus` `12.0.0-beta.15`. Older versions named in issue 647 (`beta.4` through `beta.11`, `13.0.0-beta.1`) were not on this machine. While samples were verifying, timewarp-architecture task 418 restored published `12.0.0-beta.15` of `timewarp.state`, `.blazor`, and `.telemetry` from nuget.org into `~/.nuget/packages`. That is a different version and an https source. `12.0.0-beta.16` stayed absent.

### Proof log

Guard, before the deletions (`dev pack`, exit 1). The 23 `timewarp.state*` metadata files were unchanged by that run:

```
NUGET_PACKAGES=.../task-125-.../artifacts/nuget-packages
Global NuGet cache entry for TimeWarp.State 12.0.0-beta.16 came from a local source.
Global NuGet cache entry for TimeWarp.State.Blazor 12.0.0-beta.16 came from a local source.
Global NuGet cache entry for TimeWarp.State.Plus 12.0.0-beta.16 came from a local source.
Global NuGet cache entry for TimeWarp.State.Telemetry 12.0.0-beta.16 came from a local source.
```

After the deletions, `dev pack` exited 0 and packed `12.0.0-beta.16` for State, Blazor, Plus, Policies, Telemetry, and the four DevTools packages. A console sample restore then wrote `artifacts/nuget-packages/timewarp.state/12.0.0-beta.16/.nupkg.metadata` with `source` set to this worktree's `artifacts/packages`. The 12 shared-cache metadata files taken just before that pack were byte-identical afterwards. `~/.nuget/packages/**/12.0.0-beta.16` was absent.

`dev verify-samples` exited 0 (every sample, plus a trimmed publish of sample 05). `scripts/build.cs` and `scripts/package.cs` exited 0.

`scripts/test.cs` exited 0: analyzer 44, source generator 90, state 126 passed / 1 skipped, plus 40 / 1 skipped, telemetry 21, devtools 101, devtools isolation 4, client integration 64 / 1 skipped, architecture 7 / 1 skipped. The sha256 listing of `~/.nuget/packages/timewarp.state*/**/.nupkg.metadata` was identical before and after (15 files). No `12.0.0-beta.16` folder.

`scripts/e2e.cs` exited 0: Playwright 12 passed. Chromium was already installed; the script's browser install warned that sudo needs a terminal.

`scripts/clean.cs` exited 0 with `NUGET_PACKAGES` pointed at `artifacts/nuget-packages`. It recreated `artifacts/packages`. The same 15 shared-cache metadata files were byte-identical after clean, and `12.0.0-beta.16` was still absent. `dotnet nuget locals global-packages -l` from this repo prints the worktree `artifacts/nuget-packages` path.

`ganda repo audit --fix` restored `bin/dev` after clean had removed it. A following audit exits 0 (31 passed, 2 advisory warnings).

### How to validate

Smoke:

```bash
dotnet nuget locals global-packages -l
dotnet run --file tools/dev-cli/dev.cs -- pack
dotnet build samples/07-console/sample-07-console/sample-07-console.csproj
find ~/.nuget/packages -maxdepth 3 -type d -name '12.0.0-beta.16'
python3 -c "import json; print(json.load(open('artifacts/nuget-packages/timewarp.state/12.0.0-beta.16/.nupkg.metadata'))['source'])"
```

Expect: `global-packages` is `<repo>/artifacts/nuget-packages`. Pack exits 0 when the shared cache has no local-sourced entry for `12.0.0-beta.16`, and exits 1 naming the package id when `.nupkg.metadata` `source` is a local path. The sample build restores. `find` prints nothing. The isolated metadata `source` is this worktree's `artifacts/packages`, and `~/.nuget/packages` has no new file for `12.0.0-beta.16`.
