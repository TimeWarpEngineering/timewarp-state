---
uid: TimeWarpState:Actions.md
title: Actions
---

# Actions

An action changes a state. You declare it inside the state it changes, in a nested `*ActionSet` class. The action set holds the `Action` (the message) and the code that applies it. There are two ways to write that code:

- **Short shape (recommended):** mark the `Action` with `[Reducer]` and write one `static void Reduce(TState state, Action action)`. The generator writes the handler.
- **Long shape:** write the `Handler` class yourself. Use it for async work, injected services, or anything beyond a synchronous change to the state.

Both shapes compile to the same thing at run time: a `StateActionHandler<Action>` that TimeWarp.Mediator dispatches to. Both get the same generated dispatch method, catalog entry and manifest entry, and both go through the same pipeline behaviors (transactions, persistence, action tracking, telemetry). You can mix them in one state.

## The two shapes side by side

The same `DecrementCount` action, written both ways.

**Short shape:**

```csharp
public sealed partial class CounterState : State<CounterState>
{
  public int Count { get; private set; }

  public override void Initialize() => Count = 3;

  public static partial class DecrementCountActionSet
  {
    [Reducer]
    public sealed class Action(int amount) : IAction
    {
      public int Amount { get; } = amount;
    }

    private static void Reduce(CounterState state, Action action) => state.Count -= action.Amount;
  }
}
```

**Long shape:**

```csharp
public sealed partial class CounterState : State<CounterState>
{
  public int Count { get; private set; }

  public override void Initialize() => Count = 3;

  public static class DecrementCountActionSet
  {
    public sealed class Action(int amount) : IAction
    {
      public int Amount { get; } = amount;
    }

    internal sealed class Handler(IStore store) : StateActionHandler<Action>(store)
    {
      private CounterState CounterState => Store.GetState<CounterState>();

      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        CounterState.Count -= action.Amount;
        return ValueTask.CompletedTask;
      }
    }
  }
}
```

| | Short shape (`[Reducer]`) | Long shape (hand-written `Handler`) |
|---|---|---|
| You write | `[Reducer]` on `Action`, and `static void Reduce(TState state, Action action)` | `Action` and `Handler : StateActionHandler<Action>` |
| Action set | `static partial class` | `static class` (or `partial`) |
| Generated `Handler` | Yes: `XActionSet.Handler`, which gets the state from the store and calls `Reduce` | No, yours is used |
| Generated dispatch method on the state | Yes: `CounterState.DecrementCount(int amount, CancellationToken? externalCancellationToken = null)` | Yes, the same method |
| Action catalog entry | Yes, when the `Action` also has `[CatalogAction]` | Yes, when the `Action` has `[CatalogAction]` |
| State manifest entry | Yes | Yes |
| How Mediator finds the handler | `[Reducer]` derives from Mediator's `GeneratedHandlerAttribute`, which names `Handler` | The Mediator generator finds your `Handler` in source |
| Async work, injected services | No | Yes |

The generated `Handler` for the short shape looks like this (names shortened):

```csharp
partial class CounterState
{
  static partial class DecrementCountActionSet
  {
    [GeneratedCode("TimeWarp.State.SourceGenerator", "1.0.0")]
    public sealed class Handler(IStore store) : StateActionHandler<Action>(store)
    {
      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        DecrementCountActionSet.Reduce(Store.GetState<CounterState>(), action);
        return default;
      }
    }
  }
}
```

It is nested in your action set, so a `private` `Reduce` and the state's `private set` properties are in reach. It has the Action's accessibility (`public` or `internal`).

## When to hand-write the Handler

`Reduce` takes the state and the action and returns nothing. That covers most actions: set a property, add to a list, flip a flag. Keep the long shape when the handler needs more than that:

- **Async work.** The handler awaits something: an HTTP call, JavaScript interop, a delay, a file read. `Reduce` is synchronous.
- **Injected services.** The handler's constructor takes more than `IStore`, for example `HttpClient`, `ILogger<T>` or `NavigationManager`. The generated `Handler` takes only `IStore`.
- **The cancellation token.** `Reduce` does not receive it.
- **Anything beyond a synchronous change to one state.** For example reading or changing a second state, or publishing a notification.

Converting is mechanical. To go from long to short: add `partial` to the action set, put `[Reducer]` on `Action`, move the body of `Handle` into `private static void Reduce(TState state, Action action)`, and delete the `Handler`. To go back, do the reverse and remove `[Reducer]`. Callers do not change, because the dispatch method is the same.

## Rules for the short shape

The generator emits a `Handler` only for this shape. Anything else is **TWS0014** (Error), reported on `[Reducer]` with the reason. See [Analyzers](xref:TimeWarpState:Analyzers.md).

- The `Action` is the nested `Action` class of an `*ActionSet` inside a state, and implements `IAction`.
- The state, the action set and every containing type are `partial`. None is generic, and the state is not abstract.
- The `Action` is `public` or `internal`.
- The action set declares exactly one `static void Reduce(TState state, Action action)` and no `Handler` of its own.

`[Reducer("Name")]` names the generated type something other than `Handler`. A host needs `TimeWarp.Mediator.Generators` 15.0.0-beta.5 or later for `[Reducer]` actions to dispatch.

The other analyzers treat both shapes the same. TWS0001 (an action must be nested in its state), TWS0004 to TWS0008 (`[CatalogAction]`), and TWS0010/TWS0011 (`State<T>` inheritance) report the same cases for a `[Reducer]` action as for a hand-written one, including a `[Reducer]` action that also has `[CatalogAction]`.

## Calling an action

Either shape gets a generated method on the state, named after the action set without the `ActionSet` suffix. From a component:

```csharp
await CounterState.DecrementCount(amount: 2);
```

Or send the action yourself through the pipeline with an injected `ISender<ClientPipeline>`:

```csharp
await sender.Send(new CounterState.DecrementCountActionSet.Action(2));
```

## Worked example

The [persistence sample](../../samples/05-persistence/readme.md) uses both shapes. `DisplayPreferencesState` writes `SetAccent` and `ToggleCompact` with `[Reducer]`. `DraftNoteState` keeps the hand-written `Handler` for `UpdateText` and `Clear`.
