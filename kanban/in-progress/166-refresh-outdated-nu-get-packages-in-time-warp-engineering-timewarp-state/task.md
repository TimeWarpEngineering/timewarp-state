# Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-state

## Description

Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-state.
Filter: (all)
Outdated:
  Microsoft.Playwright.MSTest.v4 1.65.0 -> 1.65.1 (dev-only: tests/test-app-end-to-end-tests/test-app-end-to-end-tests.csproj)
Every outdated pin is dev-only. Bump them. Do not change <Version> in source/Directory.Build.props: the wave merges this task and does not release this repo.
Edit Directory.Packages.props in this repo only.

## Goal (implementer restatement)

Bump the one outdated dev-only pin, Microsoft.Playwright.MSTest.v4, from 1.65.0 to 1.65.1 in the root Directory.Packages.props.

Acceptance criteria:
- Directory.Packages.props pins Microsoft.Playwright.MSTest.v4 at 1.65.1.
- source/Directory.Build.props <Version> is unchanged (no release).
- `./bin/dev build` passes and `ganda repo audit` passes.

## Session

- Minted by ganda task wave run 20261119T023451Z-133310
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5), session not reported, max-turns 500, Done — 2026-11-19T03:06:57Z

## Results

- `Directory.Packages.props`: Microsoft.Playwright.MSTest.v4 1.65.0 -> 1.65.1 (dev-only, used by tests/test-app-end-to-end-tests).
- `source/Directory.Build.props` <Version> unchanged.

### Gates

- `./bin/dev build` — PASS (0 warnings, 0 errors).
- Test files: none added or edited. The only consumer is the Playwright E2E project (MSTest, not a Jaribu runfile; needs a running app); CI runs it.
- `ganda repo audit` — PASS with 1 advisory warning (`vscode-window-icon`: .vscode/settings.json missing). `ganda repo audit --fix` wrote the git-ignored local `.vscode/settings.json`; rerun reports "Repository passes all audit checks" (44 passed, 0 failed). No tracked file changed by the fix.

### How to validate

Smoke: `grep -n 'Microsoft.Playwright.MSTest.v4' Directory.Packages.props` then `./bin/dev build`.
Expect: the pin reads `Version="1.65.1"` and the build succeeds with 0 errors; CI's E2E job restores 1.65.1 and passes.
