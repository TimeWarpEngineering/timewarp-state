---
uid: TimeWarp.State:00-StateActionHandler-Server.md
title: TimeWarp.State Blazor Interactive Server Tutorial
renderMode: InteractiveServer
description: Learn TimeWarp.State basics using Blazor with Interactive Server render mode
---

# TimeWarp.State Blazor Interactive Server Tutorial

## State, Actions, and Handlers

This tutorial will walk you through the steps to create a Blazor application with TimeWarp.State using Interactive Server render mode.

> [!NOTE]
> This tutorial uses Blazor's Interactive Server render mode. For tutorials covering other render modes (Auto or WebAssembly-only), please see the respective tutorials in this series.

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download).

### Creating the Project

1. Create a new Blazor project:
```bash
dotnet new blazor --use-program-main --interactivity Server -n Sample00Server
```

2. Navigate to the project directory:
```bash
cd Sample00Server
```

3. Test the application:
```bash
dotnet run --project ./Sample00Server/Sample00Server.csproj
```

4. Open the URL shown in the command output (e.g., http://localhost:5256) and test the counter functionality.

> [!NOTE]
> The counter resets to zero when you navigate away and return because each time you leave the page,
> the counter component is destroyed.
> When you return, a new instance of the component is created, starting the count afresh.

### Install TimeWarp.State Package

Add the TimeWarp.State NuGet package:

```bash
dotnet add ./Sample00Server/Sample00Server.csproj package TimeWarp.State --prerelease
```

### Configure Services

Create GlobalUsings.cs to centralize common using statements:

```csharp
// Sample00Server/GlobalUsings.cs
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Web;
global using Microsoft.Extensions.DependencyInjection;
global using Sample00Server.Components;
global using TimeWarp.State;
```

Update Program.cs to add TimeWarp.State services:

```csharp
// Sample00Server/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddTimeWarpState(); // Add this line

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

### Add Features

1. Create a `Features` folder in the project root.
2. Inside `Features`, add a `Counter` folder.
3. Inside `Counter`, add `CounterState.cs`:

```csharp
// CounterState.cs
namespace Sample00Server.Features.Counter;

internal sealed partial class CounterState : State<CounterState>
{
    public int Count { get; private set; }
    
    public override void Initialize()
    {
        Count = 3;
    }
}
```

### UI Integration

Modify `Components/Pages/Counter.razor`:

```razor
@page "/counter"
@rendermode InteractiveServer
@using Sample00Server.Features.Counter
@inherits TimeWarp.State.TimeWarpStateComponent

<PageTitle>Counter</PageTitle>

<h1>Counter</h1>

<p role="status">Current count: @currentCount</p>

<button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

@code {
    CounterState CounterState => GetState<CounterState>();
    private int currentCount => CounterState.Count;

    private void IncrementCount()
    {
        // Empty for now
    }
}
```

### Implementing State Mutation through Actions and Handlers

Create `CounterState.IncrementCount.cs` in the Counter folder:

```csharp
namespace Sample00Server.Features.Counter;

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
            public Handler(IStore store) : base(store) { }
            
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

### Update Counter Component

Update the `IncrementCount` method in `Counter.razor`:

```razor
private async Task IncrementCount()
{
    await CounterState.IncrementCount(amount: 5);
}
```

### Validate

Run the application and test the counter functionality:

```bash
dotnet run
```

You should see:
1. The counter starts at 3 (initialized in CounterState)
2. Each click increases the count by 5
3. The state persists as long as the server connection is maintained
4. If you refresh the page or restart the server, the state resets

### Key Differences from Auto Mode

1. **State Location**: In Server mode, state is maintained on the server, not in the browser
2. **Connection Dependency**: State persists only as long as the SignalR connection is maintained
3. **Memory Management**: Server must manage state for all connected clients
4. **Performance**: No need to serialize state between client and server

This implementation demonstrates how TimeWarp.State works in a server-side context while maintaining the same clean architecture and patterns used in other render modes.
