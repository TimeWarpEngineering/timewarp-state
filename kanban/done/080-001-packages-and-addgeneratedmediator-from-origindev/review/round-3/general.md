# Round 3 — general
**Date:** 2026-09-03
**Scope reviewed:** post-disposition delta a6700c2f..HEAD excluding kanban/

## Summary

The CI delta is sound in the places that decide whether the pipeline is honest: I re-verified the NU1301
premise (a missing local folder source only fails restore on a cold package cache — reproduced
`error NU1301: The local source '.../artifacts/packages' doesn't exist.` with an isolated `NUGET_PACKAGES`,
and it restores fine when warm), verified that Amuru 1.0.0-beta.5 `RunAsync` throws `CommandExecutionException`
on non-zero exit for both `Shell.Builder` and `DotNet.Build`, and verified that a Nuru handler exception is
caught by `app.RunAsync` and yields process exit **1** — so `test.cs` and the pre-`try` steps of `e2e.cs` do
still fail CI even though their `if (exitCode != 0)` guards are unreachable. All five runfiles compile as
file-based apps (`dotnet build ./scripts/{clean,build,test,e2e,package}.cs`, only the pre-existing CS0219
on `analyzersDirectory`). The MSTest pin's blast radius is exactly one project: `tests/test-app-end-to-end-tests`
is the only csproj referencing MSTest, and `Microsoft.Playwright.MSTest` 1.55.0 declares `MSTest.* >= 2.2.7`,
so 3.11.1 satisfies it rather than under-shooting it. The workflow YAML is well-formed for the changes made:
all three jobs default to `shell: pwsh`, so the `New-Item` step is valid on both ubuntu and windows runners;
`docfx` 2.78.5 ships a `net10.0` asset with `rollForward: Major`, so replacing `dotnet-version: 8.x` with
`global-json-file` does not strand the docs job; and I confirmed `dotnet tool install` tolerates the missing
local source even on a cold cache, so the docs job's `Ensure local NuGet feed folder` sitting *after*
`Setup DocFX` is not a live ordering bug.

Findings are three suggestions and two nits — no correctness bug that would let a red build report green.
The main one is that the dev-certs hardening in this delta is unreachable: `e2e.cs` hardcodes `useHttp = true`
and never reads the `UseHttp` env var the workflow sets.

## Issues

### Issue 1 — Severity: suggestion
- File: scripts/e2e.cs:31
- Description: `var useHttp = true;` is a hardcoded local — nothing in `e2e.cs` reads the `UseHttp`
  environment variable (`grep -rn UseHttp .github scripts` shows the script only ever *sets* it, at lines
  330, 421 and 429). Two consequences for this delta: (a) the `else { await InstallLinuxDevCerts(); }` branch
  at lines 52–55 is unreachable, which makes the whole `InstallLinuxDevCerts` method — including the
  best-effort try/catch and the exit-4 handling added in 5ce37e6b — dead code that can never be exercised or
  regression-tested; (b) `.github/workflows/workflow.yml:100` (`env: UseHttp: "true"`) is decorative, so a
  future change to `UseHttp: "false"` would silently keep running the suite over http against a SUT whose
  `appsettings.json` was rewritten to `false`, with no dev-certs step and no warning. The log line
  "Skipping dev-certs trust: E2E runs over http (UseHttp=true)" reads as if the env var drove the decision.
- Suggestion: read the env var — `var useHttp = Environment.GetEnvironmentVariable("UseHttp") is not "false";`
  (or `bool.TryParse`, defaulting to true) — which makes the workflow's `env:` block load-bearing and revives
  the https path; or, if http-only is a deliberate permanent decision for 080-003, delete
  `InstallLinuxDevCerts` and the `UseHttp` env entry rather than leaving hardened-but-dead code behind.
- Status: open

