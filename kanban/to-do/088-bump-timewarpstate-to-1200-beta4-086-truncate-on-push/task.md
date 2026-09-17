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

- [ ] Both version properties are `12.0.0-beta.4`
- [ ] `dotnet run --file tools/dev-cli/dev.cs -- build` 0 errors
- [ ] Results: next step is origin-home `dev release` after this PR merges and master CI is green

## Out of scope

- Cutting the GitHub Release / NuGet (human `/tw-release` after merge)
- Architecture / COPIC PackageReference bumps (those repos, after NuGet is up)
- Stable 12.0.0 or 13.0.0

## Notes

tw-release sequence: bump PR → merge → wait master `Packages-*` CI artifact → `dev release` from synced origin-home master.

## Session

- Created: cockpit grok 2026-09-17 after `/tw-merge` 589 — version bump was missing from 086
