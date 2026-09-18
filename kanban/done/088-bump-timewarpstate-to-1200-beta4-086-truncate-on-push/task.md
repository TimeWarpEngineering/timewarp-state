# Bump TimeWarp.State to 12.0.0-beta.4 (086 truncate-on-push)

## Description

PR **#589** (task 086) merged the RouteState truncate-on-push fix **without** a version bump. `12.0.0-beta.3` is already on NuGet (cut 2026-09-17, pre-086). Architecture and COPIC cannot consume the breadcrumb fix until a new Plus package exists.

Bump **one** prerelease: `12.0.0-beta.3` → **`12.0.0-beta.4`**. Align both SSOTs. Do **not** cut the NuGet from this PR — after merge, cockpit `/tw-release` from origin-home.

## Depends on

086 (merged #589)

## Requirements

- `source/Directory.Build.props` `<Version>` → `12.0.0-beta.4`
- `msbuild/repository.props` `<TimeWarpStateVersion>` → `12.0.0-beta.4` (samples / CPM)
- `dev workflow` SSOT assert must stay green (both strings equal)
- Changelog / release notes only if this repo already has a file for the current beta that needs a new heading — do not invent a docs site
- No other product change
- Do **not** `dev release`, tag, or `dotnet nuget push`

## Checklist

- [x] Both version properties are `12.0.0-beta.4`
- [x] `dotnet run --file tools/dev-cli/dev.cs -- build` 0 errors
- [x] Results: next step is origin-home `dev release` after this PR merges and master CI is green
- [x] Implementation review disposition recorded under `review/`

## Out of scope

- Cutting the GitHub Release / NuGet (human `/tw-release` after merge)
- Architecture / COPIC PackageReference bumps (those repos, after NuGet is up)
- Stable 12.0.0 or 13.0.0

## Notes

tw-release sequence: bump PR → merge → wait master `Packages-*` CI artifact → `dev release` from synced origin-home master.

Review kitchen: `review/review-framework.md`, `review/round-1/`, `review/disposition.md`.

## Session

- Created: cockpit grok 2026-09-17 after `/tw-merge` 589 — version bump was missing from 086
- Implementer: grok 4.6 session 01a0aff4-b941-7f52-94c3-5e8f457c9428 (2026-09-17)
- Review oracle: Grok session `01a0aff8-97f4-7060-a05f-c336fdbca18e` (2026-09-17); general reviewer grok-4.5 `01a0affa-2b6c-7321-a260-c005d53fe10d`

## Results

Bumped both version SSOTs `12.0.0-beta.3` → `12.0.0-beta.4` so the RouteState truncate-on-push fix from #589 can ship as a new Plus package. No other product change. Release notes were not added: `documentation/release-notes/` only has stable 1.0.0–11.0.0 files; there is no current-beta heading to extend.

**Files**

- `source/Directory.Build.props` — `<Version>12.0.0-beta.4</Version>`
- `msbuild/repository.props` — `<TimeWarpStateVersion>12.0.0-beta.4</TimeWarpStateVersion>`
- `Directory.Packages.props` already uses `$(TimeWarpStateVersion)` for `TimeWarp.State` / `TimeWarp.State.Plus`; unchanged

**Decisions**

- One prerelease bump only. Do not cut the GitHub Release / NuGet from this PR.
- Next step after this PR merges and master `Packages-*` CI is green: origin-home `/tw-release` (`dev release`) from synced master.

**Tests**

- `dotnet run --file tools/dev-cli/dev.cs -- build`: 0 errors (47 pre-existing RS0030/NU1510 in test-app / e2e; none in source packages)
- Local pack via `GeneratePackageOnBuild` wrote `TimeWarp.State`, `TimeWarp.State.Plus`, and `TimeWarp.State.Policies` `12.0.0-beta.4` nupkgs under `artifacts/packages/`
- SSOT: both properties equal `12.0.0-beta.4`
- Latest GitHub release is `v12.0.0-beta.3` (2026-09-17); beta.4 is the next prerelease

### How to validate

**Smoke**

```bash
grep -E '<Version>|<TimeWarpStateVersion>' source/Directory.Build.props msbuild/repository.props
dotnet run --file tools/dev-cli/dev.cs -- build
ls artifacts/packages/TimeWarp.State{,.Plus,.Policies}.12.0.0-beta.4.nupkg
```

**Expect**

- Both properties print `12.0.0-beta.4` (source `<Version>` equals `TimeWarpStateVersion`)
- Build exits 0 with `0 Error(s)` and `Build completed successfully!`
- The three `12.0.0-beta.4` nupkgs exist under `artifacts/packages/`

**Automated gate**

Same as Smoke: `dotnet run --file tools/dev-cli/dev.cs -- build` from repo root. `dev workflow` `AssertVersionSsot` will also fail if the two strings drift.

**Not in scope:** `dev release`, tagging, `dotnet nuget push`, architecture / COPIC PackageReference bumps. After merge, wait for master `Packages-*` CI, then cut from origin-home with `/tw-release`.

### Review disposition

- Body: tw-implementation-review, effort 1, roster `general` (grok-4.5 subagent, read-only); 1 round on commit `82f299eb` vs `origin/master`.
- Round 1: 0 bug, 0 suggestion, 0 nit. Merge pass confirmed both SSOTs are `12.0.0-beta.4` and equal (`AssertVersionSsot` stays green), CPM still uses `$(TimeWarpStateVersion)`, product delta is only the two version properties, and beta release notes were correctly omitted.
- Final: 0 open; 0 fixed; 0 wontfix.
- **Disposition: clean** (`review/disposition.md`; framework `review/review-framework.md`; last ledger `review/round-1/merged.md`).
