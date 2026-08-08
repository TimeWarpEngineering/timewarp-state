# Task 043: Migrate Mediator - Update Post-Processors

## Description

- Convert `IRequestPostProcessor<TRequest, TResponse>` implementations to `MessagePostProcessor<TMessage, TResponse>` abstract class
- Change from interface implementation to abstract class inheritance

## Requirements

- Change from implementing interface to extending abstract class
- Rename method from `Process()` to `Handle()` (protected override)
- Change return type from `Task` to `ValueTask`

## Verified API (Mediator.Abstractions 3.0.2, by reflection)

`MessagePostProcessor<TMessage, TResponse>` is an `abstract class : IPipelineBehavior<TMessage, TResponse>`. Override:

```csharp
protected override ValueTask Handle(TMessage message, TResponse response, CancellationToken cancellationToken)
```

Note `response` is the SECOND parameter (post-processors see the handler result). Kept existing `TRequest`/`request` type/param names (overrides need not match base param names) to minimize churn; added `sealed`; `Task.CompletedTask` → `default`. `IPublisher.Publish` returns `ValueTask`, so the test-app file's `return Publisher.Publish(...)` is unchanged.

## Checklist

### Implementation
- [x] Update `source/timewarp-state/features/render-subscriptions/render-subscriptions-post-processor.cs`:
  - [x] Change base from `IRequestPostProcessor<TRequest, TResponse>` to `MessagePostProcessor<TRequest, TResponse>`
  - [x] Rename `Process()` to `protected override ValueTask Handle()`
  - [x] Change return from `Task` to `ValueTask` (`Task.CompletedTask` → `default`)
- [x] Update `source/timewarp-state-plus/features/persistence/pipeline/persistent-state-post-processor.cs`:
  - [x] Apply same changes as above (async `ValueTask`, early `return;` kept)
- [x] Update `source/timewarp-state-plus/features/timers/multi-timer-post-processor.cs`:
  - [x] Apply same changes as above (async `ValueTask`)
- [x] Update `tests/test-app/test-app-client/pipeline/notification-post-processor/post-pipeline-notification-request-post-processor.cs`:
  - [x] Apply same changes as above

### Verification
- [x] Core project (`timewarp-state`) builds with no error on `render-subscriptions-post-processor.cs`; remaining errors are request handlers (045) and service registration (046) only
- [ ] `-plus` post-processors + test-app file compile — deferred to task 049 (both build through the core project, which doesn't compile until 045/046 land)

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

