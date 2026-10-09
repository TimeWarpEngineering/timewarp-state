---
uid: TimeWarpState:Release.12.0.0-beta.10.md
title: Release 12.0.0-beta.10
---

## Release 12.0.0-beta.10

### Breaking changes

- **`TimeWarpCacheableState<TState>` derives from `State<TState>`.** The constraint is
  `where TState : TimeWarpCacheableState<TState>`. A state declared as `class X : TimeWarpCacheableState<X>` is
  `IState<X>`, and `Hydrate` returns `X`. A type argument that is not itself a `TimeWarpCacheableState` no longer
  compiles; pass the derived state itself. The constraint does not reject passing a different cacheable state, and
  `StateInheritanceAnalyzer` checks only classes that derive directly from `State<T>`, so that mistake is not caught
  for you. An override of `Hydrate` whose return type was `TimeWarpCacheableState<X>` must return `X`. Declarations
  that already passed the state itself need no source change.

- **`FeatureFlagState` is removed from `TimeWarp.State.Plus`.** It was a public placeholder whose `Initialize`
  threw `NotImplementedException`, and it had no actions. `options.UseFeatureFlags()` was described in the 10-to-11
  migration guide and was never implemented. That section is removed. See the
  [12.0.0-beta.10 migration](xref:TimeWarpState:Migration12.0.0-beta.10.md).

- **`InvalidCloneException` takes the cause.** Construct it with `InvalidCloneException.Cause.EmptyGuid` or
  `EqualGuid`. The message names that cause. An empty Guid means the state initializer did not run (uninitialized
  instance, or a custom `ICloneable` that skips construction). An equal Guid means a custom `ICloneable.Clone`
  (for example `MemberwiseClone`) copied `Guid`, or `[IgnoreDataMember]` is missing from `Guid`. The default cloner
  skips `[IgnoreDataMember]`, `[NonSerialized]`, and `[JsonIgnore]`.

### Fixes

- `StartHandler` logs `EventIds.StartHandler_Initializing` (500) from the constructor and
  `EventIds.StartHandler_RequestReceived` (501) when it handles the startup request. It no longer logs
  `JumpToStateHandler_RequestHandled` (512). `Handle` completes with no other work.
