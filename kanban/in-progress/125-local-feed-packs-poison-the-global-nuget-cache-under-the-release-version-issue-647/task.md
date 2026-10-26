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

## Checklist

- [ ] Choose the isolation approach and record it in `## Session`
- [ ] Implement it in `scripts/package.cs`, `build.cs`, `test.cs`, `e2e.cs`, `clean.cs` and `nuget.config` so local packs/restores never write the release version into `~/.nuget/packages`
- [ ] Samples still restore and build against local packs
- [ ] e2e still works
- [ ] Dev scripts (`build`, `test`, `package`, `clean`) still work
- [ ] Add a guard (script or check) that fails or warns if the global cache entry for the version being packed came from a local source
- [ ] Document it in the scripts overview or CONTRIBUTING
- [ ] Proof in the PR: a log showing the global cache untouched after a local pack/restore
- [ ] Clear the existing poisoned cache entries on TWE-001 (`timewarp.state` `12.0.0-beta.14` and `beta.15`, and any `.blazor` / `.telemetry` siblings) and note it in the PR

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

## Notes

- Related: done task 076, done task 080-001.
- Related commits: `75f7f4de`, `03d4d931` (CI ordering).
