# Round 1 — general
**Date:** 2026-11-07
**Scope reviewed:** same as framework (a18952fd vs merge-base b2317f33; product paths only)

## Summary

`dev run` wraps `dotnet run` for `tests/test-app/test-app-server` the same way `test` and `e2e` wrap their runners: repo root from `Git.FindRoot()`, `Shell.Builder("dotnet")` with `WithNoValidation()`, and the child exit code copied to `Environment.ExitCode`. Default profile `https`, `--profile` / `-p`, and `--http` match the task. The implementer restatement matches the original task (no misread): it adds an explicit unknown-profile refusal and asks for both the profile option and the `--http` shortcut, and the diff does both while still meeting the original "or" requirement. Risk is low. The audit ignore of `.vscode/settings.json` is the committed `ganda repo audit --fix` result the task asked for.

## Verification

- `./bin/dev --help` lists `run`. `./bin/dev run --help` prints `run [--profile,-p {profile}] [--http] {*args}` and describes both options. `bin/dev` is newer than `run-command.cs`.
- `./bin/dev run --profile bogus` prints `Error: unknown profile 'bogus'. Use one of: https, http.` and exits 1. Nothing is launched.
- `./bin/dev run --http --profile https` and `./bin/dev run --profile https --http` both print the conflict error and exit 1.
- `./bin/dev run -p http -- --help` prints `Launching test app (profile: http)...` and `dotnet run … --launch-profile http --help`, then dotnet's own help, exit 0. `./bin/dev run --profile HTTPS -- --help` canonicalizes to `https`. `./bin/dev run --http --profile http -- --help` uses profile `http`.
- `./bin/dev run` with no extra args starts `dotnet run --project <repo>/tests/test-app/test-app-server --launch-profile https` with working directory the repo root. The process logged `Using launch settings from …/Properties/launchSettings.json`, then `Now listening on: https://localhost:7011` and `Now listening on: http://localhost:5058`. Those URLs match the `https` profile in `launchSettings.json`. A later SIGTERM came back as `Test app exited with code 143.` and process exit 143.
- `./bin/dev run --no-build` (no `--`) is `Unknown option '--no-build'.` exit 1. Args after `--` are appended to `dotnet run`, which is the pass-through the task allowed when Nuru can do it cleanly. Results already records that `--` is required.
- `ganda repo audit` in this sandbox: `vscode-settings-ignored` passed. `kebab-path-names` is the expected warning (7 existing paths) and stays, as the task says. `self-hosted-runner` exited with `Could not determine whether the repository is private (gh repo view --json isPrivate)` because this session could not reach `gh`; that check is not touched by the diff. `kanban-landed` passed here. `dev build` was not re-run: the solution sources are unchanged, and the new endpoint is the AOT `bin/dev` that served the checks above.
- Compared with `test-command.cs` and `e2e-command.cs`: same route attribute, nested handler, terminal injection, missing-root and missing-file exits, and non-zero child handling. `tw-csharp` file name, explicit types, file-scoped namespace, Allman braces, and type-stem `Terminal` match the neighboring endpoints. `region-annotations` passed for `tools/dev-cli`.

## Issues

None.

## Observations (not findings)

- `run --help` marks `*args` Required: Yes. `./bin/dev run` with no extra args still launches. That row is Nuru's catch-all help text; the command does not require arguments.
- The restatement says "and" for `--profile` and `--http`, and it refuses unknown profiles before launch. The original requirement allows either flag and does not mention the refusal. The diff satisfies the original requirement and the stricter restatement, so this is not a misread.
