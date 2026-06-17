# Task 045: Migrate Mediator - Update Request Handlers

## Description

- Update all `IRequestHandler` implementations to return `ValueTask<Unit>` or `ValueTask<TResponse>` instead of `Task`

## Requirements

- Change return type from `Task` to `ValueTask<Unit>` for handlers without response
- Change return type from `Task<TResponse>` to `ValueTask<TResponse>` for handlers with response
- Add `return Unit.Value;` for void-equivalent handlers

## Checklist

## ⚠️ Scope was much larger than this checklist

The checklist listed 12 files. A full sweep of `source/`, `tests/`, AND `samples/` found **30 handler files + 2 dispatch-site callers**. The note "ActionHandler<T> subclasses update automatically when the base changes (040)" is WRONG — every `override Task Handle` breaks when the base became `abstract ValueTask<Unit> Handle`, so all 27 subclasses needed editing.

Verified API (Mediator.Abstractions 3.0.2): `IRequestHandler<TRequest>.Handle` returns `ValueTask<Unit>`; `Unit` is `Mediator.Unit` (globally imported → `Unit.Value`). Transformations: `Task`→`ValueTask<Unit>`, `Task.CompletedTask`→`Unit.Value` (or `new ValueTask<Unit>(Unit.Value)` for expression bodies), async handlers get a trailing `return Unit.Value;`.

### Implementation
- [x] Direct `IRequestHandler<T>` impls (3): `start-handler.cs`, `commit-handler.cs` (core), and `tests/.../application-state.reset-store.cs` (was not in the original list)
- [x] All 10 `timewarp-state-plus` ActionSet handlers (routing ×3, theme, timers ×4, action-tracking ×2)
- [x] All 14 `test-app` ActionHandler subclasses (counter, blue, purple, clone-test, event-stream, weather ×2, application ×3, + 2 analyzer-test fixtures under `#if ANALYZER_TEST` that were on an intermediate `Task<Unit>`/`Unit.Task` shape)
- [x] All 7 `samples` handlers (00 server/wasm/auto, 01, 02 ×2, 03)
- [x] `tests/.../features/base/base-action-handler.cs` correctly NOT edited (adds properties only, no `Handle` override)
- [x] Search for any other `IRequestHandler` implementations — done across source/tests/samples; zero `Task`-returning handlers remain (`grep "override Task Handle"` → none)

### Additional dispatch-site fixes (callers of ISender/IPublisher; beyond "handlers" but required to build core)
- [x] `source/timewarp-state/store/store.cs:142` — `Publisher.Publish(...)` now returns `ValueTask`; added `.AsTask()` before `.ContinueWith(...)` (the result is stored as a `Task` in `StateInitializationTasks`)
- [x] `source/timewarp-state/features/javascript-interop/json-request-handler.cs:73` — `Sender.Send(instance)` now returns `ValueTask<object?>`; added `.AsTask()` (the `[JSInvokable]` method returns `Task`, correct for Blazor JS interop)

### Verification
- [x] Core project builds with NO request-handler errors; all remaining core errors are service registration (task 046) only
- [ ] `-plus` / test-app / samples handlers compile — deferred to 049 (build through the core project, blocked on 046). NOTE: more ValueTask-vs-Task dispatch-site fallout (like the 2 above) may surface in `-plus`/test-app/components once 046 lets those projects build — handle in 049.

## Notes

**Current pattern (void handler):**
```csharp
internal class StartHandler : IRequestHandler<StartRequest>
{
  public Task Handle(StartRequest request, CancellationToken cancellationToken)
  {
    // Handler logic
    return Task.CompletedTask;
  }
}
```

**New pattern required:**
```csharp
internal class StartHandler : IRequestHandler<StartRequest>
{
  public ValueTask<Unit> Handle(StartRequest request, CancellationToken cancellationToken)
  {
    // Handler logic
    return new ValueTask<Unit>(Unit.Value);
  }
}
```

**For async handlers:**
```csharp
// Current
public async Task Handle(...) 
{ 
  await SomeOperation();
}

// New
public async ValueTask<Unit> Handle(...) 
{ 
  await SomeOperation();
  return Unit.Value;
}
```

**Note:** Handlers that extend `ActionHandler<T>` will be updated automatically when the base class is updated (Task 040).

## Implementation Notes

