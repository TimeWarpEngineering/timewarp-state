# Task 044: Migrate Mediator - Update Notification Handlers

## Description

- Update all `INotificationHandler<T>` implementations to return `ValueTask` instead of `Task`

## Requirements

- Change return type from `Task` to `ValueTask`
- Change `Task.CompletedTask` returns to `default`

## Checklist

### Implementation
- [ ] Update `source/timewarp-state-plus/features/persistence/state-initialized-notification-handler.cs`:
  - [ ] Change return type from `Task` to `ValueTask`
  - [ ] Change `return Task.CompletedTask;` to `return default;`
- [ ] Update `tests/test-app/test-app-client/features/counter/notification/increment-count-notification-handler.cs`:
  - [ ] Apply same changes
- [ ] Update `tests/test-app/test-app-client/features/counter/notification/pre-increment-count-notification-handler.cs`:
  - [ ] Apply same changes
- [ ] Update `tests/test-app/test-app-client/features/application/notification/application-state.exception-notification-handler.cs`:
  - [ ] Apply same changes
- [ ] Search for any other `INotificationHandler` implementations and update them

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

