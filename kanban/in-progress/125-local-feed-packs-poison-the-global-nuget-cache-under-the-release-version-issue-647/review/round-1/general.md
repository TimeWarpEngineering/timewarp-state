# Round 1 — general
**Date:** 2026-10-26
**Scope reviewed:** same as framework (`41b1df74..7fde0ec1`, product commit `d0385ca2`)

## Summary

The change sends every restore in the repo to `artifacts/nuget-packages`, using three layers:

- `nuget.config` `globalPackagesFolder`, which covers runfile `#:package`, `dotnet tool restore`, IDE restores, and `dotnet nuget locals`.
- `RestorePackagesPath` in the root `Directory.Build.props`. `scripts/`, `tools/dev-cli/`, `source/`, `tests/`, and `samples/` all inherit it, and there are no nested nuget.config files.
- `NUGET_PACKAGES`, set by `dev.cs` and each runfile.

With all three in place, a local-feed pack can no longer land in `~/.nuget/packages` under the release version. The guard (`NuGetPackageIsolation.FindPoisonedEntries`) is shared by `dev pack`, `scripts/package.cs`, and the unit tests. It flags local paths, `file://` sources, and metadata that is missing or unreadable. The out-of-tree DevTools consumer test now passes `NUGET_PACKAGES` explicitly, which keeps its stamped-path assertion valid. Overall risk is low. I found no correctness defects. The one operational cost is that each worktree now restores into its own cold cache, and the docs do not mention it.

## Verification performed

- `dotnet nuget locals global-packages -l` from the worktree prints `<worktree>/artifacts/nuget-packages`.
- `dotnet build tests/timewarp-state-tests` passed (0 errors), then `dotnet fixie timewarp-state-tests --tests 'NuGetPackageIsolation*'` ran: 6 passed.
- `find ~/.nuget/packages -maxdepth 2 -mindepth 2 -mmin -10` was empty after that build and restore, so nothing new was written to the shared cache.
- `~/.nuget/packages/timewarp.state*/12.0.0-beta.16` is absent.
- Import chain: `scripts/Directory.Build.props` and `tools/dev-cli/Directory.Build.props` import the root props, so `RestorePackagesPath` applies there too. `msbuild/repository.props` defines `IsolatedNuGetPackagesDirectory` before the root props reads it.
- `Apply` is idempotent: the first call wins and keeps the inherited `NUGET_PACKAGES`. Nested runs (`dev test` → `scripts/test.cs`) inherit the isolated value, and `AddSharedFolder` drops it, so the guard never scans the isolated lane.
- Goal restatement vs task: consistent. The restatement picks "fail", which the task allows ("fails or warns"), and it adds the TWE-001 cleanup, which is already on the task checklist. No misread.

## Issues

### Issue 1 — Severity: nit
- File: scripts/overview.md:9
- Description: Each worktree restores into its own `artifacts/nuget-packages`, and `clean.cs` and `build.cs clean` delete `artifacts/`, which takes the isolated cache with it. The first restore in every new task worktree, and the first after every clean, re-downloads the full package graph from nuget.org. Nothing is shared with `~/.nuget/packages` or with sibling worktrees. The docs explain why the cache is isolated but not what that costs (disk use per worktree, time to the first restore).
- Suggestion: Add one sentence to the "Local NuGet cache" section saying the isolated cache belongs to one worktree, starts cold, and is removed by `clean`.
- Status: open
