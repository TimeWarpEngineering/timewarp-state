# Round 1 — general
**Date:** 2026-09-23
**Scope reviewed:** `.gitignore`, `Directory.Packages.props`, `source/timewarp-state/tsconfig.json`,
  generated `source/timewarp-state/wwwroot/js/*.js(.map)` — branch vs master.

## Summary

This is a mechanical dependency-update task: every NuGet package version bumped in
`Directory.Packages.props`, with two majors deliberately held back and documented inline
(MSTest 4 breaks Playwright.MSTest test discovery; `Microsoft.CodeAnalysis.CSharp` 5.9.0 raises
the SDK floor past the analyzer/source-generator's target). No product `.cs` changed. Risk is
low and independently re-verified: `dev check-version` still accepts `12.0.0-beta.5` for all four
packages, and `dev build` completes with 0 errors (102 pre-existing warnings, none new). The task's
own recorded `dev workflow` run (170 unit + 10 E2E passed, 0 failed; pack + verify-samples
succeeded) is consistent with a clean dependency bump.

The TypeScript 7.0.1 bump required a `tsconfig.json` change (`moduleResolution`: `"node"` →
`"bundler"`, plus explicit `rootDir`) and recompiled `wwwroot/js/*.js(.map)` output. Diffed the
generated JS by hand: TS 7's emit now elides uninitialized class-field declarations (e.g.
`jsonRequestHandler;`, `IsEnabled;`) instead of emitting them as explicit `undefined`-valued
fields, and downlevels field initializers with values (`IsInitialized = false`, the
`MessageHandler` arrow-function field) into the constructor instead of native class-field syntax.
Both are functionally equivalent to the prior output for this codebase — JS returns `undefined`
for both an elided property and an explicitly-declared-but-unset one, and the constructor-injected
initializers preserve the original declaration order (they still run before the rest of the
constructor body). This is TS-toolchain-generated output, not a hand edit, and is covered by the
green `dev workflow` run (Redux DevTools / DispatchRequest interop is exercised by the E2E suite).
Worth a mention in Results for anyone tracing the JS diff later, but not a defect.

Also noted: `Directory.Packages.props` gained a UTF-8 BOM (tooling artifact from
`ganda nuget outdated --update --force`), and `.gitignore`'s `.memsearch/` line now subsumes the
narrower `.memsearch/.index.pid` line it replaced. Both are harmless.

## Issues

None. Diff is a well-scoped, well-documented dependency bump; the one non-trivial generated-output
change (class-field emit shape in the recompiled JS) was hand-verified as behavior-preserving.
