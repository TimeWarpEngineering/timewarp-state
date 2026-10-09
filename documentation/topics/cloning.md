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

Primitives, enums, `string`, dates, `Guid`, `Uri`, `Version`, `BigInteger`, delegates, `Type`, `MemberInfo`, comparers, `IServiceProvider`, and types in `System.Threading` or `System.Threading.Tasks` are copied by reference or by value, matching the old shared-type list. A struct with no reference fields is copied by value. Cycles and repeated references inside one clone go through `CloneMap` (`ReferenceEqualityComparer`). The clone never waits, so it stays safe on single-threaded browser WebAssembly.

Fields are read and written with `[UnsafeAccessor]`, so `init`, `readonly`, and private fields work. A field declared on a generic type, such as `T Value` on `Wrapper<T>` or `List<TState> History` on a generic state base, goes through a generic accessor class whose type parameters mirror that type (.NET 9 or later). `Nullable<T>`, `KeyValuePair<TKey,TValue>`, and tuples are rebuilt through their public members. `StringBuilder` is copied by its text.

Records are copied field by field, not with `with`. Structs are copied by value, then reference fields are deep-copied.

## Collections

These are rebuilt element by element through their public API, and their comparers are shared:

- arrays of any rank (zero lower bound)
- `List<T>`, `HashSet<T>`, `SortedSet<T>`, `LinkedList<T>`, `Stack<T>`, `Queue<T>`, `Collection<T>`, `ObservableCollection<T>`
- `Dictionary<TKey,TValue>`, `SortedDictionary<TKey,TValue>`, `SortedList<TKey,TValue>`, `ConcurrentDictionary<TKey,TValue>`
- `ReadOnlyCollection<T>` and `ReadOnlyDictionary<TKey,TValue>`, rebuilt around a new `List<T>` or `Dictionary<TKey,TValue>`
- a subclass of one of the mutable collections above: its own fields are copied, then the items are added

Immutable and frozen collections (`ImmutableArray`, `ImmutableList`, `ImmutableHashSet`, `ImmutableSortedSet`, `ImmutableQueue`, `ImmutableStack`, `ImmutableDictionary`, `ImmutableSortedDictionary`, `FrozenSet`, `FrozenDictionary`) are shared when their elements are shared. Otherwise they are rebuilt with a builder, in the same order. A stack keeps its top on top.

A member typed as a collection interface (`IEnumerable<T>`, `ICollection<T>`, `IList<T>`, `IReadOnlyCollection<T>`, `IReadOnlyList<T>`, `ISet<T>`, `IReadOnlySet<T>`, `IDictionary<TKey,TValue>`, `IReadOnlyDictionary<TKey,TValue>`) switches on the runtime value. It tries arrays and each collection type above that is assignable to the interface, then the implementations declared in the compilation, then `ICloneable`. Any other value, such as a LINQ iterator, a compiler-generated collection-expression type, or a collection type from another library, is copied into a `List<T>`, a `HashSet<T>` (for the set interfaces), or a `Dictionary<TKey,TValue>`. The clone then has that type instead of the original one.

`ObservableCollection<T>` does not copy `CollectionChanged` subscribers. Hash collections are rebuilt, so keys that use reference identity do not survive.

## Polymorphic members

A member whose type is a non-sealed class, an abstract class, or an interface is cloned by its runtime type. The generated switch covers every concrete type in the compilation that derives from or implements the member type, deepest type first. A value whose exact type is the member type uses that type's own cloner. A runtime type the generator never saw throws `InvalidOperationException` unless it implements `ICloneable`. That only happens for a subclass declared in another assembly (or a generic subclass). A subclass of `List<T>` or another BCL collection from elsewhere is copied into the base collection type instead.

## Types from other assemblies

A type declared in another assembly is cloned field by field only when the generator can prove it sees every field:

- types from TimeWarp.State's own assemblies, such as `State<T>` and `TimeWarpCacheableState<TState>`
- types from an implementation assembly (a NuGet `lib` assembly, or a project built with `ProduceReferenceAssembly=false`). The generator re-reads that assembly with private members. Every private field must be the backing field of a visible auto-property, an event backing field (subscribers are not copied), or ignored
- types from a reference assembly (the default for a `ProjectReference`, and every framework reference pack) only when every instance property is an auto-property and every instance method is compiler-generated (records). A reference assembly drops private class fields, so anything else could hide state

Other types from other assemblies are TWSG002. Set `<ProduceReferenceAssembly>false</ProduceReferenceAssembly>` in a project whose DTOs live in your states, or implement `ICloneable`. The test app does this for `test-app-contracts`.

## When the build fails

`TWSG002` (error) is reported once, at the source member that reaches something the generator cannot clone. The message names the type, the member, the reason, and the state it was reached from. Causes:

- a member typed as `object`, or as an interface or abstract class with no implementation in the compilation
- a pointer or a ref struct
- an open generic state, or a state or member type that generated code cannot name (private, or internal to another assembly)
- a type from another assembly whose fields cannot all be seen (see above)
- a subclass of `ReadOnlyCollection<T>` or `ReadOnlyDictionary<TKey,TValue>`
- a derived type in the compilation that cannot be cloned itself

Implement `ICloneable` on that type, or change the member. The build fails. There is no runtime reflection fallback.

A state that is dispatched without a registration throws `InvalidOperationException` from `StateCloneRegistry.Clone`. That happens when the state's assembly was built without the generator.

`TWS001` requires a concrete type that derives directly from `State<T>` to implement `ICloneable` or to have a public constructor (or an internal one in the same assembly). A parameterless constructor is not required. Abstract states are exempt.

## Constructors

The generated clone creates the new instance with a constructor so field initializers run:

1. the parameterless constructor, at any accessibility (a private or protected one is called through `[UnsafeAccessor]`)
2. otherwise the accessible constructor with the fewest parameters
3. otherwise any constructor, through `[UnsafeAccessor]`

Each argument is the parameter's declared default value, or `default` cast to the parameter type, so overloads with the same number of parameters stay unambiguous. A `required` member, on the type or a base type, is set to `default` in an object initializer unless the constructor has `SetsRequiredMembers`. The copied fields then overwrite those values. The clone does not call `GetUninitializedObject`.

A constructor that throws on a `default` argument throws on every clone. A state whose constructor takes services must accept `null` for them, and should mark any field that stores a service with `[IgnoreDataMember]` or `[JsonIgnore]` so the clone does not try to copy it. Otherwise implement `ICloneable` on that state.
