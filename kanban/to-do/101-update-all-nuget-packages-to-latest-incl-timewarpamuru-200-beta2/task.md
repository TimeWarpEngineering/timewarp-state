# Update all NuGet packages to latest (incl. TimeWarp.Amuru 2.0.0-beta.2)

## Description

Steven wants every repo on the newest packages, pre-releases included (latest, not stable). Run `ganda nuget outdated --update` and take every package to the newest version it can, not just Amuru. TimeWarp.Amuru and TimeWarp.Amuru.Tools must end on 2.0.0-beta.2 or newer.

## Checklist

- [x] `ganda nuget outdated --dry-run`, then `ganda nuget outdated --update`
- [x] Bump `#:package ...@version` pins in runfiles; .githooks are ganda-owned, refresh via `ganda hooks install attest` instead of editing
- [x] Fix Amuru 1.x->2.0 breaking changes (timewarp-amuru documentation/release-notes/2.0.0.md): Git *Master* helpers removed (use *Default*), Git methods return result objects not bool, no "master" default branchName, removed/renamed dotnet builder options, WithStandardInput("") closes stdin
- [x] Fix other breaking changes from bumped packages
- [x] Build warning-free, tests green, `ganda repo audit` clean
- [ ] One PR, merge via `ganda pr merge`

## Results

`ganda nuget outdated --update --force` moved the six stable-band updates it reports. Amuru was pinned by hand because that command stays in the stable band and still calls 1.1.1 current. Runfiles have no `#:package …@version` pins; central versions flow through (`dotnet list` resolves TimeWarp.Amuru and TimeWarp.Amuru.Tools 2.0.0-beta.2 on `tools/dev-cli/dev.cs` and `scripts/build.cs`). No `.githooks` edits.

Moved:

- TimeWarp.Amuru and TimeWarp.Amuru.Tools 1.1.1 → 2.0.0-beta.2
- MSTest.TestAdapter and MSTest.TestFramework 3.11.1 → 4.5.1, with the E2E reference switched from Microsoft.Playwright.MSTest to Microsoft.Playwright.MSTest.v4 1.63.0 (the non-v4 package still binds the MSTest 3 assembly, so PageTest subclasses would not load)
- coverlet.collector 10.0.1 → 10.1.0
- Aspire.Hosting.AppHost 13.5.4 → 13.6.1, and the sample-04 `Aspire.AppHost.Sdk` pin to 13.6.1

No Amuru 2.0 call sites needed edits. This repo uses `Git.FindRoot` and the dotnet/shell builders that 2.0 still ships. It does not call the removed Git `*Master*` helpers, bool-returning Git methods, or the removed dotnet builder options.

Held:

- Microsoft.CodeAnalysis.CSharp stays 4.14.0. The outdated tool offers 5.9.0. That assembly version loads on SDK 10.0.400+ (Roslyn 5.9) and not on SDK 10.0.301/302 (Roslyn 5.6). The analyzer and source generator ship inside TimeWarp.State, so 4.14.0 remains the consumer compiler floor.
- Shouldly stays 4.3.0. 5.0.0-preview.2 is newer and is a preview that removes params overloads (`ShouldBeOneOf(1, 2, 3)` becomes a collection expression). The stable-band tool does not select it.
- Blazored.LocalStorage 4.5.0 and Blazored.SessionStorage 2.4.0 are already the newest versions on the NuGet flat container. `ganda nuget outdated` reports both as not found.

`dotnet build` via `dev build` exits 0. Pre-existing warnings remain (TW0007, RS0030, BL0010, BL0016, NU1510, ASPDEPR011, IL2026, IL2111). `ganda repo audit` exits 0 with two pre-existing advisories (kebab path `tests/test-app/test-app-client/wwwroot/Test.App.Client.lib.module.js`, leftover `.memsearch.toml`).

### How to validate

Smoke:

- `dotnet list tools/dev-cli/dev.cs package` and `dotnet list scripts/build.cs package`
- `mkdir -p artifacts/packages && dotnet run --file scripts/test.cs`
- `dotnet run --file tools/dev-cli/dev.cs -- e2e`
- `dotnet test tests/test-app-end-to-end-tests/test-app-end-to-end-tests.csproj --list-tests --nologo`
- `ganda repo audit`

Expect:

- Both `dotnet list` rows show TimeWarp.Amuru and TimeWarp.Amuru.Tools requested and resolved at 2.0.0-beta.2.
- `scripts/test.cs` exits 0: analyzer 33 passed, source generator 15 passed, state 73 passed and 1 skipped, plus 31 passed and 1 skipped, telemetry 13 passed, client integration 56 passed and 1 skipped, architecture 7 passed and 1 skipped.
- E2E exits 0 with Failed 0, Passed 11, Skipped 3, Total 14. `--list-tests` lists the PageTest methods (including TestCounterComponents and TestChangeRoute), not "No test is available".
- `ganda repo audit` exits 0.

## Session

- Implementation: grok task-work implementer (2026-10-09)

## Notes

Filed 2026-10-09 at Steven's request (Amuru 2.0 sweep). If a package can't move (e.g. a dependency cycle), record why instead of forcing it.

Update 2026-10-09: PR #622 (099-001, merged as 61205ccc) already updated Directory.Packages.props (added TimeWarp.Architecture.Analyzers 2.0.0-beta.19). Start from master after #622 and only take whatever is still outdated: re-run `ganda nuget outdated --dry-run` first.
