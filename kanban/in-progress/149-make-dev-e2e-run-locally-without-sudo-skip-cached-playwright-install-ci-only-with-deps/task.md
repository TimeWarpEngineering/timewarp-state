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

## Goal (implementer restatement)

Make `dev e2e` finish on a developer box with no password prompt, and stop paying for setup that is already done.

Acceptance criteria:

- The browser step reads the Chromium revision the pinned Playwright wants (from `browsers.json` in the test build
  output) and skips the install when that revision (chromium + headless shell) is already installed in the Playwright
  browsers path. It prints one line naming the revision and saying install was skipped.
- When install runs, `--with-deps` is added only in CI (`GITHUB_ACTIONS`/`CI`) or when `dev e2e --with-deps` asks
  for it. Locally the plain `install chromium` runs, which needs no root.
- The install cannot hang on sudo: stdin is empty, `--with-deps` is dropped (with a warning) when not root and
  `sudo -n true` fails, and a timeout backs it up. A failure still only warns.
- Dev certs: `--clean` runs only in CI. `--trust` is skipped when `dotnet dev-certs https --check --trust` passes;
  otherwise it runs with empty stdin and a timeout, and a failure only warns.
- `dev e2e --skip-setup` skips tool restore, dev certs, and browser install (clean and build still run); the command
  help documents it.
- `dev e2e` runs locally end to end with no prompt; pass/fail counts go under `## Results` (known failing tests are
  listed, not fixed).
- CI keeps taking the install path on a fresh runner (no cached revision there).
- `ganda repo audit --fix` runs and its changes are committed.

## Checklist

- [x] Browser revision check implemented; install skipped when cached (message shows revision)
- [x] `--with-deps` only in CI or with explicit flag
- [x] Install runs non-interactively; sudo prompt cannot hang the step
- [x] Dev-certs `--clean` CI-only; `--trust` non-interactive or skipped when already trusted
- [x] `dev e2e --skip-setup` added and documented
- [x] `dev e2e` run locally without prompting; results recorded
- [ ] CI run still green (install path still taken on a fresh runner) — checked on the PR; locally the cache-miss install path ran green (see Results)
- [x] `ganda repo audit --fix` run and its changes committed
- [x] `## Results` filled in

## Session

- Created: ffba6066-c22c-42a7-95c8-de75506a6687 (2026-11-07)
- Context: Steve hit the sudo prompt running `dev e2e` locally after task 147/148; asked for no reinstall when cached, maybe a version check (2026-11-07)
- Implemented under `ganda task work` implement oracle (2026-11-07): browser revision check, CI-only with-deps + sudo -n
  guard, non-interactive dev certs, `--skip-setup`/`--with-deps`; 4 local runs recorded under Results.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 0e746bf4-5483-4d66-b5ea-48a49e5fbe98, max-turns 500, Done — 2026-11-07T12:56:37Z

## Results

### What changed

- `scripts/e2e-build.cs` `EnsureBrowsersInstalled` reads the chromium revision from
  `<test output>/.playwright/package/browsers.json` (1248 for Playwright 1.64.1). It skips the install when
  `chromium-<rev>` and `chromium_headless_shell-<rev>` both have `INSTALLATION_COMPLETE` under
  `PLAYWRIGHT_BROWSERS_PATH`, or under the per-OS `ms-playwright` cache (`$XDG_CACHE_HOME` or `~/.cache` on Linux).
  It prints one line either way. `PLAYWRIGHT_BROWSERS_PATH=0` and an unreadable `browsers.json` fall back to
  installing.
- `--with-deps` is added only when `GITHUB_ACTIONS`/`CI` is truthy or `dev e2e --with-deps` is passed. Before it is
  added, a non-root Linux user must pass `sudo -n true`. Otherwise it is dropped with a warning that prints the exact
  `sudo pwsh … install-deps chromium` command. The install runs with empty stdin, `DEBIAN_FRONTEND=noninteractive`,
  and a 10-minute timeout (exit 124 → warning). A failure still only warns.
