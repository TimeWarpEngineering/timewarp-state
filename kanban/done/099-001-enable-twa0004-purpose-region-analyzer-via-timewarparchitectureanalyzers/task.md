# Enable TWA0004 (Purpose region analyzer) via TimeWarp.Architecture.Analyzers

## Description

Child of task 099. 099 backfilled `#region Purpose` and `#region Design` on every tracked `.cs` file (its Part 1;
the last PR covers samples, scripts, .githooks and tools). This task is 099's former Part 2: turn on TWA0004
("Source file lacks a #region Purpose block") by referencing the TimeWarp.Architecture.Analyzers package, so
Purpose regions can't drift again.

Start after 099's final backfill PR is merged, so the repo-wide region audit reads 0 missing on master.

## Requirements

- Add `<PackageVersion Include="TimeWarp.Architecture.Analyzers" Version="…" />` to
  `Directory.Packages.props`. Use the latest available version: 2.0.0-beta.9 or newer is required, and
  2.0.0-beta.19 is the latest on nuget.org as of 2026-10-09. Re-check when doing the work.
- Reference it in the shared root `Directory.Build.props`, in the existing "Code Analyzers" ItemGroup, with
  `PrivateAssets="all"` so it does not flow into the published packages.
- Add TWA settings to `.editorconfig`:
  - `dotnet_diagnostic.TWA0004.severity = warning`, or `error` once 099's backfill is merged.
  - Set every other TWA rule to `none` unless it fits this library (see the rule review in Notes), so
    enabling the package doesn't flood the build with unrelated diagnostics.
- TWA0004 checks **only Purpose**. The Design region stays manual and is enforced by reviewers.
- Build stays green, with no new warnings except intended TWA0004 hits, and none once 099's backfill is merged.
- Runfiles (`scripts/*.cs`, `tools/dev-cli/dev.cs`) start with a `#!` shebang and `#:` directives, and their
  regions come right after those lines. Check that TWA0004 accepts that layout, or scope the analyzer away
  from runfiles.
- Exclude `.githooks/` from TWA0004. ganda owns `.githooks/*.cs` byte for byte from its repo-baseline
  template, which has no `#region` blocks, and `ganda hooks install attest` overwrites any hook that differs,
  so these files can never carry a Purpose region. Add an `.editorconfig` section:

  ```ini
  [.githooks/**.cs]
  dotnet_diagnostic.TWA0004.severity = none
  ```

  The hooks already set `#:property RunAnalyzers=false`, so no analyzer runs when git runs them. The
  `.editorconfig` section covers IDEs and any other build that loads the analyzer for those files.

## Checklist

- [x] Add a `TimeWarp.Architecture.Analyzers` PackageVersion (latest, ≥ 2.0.0-beta.9) in `Directory.Packages.props`
- [x] Add the PackageReference (`PrivateAssets="all"`) in the root `Directory.Build.props` "Code Analyzers" ItemGroup
- [x] `.editorconfig`: `dotnet_diagnostic.TWA0004.severity = warning` (or `error`)
- [x] `.editorconfig`: set the TWA rules that don't fit to `none` (review list in Notes); record the decision for each rule
- [x] `.editorconfig`: `[.githooks/**.cs]` section with `dotnet_diagnostic.TWA0004.severity = none` (ganda-owned hook templates)
- [x] Check which projects actually get the analyzer (library, tests, samples with their own props, file-based scripts/.githooks/tools) and that it's PrivateAssets in every packed nupkg
- [x] Workflow green with no TWA warnings; packed nupkgs carry no dependency on TimeWarp.Architecture.Analyzers

## Notes

- Moved here from task 099 (Part 2) on 2026-10-09, when Steven set one kanban task per PR.
- **Enforcement today:** TWA0004 ships in the TimeWarp.Architecture.Analyzers package. timewarp-state does
  not reference that package in any props or csproj, so nothing enforces the regions today. This task fixes
  that for Purpose only. TWA0004 does not check Design, so Design stays a manual, reviewer-enforced
  convention. A Design check could be proposed upstream later.
