---
uid: TimeWarpState:Cloning.md
title: Cloning
---

# Cloning

`StateTransactionBehavior` snapshots a state before every action. The snapshot is a deep clone. Hand-written `ICloneable.Clone` wins. Every other concrete `State<T>` is cloned by code that `StateCloneSourceGenerator` emits into the assembly that declares the state. The generator ships inside the `TimeWarp.State` package (`analyzers/dotnet/cs`), in the project `timewarp-state-source-generator`. There is no reflection cloner.

## What gets a clone

Every concrete, closed, accessible `State<T>` that does not implement `ICloneable` is registered in `StateCloneRegistry` by a module initializer. The state does not have to be `partial`. The generated entry point is `TimeWarp.Features.Cloning.GeneratedCloneExtensions.Clone`.

Mark a class or struct that is not a state with `[GenerateClone]` (`TimeWarp.State`, `Inherited = false`) when that type should get the same clone. Open generics are not emitted until a closed construction appears in the compilation.

`ICloneable` is the escape hatch. The generator emits nothing for that type, and the behavior calls `Clone()` first.

## What is copied

The clone assigns each instance field, including auto-property backing fields. Members marked `IgnoreDataMember`, `NonSerialized`, or `JsonIgnore` (any namespace, on the field or on the associated property or event) keep the value the constructor set. `State<T>.Guid` stays unique. `Sender` and `CancellationTokenSource` are not shared. The behavior assigns `Sender` after the clone.

Primitives, enums, `string`, dates, `Guid`, `Uri`, delegates, `Type`, `MemberInfo`, comparers, and `IServiceProvider` are copied by reference or by value, matching the old shared-type list. Cycles and repeated references inside one clone go through `CloneMap` (`ReferenceEqualityComparer`). The clone never waits, so it stays safe on single-threaded browser WebAssembly.

Arrays, `List<T>`, `Dictionary<TKey,TValue>`, `HashSet<T>`, `Stack<T>`, `Queue<T>`, `Collection<T>`, and `ObservableCollection<T>` are rebuilt element by element. Comparers are shared. `ObservableCollection<T>` does not copy `CollectionChanged` subscribers. Immutable and frozen collections are shared when their elements are shared, and rebuilt otherwise. A field typed as a collection interface is switched on the concrete types in that compilation. Hash collections are rebuilt, so keys that use reference identity do not survive.

Records are copied field by field, not with `with`. Structs are copied by value, then reference fields are deep-copied. `init` and `readonly` fields are written with `[UnsafeAccessor]`.

Public and protected auto-properties on a base type in another assembly are copied through the `<Property>k__BackingField` name. Private fields that the compiler symbol model does not expose on a referenced assembly stay at constructor values. `State<T>.CancellationTokenSource` is one of those, and the constructor already creates a new source.

## When the build fails

`TWSG002` (error) names each reachable member the generator cannot clone: `object`, a pointer, a ref struct, an open generic, a type generated code cannot name, or a type with no constructor the generator can call. Implement `ICloneable` on that type, or change the member. The build fails. There is no runtime reflection fallback.

A state that is dispatched without a registration throws `InvalidOperationException` from `StateCloneRegistry.Clone`. That happens when the state's assembly was built without the generator.

`TWS001` requires a concrete type that derives directly from `State<T>` to implement `ICloneable` or to have a constructor the generator can call (public, or internal in the same assembly). A parameterless constructor is not required. Abstract states are exempt.

## Constructors

The generated clone calls an accessible parameterless constructor when one exists. Otherwise it calls the accessible constructor with the fewest parameters, passing `default` for each argument. It does not call `GetUninitializedObject`. Protected constructors are not accessible to the generated class. A required member is set from an object initializer when the constructor is not marked `SetsRequiredMembers`.
