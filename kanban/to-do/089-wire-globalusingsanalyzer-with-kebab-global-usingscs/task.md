# Wire GlobalUsingsAnalyzer with kebab `global-usings.cs`

## Description

`.editorconfig` already has GlobalUsingsAnalyzer keys, but they do **not** run:

- `dotnet_diagnostic.globalusingsanalyzer0001.filename = GlobalUsings.cs` (PascalCase)
- Repo files are kebab **`global-usings.cs`**
- **`GlobalUsingsAnalyzer` is not in CPM / `Directory.Build.props`**

Copy Ganda/Nuru: pin **1.4.0**, `PackageReference` with `PrivateAssets=all` on `Directory.Build.props` (all projects, including tests). Set filename to `global-usings.cs`. Fold repeated file-level usings into each project's `global-usings.cs` so Plus tests (`FakeItEasy`, `TimeWarp.State`, `TimeWarp.Mediator`) stop repeating.

`ganda repo audit` does **not** check this. `editorconfig-presence` only requires `root = true` + three csharp_style sentinels. No audit check for the GlobalUsingsAnalyzer package. Out of scope: adding that check in Ganda.

## Requirements

- CPM: `GlobalUsingsAnalyzer` **1.4.0**
- `Directory.Build.props` Code Analyzers item group: `<PackageReference Include="GlobalUsingsAnalyzer" PrivateAssets="all" />` (same as Ganda)
- `.editorconfig`:
  - `dotnet_diagnostic.GlobalUsingsAnalyzer0001.filename = global-usings.cs`
  - `dotnet_diagnostic.GlobalUsingsAnalyzer0002.enabled = true`
  - `dotnet_diagnostic.GlobalUsingsAnalyzer0003.severity = warning`
- Do **not** flip `TreatWarningsAsErrors` (stays false on this repo)
- Promote namespaces the analyzer flags (at least Plus tests: FakeItEasy, TimeWarp.State, TimeWarp.Mediator; routing if it fires). Strip redundant file usings. Keep file-local usings that are truly one-file (e.g. `TimeWarp.State.Plus.Features.Timers` if only add-timer-tests uses it)
- `dev build` / `dev test` still green. Analyzer warnings expected only if leftover file usings remain — drive those to zero in this task

## Checklist

- [ ] Package referenced; editorconfig filename is kebab `global-usings.cs`
- [ ] Repeated test/source usings moved to project `global-usings.cs`
- [ ] `dotnet run --file tools/dev-cli/dev.cs -- build` 0 errors
- [ ] Confirm analyzer loads (`/analyzer:.../GlobalUsingsAnalyzer.dll` in `csc` or a known-bad file-level using produces GlobalUsingsAnalyzer0003)

## Out of scope

- Ganda audit check for this package
- `TreatWarningsAsErrors = true`
- Samples that are meant to show file-level usings in tutorials (unless the analyzer fails CI)

## Notes

Copy: `timewarp-ganda/Directory.Build.props` (PackageReference) + `Directory.Packages.props` 1.4.0. Architecture 172 is the restore analogue.

Plus tests today only globalize Shouldly/Fixie/NetArchTest/Policies.

## Session

- Created: cockpit grok 2026-09-20 after 064 Plus tests showed repeated FakeItEasy/State/Mediator usings
