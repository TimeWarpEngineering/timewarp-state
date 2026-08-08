# Task 044: Migrate Mediator - Update Notification Handlers

## Description

- Update all `INotificationHandler<T>` implementations to return `ValueTask` instead of `Task`

## Requirements

- Change return type from `Task` to `ValueTask`
- Change `Task.CompletedTask` returns to `default`

## Checklist

### Implementation
- [x] Update `source/timewarp-state-plus/features/persistence/state-initialized-notification-handler.cs`:
  - [x] Change return type from `Task` to `ValueTask` (`async ValueTask`; awaits, no `Task.CompletedTask`)
  - [x] Change `return Task.CompletedTask;` to `return default;`
- [x] Update `tests/test-app/test-app-client/features/counter/notification/increment-count-notification-handler.cs`:
  - [x] Apply same changes
- [x] Update `tests/test-app/test-app-client/features/counter/notification/pre-increment-count-notification-handler.cs`:
  - [x] Apply same changes
- [x] Update `tests/test-app/test-app-client/features/application/notification/application-state.exception-notification-handler.cs`:
  - [x] Apply same changes
- [x] Search for any other `INotificationHandler` implementations and update them — swept `source/`, `tests/`, AND `samples/`; exactly these 4 exist, none in core or samples

### Verification
- [x] `INotificationHandler.Handle` return type confirmed `ValueTask` by reflecting Mediator.Abstractions 3.0.2
- [x] No `Task.CompletedTask` / `Task Handle` remains in the 4 handlers
- [x] Core project unchanged (only request-handler 045 + registration 046 errors remain)
- [ ] `-plus` handler + 3 test-app handlers compile — deferred to task 049 (build through the core project, which needs 045/046)

## Notes

**Current pattern:**
```csharp
public class StateInitializedNotificationHandler : INotificationHandler<StateInitializedNotification>
{
  public Task Handle(StateInitializedNotification notification, CancellationToken cancellationToken)
  {
    // Handler logic
    return Task.CompletedTask;
  }
}
```

**New pattern required:**
```csharp
public class StateInitializedNotificationHandler : INotificationHandler<StateInitializedNotification>
{
  public ValueTask Handle(StateInitializedNotification notification, CancellationToken cancellationToken)
  {
    // Handler logic
    return default;
  }
}
```

**For async handlers:**
```csharp
// Current
public async Task Handle(...) { ... }

// New
public async ValueTask Handle(...) { ... }
```

**Files to modify (4+ total):**
1. `source/timewarp-state-plus/features/persistence/state-initialized-notification-handler.cs`
2. `tests/test-app/test-app-client/features/counter/notification/increment-count-notification-handler.cs`
3. `tests/test-app/test-app-client/features/counter/notification/pre-increment-count-notification-handler.cs`
4. `tests/test-app/test-app-client/features/application/notification/application-state.exception-notification-handler.cs`

Use `grep -r "INotificationHandler" source/ tests/` to find any additional handlers.

## Implementation Notes

