# Task 042: Migrate Mediator - Update Pre-Processors

## Description

- Convert `IRequestPreProcessor<TRequest>` implementations to `MessagePreProcessor<TMessage, TResponse>` abstract class
- Change from interface implementation to abstract class inheritance

## Requirements

- Change from implementing interface to extending abstract class
- Add second generic type parameter `TResponse`
- Rename method from `Process()` to `Handle()` (protected override)
- Change return type from `Task` to `ValueTask`

## Checklist

### Implementation
- [ ] Update `source/timewarp-state/features/state-initialization/state-initialization-pre-processor.cs`:
  - [ ] Change base from `IRequestPreProcessor<TRequest>` to `MessagePreProcessor<TRequest, TResponse>`
  - [ ] Add `TResponse` generic parameter
  - [ ] Rename `Process()` to `protected override ValueTask Handle()`
  - [ ] Change return from `Task` to `ValueTask`
- [ ] Update `tests/test-app/test-app-client/pipeline/notification-pre-processor/pre-pipeline-notification-request-pre-processor.cs`:
  - [ ] Apply same changes as above

## Notes

**Current pattern:**
```csharp
public class StateInitializationPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
  where TRequest : IAction
{
  public Task Process(TRequest request, CancellationToken cancellationToken)
  {
    // Pre-processing logic
    return Task.CompletedTask;
  }
}
```

**New pattern required:**
```csharp
public sealed class StateInitializationPreProcessor<TMessage, TResponse> : MessagePreProcessor<TMessage, TResponse>
  where TMessage : IAction
{
  // Constructor with dependencies...

  protected override ValueTask Handle(TMessage message, CancellationToken cancellationToken)
  {
    // Pre-processing logic
    return default;
  }
}
```

**Key changes:**
1. Interface `IRequestPreProcessor<T>` -> Abstract class `MessagePreProcessor<TMessage, TResponse>`
2. One generic param -> Two generic params
3. `Process()` -> `Handle()` (protected override)
4. `Task` -> `ValueTask`
5. `Task.CompletedTask` -> `default`

**Files to modify (2 total):**
1. `source/timewarp-state/features/state-initialization/state-initialization-pre-processor.cs`
2. `tests/test-app/test-app-client/pipeline/notification-pre-processor/pre-pipeline-notification-request-pre-processor.cs`

## Implementation Notes

