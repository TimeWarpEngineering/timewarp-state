---
uid: TimeWarp.State:00-StateActionHandler.md
title: TimeWarp.State Blazor Interactive Auto Tutorial
renderMode: InteractiveAuto
description: Learn TimeWarp.State basics using Blazor with Interactive Auto render mode
---

# TimeWarp.State Blazor Interactive Auto Tutorial

> [!TIP]
> View the complete reference implementation for this tutorial:
> - [Sample00Auto Project](./Sample00Auto/)
> - [Client Project with TimeWarp.State](./Sample00Auto/Sample00Auto.Client/)
> - [CounterState Implementation](./Sample00Auto/Sample00Auto.Client/Features/Counter/CounterState.cs)
> - [Counter Page Component](./Sample00Auto/Sample00Auto.Client/Pages/Counter.razor)
> - [Program.cs with Service Configuration](./Sample00Auto/Sample00Auto/Program.cs)

## State, Actions, and Handlers

This tutorial will walk you through the steps to create a Blazor application with TimeWarp.State using Interactive Auto render mode.

> [!NOTE]
> This tutorial uses Blazor's Interactive Auto render mode. For tutorials covering other render modes (Server-only or WebAssembly-only), please see the respective tutorials in this series.

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download).

### Creating the Project

- Create a new Blazor project: `dotnet new blazor --use-program-main --interactivity Auto -n Sample00`
- Navigate to the new project: `cd Sample00`
- Test the application: `dotnet run --project ./Sample00/Sample00.csproj`
- Open the URL shown in the command output (e.g., <http://localhost:5256>) and test the counter functionality. Note: Your URL will differ.

> [!NOTE]
> The counter resets to zero when you navigate away and return because each time you leave the page,
> the counter component is destroyed.
> When you return, a new instance of the component is created, starting the count afresh.

### Install TimeWarp.State Package

1. Add the TimeWarp.State NuGet package to the Client project:

```bash
dotnet add ./Sample00.Client/Sample00.Client.csproj package TimeWarp.State
```

Note: The Server project doesn't need the package directly as it takes a dependency on the Client project.

2. Create GlobalUsings.cs files to centralize common using statements:

For the Client project:
```csharp
// Sample00.Client/GlobalUsings.cs
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
global using Microsoft.Extensions.DependencyInjection;
global using TimeWarp.State;
```

For the Server project:
```csharp
// Sample00/GlobalUsings.cs
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Web;
global using Microsoft.Extensions.DependencyInjection;
global using Sample00.Client.Pages;
global using Sample00.Components;
global using TimeWarp.State;
```

### Configure Services

Make TimeWarp.State functionality available from both Client and Server.

#### Sample00.Client Program.cs

- Make `Program` Public
- Create `ConfigureServices` method
- Call `ConfigureServices` from `Main`

```csharp
// Sample00.Client/Program.cs
namespace Sample00.Client;

public class Program
{
  static async Task Main(string[] args)
  {
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    ConfigureServices(builder.Services);
    await builder.Build().RunAsync();
  }

  public static void ConfigureServices(IServiceCollection serviceCollection)
  {
    serviceCollection.AddTimeWarpState();
  }
}
```

#### Sample00 Program.cs

- Call `ConfigureServices` from `Sample00.Client` `Program.cs`

```csharp
// Sample00/Program.cs
namespace Sample00;

public class Program
{
  public static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddRazorComponents()
      .AddInteractiveServerComponents()
      .AddInteractiveWebAssemblyComponents();
    
    Sample00.Client.Program.ConfigureServices(builder.Services); // <=== Add this line.

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
      app.UseWebAssemblyDebugging();
    }
    else
    {
      app.UseExceptionHandler("/Error");
      app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
      .AddInteractiveServerRenderMode()
      .AddInteractiveWebAssemblyRenderMode()
      .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

    app.Run();
  }
}
```

#### Sample00/Components/Routes.razor

Because we had to make the `Client` `Program` public, we need to qualify `Program` with `Sample00.Program`.

```html
@* Sample00/Components/Routes.razor *@

<Router AppAssembly="typeof(Sample00.Program).Assembly" AdditionalAssemblies="new[] { typeof(Client._Imports).Assembly }">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
</Router>
```

Validate the application still runs.

### Add Features

- Create a `Features` folder in the Client project.

#### Add CounterState

- Inside the `Features` folder, add a `Counter` folder.
- Inside `Counter`, add `CounterState.cs`:
- Define `CounterState` as a partial class inheriting from `State<CounterState>`.
- Override `Initialize()` to set `Count` to 3.

```csharp
// CounterState.cs
namespace Sample00.Client.Features.Counter;

internal sealed partial class CounterState : State<CounterState>
{
  public int Count { get; private set; }
  public override void Initialize()
  {
    Count = 3;
  }
}
```

#### UI Integration

Modify `Pages/Counter.razor`:
- Add `@using Sample00.Client.Features.Counter`
- Inherit from `TimeWarpStateComponent`.
- Add a property to access `CounterState`.
- Update display to use `CounterState`.
- Remove the `IncrementCount` implementation.

> Notice that inside the `IncrementCount` method the `currentCount`can no longer be incremented. The `CounterState` class is immutable from the outside.
So lets comment out that line.

The code should look as follows:

```csharp
@page "/counter"
@rendermode InteractiveAuto
@using Sample00.Client.Features.Counter

@inherits TimeWarp.State.TimeWarpStateComponent

<PageTitle>Counter</PageTitle>

<h1>Counter</h1>

<p role="status">Current count: @currentCount</p>

<button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

@code 
{
  CounterState CounterState => GetState<CounterState>();
  private int currentCount => CounterState.Count;

  private void IncrementCount()
  {
    // Empty for now.
  }
}
```

This binds the counter's UI to the state managed by TimeWarp.State.

### Implementing State Mutation through Actions and Handlers

Changes to state are done by sending an Action through the pipeline. The Action is then handled by a Handler which can freely mutate the state.

> [!WARNING]
> State must only be modified through designated handlers.
> Direct mutation of state outside these handlers is not allowed or desired.
> Maintain encapsulation by designing the state's public interface to be unmodifiable externally, with handlers implemented as nested classes within the state they modify.
> The TimeWarp.State.Analyzer will help you enforce this rule.

#### Action and Handler (ActionSet)

Create `CounterState.IncrementCount.cs` in `Features/Counter/Actions`.

Should expose the async `IncrementCount` method.

In this file, the `Action` class should:

* Be a nested class within `IncrementCount`, which in turn is a static class nested in `CounterState`.
* Inherit from `IAction`.
* Be part of the `Sample00.Client.Features.Counter` namespace.
* Contain the `Amount` property.

The `Handler` class should:

* Be a nested class of the state it will mutate `CounterState`.
* Be a nested class within `IncrementCount`.
* Inherit from `TimeWarp.State.Handlers.ActionHandler`.
* The generic parameter is the Request Type `Action`.
* Override the `Handle` method to mutate state as desired:

```csharp
// CounterState.IncrementCount.cs
namespace Sample00.Client.Features.Counter;

partial class CounterState
{
  public static class IncrementCountActionSet
  {
    public sealed class Action : IAction
    {
      public int Amount { get; }
      public Action(int amount)
      {
        Amount = amount;
      }
    }
    
    public sealed class Handler : ActionHandler<Action>
    {
      public Handler(IStore store) : base(store) {}      
      
      private CounterState CounterState => Store.GetState<CounterState>();

      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        CounterState.Count += action.Amount;
        return Task.CompletedTask;
      }
    }
  }
}

```

### Send action through the pipeline

To Send the action through the pipeline when the user clicks the Click me button, In `Pages/Counter.razor` update the IncrementCount function as follows:

```csharp
// Pages/Counter.razor
...
private async Task IncrementCount()
{
  await CounterState.IncrementCount(amount:5);
}
...
```

### Validate

When you run the application with Interactive Auto render mode, you'll notice the following behavior:

1. On initial load, the app starts on the server and the counter works as expected.
2. The first time you navigate away from the counter page and back, the app transitions from Server to WebAssembly rendering. During this one-time transition, the counter state will reset.
3. After this initial transition to WebAssembly, all subsequent navigation will maintain the counter's value, as the app continues running in WebAssembly mode.

> [!NOTE]
> This behavior is expected with Blazor's Interactive Auto render mode. The state reset occurs only during the one-time transition from Server to WebAssembly rendering. After that, the state persists across navigation as the application continues running in WebAssembly mode.

Congratulations, that is the basics of TimeWarp.State.
