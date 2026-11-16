# Research: re-evaluate isolated NuGet restore path vs project references

## Description

Commit `d0385ca2` (2026-10-26, "fix: keep local-feed packs out of the shared NuGet cache", Fixes #647) added `<RestorePackagesPath>$(IsolatedNuGetPackagesDirectory)</RestorePackagesPath>` in root `Directory.Build.props` (line 10) and `globalPackagesFolder = artifacts/nuget-packages` in `nuget.config`, plus a pack-time guard. The cause: a task-worktree pack of the release version got restored into `~/.nuget/packages`, and later restores loaded stale bits.

Owner's question: does timewarp-state need this at all? It probably should just use project references instead of consuming its own packages from a local feed. This is a research task: evaluate, do not change anything.

Also check possible dead leftovers: `msbuild/repository.props` (line 17) still defines `LocalNuGetCache` as `$(RepositoryRoot)local-nuget-feed/`, and `scripts/clean.cs` (line 53) references `./LocalNugetFeed`.

## Goal (implementer restatement)

Decide whether timewarp-state still needs the repo-wide isolated NuGet restore folder
(`RestorePackagesPath` / `globalPackagesFolder` = `artifacts/nuget-packages`, plus the
`NUGET_PACKAGES` override in dev-cli and runfiles) now that most in-repo consumers use
project references. No product files change on this task.

Acceptance criteria:

- Every in-repo consumer of a package this repo builds is listed (samples, tests, templates,
  analyzers/generators, scripts, CI), with whether a ProjectReference works and what it would lose.
- Issue 647 is re-read, and the report says whether project references alone would have prevented it.
- The report recommends keep, remove, or narrow, and lists the follow-up tasks to file.
- `LocalNuGetCache` in `msbuild/repository.props` and `./LocalNugetFeed` in `scripts/clean.cs`
  are each called dead or live, with evidence.
- The report is at `report/analysis.md` in this task folder, follows tw-analysis-report, and has no time estimates.

## Requirements

- Inventory every place in the repo that consumes a TimeWarp.State package built in this repo via PackageReference or a local feed: samples, tests, templates, source generators and analyzers, scripts, CI.
- For each consumer, say whether a ProjectReference would work and what would be lost (for example testing the real packed artifact, analyzer/source-generator packaging, template validation).
- Re-read issue 647 and state whether project references would have prevented it.
- Recommend keep, remove, or narrow, and list the follow-up implementation task(s) to file.
- Determine whether the `LocalNuGetCache` property in `msbuild/repository.props` and the `./LocalNugetFeed` reference in `scripts/clean.cs` are dead leftovers.
- Output: a report in this task's kitchen `report/` folder (follow tw-analysis-report conventions).

## Checklist

- [x] Inventory in-repo package consumers (samples, tests, templates, analyzers/generators, scripts, CI)
- [x] Per consumer: ProjectReference feasibility and what is lost
- [x] Re-read issue 647; would project references have prevented it?
- [x] Check `msbuild/repository.props` `LocalNuGetCache` and `scripts/clean.cs` `./LocalNugetFeed` for dead code
- [x] Write report in `report/` with recommendation (keep / remove / narrow)
- [x] List follow-up implementation task(s) to file

## Session

- Created: ccd4893c-4c6d-47fb-9913-a5368de1e1b4 (2026-11-11)
- 2026-11-16 implement oracle (ganda task work): inventory, history, issue 647, report, gates.

## Notes

Research only; do not change product files. Current `nuget.config` has nuget.org as the only source and the comment says the packaged consumer passes `artifacts/packages` as an explicit restore source, so at least one packed-artifact consumer exists.

## Results

Report: `report/analysis.md`. Research only; no product files changed.

**Recommendation: narrow.** Sample 00 auto (restored by `dev verify-samples` through a generated config) and the packaged DevTools test (temp feed and temp packages folder) are the only lanes that restore a packed TimeWarp.State package. Every other in-repo consumer already uses ProjectReference (`9c7dda13`), and the root `nuget.config` stopped listing `artifacts/packages` (`00f3102f`). Isolation stays on the packed-artifact lane, and the pack guard stays. The repo-wide `RestorePackagesPath`, the root `globalPackagesFolder`, and the blanket `NUGET_PACKAGES` override go.

- Issue 647: project references plus the removed root feed stop the ambient poisoning. The remaining packaged consumer would still poison `~/.nuget/packages` without its own package folder, so it keeps isolation.
- Costs of the repo-wide setting: 4.5 GB `artifacts/nuget-packages` per worktree, and `~/.dotnet/toolResolverCache` left pointing into gc'd worktrees. That broke this task's first `dev test` run; see the gates below.
- Dead: `LocalNuGetCache` (`msbuild/repository.props:17`) and `./LocalNugetFeed` (`scripts/clean.cs:53`). `LocalNuGetFeed` is live (`source/Directory.Build.props:19`).

Follow-up tasks to file (details in the report's Recommendations):

1. Narrow NuGet restore isolation to the packaged-consumer lane (high).
2. Remove dead NuGet leftovers and stale comments (low; can fold into 1).

### How to validate

- **Smoke:** read `report/analysis.md`. Then run `grep -rn 'LocalNuGetCache\|LocalNugetFeed' --exclude-dir=artifacts --exclude-dir=kanban --exclude-dir=.git .` and `grep -rn 'PackageReference Include="TimeWarp.State' samples tests --include=*.csproj`.
- **Expect:** the first grep shows only the definition at `msbuild/repository.props:17` the literal at `scripts/clean.cs:53`, and a historical note in `analysis/powershell-scripts-analysis.md:98`. Nothing reads either value. The second grep shows only `sample-00-auto-client.csproj` and the two commented-out lines in `test-app-client.csproj`. The report has every required section and no time estimates.

### Gates

| Gate | Command | Result |
|------|---------|--------|
| Build | `./bin/dev build --quiet` | Pass (exit 0; 38 projects, sample 00 auto omitted by design) |
| Test | `./bin/dev test` | First run failed: `Run "dotnet tool restore" to make the "fixie" command available.` `~/.dotnet/toolResolverCache` pointed at the gc'd task-158 worktree. Cleared it, ran `dotnet tool restore`, and reran: pass (exit 0; all suites green). |
| Audit | `ganda repo audit` | Pass with one advisory: `vscode-window-icon` (`.vscode/settings.json` missing; that file is gitignored). `ganda repo audit --fix` created it, and the audit then passed all checks. No tracked file changed. |
