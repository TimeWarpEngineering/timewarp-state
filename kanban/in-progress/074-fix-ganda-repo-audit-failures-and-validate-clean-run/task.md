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
- [ ] `.github/workflows/workflow.yml` is the Nuru/Ganda thin trigger; `dev workflow` is the only pipeline step
- [ ] `dev workflow` PR/merge includes **e2e** and **pack**
- [ ] `dev workflow --mode release` promotes CI artifacts (or documents why State must rebuild, then still one C# command)
- [ ] No `shell: pwsh` job default; no YAML `scripts/*.cs`; path filters include `tools/**`
- [ ] Docs job: `dev docs` or explicit deferral in Results
- [ ] `dev e2e` fails the process when tests fail
- [ ] Duplicate `kanban/done/074-…` kitchen gone
- [ ] Did not cut a State NuGet
- [ ] Results + How to validate updated for **this** remaining slice

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

## Session

- Created: 2026-06-12 (original audit)
- 2026-09-04: first implement (Grok) — audit scaffolding; YAML **not** converted; marked done too early
- 2026-09-04: cockpit — moved back to in-progress; remaining brief is thin YAML + `dev workflow`; duplicate done kitchen to remove
