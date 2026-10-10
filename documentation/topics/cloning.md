---
uid: TimeWarpState:Cloning.md
title: Cloning
---

# Cloning

`StateTransactionBehavior` snapshots a state before every action. The snapshot is a deep clone. Hand-written `ICloneable.Clone` wins. Every other concrete `State<T>` is cloned by code that `StateCloneSourceGenerator` emits into the assembly that declares the state. The generator ships inside the `TimeWarp.State` package (`analyzers/dotnet/cs`), in the project `timewarp-state-source-generator`. There is no reflection cloner.

## Transaction rollback

The behavior installs the clone as the live state before the handler runs. On exception or cancellation it restores the pre-action snapshot when `ReferenceEquals` shows that the store holds that clone. A different instance means a later action already committed its own clone. The behavior leaves that state in place and logs that a concurrent action advanced the state. `ExceptionNotification` is published for a non-cancellation failure. Actions on one state are not serialized. The behavior never blocks and never calls `SemaphoreSlim.Wait`. In-place mutation of the live clone is not a new commit: when the action that installed the clone fails, those writes roll back with it. The guard does not undo writes another action has already copied. If a failing action mutated its clone before a concurrent action cloned it, those writes carry into the concurrent action's clone and stay when the failing action's rollback is skipped.

## What gets a clone

Every concrete, closed, accessible `State<T>` that does not implement `ICloneable` is registered in `StateCloneRegistry` by a module initializer. The state does not have to be `partial`. The generated entry point is `TimeWarp.Features.Cloning.GeneratedCloneExtensions.Clone`.

Mark a class or struct that is not a state with `[GenerateClone]` (`TimeWarp.State`, `Inherited = false`) when that type should get the same clone. Open generics are not emitted until a closed construction appears in the compilation.

`ICloneable` is the escape hatch. The generator emits nothing for that type, and the behavior calls `Clone()` first.

## What is copied

The clone assigns each instance field, including auto-property backing fields. A field or auto-property marked `[CloneShared]` (`TimeWarp.State.CloneSharedAttribute`) is assigned from the source. The generator does not walk that member's type, so the clone holds the same instance. Use it for an injected service (`ILogger<T>`, `HttpClient`, `NavigationManager`, `IJSRuntime`). On a property with a hand-written getter or setter the attribute has no effect, so put it on the backing field. Members marked `IgnoreDataMember`, `NonSerialized`, or `JsonIgnore` (any namespace, on the field or on the associated property or event) keep the value the constructor set. When a member has both `[CloneShared]` and an ignore attribute, `[CloneShared]` wins for cloning. The ignore attribute applies to serialization. `State<T>.Guid` stays unique. `Sender` and `CancellationTokenSource` are not shared. The behavior assigns `Sender` after the clone.

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

A member whose type is a non-sealed class, an abstract class, or an interface is cloned by its runtime type. The generated switch covers every known concrete type that derives from or implements the member type, deepest type first. Known types are the types in the compilation and, when the member type is declared in a referenced (non-framework) assembly such as a contracts project, the types in that assembly and in the other referenced assemblies that reference it. Each of those passes the same checks as any type from another assembly (see below). A value whose exact type is the member type uses that type's own cloner.

A generic subtype is closed over the member type's type arguments. The generator matches the subtype's base classes and interfaces against the member type and reads each type parameter from the matching argument. For a `Result<string>` member, `record Ok<T>(T Value) : Result<T>` becomes a case for `Ok<string>`; for an `IRepository<Order>` member, `class Repository<T> : IRepository<T>` becomes `Repository<Order>`. Nested arguments work too (`class Many<T> : Base<List<T>>` for `Base<List<int>>` is `Many<int>`). A generic subtype that can never be the member type, such as `Many<T>` for a `Base<int>` member, is ignored.

A known subtype that generated code cannot name is TWSG002 unless it implements `ICloneable`. That covers a private or protected nested type, a `file` type, and a type internal to another assembly. It also covers a generic subtype the member type cannot close: a type parameter the member type does not determine (`class Pair<T, U> : Base<T>` for `Base<int>`, or `class Gen<T> : Base` for a non-generic `Base`), or a type argument that breaks a constraint (`class RefOnly<T> : Base<T> where T : class` for `Base<int>`). The generator cannot tell whether such a value ever reaches the member, so it fails the build rather than throw later.

