# Task 049: Migrate Mediator - Run Tests and Fix Issues

## Description

**Archived (080-003).** Do not execute this task as written.

049 was the martinothamar/Mediator destination soak (run tests after 038–048). That path is not the destination. TimeWarp.State consumes **TimeWarp.Mediator 14.0.0-beta.1** (`AddGeneratedMediator`, named `ClientPipeline` / `ServerPipeline`).

Tests and soak live on **080-003**. A later State NuGet release is out of scope for 080 and is not this id.

## Disposition

- Archived by 080-003 so nobody ships a martinothamar State from this checklist.
- Historical progress notes (libraries green, test-app pipeline, remaining samples → 076) stay below for archaeology.

## Requirements (obsolete — do not execute)

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

## Notes

Superseded by 080 / 080-003. Do not run the commands below as a martinothamar release gate.
