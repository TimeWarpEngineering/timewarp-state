---
uid: BlazorStateSample:README.md
title: Blazor-State Sample Application
---

# Blazor-State Sample Application

This sample shows how to add Blazor-State to a `Blazor (ASP.NET Core hosted)` application.

## Prerequisites

1. Install the latest [.NET Core 3.0 Preview SDK release](https://dotnet.microsoft.com/download/dotnet-core/3.0).
2. Install the Blazor templates by running the following command in a command shell:

```console
dotnet new -i Microsoft.AspNetCore.Blazor.Templates::3.0.0-preview6.19307.2
```

## Creating the project

1. Create a new project `dotnet new blazorhosted -o Sample`
2. Change directory to the new project `cd Sample`
3. Run the default application and confirm it works.  
  `dotnet run --project .\Sample.Server\Sample.Server.csproj`
   > [!NOTE]
   > Notice that the counter resets on page changes.
   > There is currently no state being maintained.
   > When the counter page is no longer rendered it is freed by the `GarbageCollector`.
   > When returning to the counter route a new page is created
   > and therefore the count is back to zero.

## Add Blazor-State

Add the Blazor-State NuGet package to the `Sample.Client` project.
   `dotnet add .\Sample.Client\Sample.Client.csproj package Blazor-State --version 1.0.0-preview6.19307.2-*`

## Feature File Structure

With the mediator pattern for each `Request/Action` there is an associated `Handler`
and possibly other items like a `Validator`, `Mapper` etc...
These associated items are what we call a `Feature`.
Let's organize the Features by the State they act upon.

1. In the Client project add a folder named `Features`.

## Add CounterState

1. In the `Features` folder add a folder named `Counter`.
2. Within the `Counter` folder create a class file named `CounterState.cs`.

Your class should:

* be a partial class
* inherit from `State<CounterState>`
* override the `Initialize()` method. To set the initial values.

The only value we want to maintain is a Count.
The code for the class should be as follows.

```csharp
using BlazorState;

namespace Sample.Client.Features.Counter
{
    public partial class CounterState : State<CounterState>
    {
        public int Count { get; private set; }
        protected override void Initialize() => Count = 3;
    }
}
```

## Configure the services

1. In the `Sample.Client` project in the `Startup.cs` file.
2. Change `ConfigureServices` to configure blazor-state as follows:
3. Add the required usings.
4. Configure the options passed to AddBlazorState to include the assemblies to scan for Handlers.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBlazorState
    (
      (aOptions) => aOptions.Assemblies =
        new Assembly[]
        {
          typeof(Startup).GetTypeInfo().Assembly,
        }
    );
    services.AddScoped<CounterState>();
}
```

## Displaying state in the user interface

1. Edit `Pages\Counter.razor` as follows
2. Inherit from BlazorStateComponent `@inherits BlazorStateComponent`, to do that you need to also add `@using BlazorState`
3. Next add a `CounterState` property that gets the State from the store `GetState<CounterState>()`, this will require you add `@using Sample.Client.Features.Counter` also.
4. change `currentCount` to pull the Count from state. `int currentCount => CounterState.Count;`
5. Notice that inside IncrementCount method the currentCount you can no longer increment. From the outside the state is immutable.
So lets comment out that line.

The code should look as follows:

```csharp
@page "/counter"
@using BlazorState
@using Sample.Client.Features.Counter

@inherits BlazorStateComponent

<h1>Counter</h1>

<p>Current count: @currentCount</p>

<button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

