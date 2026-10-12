---
uid: TimeWarpState:Release.12.0.0-beta.12.md
title: Release 12.0.0-beta.12
---

## Release 12.0.0-beta.12

### DevTools store identity

- **Each span names its store.** Init and action spans carry `timewarp.state.store`, the `Guid` created with that `Store`. When `AddTimeWarpStateBlazor` is registered, spans also carry `timewarp.state.render` (`prerender` or `interactive`). A host that cannot tell omits the render tag.
- **The inspector shows both init rows.** A Blazor Server page with prerendering builds two stores. Both initializations stay in the log. Each row has a badge: **pre**, **live**, or the first four characters of the store id. The colour stays with the store id. The Action tab shows the full id. The filter matches a store id or badge.
- **The State panel folds one store.** Current state is the store that first appeared latest in the log. A selector lists stores newest first, so a prerender snapshot does not replace the live store.

See [DevTools](xref:TimeWarpState:DevTools.md).

### State registration and pipeline log

- **States are registered by the clone source generator.** `AddTimeWarpState` applies `TryAddTransient<T>()` for the concrete states in `TimeWarpStateOptions.Assemblies`. The assembly type scan is gone. A host registration of the same service is kept. See [Cloning](xref:TimeWarpState:Cloning.md).
- **The pipeline log uses generated behavior definitions.** `LogTimeWarpStateMiddleware` still lists closed behavior registrations in service-collection order. `GetComponentOrder` keeps the same signature. A behavior compiled without the generator is omitted.
- **Behavior change: open generic `ICloneable` states.** They used to be registered as the open definition, so they resolved for any `T`. Now only closed constructions written in the same compilation are registered.
- **Behavior change: closed constructions follow the naming assembly.** A closed construction is registered under the assembly that names it. The host must list that assembly in `TimeWarpStateOptions.Assemblies`.
- **Behavior change: inaccessible states.** Private, protected, and file-local states are no longer registered.
- **Behavior change: the generator is required.** The assembly that declares states must be compiled with the TimeWarp.State source generator. `PackageReference` consumers get it automatically. A `ProjectReference` to `timewarp-state.csproj` must also reference the generator as an Analyzer (see `source/timewarp-state-telemetry/timewarp-state-telemetry.csproj`). An assembly without the generator registers no states.
- **No public API was removed.** `GetComponentOrder` and `AddTimeWarpState` keep their signatures.
