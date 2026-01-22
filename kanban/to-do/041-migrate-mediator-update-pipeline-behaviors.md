# Task 041: Migrate Mediator - Update Pipeline Behaviors

## Description

- Update all `IPipelineBehavior<TRequest, TResponse>` implementations to use martinothamar/Mediator's API
- Change method signatures, return types, and delegate invocation patterns

## Requirements

- Change return type from `Task<TResponse>` to `ValueTask<TResponse>`
- Change delegate type from `RequestHandlerDelegate<TResponse>` to `MessageHandlerDelegate<TMessage, TResponse>`
- Change parameter order (next delegate is now last parameter)
- Change delegate invocation from `next()` to `next(message, cancellationToken)`

## Checklist

### Implementation
- [ ] Update `source/timewarp-state/features/pipeline/state-transaction-behavior.cs`:
  - [ ] Change return type to `ValueTask<TResponse>`
  - [ ] Change delegate to `MessageHandlerDelegate<TRequest, TResponse>`
  - [ ] Move `next` parameter to last position
  - [ ] Change `await next()` to `await next(request, cancellationToken)`
- [ ] Update `source/timewarp-state/features/redux-dev-tools/pipeline/redux-dev-tools-behavior.cs`:
  - [ ] Apply same changes as above
- [ ] Update `source/timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs`:
  - [ ] Apply same changes as above
- [ ] Update `tests/test-app/test-app-client/pipeline/my-behavior.cs`:
  - [ ] Apply same changes as above
- [ ] Update `tests/test-app/test-app-client/features/event-stream/pipeline/event-stream-behavior.cs`:
  - [ ] Apply same changes as above

## Notes

**Current pattern:**
```csharp
public async Task<TResponse> Handle(
  TRequest request,
  RequestHandlerDelegate<TResponse> next,
  CancellationToken cancellationToken)
{
  // Pre-processing...
  TResponse response = await next();
  // Post-processing...
  return response;
}
```

**New pattern required:**
```csharp
public async ValueTask<TResponse> Handle(
  TRequest message,
  CancellationToken cancellationToken,
  MessageHandlerDelegate<TRequest, TResponse> next)
{
  // Pre-processing...
  TResponse response = await next(message, cancellationToken);
  // Post-processing...
  return response;
}
```

**Files to modify (5 total):**
1. `source/timewarp-state/features/pipeline/state-transaction-behavior.cs`
2. `source/timewarp-state/features/redux-dev-tools/pipeline/redux-dev-tools-behavior.cs`
3. `source/timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs`
4. `tests/test-app/test-app-client/pipeline/my-behavior.cs`
5. `tests/test-app/test-app-client/features/event-stream/pipeline/event-stream-behavior.cs`

## Implementation Notes

