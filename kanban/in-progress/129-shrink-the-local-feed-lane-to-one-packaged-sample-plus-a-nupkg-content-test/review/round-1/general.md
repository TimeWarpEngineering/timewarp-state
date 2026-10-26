# Round 1 — general
**Date:** 2026-10-26
**Scope reviewed:** branch vs merge-base with `master` (`00f3102f` regression fix, `9c7dda13` sample move + nupkg content test + docs)

## Summary

The change drops `LocalNuGetFeed` from root `nuget.config`, so a fresh worktree restores with no `artifacts/packages` folder. That is requirement 1, and it lands as a separate commit (`00f3102f`), as the Notes asked. Nine sample projects move to `ProjectReference`, including the analyzer and source generator as `OutputItemType="Analyzer"`. Sample 00 auto stays on `PackageReference` and leaves `timewarp-state.slnx`. `verify-samples` now packs, then restores sample 00 auto through a generated config that maps `TimeWarp.State*` to the feed. The new `timewarp-state-packaging-tests` project runs inside `dev pack`.

I built and ran the content test against the current `artifacts/packages`: it passes with 1 test. It matched the Blazor module through the real `build/TimeWarp.State.Blazor.PackageAssets.json` entry (`BasePath: _content/TimeWarp.State`, `RelativePath: js/TimeWarp.State.Blazor#[.{fingerprint}]!.lib.module.js`), so the pass does not depend on the fallback branch. The docs, CI comment, and dev-cli Design regions match the new flow.

The main risk is in the packaged-consumer gate. On a machine that has run it before, it can pass against an older extracted package instead of the one just packed (Issue 1). There is one weaker assertion and one parsing nit in the content test.

Goal restatement vs task: no misread. The restatement covers Requirements 1–7 and the Notes constraint ("isolated cache from task 125 stays as it is").

## Issues

### Issue 1 — Severity: bug
- File: tools/dev-cli/endpoints/verify-samples-command.cs:99
- Description: `verify-samples` packs a fresh `TimeWarp.State` / `TimeWarp.State.Blazor` at the same version (`12.0.0-beta.16`), then restores sample 00 auto with `NUGET_PACKAGES=artifacts/nuget-packages`. NuGet checks the global packages folder first. When `artifacts/nuget-packages/timewarp.state/12.0.0-beta.16/.nupkg.metadata` already exists from an earlier run, NuGet uses that extraction and never reads the new nupkg. Verified in this worktree: the cached `.nupkg.metadata` was written at 19:50:11 with `contentHash` `r4tHpL1d…`. The current `artifacts/packages/TimeWarp.State.12.0.0-beta.16.nupkg` was written at 19:53:13, and its SHA-512 is `GNdge4KA…`. So the last local `verify-samples` built sample 00 auto against an older pack than the one the content test checked. The task now relies on this one consumer to prove the packed server, WebAssembly, analyzer, and generator bits. Locally that gate can stay green after a packaging regression. This is the "same-version repack shadowing" incident the task description lists, moved into the isolated cache. CI starts with a fresh runner, so it is not affected there.
- Suggestion: After packing and before the sample 00 auto restore, delete `artifacts/nuget-packages/<lowercased id>/<version>` for each packable id. Those ids are already available from `PackableProjectService`. Then delete or force (`--force`) the restore of the two sample 00 auto projects. Alternatively, run that restore and build with `NUGET_PACKAGES` pointed at a per-run folder such as `artifacts/sample-00-auto-packages`. The env var overrides the `globalPackagesFolder` in the generated config, so changing only the config would not work. Either way, keep task 125's no-`~/.nuget/packages` guarantee.
- Status: open

### Issue 2 — Severity: suggestion
- File: tests/timewarp-state-packaging-tests/nupkg-content-tests.cs:68
- Description: The `TimeWarp.State.Plus` case passes when either `document-title.js` or `download-file.js` is present and paired. Both files ship from `source/timewarp-state-plus/wwwroot/js/` and both are in the current nupkg. If one is dropped, the test still passes, which goes against requirement 2 ("A missing entry fails the test"). The pair-failure message also names only `document-title.js`.
- Suggestion: Assert each script on its own, with the `staticwebassets/js/<name>` entry and a `HasAssetPair(..., PlusContentBase)` check, as the Blazor case does through `AssertStaticWebAsset`.
- Status: open

### Issue 3 — Severity: nit
- File: tests/timewarp-state-packaging-tests/nupkg-content-tests.cs:134
- Description: `HasAssetPair` runs `JsonDocument.Parse` on every `.json` entry in the package. That includes `contentFiles/any/net11.0/tsconfig.json` in the Blazor nupkg. tsconfig is JSONC, so a comment or trailing comma added there would throw `JsonException`, and the test would fail on an unrelated file with an unhelpful message.
- Suggestion: Parse only the static web asset manifests (`build/*.json` / `buildTransitive/*.json`, or names ending `.PackageAssets.json`). Or pass `JsonDocumentOptions { CommentHandling = Skip, AllowTrailingCommas = true }`.
- Status: open

## Out-of-scope observation (not a finding)

- The Blazor nuspec ships `contentFiles/any/net11.0/tsconfig.json` with `buildAction="Content"`, so every consumer gets a tsconfig as Content. That predates this change. If it is unintended, it belongs in a follow-up task. The content test could later assert it is absent.
