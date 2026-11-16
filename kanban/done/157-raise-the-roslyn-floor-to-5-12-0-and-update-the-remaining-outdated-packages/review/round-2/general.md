# Round 2 — general
**Date:** 2026-11-16
**Scope reviewed:** delta since round-1 base `87667996` (commit `70df11fc`). Product files: `readme.md`, `documentation/partials/installation.md`. Re-checked M1. The goal restatement, including the fix-pass paragraph, matches the task. No misread.

## Summary

The fix inserts "that bundle an older Roslyn" into the unsupported-SDK sentence in the repo readme and the installation partial. The minimum stays .NET SDK 11.0.100 (GA). That matches the task decision, the pin comment in `Directory.Packages.props`, and `documentation/release-notes/release12.0.0-beta.18.md`. No other product sentence still says every .NET 11 preview or RC SDK is unsupported.

`unzip -p artifacts/packages/TimeWarp.State.12.0.0-beta.18.nupkg readme.md` shows the same qualified sentence. Reason: round 1 found the old sentence in that nupkg, and the fix record claims a rebuild. The qualified sentence is present. The fix-pass `## Results` cover `./bin/dev build` (0 warnings, 0 errors), `./bin/dev test` (exit 0), and `ganda repo audit` (exit 0; only the pre-existing `kebab-path-names` advisory). This review did not re-run those gates. `ganda nuget outdated` was not re-run on this delta; the edit does not change pins, and the earlier record still covers currency.

## Prior findings

### M1 — Severity: suggestion — Status: fixed
- File: readme.md:44
- Description: Verified. `readme.md:44` and `documentation/partials/installation.md:8` now say ".NET 11 preview and RC SDKs that bundle an older Roslyn are not supported." The packed readme carries that sentence. The minimum stays .NET SDK 11.0.100 (GA).

## Issues

No new issues.
