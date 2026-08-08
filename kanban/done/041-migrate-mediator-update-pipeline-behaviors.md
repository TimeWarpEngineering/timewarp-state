# Task 041: Migrate Mediator - Update Pipeline Behaviors

## Description

- Update all `IPipelineBehavior<TRequest, TResponse>` implementations to use martinothamar/Mediator's API
- Change method signatures, return types, and delegate invocation patterns

## Requirements

- Change return type from `Task<TResponse>` to `ValueTask<TResponse>`
- Change delegate type from `RequestHandlerDelegate<TResponse>` to `MessageHandlerDelegate<TMessage, TResponse>`
- Change parameter order (next delegate is now last parameter)
- Change delegate invocation from `next()` to `next(message, cancellationToken)`

## ⚠️ Correction to this task's guidance

The "Move `next` parameter to last position" instruction below (and the "New pattern required" snippet in Notes) is **WRONG**. Verified by reflecting the actual `Mediator.Abstractions` 3.0.2 assembly, the real interface signature is:

```csharp
ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
```

i.e. `next` stays **second**, `cancellationToken` stays **last** — the original parameter *order* is unchanged. Only three things actually change per behavior:
1. return type `Task<TResponse>` → `ValueTask<TResponse>`
2. delegate type `RequestHandlerDelegate<TResponse>` → `MessageHandlerDelegate<TMessage, TResponse>`
3. invocation `next()` → `next(message, cancellationToken)` (the delegate's `Invoke` takes `(message, cancellationToken)`)

## Checklist

### Implementation
- [x] Update `source/timewarp-state/features/pipeline/state-transaction-behavior.cs`:
  - [x] Change return type to `ValueTask<TResponse>`
  - [x] Change delegate to `MessageHandlerDelegate<TRequest, TResponse>`
  - [x] ~~Move `next` parameter to last position~~ — N/A, `next` stays second (see correction above)
  - [x] Change `await next()` to `await next(request, cancellationToken)`
- [x] Update `source/timewarp-state/features/redux-dev-tools/redux-dev-tools-behavior.cs` (note: not in a `pipeline/` subfolder as the path above stated):
  - [x] Apply same changes as above
- [x] Update `source/timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs`:
  - [x] Apply same changes as above (two `nextHandler()` call sites)
- [x] Update `tests/test-app/test-app-client/pipeline/my-behavior.cs`:
  - [x] Apply same changes as above
- [x] Update `tests/test-app/test-app-client/features/event-stream/pipeline/event-stream-behavior.cs`:
  - [x] Apply same changes as above

### Verification
- [x] No `RequestHandlerDelegate` remains anywhere in source/tests/samples
- [x] All 5 behaviors compile against the new interface (no CS0535/CS0246 on behavior files); remaining core-project errors are pre-processors (042), post-processors (043), request handlers (045), service registration (046) only

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

