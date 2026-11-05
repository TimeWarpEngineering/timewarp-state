# Round 1 — general
**Date:** 2026-11-05
**Scope reviewed:** branch `task/136-fix-and-restructure-scriptse2ecs-dead-modes-amuru` (HEAD `de06d27a`) vs merge-base `770fa6d7`, product files under `scripts/e2e*.cs` and `scripts/global-usings.cs`

## Summary

The runfile is orchestration plus four includes. Dead `runMode` branches are gone. Required Amuru steps throw `E2eStepFailedException`, `Run` catches that, sets `Environment.ExitCode`, and returns so `ScriptContext` disposes. `SutHost` owns the published server through `StreamCombinedAsync`, and disposal cancels that call. Risk is low. The behavior changes match the restated acceptance, and the stale "Amuru throws on non-zero" premise in the original requirements is corrected rather than misunderstood.

The goal restatement matches the original task. Original item "remove the dead modes or make the mode a real argument" is satisfied by removal. The split named in the original task was already on this branch from task 135 (`e2e-build.cs`, `e2e-sut.cs`, `e2e-tests.cs`, `e2e-dev-certs.cs`); this change keeps that split and renames the host file to `e2e-sut-host.cs`. No misread.

What I checked against the repo, not only the diff:

- `Directory.Packages.props` pins `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` at `3.0.0-beta.1`. The package XML says validation defaults to none: non-zero exits are results, and `WithZeroExitCodeValidation` is the opt-in that throws. `EnsureSuccess` is live. Best-effort `pwsh` install, `dev-certs --clean`, and `dev-certs --trust` warn on a non-zero code.
- CliWrap 3.10.5 (the Amuru dependency) kills with `entireProcessTree: true` when the forceful cancellation token fires. Amuru's no-timeout `StreamCombinedAsync` passes the caller token through as that forceful token. `SutHost.DisposeAsync` cancels, waits at most 15 seconds, catches timeout and other exceptions, and returns. A second call returns immediately.
- `WaitUntilReadyAsync` disposes each `HttpResponseMessage`, logs the status or the request exception, and throws if `Pump.IsCompleted` before the next attempt. There is no bare `catch`.
- The `UseHttp` edit is `"UseHttp"\s*:\s*(?:true|false)`. It throws if the file or the setting is missing. `tests/test-app/test-app-client/wwwroot/appsettings.json` has `"UseHttp": true`, which matches.
- No `System.Diagnostics.Process`, `Environment.Exit(`, `runMode`, or `net11.0` remains under `scripts/e2e*.cs`. Indentation in the touched scripts is 2-space. Every `async` method in those files awaits.
- `RunTests` returns `exitCode == 0`. `Run` treats `false` as failure and sets `Environment.ExitCode`. The old method returned `testsFailed`; the caller was updated with the polarity.
- `UseHttp` is set on the SUT child and on the `dotnet test` child, not on the parent. `tests/test-app-end-to-end-tests/configuration.cs` treats the env value `"true"` as http. The parent `SetEnvironmentVariable` and the `-e` argument are gone.
- Playwright's script path comes from `dotnet msbuild -getProperty:TargetPath` for the Debug build just produced. The missing-script exception names `test-app-end-to-end-tests`. The published exe name matches `AssemblyName` `Test.App.Server`.
- Nuru's generated route returns `NuruCallContext.CurrentExitCode`. For `dotnet run --file`, that context is unset, so the getter reads `Environment.ExitCode`. The intercepted `RunAsync` resets that to 0 before the handler, and the handler then sets 1 on failure. That is the exit code `tools/dev-cli/endpoints/e2e-command.cs` already checks. The recorded failure path (missing `UseHttp` key, exit 1) matches this.
- `master` is two "land kanban 136" commits ahead of this HEAD. Those commits do not touch `scripts/` or `e2e-command.cs`.

## Issues

None.
