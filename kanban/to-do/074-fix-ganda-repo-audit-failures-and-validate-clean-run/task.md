# Fix ganda repo audit failures; convert CI to thin YAML + dev-cli

## Description

`ganda repo audit` on origin-home **master** (2026-09-04) is **6 passed / 17 failed / 4 skipped**. The 2026-06-12 brief is stale (`ci-cd.yml` → `workflow.yml` already landed; `kanban/archived/` exists; mediator soak is **080** not 040–049).

The failure that actually hurts shipping is the **old-school GitHub Actions graph**. Nuru, Amuru, Terminal, and Ganda run CI as one C# pipeline:

```bash
dotnet run --file tools/dev-cli/dev.cs -- workflow
dotnet run --file tools/dev-cli/dev.cs -- workflow --mode release --api-key "$NUGET_API_KEY"
```

State still orchestrates in YAML: PowerShell defaults, `dotnet run --project ./scripts/{clean,build,test,e2e,package}.cs`, a Windows DocFX job on SDK 8.x, and a release job that parses `TimeWarpStateVersion` in pwsh and `dotnet nuget push`es three packages. **077** already added OIDC `nuget/login@v1` to that YAML; it did **not** convert the pipeline to `dev workflow`.

Bring State onto the same rails: **TimeWarp.Nuru** CLI + **TimeWarp.Amuru** process + **TimeWarp.Terminal** output + **TimeWarp.Nuru.DevCli** shared endpoints. YAML stays a thin trigger. Then clear the remaining audit checks so `ganda repo audit` is 0 failed.

Reference YAML: `timewarp-nuru` / `timewarp-ganda` / `timewarp-amuru` / `timewarp-terminal` `.github/workflows/workflow.yml`.

