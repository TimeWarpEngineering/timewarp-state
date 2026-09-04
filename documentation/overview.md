---
uid: TimeWarpState:Overview.md
title: TimeWarp.State Overview
---
[!include[Badges](Partials/Badges.md)]

[!include[Installation](Partials/Summary.md)]

Please see the **[GitHub Site](https://github.com/TimeWarpEngineering/timewarp-state)** for source and filing of issues.

[!include[Installation](Partials/Installation.md)]

[!include[Installation](Partials/GettingStarted.md)]

## The TimeWarp.State Architecture

### Store 1..* State

TimeWarp.State implements a single `Store` with a collection of `State`s.

To access a state you can either inherit from the TimeWarpStateComponent and use

```csharp
Store.GetState<YourState>()
```

or move the GetState functionality into your component

```csharp
  protected T GetState<T>()
  {
    Type stateType = typeof(T);
    Subscriptions.Add(stateType, this);
    return Store.GetState<T>();
  }
```

### Pipeline

**TimeWarp.State** uses **TimeWarp.Mediator 14.0.0-beta.1** with compile-time generated registration and named pipelines — not MediatR, and not reflection-based `AddMediator()`.

Hosts reference `TimeWarp.Mediator.Generators` and call `AddGeneratedMediator<ClientPipeline>()` for the Blazor/WASM client store pipeline. Separate server API hosts that handle requests in their own compilation unit call `AddGeneratedMediator<ServerPipeline>()` and send with `ISender<ServerPipeline>`.

Library membership is declared with assembly attributes:

* `[assembly: MediatorAssembly]`
* `[assembly: MediatorScope(typeof(ClientPipeline))]`
* `[assembly: MediatorBehavior(...)]`

Hosts add their own behaviors with `[assembly: MediatorBehavior(typeof(MyBehavior<,>), order: 500+, Scope = typeof(ClientPipeline))]`.

`AddTimeWarpState` still configures options (Redux DevTools, assemblies) but does **not** register mediator pipeline behaviors.

Extend the pipeline by implementing `IPipelineBehavior<TRequest, TResponse>` (TimeWarp.Mediator).
See [`tests/test-app/test-app-client/features/event-stream/pipeline/event-stream-behavior.cs`](https://github.com/TimeWarpEngineering/timewarp-state/blob/master/tests/test-app/test-app-client/features/event-stream/pipeline/event-stream-behavior.cs) for an example.

### Behaviors/Middleware

TimeWarp.State ships with the following ClientPipeline middleware (declared via `[assembly: MediatorBehavior]` on the library).

#### StateTransactionBehavior

To ensure your application is in a known good state the `StateTransactionBehavior` creates a clone of the `State` prior to processing the `Action`.
If any exception occurs during the processing of the `Action` the state is rolled back.

#### RenderSubscriptionsPostProcessor

When a component accesses `State`, a subscription is added.
The `RenderSubscriptionsPostProcessor` will iterate over these subscriptions and re-render those components that return true for ShouldReRender.
So you don't have to worry about where to call `StateHasChanged` and still have the ability to finely control re-rendering.

#### ReduxDevToolsBehavior

> [!NOTE]
> Opt-in via `UseReduxDevTools`. Disabled by default. This should be disabled in production as it consumes significant resources.

One of the nice features of redux is the developer tools [^6].
This behavior implements the integration of these developer tools.

### JavaScript Interop

TimeWarp.State also uses the same "Command Pattern" for JavaScript interoperability.
The JavaScript creates a request and dispatches it to Blazor where it is added to the pipeline.
Handlers on the Blazor side can callback to the JavaScript side if needed.

[!include[Terminology](Partials/terminology.md)]

### PureFunctions vs NonPureFunctions

TimeWarp.State does not distinguish between these.
As they are processed via the pipeline the same.
Thus, async calls to fetch data, send emails, or just update local state
are implemented in the same manner. Although the developer **should** be aware when Handlers have side effects and
if the developer chose they could mark the Requests as such. For example **IActionWithSideEffect**

[!include[Acknowledgements](Partials/Acknowledgements.md)]

#### Footnotes:

[^1]: https://github.com/TimeWarpEngineering/timewarp-mediator

[^2]: https://redux.js.org/

[^3]: https://en.wikipedia.org/wiki/Command_pattern

[^6]: https://github.com/reduxjs/redux-devtools
