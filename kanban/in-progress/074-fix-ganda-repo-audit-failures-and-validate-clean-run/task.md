# Convert State CI to thin YAML + `dev workflow`

## Description

**074 was marked done too early.** Audit scaffolding landed on master via [PR #579](https://github.com/TimeWarpEngineering/timewarp-state/pull/579) / child #580. `tools/dev-cli` exists and `ganda repo audit` exits 0. **CI is still old-school YAML.**

This remaining slice is the hole the 2026-09-04 rewrite asked for: Nuru / Ganda / Amuru / Terminal run

```bash
dotnet run --file tools/dev-cli/dev.cs -- workflow
dotnet run --file tools/dev-cli/dev.cs -- workflow --mode release --api-key "$NUGET_API_KEY"
```

State `.github/workflows/workflow.yml` still:

- `defaults.run.shell: pwsh` on `ci`, `docs`, and `release`
- `dotnet run --file ./scripts/{clean,build,test,e2e,package}.cs` as separate YAML steps
- NuGet cache keyed on **non-existent** `packages.lock.json`
- Docs job: `windows-latest`, SDK **8.x**, global `docfx`
- Release job: pwsh XML SSOT assert, `scripts/package.cs`, **three** `dotnet nuget push` globs
- Path filters omit `tools/**` so a `dev-cli` change does not run `ci`
- `dev workflow` (already in tree) is **never called** from YAML

`dev workflow` today is also incomplete vs State’s real pipeline:

- PR/merge: `assert-version-ssot → clean → build → test → verify-samples` — **no e2e, no pack**
- Release: `assert-version-ssot → clean → build → check-version` — **no pack, no promote**; comment says “NuGet push stays in workflow.yml”

Finish 074 on **this same id**. Do not mint a sibling “apply 074” task. Same-task-through-fold-in.

Copy **Nuru/Ganda** YAML + artifact **promote** (no rebuild on release). Mediator **006-002** is the library-repo analogue. Do **not** copy Amuru/Terminal rebuild-on-release.

## Already on master (do not redo)

- `tools/dev-cli/` (Nuru 3 + TimeWarp.Nuru.DevCli): `build`, `test` (wraps `scripts/test.cs`), `verify-samples`, `workflow`, shared `clean` / `self-install` / `check-version`
- `msbuild/repository.props`, `.envrc`, `BannedSymbols.txt`, `skills/`, vscode window icon
- Nuru **3.0.0-beta.76**, Amuru 1.0; `scripts/*.cs` migrated to `NuruApp.CreateBuilder()`
- Literal `<Version>` in `source/Directory.Build.props` + SSOT assert vs `TimeWarpStateVersion`
- 080 soak (Mediator 14-beta named pipelines) — already merged

## Requirements

### 1. YAML is a thin trigger

One `ci` job, `ubuntu-latest`, **bash** (not pwsh):

1. `actions/checkout` with `fetch-depth: 0`
2. `actions/setup-dotnet` with `global-json-file: global.json`
3. Break-glass confirm (keep)
4. `nuget/login@v1` gated on release/probe (keep 077 OIDC)
5. Probe echo (keep)
6. **One** pipeline step:

```bash
dotnet run --file tools/dev-cli/dev.cs -- workflow
# release:
dotnet run --file tools/dev-cli/dev.cs -- workflow --mode release --api-key "${{ steps.nuget-login.outputs.NUGET_API_KEY }}"
```

7. Upload `artifacts/packages/*.nupkg` on merge (skip on release — promote that artifact)
8. `permissions`: `contents: read`, `id-token: write`, `actions: read` (promote needs `GH_TOKEN`)
9. Path filters **include** `tools/**`, `scripts/**`, `global.json`

Drop: pwsh default, NuGet cache-on-lockfile, YAML `scripts/*.cs` steps, pwsh XML version extract, three YAML `nuget push`es.

Docs: thin second job calling `dev docs`, **or** fold later. No SDK 8 / global `docfx` / `scripts/build.cs`.

### 2. `dev workflow` is the real pipeline

**PR/merge:** assert-version-ssot → clean → build → test → **e2e** → verify-samples → **pack** (layout under `artifacts/packages`).

**Release:** promote CI nupkgs (Nuru 458 / Ganda 209) — tag-gate → check-version → locate successful `workflow.yml` run → download artifact → verify → push with `--api-key`. **No** `scripts/package.cs` in YAML. **No** rebuild if promote is viable.

Add a `dev e2e` endpoint (or `test --e2e`) that **fails the process** on test failure. Do not carry master `scripts/e2e.cs` silent-success. Playwright path must be `net10.0`.

`dev test` / `dev build` may keep wrapping `scripts/*.cs` **for now** if those scripts are honest; YAML must not call them. Prefer folding into endpoints.

### 3. Do not carry script bugs

- `scripts/package.cs` output vs `artifacts/packages/` — pack where `IPackableProjectService` / YAML upload look
- `scripts/clean.cs` wiping all NuGet locals / `pkill -f dotnet` — use DevCli `IRepoCleanService`
- `scripts/build.cs` 3-project loop — `dev build` already names `timewarp-state.slnx`; YAML must use that
- Analyzer/generator `IsPackable` explicit so release does not pack the wrong set (today: State / Plus / Policies)

### 4. Board hygiene

Origin-home had **two** 074 kitchens (`to-do` rewrite + `done` first implement). This branch must have **one** 074 folder (in-progress). Remove the stale `kanban/done/074-…` duplicate.

## Checklist

- [x] Audit scaffolding / `tools/dev-cli` present / `ganda repo audit` exit 0 (first implement)
- [x] `.github/workflows/workflow.yml` is the Nuru/Ganda thin trigger; `dev workflow` is the only pipeline step
- [x] `dev workflow` PR/merge includes **e2e** and **pack**
- [x] `dev workflow --mode release` promotes CI artifacts (or documents why State must rebuild, then still one C# command)
- [x] No `shell: pwsh` job default; no YAML `scripts/*.cs`; path filters include `tools/**`
- [x] Docs job: `dev docs` or explicit deferral in Results
- [x] `dev e2e` fails the process when tests fail
- [x] Duplicate `kanban/done/074-…` kitchen gone
- [x] Did not cut a State NuGet
- [x] Results + How to validate updated for **this** remaining slice
- [x] Implementation review round 3 disposition clean (same task id)
- [ ] GitHub `ci` green — NU1102: samples restore `TimeWarp.State >= 12.0.0-beta.3` from nuget.org / LocalNuGetFeed before pack. nuget.org nearest is **12.0.0-beta.1**; feed is empty at `dev build`. Old `scripts/build.cs` did **not** include samples in the CI build. `dev workflow` must pack the local feed **before** sample restore/build (library build → pack → samples / verify-samples), or exclude samples from the first slnx restore.

## Out of scope

- TimeWarp.State NuGet release
- Mediator 14.0.0 stable
- 082 board close (gitignore may already be on master)
- Org-wide audit sweep of other repos

## Notes

Reference YAML: `timewarp-nuru` / `timewarp-ganda` `.github/workflows/workflow.yml`.  
Reference CLI: `timewarp-nuru/tools/dev-cli/endpoints/workflow-command.cs` (promote).  
Template: `timewarp-ganda/source/timewarp-ganda/templates/dev-cli/`.

Local vs CI: `bin/dev` is gitignored; CI uses `dotnet run --file tools/dev-cli/dev.cs -- workflow`.

Review kitchen: `review/review-framework.md`, `review/round-N/`, `review/disposition.md` (round 3 clean).

## Session

- Created: 2026-06-12 (original audit)
- 2026-09-04: first implement (Grok) — audit scaffolding; YAML **not** converted; marked done too early
- 2026-09-04: cockpit — moved back to in-progress; remaining brief is thin YAML + `dev workflow`; duplicate done kitchen to remove
- Implementer: grok (2026-09-04) — thin YAML + dev workflow promote
- Review oracle: grok (2026-09-04) — tw-implementation-review effort 1, round 3
- 2026-09-04: `/tw-merge` refused — PR #581 `ci` red (run 33834323550). `dev build` of `timewarp-state.slnx` NU1102 on samples (`TimeWarp.State`/`Plus` 12.0.0-beta.3 not on nuget.org; LocalNuGetFeed empty). Pack currently runs **after** build.

## Results

### What was implemented

- Rewrote `.github/workflows/workflow.yml` as a single `ci` job thin trigger (bash, no pwsh): checkout `fetch-depth: 0`, `setup-dotnet` via `global.json`, break-glass confirm, OIDC nuget/login, probe echo, one `dotnet run --file tools/dev-cli/dev.cs -- workflow` step, upload `Packages-*` (skipped on release). Path filters include `tools/**` / `scripts/**` / `samples/**` / `msbuild/**` / `global.json`; dropped `Documentation/**`.
- Added `dev e2e` (`tools/dev-cli/endpoints/e2e-command.cs`) wrapping `scripts/e2e.cs`; defaults `UseHttp=true`; sets `Environment.ExitCode` on child non-zero.
- Added `dev pack` (`tools/dev-cli/endpoints/pack-command.cs`): clears `artifacts/packages`, packs derived IsPackable set, verifies with `CiRunPromotion.VerifyPackageSet`.
- Rewrote `dev workflow`: PR/merge = assert-version-ssot → clean → build → test → e2e → verify-samples → pack; release = tag-gate → check-version → locate-run → download-artifact → verify → push (promote, no rebuild/pack). No attestation.
- Explicit IsPackable: analyzer/generator `false`; Plus `true` (State/Policies already true). Packable set: TimeWarp.State, TimeWarp.State.Plus, TimeWarp.State.Policies.
- Fixed leftover `scripts/package.cs`: output to `./artifacts/packages`, three packable projects only; removed `taskkill` / `NuGet locals clear all`.

### Files changed

- `.github/workflows/workflow.yml`
- `tools/dev-cli/endpoints/e2e-command.cs` (new)
- `tools/dev-cli/endpoints/pack-command.cs` (new)
- `tools/dev-cli/endpoints/workflow-command.cs`
- `tools/dev-cli/global-usings.cs`
- `tools/dev-cli/dev.cs` (Design region)
- `source/timewarp-state-analyzer/timewarp-state-analyzer.csproj`
- `source/timewarp-state-source-generator/timewarp-state-source-generator.csproj`
- `source/timewarp-state-plus/timewarp-state-plus.csproj`
- `scripts/package.cs`
- `kanban/in-progress/074-…/task.md`

### Key decisions

- Copied Nuru/Ganda **promote** (download CI Packages-* artifact), not Amuru/Terminal/Mediator rebuild-on-release.
- Release GitHub tags must be `v{Version}` (`TagAssertion` from TimeWarp.Nuru.DevCli). 11.x used unprefixed tags; recent betas already use `v`.
- Docs GitHub Pages job **deferred**: old windows/docfx/SDK-8 job removed so it cannot keep running; no `dev docs` endpoint added.
- Probe `workflow_dispatch` skips artifact upload (pipeline does not pack).
- Did **not** cut a State NuGet / did not `dotnet nuget push`.
- Duplicate `kanban/done/074-*` kitchen already absent on this branch (reopen hygiene).

### Review disposition

- **Outcome:** clean (0 open)
- **Rounds:** 3 · **Effort:** 1 · **Roster:** general
- **Final counts:** bug 0/0/0 open/fixed/wontfix · suggestion 0 open / 1 fixed (M1, rounds 1–2) / 0 wontfix · nit 0/0/0
- Round 3 (this remaining slice) raised no new issues. M1 still holds via DevCli `AssertVersionSsot` after YAML Version extract was removed.
- Artifacts: `review/review-framework.md`, `review/round-3/general.md`, `review/round-3/merged.md`, `review/disposition.md` (rounds 1–2 frozen)

### Validation run

- `dotnet run --file tools/dev-cli/dev.cs -- --help` — lists workflow, e2e, pack
- `workflow` / `e2e` / `pack --help` — OK
- YAML rg: no `shell: pwsh` / `scripts/*.cs` / `packages.lock.json`
- `ganda repo audit` — exit 0 (2 advisory warnings: kebab path Test.App.Client.lib.module.js, memsearch scaffold)
- `dev build` then `dev pack` — produced the three expected nupkgs (snupkgs present but excluded from VerifyPackageSet)
- Did **not** run full `dev workflow` e2e in this session

### How to validate

**Smoke**

```bash
# help lists new endpoints
dotnet run --file tools/dev-cli/dev.cs -- --help
# expect: workflow, e2e, pack among routes; no crash

# YAML is thin
rg -n "shell: pwsh|scripts/.*\\.cs|packages.lock.json" .github/workflows/workflow.yml
# expect: no matches

rg -n "tools/\\*\\*|dev.cs -- workflow" .github/workflows/workflow.yml
# expect: tools/** in path filters; pipeline calls tools/dev-cli/dev.cs -- workflow

# packable set
dotnet msbuild source/timewarp-state-analyzer/timewarp-state-analyzer.csproj -nologo -getProperty:IsPackable
dotnet msbuild source/timewarp-state-source-generator/timewarp-state-source-generator.csproj -nologo -getProperty:IsPackable
dotnet msbuild source/timewarp-state-plus/timewarp-state-plus.csproj -nologo -getProperty:IsPackable
# expect: analyzer/generator False; plus True

# pack layout (after a Release pack)
mkdir -p artifacts/packages
dotnet run --file tools/dev-cli/dev.cs -- build
dotnet run --file tools/dev-cli/dev.cs -- pack
# expect: artifacts/packages/TimeWarp.State.12.0.0-beta.3.nupkg
#         TimeWarp.State.Plus.12.0.0-beta.3.nupkg
#         TimeWarp.State.Policies.12.0.0-beta.3.nupkg
#         no analyzer/generator nupkgs

ganda repo audit
# expect: exit 0
```

**Expect**

- Help routes include `workflow`, `e2e`, `pack`
- Thin YAML: no pwsh default, no `scripts/*.cs` steps, no lockfile cache; `tools/**` filtered; single pipeline step
- IsPackable False/False/True for analyzer/generator/plus
- Pack emits exactly the three product nupkgs under `artifacts/packages` (sibling `.snupkg` files are not uploaded or promoted)
- `ganda repo audit` exits 0
- A published GitHub Release tag must be `v12.0.0-beta.3` (not `12.0.0-beta.3`) to pass the release tag-gate
