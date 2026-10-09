# Fix bugs found during 099 region backfill

## Description

While writing Purpose/Design regions for task 099 (PR #618, branch
`task/099-backfill-purpose-and-design-regions-on-all-c-files`), four defects turned up in library source. PR #618
only documents them as they are; it fixes nothing. This task fixes them.

All line numbers below are from `origin/master` at `7eb5d9a4`. On the 099 branch (`2f54c65b`) the same lines move
down by 9 because of the added regions (8 lines plus a blank).

### 1. `TimeWarpCacheableState<TState>` passes itself, not `TState`, to `State<>`

`source/timewarp-state-plus/state/timewarp-cacheable-state.cs:3` (099 branch: line 12):

```csharp
public abstract class TimeWarpCacheableState<TState> : State<TimeWarpCacheableState<TState>>, ITimeWarpCacheableState
where TState : IState
```

`State<TState>` (`source/timewarp-state/state/state.cs:17-18`) is CRTP, `where TState : State<TState>`, and
implements `IState<TState>` whose `Hydrate` returns `TState` (`source/timewarp-state/state/i-state.cs:21-30`). So a
derived state such as `CacheableWeatherState : TimeWarpCacheableState<CacheableWeatherState>`
(`tests/test-app/test-app-client/features/cacheable-weather/cacheable-weather-state.cs:5`) ends up as
`IState<TimeWarpCacheableState<CacheableWeatherState>>`, and its `Hydrate` returns the abstract base type instead
of `CacheableWeatherState`. The `TState` parameter is otherwise unused.

**It may be a deliberate workaround.** `StateInheritanceAnalyzer`
(`source/timewarp-state-analyzer/state-inheritance-analyzer.cs:52-64`, rule `StateInheritanceTypeArgumentRule`,
severity Error) requires the `State<T>` type argument to equal the declaring class. The analyzer runs on
`timewarp-state-plus` (`source/timewarp-state-plus/timewarp-state-plus.csproj`, ProjectReference with
`OutputItemType="Analyzer"`). Changing the base to `State<TState>` would raise that error on
`TimeWarpCacheableState` unless the analyzer is taught to accept an abstract intermediate base whose type argument
is its own self-constrained type parameter. The line has been unchanged since at least the kebab-case rename
(`017a4ba5`, 2025-08-20).

**What depends on it:** `ITimeWarpCacheableState` (`source/timewarp-state-plus/state/i-timewarp-cacheable-state.cs`)
does not depend on the generic argument. Consumers in the repo are only `CacheableWeatherState` and its tests
(`tests/client-integration-tests/caching/cacheable-state-tests.cs`,
`tests/test-app-end-to-end-tests/cacheable-weather-page-tests.cs`). Runtime callers use the concrete type
(`Store.GetState<CacheableWeatherState>()`), and Redux DevTools time travel finds `Hydrate` by name through
reflection (`source/timewarp-state/store/store.redux-dev-tools.cs:82-83`), so the return type change doesn't
affect it.

**Breaking or not:** the fix is source and binary breaking for the public `TimeWarp.State.Plus` package, though
narrowly:
- The constraint must tighten from `where TState : IState` to `where TState : TimeWarpCacheableState<TState>`
  (needed to satisfy `State<TState>`'s constraint). Any consumer passing a type other than itself stops compiling.
- `IState<T>` / `Hydrate` return types change for derived states. Consumers overriding `Hydrate` with return type
  `TimeWarpCacheableState<X>` must change it to `X`.
- Correctly written `class X : TimeWarpCacheableState<X>` states need no change. Fine for the 12.0 beta line; call
  it out in release notes.

### 2. `InvalidCloneException` message describes the wrong cause

`source/timewarp-state/features/pipeline/invalid-clone-exception.cs:3-9` (099 branch: 12-18). The message is:
"State of type {T} has an invalid clone. For the default clone to work, a parameterless constructor is required."

The only throw site is `source/timewarp-state/features/pipeline/state-transaction-behavior.cs:88-91` (same on the
099 branch):

```csharp
if (newState.Guid == Guid.Empty || originalState.Guid == newState.Guid)
{
  throw new InvalidCloneException(enclosingStateType);
}
```

Since 12.0.0-beta.9 the deep cloner doesn't need a parameterless constructor (it falls back to the fewest-parameter
ctor, then `GetUninitializedObject`). The real causes are:
- **Guid not regenerated.** The clone came from `GetUninitializedObject`, so no initializer ran and `Guid` is
  empty. Another cause is a custom `ICloneable` that builds the instance without running `State`'s initializer.
- **Guid copied.** A custom `ICloneable.Clone` (for example `MemberwiseClone`) or a removed or renamed
  `[IgnoreDataMember]` copied the original `Guid`.

### 3. `StartHandler` docs say it logs; it logs with the wrong EventId and `Handle` is a no-op

`source/timewarp-state/features/redux-dev-tools/requests/start/start-handler.cs` (099 branch: +9 lines):
- Line 6: `<remarks>currently we do nothing at start up other than log</remarks>`, but `Handle` (line 26) is
  `=> Task.CompletedTask` and logs nothing.
- Line 17: the constructor logs `Logger.LogDebug(EventIds.JumpToStateHandler_RequestHandled, "constructing")`. That
  is EventId 512 (`source/timewarp-state/event-ids.cs:49`), which looks copy-pasted. The matching IDs already exist
  and are unused: `StartHandler_Initializing` (500), `StartHandler_RequestReceived` (501) and
  `StartHandler_RequestHandled` (502) at `event-ids.cs:43-45`.