Re-audit snapshot that triggered this rewrite: cockpit session opening State [PR #579](https://github.com/TimeWarpEngineering/timewarp-state/pull/579) (080 soak). Master and the 080 feature both fail audit; 080 is **not** worse (it added journal/memsearch gitignore). Do **not** fight #579 — rebase onto it after merge, or start from `feature/080-timewarp-mediator-14-beta`.

## Depends on

<!-- 080 parent kitchen already lives on origin-home; no hard merge-wait. Sequence: after #579 or rebase onto feature/080. 082 is the journal gitignore consumer sweep (related). -->

## Requirements

### 1. Thin workflow.yml + `dev workflow` (the “old school” hole)

Today (master `workflow.yml`):

- Job `ci`: `shell: pwsh`; `dotnet run --project ./scripts/clean.cs|build.cs|test.cs|e2e.cs`
- SDK: `9.0.x` **and** `10.0.100-preview.7.25380.108` (not `global.json` / `10.0.x`)
- Job `docs`: `windows-latest`, SDK **8.x**, `dotnet tool update --global docfx`, `dotnet run --project ./scripts/build.cs`
- Job `release`: SDK **8.0.403**; pwsh XML extract of `TimeWarpStateVersion`; three `dotnet nuget push` steps
- Path filters omit `tools/`, `scripts/`, `global.json` (kanban-only PRs skip `ci` — 080-003 hit this)

Target (copy Nuru/Ganda shape, keep State’s E2E):

- Single `ci` job on `ubuntu-latest` (bash)
- `actions/setup-dotnet` with `global-json-file: global.json` **or** `dotnet-version: '10.0.x'` (Amuru uses global.json; Nuru/Ganda use 10.0.x)
- `nuget/login@v1` gated on release/probe (already present from 077)
- **One** pipeline step: `dotnet run --file tools/dev-cli/dev.cs -- workflow` (release: `--mode release --api-key …`)
- Upload `artifacts/packages/*.nupkg` on merge (Nuru 458: release **promotes** that artifact, no rebuild)
- Docs: either a `dev docs` endpoint called from a thin second job, or fold into `dev workflow` — **no** SDK 8 / global `docfx` / raw `scripts/build.cs`
- Path filters must include `tools/**`, `scripts/**`, `global.json` so a dev-cli change actually runs CI
- No `shell: pwsh` as the job default

### 2. Scaffold `tools/dev-cli` (also clears audit `bin-dev`, `dev-cli-capabilities`, `region-annotations`)

- Add `tools/dev-cli/dev.cs` (Nuru runfile, `#:package TimeWarp.Nuru`, `#:package TimeWarp.Nuru.DevCli`, Amuru, Terminal)
- Shared DevCli endpoints: `build`, `clean`, `test`, `pack`/`package`, `check-version`, `workflow`, `self-install`
- State-specific: `e2e` (today `scripts/e2e.cs`), `docs` if DocFX stays
- `dev self-install` → `./bin/dev`; `.envrc` so `direnv` puts `bin/` on PATH
- Fold `scripts/*.cs` **into** these endpoints (or thin wrappers). Do not leave YAML calling `dotnet run --project ./scripts/….cs`
- `source/Directory.Build.props`: audit `source-directory-build-props` wants a readable `<Version>` (not only `TimeWarpStateVersion` / `<PackageVersion>`). Align with Nuru SSOT without cutting a State NuGet in this task

### 3. Remaining `ganda repo audit` (master 2026-09-04)

| Check | Notes |
|-------|--------|
| `banned-api-analyzers` / `banned-symbols` | Wire in `Directory.Build.props` + add `BannedSymbols.txt`; fix or baseline violations **after** 080 is on the branch so the build is the 14-beta one |
| `cpm-consistency` | 23 orphaned `PackageVersion`s — delete dead; comment deliberate pins |
| `nuru` | `2.1.0-beta.8` → current 3.x beta (audit said `3.0.0-beta.76`). Scripts/dev-cli must move to Nuru 3 API (`NuruApp.CreateBuilder` / `[NuruRoute]`) |
| `msbuild-repository-props` | Add `msbuild/repository.props` and import from root `Directory.Build.props` |
| `envrc` | `.envrc` for `bin/dev` |
| `directory-structure` | `skills/` still missing on master (080 may have `kanban/archived/`) |
| `nuget-package-icon` | Pack `assets/logo.png` from `source/Directory.Build.props` |
| `kebab-path-names` | 12 on a clean master (not the 489 from `tests/test-app/output` build junk). Fix those 12; gitignore generated output |
| `memsearch-gitignore` / `routine-journals-gitignore` | 082 / 080-003 already on the feature branch — cherry-pick if starting from master |
| `vscode-window-icon` | Warning: `.vscode/tasks.json` + `settings.json` |
| `workflow-file` | **Already PASS** (`workflow.yml` exists). Do not “rename ci-cd.yml” |

**Validate:** `ganda repo audit` → **0 failed** (warnings ok). Paste the clean table into Results.

## Checklist

- [ ] `tools/dev-cli/dev.cs` + `self-install` → `./bin/dev`; `dev --capabilities` works
- [ ] Fold `scripts/{clean,build,test,e2e,package}.cs` into `dev` endpoints (Nuru + Amuru + Terminal)
- [ ] `dev workflow` PR/merge: clean → build → test → e2e → pack
- [ ] `dev workflow --mode release`: promote CI nupkgs (no pwsh XML, no three hand `nuget push`es)
- [ ] Collapse `.github/workflows/workflow.yml` to the Nuru/Ganda thin trigger; SDK from `global.json` / `10.0.x`; path filters include `tools/**`
- [ ] Docs job uses `dev docs` or is folded into workflow (no SDK 8 / global docfx)
- [ ] `.envrc`, `msbuild/repository.props`, `skills/`, nuget icon, `<Version>` SSOT
- [ ] BannedApiAnalyzers + `BannedSymbols.txt`; fix 14-beta build violations
- [ ] CPM prune; Nuru 3 bump
- [ ] Kebab: the 12 master hits (not build output)
- [ ] `ganda repo audit` 0 failed — paste output in Results
- [ ] Did not cut a State NuGet; did not fight 080/#579

## Out of scope

- Merging 080 / PR #579 (human gate)
- TimeWarp.State NuGet release
- TimeWarp.Mediator 14.0.0 stable
- Org-wide `ganda repo audit --fix` sweep of other repos (082 is the journal consumer only)

## Notes

**Copy from, do not invent:**

- YAML: `timewarp-nuru/master/.github/workflows/workflow.yml` (and ganda/amuru/terminal — same one-liner)
- CLI: `timewarp-nuru/master/tools/dev-cli/dev.cs` + `endpoints/workflow-command.cs`
- Shared package: `TimeWarp.Nuru.DevCli` (clean/build/test/pack/check-version/workflow)

**077** is done for OIDC in the current YAML. Keep `nuget/login@v1` + `id-token: write`; move publish **into** `dev workflow --mode release`.

**080-001** on the feature branch already converted some `dotnet run --project` scripts to runfiles and pinned SDK from `global.json`. Reuse that; still wrap them in `dev`.

**Local vs CI:** `bin/dev` is gitignored; CI should `dotnet run --file tools/dev-cli/dev.cs -- workflow` (Nuru) so the runner does not need a preinstalled `./bin/dev`.

**Copy target:** Nuru/Ganda for YAML + **promote** (no rebuild on release). Mediator **006-002** for a multi-package library with analyzers. Amuru/Terminal already have `tools/dev-cli` but **rebuild on release** and Amuru still reimplements DevCli — do not copy their release path.

**Do not carry these script bugs into `dev`:**

1. `scripts/e2e.cs` on **master** logs `testsFailed` and may not `Environment.Exit` (de-facto continue-on-error). 080-003 gated E2E in YAML; the process must still fail. Playwright helper path still mentions `net9.0` while TFM is `net10.0`.
2. `scripts/package.cs` writes `./Nuget` / `./LocalNugetFeed`; YAML push glob is `./artifacts/packages/` — only works if `GeneratePackageOnBuild` side-effects. Align pack output with `IPackableProjectService` / `artifacts/packages`.
3. `scripts/clean.cs` / `package.cs` wipe **all** NuGet locals; `build.cs` has a `pkill -f dotnet` clean route. Use DevCli `IRepoCleanService`.
4. `scripts/build.cs` builds three product csprojs, not `timewarp-state.slnx` (analyzers/tests/samples missing).
5. Analyzer/generator `IsPackable` must be explicit so `IPackableProjectService` does not pack the wrong set (release today only pushes State / Plus / Policies).
6. NuGet cache step keys `**/packages.lock.json` — this repo has **no** lock files; drop it with the YAML collapse.

**Version SSOT:** root `TimeWarpStateVersion` is `12.0.0-beta.3`. Shared `check-version` / `dev release` read `<Version>` in `source/Directory.Build.props`. Introduce `<Version>` equal to today’s pin; do **not** bump as a release in this task.

**Docs job:** unique to State (DocFX + GitHub Pages). Keep as a thin second job calling `dev docs`, or defer fold-in. Do not leave SDK 8 / global `docfx`.

**Greenfield files:** Ganda template `source/timewarp-ganda/templates/dev-cli/` (`dev.cs.txt`, `directory.build.props.template`, `build`/`test`/`verify-samples`/`workflow` endpoints). Shared `clean`/`self-install`/`check-version`/`release` come from `TimeWarp.Nuru.DevCli` content files. Pin Nuru + DevCli **3.0.0-beta.76** (or current), not 2.1.0-beta.8.

## Session

- Created: 2026-06-12 (original audit 4/12/2)
- 2026-09-04: cockpit grok — re-audited master (6/17/4); 074 still to-do; `workflow.yml` rename already done; remaining hole is YAML-orchestrated `scripts/*.cs` vs Nuru-style `dev workflow`. Folderized. Brief rewritten. No product yet.
- 2026-09-04: appended CI comparison (Nuru/Ganda promote vs Amuru rebuild; e2e/pack/clean landmines; Mediator 006-002 analogue; Ganda `templates/dev-cli`).
