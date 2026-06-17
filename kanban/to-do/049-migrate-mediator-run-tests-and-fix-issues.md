# Task 049: Migrate Mediator - Run Tests and Fix Issues

## Description

- Run all tests after migration tasks are complete
- Fix any remaining compilation errors or test failures
- Verify the application works correctly

## Requirements

- All unit tests pass
- All integration tests pass
- All E2E tests pass
- Test application runs successfully

## Checklist

### Progress (2026-06-16) — libraries + their unit tests GREEN; consumers remain

DONE:
- [x] `source/timewarp-state` builds: 0 errors
- [x] `source/timewarp-state-plus` builds: 0 errors
- [x] **`tests/timewarp-state-tests`: 16 passed, 1 skipped — GREEN**
- [x] **`tests/timewarp-state-plus-tests`: 11 passed, 1 skipped — GREEN** (includes GoBack regression tests; task 059 fixed during this work)
- [x] Architecture policies: Action/Handler convention changed `BeInternal` → `BePublic` (Mediator requires public handlers cross-assembly), so `FollowActionPolicy`/`FollowActionHandlerPolicy` pass again
- [x] Fixed stale `using TimeWarp.Mediator;` → `using Mediator;` in plus-tests
- [x] Found `MediatorOptions.GenerateTypesAsInternal = true` keeps a consumer's generated Mediator internal, so its own `internal` actions don't trip CS0051 (avoids making every consumer action public)

TEST APP — BUILDS, RUNS, and the Mediator pipeline is VALIDATED END-TO-END:
- [x] **test-app-client builds: 0 errors** (the Mediator generator produced valid registration for every real handler/action/notification)
- [x] **test-app-server builds: 0 errors; the app RUNS** (HTTP 200, serves the Blazor WASM client, clean startup with no DI/Mediator errors)
- [x] **Counter pipeline validated in a real browser** (Playwright/chromium against the running app, `/CounterPage`): clicking the counters dispatched `IncrementCountActionSet.Action` through the migrated pipeline → count went **3 → 8 → 13**, zero browser console errors. This exercises `AddMediator` registration, source-generated handler wiring, `ValueTask<Unit>` handlers, the pipeline behaviors, and render-subscription re-rendering.

test-app fixes applied to get there:
- [x] open-generic notifications → made `PrePipelineNotification` / `PostPipelineNotification` non-generic (carry `object Request`/`Response`); their 2 handlers now `INotificationHandler<...>` non-generic with a request-type filter to preserve behavior
- [x] `[PersistentState]` collision → global alias added to test-app `global-usings.cs`
- [x] `AddMediator` → added `options.GenerateTypesAsInternal = true`
- [x] **Removed `Mediator.SourceGenerator` from `timewarp-state.csproj`** — the library generated a public `AddMediator` into its dll that collided with the consumer's (CS0121); the library now references only `Mediator.Abstractions`. (Core still builds 0 errors.)
- [x] Fixed task-045 subagent error: non-async `ValueTask<Unit>` handlers had `return Unit.Value;` (async-only) → `return new ValueTask<Unit>(Unit.Value);` in 5 test-app + 5 sample handlers
- [x] `base-component.cs` `Send` helper: `Mediator.Send(request)` now returns `ValueTask<Unit>` → added `.AsTask()`

REMAINING:
- [ ] **6 samples** — same pattern as test-app: reference `Mediator.SourceGenerator`; add `AddMediator(o => { o.ServiceLifetime = Scoped; o.GenerateTypesAsInternal = true; o.Assemblies = [...] })`; the sync-handler return fix is already applied. (Mechanical, parallelizable.)
- [ ] `tests/timewarp-state-analyzer-tests` — references `TimeWarp.Mediator.Contracts.dll` (×4 string paths) → task **048** territory.
- [ ] `client-integration-tests`, `test-app-architecture-tests` — now unblocked (test-app builds); run + fix.
- [ ] `test-app-end-to-end-tests` — project builds, but the MSTest/Playwright runner reports "No test is available" (test-discovery/adapter issue under .NET 10, unrelated to the migration). Browser validation was done directly via Playwright instead; revisit the runner setup.

### Review
- [x] Behavioral change documented: pre/post-pipeline notifications are non-generic now (fire for all actions, handlers filter by type); Action/Handler types are `public` (was `internal`); architecture policy updated to match.
- [ ] Run remaining test projects (integration, architecture) and the 6 samples.

## Notes

**Common issues to watch for:**

1. **Missing Unit.Value returns** - Handler methods that don't return `Unit.Value`
2. **ValueTask handling** - Improper awaiting or caching of ValueTask
3. **Pipeline order** - Behaviors executing in wrong order
4. **Async state machine differences** - ValueTask vs Task behavior

**Commands:**
```bash
# Build everything
dotnet build

# Run all tests
dotnet test

# Run specific test project
dotnet test tests/timewarp-state-tests/
```

## Implementation Notes

