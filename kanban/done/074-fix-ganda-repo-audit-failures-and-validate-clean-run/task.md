# Fix ganda repo audit failures and validate clean run

## Description

`ganda repo audit` (run 2026-06-12) reports **4 passed, 12 failed, 2 skipped** with blocking failures. Bring the repo up to the TimeWarp repository conventions the audit enforces, then validate with a clean re-run.

Failures, grouped by kind:

### Missing conventional files/directories (org-standard scaffolding)

- **bin-dev** — `bin/dev` is missing (also blocks the `dev-cli-capabilities` check, which runs `bin/dev --capabilities`)
- **region-annotations** — `tools/dev-cli` is missing
- **msbuild-repository-props** — `msbuild/repository.props` is missing (its absence also causes the `repository-...` check to SKIP)
- **envrc** — `.envrc` is missing
- **directory-structure** — missing `skills/` and `kanban/archived/`
- **vscode-window-icon** (Warning) — `.vscode/tasks.json` and `.vscode/settings.json` are missing

These are structural conventions from the newer TimeWarp repo template — copy/adapt from a repo that passes the audit (e.g. wherever `ganda` itself or a recently scaffolded TimeWarp repo lives) rather than inventing them.

### Banned-API enforcement not wired

- **banned-api-analyzers** — `Directory.Build.props` is missing the BannedApiAnalyzers configuration
- **banned-symbols** — `BannedSymbols.txt` is missing

Note: introducing BannedApiAnalyzers may surface new build warnings/errors across `source/` — budget for fixing or baselining those, and do it after the mediator migration (tasks 040–049) restores the build so violations are even visible.

### Package hygiene

- **cpm-consistency** — 23 orphaned `PackageVersion` entries in `Directory.Packages.props` (Microsoft.CodeAnalysis.*, Microsoft.Extensions.* 9.0.0 pins, Playwright 1.19.1 + Playwright.NUnit 1.44.0, Fixie 3.4.0, NUnit trio, coverlet, Blazor.SessionStorage.WebAssembly, Serilog pair, Scrutor, System.Text.Json, etc.). Verify each is truly unreferenced before deleting — some may be transitive pins on purpose (e.g. System.Text.Json security floor) and some may come back into use as the mediator migration and e2e test work proceed. Delete the dead ones; comment the deliberate pins if the tool supports exemptions.
- **nuru** — TimeWarp.Nuru outdated: 2.1.0-beta.8 → 3.0.0-beta.70. Consumed by the runfile scripts (`scripts/build.cs` etc. use `#:package TimeWarp.Nuru`); a major-version jump likely changes the `NuruAppBuilder` API, so update the scripts alongside the bump.

### CI

- **workflow-file** — legacy `ci-cd.yml` found; rename to `workflow.yml` (update any references to the old name in docs/badges).

## Checklist

- [x] Scaffold `bin/dev` + `tools/dev-cli` (dev-cli-capabilities and region-annotations should then pass)
- [x] Add `msbuild/repository.props` (un-skips the repository props content check — fix whatever it then reports)
- [x] Add `.envrc`
- [x] Create `skills/` and `kanban/archived/`
- [x] Add `.vscode/tasks.json` + `.vscode/settings.json` window-icon config
- [x] Wire BannedApiAnalyzers in `Directory.Build.props` + add `BannedSymbols.txt`; fix surfaced violations
- [x] Prune/justify the 23 orphaned PackageVersion entries
- [x] Bump TimeWarp.Nuru to 3.0.0-beta.x and migrate `scripts/*.cs` to the new API
- [x] Rename `.github/workflows/ci-cd.yml` → `workflow.yml`; fix references
- [x] **Validate:** `ganda repo audit` reports 0 failed (warnings at most) — paste the clean output into this task before moving to done
- [x] Implementation review disposition **clean** (effort 1, 2 rounds, M1 fixed)

## Notes

Ordering: do the scaffolding/file items anytime; do BannedApiAnalyzers and the CPM prune after the mediator migration (tasks 040–049) restores a green build, otherwise violations and true package usage can't be verified by compiling.

`workflow.yml` already existed on this branch (task 078). Remaining audit debt after this implement: kebab-path-names warning on the Blazor JS initializer `Test.App.Client.lib.module.js` (assembly-name contract; severity lowered in `.editorconfig`), and memsearch-scaffold warning (not in the original brief).

Review kitchen: `kanban/to-do/074-fix-ganda-repo-audit-failures-and-validate-clean-run/review/` (effort 1, general only, 2 rounds, disposition **clean**).

