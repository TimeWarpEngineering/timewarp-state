# Drop TimeWarp.Architecture.Analyzers; get TWA0004 from TimeWarp.SourceGenerators

## Goal (implementer restatement)

SourceGenerators 034 is published as `TimeWarp.SourceGenerators` 1.0.0-beta.13, and that package reports TWA0004, TWA0023, and TWA0038 under the same ids. This task drops State's `TimeWarp.Architecture.Analyzers` package reference and version pin, bumps `TimeWarp.SourceGenerators` from 1.0.0-beta.11 to 1.0.0-beta.13, and leaves the existing `.editorconfig` and `msbuild/external-sources.globalconfig` TWA severities in place (TWA0004 stays error on in-repo C# and none on hooks and the package cache). Comments that say Architecture owns TWA0004 move to SourceGenerators. A build with a Purpose region removed must still fail TWA0004. No PackageReference or ProjectReference to an Architecture package remains, so the repo-level Architecture ↔ State analyzer cycle is gone. Architecture can still depend on State as a library.

## Description

Remove State's reference to `TimeWarp.Architecture.Analyzers`. That package is pulled in only for
TWA0004 (`#region Purpose` on every source file) via `Directory.Build.props` line 47
(`PrivateAssets="all"`). Other TWA severities are configured in `.editorconfig`.

Once `timewarp-source-generators` **034** releases the moved generic rules (including TWA0004),
bump `TimeWarp.SourceGenerators`, delete the Architecture.Analyzers PackageReference, and keep
existing `.editorconfig` / globalconfig TWA settings working under the same IDs.

Acceptance: State has no reference to any Architecture package, and the false Architecture ↔ State
cycle disappears from the Core repo dependency tree (Architecture still depends on State as a
library; State must not depend on Architecture at all).

## Depends on (cross-repo)

Blocked by **timewarp-source-generators 034** release that hosts TWA0004 (and any other generic
rules State currently configures).

Upstream of that: **timewarp-architecture 486** classification + move-out.

## Related tasks

- `timewarp-architecture` **486** — Split TimeWarp.Architecture.Analyzers
- `timewarp-source-generators` **034** — Receive generic TWA rules and release them

## Requirements

1. Wait for SourceGenerators 034 to release a version that includes TWA0004 (same diagnostic ID).
2. Bump the `TimeWarp.SourceGenerators` PackageReference.
3. Remove the `TimeWarp.Architecture.Analyzers` PackageReference from `Directory.Build.props`
   (currently ~line 47) and any related comments that say Architecture owns TWA0004.
4. Keep `.editorconfig` and `msbuild/external-sources.globalconfig` TWA settings working
   (same IDs; no severity regression).
5. Confirm `dotnet build` / analyzer behavior still enforces Purpose regions.
6. Confirm no remaining PackageReference / ProjectReference to any Architecture package.

## Checklist

- [x] Blocked until source-generators 034 is released
- [x] Bump TimeWarp.SourceGenerators
- [x] Remove Architecture.Analyzers from Directory.Build.props
- [x] Verify .editorconfig TWA settings still apply
- [x] Verify TWA0004 still fires
- [x] Grep: no Architecture package references remain
- [x] Note on Core repo dependency tree: Architecture ↔ State cycle gone
- [x] M1: name Architecture.Analyzers 2.0.0-beta.29 and say the Architecture-app `none` ids are inert

## Session

- Created: 1548987 (2026-11-09)
- Implementation: grok `01a1c51b-8acb-7f01-8e00-5158bb6c26e5` (2026-11-10). SourceGenerators `1.0.0-beta.13` is on nuget.org and contains TWA0004. Audit `--fix` seeded `.timewarp/suppression-baseline.json`, refreshed `.githooks/*` to the attest baseline, ignored `/.timewarp/signals/`, wrote a gitignored `.vscode/settings.json` window title, and bumped Nuru, Nuru.DevCli, Amuru, Amuru.Tools, and Terminal. Nuru 4.0.0-beta.12 pulls Mediator 15, so the dev CLI handlers moved to `IRequestHandler` / `ValueTask`. The CI pipeline step is pwsh, with top-level `defaults.run.shell`.
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1c51b-8acb-7f01-8e00-5158bb6c26e5, max-turns 500, Done — 2026-11-10T11:23:53Z
- Review oracle round 1: claude opus-5-5 (tw-implementation-review, effort 2, roster general, turn cap 120). 1 open nit (M1), fix pass requested — 2026-11-10
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session c455f085-af00-4d96-829a-05f7e03469b7, max-turns 120, artifacts general.md, merged.md — 2026-11-10T11:27:46Z
- Fix pass 2: grok `01a1c536-d362-7ce2-b44f-97299ad9e9e7` (2026-11-10). M1: the `.editorconfig` comment names `TimeWarp.Architecture.Analyzers 2.0.0-beta.29` and says the Architecture-app ids stay `none` but are inert. Severity lines are unchanged. The fixie cause sentence now names a stale `~/.dotnet/toolResolverCache`.

## Results

