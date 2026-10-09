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
- Redux DevTools time-travel still reflects in `Store.LoadStatesFromJson`. That method suppresses IL2026, IL2070, IL2072, IL2075, and IL3050. Replacing it with generated hydration is a follow-up. `MethodInfoExtensions.InvokeAsync` is removed.

### Fixes

- Clones of a state that derives from a base in another assembly copy that base's public auto-properties. `TimeWarpCacheableState<T>.CacheKey` and `TimeStamp` stay intact across actions, so a fresh cache is not treated as a miss.
