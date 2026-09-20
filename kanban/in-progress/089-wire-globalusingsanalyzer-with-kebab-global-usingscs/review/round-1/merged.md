# Round 1 — merged findings
**Date:** 2026-09-20
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

## Merge notes

Re-verified against the repo vs `origin/master` (product commit `4fcad2ec`):

- CPM pin `GlobalUsingsAnalyzer` 1.4.0 with `PrivateAssets` / `IncludeAssets` matches Ganda; `Directory.Build.props` Code Analyzers `PackageReference` uses `PrivateAssets="all"`.
- `TreatWarningsAsErrors` remains `false`.
- `.editorconfig` keys sit under `[*.cs]` (package README requirement). PascalCase `GlobalUsingsAnalyzer000*` matches the 1.4.0 README; filename is kebab `global-usings.cs`.
- Plus tests promote `FakeItEasy`, `TimeWarp.State`, `TimeWarp.Mediator`, `TimeWarp.Features.Routing`, `Microsoft.AspNetCore.Components`. One-file usings remain after the file-scoped namespace (`Microsoft.JSInterop`, `Microsoft.Extensions.Logging.Abstractions`, `Microsoft.Extensions.Options`, `TimeWarp.State.Plus.Features.Timers`).
- Stripped assembly-marker / Test.App mediator / sample mediator-scope / e2e `AssemblyInfo` usings are already in the corresponding project `global-usings.cs`.
- `scripts/global-usings.cs` + `Compile Include` matches `tools/dev-cli`.
- Remaining file-level usings after `namespace X;` are expected: repo `csharp_using_directive_placement = inside_namespace` and the analyzer only sees compilation-unit usings.
