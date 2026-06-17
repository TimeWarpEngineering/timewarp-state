# Task 040: Migrate Mediator - Update ActionHandler Base Class

## Description

- Update the `ActionHandler<TAction>` base class to use martinothamar/Mediator's API
- Change return type from `Task` to `ValueTask<Unit>`

## Requirements

- Update method signature to return `ValueTask<Unit>` instead of `Task`
- Ensure `IAction` interface remains compatible with `IRequest`

## Checklist

### Implementation
- [x] Update `source/timewarp-state/base/action-handler.cs`:
  - [x] Change return type from `Task` to `ValueTask<Unit>`
  - [x] Update method signature
- [x] Verify `source/timewarp-state/base/action.cs` (`IAction : IRequest`) is compatible — non-generic `Mediator.IRequest` exists and extends `IRequest<Unit>`; no change needed

### Review
- [x] Verify all derived handlers will need updates (handled in separate tasks) — confirmed: derived `ActionHandler<>` subclasses in timewarp-state-plus, samples, and tests still override `Task Handle` and will be migrated in tasks 041–045

## Notes

**Current implementation:**
```csharp
public abstract class ActionHandler<TAction>
(
  IStore store
) : IRequestHandler<TAction> where TAction : IAction
{
  protected IStore Store { get; set; } = store;

  public abstract Task Handle(TAction action, CancellationToken cancellationToken);
}
```

**New implementation required:**
```csharp
public abstract class ActionHandler<TAction>
(
  IStore store
) : IRequestHandler<TAction> where TAction : IAction
{
  protected IStore Store { get; set; } = store;

  public abstract ValueTask<Unit> Handle(TAction action, CancellationToken cancellationToken);
}
```

**Files to modify:**
1. `source/timewarp-state/base/action-handler.cs`

## Implementation Notes

