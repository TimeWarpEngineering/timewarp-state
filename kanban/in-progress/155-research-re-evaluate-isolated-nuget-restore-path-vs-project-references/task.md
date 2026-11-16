# Research: re-evaluate isolated NuGet restore path vs project references

## Description

Commit `d0385ca2` (2026-10-26, "fix: keep local-feed packs out of the shared NuGet cache", Fixes #647) added `<RestorePackagesPath>$(IsolatedNuGetPackagesDirectory)</RestorePackagesPath>` in root `Directory.Build.props` (line 10) and `globalPackagesFolder = artifacts/nuget-packages` in `nuget.config`, plus a pack-time guard. The cause: a task-worktree pack of the release version got restored into `~/.nuget/packages`, and later restores loaded stale bits.

Owner's question: does timewarp-state need this at all? It probably should just use project references instead of consuming its own packages from a local feed. This is a research task: evaluate, do not change anything.

Also check possible dead leftovers: `msbuild/repository.props` (line 17) still defines `LocalNuGetCache` as `$(RepositoryRoot)local-nuget-feed/`, and `scripts/clean.cs` (line 53) references `./LocalNugetFeed`.

## Requirements

- Inventory every place in the repo that consumes a TimeWarp.State package built in this repo via PackageReference or a local feed: samples, tests, templates, source generators and analyzers, scripts, CI.
- For each consumer, say whether a ProjectReference would work and what would be lost (for example testing the real packed artifact, analyzer/source-generator packaging, template validation).
- Re-read issue 647 and state whether project references would have prevented it.
- Recommend keep, remove, or narrow, and list the follow-up implementation task(s) to file.
- Determine whether the `LocalNuGetCache` property in `msbuild/repository.props` and the `./LocalNugetFeed` reference in `scripts/clean.cs` are dead leftovers.
- Output: a report in this task's kitchen `report/` folder (follow tw-analysis-report conventions).

## Checklist

- [ ] Inventory in-repo package consumers (samples, tests, templates, analyzers/generators, scripts, CI)
- [ ] Per consumer: ProjectReference feasibility and what is lost
- [ ] Re-read issue 647; would project references have prevented it?
- [ ] Check `msbuild/repository.props` `LocalNuGetCache` and `scripts/clean.cs` `./LocalNugetFeed` for dead code
- [ ] Write report in `report/` with recommendation (keep / remove / narrow)
- [ ] List follow-up implementation task(s) to file

## Session

- Created: ccd4893c-4c6d-47fb-9913-a5368de1e1b4 (2026-11-11)

## Notes

Research only; do not change product files. Current `nuget.config` has nuget.org as the only source and the comment says the packaged consumer passes `artifacts/packages` as an explicit restore source, so at least one packed-artifact consumer exists.
