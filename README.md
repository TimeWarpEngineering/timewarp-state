# Blazor-State

Blazor-State is a client side pipeline architecture utilizing MediatR with plugin behaviors.  
If you are familiar with 
[MediatR](https://github.com/jbogard/MediatR]),
 [Redux](https://redux.js.org/), 
or the [Command Pattern](https://en.wikipedia.org/wiki/Command_pattern]) 
you will feel right at home.
All of the behaviors are written as plug-ins/middle-ware and attached to the MediatR pipeline. 
You can pick and choose which behaviors you would like to use or even write your own.

## Installation

Blazor-State is available as a pre-release [Nuget Package](https://www.nuget.org/packages/Blazor-State/)

## Getting Started

Easiest Way to get started is to down load the repository and take a look at the sample.

A step by step guide will be created when things stabilize a bit.

## Behaviors

The MeditR pipeline allows for easy integration 
by just registering an interface with the DI container.

```csharp
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CloneStateBehavior<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DevToolsBehavior<,>));
```
The three interfaces available are `IPipelineBehavior`, `IRequestPreProcessor`,
and `IRequestPostProcessor`;

All of the behaviors in Blazor-State are implemented via these interfaces.

By implementing one of these you could integrate into the pipeline as well.

For example if you wanted to Log every `Request` you could implement a "LoggingBehavior".

### Blazor-State 

Implements a single `Store` with a collection of `State`s.
And updates the `State` in the `Store` upon completion of 
the `Request` being handled.

This is the basis of Redux.

### JavaScript Interop
Blazor-State uses the same Command Pattern for JavaScript interoperability.
The JavaScript creates a request and dispatches it to Blazor where it is put onto the pipeline.
Blazor side Handlers can callback to the JavaScript side if needed.

### CloneStateBehavior

"Don't mutate state" is the Redux mantra. Always return a new state.
This behavior handles that for you by creating a clone of the `State` 
prior to processing the `Request`
and updating `State` upon completion.  (Single responsibility principle)


### ReduxDevToolsBehavior

One of the nice features of redux was the 
[developer tools](httpshttps://github.com/zalmoxisus/redux-devtools-extension).  
They allow for the monitoring of State transitions, time travel and more.
This behavior implements the integration of these developer tools. 

Actions are logged to the Redux developer tools.

Time Travel is supported.  
Other developer tools commands are not yet implemented (Would be good spot for a PR)


## Terminology

This pattern or similar has been around for many years and goes by different names.
Given we are using MediatR will use those names but list other terminology here.

### Signals/Actions/**Requests**/Commands/

In Redux they call them "Action".  
In UML they are "Signal".  
In Command Pattern they are "Command"
In MediatR they are `Request`

### Reducer/**Handler**/Executor

In Redux they call them "Reducer". 
(State in State out doesn't reduce anything but they still call them that)
In Command Pattern we call them "Executor".
In MediatR they are `Handler`.

### Feature

A Feature is a the collection of the code needed to implement a 
particular [Vertical Slice](https://jimmybogard.com/vertical-slice-architecture/)
of the application.  

On the server side we use the same architecture, 
(see sample in Hosted Server), where the Features contain 
`Controller`, `Handler`, `Request`, `Response`, etc...

Each endpoint has its own controller 
which maps the HTTP Request to the `Request` Object and then sends 
it to the mediator. 
The `Handler` acts on the `Request` and returns a `Response`. 

Server side follows the `Request` in `Response` out pattern.

A Feature Folder on the client side will contain a `Request` and the `Handler` 
and any corresponding files needed for this feature.
The "Response" of client side feature is its `State`.

Client side follows the `State` in new `State` out pattern.

## 
PureFunctions vs NonPureFunctions:
Blazor-State doesn't distinguish these.
As they are processed via the pipeline the same.
Thus, async calls to fetch data, send emails, or just update local state
are implemented in the same manner.

The developer should be aware that Handlers have side effects and 
if the developer chose could mark the Requests as such. i.e. IRequestWithSideEffect

# Acknowlegements:
Jimmy Bogard (MediatR) Jimmy is an amazing developer and a knowledge sharer.  
Through his course at https://11xengineering.com/ , 
his many blog posts on Los Techies and now https://jimmybogard.com/. 
I have learned great amounts.

Peter Morris ([Blazor-Fluxor](https://github.com/mrpmorris/blazor-fluxor)) and I 
have been friends for many years and he is an amazing developer and person who has taught me much.
Not surprisingly Pete and I think much alike. 
We independently started working on our Redux replacement
components. Although I started first :P (By like a few days)
Pete's component attempts to solve most of the same problems.
Blazor-State draws on the strengths of a proven pipeline in MediatR where as Fluxor 
implements its own middle-ware.  
If Blazor-State does not meet your needs be sure to checkout Fluxor.

Tor Hovland ([Blazor-Redux](https://github.com/torhovland/blazor-redux)) 
I have only know Tor for a short time via the gitter on Blazor but he is already stimulating ideas.
If you use F# and need redux like functionality checkout his library.

# License

MIT

# Contributing

Time is of the essence.  Before writing a PR I recommend opening an issue for discussion.