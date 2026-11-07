# Add dev run command to launch test app

## Description

Add a `dev run` endpoint to the dev CLI (`tools/dev-cli/`) that launches the test app (`tests/test-app/test-app-server`, which hosts `test-app-client` Blazor WASM). Today you must remember `dotnet run --project tests/test-app/test-app-server --launch-profile https`.

## Requirements

- New `tools/dev-cli/endpoints/run-command.cs` following `test-command.cs` / `e2e-command.cs` pattern (`[NuruRoute("run", ...)]`, `Git.FindRoot()`, `Shell.Builder("dotnet")`, exit-code propagation).
- Runs `dotnet run --project tests/test-app/test-app-server --launch-profile <profile>`. Default profile `https` (matches `launchSettings.json`: `https://localhost:7011;http://localhost:5058`).
- Support `--profile http|https` (or `--http` flag) and pass-through of extra args to `dotnet run` if trivial; otherwise record as follow-up.
- `./bin/dev run --help` lists the command. `./bin/dev build` and `ganda repo audit` clean.
- Run `ganda repo audit --fix` in this worktree and commit whatever it changes as part of this task (audit fixes are policy; list them under `## Results`). Advisory warnings it cannot fix (kanban-landed 098, kebab-path names) stay as they are.

## Goal (implementer restatement)

Give the dev CLI a `run` command so `./bin/dev run` starts the test app
(`tests/test-app/test-app-server`, which hosts the `test-app-client` WASM) without
remembering the `dotnet run --project … --launch-profile https` line.

Acceptance:
- `tools/dev-cli/endpoints/run-command.cs` follows the test/e2e endpoint pattern
  (repo root via `Git.FindRoot()`, `Shell.Builder("dotnet")`, child exit code propagated).
- Default profile is `https`; `--profile http|https` (and a `--http` shortcut) picks another.
  Unknown profiles are refused before launch.
- Extra args after the options pass through to `dotnet run` (catch-all), when Nuru supports it cleanly.
- `./bin/dev run --help` lists it; `./bin/dev build` and `ganda repo audit` are clean.
- `ganda repo audit --fix` runs here and its changes are committed and listed under `## Results`.

## Checklist

- [x] `run-command.cs` endpoint added
- [x] Default launches test app on https profile; `--profile`/`--http` override works
- [x] `./bin/dev run --help` shows it; build + audit green
- [x] `ganda repo audit --fix` run and its changes committed

## Session

- Created: ses_e4dbb8ee4ffeHWuFv7lGTHeTG1 (2026-11-07)
- Launch: claude code review session, Steve asked to roll in audit fixes (2026-11-07)

- Implement oracle (ganda task work, claude implementer) 2026-11-07: added `run` endpoint, smoke-launched both profiles, ran audit --fix.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 89f1848b-2e51-4fbc-813e-53c369e80d2f, max-turns 500, Done — 2026-11-07T07:36:13Z

## Results

- New `tools/dev-cli/endpoints/run-command.cs`: `dev run [--profile|-p https|http] [--http] [-- <dotnet run args>]`.
  It runs `dotnet run --project tests/test-app/test-app-server --launch-profile <profile> <args>` from the repo root.
  The default profile is `https`, and `--http` is a shortcut for `--profile http`. An unknown profile, or `--http`
  with `--profile https`, is refused with exit 1 before anything launches. A non-zero child exit code is passed through.
  It prints the exact `dotnet …` line it runs.
- Pass-through: extra `dotnet run` args must come after `--` (for example `dev run -- --no-build`). Without `--`,
  Nuru rejects unknown options such as `--no-build`. So the catch-all only takes args that come after `--`.
  That is a Nuru parser rule; no follow-up is needed.
- `ganda repo audit --fix` changes, committed here (audit policy): `.vscode/settings.json` is no longer tracked
  (`git rm --cached`; the local file stays) and has been added to `.gitignore` (check `vscode-settings-ignored`).
- Gates: `dev build -q` passed. `dev self-install` rebuilt the AOT `./bin/dev` (local, not tracked), and
  `./bin/dev --help` lists `run`. `ganda repo audit` passes with 2 advisory warnings, both left alone as the task says:
  kanban-landed 098 has no usable worktree, and kebab-path-names (7 existing paths).

### How to validate

Smoke:
1. `./bin/dev run --help` (or `dotnet run --file tools/dev-cli/dev.cs -- run --help`)
2. `./bin/dev run`, then Ctrl+C
3. `./bin/dev run --http -- --no-build`, then Ctrl+C
4. `./bin/dev run --profile bogus; echo $?`

Expect:
1. Usage line `run [--profile,-p {profile}] [--http] {*args}` with both options described.
2. `Launching test app (profile: https)...`, then `Now listening on: https://localhost:7011` and `http://localhost:5058`.
3. A `dotnet run … --launch-profile http --no-build` line, then `Now listening on: http://localhost:5058`.
4. `Error: unknown profile 'bogus'. Use one of: https, http.` and exit code `1`.

## Notes

Requested by Steve 2026-11-07: "create task to add `dev run` endpoint that launches the test app please".
Test app entry: `tests/test-app/test-app-server/test-app-server.csproj`, launch profiles in `Properties/launchSettings.json`.
