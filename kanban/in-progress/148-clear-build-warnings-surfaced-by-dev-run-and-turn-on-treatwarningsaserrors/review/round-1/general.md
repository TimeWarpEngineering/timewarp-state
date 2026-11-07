# Round 1 — general
**Date:** 2026-11-07
**Scope reviewed:** branch `task/148-clear-build-warnings-surfaced-by-dev-run-and-turn` vs merge-base `ffbd2efd` (product commit `71624be0`), checked against `## Goal (implementer restatement)` and the original requirements.

## Summary

The change removes the listed warning sites at the source and sets `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` with no new repo-wide `NoWarn` and no `WarningsNotAsErrors`. `JsonRequestHandler.InitAsync` now awaits `InvokeVoidAsync` inside `catch (JSDisconnectedException)`, keeps the single `DotNetObjectReference` guard from task 063, and is still only called from `TimeWarpJavaScriptInterop` on `firstRender`. RS0030 is cleared by deleting `Console` (handlers log through `ILogger<T>`, MSTest uses `TestContext.WriteLine`, static debug lines are dropped). That matches the requirement to fix the banned call at the source. `BannedSymbols.txt` names `ITerminal` as the CLI migration hint; these call sites are the test app and E2E suite, not the dev CLI. The goal restatement does not contradict the original requirements, so there is no misread.

Scoped exceptions are the ones the task allows. Three test doubles that implement `IJSRuntime` carry a class-scoped `BL0016` pragma with a one-line reason, because the interface method is `InvokeAsync`. `[artifacts/nuget-packages/**.cs]` sets `CS8669` to `none` where `generated_code` already marks package sources this repo does not own; that is narrower than a repo-wide `WarningsNotAsErrors`. The analyzer-test `System.Security.Cryptography.Pkcs` pin is a direct reference to `11.0.0-rc.3.26522.148`, which satisfies NuGet.Packaging 7.0.3's `>= 9.0.6` dependency, rather than a suppressed advisory. `KeepTrackedTypeScriptOutputsOnClean` deletes `tsc-emitted.txt` before `CleanTypeScriptOutputs` (Microsoft.TypeScript.MSBuild 7.0.1 reads that list and deletes the paths). The next `RunTscCompile` rewrites the outputs.

Re-checked here: `dotnet build` of `test-app-server` (0 warnings), `--no-incremental` builds of `timewarp-state`, `timewarp-state-telemetry`, `timewarp-state-policies`, and `timewarp-state-analyzer-tests` (0 warnings), `sample-00-auto` (0 warnings), and `timewarp-state-tests` (153 passed, 1 skipped). `git status` shows no deleted `source/timewarp-state-blazor/wwwroot` files. This pass did not re-run `dev test`, `dev e2e`, or `ganda repo audit`.

## Issues

None.