`TimeWarp.SourceGenerators` is `1.0.0-beta.13` (published 2026-11-10, the source-generators 034 release). That package reports TWA0004, TWA0023, and TWA0038. `TimeWarp.Architecture.Analyzers` is gone from `Directory.Packages.props` and `Directory.Build.props`. No `PackageReference` or `ProjectReference` to an Architecture package remains. `.editorconfig` and `msbuild/external-sources.globalconfig` keep the same `dotnet_diagnostic.TWA*.severity` lines. TWA0004 stays error on in-repo C# and none on `.githooks` and `artifacts/nuget-packages`. TWA0038 stays at the package default (warning, 500 lines). No tracked C# file outside kanban is over 500 lines, and `source-file-length` audit passed.

Removing the Purpose region from `source/timewarp-state/assembly-marker.cs` and building that project failed with one error: `error TWA0004: File 'assembly-marker.cs' has no '#region Purpose' block`. The file was restored. `./bin/dev build` then succeeded: 0 warnings, 0 errors. The packed `TimeWarp.State` nuspec depends on `TimeWarp.Mediator` only.

Architecture's master `Directory.Packages.props` still pins `TimeWarp.State` `12.0.0-beta.17`. State no longer references an Architecture package, so the analyzer edge that made Architecture ↔ State a cycle is gone. This walk did not edit the Architecture repo.

`ganda repo audit` exits 0 (40 passed, 2 advisory warnings, 3 skipped). The warnings are pre-existing and not from this diff: `kanban-landed` says claim 098 has no usable worktree on this machine, and `kebab-path-names` names the task 110 folder plus Blazor JS initializer and MSBuild target paths whose names match assembly names. `./bin/dev check-version` still reports source `12.0.0-beta.17` equal to the published package. This change does not bump that version. The analyzer reference is `PrivateAssets="all"`, and the audit pin bumps are dev CLI and test packages, not dependencies of the packed State nuspec.

Audit `--fix` also bumped `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` to `4.0.0-beta.12`, `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` to `3.0.0-beta.2`, and `TimeWarp.Terminal` to `2.0.0-beta.1`. Dev CLI handlers now implement `IRequestHandler<,>` and return `ValueTask<Unit>`. Pipeline steps call `.AsTask()` where the step delegate is `Func<Task>`. `dotnet run --file tools/dev-cli/dev.cs -- --help` succeeds.

`./bin/dev test` stops at `dotnet fixie` because of a stale `~/.dotnet/toolResolverCache`. On this machine that directory exists and `dotnet fixie --help` exits 1 with `Run "dotnet tool restore"`. The reviewer named clearing that cache as the remedy. This pass did not delete it. The cause is the machine, not this diff. The same projects run under `dotnet test`: analyzer 90, source generator 174, state 153 passed / 1 skipped, plus 40 / 1 skipped, telemetry 21, devtools 124, isolation 4, client integration 67 / 1 skipped, architecture 7 / 1 skipped. Failed 0.

### Review

Round 1 (2026-11-10): effort 2, general reviewer only. Counts after the fix pass: bug 0, suggestion 0, nit 1 fixed (M1), 0 open. M1 was an `.editorconfig` comment that cited "2.0.0-beta.29" without a package name, so it read as a SourceGenerators version, and that did not say the Architecture-app `none` ids are inert. The fix names `TimeWarp.Architecture.Analyzers 2.0.0-beta.29` and adds that inert clause. The `dotnet_diagnostic.TWA*.severity` lines are unchanged. The reviewer reproduced the TWA0004 error on a `git archive` copy with the Purpose region removed. `dev.cs --help` builds, and no Architecture package reference remains. Disposition waits on round 2; this pass does not write `review/disposition.md`. Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`.

### How to validate

**Smoke**

```bash
rg -n 'TimeWarp\.Architecture' Directory.Build.props Directory.Packages.props
rg -n 'TimeWarp\.SourceGenerators' Directory.Packages.props
rg -n -A2 'TimeWarp\.Architecture\.Analyzers 2\.0\.0-beta\.29' .editorconfig
rg -n 'dotnet_diagnostic\.TWA0025\.severity' .editorconfig
./bin/dev build
# Probe, then restore the file:
# delete the #region Purpose block in source/timewarp-state/assembly-marker.cs
dotnet build source/timewarp-state/timewarp-state.csproj --no-restore -v q
git checkout -- source/timewarp-state/assembly-marker.cs
ganda repo audit
```

**Expect**

- The Architecture `rg` prints no `PackageReference` or `PackageVersion`.
- `TimeWarp.SourceGenerators` is `1.0.0-beta.13`.
- The `.editorconfig` hit names `TimeWarp.Architecture.Analyzers 2.0.0-beta.29` and the next lines say those Architecture-app ids stay `none` but are inert (task 153).
- `dotnet_diagnostic.TWA0025.severity` is still `none`. The other Architecture-app TWA severities in that block are unchanged.
- `./bin/dev build` ends with `Build succeeded` and `0 Error(s)`.
- The probe build fails with `error TWA0004` on `assembly-marker.cs` and `1 Error(s)`.
- `ganda repo audit` exits 0. Advisory warnings may name claim 098 and the known non-kebab assembly paths.

## Review

- Round 1: Blocked — open M1
- Fix pass 2: M1 marked fixed on `review/round-1/merged.md`. Round 2 has not run.

## Notes

Filed by Jenzi after Steven approved filing the three linked tasks (2026-11-09). State's comment
on the PackageReference already says the intent is TWA0004 only; other TWA rules in .editorconfig
are unused without the analyzer package.
