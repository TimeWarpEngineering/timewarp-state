# Keep the package version in one file

## Description

The package version is written out twice:

- `source/Directory.Build.props` `<Version>` is the version the packages are packed with. This
  is the TimeWarp convention and the place `ganda task wave`, `dev check-version`, and
  `dev release` read.
- `msbuild/repository.props` `<TimeWarpStateVersion>` feeds `Directory.Packages.props`, which
  pins `TimeWarp.State` and `TimeWarp.State.Blazor` at that value for the one packaged-consumer
  sample ("00 auto") that `dev verify-samples` restores against the freshly packed packages.

`dev ci` runs `assert-version-ssot` first and fails when the two differ. Every version bump has
to edit both files. The first `ganda task wave` run (20261118T170809Z-b6e473) bumped only
`source/Directory.Build.props` to 12.0.0-beta.19 and parked task 164 at local CI:

```
Version SSOT mismatch: source/Directory.Build.props Version='12.0.0-beta.19',
msbuild/repository.props TimeWarpStateVersion='12.0.0-beta.18'. Align both.
```

### Goal

One literal version in the repo. It stays in `source/Directory.Build.props` `<Version>`,
because that is where the org tooling reads and writes it. `TimeWarpStateVersion` is derived
from that file and is never typed by hand.

### Approach

- In `msbuild/repository.props`, replace the literal `<TimeWarpStateVersion>` with a value
  read from `source/Directory.Build.props` `<Version>` at evaluation time (for example an
  MSBuild property function that reads the file and matches the `<Version>` element).
  `repository.props` must stay importable on its own: the samples and `Directory.Packages.props`
  import it without `source/Directory.Build.props`.
- Fail the build with a clear message if the read finds no version. Do not fall back to a
  default value.
- `assert-version-ssot` (`tools/dev-cli/endpoints/workflow/repo-version-service.cs`) no longer
  has two literals to compare. Either remove the step from the `dev ci` pipeline, or keep it as
  a check that the derived value is non-empty and equals `<Version>`. Pick one and update the
  Purpose and Design regions and the pipeline line that `dev ci` prints.
- Update the comments that describe two files that must match
  (`msbuild/repository.props`, `tools/dev-cli/endpoints/build-command.cs`,
  `tools/dev-cli/endpoints/workflow-command.cs`, `repo-version-service.cs`).

### Out of scope

- Do not change the version number. Master stays at 12.0.0-beta.18. Task 164 carries the
  beta.19 bump and the Aspire.Hosting 17.0.0 pin.
- Do not change which samples use package references.

## Goal (implementer restatement)

The repo should type the TimeWarp.State package version exactly once, in
`source/Directory.Build.props` `<Version>`. `msbuild/repository.props` must compute
`TimeWarpStateVersion` from that file at evaluation time (still importable on its own), so a
bump of `<Version>` alone moves the CPM pins for `TimeWarp.State` and `TimeWarp.State.Blazor`.

Acceptance:

- No second version literal remains; `dotnet msbuild -getProperty:TimeWarpStateVersion` shows the
  derived value, and it follows an edit of `<Version>`.
- If the version cannot be read, the build stops with a clear error; no default value.
- `assert-version-ssot` is kept (its stage id stays in the CI note) but reduced: it evaluates the
  derived `TimeWarpStateVersion` with MSBuild and checks it is non-empty and equals `<Version>`.
  Purpose/Design regions, the printed pipeline line, and the "two files must match" comments are updated.
- Version number unchanged (12.0.0-beta.18); sample package-reference choice unchanged.
- Gates: `./bin/dev build`, the covering tests, `ganda repo audit` pass.

## Checklist

- [x] `TimeWarpStateVersion` is derived from `source/Directory.Build.props` `<Version>`; no second literal remains in the repo
- [x] Changing only `<Version>` in `source/Directory.Build.props` changes the version `Directory.Packages.props` pins for `TimeWarp.State` and `TimeWarp.State.Blazor` (show the evaluated value, for example with `dotnet msbuild -getProperty`)
- [x] A missing or unreadable version fails the build with a clear message
- [x] `assert-version-ssot` removed or reduced as described; comments and regions updated
- [ ] `./bin/dev ci` passes (verify-samples and the assert step run locally; full test/e2e left to CI per oracle rules — see Results), including `verify-samples` for the packaged-consumer sample
- [x] `ganda repo audit` passes

## Session

- Created: 531abc9c-a359-4f69-b125-2e4cabf1218a (2026-11-19)
- 2026-11-19 implement oracle (Claude Opus 5.5): derived TimeWarpStateVersion, reduced assert-version-ssot, comments. Boy-scout: `ganda repo audit --fix` fixed the advisory `vscode-window-icon` warning (.vscode/settings.json is git-ignored, nothing to commit).
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5), session not reported, max-turns 500, Done — 2026-11-19T01:42:35Z

## Notes

- Why: Steve 2026-11-19, "fix it in state so there's one version", after wave run
  20261118T170809Z-b6e473 parked task 164 on the mismatch.
- After this merges, task 164 can be resumed (`ganda task work 164`); its branch only has to
  take master, and the single `<Version>` bump it already made is then enough.

## Results

### Changes

