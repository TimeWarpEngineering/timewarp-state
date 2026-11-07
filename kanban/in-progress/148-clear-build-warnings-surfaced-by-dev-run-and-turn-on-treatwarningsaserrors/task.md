# Clear build warnings surfaced by dev run and turn on TreatWarningsAsErrors

## Description

`dev run` (task 147) does a full `dotnet run` of `tests/test-app/test-app-server`, which compiles the
whole dependency graph and prints every warning in it. Incremental `dev build` hides most of these, and
`TreatWarningsAsErrors` is `false` in `Directory.Build.props`, so nothing has forced a cleanup. A
`dotnet build --no-incremental tests/test-app/test-app-server` emits ~110 warning lines, ~25 distinct sites:

| Code | Where | Cause |
|---|---|---|
| CS8600 / CS8602 (42 lines) | `tests/test-app/test-app-client/tests/clone-provider-tests.cs` | AnyClone port predates nullable; null literals to non-nullable, maybe-null dereferences |
| TW0007 (~17 sites) | test-app-server (`program.cs`, weather-forecast request/handler), test-app-client (`base-component.cs`, `custom-collection-object.cs`), timewarp-state-plus (timer-state files, `service-collection-extensions.cs`, `assembly-extensions.cs`), timewarp-state (`action-catalog-arguments.cs`) | file-level `using` must move to each project's `global-usings.cs` |
| RS0030 (3 sites) | test-app-client `application-state.five-second-task.cs`, `application-state.two-second-task.cs`; test-app-server `get-weather-forecasts-handler.cs` | `Console` banned by `BannedSymbols.txt`; inject `ITerminal` |
| NU1510 (2 projects) | `tests/test-app/test-app-contracts/test-app-contracts.csproj` (System.Net.Http.Json), `source/timewarp-state/timewarp-state.csproj` (Microsoft.Extensions.Logging.Abstractions) | PackageReference already provided by the SDK; remove |
| BL0010 / BL0016 (1 site) | `source/timewarp-state-blazor/features/javascript-interop/json-request-handler.cs:173` | `InvokeAsync<object>` should be `InvokeVoidAsync`, wrapped in try/catch (JSDisconnectedException) |
| ASPDEPR011 (1 site) | `tests/test-app/test-app-server/program.cs:49` | `UseWebAssemblyDebugging()` obsolete in .NET 10; remove it and any `inspectUri` in launchSettings.json |

## Goal (implementer restatement)

Make the full test-app dependency graph build with zero warnings, then lock that in by turning
`TreatWarningsAsErrors` on repo-wide.

Acceptance criteria:
- Every listed warning (NU1510, ASPDEPR011, TW0007, RS0030, BL0010/BL0016, CS8600/CS8602) is fixed at the
  source; no blanket `NoWarn`. Per-site pragmas only for intentional behavior, with a reason.
- `json-request-handler.cs` (shipped) uses `InvokeVoidAsync` guarded by try/catch and still tolerates JS disconnect.
- `Directory.Build.props` has `TreatWarningsAsErrors=true`; `dev build`, `dev test`, and
  `dotnet build --no-incremental tests/test-app/test-app-server` pass with zero warnings (or an explicit,
  commented `WarningsNotAsErrors` naming a follow-up).
- `ganda repo audit` is clean after `--fix`, changes committed; tracked wwwroot js/types files intact.
- `## Results` records before/after warning counts and a How to validate section.

## Requirements

- Fix every warning above at the source. Do not blanket-`NoWarn` a code. A per-site `#pragma warning disable`
  with a one-line reason is acceptable only where the behavior is intentional (e.g. the test app deliberately
  writing to the console for E2E).
- For `clone-provider-tests.cs`, prefer `ShouldNotBeNull()` / nullable locals over `!`. `#nullable disable` at
  file top is acceptable as a deliberate exception for the ported file if annotating is disproportionate; say so
  in Results.
