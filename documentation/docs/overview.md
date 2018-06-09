# Blazor-State

[Project Site](https://github.com/TimeWarpEngineering/blazor-state)

Blazor-State is a client side pipeline architecture utilizing MediatR with plugin behaviors.  
If you are familiar with 
[MediatR](https://github.com/jbogard/MediatR]),
 [Redux](https://redux.js.org/), 
or the [Command Pattern](https://en.wikipedia.org/wiki/Command_pattern) 
you will feel right at home.
All of the behaviors are written as plug-ins/middle-ware and attached to the MediatR pipeline. 
You can pick and choose which behaviors you would like to use or even write your own.

## Installation

Blazor-State is available as a [Nuget Package](https://www.nuget.org/packages/Blazor-State/)

## Getting Started

The easiest way to get started is to down load the repository and take a look at the [sample](BlazorStateSample:01:README.md) project.

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

"Don't mutate state" is the Redux mantra. Always return a new state.
This behavior handles that for you by creating a clone of the `State` 
prior to processing the `Request`
and updating `State` upon completion.  (Single responsibility principle)

#### ReduxDevToolsBehavior

One of the nice features of redux was the 
[developer tools](httpshttps://github.com/zalmoxisus/redux-devtools-extension).
They allow for the monitoring of State transitions, time travel and more.
This behavior implements the integration of these developer tools. 

Redux Devtools implemented feautres:

* Actions are logged to the Redux developer tools.

* Time Travel is supported.  

Other developer tools commands are not yet implemented (Would be good spot for a PR)

[!include[Terminology](terminology.md)]

### PureFunctions vs NonPureFunctions:
Blazor-State does not distinguish these.
As they are processed via the pipeline the same.
Thus, async calls to fetch data, send emails, or just update local state
are implemented in the same manner. Although the developer should be aware that Handlers have side effects and 
if the developer chose they could mark the Requests as such. i.e. IRequestWithSideEffect

## Acknowlegements
Jimmy Bogard (MediatR). Jimmy is an amazing developer and a knowledge sharer.  
Through his course at https://11xengineering.com/ , 
his many blog posts on Los Techies and now https://jimmybogard.com/. 
I have learned great amounts.

Peter Morris ([Blazor-Fluxor](https://github.com/mrpmorris/blazor-fluxor)). Pete and I 
have been friends for many years and he is an amazing developer and person who has taught me much.
Not surprisingly Pete and I think much alike. 
We independently started working on our Redux replacement
components. Although I started first :P (By like a few days)
Pete's component attempts to solve most of the same problems.
Blazor-State draws on the strengths of a proven pipeline in MediatR where as Fluxor 
implements its own middle-ware.  
If Blazor-State does not meet your needs be sure to checkout Fluxor.

Tor Hovland ([Blazor-Redux](https://github.com/torhovland/blazor-redux)).
I have only know Tor for a short time via the gitter on Blazor but he is already stimulating ideas.
If you use F# and need redux like functionality checkout his library.

## License

MIT

## Contributing

Time is of the essence.  Before developing a Pull Request I recommend opening an issue for discussion.

Please feel free to make suggestions and help out with the DocFx documentation.
Refer to [Markdown](http://daringfireball.net/projects/markdown/) for how to write markdown files.
