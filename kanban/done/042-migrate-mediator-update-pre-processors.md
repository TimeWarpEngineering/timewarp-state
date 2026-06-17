# Task 042: Migrate Mediator - Update Pre-Processors

## Description

- Convert `IRequestPreProcessor<TRequest>` implementations to `MessagePreProcessor<TMessage, TResponse>` abstract class
- Change from interface implementation to abstract class inheritance

## Requirements

- Change from implementing interface to extending abstract class
- Add second generic type parameter `TResponse`
- Rename method from `Process()` to `Handle()` (protected override)
- Change return type from `Task` to `ValueTask`

## Verified API (Mediator.Abstractions 3.0.2, by reflection)

`MessagePreProcessor<TMessage, TResponse>` is an `abstract class` implementing `IPipelineBehavior<TMessage, TResponse>`. Override:

```csharp
protected override ValueTask Handle(TMessage message, CancellationToken cancellationToken)
```

(the public `Handle(message, next, cancellationToken)` is sealed by the base, which calls this override then `next`). `IRequestPreProcessor<T>` no longer exists. `IPublisher.Publish` returns `ValueTask`, so file 2 can `return Publisher.Publish(...)` directly. The task doc's pattern was accurate this time.

## Checklist

### Implementation
- [x] Update `source/timewarp-state/features/state-initialization/state-initialization-pre-processor.cs`:
  - [x] Change base from `IRequestPreProcessor<TRequest>` to `MessagePreProcessor<TMessage, TResponse>`
  - [x] Add `TResponse` generic parameter
  - [x] Rename `Process()` to `protected override ValueTask Handle()`
  - [x] Change return from `Task` to `ValueTask` (body kept `await initializationTask`)
- [x] Update `tests/test-app/test-app-client/pipeline/notification-pre-processor/pre-pipeline-notification-request-pre-processor.cs`:
  - [x] Apply same changes as above

### Verification
- [x] Core project (`timewarp-state`) builds with no error on `state-initialization-pre-processor.cs`; remaining errors are post-processors (043), request handlers (045), service registration (046) only
- [ ] test-app file compiles — deferred; that project builds at task 049 once registration (046) lands

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

