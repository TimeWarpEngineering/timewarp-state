---
uid: TimeWarpState:Release.12.0.0-beta.17.md
title: Release 12.0.0-beta.17
---

## Release 12.0.0-beta.17

### Source generators fixed in place (task 141)

Nothing that compiled before changes meaning. Some code that failed to compile now compiles, and some generated code that failed to compile is now a clear **TWSG003** error instead.

**ActionSet methods (`State.Method(...)`)**

- A positional record Action gets a method. `public sealed record Action(int Amount, bool Animate = false) : IAction;` generates `Add(int Amount, bool Animate = false, CancellationToken? externalCancellationToken = null)`. Before, it got nothing.
- A primary-constructor Action gets its parameters. `public sealed class Action(int amount) : IAction` used to generate a method that did not compile (CS7036). A class Action with both a primary and an explicit constructor keeps the explicit constructor's signature, as before. A record or struct Action uses its positional or primary parameter list first.
- `required` members become parameters and are set with an object initializer. They come after constructor parameters without a default and before optional ones. Plain `init` properties are not parameters, so `IncrementCount()` keeps its signature.
- A state that already declares the method by hand keeps it. For a record or struct Action, any method with that name counts, because those never had a generated method before.
- Nested, generic and global-namespace states get a partial that merges. A default value that names a constant only visible inside the Action now compiles. A public state with an internal parameter type gets an `internal` method (it was CS0051).
- An `*ActionSet` whose parent does not implement `IState`, for example one inside a static docs class, is ignored. It used to get a `partial class` that did not compile.
- **TWSG003** (Error) replaces generated code that could not compile: a state or containing type that is not `partial`, an abstract or inaccessible Action or constructor, a parameter named `externalCancellationToken`, a duplicate parameter name, a non-method member with the method's name, or a private parameter type. It applies only to a plain class Action in a class state, which always got generated code. A record or struct Action, or an ActionSet in a record or struct state, never got generated code. When one of those cannot get a method, nothing is emitted and nothing is reported, as before. See [Analyzers](xref:TimeWarpState:Analyzers.md).
- The **SG002** Info diagnostic per generated file is gone.

**Action catalog**

- `[CatalogAction]` on a record Action, or on an Action with a primary constructor, produces an entry with its parameters. Parameter order is the generated method's order.
- An entry is skipped when the ActionSet method cannot be generated, so the build shows the TWSG003 error alone, not with a follow-on CS1061.
- When the state declares the method by hand, the entry calls that method only when it takes the generated parameters plus a trailing `CancellationToken? externalCancellationToken` and returns `Task`. Otherwise the entry is skipped, so a record Action with a hand-written method of another signature still compiles.

**Persistence**

- `[PersistentStateAttribute]` written out in full now generates `Load()`, as `[PersistentState]` did.
- The generated partial has no accessibility modifier and uses `global::` type names. An `internal` state no longer gets CS0262, and the code no longer depends on the consumer's usings. A global-namespace or generic state gets a partial that merges.
- **TWSG003** is reported when the state is not `partial` (it was CS0260), or when it does not derive from `State<T>`.

**Incrementality**

- Every generator step is named, and a test pins it. After an unrelated edit, every named step is `Cached` or `Unchanged` and no source is added again. The ActionSet method model is now an equatable record. Before, an unrelated edit re-emitted every method.
- The clone generator still plans from `CompilationProvider`, so planning reruns on every edit. Its result is now an equatable value, so an equal plan leaves the output `Cached`.
