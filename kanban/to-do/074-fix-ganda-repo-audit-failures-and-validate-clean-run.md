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

- [ ] Scaffold `bin/dev` + `tools/dev-cli` (dev-cli-capabilities and region-annotations should then pass)
- [ ] Add `msbuild/repository.props` (un-skips the repository props content check — fix whatever it then reports)
- [ ] Add `.envrc`
- [ ] Create `skills/` and `kanban/archived/`
- [ ] Add `.vscode/tasks.json` + `.vscode/settings.json` window-icon config
- [ ] Wire BannedApiAnalyzers in `Directory.Build.props` + add `BannedSymbols.txt`; fix surfaced violations
- [ ] Prune/justify the 23 orphaned PackageVersion entries
- [ ] Bump TimeWarp.Nuru to 3.0.0-beta.x and migrate `scripts/*.cs` to the new API
- [ ] Rename `.github/workflows/ci-cd.yml` → `workflow.yml`; fix references
- [ ] **Validate:** `ganda repo audit` reports 0 failed (warnings at most) — paste the clean output into this task before moving to done

## Notes

Ordering: do the scaffolding/file items anytime; do BannedApiAnalyzers and the CPM prune after the mediator migration (tasks 040–049) restores a green build, otherwise violations and true package usage can't be verified by compiling.
