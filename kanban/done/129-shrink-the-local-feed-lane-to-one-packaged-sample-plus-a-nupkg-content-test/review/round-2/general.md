# Round 2 — general
**Date:** 2026-10-26
**Scope reviewed:** fix delta `d2014bcc..53b494a9` (product commit `6fa0f6b8`, kitchen commits `0f30f3ad`, `53b494a9`). Files: `tools/dev-cli/endpoints/verify-samples-command.cs`, `tools/dev-cli/endpoints/nuget-package-isolation.cs`, `tests/timewarp-state-packaging-tests/nupkg-content-tests.cs`, `tests/timewarp-state-tests/packaging/nuget-package-isolation-tests.cs`, and the four doc files (`claude.md`, `documentation/contributing/overview.md`, `samples/overview.md`, `scripts/overview.md`).

## Summary

The fix pass resolves all three round-1 findings. After pack, `verify-samples` deletes each packable id's same-version folder under `artifacts/nuget-packages`, then restores sample 00 auto with `--force`. The Plus layout now asserts both scripts on their own. JSON pairing reads only static web asset manifests. Risk is low and no new defects were found.

## Re-verification of prior findings

- **M1 (bug): fixed.** `PurgePackedExtractsAsync` (`verify-samples-command.cs:218`) runs after pack and before the packaged restore (`:102`). The restore passes `--force` (`:189`). A missing version or zero packable projects fails closed, with an error and exit code 1. IO errors are caught and reported. `DeleteVersionFolders` (`nuget-package-isolation.cs:171`) reuses `FindVersionFolder`, dedupes ids case-insensitively, and leaves other versions and other ids alone (two new unit tests cover this). Checked on disk: `artifacts/nuget-packages/timewarp.state/12.0.0-beta.16/.nupkg.metadata` was written at 20:27, after the nupkgs (20:22–20:24). Its `contentHash` `sOKW/aPZ…lrQTDg==` equals `openssl dgst -sha512 -binary | base64` of `artifacts/packages/TimeWarp.State.12.0.0-beta.16.nupkg`.
- **M2 (suggestion): fixed.** The Plus case calls `AssertStaticWebAsset` once for `document-title.js` and once for `download-file.js` (`nupkg-content-tests.cs:145-146`). `FailPlusWhenDownloadFileIsMissing` shows that a package with only one script throws `ShouldAssertException`.
- **M3 (nit): fixed.** `IsStaticWebAssetManifest` (`nupkg-content-tests.cs:226`) limits JSON parsing to `build/`, `buildTransitive/`, and `*.PackageAssets.json`. The real Plus and Blazor nupkgs keep their manifest at `build/<id>.PackageAssets.json`, so the real-package assertion still runs. The Blazor `contentFiles/any/net11.0/tsconfig.json` is now skipped. `IgnoreACommentInContentFilesTsconfig` and `PairAManifestWhenThePackageAlsoContainsJsonc` cover the JSONC case.

## Commands run

- `dotnet test tests/timewarp-state-packaging-tests`: 4 passed, 0 failed (this includes `MatchTheLayoutOfEveryPackedNupkg` against the current `artifacts/packages`).
- `dotnet test tests/timewarp-state-tests`: 128 passed, 1 skipped (a pre-existing convention example), 0 failed. This includes the two new `NuGetPackageIsolation_Should_` cases.
- `unzip -l` on the Plus and Blazor nupkgs to confirm where the manifests sit.

## Issues

None.

## Goal check

The fix-pass paragraph in `## Goal (implementer restatement)` matches the round-1 findings and the original Requirements. It keeps task 125's rule that the packed version is not written under `~/.nuget/packages`. It also explains why `NUGET_PACKAGES` was not used, which is correct because `RestorePackagesPath` takes precedence. No misread.
