# Task 047: Migrate Mediator - Update Source Generators

## Description

- Update the TimeWarp.State source generators to emit code compatible with martinothamar/Mediator
- Change generated handler return types and add `Unit.Value` returns

## Requirements

- Update `PersistenceStateSourceGenerator` to generate `ValueTask<Unit>` handlers
- Update `ActionSetMethodSourceGenerator` if needed
- Ensure generated code compiles with new Mediator library

## Scope note

This task absorbed the source-generator half of the architecture fix from 046 (the "make handlers public" decision + generator packaging), in addition to its documented ValueTask<Unit> template change.

### Implementation (DONE — core + timewarp-state-plus build with ZERO errors)
- [x] `persistence-state-source-generator.cs`:
  - [x] generated `Handle` return type `Task` → `ValueTask<global::Mediator.Unit>` (fully qualified — generated file doesn't import Mediator)
  - [x] added `return global::Mediator.Unit.Value;` at end of generated handler
  - [x] generated `LoadActionSet`/`Action`/`Handler` made `public` (consumer's generator must reach them cross-assembly)
- [x] `action-set-method-generator.cs`: reviewed — NO change needed. It emits `public async Task Method(...) { await Sender.Send(...); }`; awaiting `ValueTask<Unit>` from the new `Send` is valid.
- [x] Visibility (the "make public" decision): 11 library `ActionSet` Action+Handler pairs in `-plus` + redux-dev-tools `StartRequest`/`CommitRequest`/`StartHandler`/`CommitHandler`/`DispatchRequest`/`PayloadClass` in core → `public`. (Making `Handler` public forces its `Action` public too — CS0060.)
- [x] Generator packaging: `Mediator.SourceGenerator` in `timewarp-state.csproj` set `PrivateAssets=all` so it stops flowing into `-plus`/consumers; added a direct `Mediator.SourceGenerator` reference to `test-app-client.csproj`. The generator now runs ONLY in the consumer, which owns `AddMediator`.

### Verification
- [x] `source/timewarp-state` builds: 0 errors
- [x] `source/timewarp-state-plus` builds: 0 errors (CS0122 generator-flow errors gone)
- [x] Mediator generator now runs in test-app and registers handlers across assemblies via markers (proven earlier by a standalone 2-project probe)

## ⛔ Remaining test-app/consumer build issues → task 049 (run tests and fix issues)

The library is done; the test-app consumer surfaced two NON-library issues:
1. **.NET 10 attribute collision (unrelated to Mediator):** `[PersistentState]` is now ambiguous between `TimeWarp.Features.Persistence.PersistentStateAttribute` and the new `Microsoft.AspNetCore.Components.PersistentStateAttribute` (added in .NET 10, surfaced by the Components 9→10 bump). Fixed in `-plus`'s post-processor with a `using` alias; recurs on consumer state classes that use `[PersistentState]` (e.g. test-app `blue-state.cs`, `purple-state.cs`). Fix repo-wide via a global `using PersistentStateAttribute = TimeWarp.Features.Persistence.PersistentStateAttribute;` alias (or rename TimeWarp's attribute).
2. **Open-generic notifications incompatible with Mediator's generator:** test-app's `PrePipelineNotification<TRequest>` and `PostPipelineNotification<TRequest,TResponse>` are open-generic `INotification` types; Mediator's generator emits `Publish(...)`/wrapper code referencing bare `TRequest`/`TResponse` → CS0246 in `Mediator.g.cs`. This is test-app infrastructure design, not a library issue — the notifications need to be non-generic (carry the request/response as `object`/typed fields) or otherwise not open-generic. Decide in 049.

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

