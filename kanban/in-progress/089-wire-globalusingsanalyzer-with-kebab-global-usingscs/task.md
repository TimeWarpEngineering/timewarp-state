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

- [x] Package referenced; editorconfig filename is kebab `global-usings.cs`
- [x] Repeated test/source usings moved to project `global-usings.cs`
- [x] `dotnet run --file tools/dev-cli/dev.cs -- build` 0 errors
- [x] Confirm analyzer loads (`/analyzer:.../GlobalUsingsAnalyzer.dll` in `csc` or a known-bad file-level using produces GlobalUsingsAnalyzer0003)

## Out of scope

- Ganda audit check for this package
- `TreatWarningsAsErrors = true`
- Samples that are meant to show file-level usings in tutorials (unless the analyzer fails CI)

## Notes

Copy: `timewarp-ganda/Directory.Build.props` (PackageReference) + `Directory.Packages.props` 1.4.0. Architecture 172 is the restore analogue.

Plus tests today only globalize Shouldly/Fixie/NetArchTest/Policies.

## Session

- Created: cockpit grok 2026-09-20 after 064 Plus tests showed repeated FakeItEasy/State/Mediator usings
- Implementer: grok session 01a0bcda-d7e6-7b53-aba5-f4798f909d6c (2026-09-20)

## Results

Wired **GlobalUsingsAnalyzer 1.4.0** repo-wide (Ganda/Nuru shape) and pointed it at kebab `global-usings.cs`. Compilation-unit file-level usings the analyzer flagged are gone; Plus-test repeated usings (`FakeItEasy`, `TimeWarp.State`, `TimeWarp.Mediator`, `TimeWarp.Features.Routing`, `Microsoft.AspNetCore.Components`) now live in that project's `global-usings.cs`. `TreatWarningsAsErrors` is still `false`.

**Files changed**

- `Directory.Packages.props` — `PackageVersion` GlobalUsingsAnalyzer 1.4.0 (`PrivateAssets=all`)
- `Directory.Build.props` — Code Analyzers `PackageReference` with `PrivateAssets="all"`
- `.editorconfig` — `dotnet_diagnostic.GlobalUsingsAnalyzer0001.filename = global-usings.cs` (0002 enabled, 0003 warning)
- Plus tests `global-usings.cs` + strip of repeated file usings; kept one-file usings (`TimeWarp.State.Plus.Features.Timers`, `Microsoft.Extensions.Logging.Abstractions`, `Microsoft.Extensions.Options`, `Microsoft.JSInterop`)
- Source `assembly-marker.cs` (State + Plus): stripped usings already covered by project `global-usings.cs`
- Test.App client/server mediator files and e2e `AssemblyInfo.cs`: same strip / promote
- Sample mediator-scope files: stripped redundant top-of-file usings already present in each sample `global-usings.cs` (not tutorial-only file-level usings)
- `scripts/global-usings.cs` + `Compile Include` so runfiles (`dev test` compiles `scripts/test.cs`) do not warn

**Decisions / deviations**

- Analyzer reports `warning GlobalUsingsAnalyzer: Move using X to global-usings.cs` (0003). It only sees compilation-unit usings (before a file-scoped `namespace`); usings after `namespace X;` are not flagged. Plus tests still promoted the repeated ones from the brief.
- Sample mediator-scope leftovers were already in each sample's `global-usings.cs`, so they were stripped to drive analyzer warnings to zero. Tutorial overviews still document creating `GlobalUsings.cs`.
- Runfiles share one `scripts/global-usings.cs` (same pattern as `tools/dev-cli`).

**Test outcomes**

- `dotnet run --file tools/dev-cli/dev.cs -- build` — 0 errors (pre-existing RS0030 Console warnings only; **0** GlobalUsingsAnalyzer)
- `dotnet run --file tools/dev-cli/dev.cs -- test` — passed (analyzer 19, state, plus 19/1 skipped, client-integration 42/1 skipped, architecture 7/1 skipped)
- Known-bad load: a top-of-file `using TimeWarp.Mediator;` produced `warning GlobalUsingsAnalyzer: Move using TimeWarp.Mediator to global-usings.cs` before those lines were stripped. Package is restored at `~/.nuget/packages/globalusingsanalyzer/1.4.0/analyzers/dotnet/cs/GlobalUsingsAnalyzer.dll`.

### How to validate

**Smoke**

```bash
rg -n 'GlobalUsingsAnalyzer' Directory.Build.props Directory.Packages.props
rg -n 'GlobalUsingsAnalyzer0001.filename' .editorconfig
test -f ~/.nuget/packages/globalusingsanalyzer/1.4.0/analyzers/dotnet/cs/GlobalUsingsAnalyzer.dll && echo analyzer-dll-ok
dotnet run --file tools/dev-cli/dev.cs -- build
```

**Expect**

- CPM pin `Version="1.4.0"` and `PackageReference Include="GlobalUsingsAnalyzer" PrivateAssets="all"`
- `dotnet_diagnostic.GlobalUsingsAnalyzer0001.filename = global-usings.cs`
- `analyzer-dll-ok`
- Build: **0 Error(s)** and **no** `warning GlobalUsingsAnalyzer` lines (`TreatWarningsAsErrors` remains false; RS0030 Console warnings may still print)

**Automated gate**

```bash
dotnet run --file tools/dev-cli/dev.cs -- test
# expect: Tests completed successfully!
dotnet fixie timewarp-state-plus-tests
# expect: 19 passed, 1 skipped
```

Known-bad (optional, revert after): add `using TimeWarp.Mediator;` as the first line of `source/timewarp-state-plus/assembly-marker.cs` and `dotnet build source/timewarp-state-plus/timewarp-state-plus.csproj`. Expect `warning GlobalUsingsAnalyzer: Move using TimeWarp.Mediator to global-usings.cs`.

**Not in scope:** Ganda `repo audit` check for this package; flipping `TreatWarningsAsErrors`.
