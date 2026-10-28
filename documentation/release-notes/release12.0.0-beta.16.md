---
uid: TimeWarpState:Release.12.0.0-beta.16.md
title: Release 12.0.0-beta.16
---

## Release 12.0.0-beta.16

### TimeWarp DevTools

- The inspector product is **TimeWarp DevTools**. State is the label on the existing action log. The browser tab title is **TimeWarp DevTools**.
- **Breaking:** `AddTimeWarpStateDevTools()` and `AddTimeWarpStateDevToolsProject()` default the Aspire resource name to `timewarp-devtools`. The previous default was `timewarp-state-devtools`. A caller that passes `name` still uses that name.
- **Breaking:** the dashboard link text is **TimeWarp DevTools**. Endpoint name `http` and `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` are unchanged.
- Package ids, namespaces, and the `AddTimeWarpStateDevTools` API are unchanged.

timewarp-architecture must update its tests when it takes this package. The IngressSmoke `StateDevTools` test, `Publish_Should_HaveNoStateDevTools`, and any match on the previous resource name or the previous dashboard label need the new values.

### TimeWarp.Mediator 15.0.0-beta.2

Pins are `TimeWarp.Mediator` and `TimeWarp.Mediator.Generators` at `15.0.0-beta.2`. `TimeWarp.Mediator.Contracts` and `TimeWarp.Mediator.Analyzers` are not package ids. Contracts and the analyzer ship in `TimeWarp.Mediator`. A library that must not emit a mediator references `TimeWarp.Mediator` only. A host that emits a mediator also references `TimeWarp.Mediator.Generators`.

Migration: [TimeWarp.Mediator 15 migration](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/v15.0.0-beta.2/documentation/migration.md).

- **Breaking:** `ActionHandler<T>.Handle` returns `ValueTask`. Pipeline behaviors return `ValueTask<TResponse>`, take `MessageHandlerDelegate<TRequest, TResponse>`, and call `next(request, cancellationToken)`.
- **Breaking:** `AddGeneratedMediator<TScope>()` is `internal` in `TimeWarp.Mediator.Generated` (or in `TimeWarpMediatorNamespace` when that property is set). Import that namespace. Implicit usings do not include it.
- **Breaking:** `LogTimeWarpStateMiddleware` takes a required `mediatorManifestJson` argument, the host's `MediatorManifest.Json`. The generated manifest type is internal to the emitting assembly, so a library passes the JSON string.
- **Breaking:** `PipelineBehaviorRegistry.Register` and `PipelineBehaviorRegistrySourceGenerator` are removed. `PipelineBehaviorRegistry.Read` accepts manifest version 2 JSON (kind, scope, behaviors in pipeline order). A `profile` key is rejected.
- **Breaking:** `GetComponentOrder` keeps its signature and reads the manifest last passed to `Read` or `LogTimeWarpStateMiddleware`. `IPipelineBehavior<,>` returns distinct behavior simple names in pipeline order. Any other component type returns an empty list. The service collection is not scanned.