- `scripts/e2e-dev-certs.cs`: `--clean` runs only in CI. Locally `dotnet dev-certs https --check --trust` runs first,
  and a pass skips trust. Every dev-certs call has empty stdin and a 2-minute timeout, and a failure only warns.
- `dev e2e --skip-setup` skips tool restore, dev certs, and browser install (clean and build still run).
  `dev e2e --with-deps` asks for system packages outside CI. Both are documented in the command description and
  option help, and both pass through to `scripts/e2e.cs` (route `--skip-setup --with-deps`, both optional).
- `scripts/global-usings.cs` gained `global using System.Text.Json;` because analyzer TW0007 rejects file-level
  usings in runfiles. All scripts/*.cs runfiles still build.

### Local runs (2026-11-07, WSL2, no CI env, stdin `</dev/null`, no sudo prompt in any run)

| Run | Command | Browser / certs step | Tests |
|-----|---------|----------------------|-------|
| 1 | `dev e2e` | `Playwright Chromium 1248 found in /home/steve/.cache/ms-playwright; skipping install.` | 12 passed / 0 failed (3m01s total) |
| 2 | `PLAYWRIGHT_BROWSERS_PATH=/tmp/pw-149 dev e2e --with-deps` | not found → `sudo -n true` failed → warned, ran plain `install chromium`, downloaded 1248 + headless shell + ffmpeg | 12 passed / 0 failed (1m52s) |
| 3 | `dev e2e --skip-setup` | tool restore, dev certs, browser all skipped | 12 passed / 0 failed (1m00s) |
| 4 | `UseHttp=false dev e2e` (https) | `--check --trust` exit 7 → `--trust` ran non-interactively: "Successfully trusted the existing HTTPS certificate." | 0 passed / 12 failed |

Run 4 failures are not caused by this task. Every test fails with `net::ERR_CERT_AUTHORITY_INVALID at
https://localhost:7011/...`: Chrome for Testing does not trust the Linux dev cert even after `dotnet dev-certs https
--trust`. `--check --trust` also still reports 7 after a successful trust, so local https runs run `--trust` each time
(harmless and non-interactive). CI and the default `dev e2e` run over http (UseHttp=true), so this is not on the CI
path. It is a candidate follow-up task (https e2e on Linux), not fixed here.

The historical failing E2E baseline (memory note, 11/13 on 2026-09-03) no longer reproduces: http runs are 12/12.

CI: no Playwright cache in `.github/workflows/workflow.yml`, so a fresh `ubuntu-latest` runner misses the cache and
still runs `install chromium --with-deps` (`GITHUB_ACTIONS=true`, passwordless sudo). Confirm on the PR run.

`ganda repo audit --fix`: passes and changed no files. Two advisory warnings were already there and stay as-is:
kebab-path-names lists Blazor/MSBuild-convention file names, and kanban-landed lists a claim on task 098 with no
worktree.

### How to validate

Smoke:

```bash
dev e2e </dev/null
dev e2e --skip-setup </dev/null
dev e2e --help
```

Expect:

- `dev e2e` prints `Playwright Chromium 1248 found in ~/.cache/ms-playwright; skipping install.`, never shows a
  `[sudo] password` prompt, and ends with `Passed! - Failed: 0, Passed: 12`.
- `dev e2e --skip-setup` prints `--skip-setup: skipping tool restore, dev certs, and browser install.` and passes
  12/12.
- `dev e2e --help` lists `--skip-setup` and `--with-deps` with descriptions.
- With an empty `PLAYWRIGHT_BROWSERS_PATH` folder, the step installs `chromium` without `--with-deps` (and with
  `--with-deps` it warns that `sudo -n true` failed and drops the flag instead of prompting).
- CI e2e step log shows `not found in /home/runner/.cache/ms-playwright; installing.` and
  `Running: pwsh … install chromium --with-deps`, and the job is green.

## Notes

Related: task 150 (attestation-gated CI) wants `dev e2e` to be cheap and reliable locally so a signed local run can
stand in for the CI e2e step. This task is the prerequisite.