- Existing coverage: `tests/timewarp-state-tests/javascript-interop/json-request-handler-dispatch-tests.cs` references
  `StartRequest` dispatch.

Decide whether `Handle` should log (for example `StartHandler_RequestReceived` at Debug), then make the docs match
whatever it actually does.

### 4. `FeatureFlagState` is a public placeholder that throws

`source/timewarp-state-plus/features/feature-flags/feature-flag-state/feature-flag-state.cs:10` (099 branch: 19):
`public override void Initialize() => throw new NotImplementedException();`. It is `public sealed`, ships in
`TimeWarp.State.Plus`, has no actions or members, and is in namespace
`TimeWarp.State.Plus.Features.FeatureFlags.Actions`, which is also odd for a state. Nothing in source or tests
references it. `documentation/migrations/migration10-11.md:122-129` advertises `options.UseFeatureFlags()`, but no
such method exists anywhere in the repo, so the docs are wrong too. Any consumer whose store reaches this state's
`Initialize` (for example `Store.GetState<FeatureFlagState>()` or a reset) gets `NotImplementedException`.

## Requirements

- Fix all four items, or for item 4 apply the option Steven picks. Each fix gets tests that fail before and pass
  after.
- Behavior changes outside these four items aren't allowed. Public API changes (items 1 and 4) go in the
  release notes and migration docs.
- Once fixed, update the 099 Purpose/Design regions in the touched files to describe the new behavior. The
  current regions describe the bugs, for example "reusing the JumpToStateHandler EventId" and "a fixed message that
  points at the parameterless-constructor requirement". If PR #618 isn't merged yet, rebase on it first.
- Normal hooks on every commit (no `--no-verify`). The build stays warning-free and the full test workflow passes.

## Checklist

### 1. TimeWarpCacheableState base type
- [ ] Change the base to `State<TState>` with `where TState : TimeWarpCacheableState<TState>`.
- [ ] Teach `StateInheritanceAnalyzer` to accept an abstract base whose `State<>` type argument is its own type
  parameter constrained to itself. Keep reporting the error for concrete classes.
- [ ] Tests: an analyzer test in `tests/timewarp-state-analyzer-tests` (abstract self-constrained intermediate base
  is allowed; a concrete wrong-argument class still errors). A test that `CacheableWeatherState` is
  `IState<CacheableWeatherState>` and that `Hydrate`'s return type is `CacheableWeatherState`. Existing
  `cacheable-state-tests.cs` and `cacheable-weather-page-tests.cs` still pass.
- [ ] Release note: constraint tightened, `IState<T>` argument changed for cacheable states.

### 2. InvalidCloneException message
- [ ] Rewrite the message to give the real cause and the fix: the clone's Guid is empty (state initializer didn't
  run, for example an uninitialized instance or a custom `ICloneable` that skips construction) or equals the
  original (custom `ICloneable` or `MemberwiseClone` copied `Guid`, or `[IgnoreDataMember]` missing). Point the
  user at `ICloneable` and the opt-out attributes. Consider passing which case happened (empty vs. equal) into
  the exception.
- [ ] Tests in `tests/timewarp-state-tests/pipeline/state-transaction-behavior-tests.cs`: a state whose
  `ICloneable.Clone` returns `MemberwiseClone()` throws `InvalidCloneException` with the "equal Guid" wording, and
  a clone with `Guid.Empty` throws with the "empty Guid" wording. Both assert `EnclosingStateType`.

### 3. StartHandler
- [ ] Replace the constructor's `JumpToStateHandler_RequestHandled` with `StartHandler_Initializing`, or drop the
  constructor log.
- [ ] Decide whether `Handle` logs `StartHandler_RequestReceived`. Make the `<summary>`/`<remarks>` match.
- [ ] Tests: with a capturing `ILogger<StartHandler>`, assert the EventIds used (no 512), and that `Handle`
  completes synchronously.

### 4. FeatureFlagState
- [ ] Steven picks one option (see Notes): implement, mark `[Experimental]` / `[Obsolete]`, or remove from the
  public package.
- [ ] Fix or remove the `UseFeatureFlags()` section in `documentation/migrations/migration10-11.md`.
- [ ] Tests for the chosen option: if implemented, `Initialize` sets defaults and actions update flags. If marked,
  a test or analyzer check that the attribute is present. If removed, a public-API or architecture test that the
  type is gone.

### Wrap-up
- [ ] Update the Purpose/Design regions in all four files (and `state-inheritance-analyzer.cs` if it changes).
- [ ] Full workflow green. PR links this task and #618.

## Notes

- Source: findings from PR #618 (099 part 1), reported to Steven on 2026-10-09.
- Options for item 4:
  - **Implement it.** Needs a design (flag source, actions, `UseFeatureFlags` registration). Biggest scope; could
    split into its own task.
  - **Mark `[Experimental("TWS…")]` or `[Obsolete]`.** Keeps binary compatibility and warns consumers. Still ships
    a type that throws, so `Initialize` should at least stop throwing (empty body).
  - **Remove it from the public package.** Delete, or make it `internal`. Breaking only for anyone referencing a
    type that can't work today. Low real-world risk during the 12.0 beta line.
- Item 1 may have been intentional to satisfy `StateInheritanceTypeArgumentRule`. If Steven prefers not to touch
  the analyzer, a smaller fix is to drop the unused `TState` parameter from the class, but that breaks every
  derived state's declaration, so the analyzer change is likely the better path.
- Task 097 (source-generator cloning proposal) uses the Guid check from item 2. Keep its wording consistent with
  the new exception message.

## Session

- Created: 1075791 (2026-10-09)
