# Update all NuGet packages to latest (incl. TimeWarp.Amuru 2.0.0-beta.2)

## Description

Steven wants every repo on the newest packages, pre-releases included (latest, not stable). Run `ganda nuget outdated --update` and take every package to the newest version it can, not just Amuru. TimeWarp.Amuru and TimeWarp.Amuru.Tools must end on 2.0.0-beta.2 or newer.

## Checklist

- [ ] `ganda nuget outdated --dry-run`, then `ganda nuget outdated --update`
- [ ] Bump `#:package ...@version` pins in runfiles; .githooks are ganda-owned, refresh via `ganda hooks install attest` instead of editing
- [ ] Fix Amuru 1.x->2.0 breaking changes (timewarp-amuru documentation/release-notes/2.0.0.md): Git *Master* helpers removed (use *Default*), Git methods return result objects not bool, no "master" default branchName, removed/renamed dotnet builder options, WithStandardInput("") closes stdin
- [ ] Fix other breaking changes from bumped packages
- [ ] Build warning-free, tests green, `ganda repo audit` clean
- [ ] One PR, merge via `ganda pr merge`

## Notes

Filed 2026-10-09 at Steven's request (Amuru 2.0 sweep). If a package can't move (e.g. a dependency cycle), record why instead of forcing it.

Update 2026-10-09: PR #622 (099-001, merged as 61205ccc) already updated Directory.Packages.props (added TimeWarp.Architecture.Analyzers 2.0.0-beta.19). Start from master after #622 and only take whatever is still outdated: re-run `ganda nuget outdated --dry-run` first.