### Issue 2 — Severity: suggestion
- File: scripts/test.cs:23
- Description: Every one of the ten `if (exitCode != 0) Environment.Exit(1);` guards introduced by the
  `ExecuteAsync` → `RunAsync` conversion is unreachable under the pinned `TimeWarp.Amuru` 1.0.0-beta.5. I
  confirmed empirically that `RunAsync` throws `CommandExecutionException` on a non-zero child exit for both
  `Shell.Builder(...)` and `DotNet.Build()`, so `exitCode` is always 0 when the assignment completes. CI is
  not wrong today — Nuru catches the handler exception and returns exit 1 (also verified) — but the operator
  sees `Error executing handler: Command execution failed because the underlying process (dotnet#NNNN)
  returned a non-zero exit code (1).` with no indication of *which* of the five suites failed, and the
  script's apparent control flow (fail fast, suite by suite) is fiction. `e2e.cs` got the try/catch treatment
  for exactly this reason in d2067e8b; `test.cs` did not.
- Suggestion: either wrap each step in a try/catch that names the suite before `Environment.Exit(1)`, or
  apply `WithValidation(CommandResultValidation.None)` (the hint Amuru itself prints) so the exit codes are
  returned and the existing guards become live. A small `RunStep(string name, Func<Task<int>>)` helper would
  collapse the ten near-identical blocks at the same time.
- Status: open

### Issue 3 — Severity: suggestion
- File: .github/workflows/workflow.yml:61
- Description: The `ci` job restores `~/.nuget/packages` via `actions/cache@v4` and then, two steps later,
  runs `Clean solution` → `scripts/clean.cs:27` → `DotNet.NuGet().Locals().Clear(NuGetCacheType.All)`, which
  deletes the global-packages folder that was just restored (plus the http cache). This is newly live
  behaviour: before 057b08a9 the clean step died on MSB4025 before reaching that line, so the cache was
  effectively intact. Net effect is that every CI run re-downloads the full package graph and then pays to
  re-upload it in the post-job cache save.
- Suggestion: on an ephemeral runner the local-cache clear buys nothing — either drop the `Cache NuGet
  packages` step as misleading, or give `clean.cs` a flag (or an env check such as `CI`) that skips
  `nuget locals --clear` so the cache restore is actually used.
- Status: open

### Issue 4 — Severity: nit
- File: scripts/package.cs:19
- Description: `package.cs` is the one runfile that did not get the `Directory.CreateDirectory("./artifacts/packages")`
  guard added to `clean.cs`, `build.cs`, `test.cs` and `e2e.cs`, even though it performs `dotnet build` and
  `dotnet pack` restores that hit the same NU1301. It is covered in CI by the release job's `Ensure local NuGet
  feed folder` step, so this is cosmetic there, but a clean-clone `dotnet run --file ./scripts/package.cs` on a
  cold cache still fails. Related asymmetry: `build.cs`'s own `clean` route (lines 106–110) deletes
  `./artifacts` and does not recreate `artifacts/packages`, whereas `clean.cs:60` does.
- Suggestion: add the same one-liner to `package.cs` and to `build.cs`'s `CleanSolution`, so all entry points
  leave the tree in a restorable state. (Note the guard cannot help a runfile's *own* `#:package` restore,
  which happens before any user code runs — it only protects the child `dotnet` invocations. Worth saying so
  in the shared comment, since task.md's Results claims it makes `dotnet run --file` work from a clean clone.)
- Status: open

### Issue 5 — Severity: nit
- File: scripts/e2e.cs:458
- Description: `KillSut` calls the newly added `sutProcess.WaitForExit()` — which is the right call, since the
  parameterless overload also waits for the async output readers to drain — only inside `if (!sutProcess.HasExited)`.
  When the SUT has already exited on its own (a crash, i.e. the case where the log matters most), that branch is
  skipped and `sutOutputWriter?.Dispose()` runs while `OutputDataReceived` callbacks may still be draining
  buffered lines. The `catch (ObjectDisposedException) { }` guards added at lines 348–349 mean this no longer
  aborts the process (the exit-134 failure is genuinely fixed), but it silently truncates the tail of
  `sut_output.log` / `sut_error.log`.
- Suggestion: hoist `sutProcess.WaitForExit();` out of the `if`, so the readers are drained on both paths
  before the writers are disposed.
- Status: open
