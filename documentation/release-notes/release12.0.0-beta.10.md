---
uid: TimeWarpState:Release.12.0.0-beta.10.md
title: Release 12.0.0-beta.10
---

## Release 12.0.0-beta.10

### Breaking changes

- **Blazor features moved to `TimeWarp.State.Blazor`.** `TimeWarp.State` no longer references
  `Microsoft.AspNetCore.Components` or `Microsoft.JSInterop`. Components, JavaScript interop, Redux DevTools,
  render subscriptions, and `wwwroot` live in `TimeWarp.State.Blazor`. Namespaces are unchanged. Blazor hosts add a
  package reference and call `AddTimeWarpStateBlazor()` after `AddTimeWarpState()`. Console hosts call
  `AddTimeWarpState()` only. `TimeWarp.State.Plus` keeps its package id and references `TimeWarp.State.Blazor`.
  Static web assets stay under `/_content/TimeWarp.State/`, including `/_content/TimeWarp.State/js/timewarp-state.js`.
  Blazor loads `/_content/TimeWarp.State/js/TimeWarp.State.Blazor.lib.module.js`. See the
  [12.0.0-beta.10 migration](xref:TimeWarpState:Migration12.0.0-beta.10.md).

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

### Other changes

- Purpose and Design regions are backfilled on tracked C# files. TWA0004 (a source file must contain a
  `#region Purpose` block) is enabled through TimeWarp.Architecture.Analyzers 2.0.0-beta.19. Other TWA diagnostics
  stay off. `.githooks/**.cs` sets TWA0004 to none because ganda owns those files.

- NuGet package pins moved to the current latest, including TimeWarp.Amuru and TimeWarp.Amuru.Tools 2.0.0-beta.2,
  MSTest 4.5.1, Microsoft.Playwright.MSTest.v4 1.63.0, coverlet.collector 10.1.0, and Aspire.Hosting.AppHost 13.6.1.
  Microsoft.CodeAnalysis.CSharp stays 4.14.0. Shouldly stays 4.3.0.

### Fixes

- `StartHandler` logs `EventIds.StartHandler_Initializing` (500) from the constructor and
  `EventIds.StartHandler_RequestReceived` (501) when it handles the startup request. It no longer logs
  `JumpToStateHandler_RequestHandled` (512). `Handle` completes with no other work.