## Session

- Implementer: grok session (2026-09-04)
- Review oracle: Grok 4.6 (ganda task work, 2026-09-04) — effort 1 general, rounds 1–2

## Results

Brought timewarp-state onto the TimeWarp repo-audit baseline used by timewarp-mediator / timewarp-architecture / timewarp-nuru. `ganda repo audit` now **exits 0**: 24 passed, 2 advisory warning failures, 1 skipped.

### What was implemented

- Scaffolded `tools/dev-cli` (Nuru 3 endpoint DSL + TimeWarp.Nuru.DevCli shared commands) and AOT-installed `bin/dev`.
- Added `msbuild/repository.props` and imported it from root `Directory.Build.props`. `source/Directory.Build.props` now has a literal `<Version>12.0.0-beta.3</Version>` (check-version contract) and packs `assets/logo.png`.
- Added `.envrc` (`PATH_add bin`), `skills/.gitkeep`, `.vscode/tasks.json` + `.vscode/settings.json` (window icon + title).
- Wired `Microsoft.CodeAnalysis.BannedApiAnalyzers` + `BannedSymbols.txt` (Console / ProcessStartInfo). Product `source/` had no violations. Runfiles suppress RS0030 in `scripts/Directory.Build.props`.
- Deleted 23 orphaned CPM `PackageVersion` entries (no exemption API). Added versions for Nuru.DevCli, Amuru.Tools, Terminal, BannedApiAnalyzers.
- Bumped TimeWarp.Nuru **2.1.0-beta.8 → 3.0.0-beta.76** and TimeWarp.Amuru **1.0.0-beta.5 → 1.0.0** (DotNet API lives in Amuru.Tools). Migrated `scripts/*.cs` from `NuruAppBuilder` to `NuruApp.CreateBuilder()` fluent Map API; handlers moved to static classes so Nuru interceptors compile.
- `workflow.yml` was already the canonical name. Release version extract now reads `<Version>` from `source/Directory.Build.props`.
- Kebab: renamed `.ai/Index.md` → `index.md`; replaced `kanban/backlog/_._` with `.gitkeep`; moved GitHub PR template to `.github/PULL_REQUEST_TEMPLATE/default.md` and pruned that directory. Left `Test.App.Client.lib.module.js` (Blazor `{AssemblyName}.lib.module.js` contract) as a documented warning.

### Files changed

- New: `tools/dev-cli/**`, `msbuild/repository.props`, `BannedSymbols.txt`, `.envrc`, `skills/.gitkeep`, `.vscode/*`, `scripts/Directory.Build.props`
- Edited: `Directory.Build.props`, `Directory.Packages.props`, `source/Directory.Build.props`, `scripts/*.cs`, `.editorconfig`, `.gitignore`, `.github/workflows/workflow.yml` (version extract + M1 SSOT assert), `tools/dev-cli/endpoints/workflow-command.cs` (M1), product csproj logo packing
- Removed: `.github/pull_request_template.md`, `kanban/backlog/_._`

### Key decisions

- Dev CLI mirrors timewarp-mediator (package TimeWarp.Nuru.DevCli, not in-repo DevCli sources).
- Did not rewrite GitHub `workflow.yml` CI steps onto `./bin/dev workflow`; existing `dotnet run --file ./scripts/*.cs` path still works after the Nuru 3 migration. `dev workflow` is available for local/CI later.
- `TimeWarpStateVersion` stays in `msbuild/repository.props` for CPM pins of TimeWarp.State; packable `<Version>` is the literal in `source/Directory.Build.props` (must stay in sync). Review M1 added CI/DevCli asserts that fail on drift.
- kebab-path-names is **warning** only because the Blazor JS initializer cannot be kebab-renamed without changing `AssemblyName` and assembly-qualified type strings in E2E.

### Test outcomes

- `ganda repo audit`: exit 0 (24 passed / 2 warning fails / 1 skipped)
- `dotnet run --file tools/dev-cli/dev.cs -- --help` and `./bin/dev --capabilities`: required commands present
- `dotnet run --file scripts/{clean,build,test,package,run-test-app,e2e}.cs -- --help`: compile OK
- `dotnet build source/timewarp-state/timewarp-state.csproj -c Release`: 0 warning / 0 error

### How to validate

**Smoke**

