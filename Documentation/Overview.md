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

**TimeWarp.State** utilizes the MediatR pipeline which allows for middleware integration
by registering an interface with the dependency injection container [^4].
TimeWarp.State provides the extension method [^5] , `AddTimeWarpState`, which registers behaviors on the pipeline.

The interfaces available to extend the Pipeline are:

* `IPipelineBehavior`
* `IRequestPreProcessor`
* `IRequestPostProcessor` 
* `IStreamPipelineBehavior`

You can add functionality to the pipeline by implementing and registering one of these interfaces.
See the [timewarp-architecture `EventStreamBehavior`](https://github.com/TimeWarpEngineering/timewarp-state/blob/9e316ecaa00f21383caf4d120ad95d968b3e9dd6/Tests/Test.App/Test.App.Client/Features/EventStream/Pipeline/EventStreamBehavior.cs) for an example.

### Behaviors/Middleware

TimeWarp.State ships with the following default middleware.

#### CloneStateBehavior

To ensure your application is in a known good state the `CloneStateBehavior` creates a clone of the `State` prior to processing the `Action`.
If any exception occurs during the processing of the `Action` the state is rolled back.

#### RenderSubscriptionsPostProcessor

When a component accesses `State`, a subscription is added.
The `RenderSubscriptionsPostProcessor` will iterate over these subscriptions and re-render those components that return true for ShouldReRender.
So you don't have to worry about where to call `StateHasChanged` and still have the ability to finely control re-rendering.

### JavaScript Interop

TimeWarp.State also uses the same "Command Pattern" for JavaScript interoperability.
The JavaScript creates a request and dispatches it to Blazor where it is added to the pipeline.
Handlers on the Blazor side can callback to the JavaScript side if needed.

#### ReduxDevToolsPostProcessor

> [!NOTE]
> Disabled by default.  This should be disabled in production as it consumes significant resources.

One of the nice features of redux is the developer tools [^6].
This processor implements the integration of these developer tools.

[!include[Terminology](Partials/terminology.md)]

### PureFunctions vs NonPureFunctions

TimeWarp.State does not distinguish between these.
As they are processed via the pipeline the same.
Thus, async calls to fetch data, send emails, or just update local state
are implemented in the same manner. Although the developer **should** be aware when Handlers have side effects and
if the developer chose they could mark the Requests as such. For example **IActionWithSideEffect**

[!include[Acknowledgements](Partials/Acknowledgements.md)]

#### Footnotes:

[^1]: https://github.com/jbogard/MediatR

[^2]: https://redux.js.org/

[^3]: https://en.wikipedia.org/wiki/Command_pattern

[^4]: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection

[^5]: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods

[^6]: https://github.com/reduxjs/redux-devtools
