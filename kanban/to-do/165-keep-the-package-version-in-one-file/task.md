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

## Checklist

- [ ] `TimeWarpStateVersion` is derived from `source/Directory.Build.props` `<Version>`; no second literal remains in the repo
- [ ] Changing only `<Version>` in `source/Directory.Build.props` changes the version `Directory.Packages.props` pins for `TimeWarp.State` and `TimeWarp.State.Blazor` (show the evaluated value, for example with `dotnet msbuild -getProperty`)
- [ ] A missing or unreadable version fails the build with a clear message
- [ ] `assert-version-ssot` removed or reduced as described; comments and regions updated
- [ ] `./bin/dev ci` passes, including `verify-samples` for the packaged-consumer sample
- [ ] `ganda repo audit` passes

## Session

- Created: 531abc9c-a359-4f69-b125-2e4cabf1218a (2026-11-19)

## Notes

- Why: Steve 2026-11-19, "fix it in state so there's one version", after wave run
  20261118T170809Z-b6e473 parked task 164 on the mismatch.
- After this merges, task 164 can be resumed (`ganda task work 164`); its branch only has to
  take master, and the single `<Version>` bump it already made is then enough.