A type that implements `ICloneable` is cloned by its own `Clone()`, and the result is cast to the member's declared type. A subclass that inherits `Clone()` from its base therefore gets whatever the base returns.

Known limitation: a runtime type declared in an assembly the project does not reference at build time (loaded dynamically, or from a project that references the contracts assembly but is not referenced here) throws `InvalidOperationException` when cloned, unless it implements `ICloneable`. The generator never saw it, so it cannot be a build error. The same applies to a subclass declared in a framework assembly (`System.*`, `Microsoft.*`), which the generator does not scan. A subclass of `List<T>` or another BCL collection that the generator does not know is copied into the base collection type instead.

## Types from other assemblies

A type declared in another assembly is cloned field by field only when the generator can prove it sees every field:

- types from TimeWarp.State's own assemblies, such as `State<T>` and `TimeWarpCacheableState<TState>`
- types from an implementation assembly (a NuGet `lib` assembly, or a project built with `ProduceReferenceAssembly=false`). The generator re-reads that assembly with private members. Every private field must be the backing field of a visible auto-property, an event backing field (subscribers are not copied), or ignored
- types from a reference assembly (the default for a `ProjectReference`, and every framework reference pack) only when every instance property is an auto-property and every instance method is compiler-generated (records). A reference assembly drops private class fields, so anything else could hide state

Other types from other assemblies are TWSG002. Set `<ProduceReferenceAssembly>false</ProduceReferenceAssembly>` in a project whose DTOs live in your states, or implement `ICloneable`. The test app does this for `test-app-contracts`.

## When the build fails

`TWSG002` (error) is reported once, at the source member that reaches something the generator cannot clone. The message names the type, the member, the reason, and the state it was reached from. Causes:

- a member typed as `object`, or as an interface or abstract class with no implementation the generator can see
- a pointer or a ref struct
- an open generic state, or a state or member type that generated code cannot name (private, protected, `file`, or internal to another assembly)
- a known subtype of a polymorphic member that generated code cannot name, or a generic subtype the member type cannot close (see above)
- a type from another assembly whose fields cannot all be seen (see above)
- a subclass of `ReadOnlyCollection<T>` or `ReadOnlyDictionary<TKey,TValue>`
- a derived type in the compilation that cannot be cloned itself

Implement `ICloneable` on that type, mark the member `[CloneShared]` to copy it by reference, or change the member. `[CloneShared]` is the opt-in for an injected service whose type the generator cannot clone. The build fails. There is no runtime reflection fallback.

A state that is dispatched without a registration throws `InvalidOperationException` from `StateCloneRegistry.Clone`. That happens when the state's assembly was built without the generator.

`TWS0009` requires a concrete type that derives directly from `State<T>` to implement `ICloneable` or to have a public constructor (or an internal one in the same assembly). A parameterless constructor is not required. Abstract states are exempt.

## Constructors

The generated clone creates the new instance with a constructor so field initializers run:

1. the parameterless constructor, at any accessibility (a private or protected one is called through `[UnsafeAccessor]`)
2. otherwise the accessible constructor with the fewest parameters
3. otherwise any constructor, through `[UnsafeAccessor]`

Each argument is the parameter's declared default value, or `default` cast to the parameter type, so overloads with the same number of parameters stay unambiguous. A `required` member, on the type or a base type, is set to `default` in an object initializer unless the constructor has `SetsRequiredMembers`. The copied fields then overwrite those values. The clone does not call `GetUninitializedObject`.

A constructor that throws on a `default` argument throws on every clone. The clone passes `default` for each constructor parameter, including services, so a constructor that takes a service must accept `null`. Mark the field or auto-property that stores the service with `[CloneShared]`. The generator assigns that member from the original after construction, and the live state keeps the same instance. `[IgnoreDataMember]` or `[JsonIgnore]` leave the constructor value, which is null for a service parameter, and the next action fails when it uses the service. `ICloneable` on the state remains the escape hatch when the generated clone cannot express the copy.
