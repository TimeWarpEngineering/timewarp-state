# Review framework — task 089

**Date:** 2026-09-20
**Host task:** kanban/in-progress/089-wire-globalusingsanalyzer-with-kebab-global-usingscs/
**Diff scope:** branch `task/089-wire-globalusingsanalyzer-with-kebab-global-usings` vs `origin/master` (product commit `4fcad2ec`; kitchen `4f113fe4`)
**Plan / brief:** Wire GlobalUsingsAnalyzer 1.4.0 repo-wide (Ganda/Nuru shape), point editorconfig at kebab `global-usings.cs`, promote repeated file-level usings, drive analyzer warnings to zero. Do not flip `TreatWarningsAsErrors`. Ganda audit check out of scope.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok review oracle (2026-09-20)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Product files in scope

- `Directory.Packages.props` — CPM pin GlobalUsingsAnalyzer 1.4.0
- `Directory.Build.props` — Code Analyzers `PackageReference` with `PrivateAssets="all"`
- `.editorconfig` — `GlobalUsingsAnalyzer0001.filename = global-usings.cs`, 0002 enabled, 0003 warning
- Plus tests `global-usings.cs` and routing/timer test files (promote vs keep one-file usings)
- Source `assembly-marker.cs` (State + Plus)
- Test.App client/server mediator files and e2e `AssemblyInfo.cs` / `global-usings.cs`
- Sample mediator-scope files (strip only if already covered by sample `global-usings.cs`)
- `scripts/global-usings.cs` + `scripts/Directory.Build.props` `Compile Include`

## Requirements to check

- CPM: `GlobalUsingsAnalyzer` **1.4.0**
- `Directory.Build.props` Code Analyzers item group includes `<PackageReference Include="GlobalUsingsAnalyzer" PrivateAssets="all" />`
- `.editorconfig`: filename kebab `global-usings.cs`; 0002 enabled; 0003 warning
- `TreatWarningsAsErrors` stays false
- Repeated Plus-test usings (`FakeItEasy`, `TimeWarp.State`, `TimeWarp.Mediator`, routing if it fires) promoted; truly one-file usings kept
- Analyzer warnings driven to zero; `dev build` / `dev test` still green
- Analyzer actually loads (package restore + diagnostic ID casing)
- Runfiles (`scripts/*.cs`) compile without GlobalUsingsAnalyzer warnings
