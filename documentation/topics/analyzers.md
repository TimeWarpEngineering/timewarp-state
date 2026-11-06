---
uid: TimeWarpState:Analyzers.md
title: Analyzers
---

# Analyzers

TimeWarp.State ships Roslyn analyzers inside the `TimeWarp.State` package (`analyzers/dotnet/cs`). Consuming projects get them automatically; there is no separate analyzer package. The persistence source generator in the same package reports **TWSG001** when `[PersistentState]` is applied to a nested class. The clone source generator reports **TWSG002** when it cannot emit a clone for a reachable member. The ActionSet method generator and the persistence generator report **TWSG003** instead of emitting a member that would not compile.

> **Breaking change:** analyzer diagnostic IDs were renamed to the `TWS` prefix: `TW0001` → `TWS0001`, `TW0002` → `TWS0002`, `TW0003` → `TWS0003`. This stops colliding with TimeWarp.SourceGenerators, which owns the bare `TW0001`–`TW0006` range (its `TW0002` is the unrelated XML-docs-to-markdown rule). Update `.editorconfig` `dotnet_diagnostic.*.severity` entries to the new ids.

## TWS0001 — Action must be nested in State

- **Severity:** Error
- **Category:** TimeWarp.State
- **Message:** The Action '{0}' is not a nested type of its State

Any non-abstract type that implements `IAction` must be nested in a type that implements `IState`.

## TWS0002 — Handler must not send an action

- **Severity:** Warning (first release; promote to Error in `.editorconfig`)
- **Category:** Design
- **Message:** Action handler '{0}' sends action '{1}'. Handlers must not dispatch actions; publish a notification or sequence the action from the caller.

A State is an isolated feature boundary. Handlers do their own work. If something elsewhere must react, publish a notification. Orchestration lives outside the state (pages and components sequence actions; notification handlers react).

The analyzer flags a type whose base chain includes `StateActionHandler<>` / `TimeWarp.Mediator.ActionHandler<>` (and the legacy `TimeWarp.State.ActionHandler<>`). Handlers implement `IRequestHandler<>` through `ActionHandler<>`. TimeWarp.Mediator 15 removed `IActionHandler<>`; the analyzer still looks that name up and treats a missing type as not a handler. App bases such as `BaseHandler<>` and `DefaultApiHandler<,,>` are included by symbol, not by name. Every method body in the handler is inspected, including `Handle` and app-defined hooks (`HandleSuccess`, `HandleError`, `HandleException`). Partial classes are walked as one type.

### Flagged

1. `ISender.Send` / `IMediator.Send` / `IState.Sender.Send` whose argument implements `IAction` (including app markers such as `IBaseAction`).
2. Source-generated ActionSet entry methods on a type that implements `IState` (for example `ToastNotificationState.AddProblemDetails(...)`). These are identified by a nested `{MethodName}ActionSet` on the containing state. This is the common dispatch shape in apps that never write a literal `Send`.

### Allowed

- `IPublisher.Publish` / `IMediator.Publish` / an argument that implements `INotification`
- Direct mutation of the handler's own state
- `NavigationManager` (out of scope)

### Promote to Error

```ini
dotnet_diagnostic.TWS0002.severity = error
```

TimeWarp.SourceGenerators ships an unrelated **TW0002** (`XmlDocsToMarkdownAnalyzer`) under the bare `TW` prefix (TW0001–TW0006). This rule used to share that same bare `TW0002` id, so an `.editorconfig` that set `dotnet_diagnostic.TW0002.severity = none` to silence XML-docs-to-markdown also silenced this handler rule. TimeWarp.State's analyzer now uses the `TWS` prefix specifically to avoid that collision; consumers still targeting the old `TW0002` id for this rule should update to `TWS0002`.

### Generator ordering

Roslyn runs source generators before analyzers in the same compilation. Generated ActionSet entry methods are therefore visible to TWS0002 when user code calls them. TWS0002 does not analyze generated method bodies (`GeneratedCodeAnalysisFlags.None`), so the `Sender.Send` inside the generated wrapper is not flagged — that wrapper is not a handler. Pipeline behaviors that send Start/Complete tracking actions are not handlers and stay allowed.

## TWS0003 — AllowActionSend exemption

- **Severity:** Info
- **Category:** Design
- **Message:** Action handler '{0}' is exempt from TWS0002 ({1}). Convert the dispatch to a notification and remove AllowActionSend.

