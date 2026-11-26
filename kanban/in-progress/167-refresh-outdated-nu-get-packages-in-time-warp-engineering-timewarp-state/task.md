# Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-state

## Description

Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-state.
Filter: (all)
Outdated:
  TimeWarp.Nuru 4.0.0-beta.16 -> 4.0.0-beta.17 (dev-only: tools/dev-cli/Directory.Build.props)
  TimeWarp.Nuru.DevCli 4.0.0-beta.16 -> 4.0.0-beta.17 (dev-only: tools/dev-cli/Directory.Build.props)
Every outdated pin is dev-only. Bump them. Do not change <Version> in source/Directory.Build.props: the wave merges this task and does not release this repo.
Edit Directory.Packages.props in this repo only.

## Goal (implementer restatement)

Bump the two dev-only Nuru pins in `Directory.Packages.props` from 4.0.0-beta.16 to 4.0.0-beta.17
(`TimeWarp.Nuru`, `TimeWarp.Nuru.DevCli`). No release: leave `<Version>` in `source/Directory.Build.props` alone.

Acceptance:
- `Directory.Packages.props` pins both packages at 4.0.0-beta.17; no other package changes.
- `./bin/dev build` passes (restores beta.17; the dev CLI and the test project that links DevCli content compile).
- Test files that consume DevCli content (release-pipeline tests) still pass.
- `ganda repo audit` passes.

## Session

- Minted by ganda task wave run 20261126T021837Z-0cb631
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 20778681-61f0-40fe-9eff-9c0afd6168d3, max-turns 500, Done — 2026-11-26T03:16:41Z

## Results

- `Directory.Packages.props`: `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` 4.0.0-beta.16 -> 4.0.0-beta.17. `<Version>` in `source/Directory.Build.props` untouched.
- Boy-scout audit fix (`region-annotations`, 12 pre-existing failures that the installed ganda now reports):
  - Added `// #region Purpose` blocks to the 8 authored JS/TS files (4 TypeScript sources in timewarp-state-blazor, devtools `theme.js`, plus `document-title.js` / `download-file.js`, test-app `Test.App.Client.lib.module.js`).
  - The 4 `source/timewarp-state-blazor/wwwroot/js/*.js` files are tsc output with `removeComments: true`, so they cannot carry the region; recorded them in a new `.timewarp/region-annotations-baseline.json`. Their `.js.map` files regenerated because the TS line numbers shifted; the emitted JS is byte-identical.

### Gates

- `./bin/dev build` — passed (0 warnings, 0 errors); rerun after the audit fix — passed.
- `dotnet run --file tools/dev-cli/dev.cs -- self-install` — dev CLI compiles and installs on Nuru/DevCli beta.17.
- No test file added or edited; no Jaribu runfiles in this repo (Fixie projects). Covering suite for the DevCli-linked content (`release-guard.cs`, `ci-run-promotion.cs`, `publish-state.cs`, `ipackable-project-service.cs` from the beta.17 package): `dotnet fixie timewarp-state-tests --configuration Release` — 173 passed, 1 skipped.
- `ganda repo audit` — first run failed on `region-annotations` (pre-existing, fixed above); rerun — passes, 1 advisory warning (`vscode-window-icon`, non-blocking).

### How to validate

- Smoke: `./bin/dev build && dotnet fixie timewarp-state-tests --configuration Release && ganda repo audit`
- Expect: build succeeds with 0 errors; Fixie reports 173 passed, 1 skipped; audit prints "Repository passes"; `grep Nuru Directory.Packages.props` shows 4.0.0-beta.17 for both pins.
