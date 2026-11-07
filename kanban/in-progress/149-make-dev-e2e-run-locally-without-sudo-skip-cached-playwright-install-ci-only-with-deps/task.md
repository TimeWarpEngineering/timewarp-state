# Make dev e2e run locally without sudo: skip cached Playwright install, CI-only with-deps

## Description

`dev e2e` has only ever been run in CI. Locally it hangs at `Ensure-Browsers-Installed` on a
`[sudo: authenticate] Password:` prompt because `scripts/e2e-build.cs` `EnsureBrowsersInstalled` always runs
`playwright.ps1 install chromium --with-deps`. `--with-deps` apt-installs system libraries and needs root. GitHub
runners have passwordless sudo so this is silent there; on a dev box it blocks. The comment says a failed install
only warns, but a password prompt is a hang, not a failure.

The browser is already cached on the dev box (`~/.cache/ms-playwright/chromium-1248`, matching Playwright 1.64.1 in
`Directory.Packages.props`), so the install step is pure waste locally. CI spends ~16s on it per run too.

`scripts/e2e-dev-certs.cs` `InstallLinuxDevCerts` also runs `dotnet dev-certs https --clean` then `--trust` on every
run. `--clean` wipes the developer's existing dev cert and `--trust` can prompt on Linux.

## Requirements

- Version check before install: determine the Chromium revision Playwright 1.64.1 wants (the `browsers.json` shipped
  in the Microsoft.Playwright package, or `playwright.ps1 install --dry-run`) and skip the install when that revision
  is already present under the Playwright browsers path (`PLAYWRIGHT_BROWSERS_PATH` or `~/.cache/ms-playwright`).
  Print one line saying which revision was found and that install was skipped.
- When install is needed, pass `--with-deps` only in CI (`GITHUB_ACTIONS` / `CI` env) or when the caller passes an
  explicit `--with-deps` flag to `dev e2e`. Locally run plain `install chromium`, which needs no root.
- Never block on an interactive prompt: run the install with stdin closed / non-interactive so a sudo prompt fails
  fast and hits the existing warn-and-continue path instead of hanging.
- Dev certs: skip `--clean` outside CI. Keep `--trust` but make it non-interactive and warn-and-continue, or skip it
  when `dotnet dev-certs https --check --trust` already passes.
- Add `--skip-setup` to `dev e2e` to bypass tool restore, dev-certs, and browser install entirely for a developer who
  knows the box is ready. Document it in the command description.
- Run `dev e2e` locally end to end and record the pass/fail count under `## Results`. If tests fail for reasons
  unrelated to this task (see memory note: suite had a failing baseline historically), list them as-is; do not fix
  them here.
- Run `ganda repo audit --fix` in this worktree and commit whatever it changes (audit fixes are policy).

## Checklist

- [ ] Browser revision check implemented; install skipped when cached (message shows revision)
- [ ] `--with-deps` only in CI or with explicit flag
- [ ] Install runs non-interactively; sudo prompt cannot hang the step
- [ ] Dev-certs `--clean` CI-only; `--trust` non-interactive or skipped when already trusted
- [ ] `dev e2e --skip-setup` added and documented
- [ ] `dev e2e` run locally without prompting; results recorded
- [ ] CI run still green (install path still taken on a fresh runner)
- [ ] `ganda repo audit --fix` run and its changes committed
- [ ] `## Results` filled in

## Session

- Created: ffba6066-c22c-42a7-95c8-de75506a6687 (2026-11-07)
- Context: Steve hit the sudo prompt running `dev e2e` locally after task 147/148; asked for no reinstall when cached, maybe a version check (2026-11-07)

## Notes

Related: task 150 (attestation-gated CI) wants `dev e2e` to be cheap and reliable locally so a signed local run can
stand in for the CI e2e step. This task is the prerequisite.
