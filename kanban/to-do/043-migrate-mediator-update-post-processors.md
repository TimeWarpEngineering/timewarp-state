# Task 043: Migrate Mediator - Update Post-Processors

## Description

- Convert `IRequestPostProcessor<TRequest, TResponse>` implementations to `MessagePostProcessor<TMessage, TResponse>` abstract class
- Change from interface implementation to abstract class inheritance

## Requirements

- Change from implementing interface to extending abstract class
- Rename method from `Process()` to `Handle()` (protected override)
- Change return type from `Task` to `ValueTask`

## Checklist

### Implementation
- [ ] Update `source/timewarp-state/features/render-subscriptions/render-subscriptions-post-processor.cs`:
  - [ ] Change base from `IRequestPostProcessor<TRequest, TResponse>` to `MessagePostProcessor<TRequest, TResponse>`
  - [ ] Rename `Process()` to `protected override ValueTask Handle()`
  - [ ] Change return from `Task` to `ValueTask`
- [ ] Update `source/timewarp-state-plus/features/persistence/pipeline/persistent-state-post-processor.cs`:
  - [ ] Apply same changes as above
- [ ] Update `source/timewarp-state-plus/features/timers/multi-timer-post-processor.cs`:
  - [ ] Apply same changes as above
- [ ] Update `tests/test-app/test-app-client/pipeline/notification-post-processor/post-pipeline-notification-request-post-processor.cs`:
  - [ ] Apply same changes as above

## Notes

**Current pattern:**
```csharp
internal class RenderSubscriptionsPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
  where TRequest : IAction
{
  public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
  {
    // Post-processing logic
    return Task.CompletedTask;
  }
}
```

**New pattern required:**
```csharp
internal sealed class RenderSubscriptionsPostProcessor<TMessage, TResponse> : MessagePostProcessor<TMessage, TResponse>
  where TMessage : IAction
{
  // Constructor with dependencies...

  protected override ValueTask Handle(TMessage message, TResponse response, CancellationToken cancellationToken)
  {
    // Post-processing logic
    return default;
  }
}
```

**Key changes:**
1. Interface `IRequestPostProcessor<T,R>` -> Abstract class `MessagePostProcessor<TMessage, TResponse>`
2. `Process()` -> `Handle()` (protected override)
3. `Task` -> `ValueTask`
4. `Task.CompletedTask` -> `default`

**Files to modify (4 total):**
1. `source/timewarp-state/features/render-subscriptions/render-subscriptions-post-processor.cs`
2. `source/timewarp-state-plus/features/persistence/pipeline/persistent-state-post-processor.cs`
3. `source/timewarp-state-plus/features/timers/multi-timer-post-processor.cs`
4. `tests/test-app/test-app-client/pipeline/notification-post-processor/post-pipeline-notification-request-post-processor.cs`

## Implementation Notes

