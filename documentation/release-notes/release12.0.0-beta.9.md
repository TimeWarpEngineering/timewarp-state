---
uid: TimeWarpState:Release.12.0.0-beta.9.md
title: Release 12.0.0-beta.9
---

## Release 12.0.0-beta.9

### Breaking changes

- **Targets .NET 11.** All four packages (TimeWarp.State, .Plus, .Policies, .Telemetry) now target `net11.0`
  and depend on `Microsoft.AspNetCore.*` / `Microsoft.Extensions.*` 11.0.0 (built against .NET 11 RC1,
  SDK `11.0.100-rc.1.26425.128`). Consuming apps must target `net11.0`.
- **AnyClone and TypeSupport removed.** On .NET 11, a blocking wait on single-threaded browser WebAssembly throws
  `PlatformNotSupportedException` (dotnet/runtime#123329). TypeSupport's type cache used `SemaphoreSlim.Wait`,
  which broke `StateTransactionBehavior` in WASM. Both packages are gone, and TimeWarp.State no longer brings in
  AnyClone transitively.

  Migration: if your code called AnyClone's `.Clone()`, change `using AnyClone;` to
  `using TimeWarp.Features.Cloning;`. `Clone<T>()` and `Clone<T>(CloneErrorHandler)` are provided there. The
  error handler signature is `(Exception exception, string path)`.

### Features

- New `TimeWarp.Features.Cloning` deep clone. It never blocks, so it is safe on single-threaded browser WASM.
  It handles cycles, shared references and multi-dimensional arrays. Like AnyClone, it skips
  `[IgnoreDataMember]` / `[NonSerialized]` / `[JsonIgnore]` members, which keep their constructor values.
  `StateTransactionBehavior` uses it by default; states that implement `ICloneable` still take precedence.

### Fixes

- TimeWarp.State.Plus again includes its static web assets (scoped CSS bundle, `js/download-file.js`) when
  packed with the .NET 11 SDK.
