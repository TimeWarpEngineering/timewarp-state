# Round 3 — merged findings
**Date:** 2026-09-03
**Sources:** general
**Scope:** post-disposition delta a6700c2f..HEAD excluding kanban/ (CI workflow + runfile scripts + MSTest pin + .gitignore). Rounds 1–2 (mediator swap) are frozen; M1–M11 unchanged (0 open, M6 wontfix).

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 3 | 0 |
| nit | 0 | 2 | 0 |

## Issues

### M12 — Severity: suggestion — Status: fixed
- File: scripts/e2e.cs:31
- Description: `var useHttp = true;` is hardcoded; the script never reads the `UseHttp` env var that workflow.yml sets, so the https branch and the hardened `InstallLinuxDevCerts` are unreachable and the workflow `env:` is decorative.
- Suggestion: read the env var (default true when unset/unparseable) so the workflow drives the decision and the https path is live.
- Source: general
- Disposition notes: `useHttp` now reads the `UseHttp` env var via `bool.TryParse`, default true; `InstallLinuxDevCerts` comment trimmed so it no longer claims the run is http.

### M13 — Severity: suggestion — Status: fixed
- File: scripts/test.cs:23
- Description: All ten `if (exitCode != 0) Environment.Exit(1)` guards are dead under Amuru 1.0.0-beta.5 (`RunAsync` throws on non-zero). CI still fails (Nuru returns 1 on handler exception) but the log does not name the failing suite.
- Suggestion: a `RunStep(name, func)` helper that catches, names the suite, and exits 1.
- Source: general
- Disposition notes: `RunStep(name, step)` helper in test.cs prints `==> name`, catches exception or non-zero exit, writes `name failed: …` to stderr, exits 1. Ten blocks replaced, order unchanged.

### M14 — Severity: suggestion — Status: fixed
- File: .github/workflows/workflow.yml:61 (via scripts/clean.cs:27)
- Description: `Clean solution` runs `dotnet nuget locals all --clear`, deleting the `~/.nuget/packages` folder that `actions/cache` restored two steps earlier; newly live since 057b08a9 made the step run.
- Suggestion: skip the locals clear when running under CI (`CI` env var) so the cache restore is used.
- Source: general
- Disposition notes: clean.cs skips `nuget locals all --clear` when `CI == "true"` with a log line. Same guard applied to package.cs, because the release job also restores actions/cache before running it.

### M15 — Severity: nit — Status: fixed
- File: scripts/package.cs:19; scripts/build.cs:106
- Description: `package.cs` lacks the `artifacts/packages` guard the other four runfiles got; `build.cs`'s `clean` route deletes `./artifacts` without recreating `artifacts/packages` (clean.cs does).
- Suggestion: add the guard to both; note in the comment that it protects child `dotnet` invocations, not the runfile's own `#:package` restore.
- Source: general
- Disposition notes: package.cs got the guard; build.cs `CleanSolution` recreates `artifacts/packages` after deleting `./artifacts`; the shared comment in all five runfiles now states it protects child `dotnet` invocations, not the runfile's own `#:package` restore.

### M16 — Severity: nit — Status: fixed
- File: scripts/e2e.cs:458
- Description: `WaitForExit()` sits inside `if (!HasExited)`, so when the SUT crashed on its own the writers are disposed while output readers may still be draining; the tail of the SUT logs is truncated (no longer fatal thanks to the ObjectDisposedException guards).
- Suggestion: hoist `WaitForExit()` out of the `if`.
- Source: general
- Disposition notes: `WaitForExit()` hoisted out of `if (!HasExited)` in KillSut so readers drain on both paths before the writers are disposed.

## Duplicates / conflicts

- None (single reviewer).
