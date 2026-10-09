# Round 1 — general
**Date:** 2026-10-09
**Scope reviewed:** Directory.Packages.props, samples/04-telemetry/apphost/sample-04-apphost.csproj, tests/test-app-end-to-end-tests/test-app-end-to-end-tests.csproj, Amuru call sites in .githooks/, scripts/, tools/dev-cli/

## Summary

The change raises the central package pins. It moves Amuru to 2.0.0-beta.2 and MSTest to 4.5.1. To keep Playwright working on MSTest 4, it swaps in Microsoft.Playwright.MSTest.v4 1.63.0. It also bumps Aspire to 13.6.1, in both the CPM pin and the sample `Aspire.AppHost.Sdk` pin, and coverlet to 10.1.0.

Results record why two packages are held:
- Microsoft.CodeAnalysis.CSharp stays at 4.14.0 because it sets the minimum compiler version consumers need.
- Shouldly stays at 4.3.0 because the 5.0 preview removes overloads this repo uses.

Both holds are justified, and the task Notes asked for exactly this kind of record when a package can't move.

Checks:
- I built every runfile under `.githooks/`, `scripts/`, and `tools/dev-cli/dev.cs` against Amuru 2.0. All compiled.
  - `scripts/global-usings.cs` reports errors, but it is a shared include, not a standalone runfile, so that is expected.
- Amuru usage is limited to `Git.FindRoot`, the `DotNet.*` builders, and `Shell.Builder`. None of these use the APIs removed in 2.0.
- The `global using Microsoft.Playwright.MSTest` namespace is unchanged in the v4 package.
- No other project references the old Playwright package ID.

## Issues

None.
