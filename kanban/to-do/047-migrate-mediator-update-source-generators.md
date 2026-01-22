# Task 047: Migrate Mediator - Update Source Generators

## Description

- Update the TimeWarp.State source generators to emit code compatible with martinothamar/Mediator
- Change generated handler return types and add `Unit.Value` returns

## Requirements

- Update `PersistenceStateSourceGenerator` to generate `ValueTask<Unit>` handlers
- Update `ActionSetMethodSourceGenerator` if needed
- Ensure generated code compiles with new Mediator library

## Checklist

### Implementation
- [ ] Update `source/timewarp-state-source-generator/persistence-state-source-generator.cs`:
  - [ ] Change generated `Handle` method return type from `Task` to `ValueTask<Unit>`
  - [ ] Add `return Unit.Value;` at end of generated handler
  - [ ] Update any `async Task` to `async ValueTask<Unit>`
- [ ] Update `source/timewarp-state-source-generator/action-set-method-generator.cs`:
  - [ ] Review if any changes needed for generated code
  - [ ] Ensure generated `Sender.Send()` calls work with new API

## Notes

**Current generated code pattern:**
```csharp
internal sealed class Handler : ActionHandler<Action>
{
  public override async Task Handle(Action action, CancellationToken cancellationToken)
  {
    try
    {
      // Handler logic
      await Publisher.Publish(new StateLoadedNotification(...), cancellationToken);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Error loading...");
      await Publisher.Publish(new StateLoadedNotification(...), cancellationToken);
    }
  }
}
```

**New generated code required:**
```csharp
internal sealed class Handler : ActionHandler<Action>
{
  public override async ValueTask<Unit> Handle(Action action, CancellationToken cancellationToken)
  {
    try
    {
      // Handler logic
      await Publisher.Publish(new StateLoadedNotification(...), cancellationToken);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Error loading...");
      await Publisher.Publish(new StateLoadedNotification(...), cancellationToken);
    }
    return Unit.Value;
  }
}
```

**Key changes in template:**
1. `async Task Handle` -> `async ValueTask<Unit> Handle`
2. Add `return Unit.Value;` before closing brace

**Files to modify:**
1. `source/timewarp-state-source-generator/persistence-state-source-generator.cs`
2. `source/timewarp-state-source-generator/action-set-method-generator.cs`

## Implementation Notes

