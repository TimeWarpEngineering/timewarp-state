---
uid: TimeWarpState:Migration12.0.0-beta.11.md
title: Migrate to 12.0.0-beta.11
---

# Migration to 12.0.0-beta.11

## Cloning

`TimeWarp.Features.Cloning.CloneExtensions` and `CloneErrorHandler` are removed. Delete `Clone<T>()` calls that went through that API.

`StateTransactionBehavior` still snapshots state before each action. A state that implements `ICloneable` uses that `Clone()`. Every other concrete `State<T>` uses the clone the source generator emits into the assembly that declares the state. The generator is packed with `TimeWarp.State` (`analyzers/dotnet/cs`). A project that references the package gets it. A `ProjectReference` to `timewarp-state.csproj` does not run analyzers unless you also reference `timewarp-state-source-generator` with `OutputItemType="Analyzer"` and `ReferenceOutputAssembly="false"`.

### Build errors (TWSG002)

The generator fails the build when it cannot clone a reachable member. Typical causes:

- a field typed as `object`
- a type in another assembly whose shape is not an auto-property, a known collection, or `ICloneable`
- no constructor the generated code can call (public, or internal in the same assembly). Protected constructors do not count
- an open generic, a pointer, or a ref struct

Fix the member, or implement `ICloneable` on that type. `ICloneable.Clone` must return a new instance whose `Guid` is not empty and not equal to the source. Leave `Guid` to the constructor (mark it `[IgnoreDataMember]` if you copy fields yourself).

Types that are not states, and that you clone with `.Clone()`, need `[GenerateClone]` from `TimeWarp.State`.

### TWS001

A concrete class that derives directly from `State<T>` must implement `ICloneable` or have an accessible constructor. A parameterless constructor is no longer required. Abstract bases such as `TimeWarpCacheableState<TState>` are not flagged. Persistence still wants `[JsonConstructor]` on a constructor JSON can call. That requirement is separate from TWS001.

### What stays the same

Ignored members (`IgnoreDataMember`, `NonSerialized`, `JsonIgnore`) keep constructor values. Cycles and shared references inside one graph are preserved. The clone does not block.