`[AllowActionSend("reason")]` on a handler type or method suppresses TWS0002 so consumers can grandfather patterns such as `HandleError` → toast while converting them to a notification. The reason is required. TWS0003 keeps the debt visible.

```csharp
[AllowActionSend("grandfather HandleError toast until it publishes a notification")]
internal abstract class DefaultApiHandler<TAction, TRequest, TResponse> : ApiHandler<TAction, TRequest, TResponse>
{
}
```

## TWSG001 — Nested PersistentState is not supported

- **Severity:** Error
- **Category:** Persistence
- **Message:** [PersistentState] is not supported on nested class '{0}'. Move the state to a top-level type.

Reported by `TimeWarpStateGenerator` when it emits persistence `Load()`. Policies nest **actions** in states, not states in other types. A nested `[PersistentState]` class is skipped (no generated `Load()` partial). Move the state to a top-level type.

## TWS0009 — State needs ICloneable or an accessible constructor

- **Severity:** Error
- **Category:** Design
- **Message:** The state implementation '{0}' must implement ICloneable or have a constructor the clone source generator can call

A concrete type that derives directly from `State<T>` must implement `ICloneable` or declare a constructor the generator can call. Public constructors count. Internal constructors count in the same assembly. Protected constructors do not. A parameterless constructor is not required. Abstract states, including `TimeWarpCacheableState<TState>`, are exempt.

The unshipped id was `TWS001`. It is `TWS0009` so it sorts with `TWS0001`–`TWS0008`.

## TWS0010 — State&lt;T&gt; type argument

- **Severity:** Error
- **Category:** Design

`T` must be the derived class. An abstract class whose type argument is its own self-constrained type parameter is allowed. A concrete class with that shape is not.

## TWS0011 — Non-abstract state should be sealed

- **Severity:** Warning
- **Category:** Design

A non-abstract class that derives from `State<T>` should be sealed.

## TWS0012 — Public state properties are read-only

- **Severity:** Error
- **Category:** Design

A public property needs no setter, or an `init` setter, or a setter that is private, internal, private protected, or protected internal. A protected setter is allowed on an abstract state. Positional record properties are `init` and are not flagged. The rule reads symbols, so it sees members the compiler generates for a record.

## TWS0013 — State needs a [JsonSerializable] entry

- **Severity:** Warning
- **Category:** Design
- **Message:** The state '{0}' is serialized by {1} but no JsonSerializerContext in this project lists it; add [JsonSerializable(typeof({0}))]
- **Code fix:** yes

System.Text.Json's source generator reads only user source, so TimeWarp.State cannot generate the `[JsonSerializable]` line for you. This rule reports each concrete state that needs source-generated `JsonTypeInfo` and that no `JsonSerializerContext` in the same project lists. `{1}` names the reason:

- **persistence:** the state has `[PersistentState]` and the project declares at least one `JsonSerializerContext`. With no context, persistence uses the reflection resolver and the rule stays quiet.
- **telemetry snapshots:** the project references `TimeWarp.State.Telemetry`. Snapshots have no reflection fallback. A state whose `TypeInfoResolver` returns no type info is skipped with only a debug log, so this reason is reported whether or not a context exists.

The telemetry reason cannot see `IncludeSnapshots` (off by default) or a resolver set on the telemetry options. Both are runtime settings, and an analyzer reads only source. A project that uses telemetry for spans only, with snapshots off, gets one warning per state, and `TreatWarningsAsErrors` fails its build. Turn the rule off or down for that project in `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.TWS0013.severity = none   # or suggestion
```

The code fix offers one action per context in the project: **Add [JsonSerializable(typeof(X))] to AppJsonContext**. With no context it offers **Create StateJsonContext**, which adds `state-json-context.cs` beside the state. The new context is `public` when the state is public, so a host project (a hosted WASM server, or an Auto/prerender server) can wire it. For a non-public state it is `internal`. Then make the context the resolver, once:

```csharp
builder.Services.AddTimeWarpState(options =>
  options.JsonSerializerOptions.TypeInfoResolver = AppJsonContext.Default);
```

A context in another project is not seen. If yours lives there, suppress the rule for this project:

```ini
dotnet_diagnostic.TWS0013.severity = none
```

