# Review framework — task 074

**Date:** 2026-09-04
**Host task:** kanban/in-progress/074-fix-ganda-repo-audit-failures-and-validate-clean-run/
**Diff scope:** commit `193e83d6` on branch `task/074-fix-ganda-repo-audit-failures-and-validate-clean-r` vs `origin/feature/080-timewarp-mediator-14-beta` (excluding `kanban/` process files except this review kitchen)
**Plan / brief:** Bring timewarp-state onto the TimeWarp `ganda repo audit` baseline. Scaffold `bin/dev` + `tools/dev-cli`, `msbuild/repository.props`, `.envrc`, `skills/`, `.vscode/`, BannedApiAnalyzers + `BannedSymbols.txt`, prune orphaned CPM pins, bump TimeWarp.Nuru to 3.x and migrate `scripts/*.cs`, leave `workflow.yml` as the canonical workflow name. Validate `ganda repo audit` exits 0 (warnings at most). See `task.md` Requirements, Checklist, and Results.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle — Grok 4.6 (ganda task work, 2026-09-04)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Round 2

Re-review after M1 fix: version SSOT assert in `tools/dev-cli/endpoints/workflow-command.cs` and `.github/workflows/workflow.yml` (ci job + release extract_version). Re-verify M1; scan the fix delta for new defects.

## Round 3

Reopened remaining slice (commits `66cf4770` + `ab315c91` vs `origin/master`): thin YAML trigger + `dev workflow` PR/merge (`e2e` + `pack`) and release promote (Nuru/Ganda, no rebuild). Do not clobber rounds 1–2. Re-verify M1 still holds in DevCli (YAML SSOT extract was removed with thin YAML). Scan the new delta for defects against the reopened brief.

## Round 4

NU1102 follow-up after round 3 (commits `6237d0b9` + `75f7f4de` vs round-3 HEAD `c17fc5d1`). GitHub `ci` failed: `dev build` of `timewarp-state.slnx` NU1102 on samples (`TimeWarp.State`/`Plus` at `TimeWarpStateVersion` not on nuget.org; LocalNuGetFeed empty until pack). Fix: `dev build` derives `artifacts/timewarp-state.build.slnf` omitting `samples/**`; workflow order is pack then verify-samples. Do not clobber rounds 1–3. Re-verify M1. Scan the NU1102 delta and surrounding restore/pack/sample call sites.

## Round 5

Fixie follow-up after round 4 (commits `f6f5a32a` + `52ba2de0` vs round-4 HEAD `79605cb9`). GitHub `ci` run 33836414684: NU1102 gone; Test failed at `==> Run analyzer tests` with `Run "dotnet tool restore" to make the "fixie" command available.` Old `scripts/build.cs` ran `dotnet tool restore`; YAML no longer calls it; `dev test` / `scripts/test.cs` invoked `dotnet fixie` without restore. Fix: `dev test` and `scripts/test.cs` run `dotnet tool restore` (whole `.config/dotnet-tools.json` manifest, including `fixie.console` 3.4.0) before the first Fixie invoke. Do not clobber rounds 1–4. Re-verify M1. Scan the Fixie-restore delta and surrounding Test/workflow/e2e call sites.
