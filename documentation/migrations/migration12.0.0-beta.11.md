---
uid: TimeWarpState:Migration12.0.0-beta.11.md
title: Migrate to 12.0.0-beta.11
---

# Migration to 12.0.0-beta.11

## Cloning

`TimeWarp.Features.Cloning.CloneExtensions` and `CloneErrorHandler` are removed. Delete `Clone<T>()` calls that went through that API.

`StateTransactionBehavior` still snapshots state before each action. A state that implements `ICloneable` uses that `Clone()`. Every other concrete `State<T>` uses the clone the source generator emits into the assembly that declares the state. The generator is packed with `TimeWarp.State` (`analyzers/dotnet/cs`). A project that references the package gets it. A `ProjectReference` to `timewarp-state.csproj` does not run analyzers unless you also reference `timewarp-state-source-generator` with `OutputItemType="Analyzer"` and `ReferenceOutputAssembly="false"`.

### Build errors (TWSG002)

The generator fails the build when it cannot clone a reachable member. The error is reported at that member and names the state it was reached from. Typical causes:

- a field typed as `object`, or as an interface or abstract class with no implementation in the compilation
- a type in another assembly whose fields the generator cannot all see. A `ProjectReference` compiles against a reference assembly, which hides private fields, so a DTO from a sibling project is accepted only when it has nothing but auto-properties. Add `<ProduceReferenceAssembly>false</ProduceReferenceAssembly>` to that project so the generator can inspect its fields, or implement `ICloneable`
- a framework class with private state, for example `MemoryStream` (known collections and `StringBuilder` are supported)
- an open generic, a pointer, or a ref struct

Fix the member, implement `ICloneable` on that type, or mark the member `[CloneShared]` to copy it by reference. `[CloneShared]` is the opt-in for an injected service. `ICloneable.Clone` must return a new instance whose `Guid` is not empty and not equal to the source. Leave `Guid` to the constructor (mark it `[IgnoreDataMember]` if you copy fields yourself).

Types that are not states, and that you clone with `.Clone()`, need `[GenerateClone]` from `TimeWarp.State`.

### Constructors

The clone creates each instance with a constructor: the parameterless one at any accessibility, otherwise the accessible one with the fewest parameters, otherwise any constructor through `[UnsafeAccessor]`. Arguments are the declared defaults or `default`. A state constructor that takes services must accept `null`. Mark the field or auto-property that stores a service with `[CloneShared]` so the clone keeps that instance. `[IgnoreDataMember]` or `[JsonIgnore]` leave the constructor value, which is null, and the next action fails when it uses the service. Otherwise implement `ICloneable`.

### Runtime types

A member typed as a non-sealed class, an abstract class, or an interface is cloned by its runtime type, using the types the generator saw in the compilation. A subclass declared in another assembly throws `InvalidOperationException` when cloned unless it implements `ICloneable`. A collection interface holding a value of an unknown type (a LINQ iterator, for example) is copied into a `List<T>`, `HashSet<T>`, or `Dictionary<TKey,TValue>`.

### TWS001

A concrete class that derives directly from `State<T>` must implement `ICloneable` or have an accessible constructor. A parameterless constructor is no longer required. Abstract bases such as `TimeWarpCacheableState<TState>` are not flagged. Persistence still wants `[JsonConstructor]` on a constructor JSON can call. That requirement is separate from TWS001.

### What stays the same

A member marked `[CloneShared]` is the same instance on the clone. Ignored members (`IgnoreDataMember`, `NonSerialized`, `JsonIgnore`) keep constructor values. `[CloneShared]` wins when a member has both. Cycles and shared references inside one graph are preserved. The clone does not block.
