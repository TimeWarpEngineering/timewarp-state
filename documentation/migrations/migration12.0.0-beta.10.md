---
uid: TimeWarpState:Migration12.0.0-beta.10.md
title: Migrate to 12.0.0-beta.10
---

# Migration to 12.0.0-beta.10

## TimeWarpCacheableState

`TimeWarpCacheableState<TState>` now derives from `State<TState>` and requires
`where TState : TimeWarpCacheableState<TState>`.

A derived state written as `class X : TimeWarpCacheableState<X>` still compiles. It implements `IState<X>`, and
`Hydrate` returns `X`.

A type argument that is not a `TimeWarpCacheableState` no longer compiles. Change it so the argument is the derived
state itself. Passing a different cacheable state still compiles, and `StateInheritanceAnalyzer` checks only classes
that derive directly from `State<T>`, so check those declarations by hand and pass the derived state itself. Change an
override of `Hydrate` that returned `TimeWarpCacheableState<X>` so it returns `X`.

## FeatureFlagState

`FeatureFlagState` is no longer in `TimeWarp.State.Plus`. Delete references to it. There is no
`UseFeatureFlags` registration method. The 10-to-11 migration guide no longer tells you to call one.

## InvalidCloneException

The constructor is `InvalidCloneException(Type enclosingStateType, InvalidCloneException.Cause cause)`.

Pass `Cause.EmptyGuid` when the clone's `Guid` is empty, and `Cause.EqualGuid` when it matches the original.
`StateTransactionBehavior` chooses the cause. A custom `ICloneable.Clone` must construct the clone so `Guid` is
new. Mark `Guid` with `[IgnoreDataMember]` when the default cloner should leave it alone. That cloner also skips
`[NonSerialized]` and `[JsonIgnore]`.