- **Rule review.** Rule ids and titles come from TimeWarp.Architecture.Analyzers 2.0.0-beta.19
  (2026-10-09). Most rules target TimeWarp.Architecture apps (FluentValidation, FastEndpoints, Aspire,
  slices, the dotnet-new template), not this library. Proposed default is `none`, except TWA0004:
  - TWA0002 (nullable property has a presence validation rule) and TWA0003 (required property has a fabricated
    empty default) are FluentValidation rules: none.
  - TWA0004 (source file lacks a #region Purpose block): **warning/error**.
  - TWA0005: id present in the package, but no title was found in this quick review. Check it when doing
    the work; default none.
  - TWA0006 (routed contract has no server endpoint), TWA0013 and TWA0014 (generated endpoint auth posture),
    TWA0020 ([ApiEndpoint] with [ClientOnlyContract]) and TWA0024 ([EndpointAuthorize] policy not registered)
    are FastEndpoints/contract rules: none.
  - TWA0007 (Aspire resource name is not a ServiceNames constant): none. sample-04 uses Aspire but not
    ServiceNames.
  - TWA0008 and TWA0010 (dotnet-new template conditional tokens and flags) are template-repo only: none.
  - TWA0009 (slice references another product slice): none. This library is not sliced.
  - TWA0011 and TWA0012 (aggregate root Invariants validator) are domain-model rules: none.
  - TWA0015 and TWA0016 (feature filename function/layer grammar): none. The library's feature files don't
    follow `<name>[-<function>]-<layer>.cs`.
  - TWA0021 (mock auth registration) is a SPA auth rule: none.
  - TWA0022 (SPA client code must not call the mediator's Send directly): consider it for the samples and
    test-app, since it pushes dispatch through TimeWarp.State's generated ActionSet methods. It would hit
    the library's own internals, so it likely stays none (or is scoped to samples).
  - TWA0023 (identifier does not use the type stem) is a naming style rule. Evaluate the hit count; likely none.
- **File-based apps:** `scripts`, `.githooks` and `tools` `.cs` files are file-based apps. Confirm whether the
  analyzer reaches them through Directory.Build.props. 099 backfilled regions in `scripts` and `tools`.
  `.githooks` is out of scope: 099's region audit excludes it, and PR #621 synced the hooks to ganda's template
  with `ganda hooks install attest`.

## Results

Done on branch `task/099-001-enable-twa0004-purpose-region-analyzer-via-timewar` (2026-10-09).

**Package.** `TimeWarp.Architecture.Analyzers` 2.0.0-beta.19 (latest on nuget.org flatcontainer, 2026-10-09) in
`Directory.Packages.props`; referenced with `PrivateAssets="all"` in the root `Directory.Build.props` "Code Analyzers"
ItemGroup. The package is also `developmentDependency=true`. None of the 4 packed nupkgs (State, Plus, Policies,
Telemetry) lists it as a dependency. All 28 csproj (source, tests, test-app, samples) resolve it.

**Rule decisions (`.editorconfig`, `[*.cs]`).**

| Rule | Severity | Why |
|---|---|---|
| TWA0004 Source file lacks a #region Purpose block | **error** | 099 brought the repo to 0 missing; error keeps it there |
| TWA0002/0003 FluentValidation nullability | none | no FluentValidation contracts here |
| TWA0005 | (no entry) | retired upstream (MVC verb mismatch, task 131 F-002); id reserved, not in SupportedDiagnostics, never reported |
| TWA0006/0013/0014/0020/0024 FastEndpoints contract and auth rules | none | no generated endpoints here |
| TWA0007 Aspire ServiceNames | none | sample-04 does not use ServiceNames |
| TWA0008/0010 dotnet-new template tokens/flags | none | not a template repo |
| TWA0009 slice isolation | none | library is not sliced |
| TWA0011/0012 aggregate Invariants | none | no domain aggregates |
| TWA0015/0016 feature filename grammar | none | file names don't follow `<name>[-<function>]-<layer>.cs` |
| TWA0021 mock auth registration | none | no SPA mock auth |
| TWA0022 SPA client must not call mediator Send | none | evaluated at warning: 0 hits across the 8 Blazor WASM projects, even though test-app-client has deliberate direct `Sender.Send`/`Send` calls (should-render and persistence test pages), so the rule adds nothing here |
| TWA0023 identifier must use the type stem | none | evaluated at warning: 835 unique sites; off by default upstream too |

`[.githooks/**.cs]`: TWA0004 = none (ganda owns the hooks byte for byte; they also set `RunAnalyzers=false`).

**External sources.** Fixie injects `build/Fixie.Main.cs` from the NuGet cache into all 7 Fixie test projects. The
repo `.editorconfig` can't reach that path, so TWA0004 fell back to its package default and raised 7 new warnings.
`msbuild/external-sources.globalconfig` (added via `GlobalAnalyzerConfigFiles` in the root props) sets TWA0004 = none
globally; `.editorconfig` wins over a global config, so repo files still get error.

**Runfiles.** File-based apps DO get the analyzer: `scripts/*.cs`, `tools/dev-cli/dev.cs` (and its endpoints) all
import the root `Directory.Build.props` through their own folder props. All 6 scripts, dev.cs and
`.githooks/pre-commit.cs` build with 0 TWA diagnostics. Probes (Purpose region removed temporarily, then restored):
- `scripts/clean.cs(13,1): error TWA0004: File 'clean.cs' has no '#region Purpose' block; ...`
- `tools/dev-cli/dev.cs(25,1): error TWA0004: File 'dev.cs' has no '#region Purpose' block; ...`
TWA0004 accepts the shebang + `#:` directive layout (the region only has to exist somewhere in the file).

**TWA0004 fires in the library.** Purpose region removed from `source/timewarp-state/assembly-marker.cs`:
`source/timewarp-state/assembly-marker.cs(21,1): error TWA0004: File 'assembly-marker.cs' has no '#region Purpose' block; add one (a single // line suffices) stating what the file is for` → Build FAILED, 1 error. File restored.

**Workflow** (`dotnet run --file tools/dev-cli/dev.cs -- workflow`): Pipeline SUCCEEDED. Unit/integration 228 passed,
4 skipped; E2E 11 passed, 3 skipped. Warning-neutral: 257 build-summary warnings and the same 115 unique warning
sites as the pre-change baseline run on this branch (diff of the two site lists is empty); 0 TWA diagnostics.

**`ganda repo audit`:** passes with only the 2 pre-existing advisories (`.memsearch.toml` leftover,
`Test.App.Client.lib.module.js` name).

## Session

- Created: 1253659 (2026-10-09)