- `json-request-handler.cs` is shipped library code: fix it properly, keep the existing behavior on JS disconnect.
- After the sites are clean, set `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in `Directory.Build.props`
  and confirm `dev build`, `dev test`, and `dotnet build --no-incremental tests/test-app/test-app-server` all pass
  with zero warnings. If a warning category cannot be made green in this task, keep TreatWarningsAsErrors on and
  add an explicit `<WarningsNotAsErrors>` entry with a comment naming the follow-up, rather than leaving it off.
- Run `ganda repo audit --fix` in this worktree and commit whatever it changes (audit fixes are policy).
- Gotcha: `--no-incremental` deletes tracked `source/timewarp-state-blazor/wwwroot/js|types` files. Rebuild
  `timewarp-state-blazor` or `git checkout -- source/timewarp-state-blazor/wwwroot` and check `git status` is
  clean of those before committing.

## Checklist

- [x] NU1510: two redundant PackageReferences removed
- [x] ASPDEPR011: `UseWebAssemblyDebugging()` and inspectUri removed; app still launches via `dev run`
- [x] TW0007: all file-level usings moved to the right `global-usings.cs`
- [x] RS0030: `Console` replaced with `ITerminal` (or justified pragma)
- [x] BL0010/BL0016: `json-request-handler.cs:173` uses `InvokeVoidAsync` in try/catch
- [x] CS8600/CS8602: `clone-provider-tests.cs` nullable-clean
- [x] `TreatWarningsAsErrors` true in `Directory.Build.props`
- [x] `dev build`, `dev test`, and full no-incremental test-app build: zero warnings
- [x] `ganda repo audit --fix` run and its changes committed
- [x] wwwroot tracked files intact in `git status`
- [x] `## Results` filled in with the before/after warning count

## Session

- Created: ffba6066-c22c-42a7-95c8-de75506a6687 (2026-11-07)
- Context: Steve ran `dev run` after task 147 merged and saw the warning flood; asked for explanation and a task (2026-11-07)
- Implement (ganda task work, 2026-11-07): fixed every listed site, then the further sites TreatWarningsAsErrors exposed
  in `dev build` / `verify-samples` (projects outside the test-app graph). Audit: `ganda repo audit --fix` passes and
  changed nothing; the 2 advisory warnings (kanban-landed claim 098 with no worktree, kebab-path names for Blazor
  `*.lib.module.*` / DevTools `.targets` files whose names the framework requires) are pre-existing and not this task's.

## Notes

Warning tally was taken on master at 27e2eed8 (post-147). Counts double in places because test-app-client is
compiled once directly and once as a reference of test-app-server.

## Results

### Warning count

| Build | Before (master 27e2eed8 + 148 kanban) | After |
|---|---|---|
| `dotnet build --no-incremental tests/test-app/test-app-server` | 53 distinct warnings (~110 raw lines) | 0 warnings, 0 errors (3 back-to-back runs) |
| `./bin/dev build` (TreatWarningsAsErrors on) | 81 + 24 errors over two passes (projects outside the test-app graph) | 0 warnings, 0 errors |
| `./bin/dev verify-samples` | 1 error (ASPDEPR011 in sample-00-auto) | 13 builds, 0 errors |

`./bin/dev test`: analyzer 90, source generator 174, state 153 (+1 skipped), plus 40 (+1 skipped), telemetry 21,
devtools 124, devtools isolation 4, client integration 67 (+1 skipped), architecture 7 — all pass.
`dev run --http`: home page 200 (title "TimeWarp.State Test App"), `api/weather?days=2` 200 with 2 forecasts.
`TreatWarningsAsErrors` is `true` with no `WarningsNotAsErrors` entries.

### What changed, by code

- **NU1510**: removed `Microsoft.Extensions.Logging.Abstractions` from timewarp-state **and timewarp-state-telemetry**
  (same warning, outside the test-app graph) and `System.Net.Http.Json` from test-app-contracts; dropped both now-unused
  `PackageVersion` entries.
- **ASPDEPR011**: removed `UseWebAssemblyDebugging()` and `inspectUri` from test-app-server and from
  `samples/00-state-action-handler/auto/sample-00-auto` (verify-samples failed on the same call). The standalone WASM
  samples still have `inspectUri` in launchSettings; they raise no warning and are left alone.
