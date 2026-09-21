# Round 2 — merged findings
**Date:** 2026-09-21
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

No issues found.

## Duplicates / conflicts

None — single reviewer, empty issue list.

## Resolved prior

Round 1 (`review/round-1/`) had no `M#` findings (BDSoftware GlobalUsingsAnalyzer 1.4.0 wire). Nothing to re-open.

## Merge notes

Re-verified against the repo vs `origin/master` (retarget product commit `45686cd9`):

- CPM pin `TimeWarp.SourceGenerators` **1.0.0-beta.11**; **no** `GlobalUsingsAnalyzer` in `Directory.Packages.props`, `Directory.Build.props`, or `.editorconfig`.
- `Directory.Build.props` keeps `<PackageReference Include="TimeWarp.SourceGenerators" PrivateAssets="all" />`. `TreatWarningsAsErrors` remains `false`.
- `.editorconfig` TW0007 keys sit under `[*.cs]`: `dotnet_diagnostic.TW0007.filename = global-usings.cs` and `dotnet_diagnostic.TW0007.severity = warning`. Matches the 1.0.0-beta.11 package README (`isEnabledByDefault: false`).
- Using-fold from `4fcad2ec` is intact: Plus tests globalize FakeItEasy / State / Mediator / Routing / Components; one-file usings stay local after `namespace X;` (`Microsoft.JSInterop`, `Microsoft.Extensions.Logging.Abstractions`, `Microsoft.Extensions.Options`, `TimeWarp.State.Plus.Features.Timers`).
- Stripped assembly-marker / Test.App mediator / sample mediator-scope / e2e `AssemblyInfo` compilation-unit usings remain gone; `scripts/global-usings.cs` + `Compile Include` remains.
- Analyzer load: package DLL present; Plus-tests build emits expected `warning TW0007` for leftover one-file usings. Remaining hits are in scope as non-blocking.
- `ganda repo audit` check `global-usings-analyzer` PASS. Whole-repo audit may still fail on pre-existing `bin-dev`.
