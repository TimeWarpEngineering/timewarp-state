---
uid: BlazorState:Overview.md
title: Blazor-State Overview
---

# Blazor-State
[![Build Status](https://timewarpenterprises.visualstudio.com/Blazor-State/_apis/build/status/Blazor-State-CI-Master-Yaml)](https://timewarpenterprises.visualstudio.com/Blazor-State/_build/latest?definitionId=7)
[![nuget](https://img.shields.io/nuget/v/Blazor-State)](https://www.nuget.org/packages/Blazor-State/)
[![nuget](https://img.shields.io/nuget/dt/Blazor-State?logo=nuget&style=flat-square)](https://www.nuget.org/packages/Blazor-State/)
[![Issues Open](https://img.shields.io/github/issues/TimeWarpEngineering/blazor-state.svg?style=flat-square&logo=github)](https://github.com/TimeWarpEngineering/blazor-state/issues)
[![Discord](https://img.shields.io/discord/715274085940199487?logo=discord)](https://discord.gg/7F4bS2T)
[![License](https://img.shields.io/github/license/TimeWarpEngineering/blazor-state.svg?style=flat-square&logo=github)](https://github.com/TimeWarpEngineering/blazor-state/issues)

Blazor-State is a State Management architecture utilizing the MediatR pipeline.

If you are familiar with MediatR <sup><a href="#footnotes">1</a></sup>, Redux <sup><a href="#footnotes">2</a></sup>,
or the Command Pattern <sup><a href="#footnotes">3</a></sup>
you will feel right at home.
All of the behaviors are written as plug-ins/middle-ware and attached to the MediatR pipeline.

Please see the **[GitHub Site](https://github.com/TimeWarpEngineering/blazor-state)** for source and filing of issues.

## Installation

Blazor-State is available as a [Nuget Package](https://www.nuget.org/packages/Blazor-State/)

```console
dotnet add package Blazor-State
```

## Getting Started

If you are just beginning with blazor then I recommend you first check out [getting started with blazor](https://docs.microsoft.com/en-us/aspnet/core/blazor/get-started).

The easiest way to get started with blazor-state is to create a new application based on the [timewarp-blazor template](https://timewarpengineering.github.io/timewarp-templates/TimeWarpBlazorTemplate/Overview.html)
Which gives you a base line for both client, server, and testing.

### Tutorial

If you would like a basic step by step on adding blazor-state to the `blazorwasm --hosted` template then follow this [tutorial](xref:BlazorState:Tutorial.md).

## Architecture

### Store 1..* State

Blazor-State Implements a single `Store` with a collection of `State`s.

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
Blazor-State utilizes the MediatR pipeline which allows for easy middleware integration
by registering an interface with the dependency injection container <sup><a href="#footnotes">4</a></sup>.
Blazor-State provides the extension method <sup><a href="#footnotes">5</a></sup> , `AddBlazorState`, which registers behaviors on the pipeline.

The three interfaces available to extend the Pipeline are `IPipelineBehavior`, `IRequestPreProcessor`,
and `IRequestPostProcessor`;

You can integrate into the pipeline as well by implementing and registering one of these interfaces.
See the timewarp-blazor template `EventStreamBehavior` for an example.

### Behaviors/Middleware

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

One of the nice features of redux is the developer tools <sup><a href="#footnotes">6</a></sup>.
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
[1] https://github.com/jbogard/MediatR

[2] https://redux.js.org/

[3] https://en.wikipedia.org/wiki/Command_pattern

[4] https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection

[5] https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods

[6] https://github.com/zalmoxisus/redux-devtools-extension)