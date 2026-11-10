# Round 2 — general
**Date:** 2026-11-10
**Scope reviewed:** fix delta since round 1, `bd1a92fb..fb00d08f`. That is `.editorconfig` comment lines 294–300, plus the kitchen edits to `task.md` and `review/round-1/merged.md`. Effort 1 (by-diff, 107 lines).

## Summary

The fix pass changes comments only. It does not touch any `dotnet_diagnostic.*.severity` line. The `2.0.0-beta.29` sentence now names `TimeWarp.Architecture.Analyzers`. A new clause says the Architecture-app ids stay `none` but are inert, because State no longer references that package. That resolves M1.

I checked the inert claim against the package. `strings` on `~/.nuget/packages/timewarp.sourcegenerators/1.0.0-beta.13/analyzers/dotnet/cs/timewarp-source-generators.dll` lists only TW0001, TW0007, TWA0004, TWA0023, and TWA0038. No other TWA id in that block is reported by a referenced package. The severity lines from TWA0002 through TWA0025 are unchanged (requirement 4). The fixie sentence in Results now names the stale `~/.dotnet/toolResolverCache`, which was the optional part of the round-1 suggestion.

Goal restatement vs task: they match. Bump SourceGenerators, drop Architecture.Analyzers, keep the TWA severities, TWA0004 still fires, and no Architecture reference remains. No misread.

## Prior findings

- M1 (nit) — fixed. Verified at `.editorconfig:294-300`.

## Issues

None.

## Observations (not findings)

- The rewrap left a short ragged line, `reason. TWA0036 (ISO 8601 dates) warns on`, at `.editorconfig:300`. "stay none" also now appears twice in a row (lines 294–295). This is cosmetic only, so it is not raised.
- No build was rerun for this delta, because comment-only `.editorconfig` edits do not change analyzer behavior. Round 1 recorded the TWA0004 probe and the build.