```bash
cd /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-state/task-074-fix-ganda-repo-audit-failures-and-validate-clean-r
ganda repo audit
echo "audit_exit=$?"
./bin/dev --capabilities | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['description']); print(sorted(e['pattern'] for e in d['endpoints']))"
dotnet run --file scripts/build.cs -- --help
dotnet build source/timewarp-state/timewarp-state.csproj -c Release
```

**Expect**

- `ganda repo audit` exits **0**. Summary line: `Repository passes — 2 advisory (non-blocking) warning(s).` Error-severity failures: **none**.
- Capabilities description is `Development CLI for timewarp-state`. Endpoints include `build`, `check-version`, `clean`, `self-install`, `test`, `verify-samples`, `workflow`.
- `scripts/build.cs --help` prints Nuru 3 help (no CS0136 / CS8801 / missing `DotNet`).
- Product pack build: `Build succeeded. 0 Warning(s) 0 Error(s)`. Package at `artifacts/packages/TimeWarp.State.12.0.0-beta.3.nupkg`.

**Automated gate**

```bash
ganda repo audit; echo "expect: exit 0"
test -x ./bin/dev && ./bin/dev --help
```

**Depends on:** `artifacts/packages/` must exist before restore (`nuget.config` lists it as a local source). `bin/dev` is gitignored; bootstrap with `dotnet run --file tools/dev-cli/dev.cs -- self-install` on a fresh clone.

**Not in scope:** memsearch `.githooks` scaffold; renaming `Test.App.Client` assembly / JS initializer; converting GitHub Actions jobs from `scripts/*.cs` to `./bin/dev workflow`.

**Version SSOT (review M1)**

```bash
python3 - <<'PY'
from xml.etree import ElementTree as ET
source = ET.parse("source/Directory.Build.props")
repo = ET.parse("msbuild/repository.props")
version = source.find(".//Version").text.strip()
cpm = repo.find(".//TimeWarpStateVersion").text.strip()
print(f"pack={version} cpm={cpm}")
raise SystemExit(0 if version == cpm else 1)
PY
# expect: pack=12.0.0-beta.3 cpm=12.0.0-beta.3 and exit 0
```

### Review disposition

- **Outcome:** clean
- **Effort / roster:** 1, general only
- **Rounds:** 2
- **Final counts:** bug 0; suggestion 1 fixed (M1); nit 0; **open 0**; wontfix 0
- **M1:** dual `TimeWarpStateVersion` vs source `<Version>` had no gate. Fixed with `AssertVersionSsot` in `tools/dev-cli/endpoints/workflow-command.cs` (PR + release) and matching pwsh asserts in `.github/workflows/workflow.yml` (ci job + release `extract_version`). Round 2 verified-fixed.
- **Paths:**
  - `kanban/to-do/074-fix-ganda-repo-audit-failures-and-validate-clean-run/review/review-framework.md`
  - `kanban/to-do/074-fix-ganda-repo-audit-failures-and-validate-clean-run/review/round-1/general.md`
  - `kanban/to-do/074-fix-ganda-repo-audit-failures-and-validate-clean-run/review/round-1/merged.md`
  - `kanban/to-do/074-fix-ganda-repo-audit-failures-and-validate-clean-run/review/round-2/general.md`
  - `kanban/to-do/074-fix-ganda-repo-audit-failures-and-validate-clean-run/review/round-2/merged.md`
  - `kanban/to-do/074-fix-ganda-repo-audit-failures-and-validate-clean-run/review/disposition.md`

### Audit output (2026-09-04)

```text
Passed: 24 | Failed: 2 | Skipped: 1
kebab-path-names (Warning) — tests/test-app/test-app-client/wwwroot/Test.App.Client.lib.module.js
memsearch-scaffold (Warning) — .memsearch.toml / .githooks missing
Repository passes — 2 advisory (non-blocking) warning(s).
```

Full table:

```text
assembly-metadata PASS
banned-api-analyzers PASS
banned-symbols PASS
bin-dev PASS
cpm-consistency PASS
dev-cli-capabilities PASS
directory-packages-props PASS
directory-structure PASS
editorconfig PASS
envrc PASS
kebab-path-names FAIL (Warning)
memsearch-memory-gitignore PASS
memsearch-scaffold FAIL (Warning)
msbuild-repository-props PASS
nuget-package-icon PASS
nuget-package-urls PASS
nuru PASS
region-annotations PASS
repository-root-alignment PASS
routine-journals-gitignore PASS
runfile-executables PASS
runfile-project-directives SKIP
runfile-shebang PASS
slnx PASS
source-directory-build-props PASS
vscode-window-icon PASS
workflow-file PASS
```
