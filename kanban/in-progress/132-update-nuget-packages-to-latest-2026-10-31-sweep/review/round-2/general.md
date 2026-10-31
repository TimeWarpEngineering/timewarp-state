# Round 2 — general
**Date:** 2026-10-31
**Scope reviewed:** delta `f340c819..a1876db4` (commit a1876db4 "fix: turn off the two beta.29 rules the sweep missed"): `.editorconfig`, `msbuild/external-sources.globalconfig`, `documentation/release-notes/release12.0.0-beta.16.md`, task.md Results/Goal.

## Summary

The fix pass closes both round-1 findings. TWA0025 and TWA0035 are now `none` in the `[*.cs]` block and in `msbuild/external-sources.globalconfig`. That globalconfig is wired for every project through `GlobalAnalyzerConfigFiles` in `Directory.Build.props:53`. All ten beta.29 ids are now named in the `.editorconfig` comment, the beta.16 release notes, and Results Exceptions. The dead `[*.razor]` block is gone, and both config comments now agree that the globalconfig silences Razor TWA0036. Risk is low. The change is config and docs only.

## Re-verification of prior findings

- M1 (suggestion): fixed. `.editorconfig:326` and `:333` set TWA0025 and TWA0035 to `none`. `msbuild/external-sources.globalconfig` lists the same ten ids. The release-notes line and the Results Exceptions paragraph list all ten.
- M2 (nit): fixed. The `[*.razor]` section is removed. The `.editorconfig` comment (lines ~295-301) and the globalconfig comment (lines 14-18) give the same explanation. The Results section records the implementer's sample 00 probe that backs it.

## Independent check

- `dotnet build -c Release` on the four sample 00 projects (server, wasm, auto, auto-client): all four succeeded with no `TWA0` diagnostics. The only warning shown was a pre-existing BL0010 in `timewarp-state-blazor`.

## Issues

None.

Goal restatement still matches the original task. The new paragraph only adds the round-1 fix scope, so there is no misread.
