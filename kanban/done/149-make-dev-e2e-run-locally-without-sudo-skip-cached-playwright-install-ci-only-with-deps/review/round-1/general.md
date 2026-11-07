# Round 1 — general

**Date:** 2026-11-07
**Scope reviewed:** branch `task/149-make-dev-e2e-run-locally-without-sudo-skip-cached` vs merge-base `a85ce50c`. Product diff in `scripts/e2e-build.cs`, `scripts/e2e-dev-certs.cs`, `scripts/e2e.cs`, `scripts/global-usings.cs`, and `tools/dev-cli/endpoints/e2e-command.cs`.

## Summary

`dev e2e` now skips Playwright's Chromium install when revision 1248 (chromium and the headless shell) is already in the browsers cache, and it adds `--with-deps` only in CI or when the caller passes the flag. Install and dev-cert commands get empty stdin and a timeout, and a non-root Linux box that fails `sudo -n true` drops `--with-deps` instead of waiting on a password. The implementer's restatement matches the task. Local logs and the Playwright 1.64.1 registry agree with the code. No issues.

## Checked

- `browsers.json` for Microsoft.Playwright 1.64.1 stores `"revision": "1248"` as a string for both `chromium` and `chromium-headless-shell`, so `JsonElement.GetString()` reads it. The cache at `~/.cache/ms-playwright` has `INSTALLATION_COMPLETE` for `chromium-1248` and `chromium_headless_shell-1248`. Folder names match Playwright's `name.replace(/-/g, "_") + "-" + revision`.
- Playwright's registry uses `PLAYWRIGHT_BROWSERS_PATH` when set, `0` as the in-package sentinel, and otherwise `XDG_CACHE_HOME` or `~/.cache` on Linux, `~/Library/Caches` on macOS, and LocalAppData on Windows. `ResolvePlaywrightBrowsersPath` follows that. The sentinel `0` falls back to install, which the restatement states.
- Bound Nuru bool flags are optional: absence is false, presence is true, and `--skip-setup` binds to `skipSetup`. `./bin/dev e2e --help` lists both options and the route description. `dev workflow` still constructs a default `E2eCommand`, so CI keeps the install and adds `--with-deps` from `GITHUB_ACTIONS` / `CI`. `.github/workflows/workflow.yml` does not cache Playwright and sets `UseHttp=true`.
- Amuru `WithStandardInput("")` is an immediate EOF, and a timeout with validation off returns `CommandResult.TimeoutExitCode` instead of throwing. The install stays best-effort. `--clean` runs only when `IsCi()` is true. Local https skips `--trust` only when `dotnet dev-certs https --check --trust` exits 0.
- `/tmp/e2e-149-run1.log` through `run4.log` match Results: cache hit skips install (12 passed), empty `PLAYWRIGHT_BROWSERS_PATH` warns that `sudo -n true` failed and runs `install chromium` with no `--with-deps` (12 passed), `--skip-setup` skips tool restore, dev certs, and the browser step (12 passed), and https trust runs non-interactively then fails 0/12 on `ERR_CERT_AUTHORITY_INVALID`. No sudo password prompt in those logs. That https failure is outside this task.

## Issues

None.
