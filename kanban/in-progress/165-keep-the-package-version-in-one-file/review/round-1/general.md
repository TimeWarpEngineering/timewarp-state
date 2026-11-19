# Round 1 — general
**Date:** 2026-11-19
**Scope reviewed:** branch `task/165-keep-the-package-version-in-one-file` vs merge-base `b1bd4c0e` (`master...HEAD`), same scope as `review/review-framework.md`.

## Summary

`msbuild/repository.props` no longer stores a second version literal. At evaluation it reads `source/Directory.Build.props` with `File::ReadAllText` and takes the `<Version>` element text. `Directory.Packages.props` still pins `TimeWarp.State` and `TimeWarp.State.Blazor` to `$(TimeWarpStateVersion)`. An empty read fails the build through `InitialTargets="_TimeWarpStateAssertVersion"` (`TWSV001`), with no default. `AssertVersionSsot` stays in the PR and release pipelines: it reads `<Version>` as XML and compares it to `dotnet msbuild msbuild/repository.props -getProperty:TimeWarpStateVersion`. The stage id `assert-version-ssot` is unchanged, so `CiNoteInput.StageIds`, `scripts/ci-fast-path-gate.ps1`, and `ci-note-input-tests` still match. The version in `source/Directory.Build.props` is still `12.0.0-beta.18`, and that string is the only `12.0.0-beta.18` in `*.props`, `*.targets`, `*.csproj`, and `*.cs`.

The implementer restatement matches the original task. Keeping `assert-version-ssot` as a non-empty equality check is one of the two options the task allows. Comments and Purpose/Design regions in the named files describe one literal and a derived pin. `Directory.Build.props` still imports `repository.props` on its own; the derivation reads the source file from disk, so samples do not need to import `source/Directory.Build.props`. `source/Directory.Build.props` has a single bare `<Version>` element, so the regex, the XML reader, and the MSBuild `Version` property agree.

The recorded `## Results` cover this change: `./bin/dev build` pass, dev-cli self-install, `./bin/dev verify-samples` pass, `ganda repo audit` pass, and manual checks for the derived value, a one-file bump, an empty `<Version>`, a missing file, the success message, and a hand-typed mismatch. No test file asserts the old banner or reads `TimeWarpStateVersion` from `repository.props`. This review did not re-run build, test, or audit. Full `./bin/dev ci` remains the host CI run; the changed stage was exercised through `./bin/dev workflow --mode merge`.

## Issues

None.
