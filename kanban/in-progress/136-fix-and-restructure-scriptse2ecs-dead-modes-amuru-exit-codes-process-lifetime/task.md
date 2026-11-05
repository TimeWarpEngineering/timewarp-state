# Fix and restructure scripts/e2e.cs (dead modes, Amuru exit codes, process lifetime)
## Description

`scripts/e2e.cs` (517 LOC) has dead code paths, exit-code checks that can never fire, and an undisposed SUT process. Fix the bugs and split the script into a few cohesive parts.

## Requirements

Bugs:
- `runMode` is the constant `"Auto"` (line 55). Manual, Development and Release branches are dead, and Dev/Release would block forever because they `await RunAsync()` on the server. Remove them or make the mode a real argument that starts the server in the background.
- Every `if (exitCode != 0) Environment.Exit(1)` is dead: Amuru beta.5 throws on non-zero exit. Either use `WithNoValidation()` and keep the checks, or drop the checks and let the exception carry the failure. `Environment.Exit` also skips the `using var context` disposal.
- The SUT `Process` (line ~346) is never disposed. `KillSut` checks `HasExited` then `Kill()`, which can throw inside `finally`; `WaitForExit()` has no timeout.
- `WaitForSut` does not dispose the `HttpResponseMessage`, has a bare `catch`, retries silently on non-success, and never checks whether the server process already died.
- The `UseHttp` string replace (~line 248) is a silent no-op if the JSON has no space after the colon.
- Exception at ~199 names the wrong project.

Conventions:
- Use TimeWarp.Amuru instead of `System.Diagnostics.Process` (lines ~300, 346, 470).
- Normalise indentation (file mixes 2 and 4 spaces).
- Remove `async` from methods with no `await` (CS1998).

Split:
- Build steps (~203-298)
- A `SutHost` class owning the process and log writers, `IAsyncDisposable` (~300-427, 470-485)
- Test runner (~429-468)
- Dev-certs step (~487-517)
- `Run` stays as orchestration.

Cleanups: hard-coded `./tests/test-app/output` though `outputPath` exists; log paths built twice; `UseHttp` env var set twice; `net11.0` hard-coded; `settings` loop has one element; `WithClean()` dev-certs result discarded.

## Goal (implementer restatement)

Make the e2e runfile honest about what it does and fails on. The current state of the code changes the plan in two ways:

- Commit 8eadac32 (task 135) already moved the steps into `e2e-build.cs`, `e2e-sut.cs`, `e2e-tests.cs` and `e2e-dev-certs.cs`. Only the bugs and cleanups are left.
- CPM now pins TimeWarp.Amuru **3.0.0-beta.1**. Its default validation is `None`: `RunAsync` returns a non-zero exit code instead of throwing. The `exitCode != 0` checks are live again, and `Environment.Exit(1)` still skips disposal.

Acceptance:

1. The fixed `runMode` and its Manual/Development/Release branches are removed. Auto is the only mode.
2. Required steps check the returned exit code and throw a step exception. `Run` catches it, prints it and sets `Environment.ExitCode = 1`, so `ScriptContext` disposes. No `Environment.Exit`. Best-effort steps (Playwright `--with-deps`, dev-certs clean and trust) warn on non-zero.
3. A `SutHost : IAsyncDisposable` owns the server, run through Amuru `StreamCombinedAsync` with a cancellation token, plus both log writers. Disposal cancels, which kills the process tree, waits a bounded time, never throws, and is idempotent.
4. `WaitUntilReadyAsync` disposes each response, logs the status code or the exception message on every failed attempt, and stops early with a clear error if the server already exited.
5. No `System.Diagnostics.Process` is left in scripts/e2e*.
6. The `UseHttp` edit uses a whitespace-tolerant regex and fails when the setting or the file is missing.
7. The Playwright-missing message names `test-app-end-to-end-tests`, and the output dir comes from MSBuild `TargetPath`, so `net11.0` is no longer hard-coded.
8. Cleanups: `outputPath` is passed in, log paths come from `SutHost`, `UseHttp` is set once per child, the runsettings loop is gone, the dev-certs `--clean` result is checked, indentation is 2-space, and there are no async-without-await methods.
9. The E2E run is no worse than the before-run on this branch.

## Checklist

- [x] Dead modes removed or made real
- [x] Exit-code handling consistent with Amuru semantics (3.0.0-beta.1 returns codes; checked, no Environment.Exit)
- [x] SUT process owned by a disposable host; no throw from finally
- [x] WaitForSut disposes responses, logs failures, detects early exit
- [x] Amuru replaces Process
- [x] File split as above
- [x] E2E run still passes (or fails for the same pre-existing reasons as origin/dev)