@code {
    CounterState CounterState => GetState<CounterState>();

    int currentCount => CounterState.Count;

    void IncrementCount()
    {
        //currentCount++;
    }
}
```

Run the application and on the Counter Page you should notice the count is being displayed as we initialized.
Although the button no longer works.

## Sending requests that will mutate the state

Changes to state are done by sending an `Action` through the mediator pipeline.
The `Action` is then handled by a `Handler` which can freely mutate the state.

> [!Warning]
> State should NOT be mutated by anything other than handlers.
> All state changes should be done in handlers.
> This can be controlled by making state immutable and your handlers a nested class of the state they modify.

## Create the `IncrementCounterRequest`

1. In the Client project ensure the path `Features\Counter\Actions\IncrementCount` folder.
2. In this folder create a class named `IncrementCountAction.cs`.

* The class should inherit from `IRequest<CounterState>`
as follows:

```csharp
using MediatR;

namespace Sample.Client.Features.Counter
{
    public class IncrementCountAction: IRequest<CounterState>
    {
        public int Amount { get; set; }
    }
}
```

## Sending the action through the mediator pipeline

To Send the action to the pipeline when the user clicks the `Click me` button,
In `Pages\Counter.razor` update the `IncrementCount` function as follows:

```csharp
void IncrementCount()
{
    Mediator.Send(new IncrementCountAction { Amount = 5 });
}
```

## Handling the action

The `Handler` is where we actually mutate the state to complete the `Action`.  

1. In the `Features\Counter\IncrementCount` folder create a new class file named
 `IncrementCountHandler.cs`

The Handler should:

* be a nested class of the state it will mutate `CounterState`
* Inherit from `BlazorState.Handlers.RequestHandler`.
* The generic parameters are the Request Type `IncrementCountRequest` and the state type `CounterState`.
* Override the `Handle` method to mutate state as desired:

```csharp
using BlazorState;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Client.Features.Counter
{
    public partial class CounterState
    {

        public class IncrementCountHandler : RequestHandler<IncrementCountAction, CounterState>
        {
            public IncrementCountHandler(IStore aStore) : base(aStore) { }

            CounterState CounterState => Store.GetState<CounterState>();

            public override Task<CounterState> Handle(IncrementCountAction aIncrementCountAction, CancellationToken aCancellationToken)
            {
                CounterState.Count = CounterState.Count + aIncrementCountAction.Amount;
                return Task.FromResult(CounterState);
            }
        }
    }
}
```

## Validate

Execute the app and confirm that the "Click me" button properly increments the value.
And when you navigate away from the page and back the value persists.

## ReduxDevTools Javascipt Interop and RouteState

[ReduxDevTools](https://chrome.google.com/webstore/detail/redux-devtools/lmhkpmbekcpmknklioeibfkpmmfibljd) are a chrome extension that let you view the Actions and State for each action.
This is quite handy for debugging.

> [!NOTE]
> Support for "TimeTravel" via ReduxDevTools is being removed from Blazor-State as this feature, although cool for demos, does little to assist in debugging and requires more code to implement.

ReduxDevTools requires Javascript Interop.

To facilitate Javascript Interop, enable ReduxDevTools, and manage RouteState, add `App.razor.cs` in the same directory as `App.razor` as follows:

```csharp
namespace Sample.Client
{
    using System.Threading.Tasks;
    using BlazorState.Pipeline.ReduxDevTools;
    using BlazorState.Features.JavaScriptInterop;
    using BlazorState.Features.Routing;
    using Microsoft.AspNetCore.Components;

    public class AppBase : ComponentBase
    {
        [Inject] private JsonRequestHandler JsonRequestHandler { get; set; }
        [Inject] private ReduxDevToolsInterop ReduxDevToolsInterop { get; set; }

        // Injected so it is created by the container. Even though the ide says it is not used it is.
        [Inject] private RouteManager RouteManager { get; set; }

        protected override async Task OnAfterRenderAsync()
        {
            await ReduxDevToolsInterop.InitAsync();
            await JsonRequestHandler.InitAsync();
        }
    }
}
```

In your `App.razor` add `@inherits AppBase`

Now run your app again and then Open the Redux Dev Tools (a tab in Chrome Dev Tools) and you should see Actions as they are executed.

If you inspect the State in the DevTools you will also notice it maintains the current Route in RouteState.

That is the basics of getting started.
