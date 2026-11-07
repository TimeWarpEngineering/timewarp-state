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

- [ ] NU1510: two redundant PackageReferences removed
- [ ] ASPDEPR011: `UseWebAssemblyDebugging()` and inspectUri removed; app still launches via `dev run`
- [ ] TW0007: all file-level usings moved to the right `global-usings.cs`
- [ ] RS0030: `Console` replaced with `ITerminal` (or justified pragma)
- [ ] BL0010/BL0016: `json-request-handler.cs:173` uses `InvokeVoidAsync` in try/catch
- [ ] CS8600/CS8602: `clone-provider-tests.cs` nullable-clean
- [ ] `TreatWarningsAsErrors` true in `Directory.Build.props`
- [ ] `dev build`, `dev test`, and full no-incremental test-app build: zero warnings
- [ ] `ganda repo audit --fix` run and its changes committed
- [ ] wwwroot tracked files intact in `git status`
- [ ] `## Results` filled in with the before/after warning count

## Session

- Created: ffba6066-c22c-42a7-95c8-de75506a6687 (2026-11-07)
- Context: Steve ran `dev run` after task 147 merged and saw the warning flood; asked for explanation and a task (2026-11-07)

## Notes

Warning tally was taken on master at 27e2eed8 (post-147). Counts double in places because test-app-client is
compiled once directly and once as a reference of test-app-server.
