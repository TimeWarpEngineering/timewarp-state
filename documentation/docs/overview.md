# Blazor-State

Blazor-State is a client side pipeline architecture utilizing MediatR with plugin behaviors.  

If you are familiar with 
[MediatR](https://github.com/jbogard/MediatR),
 [Redux](https://redux.js.org/), 
or the [Command Pattern](https://en.wikipedia.org/wiki/Command_pattern) 
you will feel right at home.
All of the behaviors are written as plug-ins/middle-ware and attached to the MediatR pipeline. 
You can pick and choose which behaviors you would like to use or even write your own.

Please see our **[GitHub Site](https://github.com/TimeWarpEngineering/blazor-state)** for source and filing of issues.

## Installation

Blazor-State is available as a [Nuget Package](https://www.nuget.org/packages/Blazor-State/)

```
dotnet add package Blazor-State
```

## Getting Started

After you have completed the [getting started for blazor](https://docs.microsoft.com/en-us/aspnet/core/client-side/spa/blazor/get-started)
the easiest way to get started with blazor-state is to follow the [tutorial](xref:BlazorStateSample:README.md).

Or create new application based on the [timewarp-blazor template](Template/TemplateOverview.md)

## Architecture

Blazor-State Implements a single `Store` with a collection of `State`s.
The MediatR pipeline allows for easy integration 
by registering an interface with the DI container.

```csharp
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CloneStateBehavior<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DevToolsBehavior<,>));
```
The three interfaces available to extend the Pipeline are `IPipelineBehavior`, `IRequestPreProcessor`,
and `IRequestPostProcessor`;

All of the behaviors in Blazor-State are implemented via one of these interfaces.
You can integrate into the pipeline as well by implementing and registering one of these interfaces.

### JavaScript Interop
Blazor-State uses the same "Command Pattern" for JavaScript interoperability.
The JavaScript creates a request and dispatches it to Blazor where it is added to the pipeline.
Handlers on the Blazor side can callback to the JavaScript side if needed.

### Behaviors

#### CloneStateBehavior

"Don't mutate state" always return a new state.
The `CloneStateBehavior` behavior handles that for you by creating a clone of the `State` 
prior to processing the `Request`
and updating `State` upon completion.  (Single responsibility principle)

#### ReduxDevToolsBehavior

One of the nice features of redux is the 
[developer tools](https://github.com/zalmoxisus/redux-devtools-extension).
They allow for the monitoring of State transitions, time travel and more.
This behavior implements the integration of these developer tools. 

Redux Devtools implemented feautres:

* Actions are logged to the Redux developer tools.

* Time Travel is supported.  

Other developer tools commands are not yet implemented (Would be good spot for a PR)

[!include[Terminology](../terminology.md)]

### PureFunctions vs NonPureFunctions:
Blazor-State does not distinguish these.
As they are processed via the pipeline the same.
Thus, async calls to fetch data, send emails, or just update local state
are implemented in the same manner. Although the developer **should** be aware that Handlers have side effects and 
if the developer chose they could mark the Requests as such. i.e. IRequestWithSideEffect

[!include[Contributing](../acknowledgements.md)]

## License

[The Unlicense](https://choosealicense.com/licenses/unlicense/)

[!include[Contributing](../contributing.md)]


2019-01-05 12:59:58