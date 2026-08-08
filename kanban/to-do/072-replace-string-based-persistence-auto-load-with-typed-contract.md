# Replace string-based persistence auto-load with typed contract

## Description

Code review 2026-06-11, finding 28 (`code-review-2026-06-11.md`).

`source/timewarp-state-plus/features/persistence/state-initialized-notification-handler.cs:22–46` implements auto-load on state initialization by string surgery:

```csharp
string typeName = assemblyQualifiedName.Replace(fullName, $"{fullName}+LoadActionSet+Action");
```

then `Type.GetType(typeName)` and `Activator.CreateInstance`. This bakes a naming convention into the library as string dispatch:

- Works only for the generator-emitted `LoadActionSet.Action`; any hand-written, renamed, generic, or differently-nested load action **silently** gets no load (the else branch only logs Debug and returns).
- `Activator.CreateInstance` requires a parameterless ctor.
- The string-only type reference is not statically reachable, so trimming/AOT can remove the generated type and silently break persistence.
- Renames refactor cleanly in consumer code but invisibly sever the hookup.

## Fix

Express the contract in the type system, discovered once at registration instead of reflected by name per notification — e.g.:

- an `IPersistentState` interface (or extension of the `[PersistentState]` attribute) that references the load action `Type` directly, or
- an `IStateLoader<TState>` service the handler resolves via DI.

Either makes the compiler enforce the hookup and keeps the type statically reachable for trimming.

## Checklist

- [ ] Choose contract shape (attribute-with-Type vs marker interface vs DI service); coordinate with the source generator (task 071), which emits the load action
- [ ] Implement discovery at registration; remove the string mangling
- [ ] Loud failure (or analyzer diagnostic) when a `[PersistentState]` state has no load contract, replacing today's silent LogDebug
- [ ] Test: auto-load fires for generator-emitted and hand-written load actions
