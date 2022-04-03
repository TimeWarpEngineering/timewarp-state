---
uid: BlazorState:Overview.md
title: Blazor-State Overview
---

[![Dotnet](https://img.shields.io/badge/dotnet-6.0-blue)](https://dotnet.microsoft.com)
[![Stars](https://img.shields.io/github/stars/TimeWarpEngineering/blazor-state?logo=github)](https://github.com/TimeWarpEngineering/blazor-state)
[![Discord](https://img.shields.io/discord/715274085940199487?logo=discord)](https://discord.gg/7F4bS2T)
[![workflow](https://github.com/TimeWarpEngineering/blazor-state/actions/workflows/release-build.yml/badge.svg)](https://github.com/TimeWarpEngineering/blazor-state/actions)
[![nuget](https://img.shields.io/nuget/v/Blazor-State?logo=nuget)](https://www.nuget.org/packages/Blazor-State/)
[![nuget](https://img.shields.io/nuget/dt/Blazor-State?logo=nuget)](https://www.nuget.org/packages/Blazor-State/)
[![Issues Open](https://img.shields.io/github/issues/TimeWarpEngineering/blazor-state.svg?logo=github)](https://github.com/TimeWarpEngineering/blazor-state/issues)
[![Forks](https://img.shields.io/github/forks/TimeWarpEngineering/blazor-state)](https://github.com/TimeWarpEngineering/blazor-state)
[![License](https://img.shields.io/github/license/TimeWarpEngineering/blazor-state.svg?style=flat-square&logo=github)](https://github.com/TimeWarpEngineering/blazor-state/issues)
[![Twitter](https://img.shields.io/twitter/url?style=social&url=https%3A%2F%2Fgithub.com%2FTimeWarpEngineering%2Fblazor-state)](https://twitter.com/intent/tweet?url=https://github.com/TimeWarpEngineering/blazor-state)

[![Twitter](https://img.shields.io/twitter/follow/StevenTCramer.svg)](https://twitter.com/intent/follow?screen_name=StevenTCramer)
[![Twitter](https://img.shields.io/twitter/follow/TheFreezeTeam1.svg)](https://twitter.com/intent/follow?screen_name=TheFreezeTeam1)

# Blazor-State

![TimeWarp Logo](Assets/Logo.png)

Blazor-State is a State Management architecture utilizing the MediatR pipeline.

If you are familiar with MediatR [^1], Redux [^2],
or the Command Pattern [^3]
you will feel right at home.
All of the behaviors are written as middleware to the MediatR pipeline.

Please see the **[GitHub Site](https://github.com/TimeWarpEngineering/blazor-state)** for source and filing of issues.

## Installation

You can see the latest NuGet packages from the official [TimeWarp NuGet page](https://www.nuget.org/profiles/TimeWarp.Enterprises).

* [Blazor-State](https://www.nuget.org/packages/Blazor-State/) [![nuget](https://img.shields.io/nuget/v/Blazor-State?logo=nuget)](https://www.nuget.org/packages/Blazor-State/)

```console
dotnet add package Blazor-State
```

## Getting Started

If you are just beginning with Blazor then I recommend you start at the [dotnet blazor site](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor).

If you already know a bit about Blazor then I recommend the [tutorial](xref:BlazorState:Tutorial.md)

### Tutorial

If you would like a basic step by step on adding blazor-state to the `blazorwasm --hosted` template then follow this [tutorial](xref:BlazorState:Tutorial.md).

### TimeWarp Architecture Template

To create a distributed application that utilizes blazor-state see the [timewarp-architecture template.](https://timewarpengineering.github.io/timewarp-architecture/TimeWarpBlazorTemplate/Overview.html)


## The Blazor-State Architecture

### Store 1..* State

Blazor-State implements a single `Store` with a collection of `State`s.

To access a state you can either inherit from the BlazorStateComponent and use

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

Blazor-State utilizes the MediatR pipeline which allows for middleware integration
by registering an interface with the dependency injection container [^4].
Blazor-State provides the extension method [^5] , `AddBlazorState`, which registers behaviors on the pipeline.

The interfaces available to extend the Pipeline are:

* `IPipelineBehavior`
* `IRequestPreProcessor`
* `IRequestPostProcessor` 
* `IStreamPipelineBehavior`

You can add functionality to the pipeline by implementing and registering one of these interfaces.
See the [timewarp-architecture `EventStreamBehavior`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/Source/TimeWarp.Architecture.Template/templates/TimeWarp.Architecture/Source/ContainerApps/Web/Web.Spa/Features/EventStream/Pipeline/EventStreamBehavior.cs) for an example.

### Behaviors/Middleware

Blazor-State ships with the following default middleware.

#### CloneStateBehavior

To ensure your application is in a known good state the `CloneStateBehavior` creates a clone of the `State` prior to processing the `Action`.
If any exception occurs during the processing of the `Action` the state is rolled back.

#### RenderSubscriptionsPostProcessor

When a component accesses `State` a subscription is added.
The `RenderSubscriptionsPostProcessor` will iterate over these subscriptions and re-render those components.
So you don't have to worry about where to call `StateHasChanged`.

### JavaScript Interop

Blazor-State uses the same "Command Pattern" for JavaScript interoperability.
The JavaScript creates a request and dispatches it to Blazor where it is added to the pipeline.
Handlers on the Blazor side can callback to the JavaScript side if needed.

#### ReduxDevToolsPostProcessor

> [!NOTE]
> Disabled by default.  This should be disabled in production.

One of the nice features of redux is the developer tools [^6].
This processor implements the integration of these developer tools.

[!include[Terminology](Partials/terminology.md)]

### PureFunctions vs NonPureFunctions

Blazor-State does not distinguish between these.
As they are processed via the pipeline the same.
Thus, async calls to fetch data, send emails, or just update local state
are implemented in the same manner. Although the developer **should** be aware that Handlers have side effects and
if the developer chose they could mark the Requests as such. For example **IActionWithSideEffect**

[!include[Contributing](Partials/acknowledgements.md)]

## UnLicense

[The Unlicense](https://choosealicense.com/licenses/unlicense/)

[!include[Contributing](Partials/contributing.md)]

#### Footnotes:
[^1]: https://github.com/jbogard/MediatR

[^2]: https://redux.js.org/

[^3]: https://en.wikipedia.org/wiki/Command_pattern

[^4]: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection

[^5]: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods

[^6]: https://github.com/zalmoxisus/redux-devtools-extension)
