# Task 045: Migrate Mediator - Update Request Handlers

## Description

- Update all `IRequestHandler` implementations to return `ValueTask<Unit>` or `ValueTask<TResponse>` instead of `Task`

## Requirements

- Change return type from `Task` to `ValueTask<Unit>` for handlers without response
- Change return type from `Task<TResponse>` to `ValueTask<TResponse>` for handlers with response
- Add `return Unit.Value;` for void-equivalent handlers

## Checklist

### Implementation
- [ ] Update `source/timewarp-state/features/redux-dev-tools/requests/start/start-handler.cs`:
  - [ ] Change return type to `ValueTask<Unit>`
  - [ ] Add `return Unit.Value;` at end
- [ ] Update `source/timewarp-state/features/redux-dev-tools/requests/commit/commit-handler.cs`:
  - [ ] Apply same changes
- [ ] Update ActionSet handlers in `source/timewarp-state-plus/`:
  - [ ] `features/routing/route-state/route-state.change-route.cs`
  - [ ] `features/routing/route-state/route-state.go-back.cs`
  - [ ] `features/routing/route-state/route-state.push-route-info.cs`
  - [ ] `features/theme/theme-state/theme-state.update.cs`
  - [ ] `features/timers/timer-state/timer-state.add-timer.cs`
  - [ ] `features/timers/timer-state/timer-state.remove-timer.cs`
  - [ ] `features/timers/timer-state/timer-state.update-timer.cs`
  - [ ] `features/timers/timer-state/timer-state.reset-timers-on-activity.cs`
  - [ ] `features/action-tracking/action-tracking-state/action-tracking-state.start-processing.cs`
  - [ ] `features/action-tracking/action-tracking-state/action-tracking-state.complete-processing.cs`
- [ ] Search for any other `IRequestHandler` implementations and update them

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