- `msbuild/repository.props`: the `TimeWarpStateVersion` literal is gone. A `Product Version` group reads
  `$(SourceDirectory)Directory.Build.props` with `File::ReadAllText` and takes the `<Version>` element text with
  `Regex::Match(... '(?<=<Version>)[^<]*(?=</Version>)').Value.Trim()`. No fallback. `<Project InitialTargets=
  "_TimeWarpStateAssertVersion">` runs an `Error` (code `TWSV001`) when the value is empty, which covers a missing
  file, a missing element, and an empty element. The file still imports on its own.
- `tools/dev-cli/endpoints/workflow/repo-version-service.cs`: `AssertVersionSsot` is kept (stage id
  `assert-version-ssot` unchanged, so the CI note, `scripts/ci-fast-path-gate.ps1`, and `ci-note-input-tests` stay
  valid). It is now async. It reads `<Version>` and evaluates `dotnet msbuild msbuild/repository.props
  -getProperty:TimeWarpStateVersion`, then checks that the derived value is non-empty and equal. Purpose and Design
  regions are rewritten. Callers in `pr-pipeline.cs` and `release-pipeline.cs` await it.
- `pr-pipeline.cs` now prints `Pipeline: assert-version-ssot (derived TimeWarpStateVersion == Version) → clean → …`
  and its Design region gained a note.
- Comments updated in `workflow-command.cs`, `build-command.cs`, and `Directory.Packages.props`.
- Version stays at 12.0.0-beta.18. Sample package-reference choices are unchanged.

### Gates

- `./bin/dev build`: pass (the 40-project filter builds; sample 00 auto is left out until pack).
- `dotnet run --file tools/dev-cli/dev.cs -- self-install`: pass. The dev-cli changed, so it was recompiled.
- Test files: none were added or edited. No test covers `RepoVersionService`, `PrPipeline`, `ReleasePipeline`, or
  `repository.props` (searched `tests/` for those names). `ci-note-input-tests.cs` checks the stage ids, which did
  not change. Not run; CI runs the full suite.
- `./bin/dev verify-samples`: pass. It packed the packages, then restored and built sample 00 auto against
  `TimeWarp.State` 12.0.0-beta.18 from the local feed.
- `ganda repo audit`: pass after `--fix`. The first run passed with one advisory warning (`vscode-window-icon`);
  `--fix` cleared it and the rerun shows "Repository passes all audit checks."
- `./bin/dev ci` was not run in full. It refuses a dirty tree, and it runs the full test and e2e suites, which this
  oracle leaves to CI. Its first stage was exercised through `./bin/dev workflow --mode merge` (the same PrPipeline;
  see below).

### Evidence

- `dotnet msbuild msbuild/repository.props -getProperty:TimeWarpStateVersion` → `12.0.0-beta.18`. The project
  context (`source/timewarp-state/timewarp-state.csproj`) shows `TimeWarpStateVersion` and `Version` both
  `12.0.0-beta.18`.
- Changing only `<Version>` to `12.0.0-beta.99` in `source/Directory.Build.props` (temporary edit, then reverted):
  `-getProperty:TimeWarpStateVersion` gave `12.0.0-beta.99`. `-getItem:PackageVersion` showed `TimeWarp.State` and
  `TimeWarp.State.Blazor` both at `12.0.0-beta.99`.
- With an empty `<Version></Version>`, restore failed with `error TWSV001: TimeWarpStateVersion is empty: no
  <Version> element with a value was read from '.../source/Directory.Build.props'. ...`
- With `source/Directory.Build.props` moved away, evaluating `repository.props` on its own failed with the same
  `TWSV001`.
- `./bin/dev workflow --mode merge`, success path (stopped once the check had printed):
  `Version SSOT: 12.0.0-beta.18 (msbuild/repository.props TimeWarpStateVersion is derived from source Version)`.
- With a hand-typed `TimeWarpStateVersion` literal of 12.0.0-beta.17: `Version SSOT: ... Version='12.0.0-beta.18',
  but msbuild/repository.props evaluates TimeWarpStateVersion='12.0.0-beta.17'...` → `Pipeline FAILED — Assert
  Version SSOT failed`.
- With an empty `<Version>`: `Version SSOT: source/Directory.Build.props has no <Version> value.` → pipeline failed.

### How to validate

Smoke:

```bash
dotnet msbuild msbuild/repository.props -getProperty:TimeWarpStateVersion
sed -i 's|<Version>12.0.0-beta.18</Version>|<Version>12.0.0-beta.99</Version>|' source/Directory.Build.props
dotnet msbuild source/timewarp-state/timewarp-state.csproj -getItem:PackageVersion | grep -A3 '"TimeWarp.State'
git checkout source/Directory.Build.props
git grep -n "12.0.0-beta.18" -- "*.props" "*.targets" "*.csproj" "*.cs"
```

Expect:

- The first command prints `12.0.0-beta.18`.
- After the temporary edit, `TimeWarp.State` and `TimeWarp.State.Blazor` both pin `12.0.0-beta.99`.
- `git grep` over build files finds the version only in `source/Directory.Build.props`. Release-note and readme links in `documentation/` and `readme.md` are prose and stay as they are.
- On the pull request, CI's `assert-version-ssot` stage prints `Version SSOT: 12.0.0-beta.18 (... derived ...)`,
  and `verify-samples` passes.

