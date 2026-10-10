---
uid: TimeWarpState:Release.12.0.0-beta.11.md
title: Release 12.0.0-beta.11
---

## Release 12.0.0-beta.11

### Breaking changes

- **State clones are generated. The reflection cloner is removed.** `StateCloneSourceGenerator` emits the clone for each concrete `State<T>` and for types marked `[GenerateClone]`. `StateTransactionBehavior` calls `ICloneable.Clone` when the state implements it, and otherwise `StateCloneRegistry.Clone`. `DeepCloner`, `CloneExtensions.Clone<T>()`, and `CloneErrorHandler` are gone. A member the generator cannot clone is build error **TWSG002**. See the [12.0.0-beta.11 migration](xref:TimeWarpState:Migration12.0.0-beta.11.md) and [Cloning](xref:TimeWarpState:Cloning.md).

- **`TWS001` no longer requires a parameterless constructor.** A concrete state that derives directly from `State<T>` must implement `ICloneable` or have a constructor the generator can call (public, or internal in the same assembly). Abstract states are exempt.

### Other changes

- `State<T>.Guid`, `Sender`, and `CancellationTokenSource` stay unshared. Ignored members are `IgnoreDataMember`, `NonSerialized`, and `JsonIgnore`, including on backing fields.
- `TimeWarp.State` builds with `IsAotCompatible=true`, and trim/AOT warnings (IL2xxx/IL3xxx) are build errors in that project. The remaining reflection is suppressed in place with a justification and is a follow-up: the state assembly scan in `AddTimeWarpState` (`EnsureStates`, IL2026 and IL2072), Redux DevTools time travel in `Store.LoadStatesFromJson` (IL2026, IL3050) and `LoadStateFromJson` (IL2026, IL2070, IL2072, IL2075, IL3050), and `GetInterfaces` in `LogTimeWarpStateMiddleware` (no warning). `MethodInfoExtensions.InvokeAsync` is removed.
- The generated clone covers fields declared on generic types, tuples, `KeyValuePair`, `Nullable<T>` structs, sorted, linked, concurrent, and read-only collections, `StringBuilder`, and BCL values held behind collection interfaces (copied into `List<T>`, `HashSet<T>`, or `Dictionary<TKey,TValue>` when the type is unknown). Polymorphic members keep their runtime type. See [Cloning](xref:TimeWarpState:Cloning.md).
- A type from another assembly is cloned only when the generator can see all of its fields. DTOs from a sibling project that are not plain auto-property types need `ProduceReferenceAssembly=false` on that project, or `ICloneable`.

### Fixes

- Clones of a state that derives from a base in another assembly copy that base's public auto-properties. `TimeWarpCacheableState<T>.CacheKey` and `TimeStamp` stay intact across actions, so a fresh cache is not treated as a miss.
