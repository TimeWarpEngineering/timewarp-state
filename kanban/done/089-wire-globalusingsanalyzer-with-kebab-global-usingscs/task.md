# Opt in TW0007; drop BDSoftware; keep using-fold

## Description

**Retarget.** First implement (PR **#593**) wired **GlobalUsingsAnalyzer 1.4.0**. We now ship **TW0007** in TimeWarp.SourceGenerators **1.0.0-beta.11** (namespace-first). Ganda `global-usings-analyzer` audits SourceGenerators + kebab `TW0007.filename`, not the NuGet analyzer.

**Keep** the using-fold already on this branch (Plus tests `global-usings.cs`, `scripts/global-usings.cs`, stripped duplicates). **Do not** merge #593 until this retarget is committed on the same branch.

## Remaining

- CPM `TimeWarp.SourceGenerators` **1.0.0-beta.4 → 1.0.0-beta.11**
- Remove `GlobalUsingsAnalyzer` from `Directory.Packages.props` and `Directory.Build.props`
- `.editorconfig`: drop GlobalUsingsAnalyzer0001/0002/0003. Add `dotnet_diagnostic.TW0007.filename = global-usings.cs` under `[*.cs]` (copy Ganda). Enable TW0007 at **warning** so namespace-first file usings actually fire (`isEnabledByDefault: false` in the package)
- Do **not** undo the using-fold
- `dev build` / `dev test` green; `ganda repo audit` should not fail `global-usings-analyzer` (package pin + kebab filename)
- Do not flip `TreatWarningsAsErrors`

Same-task-through-fold-in. Same PR #593.

---

Original (superseded) brief:

`.editorconfig` already has GlobalUsingsAnalyzer keys, but they do **not** run:

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

- [x] Using-fold (Plus tests / scripts / samples / assembly-markers) — keep
- [x] SourceGenerators **1.0.0-beta.11**; **no** GlobalUsingsAnalyzer package
- [x] TW0007 filename kebab; TW0007 enabled at warning
- [x] `ganda repo audit` `global-usings-analyzer` PASS
- [x] Do not merge #593 until retarget is on the branch (retarget committed; merge is a later host node)
- [x] `dotnet run --file tools/dev-cli/dev.cs -- build` 0 errors
- [x] Confirm TW0007 loads (namespace-first file usings produce `warning TW0007`)

## Out of scope

- Ganda audit check for this package
- `TreatWarningsAsErrors = true`
- Samples that are meant to show file-level usings in tutorials (unless the analyzer fails CI)

## Notes

Copy (retarget): Ganda `Directory.Packages.props` TimeWarp.SourceGenerators **1.0.0-beta.11** + `.editorconfig` `dotnet_diagnostic.TW0007.filename = global-usings.cs` under `[*.cs]`. This repo also sets `dotnet_diagnostic.TW0007.severity = warning` (Ganda does not; TW0007 is disabled by default).

Using-fold from the first implement is kept. Plus tests globalize FakeItEasy/State/Mediator/Routing/Components in addition to Shouldly/Fixie/NetArchTest/Policies.

Implementation review (effort 1, general): `review/` — round 1 clean (BDSoftware wire); round 2 clean (TW0007 retarget). Disposition `clean`.

## Session

- Created: cockpit grok 2026-09-20 after 064 Plus tests showed repeated FakeItEasy/State/Mediator usings
- Implementer: grok session 01a0bcda-d7e6-7b53-aba5-f4798f909d6c (2026-09-20)
- Review oracle: grok session 01a0bce8-8b93-7f63-b02d-3b1639a9964b (2026-09-20)
- Reviewer (general, round 1): grok-4.5 `01a0bcea-5053-7f43-a655-ffe5a0e7a243` (2026-09-20)
- 2026-09-21: cockpit — retarget: keep using-fold; bump SourceGenerators 1.0.0-beta.11; drop BDSoftware; TW0007. Same PR #593.
- Implementer (retarget): grok session 01a0c181-ee7d-7f51-9081-98ac30980f4a (2026-09-21)
- Review oracle (retarget / round 2): grok session 01a0c188-e700-7a52-99f7-167823957d88 (2026-09-21)
- Reviewer (general, round 2): grok-4.5 `01a0c18a-e2f3-71b2-b171-bdd8f4417fa6` (2026-09-21)

## Results

Retargeted from BDSoftware **GlobalUsingsAnalyzer 1.4.0** to **TimeWarp.SourceGenerators 1.0.0-beta.11** + **TW0007**. The using-fold from the first implement is unchanged. `TreatWarningsAsErrors` is still `false`. Ganda check `global-usings-analyzer` **PASS**.

**Files changed (retarget)**

- `Directory.Packages.props` — `TimeWarp.SourceGenerators` **1.0.0-beta.11**; **removed** `GlobalUsingsAnalyzer`
- `Directory.Build.props` — dropped `GlobalUsingsAnalyzer` `PackageReference`; SourceGenerators stays `PrivateAssets="all"` (TW0007 comment)
- `.editorconfig` under `[*.cs]` — dropped `GlobalUsingsAnalyzer0001/0002/0003`; added `dotnet_diagnostic.TW0007.filename = global-usings.cs` and `dotnet_diagnostic.TW0007.severity = warning`

**Using-fold kept (first implement, not undone)**

- Plus tests `global-usings.cs` (`FakeItEasy`, `TimeWarp.State`, `TimeWarp.Mediator`, `TimeWarp.Features.Routing`, `Microsoft.AspNetCore.Components`)
- One-file Plus-test usings still local (`TimeWarp.State.Plus.Features.Timers`, `Microsoft.Extensions.Logging.Abstractions`, `Microsoft.Extensions.Options`, `Microsoft.JSInterop`)
- Stripped compilation-unit duplicates (assembly-markers, Test.App mediator files, e2e `AssemblyInfo.cs`, sample mediator-scope, `scripts/global-usings.cs`)

**Decisions / deviations**

- TW0007 is **namespace-first** and **disabled by default** in the package. Enabling it at **warning** is required for leftover file usings (including after `namespace X;`) to fire. That is intentional; this retarget does **not** fold every remaining hit.
- Remaining `warning TW0007` lines (one-file Plus-test usings, analyzer `Microsoft.CodeAnalysis.CSharp`, timer `System.Timers`, Test.App / client-integration feature usings, etc.) are expected. They do not fail the build because `TreatWarningsAsErrors` stays `false`.
- Did **not** re-introduce BDSoftware `GlobalUsingsAnalyzer`.
- Whole-repo `ganda repo audit` still exits 1 on pre-existing `bin-dev` / `dev-cli-capabilities` (`bin/dev` missing). The `global-usings-analyzer` row is **PASS**.

**Test outcomes**

- `dotnet run --file tools/dev-cli/dev.cs -- build` — **0 Error(s)** (`Build completed successfully!`). TW0007 warnings print; pre-existing RS0030 Console warnings remain.
- `dotnet run --file tools/dev-cli/dev.cs -- test` — `Tests completed successfully!` (analyzer 19, state 16/1 skipped, plus 19/1 skipped, client-integration 42/1 skipped, architecture 7/1 skipped)
- `ganda repo audit` — check `global-usings-analyzer` **PASS** (`TimeWarp.SourceGenerators pin, PackageReference, TW0007 filename`)
- Analyzer load: Plus tests `add-timer-tests.cs` produces `warning TW0007: Move 'TimeWarp.State.Plus.Features.Timers' to 'global-usings.cs'`. Package restored at `~/.nuget/packages/timewarp.sourcegenerators/1.0.0-beta.11/analyzers/dotnet/cs/timewarp-source-generators.dll`.

### How to validate

**Smoke**

```bash
rg -n 'GlobalUsingsAnalyzer' Directory.Build.props Directory.Packages.props .editorconfig || true
rg -n 'TimeWarp.SourceGenerators' Directory.Packages.props Directory.Build.props
rg -n 'dotnet_diagnostic.TW0007' .editorconfig
test -f ~/.nuget/packages/timewarp.sourcegenerators/1.0.0-beta.11/analyzers/dotnet/cs/timewarp-source-generators.dll && echo tw-sg-dll-ok
dotnet run --file tools/dev-cli/dev.cs -- build
ganda repo audit 2>/dev/null | rg 'global-usi'
```

**Expect**

- No `GlobalUsingsAnalyzer` in CPM, `Directory.Build.props`, or `.editorconfig`
- CPM pin `TimeWarp.SourceGenerators` `Version="1.0.0-beta.11"` and `PackageReference Include="TimeWarp.SourceGenerators" PrivateAssets="all"`
- Under `[*.cs]`: `dotnet_diagnostic.TW0007.filename = global-usings.cs` and `dotnet_diagnostic.TW0007.severity = warning`
- `tw-sg-dll-ok`
- Build: **0 Error(s)** (`TreatWarningsAsErrors` remains false). `warning TW0007` lines are expected for remaining file-level usings. RS0030 Console warnings may still print.
- Audit row `global-usings-analyzer` is **PASS**. Do not require a clean whole-repo audit (pre-existing `bin-dev` missing).

**Automated gate**

```bash
dotnet run --file tools/dev-cli/dev.cs -- test
# expect: Tests completed successfully!
dotnet fixie timewarp-state-plus-tests
# expect: 19 passed, 1 skipped
```

Known-good fire (no revert needed): `dotnet build tests/timewarp-state-plus-tests --nologo -v q` includes `warning TW0007: Move 'TimeWarp.State.Plus.Features.Timers' to 'global-usings.cs'`.

**Not in scope:** flipping `TreatWarningsAsErrors`; folding every remaining TW0007 hit; installing `bin/dev` to clear unrelated audit Errors.

### Review disposition

**Outcome:** `clean` (0 open findings; no `wontfix`)
**Effort:** 1 (general only)
**Rounds:** 2
**Roster:** general (`review/round-1/general.md`, `review/round-2/general.md`)

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

Round 1 found no issues on the **GlobalUsingsAnalyzer 1.4.0** wire. Round 2 found no issues on the **TW0007 / SourceGenerators 1.0.0-beta.11** retarget: CPM pin, no BDSoftware package, kebab `TW0007.filename` under `[*.cs]`, severity warning, using-fold kept, `TreatWarningsAsErrors` still false, audit check `global-usings-analyzer` PASS. No fix loop. No escalations.

**Review paths**

- `kanban/in-progress/089-wire-globalusingsanalyzer-with-kebab-global-usingscs/review/review-framework.md`
- `kanban/in-progress/089-wire-globalusingsanalyzer-with-kebab-global-usingscs/review/round-1/general.md`
- `kanban/in-progress/089-wire-globalusingsanalyzer-with-kebab-global-usingscs/review/round-1/merged.md`
- `kanban/in-progress/089-wire-globalusingsanalyzer-with-kebab-global-usingscs/review/round-2/general.md`
- `kanban/in-progress/089-wire-globalusingsanalyzer-with-kebab-global-usingscs/review/round-2/merged.md`
- `kanban/in-progress/089-wire-globalusingsanalyzer-with-kebab-global-usingscs/review/disposition.md`