- **TW0007**: file-level usings moved to each project's `global-usings.cs` (timewarp-state, timewarp-state-plus,
  timewarp-state-policies, test-app-client, test-app-server, client-integration-tests, timewarp-state-tests,
  timewarp-state-plus-tests, e2e tests). Two binding traps: in timewarp-state-plus `System.Timers` would make `Timer`
  ambiguous with `System.Threading.Timer`, so it is a global alias `Timer = System.Timers.Timer`; in test-app-server
  the bare `App` binds to the `Test.App` namespace, so `MapRazorComponents<Test.App.Components.App>()` is qualified.
  The two client-integration test files in `namespace CounterState` became `CounterState_` (matching `BlueState_` etc.),
  because a namespace named like the type shadows the global `Counter` using.
- **RS0030**: test-app handlers (five/two-second tasks, weather handler) log through injected `ILogger<T>`. These are
  Information level, so the default `Warning` config in test-app-server appsettings now filters them. E2E tests write
  through MSTest `TestContext.WriteLine`. Static helpers (`Configuration`, `PageUtilities`) and the integration
  `BaseTest` drop their debug-only `Console` lines. No pragmas.
- **BL0010/BL0016** (shipped): `JsonRequestHandler.InitAsync` is now `async ValueTask` and calls `InvokeVoidAsync` inside
  `try/catch (JSDisconnectedException)`, logged at Debug, matching how `DisposeCore` treats disconnect. **Public API
  change**: the return type went from `ValueTask<object>` to `ValueTask`. The only caller (`TimeWarpJavaScriptInterop.razor`)
  just awaits it. The package is pre-release (12.0.0-beta.17). BL0016 also flagged three test doubles that *implement*
  `IJSRuntime` (any call inside them counts); each has a class-scoped `#pragma warning disable BL0016` with a one-line reason.
- **CS8600/CS8602**: the generated `Clone()` returns `T?`. clone-provider-tests, clone-graph-tests and three
  client-integration clone tests use `T? clone = ...Clone(); clone.ShouldNotBeNull();`. No `#nullable disable`, no `!`.
- **CS8669** (newly fatal): `.editorconfig` marks `artifacts/nuget-packages/**.cs` as `generated_code`, which turns
  nullable annotations in TimeWarp.Nuru.DevCli content files into CS8669. That `[artifacts/nuget-packages/**.cs]`
  section now has `CS8669 = none`. This is path-scoped to package sources we cannot edit; it is not a repo-wide NoWarn.
- **NU1902/NU1903** (newly fatal): timewarp-state-analyzer-tests got `System.Security.Cryptography.Pkcs` 9.0.6
  transitively from NuGet.Packaging 7.0.3, which has known advisories. It now has a direct reference pinned to
  11.0.0-rc.3.26522.148.
- **`--no-incremental` gotcha fixed at the root**: Microsoft.TypeScript.MSBuild's `CleanTypeScriptOutputs` deleted the
  tracked `wwwroot/js|types` files during Rebuild. Every second no-incremental build then failed with "No file exists
  for the asset". A `KeepTrackedTypeScriptOutputsOnClean` target in timewarp-state-blazor.csproj drops `tsc-emitted.txt`
  before that clean, so the tracked files survive and tsc still rewrites them on every build.

### Follow-ups / risks

- `dev e2e` was not run. The E2E edits are `Console` → `TestContext.WriteLine` and log-line removals only, and they
  compile in `dev build`. The suite's known baseline is 11/13 failing on dev and CI runs it continue-on-error.
- `InitAsync` return type change: mention it in the next beta release notes.

### How to validate

Smoke:
```bash
dotnet build --no-incremental tests/test-app/test-app-server   # run it twice
git status --short source/timewarp-state-blazor/wwwroot
./bin/dev build && ./bin/dev test && ./bin/dev verify-samples
grep -n TreatWarningsAsErrors Directory.Build.props
./bin/dev run --http   # then open http://localhost:5058 and http://localhost:5058/api/weather?days=2
```

Expect:
- Both no-incremental builds end with `0 Warning(s)` / `0 Error(s)`. `git status` shows no deleted wwwroot js/types files.
- `dev build` succeeds with 0 warnings, all `dev test` suites pass, and verify-samples prints "Samples verified successfully!".
- `Directory.Build.props` shows `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
- The test app serves the home page (title "TimeWarp.State Test App") and the weather API returns JSON forecasts.