## Notes

Found by a code review on 2026-11-04 of all files over 500 LOC. The E2E suite has a known failing baseline; compare before/after rather than requiring green.

## Results

Amuru 3.0.0-beta.1 returns exit codes instead of throwing (default validation `None`). So the checks stay, but each failure now throws `E2eStepFailedException`, which `Run` catches. `Environment.Exit` is gone. `Run` sets `Environment.ExitCode = 1` and returns, so `ScriptContext` disposes.

- `scripts/e2e.cs` is orchestration only. The paths and ports are constants. `RunStep` prints the step header and footer, and `RunTestsAgainstSut` owns the SUT lifetime. `runMode` and the Manual, Development and Release branches are removed.
- `scripts/e2e-sut-host.cs` (renamed from `e2e-sut.cs`) holds `SutHost : IAsyncDisposable`.
  - The server runs through Amuru `StreamCombinedAsync` on a background pump that writes to `sut_output.log` and `sut_error.log`.
  - Disposal cancels the token, so CliWrap kills the process tree. It then waits at most 15 s, closes the writers only after the pump has finished, never throws, and is idempotent.
  - `WaitUntilReadyAsync` disposes each response, logs the status code or exception on each failed attempt, and stops early if the server has already exited.
  - Logs are displayed after disposal, so they are complete and no writer holds the files.
- `scripts/e2e-build.cs`:
  - `EnsureSuccess` checks every required step.
  - The analyzer and source-generator builds share `BuildReleaseProject`.
  - `outputPath` is passed in.
  - The `UseHttp` edit uses a whitespace-tolerant regex and fails if the file or the setting is missing.
  - The Playwright output dir comes from `dotnet msbuild -getProperty:TargetPath`, so `net11.0` is no longer hard-coded.
  - The missing-script message names `test-app-end-to-end-tests`.
  - A failed Playwright `--with-deps` install still only warns.
- `scripts/e2e-tests.cs`: one runsettings and no loop. `UseHttp` is set once, on the `dotnet test` child, and `-e` is dropped. It returns `passed`.
- `scripts/e2e-dev-certs.cs`: reports the exit codes of both `--clean` and `--trust` (best effort).
- `scripts/global-usings.cs`: added `System.Text.RegularExpressions`, which TW0007 flagged on the runfile build.
- No `System.Diagnostics.Process` is left, indentation is 2-space throughout, and no method is async without an await.

Gates:

- Baseline on this branch before any edit, with `UseHttp=true dotnet run --file scripts/e2e.cs`: 12/12 passed, exit 0.
- After the change, with `UseHttp` unset: 12/12 passed, exit 0. This also proves the test host receives `UseHttp` from the child environment. The SUT was stopped and no `Test.App.Server` was left running.
- Failure path: with the `UseHttp` key removed from the client appsettings, the run exited 1 with `E2E run failed: ... has no "UseHttp": true|false setting to update.` (The file was restored afterwards.)
- `dotnet build scripts/e2e.cs` is clean.
- `ganda repo audit` passes, with 2 pre-existing advisory warnings that this task does not touch: `kanban-landed` for task 098, and `kebab-path-names` for library JS and targets files.
- `./bin/dev test` was not run. Only `scripts/` changed, and the e2e run itself rebuilt the analyzer, generator, SUT and tests.

### How to validate

Smoke: from the task worktree, run `env -u UseHttp dotnet run --file scripts/e2e.cs` (or `./bin/dev e2e`).

Expect:

- The steps run from Restore-Tools-And-Cleanup through Wait-For-Sut and Run-Tests.
- `SUT is ready at http://localhost:7011` appears after at most a few logged attempts.
- `Passed! - Failed: 0, Passed: 12`, then `SUT stopped (stopped by the e2e run).`
- Exit code 0, and `pgrep -f Test.App.Server` finds nothing afterwards.
- Removing `"UseHttp"` from `tests/test-app/test-app-client/wwwroot/appsettings.json` makes the run exit 1 at Update-ClientAppSettings with a named error.

## Session

- Created: claude code review session (2026-11-04)
- implement (claude, 2026-11-05): rewrote the e2e runfile parts for Amuru 3 exit codes and added SutHost. Baseline and after are both 12/12 green, and the failure path exits 1. Audit passes.
