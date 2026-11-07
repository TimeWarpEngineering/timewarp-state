# Add dev run command to launch test app

## Description

Add a `dev run` endpoint to the dev CLI (`tools/dev-cli/`) that launches the test app (`tests/test-app/test-app-server`, which hosts `test-app-client` Blazor WASM). Today you must remember `dotnet run --project tests/test-app/test-app-server --launch-profile https`.

## Requirements

- New `tools/dev-cli/endpoints/run-command.cs` following `test-command.cs` / `e2e-command.cs` pattern (`[NuruRoute("run", ...)]`, `Git.FindRoot()`, `Shell.Builder("dotnet")`, exit-code propagation).
- Runs `dotnet run --project tests/test-app/test-app-server --launch-profile <profile>`. Default profile `https` (matches `launchSettings.json`: `https://localhost:7011;http://localhost:5058`).
- Support `--profile http|https` (or `--http` flag) and pass-through of extra args to `dotnet run` if trivial; otherwise record as follow-up.
- `./bin/dev run --help` lists the command. `./bin/dev build` and `ganda repo audit` clean.
- Run `ganda repo audit --fix` in this worktree and commit whatever it changes as part of this task (audit fixes are policy; list them under `## Results`). Advisory warnings it cannot fix (kanban-landed 098, kebab-path names) stay as they are.

## Checklist

- [ ] `run-command.cs` endpoint added
- [ ] Default launches test app on https profile; `--profile`/`--http` override works
- [ ] `./bin/dev run --help` shows it; build + audit green
- [ ] `ganda repo audit --fix` run and its changes committed

## Session

- Created: ses_e4dbb8ee4ffeHWuFv7lGTHeTG1 (2026-11-07)
- Launch: claude code review session, Steve asked to roll in audit fixes (2026-11-07)

## Notes

Requested by Steve 2026-11-07: "create task to add `dev run` endpoint that launches the test app please".
Test app entry: `tests/test-app/test-app-server/test-app-server.csproj`, launch profiles in `Properties/launchSettings.json`.