Abstract and open generic states are skipped. See [Persistence](xref:TimeWarpState:Persistence.md) and [Telemetry](xref:TimeWarpState:Telemetry.md).

## TWS0014 — [Reducer] handler cannot be generated

- **Severity:** Error
- **Category:** Design
- **Message:** [Reducer] on '{0}' gets no generated handler because {1}

`[Reducer]` asks the TimeWarp.State generator to emit an action set's `Handler`. You write only the Action and one static `Reduce` method:

```csharp
public partial class CounterState
{
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

The generator emits `DecrementCountActionSet.Handler : StateActionHandler<Action>`, which gets the state from the store and calls `Reduce`. `ReducerAttribute` derives from TimeWarp.Mediator's `GeneratedHandlerAttribute` (TimeWarp.Mediator 15.0.0-beta.5 or later), so the generated mediator dispatches the Action to that `Handler` even though the Mediator generator cannot see another generator's output.

TimeWarp.Mediator names the handler whether or not the State generator emitted it. When the State generator did not, the build fails inside `Mediator.g.cs` (CS0234 or CS0246, plus TWM012), and none of those says what to change. TWS0014 runs the generator's own rule and reports the reason on the attribute. `{1}` is one of:

- it is not the nested `Action` class of an `*ActionSet` inside a state
- the state, the action set or the Action is generic, or the state is abstract
- the Action does not implement `IAction`
- the action set, the state or a containing type is not `partial`
- the Action is `private` or `protected`
- the handler name is not a simple type name (`[Reducer("Other.Handler")]`)
- the action set already declares `Handler` by hand
- the action set does not declare exactly one `static void Reduce(TState state, Action action)`

The generated `Handler` has the Action's accessibility (`public` or `internal`). `[Reducer("Name")]` names it something other than `Handler`. The hand-written `XActionSet { Action; Handler }` shape keeps working beside it. A `Reduce` declared in another partial file of the action set is picked up the next time the Action's own file changes.

## TWSG002 — State clone cannot be generated

- **Severity:** Error
- **Category:** Cloning
- **Message:** Cannot generate a clone for '{0}'{2} because {1}. Implement ICloneable on '{0}', mark the member [CloneShared] to copy it by reference, or change the unsupported member.

Reported by the clone stage of `TimeWarpStateGenerator` once, at the source member that reaches something it cannot clone: `object`, an interface or abstract class with no implementation, an inaccessible type, an open generic, a pointer, a ref struct, or a type from another assembly whose fields it cannot all see. `{2}` names the state or `[GenerateClone]` root the member was reached from. The build fails. There is no reflection fallback. Implement `ICloneable` on that type, mark the member `[CloneShared]` to copy it by reference, or change the member. `[CloneShared]` is how an injected service (`ILogger<T>`, `HttpClient`, `NavigationManager`) stays on the clone. See [Cloning](xref:TimeWarpState:Cloning.md).

## TWSG003 — State member cannot be generated

- **Severity:** Error
- **Category:** Generation
- **Message:** Cannot generate '{0}' for '{1}' because {2}

Reported by `TimeWarpStateGenerator` for the `State.Method(...)` of each nested `*ActionSet` and for `Load()`, at the Action or the state. Each case used to emit generated code that failed to compile. For ActionSet methods that means a plain class Action in a class state, the only shape the generator served before 12.0.0-beta.17. A record or struct Action, or an ActionSet in a record or struct state, got no generated code before. When one of those cannot get a method, the generator emits nothing and reports nothing, so a build that passed still passes. The message names the reason:

- The state, or a type that contains it, is not `partial`.
- The `[PersistentState]` type does not derive from `State<T>`.
- The ActionSet is named just `ActionSet`, or the Action or its ActionSet has its own type parameters.
- The Action is abstract, or the Action or its chosen constructor is private or protected.
- A parameter would be named `externalCancellationToken`, or a `required` member and a constructor parameter would get the same parameter name.
- The state already has a field, property, event or nested type with the method's name.
- A parameter type is private or protected.

The ActionSet method generator reads the constructor like this. For a record or struct Action, it uses the positional record or primary constructor, else the first explicit constructor. For a plain class Action, it uses the first explicit constructor, else the primary constructor, so a class with both keeps the signature it had before. `required` members become parameters too. A state that already declares the method by hand keeps it, and nothing is generated. An `*ActionSet` whose parent does not implement `IState` is ignored without a diagnostic.
